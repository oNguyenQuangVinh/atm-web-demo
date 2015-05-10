using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Objects;
using System.Data.Objects.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ATMSystemGroup4.Models;
using PagedList;

namespace ATMSystemGroup4.Controllers
{
    public class LogController : Controller
    {
        private ATM_SystemEntities db = new ATM_SystemEntities();


        //
        // SelectPeriod 
        // GET
        public ActionResult SelectPeriod()
        {
            Card card = (Card)Session["Card"];
            ATM atm = (ATM)Session["ATM"];
            if (card != null && atm != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        //
        // SelectPeriod
        // POST
        [HttpPost]
        public ActionResult SelectPeriod(string submitButton)
        {
            switch (submitButton)
            {
                case "1 tuần trước":
                    return RedirectToAction("ViewHistory", new { period = "1 tuần" });

                case "1 tháng trước":
                    return RedirectToAction("ViewHistory", new { period = "1 tháng" });

                case "4 tuần trước":
                    return RedirectToAction("ViewHistory", new { period = "4 tháng" });

                case "6 tuần trước":
                    return RedirectToAction("ViewHistory", new { period = "6 tháng" });

                case "1 năm trước":
                    return RedirectToAction("ViewHistory", new { period = "1 năm" });

                case "2 năm trước":
                    return RedirectToAction("ViewHistory", new { period = "2 năm" });

                default:
                    return View();

            }
        }



        //
        // GET: /Log/ViewHistory

        public ViewResult ViewHistory(string period, int? page)
        {
            Card card = (Card)Session["Card"];
            var logs = db.Logs.Include(l => l.Card).Include(l => l.LogType);
            switch (period)
            {
                case "1 tuần":
                    logs = from l in db.Logs
                           where SqlFunctions.DateDiff("dd", l.LogDate, DateTime.Now) <= 7
                           && l.CardNo == card.CardNo
                           select l;                    
                    break;

                case "1 tháng":
                    logs = from l in db.Logs
                           where SqlFunctions.DateDiff("dd", l.LogDate, DateTime.Now) <= 30
                           && l.CardNo == card.CardNo
                           select l;                  
                    break;

                case "4 tháng":
                    logs = from l in db.Logs
                           where SqlFunctions.DateDiff("dd", l.LogDate, DateTime.Now) <= 120
                           && l.CardNo == card.CardNo
                           select l;                   
                    break;

                case "6 tháng":
                    logs = from l in db.Logs
                           where SqlFunctions.DateDiff("dd", l.LogDate, DateTime.Now) <= 180
                           && l.CardNo == card.CardNo
                           select l;                    
                    break;

                case "1 năm":
                    logs = from l in db.Logs
                           where SqlFunctions.DateDiff("dd", l.LogDate, DateTime.Now) <= 365
                           && l.CardNo == card.CardNo
                           select l;                   
                    break;

                case "2 năm":
                    logs = from l in db.Logs
                           where SqlFunctions.DateDiff("dd", l.LogDate, DateTime.Now) <= 700
                           && l.CardNo == card.CardNo
                           select l;                  
                    break;

            }

            logs = logs.OrderByDescending(s => s.LogDate);
            int countLogs = logs.Count();

            Config con = db.Configs.Find(1);
            int pageSize = 5;

            int pageNumber = (page ?? 5);

            ViewBag.Period = period;
            ViewBag.CountLogs = countLogs;

            return View(logs.ToPagedList(pageNumber,pageSize));
        }

        //
        //WriteLog
        public bool WriteLog(int logType, int atmID, string cardNo, decimal amount, string details)
        {
            Log log = new Log();
            log.LogTypeID = logType;
            //log.ATMID = atmID;
            log.CardNo = cardNo;
            log.LogDate = DateTime.Now;
            log.Amount = amount;
            log.Details = details;

            db.Logs.Add(log);
            db.SaveChanges();
            return true;
        }

    }
}