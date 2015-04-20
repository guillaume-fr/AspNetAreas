using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UsefulBits.Web.Demo.Services;

namespace UsefulBits.Web.Demo.Areas.Area51.Controllers
{
    public class HomeController : Controller
    {
        private ICustomService _customService;

        public HomeController(ICustomService customService)
        {
          _customService = customService;
        }

        // GET: Area51/Home
        public ActionResult Index()
        {
          ViewBag.Title = _customService.Name;

          return View();
        }
    }
}