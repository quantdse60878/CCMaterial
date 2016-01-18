
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Web.Mvc;

namespace WebRole1.Controllers
{
    public class HomeController : Controller
    {
        /**
        * OK status.
        */
        private static String OK_STATUS = "{\"status\":\"ok\"}";

        /**
        * FAIL status.
        */
        private static String FAIL_STATUS = "{\"status\":\"fail\"}";

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page 2222.";

            return View();
        }

        [HttpPost]
        public JsonResult CreateName(String name)
        {
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(HomeController.BuildConnectionString());
                if (null != con)
                {
                    SqlCommand cmd = new SqlCommand("INSERT INTO people (name) VALUES (N'" + name + "')");
                    cmd.Connection = con;
                    con.Open();

                    if (cmd.ExecuteNonQuery() == 0)
                    {
                        return Json(FAIL_STATUS);
                    }
                } else
                {
                    Console.WriteLine("Fail to open connection");
                    return Json(FAIL_STATUS);
                }
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                return Json(FAIL_STATUS);
            } finally {
                if (null != con && con.State != ConnectionState.Closed)
                {
                    con.Close();
                }
            }
            Console.WriteLine("Successfully insert [" + name + "] into db.");
            return Json(OK_STATUS);
        }

        public JsonResult Names()
        {
            List<string> names = this.FindAllNamesFromQueue();
            Trace.TraceInformation("Got " + names.Count + " records");
            return Json(new { dataList = names }, JsonRequestBehavior.AllowGet);
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
            var dbCon = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            return dbCon;
        }

        private List<string> FindAllNamesFromQueue()
        {
            Console.WriteLine(IConsts.BEGIN_METHOD);
            try
            {
                // TODO put storage string into file ServiceConfiguration.Local.csfg
                // read method like WorkerSqlConnectionString
                CloudStorageAccount account = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=se0870group5;AccountKey=5LoJv8tIkDXmLklfQG6HqwfsNL56fqg5AUEVot/ejnWTtyfjl92+mWs19b/R1s1I7XuaFe4HqE5tuKZSwpqB1A==;BlobEndpoint=https://se0870group5.blob.core.windows.net/;TableEndpoint=https://se0870group5.table.core.windows.net/;QueueEndpoint=https://se0870group5.queue.core.windows.net/;FileEndpoint=https://se0870group5.file.core.windows.net/");
                CloudQueueClient client = account.CreateCloudQueueClient();
                CloudQueue queue = client.GetQueueReference("people-queue");
                if (!queue.Exists())
                {
                    Trace.TraceInformation("Not exists, create new queye");
                    queue.CreateIfNotExists();
                }
                else
                {
                    Trace.TraceInformation("Existence queue");
                }

                queue.FetchAttributes();

                int totalCount = (int) queue.ApproximateMessageCount;
                Trace.TraceInformation("total count: " + totalCount);
                List<string> result = new List<string>();
                if (0 < totalCount)
                {
                    foreach(CloudQueueMessage message in queue.PeekMessages(totalCount))
                    {
                        result.Add(message.AsString);
                    }
                }
                Console.WriteLine("Got " + result.Count + " results.");
                return result;
            } finally
            {
                Console.WriteLine(IConsts.END_METHOD);
            }
        }
    }

    public static class IConsts
    {
        public static string BEGIN_METHOD = "---begin---";

        public static string END_METHOD = "---end---";
    }
}