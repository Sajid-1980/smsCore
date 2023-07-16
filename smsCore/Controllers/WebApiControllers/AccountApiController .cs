//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Web.Http;
//using System.Web.Mvc;
//using Lib.Web.Mvc.JQuery.JqGrid;
//using Models;
//using Newtonsoft.Json;
//using Utilities;
//using System.Linq.Dynamic;
//using RouteAttribute = System.Web.Http.RouteAttribute;
//using RoutePrefixAttribute = System.Web.Http.RoutePrefixAttribute;
//using AuthorizeAttribute = System.Web.Http.AuthorizeAttribute;
//using smsCore.Data;

//namespace sms.WebApiControllers
//{
//    [Authorize]
//    [RoutePrefix("api/Account")]
//    public class AccountApiController : ApiController
//    {
//        private readonly SchoolEntities db;
//        private readonly CurrentUser _user;
//        public AccountApiController(SchoolEntities _db,CurrentUser Cuser)
//        {
//            db = _db;
//            _user = Cuser;
//        }
//        // GET api/<controller>
//        public IEnumerable<string> Get()
//        {
//            return new string[] { "value1", "value2" };
//        }

//        // GET api/<controller>/5
//        public string Get(int id)
//        {
//            return "value";
//        }

//        // POST api/<controller>
//        public void Post([FromBody]string value)
//        {
//        }

//        // PUT api/<controller>/5
//        public void Put(int id, [FromBody]string value)
//        {
//        }

//        // DELETE api/<controller>/5
//        public void Delete(int id)
//        {
//        }
        
//        [Route("ContraVoucher")]
//        public string getdataforContraVoucher(string FromDate, string ToDate, string LedgerId, string VoucherNo, string radio)
//        {
//            string strType = string.Empty;
//            if (radio == "deposit")
//            {
//                strType = "Deposit";
//            }
//            else
//            {
//                strType = "Withdraw";
//            }
//            ContraMasterSP spContraMaster = new ContraMasterSP();
   
//            DataTable dtbl = new DataTable();
//            if (VoucherNo == null)
//            {
//                VoucherNo = "";
//            }
//            if (FromDate == null || ToDate == null)
//            {
//                FromDate = DateTime.Now.ToString();
//                ToDate = DateTime.Now.ToString();
//            }
//            if (LedgerId == null)
//            {
//                LedgerId = "All";
//            }
//            dtbl = spContraMaster.ContraVoucherRegisterSearch(Convert.ToDateTime(FromDate), Convert.ToDateTime(ToDate), VoucherNo.Trim(), LedgerId, strType);

//            var details = dtbl.AsEnumerable().Select(s => new {
//                SlNo= s["SlNo"],
//                invoiceNo = s["invoiceNo"],
//                voucherTypeName = s["voucherTypeName"],
//                date = s["date"],
//                narration = s["narration"],
//                ledgerName = s["ledgerName"],
//                amount = s["amount"],
//                action = $"<a title=\"Edit\" href=\"{this.Url.Link("Default", new { Controller = "AccountTransaction", Action = "ContraVoucherEdit", id = s["contraMasterId"] })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-edit\"></i></a> <a title=\"Delete\" href=\"{this.Url.Link("Default", new { Controller = "AccountTransaction", Action = "ContraVoucherDelete", id =s["contraMasterId"] })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-trash\"></i></a> "
//            });
          
//            var jsonString = JsonConvert.SerializeObject(details);
//            return jsonString;
//        }


//        [Route("PaymentVoucher")]
//        public string getdataforPaymentVoucher(string FromDate, string ToDate, string LedgerId, string VoucherNo)
//        {
//            TransactionsGeneralFill Obj = new TransactionsGeneralFill();
//            PaymentMasterSP Master = new PaymentMasterSP();
//            DataTable dtbl = new DataTable();
//            if (VoucherNo == null || VoucherNo =="Undefine")
//            {
//                VoucherNo = "";
//            }
//            if (FromDate == null || ToDate == null)
//            {
//                FromDate = DateTime.Now.ToString();
//                ToDate = DateTime.Now.ToString();
//            }
//            if (LedgerId == null)
//            {
//                LedgerId = "0";
//            }
//            dtbl = Master.PaymentMasterSearch(Convert.ToDateTime(FromDate), Convert.ToDateTime(ToDate), decimal.Parse(LedgerId), VoucherNo.Trim());

//            var details = dtbl.AsEnumerable().Select(s => new {
//                SlNo = s["SL.NO"],
//                invoiceNo = s["invoiceNo"],
//                voucherTypeName = s["voucherTypeName"],
//                date = s["date"],
//                narration = s["narration"],
//                ledgerName = s["ledgerName"],
//                amount = s["totalAmount"],
//                action = $"<a title=\"Edit\" href=\"{this.Url.Link("Default", new { Controller = "AccountTransaction", Action = "PaymentVoucherEdit", id = s["paymentMasterId"] })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-edit\"></i></a> <a title=\"Delete\" href=\"{this.Url.Link("Default", new { Controller = "AccountTransaction", Action = "PaymentVoucherDelete", id = s["paymentMasterId"] })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-trash\"></i></a> "
//            });

//            var jsonString = JsonConvert.SerializeObject(details);
//            return jsonString;
//        }


//        [Route("ReceiptVoucher")]
//        public string getdataforReceiptVoucher(string FromDate, string ToDate, string LedgerId, string VoucherNo)
//        {
//            TransactionsGeneralFill Obj = new TransactionsGeneralFill();
//            ReceiptMasterSP Master = new ReceiptMasterSP();
//            DataTable dtbl = new DataTable();
//            if (VoucherNo == null || VoucherNo == "Undefine")
//            {
//                VoucherNo = "";
//            }
//            if (FromDate == null || ToDate == null)
//            {
//                FromDate = DateTime.Now.ToString();
//                ToDate = DateTime.Now.ToString();
//            }
//            if (LedgerId == null)
//            {
//                LedgerId = "0";
//            }
//            dtbl = Master.ReceiptMasterSearch(Convert.ToDateTime(FromDate), Convert.ToDateTime(ToDate), decimal.Parse(LedgerId), VoucherNo.Trim());

//            var details = dtbl.AsEnumerable().Select(s => new {
//                SlNo = s["SL.NO"],
//                invoiceNo = s["invoiceNo"],
//                voucherTypeName = s["voucherTypeName"],
//                date = s["date"],
//                narration = s["narration"],
//                ledgerName = s["ledgerName"],
//                amount = s["totalAmount"],
//                action = $"<a title=\"Edit\" href=\"{this.Url.Link("Default", new { Controller = "AccountTransaction", Action = "ReceiptVoucherEdit", id = s["receiptMasterId"] })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-edit\"></i></a> <a title=\"Delete\" href=\"{this.Url.Link("Default", new { Controller = "AccountTransaction", Action = "ReceiptVoucherDelete", id = s["receiptMasterId"] })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-trash\"></i></a> "
//            });

//            var jsonString = JsonConvert.SerializeObject(details);
//            return jsonString;
//        }

//        [Route("JournalVoucher")]
//        public string getdataforJournalVoucher(string FromDate, string ToDate ,string VoucherNo)
//        {
//            TransactionsGeneralFill Obj = new TransactionsGeneralFill();
//            JournalMasterSP Master = new JournalMasterSP();
//            DataTable dtbl = new DataTable();
//            if (VoucherNo == null || VoucherNo == "Undefine")
//            {
//                VoucherNo = "";
//            }
//            if (FromDate == null || ToDate == null)
//            {
//                FromDate = DateTime.Now.ToString();
//                ToDate = DateTime.Now.ToString();
//            }
        
//            dtbl = Master.JournalRegisterSearch(VoucherNo.Trim(),FromDate, ToDate);

//            var details = dtbl.AsEnumerable().Select(s => new {
//                SlNo = s["SlNo"],
//                invoiceNo = s["invoiceNo"],
//                voucherTypeName = s["voucherTypeName"],
//                date = s["date"],
//                narration = s["narration"],
//                amount = s["totalAmount"],
//                action = $"<a title=\"Edit\" href=\"{this.Url.Link("Default", new { Controller = "AccountTransaction", Action = "JournalVoucherEdit", id = s["JournalMasterId"] })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-edit\"></i></a> <a title=\"Delete\" href=\"{this.Url.Link("Default", new { Controller = "AccountTransaction", Action = "JournalVoucherDelete", id = s["JournalMasterId"] })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-trash\"></i></a> "
//            });

//            var jsonString = JsonConvert.SerializeObject(details);
//            return jsonString;
//        }
//        [Route("PDCPayable")]
//        public string getdataforPDCPayable(string FromDate, string ToDate, string LedgerId, string VoucherNo)
//        {
//            TransactionsGeneralFill Obj = new TransactionsGeneralFill();
//            PDCPayableMasterSP Master = new PDCPayableMasterSP();
//            DataTable dtbl = new DataTable();
//            if (VoucherNo == null || VoucherNo == "undefined")
//            {
//                VoucherNo = "";
//            }
//            if (FromDate == null || ToDate == null)
//            {
//                FromDate = DateTime.Now.ToString();
//                ToDate = DateTime.Now.ToString();
//            }

//            dtbl = Master.PDCpayableRegisterSearch(Convert.ToDateTime(FromDate.ToString()), Convert.ToDateTime(ToDate), VoucherNo, LedgerId);
//            var details = dtbl.AsEnumerable().Select(s => new {
//                SlNo = s["SlNo"],
//                invoiceNo = s["invoiceNo"],
//                voucherTypeName = s["voucherTypeName"],
//                date = s["date"],
//                narration = s["narration"],
//                ledgerName = s["ledgerName"],
//                amount = s["amount"],
//                action = $"<a title=\"Edit\" href=\"{this.Url.Link("Default", new { Controller = "AccountTransaction", Action = "PDCpayableEdit", id = s["pdcPayableMasterId"] })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-edit\"></i></a> <a title=\"Delete\" href=\"{this.Url.Link("Default", new { Controller = "AccountTransaction", Action = "PDCpayableDelete", id = s["pdcPayableMasterId"] })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-trash\"></i></a> "
//            });

//            var jsonString = JsonConvert.SerializeObject(details);
//            return jsonString;
//        }

//        [Route("PDCReceiveable")]
//        public string getdataforPDCReceiveable(string FromDate, string ToDate, string LedgerId, string VoucherNo)
//        {
//            TransactionsGeneralFill Obj = new TransactionsGeneralFill();
//            PDCReceivableMasterSP Master = new PDCReceivableMasterSP();
//            DataTable dtbl = new DataTable();
//            if (VoucherNo == null || VoucherNo.ToLower().Trim() == "undefined".ToLower().Trim())
//            {
//                VoucherNo = "";
//            }
//            if (FromDate == null || ToDate == null)
//            {
//                FromDate = DateTime.Now.ToString();
//                ToDate = DateTime.Now.ToString();
//            }

//            dtbl = Master.PDCReceivableRegisterSearch(Convert.ToDateTime(FromDate.ToString()), Convert.ToDateTime(ToDate), VoucherNo, LedgerId);
//            var details = dtbl.AsEnumerable().Select(s => new {
//                SlNo = s["SlNo"],
//                invoiceNo = s["invoiceNo"],
//                voucherTypeName = s["voucherTypeName"],
//                date = s["date"],
//                narration = s["narration"],
//                ledgerName = s["ledgerName"],
//                amount = s["amount"],
//                action = $"<a title=\"Edit\" href=\"{this.Url.Link("Default", new { Controller = "AccountTransaction", Action = "PDCReceiveableEdit", id = s["pdcReceivableMasterId"] })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-edit\"></i></a> <a title=\"Delete\" href=\"{this.Url.Link("Default", new { Controller = "AccountTransaction", Action = "PDCReceiveableDelete", id = s["pdcReceivableMasterId"] })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-trash\"></i></a> "
//            });

//            var jsonString = JsonConvert.SerializeObject(details);
//            return jsonString;
//        }

//        [Route("PDCclearance")]
//        public string getdataforPDCclearance(string FromDate, string ToDate, string AccountLedger, string Status, string ChequeNo, string ChequeDate, string Bank, string voucherTypeId="")
//        {
//            PDCClearanceMasterSP sppdcClearance = new PDCClearanceMasterSP();
         
//            if (voucherTypeId == null)
//            {
//                voucherTypeId = "";
//            }
//            if (AccountLedger == null)
//            {
//                AccountLedger = "";
//            }
//            if (Status == null)
//            {
//                Status = "";
//            }
//            if (Bank == null)
//            {
//                Bank = "0";
//            }
//            if (ChequeDate == null)
//            {
//                Status = "";
//            }
//            if (ChequeNo == null)
//            {
//                ChequeNo = "";
//            }
//            if (FromDate == null || ToDate == null || FromDate == "" || ToDate == "")
//            {
//                FromDate = DateTime.Now.ToString();
//                ToDate = DateTime.Now.ToString();
//            }
//            DataTable dtbl = new DataTable();
//            PDCClearanceMasterSP SppdcClearance = new PDCClearanceMasterSP();
//            dtbl = SppdcClearance.PDCClearanceRegisterSearch(Convert.ToDateTime(FromDate.ToString()), Convert.ToDateTime(ToDate.ToString()), AccountLedger, voucherTypeId, ChequeNo.Trim(), Convert.ToDecimal(Bank), Status);
//            var details = dtbl.AsEnumerable().Select(s => new {
//                SlNo = s["SlNo"],
//                invoiceNo = s["invoiceNo"],
//                voucherTypeName = s["voucherTypeName"],
//                date = s["date"],
//                narration = s["narration"],
//                ledgerName = s["ledgerName"],
//                amount = s["amount"],
//                action = $"<a title=\"Edit\" href=\"{this.Url.Link("Default", new { Controller = "AccountTransaction", Action = "PDCClearanceEdit", id = s["pdcClearanceMasterId"] })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-edit\"></i></a> <a title=\"Delete\" href=\"{this.Url.Link("Default", new { Controller = "AccountTransaction", Action = "PDCClearanceDelete", id = s["pdcClearanceMasterId"] })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-trash\"></i></a> "
//            });

//            var jsonString = JsonConvert.SerializeObject(details);
//            return jsonString;
//        }
//    }
//}