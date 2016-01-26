using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AzureCoreService;
using AzureCoreService.Entity;
using AzureCoreService.Repository;
using Microsoft.Data.Edm.Csdl;

namespace WebRole1.Controllers
{
    public class HomeController : Controller
    {
        public AdRepo AdRepo;

        public CategoryRepo CategoryRepo;

        public HomeController()
        {
            Console.WriteLine("Initial context");
            //AdRepo = new AdRepo(new AzureDbContext());
            AzureDbContext context = new AzureDbContext();

            CategoryRepo = new CategoryRepo(context);
            

        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            // test
            Category newcategory = new Category();
            newcategory.Id = 1;
            newcategory.name = "TEST";
            CategoryRepo.Insert(newcategory);
            CategoryRepo.Save();
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}