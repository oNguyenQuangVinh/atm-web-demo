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
    public class AccountController : Controller
    {
        private LogController lc;
        private ATM_SystemEntities db = new ATM_SystemEntities();


      
        // TransferCashID
        //
        // GET: /Account/TransferCashID
        public ActionResult TransferCashID()
        {
            Card card = (Card)Session["Card"];
            Account accountFrom = db.Accounts.Find(card.AccountID);
            Customer cusFrom = db.Customers.Find(accountFrom.CustID);
            ViewBag.CusNameFrom = cusFrom.Name;
            ViewBag.AccIDFrom = accountFrom.AccountID;
            return View();
        }

        // TransferCashID
        //
        // POST: /Account/TransferCashID
        [HttpPost]
        public ActionResult TransferCashID(Account account)
        {
            if (ModelState.IsValid)
            {
                int accountID = account.AccountID;

                Card card = (Card)Session["Card"];


                Account accountTo = db.Accounts.Find(accountID);
                Account accountFrom = db.Accounts.Find(card.AccountID);
                Customer cusFrom = db.Customers.Find(accountFrom.CustID);

                if (accountTo != null && card.AccountID != account.AccountID)
                {
                    Session["AccountTo"] = accountTo;
                    return RedirectToAction("TransferCashAmount", "Account");
                }
                else
                {
                    ViewBag.CusNameFrom = cusFrom.Name;
                    ViewBag.AccIDFrom = accountFrom.AccountID;
                    ViewBag.Error = " Bạn đang chuyển tiền vào chính tài khoản của mình hoặc tài khoản bạn chuyển tiền đến không tồn tại."
                    + " Xin vui lòng nhập lại !";
                    return View();
                }
            }
            return View(account);
        }



        //
        // Kiểm Tra dữ liệu số tiền cần chuyển được nhập vào
        //
        public bool CheckAmountInput(string amountInput)
        {
            bool b = false;
            decimal amount;
            if (amountInput.Length != 0 && decimal.TryParse(amountInput, out amount) == true)
            {
                b = true;
            }
            return b;
        }


        //TransferCashAmount

        //GET: /Account/TransferCashAmount


        public ActionResult TransferCashAmount()
        {
            return View();
        }

        // TransferCashAmount
        //
        // POST: /Account/TransferCashAmount
        [HttpPost]
        public ActionResult TransferCashAmount(string txtamount)
        {
            if (CheckAmountInput(txtamount) == true)
            {
                decimal amount = decimal.Parse(txtamount);

                ATM atm = (ATM)Session["ATM"];
                Card card = (Card)Session["Card"];

                Account accountFrom = db.Accounts.Find(card.AccountID);
                Account accountTo = (Account)Session["AccountTo"];

                if (accountFrom.Balance > 0 && accountFrom.Balance > amount)
                {
                    Session["AmountTransfer"] = amount;
                    return RedirectToAction("TransferCashConfirm", "Account");
                }
                else
                {
                    ViewBag.Error = "Tài khoản của bạn < 0 VNĐ hoặc số tiền bạn cần chuyển lớn hơn số tiền có trong tài khoản . Xin vui lòng nhập lại";
                    return View();
                    // thông báo lỗi tài khoản < 0 hoặc số dư không đủ số tiền cần chuyển
                }
            }
            else
            {
                ViewBag.Error = "Không được để trống hoặc giá trị nhập vào không phải là số";
                return View();
            }
        }


        // TransferCashConfirm
        //
        // GET: /Account/TransferCashConfirm
        public ActionResult TransferCashConfirm()
        {
            Account accountTo = (Account)Session["AccountTo"];
            decimal amount = (decimal)Session["AmountTransfer"];
            ViewBag.AccountToID = accountTo.AccountID;
            ViewBag.CusNameTo = accountTo.Customer.Name;
            ViewBag.AmountTrans = amount;
            return View();
        }

        // TransferCashConfirm
        //
        // POST: /Account/TransferCashConfirm
        [HttpPost]
        public ActionResult TransferCashConfirm(Account account)
        {
            ATM atm = (ATM)Session["ATM"];

            Card card = (Card)Session["Card"];
            Account accountFrom = db.Accounts.Find(card.AccountID);


            // Tránh lỗi mutipleEntity ?!?
            Account accountto = (Account)Session["AccountTo"];
            int accountToID = accountto.AccountID;
            Account accountTo = db.Accounts.Find(accountToID);

            decimal amount = (decimal)Session["AmountTransfer"];

            accountFrom.Balance = accountFrom.Balance - amount;
            accountTo.Balance = accountTo.Balance + amount;
            db.Entry(accountFrom).State = EntityState.Modified;
            db.Entry(accountTo).State = EntityState.Modified;
            db.SaveChanges();

            // Ghi Log cho tk gửi
            lc = new LogController();
            // Details chỉ đến tên chủ tài khoản + AccNo
            string detailsFrom = "Transfer cash to " + accountTo.Customer.Name + " AccountID: " + accountTo.AccountID;
            lc.WriteLog(2, atm.ATMID, card.CardNo, amount, detailsFrom);

            //string detailsTo = "Receive cash from " + accountFrom.Customer.Name + " AccountID: " + accountFrom.AccountID;

            // Xóa Session
            Session["AccountTo"] = null;
            Session["AmountTransfer"] = null;

            return RedirectToAction("SuccessTransAsking", "Home");
        }


        /// <summary>
        /// Check Balance
        /// </summary>
        /// <param name="submitButton"></param>
        /// <returns></returns>
        // Lựa chọn hình thức kiểm tra số dư

        public ActionResult CheckBalanceMenu(string submitButton)
        {
            switch (submitButton)
            {
                case "  Hiển Thị  ":
                    return RedirectToAction("CheckBalance", "Account");
                case "In Hóa Đơn":
                    return RedirectToAction("CheckNoPeciept", "Account");
                default:
                    return View();
            }
        }

        /// <summary>
        /// Kiểm tra số dư
        /// </summary>
        /// <returns></returns>
        public ActionResult CheckBalance()
        {
            ATM atm = (ATM)Session["ATM"];
            Card card = (Card)Session["Card"];
            string cardNo;
            cardNo = card.CardNo;

            Account account = db.Accounts.Find(card.AccountID);
            decimal balance = (decimal)account.Balance;
            ViewBag.balanc = balance;

            lc = new LogController();
            string detailsFrom = "";
            lc.WriteLog(3, 1, card.CardNo, 0, detailsFrom);

            return View();
        }
        public ViewResult CheckNoPeciept()
        {
            return View();
        }


        //
        // GET: WithdrawOther
        //
        public ActionResult Withdraw()
        {
            return View();
        }

        //
        // POST: WithdrawOther
        //
        [HttpPost]
        public ActionResult Withdraw(string submitButton)
        {

            Card card = (Card)Session["Card"];
            ATM atm = (ATM)Session["ATM"];
            int amount = 0;
            bool kiemTraAcc, kiemTraAtm, kiemTraLimitWithdraw;
            // lay account qua card
            Account account = db.Accounts.Find(card.AccountID);
            // xu ly lua chon rut tien
            switch (submitButton)
            {
                case "500.000":
                    {
                        amount = 500000;
                    }
                    break;
                case "1.000.000":
                    {
                        amount = 1000000;
                    }
                    break;

                case "2.000.000":
                    {
                        amount = 2000000;
                    }
                    break;

                case "3.000.000":
                    {
                        amount = 3000000;
                    }
                    break;

                case "Other":
                    {
                        return RedirectToAction("WithdrawOther", "Account");
                    }
                //break;
                default:
                    {
                        return View();
                    }
                //break;
            }
            kiemTraAcc = checkAccount(amount, account);// goi ham kiem tra  tai khoan account
            kiemTraAtm = checkATM(amount); // goi ham kiem tra tien trong ATM
            string kiemTraType = checkTypeMoney(amount); // goi ham kiem tra tung menh gia trong ATM
            kiemTraLimitWithdraw = checkLimitWithdraw(amount, account);//goi ham kiem tra gioi han rut tien cua tai khoan
            if (kiemTraAcc == true && kiemTraAtm == true && kiemTraLimitWithdraw == true && kiemTraType != "")
            {
                // tru tien trong tai khoan
                account.Balance -= amount;

                //ghi log
                lc = new LogController();
                string detailsFrom = "";
                lc.WriteLog(1, 1, card.CardNo, amount, detailsFrom);
                // goi ham tru tien o ATM và hien view so tien ma khach se nhan duoc
                ViewBag.money = kiemTraType;
                db.SaveChanges();
                return View();

            }
            else
            {
                ViewBag.Error = showError(kiemTraAcc, kiemTraLimitWithdraw, kiemTraAtm, kiemTraType);
                return View();
            }
        }


        //
        // GET: WithdrawOther
        //
        public ActionResult WithdrawOther()
        {
            return View();
        }

        //
        // POST: WithdrawOther
        //
        [HttpPost]
        public ActionResult WithdrawOther(Account a)
        {
            decimal amount = (decimal)a.Balance;


            if (amount % 50000 == 0)
            {
                if (amount <= 10000000)
                {
                    Card card = (Card)Session["Card"];
                    ATM atm = (ATM)Session["ATM"];

                    Account account = db.Accounts.Find(card.AccountID);
                    bool kiemTraAcc, kiemTraAtm, kiemTraLimitWithdraw;
                    kiemTraAcc = checkAccount(amount, account);// goi ham kiem tra  tai khoan account
                    kiemTraAtm = checkATM(amount); // goi ham kiem tra tien trong ATM
                    string kiemTraType = checkTypeMoney(amount); // goi ham kiem tra tung menh gia trong ATM
                    kiemTraLimitWithdraw = checkLimitWithdraw(amount, account);//goi ham kiem tra gioi han rut tien cua tai khoan
                    if (kiemTraAcc == true && kiemTraAtm == true && kiemTraLimitWithdraw == true && kiemTraType != "")
                    {
                        // tru tien trong tai khoan
                        account.Balance -= amount;

                        //ghi log
                        lc = new LogController();
                        string detailsFrom = "";
                        lc.WriteLog(1, 1, card.CardNo, amount, detailsFrom);
                        // goi ham tru tien o ATM và hien view so tien ma khach se nhan duoc
                        ViewBag.money = kiemTraType;
                        db.SaveChanges();
                        return View();

                    }
                    else
                    {
                        ViewBag.Error = showError(kiemTraAcc, kiemTraLimitWithdraw, kiemTraAtm, kiemTraType);
                        return View();
                    }
                }
                else
                {
                    ViewBag.Error = "Số tiền rút cho phép là dưới mười triệu đồng :10000000";
                    return View();
                }
            }
            else
            {
                ViewBag.Error = "Số tiền rút phải là bội của năm mươi nghìn đồng:50.000";
                return View();

            }

        }


        //
        //////////////////////////////////////////////////////////////
        // kiem tra so tien trong tai khoan du cho rut khong?
        // neu ham tra True thi du nguoc lai khong du.
        public bool checkAccount(decimal amount, Account account)
        {
            bool kiemTra = false;
            if (amount <= (account.Balance + account.OverDraftLimit.Value))
            {
                kiemTra = true;
            }
            return kiemTra;

        }


        //////////////////////////////////////////////////////////////////////////
        // kiem tra tien trong Atm co du de rut khong?
        // neu ham tra True thi du va nguoc lai thi khong du
        public bool checkATM(decimal amount)
        {
            //bool kiemTra = false;
            //// loc cac kho chua tien cua cay atm dang rut
            //var stock = from e in db.stocks
            //            where e.atmid == atm.atmid
            //            select e;
            //ilist<stock> liststock = stock.tolist<stock>();
            //decimal money = 0;
            //// tinh tong tien co trong cay atm dang rut
            //foreach (stock i in liststock)
            //{
            //    money += (decimal)(i.moneytype.moneyvalue * i.quantity);
            //}
            //if (amount <= money)
            //{
            //    kiemtra = true;
            //}
            //return kiemtra;
            return true;
        }


        // kiem tra gioi han rut tien cua tai khoan
        public bool checkLimitWithdraw(decimal amount, Account account)
        {
            bool kiemtra = false;
            if (amount <= account.WithDrawLimit.Value)
            {
                kiemtra = true;
            }
            return kiemtra;
        }


        ///////////////////////////////////////////////////////////////
        // kiem tra cac menh gia cua cay Atm dang rut phu hop voi so tien muon rut k?
        // mỗi kho là một loại mệnh giá
        // hàm trả giá trị "" thì khong du , neu dủ thì nó sẽ hiển thị số luong từng mệnh giá mà khách nhận được 
        public string checkTypeMoney(decimal amount)
        {
            //   bool kiemtra = false;
            string s = "";
            var stock = from e in db.Stocks
                        select e;
            IList<Stock> listStock = stock.ToList<Stock>();
            // int du500, du200, du100, du50, to500, to200, to100, to50;
            int type500 = 0, type200 = 0, type100 = 0, type50 = 0;
            foreach (Stock i in listStock)
            {
                switch ((int)i.MoneyType.MoneyValue)
                {
                    case 500000:
                        type500 = (int)i.Quantity;
                        break;
                    case 200000:
                        type200 = (int)i.Quantity;
                        break;
                    case 100000:
                        type100 = (int)i.Quantity;
                        break;
                    case 50000:
                        type50 = (int)i.Quantity;
                        break;
                }
            }


            for (int i = 0; i <= (int)amount / 50000; i++)
            {
                for (int j = 0; j <= (int)amount / 100000; j++)
                {
                    for (int k = 0; k <= (int)amount / 200000; k++)
                    {
                        for (int m = 0; m <= (int)amount / 500000; m++)
                        {
                            if (((i * 50000 + j * 100000 + k * 200000 + m * 500000) == amount) && (m <= type500) && (k <= type200) && (j <= type100) && (i <= type50))
                            {
                                trutien(m, k, j, i);
                                return s = "Quý khách nhận được " + m + " tờ 500000, " + k + " tờ 200000, "
                                    + j + " tờ 100000, " + i + " tờ 50000 ";
                            }
                        }
                    }
                }
            }
            return s = "";

        }


        /// ////////////////////////////////////////////////////////
        // ham tru tien trong cay atm
        public void trutien(int to500, int to200, int to100, int to50)
        {
            var stock = from e in db.Stocks
                        select e;
            IList<Stock> listStock = stock.ToList<Stock>();
            foreach (Stock i in listStock)
            {
                switch ((int)i.MoneyType.MoneyValue)
                {
                    case 500000:
                        i.Quantity -= to500;
                        break;
                    case 200000:
                        i.Quantity -= to200;
                        break;
                    case 100000:
                        i.Quantity -= to100;
                        break;
                    case 50000:
                        i.Quantity -= to50;
                        break;
                }
            }
            db.SaveChanges();

        }


        /////
        // hàm hien thi cac loi 
        public string showError(bool kiemTraAcc, bool kiemTraLimitWithdraw, bool kiemTraAtm, string kiemTraType)
        {
            if (kiemTraLimitWithdraw == false)
            {
                return ("Sô tiền rút vượt quá giới hạn cho phép rút của tài khoản của quý khách");

            }
            else if (kiemTraAcc == false)
            {
                return ("Tài khoản quý khách không đủ để thực hiện giao dịch này! Xin quý khách vui long thực hiện giao dịch lần sau");

            }
            else if (kiemTraAtm == false)
            {

                return ("Xin lỗi quý khách! Tiền trong cây ATM không đủ để thực hiện giao dịch này");

            }
            else if (kiemTraType == "")
            {
                return ("Xin lỗi quý khách ! Loại mệnh giá tiền trong cây ATM không phù hợp để thực hiện giao dịch này");

            }
            else
            {
                return "";
            }
        }




    }
}