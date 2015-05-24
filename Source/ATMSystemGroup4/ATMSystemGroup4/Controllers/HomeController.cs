using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ATMSystemGroup4.Models;

namespace ATMSystemGroup4.Controllers
{
    public class HomeController : Controller
    {
        private ATM_SystemEntities db = new ATM_SystemEntities();

        public ActionResult Index()
        {
            ViewBag.Message = "Chào mừng đến với ANZ's ATM System ! ";
            ViewBag.Inviting = "Vui lòng đưa thẻ vào để thực hiện giao dịch.";
            Session["Card"] = null;
            Session["ATM"] = null;

            return View();
        }


        public ActionResult Main()
        {
            Card card = (Card)Session["Card"];
            ATM atm = (ATM)Session["ATM"];
            string pin = (string)(Session["CardPin"]);
            if (pin == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                Account account = db.Accounts.Find(card.AccountID);
                Customer customer = db.Customers.Find(account.CustID);
                ViewBag.Message = "Xin chào khách hàng: " + customer.Name + " - đến với ATM tại " + atm.Address;
                ViewBag.Inviting = "Xin vui lòng chọn chức năng ! ";
            }
            return View();
        }


        //
        // GET:SuccessTransAsking
        //
        public ActionResult SuccessTransAsking()
        {
            return View();
        }

        //
        // POST:SuccessTransAsking
        //
        [HttpPost]
        public ActionResult SuccessTransAsking(string submitButton)
        {
            switch (submitButton)
            {
                case "CÓ":
                    return RedirectToAction("Main", "Home");

                case "KHÔNG":
                    return RedirectToAction("Index", "Home");

                default:
                    return View();
            }
        }


        public ActionResult Error(string noti)
        {
            ViewBag.Notis = noti;
            Session["Card"] = null;
            Session["ATM"] = null;
            return View();
        }

        //
        //Exit
        //
        public ActionResult Exit()
        {
            Session["Card"] = null;
            Session["ATM"] = null;
            return RedirectToAction("Index", "ATM");
        }
    }
}
