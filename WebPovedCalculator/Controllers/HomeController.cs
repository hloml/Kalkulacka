using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebPovedCalculator.Models;

namespace WebPovedCalculator.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            CounterModel counter = new CounterModel();
            ViewBag.Price = counter.GetPrice();
            return View();
        }
    }
}