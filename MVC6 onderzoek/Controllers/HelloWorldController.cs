using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC6_onderzoek.Controllers
{
    public class HelloWorldController : Controller
    {
        // GET: HelloWorld
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Welcome(string name,int numtimes=1)
        {
            ViewData["name"] = name;
            ViewData["numtimes"] = numtimes;

            return View();
        }
    }
}