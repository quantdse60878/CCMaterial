using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureCoreService;
using AzureCoreService.Entity;
using AzureCoreService.Repository;
using HtmlAgilityPack;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private AzureDbContext context;
        private TopicRepo topicRepo;
        private NewsRepo newsRepo;
        private static readonly string dbSettingName = "AzureDbConnection";

        public override void Run()
        {
            Trace.TraceInformation("WorkerRole1 is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            // Initital db context;
            string connection = CloudConfigurationManager.GetSetting(dbSettingName);
            context = new AzureDbContext(connection);
            Trace.TraceInformation("WorkerRole1 has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("WorkerRole1 is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("WorkerRole1 has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                crawData();
                await Task.Delay(1000);
            }
        }

        private void crawData()
        {
            // Read all topic from table Topic
            Trace.TraceInformation("--begin craw data--");
            topicRepo = new TopicRepo(context);
            newsRepo = new NewsRepo(context);
            try
            {
                
                List<Topic> topics = 
                    (List<Topic>) topicRepo.ExcecuteNativeSql("SELECT * FROM Topic");
                if (null != topics)
                {
                    Trace.TraceInformation("Total {0} topics need to parse", topics.Count);

                    foreach(Topic topic in topics)
                    {
                        List<News> newses = this.parseNewsForTopic(topic);
                        if (newses != null && newses.Count > 0)
                        {
                            // Filter by title before adding to db
                            string sqlQuery = "SELECT * FROM News Where Title = @Title AND Link = @Link";
                            foreach (News news in newses)
                            {
                                SqlParameter titleParameter = new SqlParameter("Title", news.Title);
                                SqlParameter linkParameter = new SqlParameter("Link", news.Link);
                                object[] parameters = new object[] { titleParameter, linkParameter};
                                var sameTopic =
                                    newsRepo.ExcecuteNativeSql(sqlQuery, parameters);
                                if (sameTopic != null && !sameTopic.Any())
                                {
                                    newsRepo.Insert(news);
                                }
                                else { Trace.TraceInformation("Duplicate data");}
                            }
                        }
                    }
                    newsRepo.Save();
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(String.Format(" Error at: {0}", e.Message));
            }
            finally
            {
                topicRepo = null;
                newsRepo = null;
                Trace.TraceInformation("--end craw data--");
            }

            
            // Save all to db
        }

        private List<News> parseNewsForTopic(Topic topic)
        {
            string link = "";
            string title = "";
            string paragraph = "";
            string html = "";

            Trace.TraceInformation("Begin parse new for topic: {}" + topic.Name);
            try
            {
                HtmlWeb htmlWeb = new HtmlWeb()
                {
                    AutoDetectEncoding = false,
                    OverrideEncoding = Encoding.UTF8 // Set UTF8 to display vietnamese
                };
                HtmlDocument htmlDocument = htmlWeb.Load(topic.Link);
                List<News> items = new List<News>();
                //load all div with class = title_news
                var threadItems = htmlDocument.DocumentNode.SelectNodes("//*[@class='title_news']");
                if (threadItems != null && threadItems.Count > 0)
                {
                    foreach (var item in threadItems.ToList())
                    {
                        var linkNode = item.SelectSingleNode(".//a[contains(@class,'txt_link')]");
                        link = linkNode.Attributes["href"].Value.Trim();
                        Trace.TraceInformation("Parsing for news: " + link);
                        title = linkNode.InnerText.Trim();

                        htmlDocument = htmlWeb.Load(link);
                        html = htmlDocument.DocumentNode.InnerHtml;
                        var intro = htmlDocument.DocumentNode.SelectSingleNode(".//div[@class='short_intro txt_666']");
                        var fullText =
                            htmlDocument.DocumentNode.SelectSingleNode(".//div[@class='fck_detail width_common']");
                        paragraph = "";
                        if (fullText != null)
                        {
                            var text = fullText.SelectNodes(".//p[contains(@class,'Normal')]");
                            if (intro != null)
                            {
                                paragraph = paragraph + intro.InnerText.Trim();
                                if (text != null)
                                {
                                    foreach (var t in text)
                                    {
                                        paragraph = paragraph + "\n" + t.InnerText.Trim();
                                    }
                                }
                            }
                        }

                        // Add to list
                        var data = new News();
                        data.Title = title;
                        data.Link = link;
                        data.Html = html;
                        data.Text = paragraph;

                        items.Add(data);
                    }
                }

                return items;
            }
            catch (Exception e)
            {
                // Ignore
                Trace.TraceError("Error at {0}", e.Message);
                return null;
            }
            finally
            {
                Trace.TraceInformation("End parse");
            }
            
        } 
    }
}
