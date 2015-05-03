using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ATMSystemGroup4.Models;

namespace ATMSystemGroup4.Controllers
{
    public class CardController : Controller
    {
        LogController lc;
        private ATMSystemEntities db = new ATMSystemEntities();

        // Validate
        //
        // GET: /Card/Validate


        public ActionResult Validate()
        {
            return View();
        }

        //
        // POST: /Card/Validate

        [HttpPost]
        public ActionResult Validate(string cardNo)
        {
            if (ModelState.IsValid)
            {
                Card card = db.Cards.Find(cardNo);
                if ((card != null))
                {
                    if (card.Attempt < 3)
                    {
                        Session["Card"] = card;
                        return RedirectToAction("Authenticate");
                    }
                    else
                    {
                        string s = "Thẻ đã bị khóa." +
               "Vui lòng liên hệ với phòng giao dịch gần nhất để biết thêm thông tin chi tiết !" +
               " Đang đưa thẻ ra ngoài....";
                        return RedirectToAction("Error", "Home", new { noti = s });
                    }
                }
                else
                {
                    string s = "Thẻ không hợp lệ." +
                               " Đang đưa thẻ ra ngoài....";
                    return RedirectToAction("Error", "Home", new { noti = s });
                }
            }
            return View(cardNo);
        }


        //Authenticate
        //
        // GET: /Card/Authenticate


        public ActionResult Authenticate()
        {
            return View();
        }

        //
        // POST: /Card/Authenticate

        [HttpPost]
        public ActionResult Authenticate(string pin)
        {
            if (ModelState.IsValid)
            {
                Card card = (Card)Session["Card"];
                if (card != null)
                {
                    if (card.PIN.Equals(pin) && card.Attempt < 3)
                    {
                        card.Attempt = 0;
                        db.Entry(card).State = EntityState.Modified;
                        db.SaveChanges();
                        return RedirectToAction("Main", "Home");
                    }
                    else
                    {
                        card.Attempt += 1;
                        db.Entry(card).State = EntityState.Modified;
                        db.SaveChanges();
                        if (card.Attempt < 3)
                        {
                            ViewBag.Error = "PIN không hợp lệ. Bạn còn " + (3 - card.Attempt) +
                                       " lần nhập PIN." +
                                       " Xin vui lòng nhập lại PIN....";
                            return View();
                        }
                        else
                        {
                            string s = "PIN không hợp lệ. Thẻ của bạn đã bị khóa." +
               " Vui lòng liên hệ với phòng giao dịch gần nhất để biết thêm thông tin chi tiết." +
               " Đang đưa thẻ ra ngoài....";
                            return RedirectToAction("Error", "Home", new { noti = s });
                        }
                    }
                }
                else
                {
                    string s = "Máy chưa nhận được thẻ";
                    return RedirectToAction("Error", "Home", new { noti = s });
                }
            }
            return View(pin);
        }



        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }


        /// <summary>
        /// Change PIN
        /// </summary>
        /// <returns></returns>

        // Nhập PIN cũ
        public ActionResult ChangePINold()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ChangePINold(FormCollection collection)
        {

            //  Card card = (Card)Session["Card"];
            string pin = collection["txtOldPin"];
            if (CheckPinOld(pin))
            {
                return RedirectToAction("ChangePINnew", "Card");
            }
            else if (CheckSystax(pin))
            {
                return View();
            }

            return View();
        }
        //--------------------------------


        /// <summary>
        ///Nhập PIN mới
        /// </summary>
        /// <returns></returns>
        public ActionResult ChangePINnew()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ChangePINnew(FormCollection collection)
        {
            Card card = (Card)Session["Card"];
            string newPIN = collection["txtNewPin"];
            if (CheckMatchNewAndOld(newPIN, card.PIN))
            {

                return View();
            }
            else if (CheckSystax(newPIN))
            {

                return View();
            }

            Session["NewPin"] = newPIN;
            return RedirectToAction("ChangePINconfirm", "Card");

        }


        //--////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Nhập lại PIN mới
        /// </summary>
        /// <returns></returns>
        public ActionResult ChangePINconfirm()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ChangePINconfirm(FormCollection collection)
        {
            ATM atm = (ATM)Session["ATM"];
            Card card = (Card)Session["Card"];
            string newPin = (string)Session["NewPin"];
            string confPin = collection["txtConfPin"];

            if (CheckSystax(confPin))
            {

                return View();

            }
            else if (CheckMatchNewAndConf(confPin, newPin) == false)
            {
                return View();
            }

            card.PIN = newPin;
            Card cardPIN = db.Cards.Find(card.CardNo);
            cardPIN.PIN = card.PIN;
            db.Entry(cardPIN).State = EntityState.Modified;
            db.SaveChanges();

            // Ghi Log cho change PIN
            lc = new LogController();
            string detailsFrom = "";
            lc.WriteLog(4, atm.ATMID, card.CardNo, 0, detailsFrom);
            return RedirectToAction("SuccessTransAsking", "Home");

        }


        /// <summary>
        /// ///////////////////////////////////////////// ham kiem tra///////////////////////////////////
        /// Kiểm tra PIN cũ đúng hay không?
        public bool CheckPinOld(string pinInput)
        {
            Card card = (Card)Session["Card"];

            if (card.PIN.Equals(pinInput.Trim()))
                return true;
            else
            {
                ViewBag.Error = "PIN không chính xác."
                                             + " Xin vui lòng nhập lại !";
                return false;
            }

        }
        // Kiểm tra lỗi cú pháp
        public bool CheckSystax(string pinInput)
        {
            if (pinInput.Length != 6 || pinInput == null)
            {
                ViewBag.Error = "PIN phải có độ dài là 6 chữ số."
                                      + " Xin vui lòng nhập lại !";
                return true;
            }
            return false;
        }
        //Kiểm tra new Pin và confirm Pin có khớp nhau hay không
        public bool CheckMatchNewAndConf(string pinConf, string pinNew)
        {
            if (pinConf.Trim() == pinNew.Trim())
                return true;
            else
            {
                ViewBag.Error = "Mã PIN mới không khớp."
                                       + " Xin vui lòng nhập lại !";
                return false;
            }
        }
        // Kiểm tra Pin mới có trùng với Pin cũ hay không?
        public bool CheckMatchNewAndOld(string pinNew, string pinOld)
        {
            if (pinNew.Trim() == pinOld.Trim())
            {
                ViewBag.Error = "Mã PIN mới không được trùng với PIN cũ."
                                       + " Xin vui lòng nhập lại !";
                return true;
            }
            else
            {
                return false;
            }
        }


    }
}