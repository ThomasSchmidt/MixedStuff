using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Redis.SessionStateProvider.Web.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        public ActionResult Index()
        {
	        var val = Session["redis"];

			Session["redis"] = "value";

            return View();

	        
        }
    }
}
