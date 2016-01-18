using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Data.SqlClient;
using System.Data;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

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
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working at " + DateTime.Now);
                this.SynchronizeDbAndQueue();
                await Task.Delay(10 * 1000);
            }
        }

        private void SynchronizeDbAndQueue()
        {
            SqlConnection con = null;
            CloudQueue queue = null;
            Trace.TraceInformation("Begin procesingg");
            try
            {
                con = new SqlConnection(WorkerRole.BuildConnectionString());
                if (null != con)
                {
                    SqlCommand cmd = new SqlCommand("Select * FROM people");
                    cmd.Connection = con;

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    // fetch sql list
                    List<string> sqlList = new List<string>();
                    while(reader.Read())
                    {
                        IDataRecord record = (IDataRecord) reader;
                        if (null != record)
                        {
                            sqlList.Add(record["name"].ToString());
                        }
                    }
                    reader.Close();
                    Trace.TraceInformation("got " + sqlList.Count + " record");
                    // fetch sql queue
                    queue = BuildQueue(true);
                    if (null != queue && sqlList.Count > 0)
                    {
                        queue.FetchAttributes();

                        int totalCount = (int)queue.ApproximateMessageCount;
                        List<string> result = new List<string>();
                        if (0 < totalCount)
                        {
                            foreach (CloudQueueMessage message in queue.PeekMessages(totalCount))
                            {
                                result.Add(message.AsString);
                            }
                        }

                        Trace.TraceInformation("queue got " + result.Count + " record");
                        foreach(string message in result)
                        {
                            Trace.TraceInformation(message);
                        }


                        // compare data
                        foreach (string name in sqlList)
                        {
                            if (!result.Contains(name))
                            {
                                // Create a message and add it to the queue.
                                CloudQueueMessage message = new CloudQueueMessage(name);
                                Trace.TraceInformation("add new name: " + name);
                                queue.AddMessage(message);
                            } // else -> exists
                        }

                    }
                }
                else
                {
                    Console.WriteLine("Fail to open connection");
                }
            } catch (Exception e)
            {
                Trace.TraceError(e.Message);
            } finally
            {
                if (con != null && con.State != System.Data.ConnectionState.Closed)
                {
                    con.Close();
                }
                Trace.TraceInformation("End procesingg");
            }
        }

        private static CloudQueue BuildQueue(Boolean createIfNeed)
        {
            CloudStorageAccount account = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=se0870group5;AccountKey=5LoJv8tIkDXmLklfQG6HqwfsNL56fqg5AUEVot/ejnWTtyfjl92+mWs19b/R1s1I7XuaFe4HqE5tuKZSwpqB1A==;BlobEndpoint=https://se0870group5.blob.core.windows.net/;TableEndpoint=https://se0870group5.table.core.windows.net/;QueueEndpoint=https://se0870group5.queue.core.windows.net/;FileEndpoint=https://se0870group5.file.core.windows.net/");
            CloudQueueClient client = account.CreateCloudQueueClient();
            CloudQueue queue = client.GetQueueReference("people-queue");
            if (!queue.Exists())
            {
                Trace.TraceInformation("Not exists, create new queye");
                queue.CreateIfNotExists();
            } else
            {
                Trace.TraceInformation("Existence queue");
            }
            
            return queue;
        }

        private static string BuildConnectionString()
        {
            //SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            //builder.DataSource = "QUANTDSE60878\\SQLEXPRESS";
            //builder.InitialCatalog = "azure-queue";
            //builder.UserID = "sa";
            //builder.Password = "123456";
            //builder.MultipleActiveResultSets = true;
            //return builder.ConnectionString;
            //string conString = "Server=tcp:se0870cc-group5.database.windows.net,1433;Database=Peoples;User ID=group5@se0870cc-group5;Password=Toan310793;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            //return conString;
            var dbCon = CloudConfigurationManager.GetSetting("WorkerSqlConnectionString");
            return dbCon;
        }
    }
}
