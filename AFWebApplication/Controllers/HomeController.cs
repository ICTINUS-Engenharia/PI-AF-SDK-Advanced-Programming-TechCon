using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OSIsoft.AF;


namespace AFWebApplication.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View("QueryExercise");
        }

        public ActionResult ResultExercise(string exerciseQuery) //, string inputQuery)
        {
            var results = CommonLib.Exercises.RunWithRedirection().Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            return View(results.ToList());
        }

    }
}