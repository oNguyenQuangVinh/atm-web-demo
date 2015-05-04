using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ATMSystemGroup4.Models;

namespace ATMSystemGroup4.Controllers
{
    public class ATMController : Controller
    {
        private ATM_SystemEntities db = new ATM_SystemEntities();

        //
        // GET: /ATM/

        public ViewResult Index()
        {
            return View(db.ATMs.ToList());
        }

        //
        // GET: /ATM/Select

        public ActionResult Select(int id)
        {
            ATM atm = db.ATMs.Find(id);
            Session["ATM"] = atm;
            return RedirectToAction("Validate", "Card");
        }


    }
}