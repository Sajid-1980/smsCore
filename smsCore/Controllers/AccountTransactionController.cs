using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
using sms.Helpers;
using smsCore.Data;
using System.Collections;
using System.Data;

namespace smsCore.Controllers
{
    [Authorize]
    public class AccountTransactionController : BaseController
    {
        private readonly SchoolEntities _db ;
        private readonly CurrentUser _user;

        public AccountTransactionController(SchoolEntities db , CurrentUser user)
        {
            _db = db;
            _user = user;
        }
        DataTable dtblPartyBalance = new DataTable();
        // GET: AccountTransaction
        #region Contra Voucher   
       // [AuthorizationActionFilter(EnumManager.Actions.Any, EnumManager.Modules.ContraVoucher)]
        public IActionResult indexContra()
        {
            TransactionsGeneralFill Obj = new TransactionsGeneralFill();
            var Ledgers = Obj.BankOrCashComboFill(false).AsEnumerable().Select(s => new
            {
                LedgerId = s["LedgerName"].ToString(),
                LedgerName = s["LedgerName"].ToString()
            });
            ViewBag.LedgerLists = new SelectList(Ledgers, "LedgerId", "LedgerName");
            return View();
        }
      //  [AuthorizationActionFilter(EnumManager.Actions.Save, EnumManager.Modules.ContraVoucher)]
        public ActionResult ContraVoucher()
        {
            DailySalaryVoucherMasterSP spMaster = new DailySalaryVoucherMasterSP();
            var Ledgers = spMaster.DailySalaryVoucherCashOrBankLedgersComboFill().AsEnumerable().Select(s => new
            {
                LedgerId = (decimal)s["LedgerId"],
                LedgerName = s["LedgerName"].ToString()
            });
            ViewBag.LedgerLists = new SelectList(Ledgers, "LedgerId", "LedgerName");
            return View();
        }
      //  [AuthorizationActionFilter(EnumManager.Actions.Save, EnumManager.Modules.ContraVoucher)]
        [HttpPost]
        public string ContraVoucher(ContraMasterSP sss)
        {
            var isAutomatic = _user.Isautomatic();
            var messages = "";
            decimal DecContraVoucherTypeId = 3;

            decimal decContraSuffixPrefixId = 0;
            string strStatus = string.Empty;
            var radio = Request.Form["radio"].ToString();
            var dgvcmbBankorCashAccount = Request.Form["LegerId"].ToString().Split(',');
            ContraMasterSP spContraMaster = new ContraMasterSP();
            AccountLedgerSP spAccountLedger = new AccountLedgerSP();
            SettingsSP spSettings = new SettingsSP();
            if(Request.Form["VoucherNo"].ToString()==null)
            {
                return "Voucher No Is Invalid!";
            }
            var voucherNo = Request.Form["VoucherNo"].ToString().Trim();
            if (!isAutomatic)
            {
                if (spContraMaster.ContraVoucherCheckExistence(voucherNo, DecContraVoucherTypeId, 0) == true)
                {
                }
               
            }
            else
            {
                if (spContraMaster.ContraVoucherCheckExistence(voucherNo, DecContraVoucherTypeId, 0) == true)
                {
                    return "Voucher number already exist";

                }
              
            }
            try
            {
                decimal decContraDetailsId = 0;
                ContraMasterInfo infoContraMaster = new ContraMasterInfo();
                ContraDetailsSP spContraDetails = new ContraDetailsSP();
                ContraDetailsInfo infoCOntraDetails = new ContraDetailsInfo();
                ExchangeRateSP spExchangeRate = new ExchangeRateSP();
                decimal decIdentity = 0;
                decimal decLedgerId = 0;
                decimal decDebit = 0;
                decimal decCredit = 0;
                int inValue = 0;
                for (int i = 0; i < dgvcmbBankorCashAccount.Count(); i++)
                {
                    if (dgvcmbBankorCashAccount[i] != null &&
                        dgvcmbBankorCashAccount[i].ToString() != null)
                    {
                        inValue++;
                    }
                }
                var strVoucherNo = string.Empty;
                TransactionsGeneralFill obj = new TransactionsGeneralFill();
                if (strVoucherNo == string.Empty)
                {
                    strVoucherNo = "0";
                }
                strVoucherNo = obj.VoucherNumberAutomaicGeneration(DecContraVoucherTypeId, Convert.ToDecimal(strVoucherNo), DateTime.Parse(Request.Form["date"]), "ContraMaster");
                if (Convert.ToDecimal(strVoucherNo) != spContraMaster.ContraVoucherMasterGetMaxPlusOne(DecContraVoucherTypeId))
                {
                    strVoucherNo = spContraMaster.ContraVoucherMasterGetMax(DecContraVoucherTypeId).ToString();
                    strVoucherNo = obj.VoucherNumberAutomaicGeneration(DecContraVoucherTypeId, Convert.ToDecimal(strVoucherNo), DateTime.Parse(Request.Form["date"]), "ContraMaster");
                    if (spContraMaster.ContraVoucherMasterGetMax(DecContraVoucherTypeId) == "0")
                    {
                        strVoucherNo = "0";
                        strVoucherNo = obj.VoucherNumberAutomaicGeneration(DecContraVoucherTypeId, Convert.ToDecimal(strVoucherNo), DateTime.Parse(Request.Form["date"]), "ContraMaster");
                    }
                }
                SuffixPrefixSP spSuffisprefix = new SuffixPrefixSP();
                SuffixPrefixInfo infoSuffixPrefix = new SuffixPrefixInfo();
                infoSuffixPrefix = spSuffisprefix.GetSuffixPrefixDetails(DecContraVoucherTypeId, DateTime.Parse(Request.Form["date"]));
                var strPrefix = infoSuffixPrefix.Prefix;
                var strSuffix = infoSuffixPrefix.Suffix;
                decContraSuffixPrefixId = infoSuffixPrefix.SuffixprefixId;

                var strInvoiceNo = strPrefix + strVoucherNo + strSuffix;
                if (inValue > 0)
                {
                    if (Convert.ToDecimal(Request.Form["TotalAmount"]) != 0)
                    {
                        infoContraMaster.LedgerId = Convert.ToDecimal(Request.Form["Legers"].ToString());
                        if (isAutomatic)
                        {
                            infoContraMaster.VoucherNo = strVoucherNo;
                        }
                        else
                        {
                            decimal decVoucherNo = spContraMaster.ContraVoucherMasterGetMaxPlusOne(DecContraVoucherTypeId) + 1;
                            infoContraMaster.VoucherNo = Convert.ToString(decVoucherNo);
                        }
                        infoContraMaster.Date = Convert.ToDateTime(Request.Form["date"].ToString());
                        var narration = "";
                        if(Request.Form["Narration"].ToString().Trim()!=null)
                        {
                            narration = Request.Form["Narration"].ToString().Trim();
                        }
                        infoContraMaster.Narration =narration;
                        infoContraMaster.TotalAmount = Convert.ToDecimal(Request.Form["TotalAmount"].ToString());
                        infoContraMaster.Extra1 = string.Empty;
                        infoContraMaster.Extra2 = string.Empty;
                        if (radio == "deposit")
                        {
                            infoContraMaster.Type = "Deposit";
                        }
                        else
                        {
                            infoContraMaster.Type = "Withdraw";
                        }
                        infoContraMaster.SuffixPrefixId = decContraSuffixPrefixId;
                        infoContraMaster.VoucherTypeId = DecContraVoucherTypeId;
                        infoContraMaster.FinancialYearId = Convert.ToDecimal(PublicVariables._decCurrentFinancialYearId.ToString());
                        infoContraMaster.UserId = _user.UserID;
                        if (isAutomatic)
                        {
                            infoContraMaster.InvoiceNo = strInvoiceNo;
                        }
                        else
                        {
                            infoContraMaster.InvoiceNo =voucherNo;
                        }
                        decIdentity = spContraMaster.ContraMasterAdd(infoContraMaster);
                        infoCOntraDetails.ContraMasterId = decIdentity;
                        infoCOntraDetails.Extra1 = string.Empty;
                        infoCOntraDetails.Extra2 = string.Empty;
                        var dgvtxtAmount = Request.Form["Amount"].ToString().Split(',');
                        var dgvtxtChequeNo = Request.Form["ChequeNumber"].ToString().Split(',');
                        var dgvtxtChequeDate = Request.Form["ChequeDate"].ToString().Split(',');
                        var dgvcmbCurrency = PublicVariables._decCurrencyId;

                        //-------------------------------Saving grid details--------------------------------------------------------------------
                        for (int i = 0; i < dgvcmbBankorCashAccount.Count(); i++)
                        {
                            if (dgvcmbBankorCashAccount[i] != null && dgvcmbBankorCashAccount[i].ToString() != string.Empty)
                            {
                                infoCOntraDetails.LedgerId = Convert.ToDecimal(dgvcmbBankorCashAccount[i].ToString());
                            }
                            if (int.Parse(dgvtxtAmount[i]) >0)
                            {
                                infoCOntraDetails.Amount = Convert.ToDecimal(dgvtxtAmount[i].ToString());
                            }
                            if (dgvtxtChequeNo[i]!=null && dgvtxtChequeNo[i] !="")
                            {
                                infoCOntraDetails.ChequeNo = dgvtxtChequeNo[i].ToString();
                            }
                            else
                            {
                                infoCOntraDetails.ChequeNo = string.Empty;
                            }
                            if (dgvtxtChequeDate[i]!= null && dgvtxtChequeDate[i] !="")
                            {
                                infoCOntraDetails.ChequeDate = Convert.ToDateTime(dgvtxtChequeDate[i].ToString());
                            }
                            else
                            {
                                infoCOntraDetails.ChequeDate = Convert.ToDateTime("1/1/1753");
                            }
                            infoCOntraDetails.ExchangeRateId = Convert.ToDecimal(dgvcmbCurrency);
                            decContraDetailsId = spContraDetails.ContraDetailsAddReturnWithhIdentity(infoCOntraDetails);
                            //---------------------------------------------------------Ledger Posting-----------------------------------------/---------------------------------------------------/
                            if (dgvcmbBankorCashAccount[i] != null && dgvcmbBankorCashAccount[i].ToString() != string.Empty)
                            {
                                decLedgerId = Convert.ToDecimal(dgvcmbBankorCashAccount[i].ToString());
                            }
                            if (int.Parse(dgvtxtAmount[i]) > 0 && dgvtxtAmount[i].ToString() != string.Empty)
                            {
                                var decSelectedCurrencyRate = spExchangeRate.GetExchangeRateByExchangeRateId(Convert.ToDecimal(dgvcmbCurrency));
                                var Amount = Convert.ToDecimal(dgvtxtAmount[i].ToString());
                                var decConvertRate = Amount * decSelectedCurrencyRate;
                                if (radio == "deposite")
                                {
                                    decCredit = decConvertRate;
                                    decDebit = 0;
                                    LedgerPosting(voucherNo, decLedgerId, decCredit, decDebit, decContraDetailsId, i, dgvtxtChequeDate[i].ToString(), dgvtxtChequeNo[i].ToString(), strVoucherNo, strInvoiceNo);
                                }
                                else
                                {
                                    decDebit = decConvertRate;
                                    decCredit = 0;
                                    LedgerPosting(voucherNo, decLedgerId, decCredit, decDebit, decContraDetailsId, i, dgvtxtChequeDate[i].ToString(), dgvtxtChequeNo[i].ToString(), strVoucherNo, strInvoiceNo);
                                }
                            }
                        }
                        var decAmount = Convert.ToDecimal(Request.Form["totalAmount"].ToString());
                        decContraDetailsId = 0;
                        if (radio == "deposite")
                        {
                            decDebit = decAmount;
                            decCredit = 0;
                            LedgerPosting(voucherNo, infoContraMaster.LedgerId, decCredit, decDebit, decContraDetailsId, -1, "", "", strVoucherNo, strInvoiceNo);
                        }
                        else
                        {
                            decCredit = decAmount;
                            decDebit = 0;
                            LedgerPosting(voucherNo, infoContraMaster.LedgerId, decCredit, decDebit, decContraDetailsId, -1, "", "", strVoucherNo, strInvoiceNo);
                        }
                        //------------------------------------------------------------------Ledger Posting---end---------------------------------------------------------------------//
                        messages = "success";

                    }

                    else
                    {
                        messages = "Cant save contra voucher without atleast one ledger with complete details";
                    }
                }
            }
            catch (Exception ex)
            {
                messages = ex.Message;
            }
            
           
            return messages;
        }
       // [AuthorizationActionFilter(EnumManager.Actions.Edit, EnumManager.Modules.ContraVoucher)]
        public IActionResult ContraVoucherEdit(decimal id)
        {
            ContraDetailsSP spContraDetails = new ContraDetailsSP();
            ContraMasterSP spContraMaster = new ContraMasterSP();
            ContraMasterInfo ContraMasterinfo = new ContraMasterInfo();
            LedgerPostingSP SpLedgerPosting = new LedgerPostingSP();
            VoucherTypeSP spVoucherType = new VoucherTypeSP();
            ContraMasterinfo = spContraMaster.ContraMasterView(id);
          var  dtbl = spContraDetails.ContraDetailsViewWithMasterId(id);
          var     strVoucherNo = ContraMasterinfo.VoucherNo;

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("DetailsId");
            dataTable.Columns.Add("Bank/Cash");
            //dataTable.Columns.Add("Currency");
            dataTable.Columns.Add("Amount");
            dataTable.Columns.Add("ChequeNo");
            dataTable.Columns.Add("ChequeDate");
            dataTable.Columns.Add("Masterid");
            dataTable.Columns.Add("VoucherNo");
            dataTable.Columns.Add("Date");
            dataTable.Columns.Add("LedgerId");
            dataTable.Columns.Add("Narration");
            dataTable.Columns.Add("dgvtxtLedgerPostingId");
            decimal totalamount = 0;
            var radio = "false";
            if (ContraMasterinfo.Type == "Deposit")
            {
                radio= "true";
            }
           if(dtbl.Rows.Count==0)
            {
                DataRow dr = dataTable.NewRow();
                dataTable.Rows.Add(dr);
                dr["Masterid"] = ContraMasterinfo.ContraMasterId;
                dr["VoucherNo"] = ContraMasterinfo.VoucherNo;
                dr["Date"] = ContraMasterinfo.Date;
                dr["LedgerId"] = ContraMasterinfo.LedgerId;
                dr["Narration"] = ContraMasterinfo.Narration;
            }
            for (int i = 0; i < dtbl.Rows.Count; i++)
            {
                DataRow dr = dataTable.NewRow();
                dataTable.Rows.Add(dr);
                dr["Masterid"] = ContraMasterinfo.ContraMasterId;
                dr["VoucherNo"] = ContraMasterinfo.VoucherNo;
                dr["Date"] = ContraMasterinfo.Date;
                dr["LedgerId"] = ContraMasterinfo.LedgerId;
                dr["Narration"] = ContraMasterinfo.Narration;
                dr["DetailsId"] = decimal.Parse(dtbl.Rows[i]["ContraDetailsId"].ToString());
                dr["Bank/Cash"] = dtbl.Rows[i]["LedgerId"].ToString();
                dr["Amount"] = dtbl.Rows[i]["Amount"].ToString();
                totalamount += decimal.Parse(dtbl.Rows[i]["Amount"].ToString());
                dr["ChequeDate"] = dtbl.Rows[i]["ChequeDate"].ToString();
                dr["ChequeNo"] = dtbl.Rows[i]["ChequeNo"].ToString();
                //dr["Currency"] = dtbl.Rows[i]["ExchangeRateId"].ToString();
                decimal decDetailsId1 = Convert.ToDecimal(dtbl.Rows[i]["contraDetailsId"].ToString());
                decimal decLedgerPostingId = SpLedgerPosting.LedgerPostingIdFromDetailsId(decDetailsId1, strVoucherNo, 3);
                dr["dgvtxtLedgerPostingId"] = decLedgerPostingId.ToString();
            }

            DailySalaryVoucherMasterSP spMaster = new DailySalaryVoucherMasterSP();
            var Ledgers = spMaster.DailySalaryVoucherCashOrBankLedgersComboFill().AsEnumerable().Select(s => new
            {
                LedgerId = (decimal)s["LedgerId"],
                LedgerName = s["LedgerName"].ToString()
            });
            ViewBag.LedgerLists = new SelectList(Ledgers, "LedgerId", "LedgerName");
            ViewBag.totalamount = totalamount;
            ViewBag.radio = radio;
            return View(dataTable);

        }
       // [AuthorizationActionFilter(EnumManager.Actions.Edit, EnumManager.Modules.ContraVoucher)]
        [HttpPost]
        public string ContraVoucherEdit()
        {
            string message;
            try
            {
                decimal Masterid =decimal.Parse(Request.Form["Masterid"].ToString());
                ArrayList arrlstOfRemove = new ArrayList();
                decimal DecContraVoucherTypeId = 3;
                var strVoucherNo = string.Empty;
                TransactionsGeneralFill obj = new TransactionsGeneralFill();
                ContraMasterSP spContraMaster = new ContraMasterSP();
                bool isAutomatic = Helpers.CurrentUser.Isautomatic();
                var voucherNo = Request.Form["VoucherNo"];
                ContraDetailsSP spContraDetails = new ContraDetailsSP();
                ContraMasterInfo infoContraMaster = new ContraMasterInfo();
                LedgerPostingSP SpLedgerPosting = new LedgerPostingSP();
                VoucherTypeSP spVoucherType = new VoucherTypeSP();
                infoContraMaster = spContraMaster.ContraMasterView(Masterid);
                strVoucherNo = infoContraMaster.VoucherNo;               
              var  strInvoiceNo = infoContraMaster.InvoiceNo;
               decimal decContraSuffixPrefixId = Convert.ToDecimal(infoContraMaster.SuffixPrefixId.ToString());
                DecContraVoucherTypeId = Convert.ToDecimal(infoContraMaster.VoucherTypeId.ToString());
                int inDecimalPlace = PublicVariables._inNoOfDecimalPlaces;
                strVoucherNo = infoContraMaster.VoucherNo;    
                ContraDetailsInfo infoCOntraDetails = new ContraDetailsInfo();
                ExchangeRateSP spExchangeRate = new ExchangeRateSP();
                LedgerPostingSP spLedgerPosting = new LedgerPostingSP();
                decimal decContraDetailsId = 0;
                decimal decLedgerPostId = 0;
                infoContraMaster.ContraMasterId = Masterid;
                infoContraMaster.LedgerId = Convert.ToDecimal(Request.Form["Legers"].ToString());
                if (isAutomatic)
                {
                    infoContraMaster.VoucherNo = strVoucherNo;
                }
                if (isAutomatic)
                {
                    infoContraMaster.InvoiceNo = strInvoiceNo;
                }
                else
                {
                    infoContraMaster.InvoiceNo = Request.Form["VoucherNo"].ToString();
                }
                infoContraMaster.Date = Convert.ToDateTime(Request.Form["date"].ToString());
                infoContraMaster.Narration = Request.Form["Narration"].ToString().Trim();
                infoContraMaster.TotalAmount = Convert.ToDecimal(Request.Form["totalAmount"]);
                var radio = Request.Form["radio"];
                if (radio == "deposit")
                    infoContraMaster.Type = "Deposit";
                else
                    infoContraMaster.Type = "Withdraw";
                infoContraMaster.SuffixPrefixId = decContraSuffixPrefixId;
                infoContraMaster.VoucherTypeId = DecContraVoucherTypeId;
                infoContraMaster.FinancialYearId = Convert.ToDecimal(PublicVariables._decCurrentFinancialYearId.ToString());
                infoContraMaster.UserId =_user.UserID; //PublicVariables._decCurrentUserId;
                infoContraMaster.Extra1 = string.Empty;
                infoContraMaster.Extra2 = string.Empty;
                infoContraMaster.Date = Convert.ToDateTime(Request.Form["date"]);
                int inValue = 0;
                var dgvtxtAmount = Request.Form["Amount"].ToString().Split(',');
                var dgvtxtChequeNo = Request.Form["ChequeNumber"].ToString().Split(',');
                var dgvtxtChequeDate = Request.Form["ChequeDate"].ToString().Split(',');
                var dgvtxtDetailsId = Request.Form["DetailsId"].ToString().Split(',');
                //var dgvcmbCurrency = Request.Form["Currency"].Split(',');
                var dgvcmbBankorCashAccount = Request.Form["LegerId"].ToString().Split(',');
                var dgvtxtLedgerPostingId =Request.Form["dgvtxtLedgerPostingId"].ToString().Split(',');
                int detailsidcount = dgvtxtDetailsId.Count();
                for (int i = 0; i < dgvcmbBankorCashAccount.Count(); i++)
                {
                    if (dgvcmbBankorCashAccount[i] != null &&
                       dgvcmbBankorCashAccount[i].ToString() != null)
                    {
                        inValue++;
                    }
                }

                if (inValue > 0)
                {
                    if (Convert.ToDecimal(Request.Form["totalAmount"].ToString()) != 0)
                    {
                        infoCOntraDetails.ContraMasterId =Masterid;
                        infoCOntraDetails.Extra1 = string.Empty;
                        infoCOntraDetails.Extra2 = string.Empty;
                        //------------------deleting removed rows----------------------------------------//
                        foreach (var item in arrlstOfRemove)
                        {
                            decimal decId = Convert.ToDecimal(item);
                            spContraDetails.ContraDetailsDelete(Convert.ToDecimal(decId));
                            spLedgerPosting.LedgerPostDeleteByDetailsId(Convert.ToDecimal(decId), strVoucherNo, 3);
                        }
                        //--------------------------End---------------------------------------------------//
                        decimal decLedgerId = 0;
                        decimal decLedger1Id = 0;
                        decimal decDebit = 0;
                        decimal decCredit = 0;
                        //decLedger1Id =decimal.Parse(Request.Form["Leger"].ToString());
                        for (int i = 0; i < dgvcmbBankorCashAccount.Count(); i++)
                        {
                            if (dgvcmbBankorCashAccount[i] != null && dgvcmbBankorCashAccount[i].ToString() != "")
                            {
                                decLedgerId = Convert.ToDecimal(dgvcmbBankorCashAccount[i].ToString());
                                infoCOntraDetails.LedgerId = Convert.ToDecimal(dgvcmbBankorCashAccount[i].ToString());
                            }
                            if (dgvtxtChequeNo[i] != null && dgvtxtChequeNo[i].ToString() != "")
                            {
                                infoCOntraDetails.ChequeNo = dgvtxtChequeNo[i].ToString();
                            }
                            else
                            {
                                infoCOntraDetails.ChequeNo = string.Empty;
                            }
                            if (dgvtxtChequeDate[i] != null && dgvtxtChequeDate[i].ToString() != "")
                            {
                                infoCOntraDetails.ChequeDate = Convert.ToDateTime(dgvtxtChequeDate[i].ToString());
                            }
                            else
                            {
                                infoCOntraDetails.ChequeDate = Convert.ToDateTime("1/1/1753");
                            }
                            if (dgvtxtAmount[i] != null && dgvtxtAmount[i].ToString() != "")
                            {

                                infoCOntraDetails.Amount = Convert.ToDecimal(dgvtxtAmount[i].ToString());
                                detailsidcount--;
                                if (detailsidcount >=0)
                                {
                                    infoCOntraDetails.ContraDetailsId = Convert.ToDecimal(dgvtxtDetailsId[i].ToString());
                                    decContraDetailsId = infoCOntraDetails.ContraDetailsId;
                                    //var  decSelectedCurrencyRate = spExchangeRate.GetExchangeRateByExchangeRateId(Convert.ToDecimal(dgvcmbCurrency[i].ToString()));
                                    var decAmounts = Convert.ToDecimal(dgvtxtAmount[i].ToString());
                                    var decConvertRate = decAmounts * 1;//decSelectedCurrencyRate;
                                    if (radio == "deposit")
                                    {
                                        decCredit = decConvertRate;
                                        decDebit = 0;
                                    }
                                    else
                                    {
                                        decDebit = decConvertRate;
                                        decCredit = 0;
                                    }
                                    infoCOntraDetails.ContraMasterId = decimal.Parse(Request.Form["MasterId"].ToString());
                                    //infoCOntraDetails.ExchangeRateId = Convert.ToDecimal(dgvcmbCurrency[i].ToString());
                                    spContraDetails.ContraDetailsEdit(infoCOntraDetails);
                                    decLedgerPostId = Convert.ToDecimal(dgvtxtLedgerPostingId[i].ToString());
                                    LedgerPostingEdit(decLedgerPostId, decLedgerId, decCredit, decDebit, decContraDetailsId, i, dgvtxtChequeNo[i], dgvtxtChequeDate[i], Request.Form["VoucherNo"], strVoucherNo, DecContraVoucherTypeId);
                                }
                                else
                                {
                                    //    infoCOntraDetails.ExchangeRateId = Convert.ToDecimal(dgvcmbCurrency[i].ToString());
                                    //  var  decSelectedCurrencyRate = spExchangeRate.GetExchangeRateByExchangeRateId(Convert.ToDecimal(dgvcmbCurrency[i].ToString()));
                                    var decAmounts = Convert.ToDecimal(dgvtxtAmount[i].ToString());
                                    var decConvertRate = decAmounts * 1;//decSelectedCurrencyRate;
                                    if (radio == "deposit")
                                    {
                                        decCredit = decConvertRate;
                                        decDebit = 0;
                                    }
                                    else
                                    {
                                        decDebit = decConvertRate;
                                        decCredit = 0;
                                    }
                                    decContraDetailsId = spContraDetails.ContraDetailsAddReturnWithhIdentity(infoCOntraDetails);
                                    LedgerPosting(voucherNo, decLedgerId, decCredit, decDebit, decContraDetailsId, i, dgvtxtChequeDate[i].ToString(), dgvtxtChequeNo[i].ToString(), strVoucherNo, strInvoiceNo);
                                    //LedgerPostingEdit(decLedgerPostId, decLedgerId, decCredit, decDebit, decContraDetailsId, i, dgvtxtChequeNo[i], dgvtxtChequeDate[i], Request.Form["VoucherNo"], strVoucherNo, DecContraVoucherTypeId);
                                }
                            }
                        }
                        spContraMaster.ContraMasterEdit(infoContraMaster);
                        decLedgerPostId = spLedgerPosting.LedgerPostingIdForTotalAmount(strVoucherNo, DecContraVoucherTypeId);
                        var decAmount = Convert.ToDecimal(Request.Form["totalAmount"]);
                        decContraDetailsId = 0;
                        if (radio == "deposit")
                        {
                            decDebit = decAmount;
                            decCredit = 0;
                            LedgerPostingEdit(decLedgerPostId, decLedger1Id, decCredit, decDebit, decContraDetailsId, -1, "", "", "", "", 0);
                        }
                        else
                        {
                            decCredit = decAmount;
                            decDebit = 0;
                            LedgerPostingEdit(decLedgerPostId, decLedger1Id, decCredit, decDebit, decContraDetailsId, -1, "", "", "", "", 0);
                        }
                        message = "success";
                    }
                    else
                    {
                        message = "Cannot save total amount as 0";
                    }
                }
                else
                {
                    message = "Cant update contra voucher without atleast one ledger with complete details";

                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return message;
        }
     //   [AuthorizationActionFilter(EnumManager.Actions.Delete, EnumManager.Modules.ContraVoucher)]
        public IActionResult ContraVoucherDelete(decimal id)
        {

            ContraDetailsSP spContraDetails = new ContraDetailsSP();
            ContraMasterSP spContraMaster = new ContraMasterSP();
            ContraMasterInfo ContraMasterinfo = new ContraMasterInfo();
            LedgerPostingSP SpLedgerPosting = new LedgerPostingSP();
            VoucherTypeSP spVoucherType = new VoucherTypeSP();
            ContraMasterinfo = spContraMaster.ContraMasterView(id);
            var dtbl = spContraDetails.ContraDetailsViewWithMasterId(id);
            var strVoucherNo = ContraMasterinfo.VoucherNo;

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("DetailsId");
            dataTable.Columns.Add("Bank/Cash");
            //dataTable.Columns.Add("Currency");
            dataTable.Columns.Add("Amount");
            dataTable.Columns.Add("ChequeNo");
            dataTable.Columns.Add("ChequeDate");
            dataTable.Columns.Add("Masterid");
            dataTable.Columns.Add("VoucherNo");
            dataTable.Columns.Add("Date");
            dataTable.Columns.Add("LedgerId");
            dataTable.Columns.Add("Narration");
            dataTable.Columns.Add("dgvtxtLedgerPostingId");
            decimal totalamount = 0;
            var radio = "false";
            if (ContraMasterinfo.Type == "Deposit")
            {
                radio = "true";
            }

            for (int i = 0; i < dtbl.Rows.Count; i++)
            {
                DataRow dr = dataTable.NewRow();
                dataTable.Rows.Add(dr);
                dr["Masterid"] = ContraMasterinfo.ContraMasterId;
                dr["VoucherNo"] = ContraMasterinfo.VoucherNo;
                dr["Date"] = ContraMasterinfo.Date;
                dr["LedgerId"] = ContraMasterinfo.LedgerId;
                dr["Narration"] = ContraMasterinfo.Narration;
                dr["DetailsId"] = decimal.Parse(dtbl.Rows[i]["ContraDetailsId"].ToString());
                dr["Bank/Cash"] = dtbl.Rows[i]["LedgerId"].ToString();
                dr["Amount"] = dtbl.Rows[i]["Amount"].ToString();
                totalamount += decimal.Parse(dtbl.Rows[i]["Amount"].ToString());
                dr["ChequeDate"] = dtbl.Rows[i]["ChequeDate"].ToString();
                dr["ChequeNo"] = dtbl.Rows[i]["ChequeNo"].ToString();
                //dr["Currency"] = dtbl.Rows[i]["ExchangeRateId"].ToString();
                decimal decDetailsId1 = Convert.ToDecimal(dtbl.Rows[i]["contraDetailsId"].ToString());
                decimal decLedgerPostingId = SpLedgerPosting.LedgerPostingIdFromDetailsId(decDetailsId1, strVoucherNo, 3);
                dr["dgvtxtLedgerPostingId"] = decLedgerPostingId.ToString();
            }

            DailySalaryVoucherMasterSP spMaster = new DailySalaryVoucherMasterSP();
            var Ledgers = spMaster.DailySalaryVoucherCashOrBankLedgersComboFill().AsEnumerable().Select(s => new
            {
                LedgerId = (decimal)s["LedgerId"],
                LedgerName = s["LedgerName"].ToString()
            });
            ViewBag.LedgerLists = new SelectList(Ledgers, "LedgerId", "LedgerName");
           
            ViewBag.totalamount = totalamount;
            ViewBag.radio = radio;
            return View(dataTable);
        }
      //  [AuthorizationActionFilter(EnumManager.Actions.Delete, EnumManager.Modules.ContraVoucher)]
        [HttpPost]
        public string ContraVoucherDelete()
        {
            string message;
            decimal DecContraVoucherTypeId =3;
            var decContraMasterId = int.Parse(Request.Form["MasterId"].ToString());
            ContraMasterSP contraMasterSP = new ContraMasterSP();
            ContraMasterInfo infoContraMaster = new ContraMasterInfo();
            infoContraMaster = contraMasterSP.ContraMasterView(decContraMasterId);
            var strVoucherNo = infoContraMaster.VoucherNo;
            ContraDetailsSP spContraDetails = new ContraDetailsSP();
            try
            {
                spContraDetails.ContraVoucherDelete(decContraMasterId, DecContraVoucherTypeId, strVoucherNo);
                message = "success";
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            //Messages.DeletedMessage();
            return message;
        }
       // [AuthorizationActionFilter(EnumManager.Actions.Delete, EnumManager.Modules.ContraVoucher)]
        [HttpPost]
        public string DeletecontraDetails(int id,int masterId)
        {
            ContraDetailsSP spContraDetails = new ContraDetailsSP();
            LedgerPostingSP postingSP = new LedgerPostingSP();
            ContraMasterInfo infoContraMaster = new ContraMasterInfo();
            ContraMasterSP contraMasterSP = new ContraMasterSP();
            VoucherTypeSP spVoucherType = new VoucherTypeSP();
            infoContraMaster = contraMasterSP.ContraMasterView(masterId);
           var strVoucherNo = infoContraMaster.VoucherNo;
            try
            {
                postingSP.LedgerPostDeleteByDetailsId(id, strVoucherNo, 3);
                spContraDetails.ContraDetailsDelete(id);//.ContraVoucherDelete(masterId, 3, strVoucherNo);
                return  "true";
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }
        public void LedgerPosting(string voucherNo, decimal decid, decimal decCredit, decimal decDebit, decimal decDetailsId, int inI, string chequeDate, string chequeNo, string strVoucherNo,string InvoiceNo)
        {
            decimal DecContraVoucherTypeId =3;
            bool isAutomatic = _user.Isautomatic();
            var strInvoiceNo = string.Empty;
            try
            {
                LedgerPostingSP spLedgerPosting = new LedgerPostingSP();
                LedgerPostingInfo infoLedgerPosting = new LedgerPostingInfo();
                infoLedgerPosting.VoucherTypeId = DecContraVoucherTypeId;
                TransactionsGeneralFill obj = new TransactionsGeneralFill();
                ContraMasterSP spContraMaster = new ContraMasterSP();
                strInvoiceNo = InvoiceNo;
                if (isAutomatic)
                {
                    infoLedgerPosting.VoucherNo = strVoucherNo;
                }
                else
                {
                    infoLedgerPosting.VoucherNo = voucherNo;
                }
                infoLedgerPosting.Date = PublicVariables._dtCurrentDate;
                infoLedgerPosting.LedgerId = decid;
                infoLedgerPosting.DetailsId = decDetailsId;
                infoLedgerPosting.Debit = decDebit;
                infoLedgerPosting.Credit = decCredit;
                if (isAutomatic)
                {
                    infoLedgerPosting.InvoiceNo = strInvoiceNo;
                }
                else
                {
                    infoLedgerPosting.InvoiceNo = voucherNo;
                }
                infoLedgerPosting.YearId = Convert.ToDecimal(PublicVariables._decCurrentFinancialYearId.ToString());
                if (inI > -1)
                {
                    if (chequeNo == null)
                    {
                        infoLedgerPosting.ChequeNo = chequeNo.ToString();
                    }
                    else
                    {
                        infoLedgerPosting.ChequeNo = string.Empty;
                    }
                    if (chequeDate != null && chequeDate.ToString() != string.Empty)
                    {
                        infoLedgerPosting.ChequeDate = Convert.ToDateTime(chequeDate);
                    }
                    else
                    {
                        infoLedgerPosting.ChequeDate = DateTime.Now;
                    }
                }
                else
                {
                    infoLedgerPosting.ChequeNo = string.Empty;
                    infoLedgerPosting.ChequeDate = DateTime.Now;
                }
                infoLedgerPosting.Extra1 = string.Empty;
                infoLedgerPosting.Extra2 = string.Empty;
                spLedgerPosting.LedgerPostingAdd(infoLedgerPosting);
            }
            catch (Exception ex)
            {
                PublicVariables.infoError.ErrorString = "CV19:" + ex.Message;
            }
        }
        public void LedgerPostingEdit(decimal decLedgerPostingId, decimal decLedgerId, decimal decCredit, decimal decDebit, decimal decDetailsId, int inI, string dgvtxtChequeNo, string dgvtxtChequeDate, string txtVoucherNo, string strVoucherNo, decimal DecContraVoucherTypeId)
        {
            try
            {
                LedgerPostingSP spLedgerPosting = new LedgerPostingSP();
                LedgerPostingInfo infoLedgerPosting = new LedgerPostingInfo();
                infoLedgerPosting.LedgerPostingId = decLedgerPostingId;
                infoLedgerPosting.Date = PublicVariables._dtCurrentDate;
                infoLedgerPosting.VoucherTypeId = DecContraVoucherTypeId;
                infoLedgerPosting.VoucherNo = strVoucherNo;
                infoLedgerPosting.DetailsId = decDetailsId;
                infoLedgerPosting.YearId = PublicVariables._decCurrentFinancialYearId;
                infoLedgerPosting.InvoiceNo = txtVoucherNo;
                infoLedgerPosting.Extra1 = string.Empty;
                infoLedgerPosting.Extra2 = string.Empty;
                infoLedgerPosting.LedgerId = decLedgerId;
                infoLedgerPosting.Credit = decCredit;
                infoLedgerPosting.Debit = decDebit;
                if (inI > -1)
                {
                    if (dgvtxtChequeNo != null && dgvtxtChequeNo.ToString() != string.Empty)
                    {
                        infoLedgerPosting.ChequeNo = dgvtxtChequeNo.ToString();
                    }
                    else
                    {
                        infoLedgerPosting.ChequeNo = string.Empty;
                    }
                    if (dgvtxtChequeDate != null && dgvtxtChequeDate.ToString() != string.Empty)
                    {
                        infoLedgerPosting.ChequeDate = Convert.ToDateTime(dgvtxtChequeDate);
                    }
                    else
                    {
                        infoLedgerPosting.ChequeDate = DateTime.Now;
                    }
                }
                else
                {
                    infoLedgerPosting.ChequeNo = string.Empty;
                    infoLedgerPosting.ChequeDate = DateTime.Now;
                }
                spLedgerPosting.LedgerPostingEdit(infoLedgerPosting);
            }
            catch (Exception ex)
            {
                PublicVariables.infoError.ErrorString = "CV20:" + ex.Message;
            }
        }
        
        #endregion

        #region Payment Voucher
      //  [AuthorizationActionFilter(EnumManager.Actions.Any, EnumManager.Modules.PaymentVoucher)]
        public IActionResult indexPayment()
        {
            TransactionsGeneralFill Obj = new TransactionsGeneralFill();
            var Ledgers = Obj.BankOrCashComboFill(false).AsEnumerable().Select(s => new
            {
                LedgerId = (decimal)s["LedgerId"],
                LedgerName = s["LedgerName"].ToString()
            });
            ViewBag.LedgerLists = new SelectList(Ledgers, "LedgerId", "LedgerName");
           return View();
        }
     //   [AuthorizationActionFilter(EnumManager.Actions.Save, EnumManager.Modules.PaymentVoucher)]
        public IActionResult PaymentVoucher()
        {
            DailySalaryVoucherMasterSP spMaster = new DailySalaryVoucherMasterSP();
            var Ledgers = spMaster.DailySalaryVoucherCashOrBankLedgersComboFill().AsEnumerable().Select(s => new
            {
                LedgerId = (decimal)s["LedgerId"],
                LedgerName = s["LedgerName"].ToString()
            });
            ViewBag.LedgerLists = new SelectList(Ledgers, "LedgerId", "LedgerName");
            TransactionsGeneralFill TransactionGeneralFillObj = new TransactionsGeneralFill();
            //TransactionGeneralFillObj.CashOrBankComboFill(cmbCashOrBank, true);
            return View();
        }
     //   [AuthorizationActionFilter(EnumManager.Actions.Save, EnumManager.Modules.PaymentVoucher)]
        [HttpPost]
        public string PaymentVoucher(PaymentMasterSP masterSP)
        {
            string message;

            decimal decDailySuffixPrefixId = 0;
            decimal decPaymentVoucherTypeId = 4;
            var strVoucherNo = string.Empty;
            TransactionsGeneralFill obj = new TransactionsGeneralFill();
            PaymentMasterSP SpPaymentMaster = new PaymentMasterSP();
            if (strVoucherNo == string.Empty)
            {
                strVoucherNo = "0";
            }
            if (SpPaymentMaster.PaymentVoucherCheckExistence(Request.Form["VoucherNo"].ToString().Trim(), decPaymentVoucherTypeId, 0) == false)
            {
                strVoucherNo = obj.VoucherNumberAutomaicGeneration(decPaymentVoucherTypeId, Convert.ToDecimal(strVoucherNo), DateTime.Parse(Request.Form["date"]), "PaymentMaster");

                if (Convert.ToDecimal(strVoucherNo) != SpPaymentMaster.PaymentMasterMax(decPaymentVoucherTypeId) + 1)
                {
                    strVoucherNo = SpPaymentMaster.PaymentMasterMax(decPaymentVoucherTypeId).ToString();
                    strVoucherNo = obj.VoucherNumberAutomaicGeneration(decPaymentVoucherTypeId, Convert.ToDecimal(strVoucherNo), DateTime.Parse(Request.Form["date"]), "PaymentMaster");
                    if (SpPaymentMaster.PaymentMasterMax(decPaymentVoucherTypeId) == 0)
                    {
                        strVoucherNo = "0";
                        strVoucherNo = obj.VoucherNumberAutomaicGeneration(decPaymentVoucherTypeId, Convert.ToDecimal(strVoucherNo), DateTime.Parse(Request.Form["date"]), "PaymentMaster");
                    }
                }

                try
                {
                    //int inB = 0;
                    PaymentMasterInfo InfoPaymentMaster = new PaymentMasterInfo();
                    PaymentDetailsInfo InfoPaymentDetails = new PaymentDetailsInfo();
                    PaymentDetailsSP SpPaymentDetails = new PaymentDetailsSP();
                    PartyBalanceSP SpPartyBalance = new PartyBalanceSP();
                    PartyBalanceInfo InfopartyBalance = new PartyBalanceInfo();
                    DateValidation objVal = new DateValidation();
                    //TextBox txtDate1 = new TextBox();
                    InfoPaymentMaster.Date = DateTime.Parse(Request.Form["date"]);
                    InfoPaymentMaster.Extra1 = string.Empty;
                    InfoPaymentMaster.Extra2 = string.Empty;
                    InfoPaymentMaster.FinancialYearId = Convert.ToDecimal(PublicVariables._decCurrentFinancialYearId.ToString());
                    InfoPaymentMaster.InvoiceNo = Request.Form["voucherNo"].ToString().Trim();
                    InfoPaymentMaster.LedgerId = Convert.ToDecimal(Request.Form["legers"].ToString());
                    var narration ="";
                    if (Request.Form["Narration"].ToString().Trim()!=null)
                    {
                        narration = Request.Form["Narration"].ToString().Trim();
                    }
                    InfoPaymentMaster.Narration =narration;
                    InfoPaymentMaster.SuffixPrefixId = decDailySuffixPrefixId;
                    decimal decSelectedCurrencyRate = 1; //decSelectedCurrencyRate = SpExchangRate.GetExchangeRateByExchangeRateId(Convert.ToDecimal(dgvPaymentVoucher.Rows[inI].Cells["dgvcmbCurrency"].Value.ToString()));//Exchange rate of grid's row
                   
                    decimal decTotalAmount = decimal.Parse(Request.Form["totalAmount"].ToString()) * decSelectedCurrencyRate; //TotalAmountCalculation();
                    InfoPaymentMaster.TotalAmount = decTotalAmount;
                    InfoPaymentMaster.UserId = _user.UserID; //userid datatype not valid in store procedure
                    InfoPaymentMaster.VoucherNo = strVoucherNo;
                    InfoPaymentMaster.VoucherTypeId = decPaymentVoucherTypeId;
                    decimal decPaymentMasterId = SpPaymentMaster.PaymentMasterAdd(InfoPaymentMaster);

                    if (decPaymentMasterId != 0)
                    {
                        MasterLedgerPosting(Request.Form["totalAmount"].ToString(), Request.Form["date"].ToString(), Request.Form["voucherNo"].ToString(), Request.Form["legers"].ToString(), strVoucherNo, decPaymentVoucherTypeId);
                    }
                    var dgvtxtAmount = Request.Form["Amount"].ToString().Split(',');
                    var dgvcmbCurrency =PublicVariables._decCurrencyId;
                    var dgvcmbAccountLedger = Request.Form["LegerId"].ToString().Split(',');
                    var dgvtxtChequeNo = Request.Form["ChequeNumber"].ToString().Split(',');
                    var dgvtxtChequeDate = Request.Form["ChequeDate"].ToString().Split(',');
                    var LedgerId = Request.Form["LegerId"].ToString().Split(',');
                    SuffixPrefixSP spSuffisprefix = new SuffixPrefixSP();
                    SuffixPrefixInfo infoSuffixPrefix = new SuffixPrefixInfo();
                    infoSuffixPrefix = spSuffisprefix.GetSuffixPrefixDetails(decPaymentVoucherTypeId, DateTime.Parse(Request.Form["date"].ToString()));
                    var strPrefix = infoSuffixPrefix.Prefix;
                    var strSuffix = infoSuffixPrefix.Suffix;
                    var strInvoiceNo = strPrefix + strVoucherNo + strSuffix;
                    for (int inI = 0; inI < LedgerId.Count(); inI++)
                    {
                        if (dgvtxtAmount[inI] != null && dgvtxtAmount[inI].ToString() != string.Empty && dgvtxtAmount[inI] != "" && int.Parse(dgvtxtAmount[inI]) > 0)
                        {
                            InfoPaymentDetails.Amount = Convert.ToDecimal(dgvtxtAmount[inI].ToString());
                        }
                        else
                        {
                            InfoPaymentDetails.Amount =0;
                        }
                        InfoPaymentDetails.ExchangeRateId = Convert.ToDecimal(dgvcmbCurrency);
                        InfoPaymentDetails.Extra1 = string.Empty;
                        InfoPaymentDetails.Extra2 = string.Empty;
                        InfoPaymentDetails.LedgerId = Convert.ToDecimal(dgvcmbAccountLedger[inI].ToString());
                        InfoPaymentDetails.PaymentMasterId = decPaymentMasterId;
                        if (dgvcmbAccountLedger[inI] != null && dgvcmbAccountLedger[inI].ToString() != string.Empty)
                        {
                            InfoPaymentDetails.LedgerId = Convert.ToDecimal(dgvcmbAccountLedger[inI].ToString());
                        }
                        if (dgvtxtChequeNo[inI] != null && dgvtxtChequeNo[inI].ToString() != string.Empty && dgvtxtChequeNo[inI] !="")
                        {
                            InfoPaymentDetails.ChequeNo = dgvtxtChequeNo[inI].ToString();

                            if (dgvtxtChequeDate[inI] != null && dgvtxtChequeDate[inI] != string.Empty)
                            {
                                InfoPaymentDetails.ChequeDate = Convert.ToDateTime(dgvtxtChequeDate[inI]);
                            }
                            else
                            {
                                InfoPaymentDetails.ChequeDate = DateTime.Now;
                            }
                        }
                        else
                        {
                            InfoPaymentDetails.ChequeNo = string.Empty;
                            InfoPaymentDetails.ChequeDate = DateTime.Now;
                        }
                        decimal decPaymentDetailsId = SpPaymentDetails.PaymentDetailsAdd(InfoPaymentDetails);
                        if (decPaymentDetailsId != 0)
                        {
                            //    for (int inJ = 0; inJ < dgvtxtAmount.Count(); inJ++)
                            //    {
                            //        if (dgvcmbAccountLedger[inJ].ToString() == LedgerId[inJ].ToString())
                            //        {
                            //            PartyBalanceAddOrEdit(inJ, Request.Form["date"], decPaymentVoucherTypeId, strInvoiceNo, Request.Form["voucherNo"], strVoucherNo);
                            //        }
                            //    }
                            //    inB++;
                            DetailsLedgerPostings(inI, decPaymentDetailsId, dgvtxtAmount[inI], Request.Form["legers"].ToString(), dgvtxtChequeNo[inI], dgvtxtChequeDate[inI], Request.Form["date"].ToString(), dgvcmbAccountLedger[inI], Request.Form["VoucherNo"].ToString(), strVoucherNo, decPaymentVoucherTypeId);
                        }

                    }
                        message = "success";
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                }
            }
            else
            {
                message = "Voucher number already exist";
            }
            return message;
        }
       // [AuthorizationActionFilter(EnumManager.Actions.Edit, EnumManager.Modules.PaymentVoucher)]
        public IActionResult PaymentVoucherEdit(decimal? masterId)
        {
            PaymentMasterInfo contraMasterInfo = new PaymentMasterInfo();
            PaymentMasterSP masterSP = new PaymentMasterSP();
            PaymentDetailsSP detailsSP = new PaymentDetailsSP();
            PaymentDetailsInfo detailsInfo = new PaymentDetailsInfo();
            LedgerPostingSP ledgerPostingSP = new LedgerPostingSP();
            LedgerPostingInfo ledgerPostingInfo = new LedgerPostingInfo();
            contraMasterInfo = masterSP.PaymentMasterView(Convert.ToDecimal(masterId));
            var dtbl = detailsSP.PaymentDetailsViewByMasterId(Convert.ToDecimal(masterId));
            string voucherNo = contraMasterInfo.VoucherNo;
            decimal VoucherTypeId = contraMasterInfo.VoucherTypeId;
            DataTable dtbledgerId = new DataTable();
            dtbledgerId = ledgerPostingSP.GetLedgerPostingIds(voucherNo, VoucherTypeId);

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("DetailsId");
            dataTable.Columns.Add("Bank/Cash");
            dataTable.Columns.Add("Currency");
            dataTable.Columns.Add("Amount");
            dataTable.Columns.Add("ChequeNo");
            dataTable.Columns.Add("ChequeDate");
            dataTable.Columns.Add("Masterid");
            dataTable.Columns.Add("VoucherNo");
            dataTable.Columns.Add("Date");
            dataTable.Columns.Add("LedgerId");
            dataTable.Columns.Add("Narration");
            dataTable.Columns.Add("LedgerPostingId");

            if (dtbl.Rows.Count == 0)
            {
                DataRow dr = dataTable.NewRow();
                dataTable.Rows.Add(dr);
                dr["Masterid"] = contraMasterInfo.PaymentMasterId;
                dr["VoucherNo"] = contraMasterInfo.VoucherNo;
                dr["Date"] = contraMasterInfo.Date;
                dr["LedgerId"] = contraMasterInfo.LedgerId;
                dr["Narration"] = contraMasterInfo.Narration;
            }
            for (int i = 0; i < dtbl.Rows.Count; i++)
            {
                DataRow dr = dataTable.NewRow();
                dataTable.Rows.Add(dr);
                dr["Masterid"] = contraMasterInfo.PaymentMasterId;
                dr["VoucherNo"] = contraMasterInfo.VoucherNo;
                dr["Date"] = contraMasterInfo.Date;
                dr["LedgerId"] = contraMasterInfo.LedgerId;
                dr["Narration"] = contraMasterInfo.Narration;
                dr["DetailsId"] = decimal.Parse(dtbl.Rows[i]["PaymentDetailsId"].ToString());
                dr["Bank/Cash"] = dtbl.Rows[i]["LedgerId"].ToString();
                dr["Amount"] = dtbl.Rows[i]["Amount"].ToString();
                dr["ChequeDate"] = dtbl.Rows[i]["ChequeDate"].ToString();
                dr["ChequeNo"] = dtbl.Rows[i]["ChequeNo"].ToString();
                dr["Currency"] = dtbl.Rows[i]["ExchangeRateId"].ToString();
                if (dtbledgerId.Rows[i]["LedgerPostingId"] != null)
                {
                    dr["LedgerPostingId"] = dtbledgerId.Rows[i]["LedgerPostingId"].ToString();
                }
            }

            DailySalaryVoucherMasterSP spMaster = new DailySalaryVoucherMasterSP();
            var Ledgers = spMaster.DailySalaryVoucherCashOrBankLedgersComboFill().AsEnumerable().Select(s => new
            {
                LedgerId = (decimal)s["LedgerId"],
                LedgerName = s["LedgerName"].ToString()
            });
            ViewBag.LedgerLists = new SelectList(Ledgers, "LedgerId", "LedgerName");
            TransactionsGeneralFill TransactionGeneralFillObj = new TransactionsGeneralFill();
            var currencylist = TransactionGeneralFillObj.CurrencyComboByDate(DateTime.Now).AsEnumerable().Select(s => new
            {
                exchangeRateId = (decimal)s["exchangeRateId"],
                currencyName = s["currencyName"].ToString()
            });
            ViewBag.currncylist = new SelectList(currencylist, "exchangeRateId", "currencyName");
            return View(dataTable);
        }
       // [AuthorizationActionFilter(EnumManager.Actions.Edit, EnumManager.Modules.PaymentVoucher)]
        [HttpPost]
        public string PaymentVoucherEdit()
        {
            string message;
            try
            {
                decimal decDailySuffixPrefixId = 0;
                int inTableRowCount = dtblPartyBalance.Rows.Count;
                int inB = 0;
                PaymentMasterInfo InfoPaymentMaster = new PaymentMasterInfo();
                PaymentMasterSP SpPaymentMaster = new PaymentMasterSP();
                PaymentDetailsInfo InfoPaymentDetails = new PaymentDetailsInfo();
                PaymentDetailsSP SpPaymentDetails = new PaymentDetailsSP();
                PartyBalanceSP SpPartyBalance = new PartyBalanceSP();
                PartyBalanceInfo InfopartyBalance = new PartyBalanceInfo();
                DateValidation objVal = new DateValidation();
                decimal decPaymentVoucherTypeId = 4;
                var date = Request.Form["date"];
                var txtVoucherNo = Request.Form["VoucherNo"].ToString();
                var txtNarration = Request.Form["Narration"].ToString();
                var cmbBankorCash = Request.Form["Legers"].ToString();
                InfoPaymentMaster.Date = DateTime.Parse(date);
                InfoPaymentMaster.Extra1 = string.Empty;
                InfoPaymentMaster.Extra2 = string.Empty;
                InfoPaymentMaster.FinancialYearId = Convert.ToDecimal(PublicVariables._decCurrentFinancialYearId.ToString());
                InfoPaymentMaster.InvoiceNo = txtVoucherNo.Trim(); ;
                InfoPaymentMaster.LedgerId = Convert.ToDecimal(cmbBankorCash.ToString());
                InfoPaymentMaster.Narration = txtNarration.Trim();
                InfoPaymentMaster.SuffixPrefixId = decDailySuffixPrefixId;
                InfoPaymentMaster.PaymentMasterId = decimal.Parse(Request.Form["MasterId"].ToString());
                int decSelectedCurrencyRate = 1; //decSelectedCurrencyRate = SpExchangRate.GetExchangeRateByExchangeRateId(Convert.ToDecimal(dgvPaymentVoucher.Rows[inI].Cells["dgvcmbCurrency"].Value.ToString()));//Exchange rate of grid's row

                decimal decTotalAmount = int.Parse(Request.Form["totalAmount"].ToString()) * decSelectedCurrencyRate; //TotalAmountCalculation();
                InfoPaymentMaster.TotalAmount = decTotalAmount;
                InfoPaymentMaster.UserId = "1";//Helpers.CurrentUser.UserID.ToString();// PublicVariables._decCurrentUserId;
                var strVoucherNo = string.Empty;
                TransactionsGeneralFill obj = new TransactionsGeneralFill();
                if (strVoucherNo == string.Empty)
                {
                    strVoucherNo = "0";
                }
                strVoucherNo = obj.VoucherNumberAutomaicGeneration(decPaymentVoucherTypeId, Convert.ToDecimal(strVoucherNo), DateTime.Parse(Request.Form["date"].ToString()), "PaymentMaster");
                if (Convert.ToDecimal(strVoucherNo) != SpPaymentMaster.PaymentMasterMax(decPaymentVoucherTypeId) + 1)
                {
                    strVoucherNo = SpPaymentMaster.PaymentMasterMax(decPaymentVoucherTypeId).ToString();
                    strVoucherNo = obj.VoucherNumberAutomaicGeneration(decPaymentVoucherTypeId, Convert.ToDecimal(strVoucherNo), DateTime.Parse(Request.Form["date"].ToString()), "PaymentMaster");
                    if (SpPaymentMaster.PaymentMasterMax(decPaymentVoucherTypeId) == 0)
                    {
                        strVoucherNo = "0";
                        strVoucherNo = obj.VoucherNumberAutomaicGeneration(decPaymentVoucherTypeId, Convert.ToDecimal(strVoucherNo), DateTime.Parse(Request.Form["date"].ToString()), "PaymentMaster");
                    }
                }
                SuffixPrefixSP spSuffisprefix = new SuffixPrefixSP();
                SuffixPrefixInfo infoSuffixPrefix = new SuffixPrefixInfo();
                infoSuffixPrefix = spSuffisprefix.GetSuffixPrefixDetails(decPaymentVoucherTypeId, DateTime.Parse(Request.Form["date"]));
                var strPrefix = infoSuffixPrefix.Prefix;
                var strSuffix = infoSuffixPrefix.Suffix;
                var strInvoiceNo = strPrefix + strVoucherNo + strSuffix;
                InfoPaymentMaster.VoucherNo = strVoucherNo;
                InfoPaymentMaster.VoucherTypeId = decPaymentVoucherTypeId;
                decimal decPaymentMasterId = SpPaymentMaster.PaymentMasterEdit(InfoPaymentMaster);
                if (decPaymentMasterId != 0)
                {
                    MasterLedgerPostingEdits(Request.Form["totalAmount"], Request.Form["date"], strInvoiceNo, Request.Form["voucherNo"], Request.Form["legers"], strVoucherNo, decPaymentVoucherTypeId);
                }
                var dgvtxtAmount = Request.Form["Amount"].ToString().Split(',');
                //var dgvcmbCurrency = Request.Form["Currency"].Split(',');
                var dgvcmbAccountLedger = Request.Form["LegerId"].ToString().Split(',');
                var dgvtxtChequeNo = Request.Form["ChequeNumber"].ToString().Split(',');
                var dgvtxtChequeDate = Request.Form["ChequeDate"].ToString().Split(',');
                var LedgerId = Request.Form["LegerId"].Split(',');
                var dgvtxtpaymentDetailsId = Request.Form["DetailsId"].ToString().Split(',');
                var dgvtxtLedgerPostingId = Request.Form["LedgerPostingId"].ToString().Split(',');
                for (int inI = 0; inI < dgvtxtChequeNo.Count(); inI++)
                {
                    if (dgvtxtAmount[inI] != null && dgvtxtAmount[inI].ToString() != string.Empty && dgvtxtAmount[inI] != "" && int.Parse(dgvtxtAmount[inI]) > 0)
                    {
                        InfoPaymentDetails.Amount = Convert.ToDecimal(dgvtxtAmount[inI].ToString());
                    }
                    else
                    {
                        InfoPaymentDetails.Amount = 0;
                    }
                   // InfoPaymentDetails.Amount = Convert.ToDecimal(dgvtxtAmount[inI].ToString());
                    //InfoPaymentDetails.ExchangeRateId = Convert.ToDecimal(dgvcmbCurrency[inI].ToString());
                    InfoPaymentDetails.Extra1 = string.Empty;
                    InfoPaymentDetails.Extra2 = string.Empty;
                    if (dgvcmbAccountLedger[inI] != null && dgvcmbAccountLedger[inI].ToString() != string.Empty)
                    {
                        InfoPaymentDetails.LedgerId = Convert.ToDecimal(dgvcmbAccountLedger[inI].ToString());
                    }
                    if (dgvtxtChequeNo[inI] != null && dgvtxtChequeNo[inI].ToString() != string.Empty)
                    {
                        InfoPaymentDetails.ChequeNo = dgvtxtChequeNo[inI].ToString();
                        if (dgvtxtChequeDate[inI] != null && dgvtxtChequeDate[inI].ToString() != string.Empty)
                        {
                            InfoPaymentDetails.ChequeDate = Convert.ToDateTime(dgvtxtChequeDate[inI]);
                        }
                        else
                        {
                            InfoPaymentDetails.ChequeDate = DateTime.Now;
                        }
                    }
                    else
                    {
                        InfoPaymentDetails.ChequeNo = string.Empty;
                        InfoPaymentDetails.ChequeDate = DateTime.Now;
                    }
                    InfoPaymentDetails.PaymentMasterId = decPaymentMasterId;
                    var j = inI + 1;
                    var detailid = dgvtxtpaymentDetailsId.Count();

                    if (j > detailid)
                    {
                        decimal decPaymentDetailsId = SpPaymentDetails.PaymentDetailsAdd(InfoPaymentDetails);//to add new rows in payment details
                        if (decPaymentDetailsId != 0)
                        {
                            for (int inJ = 0; inJ < inTableRowCount; inJ++)
                            {
                                if (dgvcmbAccountLedger[inI].ToString() == dtblPartyBalance.Rows[inJ]["LedgerId"].ToString())
                                {
                                    PartyBalanceAddOrEdit(inJ, Request.Form["date"].ToString(), decPaymentVoucherTypeId, strInvoiceNo, Request.Form["voucherNo"].ToString(), strVoucherNo);
                                }
                            }
                            inB++;
                            DetailsLedgerPostings(inI, decPaymentDetailsId, dgvtxtAmount[inI], "1", dgvtxtChequeNo[inI], dgvtxtChequeDate[inI], Request.Form["date"].ToString(), dgvcmbAccountLedger[inI], Request.Form["VoucherNo"].ToString(), strVoucherNo, decPaymentVoucherTypeId);
                        }

                    }
                    else
                    {

                        InfoPaymentDetails.PaymentDetailsId = Convert.ToDecimal(dgvtxtpaymentDetailsId[inI].ToString());
                        decimal decPaymentDetailsId = SpPaymentDetails.PaymentDetailsEdit(InfoPaymentDetails);//to edit rows
                        if (decPaymentDetailsId != 0)
                        {
                            for (int inJ = 0; inJ < inTableRowCount; inJ++)
                            {
                                if (dgvcmbAccountLedger[inI].ToString() == dtblPartyBalance.Rows[inJ]["LedgerId"].ToString())
                                {
                                    PartyBalanceAddOrEdit(inJ, Request.Form["date"].ToString(), decPaymentVoucherTypeId, strInvoiceNo, Request.Form["voucherNo"], strVoucherNo);
                                }
                            }
                            inB++;
                            var decPaymentDetailsId1 = InfoPaymentDetails.PaymentDetailsId;
                            decimal decLedgerPostId = Convert.ToDecimal(dgvtxtLedgerPostingId[inI].ToString());
                            DetailsLedgerPostingEdit(inI, decLedgerPostId, decPaymentDetailsId, strInvoiceNo, dgvtxtAmount[inI], "1", dgvtxtChequeNo[inI], dgvtxtChequeDate[inI], Request.Form["date"], dgvcmbAccountLedger[inI], Request.Form["VoucherNo"], strVoucherNo, decPaymentVoucherTypeId);
                        }


                    }

                }

                message = "success";
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return message;
        }
       // [AuthorizationActionFilter(EnumManager.Actions.Delete, EnumManager.Modules.PaymentVoucher)]
        public IActionResult PaymentVoucherDelete(decimal? masterId)
        {
            PaymentMasterInfo contraMasterInfo = new PaymentMasterInfo();
            PaymentMasterSP masterSP = new PaymentMasterSP();
            PaymentDetailsSP detailsSP = new PaymentDetailsSP();
            PaymentDetailsInfo detailsInfo = new PaymentDetailsInfo();
            contraMasterInfo = masterSP.PaymentMasterView(Convert.ToDecimal(masterId));
            var dtbl = detailsSP.PaymentDetailsViewByMasterId(Convert.ToDecimal(masterId));
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("DetailsId");
            dataTable.Columns.Add("Bank/Cash");
            dataTable.Columns.Add("Currency");
            dataTable.Columns.Add("Amount");
            dataTable.Columns.Add("ChequeNo");
            dataTable.Columns.Add("ChequeDate");
            dataTable.Columns.Add("Masterid");
            dataTable.Columns.Add("VoucherNo");
            dataTable.Columns.Add("Date");
            dataTable.Columns.Add("LedgerId");
            dataTable.Columns.Add("Narration");

            for (int i = 0; i < dtbl.Rows.Count; i++)
            {
                DataRow dr = dataTable.NewRow();
                dataTable.Rows.Add(dr);
                dr["Masterid"] = contraMasterInfo.PaymentMasterId;
                dr["VoucherNo"] = contraMasterInfo.VoucherNo;
                dr["Date"] = contraMasterInfo.Date;
                dr["LedgerId"] = contraMasterInfo.LedgerId;
                dr["Narration"] = contraMasterInfo.Narration;
                dr["DetailsId"] = decimal.Parse(dtbl.Rows[i]["PaymentDetailsId"].ToString());
                dr["Bank/Cash"] = dtbl.Rows[i]["LedgerId"].ToString();
                dr["Amount"] = dtbl.Rows[i]["Amount"].ToString();
                dr["ChequeDate"] = dtbl.Rows[i]["ChequeDate"].ToString();
                dr["ChequeNo"] = dtbl.Rows[i]["ChequeNo"].ToString();
                dr["Currency"] = dtbl.Rows[i]["ExchangeRateId"].ToString();
            }

            DailySalaryVoucherMasterSP spMaster = new DailySalaryVoucherMasterSP();
            var Ledgers = spMaster.DailySalaryVoucherCashOrBankLedgersComboFill().AsEnumerable().Select(s => new
            {
                LedgerId = (decimal)s["LedgerId"],
                LedgerName = s["LedgerName"].ToString()
            });
            ViewBag.LedgerLists = new SelectList(Ledgers, "LedgerId", "LedgerName");
            TransactionsGeneralFill TransactionGeneralFillObj = new TransactionsGeneralFill();
            var currencylist = TransactionGeneralFillObj.CurrencyComboByDate(DateTime.Now).AsEnumerable().Select(s => new
            {
                exchangeRateId = (decimal)s["exchangeRateId"],
                currencyName = s["currencyName"].ToString()
            });
            ViewBag.currncylist = new SelectList(currencylist, "exchangeRateId", "currencyName");
            return View(dataTable);
        }
       // [AuthorizationActionFilter(EnumManager.Actions.Delete, EnumManager.Modules.PaymentVoucher)]
        [HttpPost]
        public string PaymentVoucherDelete()
        {
            string message;
            try
            {
                var decPaymentVoucherTypeId = 0;
                var decPaymentmasterId = Request.Form["MasterId"].ToString();
                PaymentMasterSP SpPaymentMaster = new PaymentMasterSP();
                PartyBalanceSP SpPartyBalance = new PartyBalanceSP();
                TransactionsGeneralFill obj = new TransactionsGeneralFill();
                var strVoucherNo = string.Empty;
                if (strVoucherNo == string.Empty)
                {
                    strVoucherNo = "0";
                }
                strVoucherNo = obj.VoucherNumberAutomaicGeneration(decPaymentVoucherTypeId, Convert.ToDecimal(strVoucherNo), DateTime.Parse(Request.Form["date"]), "PaymentMaster");
                if (Convert.ToDecimal(strVoucherNo) != SpPaymentMaster.PaymentMasterMax(decPaymentVoucherTypeId) + 1)
                {
                    strVoucherNo = SpPaymentMaster.PaymentMasterMax(decPaymentVoucherTypeId).ToString();
                    strVoucherNo = obj.VoucherNumberAutomaicGeneration(decPaymentVoucherTypeId, Convert.ToDecimal(strVoucherNo), DateTime.Parse(Request.Form["date"]), "PaymentMaster");
                    if (SpPaymentMaster.PaymentMasterMax(decPaymentVoucherTypeId) == 0)
                    {
                        strVoucherNo = "0";
                        strVoucherNo = obj.VoucherNumberAutomaicGeneration(decPaymentVoucherTypeId, Convert.ToDecimal(strVoucherNo), DateTime.Parse(Request.Form["date"]), "PaymentMaster");
                    }
                }
                if (!SpPartyBalance.PartyBalanceCheckReference(decPaymentVoucherTypeId, strVoucherNo))
                {
                    SpPaymentMaster.PaymentVoucherDelete(decimal.Parse(decPaymentmasterId), decPaymentVoucherTypeId, strVoucherNo);
                    message = "success";

                }
                else
                {
                    message = "Reference exist. Cannot delete";
                }

            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return message;
        }

        public void MasterLedgerPosting(string txtTotal, string dtpDate, string txtVoucherNo, string cmbBankorCash, string strVoucherNo, decimal decPaymentVoucherTypeId)
        {
            TransactionsGeneralFill obj = new TransactionsGeneralFill();
            bool isAutomatic = _user.Isautomatic();
            try
            {
                LedgerPostingInfo InfoLedgerPosting = new LedgerPostingInfo();
                LedgerPostingSP SpLedgerPosting = new LedgerPostingSP();
                ExchangeRateSP SpExchangRate = new ExchangeRateSP();
                InfoLedgerPosting.Credit = Convert.ToDecimal(txtTotal);
                InfoLedgerPosting.Date = DateTime.Parse(dtpDate);
                InfoLedgerPosting.Debit = 0;
                InfoLedgerPosting.DetailsId = 0;
                InfoLedgerPosting.Extra1 = string.Empty;
                InfoLedgerPosting.Extra2 = string.Empty;
                InfoLedgerPosting.InvoiceNo = txtVoucherNo;
                InfoLedgerPosting.ChequeNo = string.Empty;
                InfoLedgerPosting.ChequeDate = DateTime.Now;
                InfoLedgerPosting.LedgerId = Convert.ToDecimal(cmbBankorCash.ToString());
                if (!isAutomatic)
                {
                    InfoLedgerPosting.VoucherNo = txtVoucherNo.Trim();
                }
                else
                {
                    InfoLedgerPosting.VoucherNo = strVoucherNo;
                }
                InfoLedgerPosting.VoucherTypeId = decPaymentVoucherTypeId;
                InfoLedgerPosting.YearId = Convert.ToDecimal(PublicVariables._decCurrentFinancialYearId.ToString());
                SpLedgerPosting.LedgerPostingAdd(InfoLedgerPosting);
            }
            catch (Exception ex)
            {
                PublicVariables.infoError.ErrorString = "PV21:" + ex.Message;
            }
        }
        public void MasterLedgerPostingEdits(string txtTotal, string dtpDate, string txtVoucherNo, string cmbBankorCash, string strInvoiceNo, string strVoucherNo, decimal decPaymentVoucherTypeId)
        {
            var isAutomatic = true;
            try
            {
                LedgerPostingInfo InfoLedgerPosting = new LedgerPostingInfo();
                LedgerPostingSP SpLedgerPosting = new LedgerPostingSP();
                ExchangeRateSP SpExchangRate = new ExchangeRateSP();
                InfoLedgerPosting.Credit = Convert.ToDecimal(txtTotal);
                InfoLedgerPosting.Date = DateTime.Parse(dtpDate);
                InfoLedgerPosting.Debit = 0;
                InfoLedgerPosting.DetailsId = 0;
                InfoLedgerPosting.Extra1 = string.Empty;
                InfoLedgerPosting.Extra2 = string.Empty;
                InfoLedgerPosting.InvoiceNo = strInvoiceNo;
                InfoLedgerPosting.ChequeNo = string.Empty;
                InfoLedgerPosting.ChequeDate = DateTime.Now;
                InfoLedgerPosting.LedgerId = Convert.ToDecimal(cmbBankorCash.ToString());
                if (!isAutomatic)
                {
                    InfoLedgerPosting.VoucherNo = txtVoucherNo.Trim();
                }
                else
                {
                    InfoLedgerPosting.VoucherNo = strVoucherNo;
                }
                InfoLedgerPosting.VoucherTypeId = decPaymentVoucherTypeId;
                InfoLedgerPosting.YearId = PublicVariables._decCurrentFinancialYearId;
                SpLedgerPosting.LedgerPostingEditByVoucherTypeAndVoucherNo(InfoLedgerPosting);
            }
            catch (Exception ex)
            {
                PublicVariables.infoError.ErrorString = "PV22:" + ex.Message;
            }
        }
        public void DetailsLedgerPostings(int inA, decimal decPaymentDetailsId, string dgvtxtAmount, string dgvcmbCurrency, string dgvtxtChequeNo, string dgvtxtChequeDate, string dtpDate, string dgvcmbAccountLedger, string txtVoucherNo, string strVoucherNo, decimal decPaymentVoucherTypeId)
        {
            var isAutomatic = true;
            SuffixPrefixSP spSuffisprefix = new SuffixPrefixSP();
            SuffixPrefixInfo infoSuffixPrefix = new SuffixPrefixInfo();
            infoSuffixPrefix = spSuffisprefix.GetSuffixPrefixDetails(decPaymentVoucherTypeId, DateTime.Parse(dtpDate));
            var strPrefix = infoSuffixPrefix.Prefix;
            var strSuffix = infoSuffixPrefix.Suffix;
            var strInvoiceNo = strPrefix + strVoucherNo + strSuffix;
            LedgerPostingInfo InfoLedgerPosting = new LedgerPostingInfo();
            LedgerPostingSP SpLedgerPosting = new LedgerPostingSP();
            ExchangeRateSP SpExchangRate = new ExchangeRateSP();
            decimal decOldExchange = 0;
            decimal decNewExchangeRate = 0;
            decimal decNewExchangeRateId = 0;
            decimal decOldExchangeId = 0;
            try
            {


                if (dgvtxtAmount != null)
                {
                    decimal d = Convert.ToDecimal(dgvcmbCurrency.ToString());
                    var decSelectedCurrencyRate = SpExchangRate.GetExchangeRateByExchangeRateId(Convert.ToDecimal(dgvcmbCurrency.ToString()));
                    var decAmount = Convert.ToDecimal(dgvtxtAmount.ToString());
                    var decConvertRate = decAmount * decSelectedCurrencyRate;
                    InfoLedgerPosting.Credit = 0;
                    InfoLedgerPosting.Date = DateTime.Parse(dtpDate);
                    InfoLedgerPosting.Debit = decConvertRate;
                    InfoLedgerPosting.DetailsId = decPaymentDetailsId;
                    InfoLedgerPosting.Extra1 = string.Empty;
                    InfoLedgerPosting.Extra2 = string.Empty;
                    InfoLedgerPosting.InvoiceNo = strInvoiceNo;

                    if (dgvtxtChequeNo != null && dgvtxtChequeNo.ToString() != string.Empty)
                    {
                        InfoLedgerPosting.ChequeNo = dgvtxtChequeNo.ToString();
                        if (dgvtxtChequeDate != null && dgvtxtChequeDate.ToString() != string.Empty)
                        {
                            InfoLedgerPosting.ChequeDate = Convert.ToDateTime(dgvtxtChequeDate.ToString());
                        }
                        else
                            InfoLedgerPosting.ChequeDate = DateTime.Now;

                    }
                    else
                    {
                        InfoLedgerPosting.ChequeNo = string.Empty;
                        InfoLedgerPosting.ChequeDate = DateTime.Now;
                    }
                    InfoLedgerPosting.LedgerId = Convert.ToDecimal(dgvcmbAccountLedger.ToString());
                    if (!isAutomatic)
                    {
                        InfoLedgerPosting.VoucherNo = txtVoucherNo.Trim();
                    }
                    else
                    {
                        InfoLedgerPosting.VoucherNo = strVoucherNo;
                    }
                    InfoLedgerPosting.VoucherTypeId = decPaymentVoucherTypeId;
                    InfoLedgerPosting.YearId = Convert.ToDecimal(PublicVariables._decCurrentFinancialYearId.ToString());
                    SpLedgerPosting.LedgerPostingAdd(InfoLedgerPosting);
                }
                else
                {
                    InfoLedgerPosting.Date = DateTime.Parse(dtpDate);

                    InfoLedgerPosting.Extra1 = string.Empty;
                    InfoLedgerPosting.Extra2 = string.Empty;
                    InfoLedgerPosting.InvoiceNo = strInvoiceNo;
                    InfoLedgerPosting.VoucherTypeId = decPaymentVoucherTypeId;
                    InfoLedgerPosting.YearId = Convert.ToDecimal(PublicVariables._decCurrentFinancialYearId.ToString());
                    InfoLedgerPosting.Credit = 0;
                    InfoLedgerPosting.LedgerId = Convert.ToDecimal(dgvcmbAccountLedger.ToString());
                    InfoLedgerPosting.VoucherNo = strVoucherNo;
                    InfoLedgerPosting.DetailsId = decPaymentDetailsId;
                    InfoLedgerPosting.InvoiceNo = txtVoucherNo.Trim();
                    if (dgvtxtChequeNo != null && dgvtxtChequeNo.ToString() != string.Empty)
                    {
                        InfoLedgerPosting.ChequeNo = dgvtxtChequeNo.ToString();
                        if (dgvtxtChequeDate != null && dgvtxtChequeDate.ToString() != string.Empty)
                        {
                            InfoLedgerPosting.ChequeDate = Convert.ToDateTime(dgvtxtChequeDate.ToString());
                        }
                        else
                            InfoLedgerPosting.ChequeDate = DateTime.Now;
                    }
                    else
                    {
                        InfoLedgerPosting.ChequeNo = string.Empty;
                        InfoLedgerPosting.ChequeDate = DateTime.Now;
                    }
                    decimal decConvertRate = 0;
                    foreach (DataRow dr in dtblPartyBalance.Rows)
                    {
                        if (InfoLedgerPosting.LedgerId == Convert.ToDecimal(dr["LedgerId"].ToString()))
                        {
                            decOldExchange = Convert.ToDecimal(dr["OldExchangeRate"].ToString());
                            decNewExchangeRateId = Convert.ToDecimal(dr["CurrencyId"].ToString());
                            var decSelectedCurrencyRate = SpExchangRate.GetExchangeRateByExchangeRateId(decOldExchange);
                            var decAmount = Convert.ToDecimal(dr["Amount"].ToString());
                            decConvertRate = decConvertRate + (decAmount * decSelectedCurrencyRate);

                        }
                    }
                    InfoLedgerPosting.Debit = decConvertRate;
                    SpLedgerPosting.LedgerPostingAdd(InfoLedgerPosting);

                    InfoLedgerPosting.LedgerId = 12;
                    //foreach (DataRow dr in dtblPartyBalance.Rows)
                    //{
                    //    if (Convert.ToDecimal(dgvcmbAccountLedger.ToString()) == Convert.ToDecimal(dr["LedgerId"].ToString()))
                    //    {
                    //        if (dr["ReferenceType"].ToString() == "Against")
                    //        {
                    //            decNewExchangeRateId = Convert.ToDecimal(dr["CurrencyId"].ToString());
                    //            decNewExchangeRate = SpExchangRate.GetExchangeRateByExchangeRateId(decNewExchangeRateId);
                    //            decOldExchangeId = Convert.ToDecimal(dr["OldExchangeRate"].ToString());
                    //            decOldExchange = SpExchangRate.GetExchangeRateByExchangeRateId(decOldExchangeId);
                    //            var decAmount = Convert.ToDecimal(dr["Amount"].ToString());
                    //            decimal decForexAmount = (decAmount * decNewExchangeRate) - (decAmount * decOldExchange);
                    //            if (decForexAmount >= 0)
                    //            {

                    //                InfoLedgerPosting.Debit = decForexAmount;
                    //                InfoLedgerPosting.Credit = 0;
                    //            }
                    //            else
                    //            {
                    //                InfoLedgerPosting.Credit = -1 * decForexAmount;
                    //                InfoLedgerPosting.Debit = 0;
                    //            }
                    //            SpLedgerPosting.LedgerPostingAdd(InfoLedgerPosting);
                    //        }
                    //    }

                    //}
                }
            }
            catch (Exception ex)
            {
                PublicVariables.infoError.ErrorString = "PV23:" + ex.Message;
            }
        }
        public void DetailsLedgerPostingEdit(int inA, decimal decLedgerPostingId, decimal decPaymentDetailsId, string strInvoiceNo, string dgvtxtAmount, string dgvcmbCurrency, string dgvtxtChequeNo, string dgvtxtChequeDate, string dtpDate, string dgvcmbAccountLedger, string txtVoucherNo, string strVoucherNo, decimal decPaymentVoucherTypeId)
        {
            var isAutomatic = true;
            LedgerPostingInfo InfoLedgerPosting = new LedgerPostingInfo();
            LedgerPostingSP SpLedgerPosting = new LedgerPostingSP();
            ExchangeRateSP SpExchangRate = new ExchangeRateSP();
            decimal decOldExchange = 0;
            decimal decNewExchangeRate = 0;
            decimal decNewExchangeRateId = 0;
            decimal decOldExchangeId = 0;
            try
            {
                if (decPaymentDetailsId != 0)
                {
                    var decSelectedCurrencyRate = SpExchangRate.GetExchangeRateByExchangeRateId(Convert.ToDecimal(dgvcmbCurrency.ToString()));
                    var decAmount = Convert.ToDecimal(dgvtxtAmount.ToString());
                    var decConvertRate = decAmount * decSelectedCurrencyRate;
                    InfoLedgerPosting.Credit = 0;
                    InfoLedgerPosting.Date = DateTime.Parse(dtpDate);
                    InfoLedgerPosting.Debit = decConvertRate;
                    InfoLedgerPosting.DetailsId = decPaymentDetailsId;
                    InfoLedgerPosting.Extra1 = string.Empty;
                    InfoLedgerPosting.Extra2 = string.Empty;
                    InfoLedgerPosting.InvoiceNo = strInvoiceNo;
                    InfoLedgerPosting.LedgerId = Convert.ToDecimal(dgvcmbAccountLedger.ToString());
                    if (!isAutomatic)
                    {
                        InfoLedgerPosting.VoucherNo = txtVoucherNo.Trim();
                    }
                    else
                    {
                        InfoLedgerPosting.VoucherNo = strVoucherNo;
                    }
                    if (dgvtxtChequeNo != null && dgvtxtChequeNo.ToString() != string.Empty)
                    {
                        InfoLedgerPosting.ChequeNo = dgvtxtChequeNo.ToString();
                        if (dgvtxtChequeDate != null && dgvtxtChequeDate.ToString() != string.Empty)
                        {
                            InfoLedgerPosting.ChequeDate = Convert.ToDateTime(dgvtxtChequeDate.ToString());
                        }
                        else
                            InfoLedgerPosting.ChequeDate = DateTime.Now;
                    }
                    else
                    {
                        InfoLedgerPosting.ChequeNo = string.Empty;
                        InfoLedgerPosting.ChequeDate = DateTime.Now;
                    }

                    InfoLedgerPosting.VoucherTypeId = decPaymentVoucherTypeId;
                    InfoLedgerPosting.YearId = Convert.ToDecimal(PublicVariables._decCurrentFinancialYearId.ToString());
                    InfoLedgerPosting.LedgerPostingId = decLedgerPostingId;
                    SpLedgerPosting.LedgerPostingEdit(InfoLedgerPosting);
                }
                else
                {
                    InfoLedgerPosting.Date = DateTime.Parse(dtpDate);

                    InfoLedgerPosting.Extra1 = string.Empty;
                    InfoLedgerPosting.Extra2 = string.Empty;
                    InfoLedgerPosting.InvoiceNo = strInvoiceNo;
                    InfoLedgerPosting.VoucherTypeId = decPaymentVoucherTypeId;
                    InfoLedgerPosting.YearId = Convert.ToDecimal(PublicVariables._decCurrentFinancialYearId.ToString());
                    InfoLedgerPosting.Credit = 0;
                    InfoLedgerPosting.LedgerId = Convert.ToDecimal(dgvcmbAccountLedger.ToString());
                    InfoLedgerPosting.VoucherNo = strVoucherNo;
                    InfoLedgerPosting.DetailsId = decPaymentDetailsId;
                    InfoLedgerPosting.InvoiceNo = txtVoucherNo.Trim();
                    if (dgvtxtChequeNo != null && dgvtxtChequeNo.ToString() != string.Empty)
                    {
                        InfoLedgerPosting.ChequeNo = dgvtxtChequeNo.ToString();
                        if (dgvtxtChequeDate != null && dgvtxtChequeDate.ToString() != string.Empty)
                        {
                            InfoLedgerPosting.ChequeDate = Convert.ToDateTime(dgvtxtChequeDate.ToString());
                        }
                        else
                            InfoLedgerPosting.ChequeDate = DateTime.Now;
                    }
                    else
                    {
                        InfoLedgerPosting.ChequeNo = string.Empty;
                        InfoLedgerPosting.ChequeDate = DateTime.Now;
                    }
                    decimal decConvertRate = 0;
                    foreach (DataRow dr in dtblPartyBalance.Rows)
                    {
                        if (InfoLedgerPosting.LedgerId == Convert.ToDecimal(dr["LedgerId"].ToString()))
                        {
                            decOldExchange = Convert.ToDecimal(dr["OldExchangeRate"].ToString());
                            decNewExchangeRateId = Convert.ToDecimal(dr["CurrencyId"].ToString());
                            var decSelectedCurrencyRate = SpExchangRate.GetExchangeRateByExchangeRateId(decOldExchange);
                            var decAmount = Convert.ToDecimal(dr["Amount"].ToString());
                            decConvertRate = decConvertRate + (decAmount * decSelectedCurrencyRate);

                        }
                    }
                    InfoLedgerPosting.Debit = decConvertRate;
                    InfoLedgerPosting.LedgerPostingId = decLedgerPostingId;
                    SpLedgerPosting.LedgerPostingEdit(InfoLedgerPosting);
                    InfoLedgerPosting.LedgerId = 12;
                    foreach (DataRow dr in dtblPartyBalance.Rows)
                    {
                        if (Convert.ToDecimal(dgvcmbAccountLedger.ToString()) == Convert.ToDecimal(dr["LedgerId"].ToString()))
                        {
                            if (dr["ReferenceType"].ToString() == "Against")
                            {
                                decNewExchangeRateId = Convert.ToDecimal(dr["CurrencyId"].ToString());
                                decNewExchangeRate = SpExchangRate.GetExchangeRateByExchangeRateId(decNewExchangeRateId);
                                decOldExchangeId = Convert.ToDecimal(dr["OldExchangeRate"].ToString());
                                decOldExchange = SpExchangRate.GetExchangeRateByExchangeRateId(decOldExchangeId);
                                var decAmount = Convert.ToDecimal(dr["Amount"].ToString());
                                decimal decForexAmount = (decAmount * decNewExchangeRate) - (decAmount * decOldExchange);
                                if (decForexAmount >= 0)
                                {

                                    InfoLedgerPosting.Debit = decForexAmount;
                                    InfoLedgerPosting.Credit = 0;
                                }
                                else
                                {
                                    InfoLedgerPosting.Credit = -1 * decForexAmount;
                                    InfoLedgerPosting.Debit = 0;
                                }
                                SpLedgerPosting.LedgerPostingAdd(InfoLedgerPosting);
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                PublicVariables.infoError.ErrorString = "PV24:" + ex.Message;
            }
        }

        #endregion

        #region Receipt Voucher
       // [AuthorizationActionFilter(EnumManager.Actions.Any, EnumManager.Modules.ReceiptVoucher)]
        public IActionResult IndexReceipt(string FromDate, string ToDate, string LedgerId, string VoucherNo)
        {
            TransactionsGeneralFill Obj = new TransactionsGeneralFill();
            var Ledgers = Obj.BankOrCashComboFill(false).AsEnumerable().Select(s => new
            {
                LedgerId = (decimal)s["LedgerId"],
                LedgerName = s["LedgerName"].ToString()
            });
            ViewBag.LedgerLists = new SelectList(Ledgers, "LedgerId", "LedgerName");
            ReceiptMasterSP Master = new ReceiptMasterSP();
            DataTable dtbl = new DataTable();
            if (VoucherNo == null)
            {
                VoucherNo = "";
            }
            if (FromDate == null || ToDate == null)
            {
                FromDate = DateTime.Now.ToString();
                ToDate = DateTime.Now.ToString();
            }
            if (LedgerId == null)
            {
                LedgerId = "0";
            }
            dtbl = Master.ReceiptMasterSearch(DateTime.Parse(FromDate), DateTime.Parse(ToDate), decimal.Parse(LedgerId), VoucherNo.Trim());
            return View(dtbl);
        }
       // [AuthorizationActionFilter(EnumManager.Actions.Save, EnumManager.Modules.ReceiptVoucher)]
        public IActionResult ReceiptVoucher()
        {
            DailySalaryVoucherMasterSP spMaster = new DailySalaryVoucherMasterSP();
            var Ledgers = spMaster.DailySalaryVoucherCashOrBankLedgersComboFill().AsEnumerable().Select(s => new
            {
                LedgerId = (decimal)s["LedgerId"],
                LedgerName = s["LedgerName"].ToString()
            });
            ViewBag.LedgerLists = new SelectList(Ledgers, "LedgerId", "LedgerName");
            TransactionsGeneralFill TransactionGeneralFillObj = new TransactionsGeneralFill();
            var currencylist = TransactionGeneralFillObj.CurrencyComboByDate(DateTime.Now).AsEnumerable().Select(s => new
            {
                exchangeRateId = (decimal)s["exchangeRateId"],
                currencyName = s["currencyName"].ToString()
            });
            ViewBag.currncylist = new SelectList(currencylist, "exchangeRateId", "currencyName");
            return View();
        }
      //  [AuthorizationActionFilter(EnumManager.Actions.Save, EnumManager.Modules.ReceiptVoucher)]
        [HttpPost]
        public string ReceiptVoucher(ReceiptDetailsInfo receipt)
        {
            string message;
            bool isAutomatic = _user.Isautomatic();

            try
            {

                int inB = 0;
                ReceiptMasterInfo InfoReceiptMaster = new ReceiptMasterInfo();
                ReceiptMasterSP SpReceiptMaster = new ReceiptMasterSP();
                ReceiptDetailsInfo InfoReceiptDetails = new ReceiptDetailsInfo();
                ReceiptDetailsSP SpReceiptDetails = new ReceiptDetailsSP();
                PartyBalanceSP SpPartyBalance = new PartyBalanceSP();
                PartyBalanceInfo InfopartyBalance = new PartyBalanceInfo();
                InfoReceiptMaster.Date = DateTime.Parse(Request.Form["date"].ToString()); //dtpDate.Value;
                InfoReceiptMaster.Extra1 = string.Empty;
                InfoReceiptMaster.Extra2 = string.Empty;
                InfoReceiptMaster.ExtraDate = DateTime.Now;
                InfoReceiptMaster.FinancialYearId = Convert.ToDecimal(PublicVariables._decCurrentFinancialYearId.ToString());
                InfoReceiptMaster.LedgerId = Convert.ToDecimal(Request.Form["legers"].ToString());
                var narration = "";
                if(Request.Form["Narration"].ToString() != null)
                {
                    narration = Request.Form["Narration"].ToString();
                }
                InfoReceiptMaster.Narration = narration;
                //decimal decTotalAmount = TotalAmountCalculation();
                int decSelectedCurrencyRate = 1; //decSelectedCurrencyRate = SpExchangRate.GetExchangeRateByExchangeRateId(Convert.ToDecimal(dgvPaymentVoucher.Rows[inI].Cells["dgvcmbCurrency"].Value.ToString()));//Exchange rate of grid's row
                decimal decTotalAmount = decimal.Parse(Request.Form["totalAmount"].ToString()) * decSelectedCurrencyRate; //TotalAmountCalculation();
                InfoReceiptMaster.TotalAmount = decTotalAmount;
                InfoReceiptMaster.UserId =_user.UserID; //PublicVariables._decCurrentUserId;
                TransactionsGeneralFill obj = new TransactionsGeneralFill();
                decimal decReceiptVoucherTypeId = 5;
                var strInvoiceNo = "";
                string tableName = "ReceiptMaster";
                var strVoucherNo = string.Empty;
                if (strVoucherNo == string.Empty)
                {
                    strVoucherNo = "0";
                }
                strVoucherNo = obj.VoucherNumberAutomaicGeneration(decReceiptVoucherTypeId, Convert.ToDecimal(strVoucherNo), DateTime.Parse(Request.Form["date"]), tableName);
                if (Convert.ToDecimal(strVoucherNo) != SpReceiptMaster.ReceiptMasterGetMax(decReceiptVoucherTypeId) + 1)
                {
                    strVoucherNo = SpReceiptMaster.ReceiptMasterGetMax(decReceiptVoucherTypeId).ToString();
                    strVoucherNo = obj.VoucherNumberAutomaicGeneration(decReceiptVoucherTypeId, Convert.ToDecimal(strVoucherNo), DateTime.Parse(Request.Form["date"]), tableName);
                    if (SpReceiptMaster.ReceiptMasterGetMax(decReceiptVoucherTypeId) == 0)
                    {
                        strVoucherNo = "0";
                        strVoucherNo = obj.VoucherNumberAutomaicGeneration(decReceiptVoucherTypeId, Convert.ToDecimal(strVoucherNo), DateTime.Parse(Request.Form["date"]), tableName);
                    }
                }

                if (!isAutomatic)
                {
                    InfoReceiptMaster.VoucherNo = Request.Form["VoucherNo"].ToString().Trim();
                    InfoReceiptMaster.InvoiceNo = Request.Form["VoucherNo"].ToString().Trim();
                    InfoReceiptMaster.SuffixPrefixId = 0;
                }
                else
                {
                    decimal decDailySuffixPrefixId = InfoReceiptMaster.SuffixPrefixId;

                    string strPrefix = string.Empty;
                    string strSuffix = string.Empty;
                    SuffixPrefixInfo infoSuffixPrefix = new SuffixPrefixInfo();
                    SuffixPrefixSP spSuffisprefix = new SuffixPrefixSP();
                    infoSuffixPrefix = spSuffisprefix.GetSuffixPrefixDetails(decReceiptVoucherTypeId, DateTime.Parse(Request.Form["date"].ToString()));
                    strPrefix = infoSuffixPrefix.Prefix;
                    strSuffix = infoSuffixPrefix.Suffix;
                    InfoReceiptMaster.VoucherNo = strVoucherNo;
                    strInvoiceNo = strPrefix + strVoucherNo + strSuffix;
                    InfoReceiptMaster.InvoiceNo = strInvoiceNo;
                    InfoReceiptMaster.SuffixPrefixId = decDailySuffixPrefixId;
                }
                InfoReceiptMaster.VoucherTypeId = decReceiptVoucherTypeId;
                var decReceiptMasterId = SpReceiptMaster.ReceiptMasterAdd(InfoReceiptMaster);

                if (decReceiptMasterId != 0)
                {
                    MasterLedgerPosting(Request.Form["totalAmount"].ToString(), Request.Form["date"].ToString(), Request.Form["voucherNo"].ToString(), Request.Form["legers"].ToString(), strVoucherNo, decReceiptVoucherTypeId);
                }
                var dgvtxtAmount = Request.Form["Amount"].ToString().Split(',');
                var dgvcmbAccountLedger = Request.Form["LegerId"].ToString().Split(',');
                var dgvtxtChequeNo = Request.Form["ChequeNumber"].ToString().Split(',');
                var dgvtxtChequeDate = Request.Form["ChequeDate"].ToString().Split(',');
                var dgvcmbCurrency =PublicVariables._decCurrencyId;
                for (int inI = 0; inI < dgvtxtAmount.Count(); inI++)
                {
                    InfoReceiptDetails.Amount = Convert.ToDecimal(dgvtxtAmount[inI].ToString());
                    InfoReceiptDetails.ExchangeRateId = Convert.ToDecimal(dgvcmbCurrency);
                    InfoReceiptDetails.Extra1 = string.Empty;
                    InfoReceiptDetails.Extra2 = string.Empty;
                    InfoReceiptDetails.LedgerId = Convert.ToDecimal(dgvcmbAccountLedger[inI].ToString());
                    InfoReceiptDetails.ReceiptMasterId = decReceiptMasterId;
                    if (dgvcmbAccountLedger[inI] != null && dgvcmbAccountLedger[inI] != string.Empty)
                    {
                        InfoReceiptDetails.LedgerId = Convert.ToDecimal(dgvcmbAccountLedger[inI].ToString());
                    }
                    if (dgvtxtChequeNo[inI] != null && dgvtxtChequeNo[inI].ToString() != string.Empty && dgvtxtChequeNo[inI] !="")
                    {
                        InfoReceiptDetails.ChequeNo = dgvtxtChequeNo[inI].ToString();
                        if (dgvtxtChequeDate[inI] != null && dgvtxtChequeDate[inI].ToString() != string.Empty)
                        {
                            InfoReceiptDetails.ChequeDate = Convert.ToDateTime(dgvtxtChequeDate[inI]);
                        }
                        else
                        {
                            InfoReceiptDetails.ChequeDate = DateTime.Now;
                        }
                    }
                    else
                    {
                        InfoReceiptDetails.ChequeNo = string.Empty;
                        InfoReceiptDetails.ChequeDate = DateTime.Now;
                    }
                    decimal decReceiptDetailsId = SpReceiptDetails.ReceiptDetailsAdd(InfoReceiptDetails);
                    if (decReceiptDetailsId != 0)
                    {
                        //for (int inJ = 0; inJ < dgvcmbAccountLedger.Count(); inJ++)
                        //{
                        //    //if (dgvcmbAccountLedger[inJ].ToString() == dtblPartyBalance.Rows[inJ]["LedgerId"].ToString())
                        //    //{
                        //        PartyBalanceAddOrEdit(inJ, Request.Form["date"], decReceiptVoucherTypeId, strInvoiceNo, Request.Form["VoucherNo"], strVoucherNo);
                        //   // }
                        //}
                        //inB++;
                        //DetailsLedgerPosting(inI, decReceiptDetailsId);
                        DetailsLedgerPostings(inI, decReceiptDetailsId, dgvtxtAmount[inI], "1", dgvtxtChequeNo[inI], dgvtxtChequeDate[inI], Request.Form["date"].ToString(), dgvcmbAccountLedger[inI], Request.Form["VoucherNo"].ToString(), strVoucherNo, decReceiptVoucherTypeId);

                    }

                }
                message = "success";
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return message;
        }
       // [AuthorizationActionFilter(EnumManager.Actions.Edit, EnumManager.Modules.ReceiptVoucher)]
        public IActionResult ReceiptVoucherEdit(decimal masterId)
        {

            ReceiptMasterInfo MasterInfo = new ReceiptMasterInfo();
            ReceiptMasterSP masterSP = new ReceiptMasterSP();
            ReceiptDetailsSP detailsSP = new ReceiptDetailsSP();
            ReceiptDetailsInfo detailsInfo = new ReceiptDetailsInfo();

            MasterInfo = masterSP.ReceiptMasterViewByMasterId(masterId);
            var dtbl = detailsSP.ReceiptDetailsViewByMasterId(masterId);
            LedgerPostingSP ledgerPostingSP = new LedgerPostingSP();
            LedgerPostingInfo ledgerPostingInfo = new LedgerPostingInfo();
            string voucherNo = MasterInfo.VoucherNo;
            decimal VoucherTypeId = MasterInfo.VoucherTypeId;
            DataTable dtbledgerId = new DataTable();
            dtbledgerId = ledgerPostingSP.GetLedgerPostingIds(voucherNo, VoucherTypeId);

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("DetailsId");
            dataTable.Columns.Add("Bank/Cash");
            dataTable.Columns.Add("Currency");
            dataTable.Columns.Add("Amount");
            dataTable.Columns.Add("ChequeNo");
            dataTable.Columns.Add("ChequeDate");
            dataTable.Columns.Add("Masterid");
            dataTable.Columns.Add("VoucherNo");
            dataTable.Columns.Add("Date");
            dataTable.Columns.Add("LedgerId");
            dataTable.Columns.Add("Narration");
            dataTable.Columns.Add("LedgerPostingId");

            for (int i = 0; i < dtbl.Rows.Count; i++)
            {
                DataRow dr = dataTable.NewRow();
                dataTable.Rows.Add(dr);
                dr["Masterid"] = masterId;
                dr["VoucherNo"] = MasterInfo.VoucherNo;
                dr["Date"] = MasterInfo.Date;
                dr["LedgerId"] = MasterInfo.LedgerId;
                dr["Narration"] = MasterInfo.Narration;
                dr["DetailsId"] = decimal.Parse(dtbl.Rows[i]["receiptDetailsId"].ToString());
                dr["Bank/Cash"] = dtbl.Rows[i]["LedgerId"].ToString();
                dr["Amount"] = dtbl.Rows[i]["Amount"].ToString();
                dr["ChequeDate"] = dtbl.Rows[i]["ChequeDate"].ToString();
                dr["ChequeNo"] = dtbl.Rows[i]["ChequeNo"].ToString();
                dr["Currency"] = dtbl.Rows[i]["ExchangeRateId"].ToString();
                dr["LedgerPostingId"] = dtbledgerId.Rows[i]["LedgerPostingId"].ToString();

            }

            DailySalaryVoucherMasterSP spMaster = new DailySalaryVoucherMasterSP();
            var Ledgers = spMaster.DailySalaryVoucherCashOrBankLedgersComboFill().AsEnumerable().Select(s => new
            {
                LedgerId = (decimal)s["LedgerId"],
                LedgerName = s["LedgerName"].ToString()
            });
            ViewBag.LedgerLists = new SelectList(Ledgers, "LedgerId", "LedgerName");
            TransactionsGeneralFill TransactionGeneralFillObj = new TransactionsGeneralFill();
            var currencylist = TransactionGeneralFillObj.CurrencyComboByDate(DateTime.Now).AsEnumerable().Select(s => new
            {
                exchangeRateId = (decimal)s["exchangeRateId"],
                currencyName = s["currencyName"].ToString()
            });
            ViewBag.currncylist = new SelectList(currencylist, "exchangeRateId", "currencyName");
            return View(dataTable);

        }
        //[AuthorizationActionFilter(EnumManager.Actions.Edit, EnumManager.Modules.ReceiptVoucher)]
        [HttpPost]
        public string ReceiptVoucherEdit()
        {
            string message;
            var isAutomatic = true;
            try
            {
                decimal decDailySuffixPrefixId = 0;
                int inTableRowCount = dtblPartyBalance.Rows.Count;
                int inB = 0;
                ReceiptMasterInfo receiptMasterInfo = new ReceiptMasterInfo();
                ReceiptMasterSP receiptMasterSP = new ReceiptMasterSP();
                ReceiptDetailsInfo receiptDetailsInfo = new ReceiptDetailsInfo();
                ReceiptDetailsSP receiptDetailsSP = new ReceiptDetailsSP();
                PartyBalanceSP SpPartyBalance = new PartyBalanceSP();
                PartyBalanceInfo InfopartyBalance = new PartyBalanceInfo();
                DateValidation objVal = new DateValidation();
                decimal decPaymentVoucherTypeId = 0;
                var date = Request.Form["date"];
                var txtVoucherNo = Request.Form["VoucherNo"].ToString();
                var txtNarration = Request.Form["Narration"].ToString();
                var cmbBankorCash = Request.Form["Legers"];
                receiptMasterInfo.Date = DateTime.Parse(date);
                receiptMasterInfo.Extra1 = string.Empty;
                receiptMasterInfo.Extra2 = string.Empty;
                receiptMasterInfo.FinancialYearId = Convert.ToDecimal(PublicVariables._decCurrentFinancialYearId.ToString());
                receiptMasterInfo.InvoiceNo = txtVoucherNo.Trim();
                receiptMasterInfo.LedgerId = Convert.ToDecimal(cmbBankorCash.ToString());
                receiptMasterInfo.Narration = txtNarration.Trim();
                receiptMasterInfo.SuffixPrefixId = decDailySuffixPrefixId;
                int decSelectedCurrencyRate = 1; //decSelectedCurrencyRate = SpExchangRate.GetExchangeRateByExchangeRateId(Convert.ToDecimal(dgvPaymentVoucher.Rows[inI].Cells["dgvcmbCurrency"].Value.ToString()));//Exchange rate of grid's row
                decimal decReceiptVoucherTypeId = 5;
                decimal decTotalAmount = int.Parse(Request.Form["totalAmount"].ToString()) * decSelectedCurrencyRate; //TotalAmountCalculation();
                receiptMasterInfo.TotalAmount = decTotalAmount;
                receiptMasterInfo.UserId = Helpers.CurrentUser.UserID;
                var strVoucherNo = string.Empty;
                TransactionsGeneralFill obj = new TransactionsGeneralFill();
                if (strVoucherNo == string.Empty)
                {
                    strVoucherNo = "0";
                }
                strVoucherNo = obj.VoucherNumberAutomaicGeneration(decReceiptVoucherTypeId, Convert.ToDecimal(strVoucherNo), DateTime.Parse(Request.Form["date"].ToString()), "ReceiptMaster");
                if (Convert.ToDecimal(strVoucherNo) != receiptMasterSP.ReceiptMasterGetMax(decReceiptVoucherTypeId) + 1)
                {
                    strVoucherNo = receiptMasterSP.ReceiptMasterGetMax(decReceiptVoucherTypeId).ToString();
                    strVoucherNo = obj.VoucherNumberAutomaicGeneration(decReceiptVoucherTypeId, Convert.ToDecimal(strVoucherNo), DateTime.Parse(Request.Form["date"].ToString()), "ReceiptMaster");
                    if (receiptMasterSP.ReceiptMasterGetMax(decReceiptVoucherTypeId) == 0)
                    {
                        strVoucherNo = "0";
                        strVoucherNo = obj.VoucherNumberAutomaicGeneration(decReceiptVoucherTypeId, Convert.ToDecimal(strVoucherNo), DateTime.Parse(Request.Form["date"].ToString()), "ReceiptMaster");
                    }
                }

                SuffixPrefixSP spSuffisprefix = new SuffixPrefixSP();
                SuffixPrefixInfo infoSuffixPrefix = new SuffixPrefixInfo();
                infoSuffixPrefix = spSuffisprefix.GetSuffixPrefixDetails(decReceiptVoucherTypeId, DateTime.Parse(Request.Form["date"].ToString()));
                var strPrefix = infoSuffixPrefix.Prefix;
                var strSuffix = infoSuffixPrefix.Suffix;
                var strInvoiceNo = strPrefix + strVoucherNo + strSuffix;

                receiptMasterInfo.VoucherNo = strVoucherNo;
                receiptMasterInfo.VoucherTypeId = decPaymentVoucherTypeId;
                receiptMasterInfo.ReceiptMasterId = decimal.Parse(Request.Form["MasterId"].ToString());
                receiptMasterInfo.TotalAmount = decTotalAmount;
                receiptMasterInfo.UserId = Helpers.CurrentUser.UserID;// "1"; //PublicVariables._decCurrentUserId;
                if (!isAutomatic)
                {
                    receiptMasterInfo.VoucherNo = txtVoucherNo.Trim();
                    receiptMasterInfo.InvoiceNo = txtVoucherNo.Trim();
                    receiptMasterInfo.SuffixPrefixId = 0;
                }
                else
                {
                    receiptMasterInfo.VoucherNo = strVoucherNo;
                    receiptMasterInfo.InvoiceNo = strInvoiceNo;
                    receiptMasterInfo.SuffixPrefixId = decDailySuffixPrefixId;
                    receiptMasterInfo.ExtraDate =DateTime.Now;
                }
                receiptMasterInfo.VoucherTypeId = decReceiptVoucherTypeId;
                decimal decEffectRow = receiptMasterSP.ReceiptMasterEdit(receiptMasterInfo);
                if (decEffectRow != 0)
                {
                    MasterLedgerPosting(Request.Form["totalAmount"], Request.Form["date"], Request.Form["voucherNo"], Request.Form["legers"], strVoucherNo, decReceiptVoucherTypeId);
                }
                var dgvtxtAmount = Request.Form["Amount"].ToString().Split(',');
                var dgvcmbCurrency = Request.Form["Currency"].ToString().Split(',');
                var dgvcmbAccountLedger = Request.Form["LegerId"].ToString().Split(',');
                var dgvtxtChequeNo = Request.Form["ChequeNumber"].ToString().Split(',');
                var dgvtxtChequeDate = Request.Form["ChequeDate"].ToString().Split(',');
                var LedgerId = Request.Form["LegerId"].Split(',');
                var dgvtxtreceiptDetailsId = Request.Form["DetailsId"].ToString().Split(',');
                var dgvtxtLedgerPostingId = Request.Form["LedgerPostingId"].ToString().Split(',');
                for (int inI = 0; inI < dgvtxtAmount.Count(); inI++)
                {
                    receiptDetailsInfo.Amount = Convert.ToDecimal(dgvtxtAmount[inI].ToString());
                    receiptDetailsInfo.ExchangeRateId = Convert.ToDecimal(dgvcmbCurrency[inI].ToString());
                    receiptDetailsInfo.Extra1 = string.Empty;
                    receiptDetailsInfo.Extra2 = string.Empty;
                    receiptDetailsInfo.ReceiptMasterId = receiptMasterInfo.ReceiptMasterId;
                    if (dgvcmbAccountLedger[inI] != null && dgvcmbAccountLedger[inI].ToString() != string.Empty)
                    {
                        receiptDetailsInfo.LedgerId = Convert.ToDecimal(dgvcmbAccountLedger[inI].ToString());
                    }
                    if (dgvtxtChequeNo[inI] != null && dgvtxtChequeNo[inI].ToString() != string.Empty)
                    {
                        receiptDetailsInfo.ChequeNo = dgvtxtChequeNo[inI].ToString();
                        if (dgvtxtChequeDate[inI] != null && dgvtxtChequeDate[inI].ToString() != string.Empty)
                        {
                            receiptDetailsInfo.ChequeDate = Convert.ToDateTime(dgvtxtChequeDate[inI]);
                        }
                        else
                        {
                            receiptDetailsInfo.ChequeDate = DateTime.Now;
                        }
                    }
                    else
                    {
                        receiptDetailsInfo.ChequeNo = string.Empty;
                        receiptDetailsInfo.ChequeDate = DateTime.Now;
                    }
                    var j = inI + 1;
                    var detailid = dgvtxtreceiptDetailsId.Count();

                    if (j > detailid)
                    {
                        decimal decReceiptDetailsId = receiptDetailsSP.ReceiptDetailsAdd(receiptDetailsInfo);
                        if (decReceiptDetailsId != 0)
                        {
                            for (int inJ = 0; inJ < inTableRowCount; inJ++)
                            {
                                if (dgvcmbAccountLedger[inI].ToString() == dtblPartyBalance.Rows[inJ]["LedgerId"].ToString())
                                {
                                    PartyBalanceAddOrEdit(inJ, Request.Form["date"], decReceiptVoucherTypeId, strInvoiceNo, Request.Form["voucherNo"], strVoucherNo);
                                }
                            }
                            inB++;
                            //DetailsLedgerPosting(inI, decReceiptDetailsId);
                            DetailsLedgerPostings(inI, decReceiptDetailsId, dgvtxtAmount[inI], "1", dgvtxtChequeNo[inI], dgvtxtChequeDate[inI], Request.Form["date"], dgvcmbAccountLedger[inI], Request.Form["VoucherNo"], strVoucherNo, decPaymentVoucherTypeId);

                        }
                    }
                    else
                    {

                        receiptDetailsInfo.ReceiptDetailsId = Convert.ToDecimal(dgvtxtreceiptDetailsId[inI].ToString());
                        decimal decReceiptDetailsId = receiptDetailsSP.ReceiptDetailsEdit(receiptDetailsInfo);
                        if (decReceiptDetailsId != 0)
                        {
                            for (int inJ = 0; inJ < inTableRowCount; inJ++)
                            {
                                if (dgvcmbAccountLedger[inI].ToString() == dtblPartyBalance.Rows[inJ]["LedgerId"].ToString())
                                {
                                    //PartyBalanceAddOrEdit(inJ);
                                    PartyBalanceAddOrEdit(inJ, Request.Form["date"].ToString(), decReceiptVoucherTypeId, strInvoiceNo, Request.Form["voucherNo"], strVoucherNo);

                                }
                            }
                            inB++;
                            decReceiptDetailsId = receiptDetailsInfo.ReceiptDetailsId;
                            decimal decLedgerPostId = Convert.ToDecimal(dgvtxtLedgerPostingId[inI].ToString());
                            //DetailsLedgerPostingEdit(inI, decLedgerPostId, decReceiptDetailsId1);
                            DetailsLedgerPostingEdit(inI, decLedgerPostId, decReceiptDetailsId, strInvoiceNo, dgvtxtAmount[inI], "1", dgvtxtChequeNo[inI], dgvtxtChequeDate[inI], Request.Form["date"], dgvcmbAccountLedger[inI], Request.Form["VoucherNo"], strVoucherNo, decPaymentVoucherTypeId);

                        }
                        else
                        {
                            decimal decDetailsId = Convert.ToDecimal(dgvtxtreceiptDetailsId[inI].ToString());
                            receiptDetailsSP.ReceiptDetailsDelete(decDetailsId);
                            //SpLedgerPosting.LedgerPostDeleteByDetailsId(decDetailsId, strVoucherNo, decReceiptVoucherTypeId);
                            for (int inJ = 0; inJ < dtblPartyBalance.Rows.Count; inJ++)
                            {
                                if (dtblPartyBalance.Rows.Count == inJ)
                                {
                                    break;
                                }
                                if (dgvcmbAccountLedger[inI] != null && dgvcmbAccountLedger[inI].ToString() != string.Empty)
                                {
                                    if (dtblPartyBalance.Rows[inJ]["LedgerId"].ToString() == dgvcmbAccountLedger[inI].ToString())
                                    {
                                        if (dtblPartyBalance.Rows[inJ]["PartyBalanceId"].ToString() != "0")
                                        {
                                            //arrlstOfDeletedPartyBalanceRow.Add(dtblPartyBalance.Rows[inJ]["PartyBalanceId"]);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                message = "success";
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return message;
        }
       // [AuthorizationActionFilter(EnumManager.Actions.Delete, EnumManager.Modules.ReceiptVoucher)]
        public IActionResult ReceiptVoucherDelete(decimal? masterId)
        {
            ReceiptMasterInfo contraMasterInfo = new ReceiptMasterInfo();
            ReceiptMasterSP masterSP = new ReceiptMasterSP();
            ReceiptDetailsSP detailsSP = new ReceiptDetailsSP();
            ReceiptDetailsInfo detailsInfo = new ReceiptDetailsInfo();
            contraMasterInfo = masterSP.ReceiptMasterViewByMasterId(Convert.ToDecimal(masterId));
            var dtbl = detailsSP.ReceiptDetailsViewByMasterId(Convert.ToDecimal(masterId));
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("DetailsId");
            dataTable.Columns.Add("Bank/Cash");
            dataTable.Columns.Add("Currency");
            dataTable.Columns.Add("Amount");
            dataTable.Columns.Add("ChequeNo");
            dataTable.Columns.Add("ChequeDate");
            dataTable.Columns.Add("Masterid");
            dataTable.Columns.Add("VoucherNo");
            dataTable.Columns.Add("Date");
            dataTable.Columns.Add("LedgerId");
            dataTable.Columns.Add("Narration");

            for (int i = 0; i < dtbl.Rows.Count; i++)
            {
                DataRow dr = dataTable.NewRow();
                dataTable.Rows.Add(dr);
                dr["Masterid"] = masterId;
                dr["VoucherNo"] = contraMasterInfo.VoucherNo;
                dr["Date"] = contraMasterInfo.Date;
                dr["LedgerId"] = contraMasterInfo.LedgerId;
                dr["Narration"] = contraMasterInfo.Narration;
                dr["DetailsId"] = decimal.Parse(dtbl.Rows[i]["ReceiptDetailsId"].ToString());
                dr["Bank/Cash"] = dtbl.Rows[i]["LedgerId"].ToString();
                dr["Amount"] = dtbl.Rows[i]["Amount"].ToString();
                dr["ChequeDate"] = dtbl.Rows[i]["ChequeDate"].ToString();
                dr["ChequeNo"] = dtbl.Rows[i]["ChequeNo"].ToString();
                dr["Currency"] = dtbl.Rows[i]["ExchangeRateId"].ToString();
            }

            DailySalaryVoucherMasterSP spMaster = new DailySalaryVoucherMasterSP();
            var Ledgers = spMaster.DailySalaryVoucherCashOrBankLedgersComboFill().AsEnumerable().Select(s => new
            {
                LedgerId = (decimal)s["LedgerId"],
                LedgerName = s["LedgerName"].ToString()
            });
            ViewBag.LedgerLists = new SelectList(Ledgers, "LedgerId", "LedgerName");
            TransactionsGeneralFill TransactionGeneralFillObj = new TransactionsGeneralFill();
            var currencylist = TransactionGeneralFillObj.CurrencyComboByDate(DateTime.Now).AsEnumerable().Select(s => new
            {
                exchangeRateId = (decimal)s["exchangeRateId"],
                currencyName = s["currencyName"].ToString()
            });
            ViewBag.currncylist = new SelectList(currencylist, "exchangeRateId", "currencyName");
            return View(dataTable);
        }
       // [AuthorizationActionFilter(EnumManager.Actions.Delete, EnumManager.Modules.ReceiptVoucher)]
        [HttpPost]
        public string ReceiptVoucherDelete()
        {
            string message;
            try
            {
                var decReceiptVoucherTypeId = 5;
                var decPaymentVoucherTypeId = Request.Form["MasterId"];
                ReceiptMasterSP receiptMasterSP = new ReceiptMasterSP();
                PartyBalanceSP SpPartyBalance = new PartyBalanceSP();
                var strVoucherNo = string.Empty;
                TransactionsGeneralFill obj = new TransactionsGeneralFill();
                if (strVoucherNo == string.Empty)
                {
                    strVoucherNo = "0";
                }
                strVoucherNo = obj.VoucherNumberAutomaicGeneration(decReceiptVoucherTypeId, Convert.ToDecimal(strVoucherNo), DateTime.Parse(Request.Form["date"]), "ReceiptMaster");
                if (Convert.ToDecimal(strVoucherNo) != receiptMasterSP.ReceiptMasterGetMax(decReceiptVoucherTypeId) + 1)
                {
                    strVoucherNo = receiptMasterSP.ReceiptMasterGetMax(decReceiptVoucherTypeId).ToString();
                    strVoucherNo = obj.VoucherNumberAutomaicGeneration(decReceiptVoucherTypeId, Convert.ToDecimal(strVoucherNo), DateTime.Parse(Request.Form["date"]), "ReceiptMaster");
                    if (receiptMasterSP.ReceiptMasterGetMax(decReceiptVoucherTypeId) == 0)
                    {
                        strVoucherNo = "0";
                        strVoucherNo = obj.VoucherNumberAutomaicGeneration(decReceiptVoucherTypeId, Convert.ToDecimal(strVoucherNo), DateTime.Parse(Request.Form["date"]), "ReceiptMaster");
                    }
                }
                if (!SpPartyBalance.PartyBalanceCheckReference(decReceiptVoucherTypeId, strVoucherNo))
                {
                    receiptMasterSP.ReceiptVoucherDelete(decimal.Parse(decPaymentVoucherTypeId), decReceiptVoucherTypeId, strVoucherNo);
                    message = "success";

                }
                else
                {
                    message = "Reference exist. Cannot delete";
                }

            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return message;
        }
        #endregion

        #region Journel Voucher
      //  [AuthorizationActionFilter(EnumManager.Actions.Any, EnumManager.Modules.JournalVoucher)]
        public IActionResult IndexJournal(string FromDate, string ToDate, string VoucherNo)
        {
            TransactionsGeneralFill Obj = new TransactionsGeneralFill();
            var Ledgers = Obj.BankOrCashComboFill(false).AsEnumerable().Select(s => new
            {
                LedgerId = (decimal)s["LedgerId"],
                LedgerName = s["LedgerName"].ToString()
            });
            ViewBag.LedgerLists = new SelectList(Ledgers, "LedgerId", "LedgerName");
            JournalMasterSP Master = new JournalMasterSP();
            DataTable dtbl = new DataTable();
            if (VoucherNo == null)
            {
                VoucherNo = "";
            }
            if (FromDate == null || ToDate == null)
            {
                FromDate = DateTime.Now.ToString();
                ToDate = DateTime.Now.ToString();
            }

            dtbl = Master.JournalRegisterSearch(VoucherNo.Trim(), FromDate, ToDate);
            return View(dtbl);
        }
       // [AuthorizationActionFilter(EnumManager.Actions.Save, EnumManager.Modules.JournalVoucher)]
        public IActionResult JournelVoucher()
        {
            DailySalaryVoucherMasterSP spMaster = new DailySalaryVoucherMasterSP();
            var Ledgers = spMaster.DailySalaryVoucherCashOrBankLedgersComboFill().AsEnumerable().Select(s => new
            {
                LedgerId = (decimal)s["LedgerId"],
                LedgerName = s["LedgerName"].ToString()
            });
            ViewBag.LedgerLists = new SelectList(Ledgers, "LedgerId", "LedgerName");
            TransactionsGeneralFill TransactionGeneralFillObj = new TransactionsGeneralFill();
            var currencylist = TransactionGeneralFillObj.CurrencyComboByDate(DateTime.Now).AsEnumerable().Select(s => new
            {
                exchangeRateId = (decimal)s["exchangeRateId"],
                currencyName = s["currencyName"].ToString()
            });
            ViewBag.currncylist = new SelectList(currencylist, "exchangeRateId", "currencyName");
            return View();
        }
       // [AuthorizationActionFilter(EnumManager.Actions.Save, EnumManager.Modules.JournalVoucher)]
        [HttpPost]
        public string JournelVoucher(JournalDetailsSP journal)
        {
            string message;
            try
            {
                decimal decTotalDebit = 0;
                decimal decTotalCredit = 0;

                decTotalDebit = Convert.ToDecimal(Request.Form["totalDr"].ToString().Trim());
                decTotalCredit = Convert.ToDecimal(Request.Form["totalCr"].ToString().Trim());

                JournalMasterSP spJournalMaster = new JournalMasterSP();
                JournalDetailsSP spJournalDetails = new JournalDetailsSP();
                JournalMasterInfo infoJournalMaster = new JournalMasterInfo();
                JournalDetailsInfo infoJournalDetails = new JournalDetailsInfo();
                PartyBalanceSP SpPartyBalance = new PartyBalanceSP();
                PartyBalanceInfo InfopartyBalance = new PartyBalanceInfo();
                ExchangeRateSP spExchangeRate = new ExchangeRateSP();
                TransactionsGeneralFill obj = new TransactionsGeneralFill();
                var strVoucherNo = string.Empty;
                string tableName = "JournalMaster";
                decimal decJournalVoucherTypeId = 6;
                decimal decJournalSuffixPrefixId = 0;
                if(Request.Form["VoucherNo"].ToString() == null)
                {
                    return "Invalid Voucher No";
                }
                var voucherNo = Request.Form["VoucherNo"].ToString();
                if (strVoucherNo == string.Empty)
                {
                    strVoucherNo = "0"; //strMax;
                }
                if (spJournalMaster.JournalVoucherCheckExistance(Request.Form["VoucherNo"].ToString().Trim(), decJournalVoucherTypeId, 0) == false)
                {
                    strVoucherNo = obj.VoucherNumberAutomaicGeneration(decJournalVoucherTypeId, Convert.ToDecimal(strVoucherNo), DateTime.Parse(Request.Form["date"].ToString()), tableName);
                    if (Convert.ToDecimal(strVoucherNo) != spJournalMaster.JournalMasterGetMaxPlusOne(decJournalVoucherTypeId))
                    {
                        strVoucherNo = spJournalMaster.JournalMasterGetMax(decJournalVoucherTypeId).ToString();
                        strVoucherNo = obj.VoucherNumberAutomaicGeneration(decJournalVoucherTypeId, Convert.ToDecimal(strVoucherNo), DateTime.Parse(Request.Form["date"]), tableName);
                        if (spJournalMaster.JournalMasterGetMax(decJournalVoucherTypeId).ToString() == "0")
                        {
                            strVoucherNo = "0";
                            strVoucherNo = obj.VoucherNumberAutomaicGeneration(decJournalVoucherTypeId, Convert.ToDecimal(strVoucherNo), DateTime.Parse(Request.Form["date"]), tableName);
                        }
                    }
                    infoJournalMaster.VoucherNo = strVoucherNo;
                    infoJournalMaster.InvoiceNo =voucherNo;
                    SuffixPrefixInfo infoSuffixPrefix = new SuffixPrefixInfo();
                    decJournalSuffixPrefixId = infoSuffixPrefix.SuffixprefixId;
                    infoJournalMaster.SuffixPrefixId = decJournalSuffixPrefixId;
                    infoJournalMaster.Date = Convert.ToDateTime(Request.Form["date"]);
                    var narration = "";
                    if(Request.Form["Narration"].ToString() != null)
                    {
                        narration = Request.Form["Narration"].ToString().Trim();
                    }
                    infoJournalMaster.Narration = narration;
                    infoJournalMaster.UserId =_user.UserID;//PublicVariables._decCurrentUserId;
                    infoJournalMaster.VoucherTypeId = decJournalVoucherTypeId;
                    infoJournalMaster.FinancialYearId = Convert.ToDecimal(PublicVariables._decCurrentFinancialYearId.ToString());

                    infoJournalMaster.Extra1 = string.Empty;
                    infoJournalMaster.Extra2 = string.Empty;
                    infoJournalMaster.ExtraDate = DateTime.Now;


                    infoJournalMaster.TotalAmount = decTotalDebit;
                    decimal decJournalMasterId = spJournalMaster.JournalMasterAdd(infoJournalMaster);

                    /*******************JournalDetailsAdd and LedgerPosting*************************/
                    infoJournalDetails.JournalMasterId = decJournalMasterId;
                    infoJournalDetails.ExtraDate = DateTime.Now;
                    infoJournalDetails.Extra1 = string.Empty;
                    infoJournalDetails.Extra2 = string.Empty;
                    var dgvcmbDrOrCr = Request.Form["DrCr"].ToString().Split(',');
                    var dgvcmbAccountLedger = Request.Form["LegerId"].ToString().Split(',');
                    var dgvtxtAmount = Request.Form["Amount"].ToString().Split(',');
                    var dgvtxtChequeNo = Request.Form["ChequeNumber"].ToString().Split(',');
                    var dgvtxtChequeDate = Request.Form["ChequeDate"].ToString().Split(',');
                    
                    var dgvcmbCurrency =PublicVariables._decCurrencyId;
                    SuffixPrefixSP spSuffisprefix = new SuffixPrefixSP();
                    infoSuffixPrefix = spSuffisprefix.GetSuffixPrefixDetails(decJournalVoucherTypeId, DateTime.Parse(Request.Form["date"]));
                    var strPrefix = infoSuffixPrefix.Prefix;
                    var strSuffix = infoSuffixPrefix.Suffix;
                    var strInvoiceNo = strPrefix + strVoucherNo + strSuffix;
                    decimal decLedgerId = 0;
                    decimal decDebit = 0;
                    decimal decCredit = 0;
                    for (int inI = 0; inI < dgvtxtAmount.Count(); inI++)
                    {
                        if (dgvcmbAccountLedger[inI] != null && dgvcmbAccountLedger[inI].ToString() != string.Empty)
                        {
                            infoJournalDetails.LedgerId = Convert.ToDecimal(dgvcmbAccountLedger[inI].ToString());
                            decLedgerId = infoJournalDetails.LedgerId;
                        }
                        if (dgvcmbDrOrCr[inI] != null && dgvcmbDrOrCr[inI].ToString() != string.Empty)
                        {
                            if (dgvtxtAmount[inI] != null && dgvtxtAmount[inI].ToString() != string.Empty)
                            {
                                //--------Currency conversion--------------//
                                decimal decSelectedCurrencyRate = 1;
                                //decSelectedCurrencyRate = spExchangeRate.GetExchangeRateByExchangeRateId(Convert.ToDecimal(dgvcmbCurrency[inI].ToString()));
                                var decAmount = Convert.ToDecimal(dgvtxtAmount[inI].ToString());
                                var decConvertRate = decAmount * decSelectedCurrencyRate;
                                //===========================================//
                                if (dgvcmbDrOrCr[inI].ToString() == "Dr")
                                {
                                    infoJournalDetails.Debit = Convert.ToDecimal(dgvtxtAmount[inI].ToString());
                                    infoJournalDetails.Credit = 0;
                                    decDebit = decConvertRate;
                                    decCredit = infoJournalDetails.Credit;
                                }
                                else
                                {
                                    infoJournalDetails.Credit = Convert.ToDecimal(dgvtxtAmount[inI].ToString());
                                    infoJournalDetails.Debit = 0;
                                    decDebit = infoJournalDetails.Debit;
                                    decCredit = decConvertRate;
                                }
                            }
                            //infoJournalDetails.ExchangeRateId = Convert.ToDecimal(dgvcmbCurrency[inI].ToString());
                            if (dgvtxtChequeNo[inI] != null && dgvtxtChequeNo[inI].ToString() != string.Empty)
                            {
                                infoJournalDetails.ChequeNo = dgvtxtChequeNo[inI].ToString();
                            }
                            else
                            {
                                infoJournalDetails.ChequeNo = string.Empty;
                            }
                            if (dgvtxtChequeDate[inI] != null && dgvtxtChequeDate[inI].ToString() != string.Empty)
                            {
                                infoJournalDetails.ChequeDate = Convert.ToDateTime(dgvtxtChequeDate[inI].ToString());
                            }
                            else
                            {
                                infoJournalDetails.ChequeDate = DateTime.Now;
                            }
                            decimal decJournalDetailsId = spJournalDetails.JournalDetailsAdd(infoJournalDetails);
                            if (decJournalDetailsId != 0)
                            {
                                //    PartyBalanceAddOrEdit(inI);
                              //  PartyBalanceAddOrEdit(inI, Request.Form["date"], decJournalVoucherTypeId, strInvoiceNo, Request.Form["voucherNo"], strVoucherNo);
                                //    LedgerPosting(decLedgerId, decCredit, decDebit, decJournalDetailsId, inI);
                                LedgerPosting(dgvcmbAccountLedger[inI], strVoucherNo, Request.Form["VoucherNo"].ToString(), dgvtxtAmount[inI], Request.Form["date"].ToString(), dgvtxtChequeDate[inI], dgvtxtChequeNo[inI], decJournalVoucherTypeId, decLedgerId, decCredit, decDebit, decJournalDetailsId, inI);

                            }
                        }
                    }
                    message = "success";

                    //----------------If print after save is enable-----------------------//

                }
                //===================================================================//
                else
                {
                    message = "Voucher number already exist";
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }                  

            return message;
        }
       // [AuthorizationActionFilter(EnumManager.Actions.Edit, EnumManager.Modules.JournalVoucher)]
        public IActionResult JournelVoucherEdit(decimal? masterId)
        {
            JournalMasterInfo MasterInfo = new JournalMasterInfo();
            JournalMasterSP masterSP = new JournalMasterSP();
            JournalDetailsSP detailsSP = new JournalDetailsSP();
            JournalDetailsInfo detailsInfo = new JournalDetailsInfo();
            MasterInfo = masterSP.JournalMasterView(Convert.ToDecimal(masterId));
            var dtbl = detailsSP.JournalDetailsViewByMasterId(Convert.ToDecimal(masterId));
            LedgerPostingSP ledgerPostingSP = new LedgerPostingSP();
            LedgerPostingInfo ledgerPostingInfo = new LedgerPostingInfo();
            string voucherNo = MasterInfo.VoucherNo;
            decimal VoucherTypeId = MasterInfo.VoucherTypeId;
            DataTable dtbledgerId = new DataTable();
            dtbledgerId = ledgerPostingSP.GetLedgerPostingIds(voucherNo, VoucherTypeId);

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("DetailsId");
            dataTable.Columns.Add("Bank/Cash");
            dataTable.Columns.Add("Currency");
            dataTable.Columns.Add("Amount");
            dataTable.Columns.Add("ChequeNo");
            dataTable.Columns.Add("ChequeDate");
            dataTable.Columns.Add("Masterid");
            dataTable.Columns.Add("VoucherNo");
            dataTable.Columns.Add("Date");
            dataTable.Columns.Add("LedgerId");
            dataTable.Columns.Add("Narration");
            dataTable.Columns.Add("dgvcmbDrOrCr");
            dataTable.Columns.Add("LedgerPostingId");

            for (int i = 0; i < dtbl.Rows.Count; i++)
            {
                DataRow dr = dataTable.NewRow();
                dataTable.Rows.Add(dr);
                dr["Masterid"] = MasterInfo.JournalMasterId;
                dr["VoucherNo"] = MasterInfo.VoucherNo;
                dr["Date"] = MasterInfo.Date;
                //dr["LedgerId"] = MasterInfo.LedgerId;
                dr["Narration"] = MasterInfo.Narration;
                dr["DetailsId"] = decimal.Parse(dtbl.Rows[i]["JournalDetailsId"].ToString());
                dr["Bank/Cash"] = dtbl.Rows[i]["LedgerId"].ToString();
                if (Convert.ToDecimal(dtbl.Rows[i]["debit"].ToString()) == 0)
                {
                    dr["dgvcmbDrOrCr"] = "Cr";
                    dr["Amount"] = Convert.ToDecimal(dtbl.Rows[i]["credit"].ToString());
                }
                else
                {
                    dr["dgvcmbDrOrCr"] = "Dr";
                    dr["Amount"] = Convert.ToDecimal(dtbl.Rows[i]["debit"].ToString());
                }
                dr["ChequeDate"] = dtbl.Rows[i]["ChequeDate"].ToString();
                dr["ChequeNo"] = dtbl.Rows[i]["ChequeNo"].ToString();
                dr["Currency"] = dtbl.Rows[i]["ExchangeRateId"].ToString();
                if (dtbledgerId.Rows[i]["LedgerPostingId"] != null)
                {
                    dr["LedgerPostingId"] = dtbledgerId.Rows[i]["LedgerPostingId"].ToString();
                }
            }

            DailySalaryVoucherMasterSP spMaster = new DailySalaryVoucherMasterSP();
            var Ledgers = spMaster.DailySalaryVoucherCashOrBankLedgersComboFill().AsEnumerable().Select(s => new
            {
                LedgerId = (decimal)s["LedgerId"],
                LedgerName = s["LedgerName"].ToString()
            });
            ViewBag.LedgerLists = new SelectList(Ledgers, "LedgerId", "LedgerName");
            TransactionsGeneralFill TransactionGeneralFillObj = new TransactionsGeneralFill();
            var currencylist = TransactionGeneralFillObj.CurrencyComboByDate(DateTime.Now).AsEnumerable().Select(s => new
            {
                exchangeRateId = (decimal)s["exchangeRateId"],
                currencyName = s["currencyName"].ToString()
            });
            ViewBag.currncylist = new SelectList(currencylist, "exchangeRateId", "currencyName");
            return View(dataTable);
        }
       // [AuthorizationActionFilter(EnumManager.Actions.Edit, EnumManager.Modules.JournalVoucher)]
        [HttpPost]
        public string JournelVoucherEdit()
        {
            string message;
            ArrayList arrlstOfRemove = new ArrayList();
            ArrayList arrlstOfRemovedLedgerPostingId = new ArrayList();
            var dgvcmbDrOrCr = Request.Form["DrCr"].ToString().Split(',');
            var dgvcmbAccountLedger = Request.Form["LegerId"].ToString().Split(',');
            var dgvtxtAmount = Request.Form["Amount"].ToString().Split(',');
            var dgvtxtChequeNo = Request.Form["ChequeNumber"].Split(',');
            var dgvtxtChequeDate = Request.Form["ChequeDate"].ToString().Split(',');
            //var dgvtxtSlNo = Request.Form["SINo"].Split(',');
            //var dgvcmbCurrency = Request.Form["Currency"].Split(',');
            var dgvtxtDetailsId = Request.Form["DetailsId"].ToString().Split(',');
            var dgvtxtLedgerPostingId = Request.Form["LedgerPostingId"].ToString().Split(',');

            try
            {
                JournalMasterSP spJournalMaster = new JournalMasterSP();
                JournalMasterInfo infoJournalMaster = new JournalMasterInfo();
                JournalDetailsSP spJournalDetails = new JournalDetailsSP();
                JournalDetailsInfo infoJournalDetails = new JournalDetailsInfo();
                ExchangeRateSP spExchangeRate = new ExchangeRateSP();

                /*****************Update in JournalMaster table *************/

                decimal decTotalDebit = 0;
                decimal decTotalCredit = 0;
                TransactionsGeneralFill obj = new TransactionsGeneralFill();
                var strVoucherNo = string.Empty;
                string tableName = "JournalMaster";
                decimal decJournalVoucherTypeId = 6;
                decimal decJournalSuffixPrefixId = 0;
                if (strVoucherNo == string.Empty)
                {

                    strVoucherNo = "0"; //strMax;
                }
                strVoucherNo = obj.VoucherNumberAutomaicGeneration(decJournalVoucherTypeId, Convert.ToDecimal(strVoucherNo), DateTime.Parse(Request.Form["date"]), tableName);
                if (Convert.ToDecimal(strVoucherNo) != spJournalMaster.JournalMasterGetMaxPlusOne(decJournalVoucherTypeId))
                {
                    strVoucherNo = spJournalMaster.JournalMasterGetMax(decJournalVoucherTypeId).ToString();
                    strVoucherNo = obj.VoucherNumberAutomaicGeneration(decJournalVoucherTypeId, Convert.ToDecimal(strVoucherNo), DateTime.Parse(Request.Form["date"]), tableName);
                    if (spJournalMaster.JournalMasterGetMax(decJournalVoucherTypeId).ToString() == "0")
                    {
                        strVoucherNo = "0";
                        strVoucherNo = obj.VoucherNumberAutomaicGeneration(decJournalVoucherTypeId, Convert.ToDecimal(strVoucherNo), DateTime.Parse(Request.Form["date"]), tableName);
                    }
                }
                var decJournalMasterId = decimal.Parse(Request.Form["MasterId"].ToString());
                var txtDate = Request.Form["date"].ToString();
                var txtVoucherNo = Request.Form["VoucherNo"].ToString();
                var txtNarration = Request.Form["Narration"].ToString();
                infoJournalMaster.JournalMasterId = decJournalMasterId;
                infoJournalMaster.VoucherNo = strVoucherNo;
                infoJournalMaster.InvoiceNo = txtVoucherNo.Trim();
                infoJournalMaster.SuffixPrefixId = decJournalSuffixPrefixId;
                infoJournalMaster.Date = Convert.ToDateTime(txtDate);
                infoJournalMaster.Narration = txtNarration.Trim();
                infoJournalMaster.UserId = "1";// Helpers.CurrentUser.UserID; //PublicVariables._decCurrentUserId;
                infoJournalMaster.VoucherTypeId = decJournalVoucherTypeId;
                infoJournalMaster.FinancialYearId = Convert.ToDecimal(PublicVariables._decCurrentFinancialYearId.ToString());
                infoJournalMaster.ExtraDate = DateTime.Now;
                infoJournalMaster.Extra1 = string.Empty;
                infoJournalMaster.Extra2 = string.Empty;


                decTotalDebit = Convert.ToDecimal(Request.Form["totalDr"].ToString().Trim());
                decTotalCredit = Convert.ToDecimal(Request.Form["totalCr"].ToString().Trim());

                infoJournalMaster.TotalAmount = decTotalDebit;
                decimal decEffectRow = spJournalMaster.JournalMasterEdit(infoJournalMaster);

                /**********************JournalDetails Edit********************/
                if (decEffectRow > 0)
                {
                    infoJournalDetails.JournalMasterId = decJournalMasterId;
                    infoJournalDetails.ExtraDate = DateTime.Now;
                    infoJournalDetails.Extra1 = string.Empty;
                    infoJournalDetails.Extra2 = string.Empty;

                    //-----------to delete details, LedgerPosting and bankReconciliation of removed rows--------------// 
                    LedgerPostingSP spLedgerPosting = new LedgerPostingSP();

                    foreach (object objs in arrlstOfRemove)
                    {
                        string str = Convert.ToString(objs);
                        spJournalDetails.JournalDetailsDelete(Convert.ToDecimal(str));
                        spLedgerPosting.LedgerPostDeleteByDetailsId(Convert.ToDecimal(str), strVoucherNo, decJournalVoucherTypeId);
                    }
                    spLedgerPosting.LedgerPostingDeleteByVoucherNoVoucherTypeIdAndLedgerId(strVoucherNo, decJournalVoucherTypeId, 12);
                    //=============================================================================================//

                    decimal decLedgerId = 0;
                    decimal decDebit = 0;
                    decimal decCredit = 0;
                    decimal decJournalDetailsId = 0;
                    SuffixPrefixSP spSuffisprefix = new SuffixPrefixSP();
                    SuffixPrefixInfo infoSuffixPrefix = new SuffixPrefixInfo();
                    infoSuffixPrefix = spSuffisprefix.GetSuffixPrefixDetails(decJournalVoucherTypeId, DateTime.Parse(Request.Form["date"]));
                    var strPrefix = infoSuffixPrefix.Prefix;
                    var strSuffix = infoSuffixPrefix.Suffix;
                    var strInvoiceNo = strPrefix + strVoucherNo + strSuffix;
                    for (int inI = 0; inI < dgvtxtAmount.Count(); inI++)
                    {
                        if (dgvcmbAccountLedger[inI] != null && dgvcmbAccountLedger[inI].ToString() != string.Empty)
                        {
                            infoJournalDetails.LedgerId = Convert.ToDecimal(dgvcmbAccountLedger[inI].ToString());
                            decLedgerId = infoJournalDetails.LedgerId;
                        }
                        if (dgvcmbDrOrCr[inI] != null && dgvcmbDrOrCr[inI].ToString() != string.Empty)
                        {
                            //------------------Currency conversion------------------//
                            var decSelectedCurrencyRate = 1; //spExchangeRate.GetExchangeRateByExchangeRateId(Convert.ToDecimal(dgvcmbCurrency[inI]));
                            var decAmount = Convert.ToDecimal(dgvtxtAmount[inI].ToString());
                            var decConvertRate = decAmount * decSelectedCurrencyRate;
                            //======================================================//

                            if (dgvcmbDrOrCr[inI].ToString() == "Dr")
                            {
                                infoJournalDetails.Debit = Convert.ToDecimal(dgvtxtAmount[inI].ToString());
                                infoJournalDetails.Credit = 0;

                                decDebit = decConvertRate;
                                decCredit = infoJournalDetails.Credit;
                            }
                            else
                            {
                                infoJournalDetails.Credit = Convert.ToDecimal(dgvtxtAmount[inI].ToString());
                                infoJournalDetails.Debit = 0;
                                decDebit = infoJournalDetails.Debit;
                                decCredit = decConvertRate;
                            }
                            //infoJournalDetails.ExchangeRateId = Convert.ToDecimal(dgvcmbCurrency[inI].ToString());
                            if (dgvtxtChequeNo[inI] != null && dgvtxtChequeNo[inI].ToString() != string.Empty)
                            {
                                infoJournalDetails.ChequeNo = dgvtxtChequeNo[inI].ToString();
                            }
                            else
                            {
                                infoJournalDetails.ChequeNo = string.Empty;
                            }
                            if (dgvtxtChequeDate[inI] != null && dgvtxtChequeDate[inI].ToString() != string.Empty)
                            {
                                infoJournalDetails.ChequeDate = Convert.ToDateTime(dgvtxtChequeDate[inI].ToString());
                            }
                            else
                            {
                                infoJournalDetails.ChequeDate = DateTime.Now;
                            }
                            var j = inI + 1;
                            var detailid = dgvtxtDetailsId.Count();

                            if (j <= detailid)
                            {
                                infoJournalDetails.JournalDetailsId = Convert.ToDecimal(dgvtxtDetailsId[inI].ToString());
                                spJournalDetails.JournalDetailsEdit(infoJournalDetails);
                                PartyBalanceAddOrEdit(inI, Request.Form["date"], decJournalVoucherTypeId, strInvoiceNo, Request.Form["voucherNo"], strVoucherNo);
                                decJournalDetailsId = infoJournalDetails.JournalDetailsId;
                                decimal decLedgerPostId = Convert.ToDecimal(dgvtxtLedgerPostingId[inI].ToString());
                                //LedgerPostingEdit(decLedgerPostId, decLedgerId, decCredit, decDebit, decJournalDetailsId, inI);
                                LedgerPostingEdit(dgvcmbAccountLedger[inI], strVoucherNo, Request.Form["VoucherNo"], dgvtxtAmount[inI], Request.Form["date"], dgvtxtChequeDate[inI], dgvtxtChequeNo[inI], decJournalVoucherTypeId, decLedgerPostId, decLedgerId, decCredit, decDebit, decJournalDetailsId, inI);


                            }
                            else
                            {
                                decJournalDetailsId = spJournalDetails.JournalDetailsAdd(infoJournalDetails);
                                PartyBalanceAddOrEdit(inI, Request.Form["date"], decJournalVoucherTypeId, strInvoiceNo, Request.Form["voucherNo"], strVoucherNo);
                                //LedgerPosting(decLedgerId, decCredit, decDebit, decJournalDetailsId, inI);
                                LedgerPosting(dgvcmbAccountLedger[inI], strVoucherNo, Request.Form["VoucherNo"], dgvtxtAmount[inI], Request.Form["date"], dgvtxtChequeDate[inI], dgvtxtChequeNo[inI], decJournalVoucherTypeId, decLedgerId, decCredit, decDebit, decJournalDetailsId, inI);

                            }

                        }

                    }
                    //DeletePartyBalanceOfRemovedRow();
                    message = "success";
                }
                //----------------If print after save is enable-----------------------//

                message = "success";
                //===================================================================//
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return message;
        }
       // [AuthorizationActionFilter(EnumManager.Actions.Delete, EnumManager.Modules.JournalVoucher)]
        public IActionResult JournelVoucherDelete(decimal? masterId)
        {
            JournalMasterInfo MasterInfo = new JournalMasterInfo();
            JournalMasterSP masterSP = new JournalMasterSP();
            JournalDetailsSP detailsSP = new JournalDetailsSP();
            JournalDetailsInfo detailsInfo = new JournalDetailsInfo();
            MasterInfo = masterSP.JournalMasterView(Convert.ToDecimal(masterId));
            var dtbl = detailsSP.JournalDetailsViewByMasterId(Convert.ToDecimal(masterId));
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("DetailsId");
            dataTable.Columns.Add("Bank/Cash");
            dataTable.Columns.Add("Currency");
            dataTable.Columns.Add("Amount");
            dataTable.Columns.Add("ChequeNo");
            dataTable.Columns.Add("ChequeDate");
            dataTable.Columns.Add("Masterid");
            dataTable.Columns.Add("VoucherNo");
            dataTable.Columns.Add("Date");
            dataTable.Columns.Add("LedgerId");
            dataTable.Columns.Add("Narration");
            dataTable.Columns.Add("VoucherTypeId");
            for (int i = 0; i < dtbl.Rows.Count; i++)
            {
                DataRow dr = dataTable.NewRow();
                dataTable.Rows.Add(dr);
                dr["Masterid"] = MasterInfo.JournalMasterId;
                dr["VoucherTypeId"] = MasterInfo.VoucherTypeId;

                dr["VoucherNo"] = MasterInfo.VoucherNo;
                dr["Date"] = MasterInfo.Date;
                //dr["LedgerId"] = MasterInfo.LedgerId;
                dr["Narration"] = MasterInfo.Narration;
                dr["DetailsId"] = decimal.Parse(dtbl.Rows[i]["JournalDetailsId"].ToString());
                dr["Bank/Cash"] = dtbl.Rows[i]["LedgerId"].ToString();
                //dr["Amount"] = dtbl.Rows[i]["Amount"].ToString();
                dr["ChequeDate"] = dtbl.Rows[i]["ChequeDate"].ToString();
                dr["ChequeNo"] = dtbl.Rows[i]["ChequeNo"].ToString();
                dr["Currency"] = dtbl.Rows[i]["ExchangeRateId"].ToString();
            }

            DailySalaryVoucherMasterSP spMaster = new DailySalaryVoucherMasterSP();
            var Ledgers = spMaster.DailySalaryVoucherCashOrBankLedgersComboFill().AsEnumerable().Select(s => new
            {
                LedgerId = (decimal)s["LedgerId"],
                LedgerName = s["LedgerName"].ToString()
            });
            ViewBag.LedgerLists = new SelectList(Ledgers, "LedgerId", "LedgerName");
            TransactionsGeneralFill TransactionGeneralFillObj = new TransactionsGeneralFill();
            var currencylist = TransactionGeneralFillObj.CurrencyComboByDate(DateTime.Now).AsEnumerable().Select(s => new
            {
                exchangeRateId = (decimal)s["exchangeRateId"],
                currencyName = s["currencyName"].ToString()
            });
            ViewBag.currncylist = new SelectList(currencylist, "exchangeRateId", "currencyName");
            return View(dataTable);
        }
     //   [AuthorizationActionFilter(EnumManager.Actions.Delete, EnumManager.Modules.JournalVoucher)]
        [HttpPost]
        public string JournelVoucherDelete()
        {
            string message;
            var decJournelVoucherTypeId = Request.Form["VoucherTypeId"].ToString();
            var strVoucherNo = Request.Form["VoucherNo"].ToString();
            var decJournelMasterId = Request.Form["MasterId"].ToString();

            try
            {
                JournalMasterSP spJournelMaster = new JournalMasterSP();
                PartyBalanceSP spPartyBalance = new PartyBalanceSP();

                if (!spPartyBalance.PartyBalanceCheckReference(decimal.Parse(decJournelVoucherTypeId), strVoucherNo))
                {
                    spJournelMaster.JournalVoucherDelete(decimal.Parse(decJournelMasterId), decimal.Parse(decJournelVoucherTypeId), strVoucherNo);

                    message = "success";
                }
                message = "success";
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return message;
        }
        #endregion

        #region Vendor Pay Bill
        public IActionResult Indexvender(string FromDate, string ToDate, string VoucherNo, string LedgerId)
        {
            TransactionsGeneralFill Obj = new TransactionsGeneralFill();
            var Ledgers = Obj.BankOrCashComboFill(false).AsEnumerable().Select(s => new
            {
                LedgerId = (decimal)s["LedgerId"],
                LedgerName = s["LedgerName"].ToString()
            });
            ViewBag.LedgerLists = new SelectList(Ledgers, "LedgerId", "LedgerName");
            if (VoucherNo == null)
            {
                VoucherNo = "";
            }
            if (FromDate == null || ToDate == null)
            {
                FromDate = DateTime.Now.ToString();
                ToDate = DateTime.Now.ToString();
            }
            PaymentMasterSP Master = new PaymentMasterSP();
            PaymentMasterInfo infoPaymentMaster = new PaymentMasterInfo();
            DataTable dtbl = new DataTable();

            if (LedgerId != "System.Data.DataRowView")
            {
                if (FromDate != string.Empty && ToDate.Trim() != string.Empty)
                {
                    dtbl = Master.PaymentMasterSearch(Convert.ToDateTime(FromDate.ToString()), Convert.ToDateTime(ToDate.ToString()), Convert.ToDecimal(LedgerId), VoucherNo);
                }
            }

            return View(dtbl);
        }

        public IActionResult VendorPayBill()
        {
            DailySalaryVoucherMasterSP spMaster = new DailySalaryVoucherMasterSP();
            var Ledgers = spMaster.DailySalaryVoucherCashOrBankLedgersComboFill().AsEnumerable().Select(s => new
            {
                LedgerId = (decimal)s["LedgerId"],
                LedgerName = s["LedgerName"].ToString()
            });
            ViewBag.LedgerLists = new SelectList(Ledgers, "LedgerId", "LedgerName");
            TransactionsGeneralFill TransactionGeneralFillObj = new TransactionsGeneralFill();
            var currencylist = TransactionGeneralFillObj.CurrencyComboByDate(DateTime.Now).AsEnumerable().Select(s => new
            {
                exchangeRateId = (decimal)s["exchangeRateId"],
                currencyName = s["currencyName"].ToString()
            });
            ViewBag.currncylist = new SelectList(currencylist, "exchangeRateId", "currencyName");
            return View();
        }
        [HttpPost]
        public string VendorPayBill(PayHeadSP headSP)
        {

                string message;
                try
                {
                    decimal decCalcAmount = 0;
                    decimal decBalance = 0;
                    string strStatus = string.Empty;
                    ContraMasterSP spContraMaster = new ContraMasterSP();
                
                    AccountLedgerSP spAccountLedger = new AccountLedgerSP();
                    SettingsSP spSettings = new SettingsSP();
                    var radio = Request.Form["radio"].ToString();
                    int a = 0;
                    var dgvcmbBankorCashAccount = Request.Form["LegerId"].ToString().Split(',');

                    if (Request.Form["date"].ToString() != string.Empty)
                    {
                        strStatus = spSettings.SettingsStatusCheck("NegativeCashTransaction");
                        if (radio == "withdraw")
                        {
                            decBalance = spAccountLedger.CheckLedgerBalance(Convert.ToDecimal(Request.Form["Legers"].ToString()));
                            decCalcAmount = decBalance - Convert.ToDecimal(Request.Form["TotalAmount"].ToString());
                            if (decCalcAmount < 0)
                            {
                                if (strStatus == "Warn")
                                {
                                    //message = saveOrEdit(isAutomatic, decContraSuffixPrefixId, DecContraVoucherTypeId, spContraMaster, radio, dgvcmbBankorCashAccount);
                                }
                                else if (strStatus == "Block")
                                {
                                    return "Cannot continue ,due to negative balance";
                                }
                                else
                                {
                                    //message = saveOrEdit(isAutomatic, decContraSuffixPrefixId, DecContraVoucherTypeId, spContraMaster, radio, dgvcmbBankorCashAccount);
                                }
                            }
                            else
                            {
                               // message = saveOrEdit(isAutomatic, decContraSuffixPrefixId, DecContraVoucherTypeId, spContraMaster, radio, dgvcmbBankorCashAccount);
                            }
                        }
                        else
                        {

                            a = dgvcmbBankorCashAccount.Count();
                            bool isNegativeLedger = false;
                            for (int i = 0; i < a; i++)
                            {
                                decCalcAmount = 0;
                                decimal decledgerId = 0;
                                if (dgvcmbBankorCashAccount[i] != null && dgvcmbBankorCashAccount[i] != "")
                                {
                                    decledgerId = Convert.ToDecimal(dgvcmbBankorCashAccount[i].ToString());
                                }
                                decBalance = spAccountLedger.CheckLedgerBalance(decledgerId);
                                decCalcAmount = decBalance - Convert.ToDecimal(Request.Form["totalAmount"].ToString());
                                if (decCalcAmount < 0)
                                {
                                    isNegativeLedger = true;
                                    break;
                                }
                            }
                            if (isNegativeLedger)
                            {
                                if (strStatus == "Warn")
                                {
                                    //message = saveOrEdit(isAutomatic, decContraSuffixPrefixId, DecContraVoucherTypeId, spContraMaster, radio, dgvcmbBankorCashAccount);
                                }
                                else if (strStatus == "Block")
                                {
                                    return "Cannot continue ,due to negative balance";
                                }
                                else
                                {
                                  //  message = saveOrEdit(isAutomatic, decContraSuffixPrefixId, DecContraVoucherTypeId, spContraMaster, radio, dgvcmbBankorCashAccount);
                                }
                            }
                            else
                            {
                                //message = saveOrEdit(isAutomatic, decContraSuffixPrefixId, DecContraVoucherTypeId, spContraMaster, radio, dgvcmbBankorCashAccount);
                            }
                        }
                    }
                    else
                    {
                        message = "Please Enter Valid Date:";
                    }
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                }
                return "";
        }
        #endregion


        public void PartyBalanceAddOrEdit(int inJ, string dtpDate, decimal decPaymentVoucherTypeId, string strInvoiceNo, string txtVoucherNo, string strVoucherNo)
        {
            var isAutomatic = true;
            try
            {


                int inTableRowCount = dtblPartyBalance.Rows.Count;
                PartyBalanceInfo InfopartyBalance = new PartyBalanceInfo();
                PartyBalanceSP spPartyBalance = new PartyBalanceSP();
                InfopartyBalance.Credit = 0;
                InfopartyBalance.CreditPeriod = 0;
                InfopartyBalance.Date = DateTime.Parse(dtpDate);
                InfopartyBalance.Debit = Convert.ToDecimal(dtblPartyBalance.Rows[inJ]["Amount"].ToString());
                InfopartyBalance.ExchangeRateId = Convert.ToDecimal(dtblPartyBalance.Rows[inJ]["CurrencyId"].ToString());
                InfopartyBalance.Extra1 = string.Empty;
                InfopartyBalance.Extra2 = string.Empty;
                InfopartyBalance.ExtraDate = DateTime.Now;
                InfopartyBalance.FinancialYearId = Convert.ToDecimal(PublicVariables._decCurrentFinancialYearId.ToString());
                InfopartyBalance.LedgerId = Convert.ToDecimal(dtblPartyBalance.Rows[inJ]["LedgerId"].ToString());
                InfopartyBalance.ReferenceType = dtblPartyBalance.Rows[inJ]["ReferenceType"].ToString();
                if (dtblPartyBalance.Rows[inJ]["ReferenceType"].ToString() == "New" || dtblPartyBalance.Rows[inJ]["ReferenceType"].ToString() == "OnAccount")
                {
                    InfopartyBalance.AgainstInvoiceNo = "0";
                    InfopartyBalance.AgainstVoucherNo = "0";
                    InfopartyBalance.AgainstVoucherTypeId = 0;
                    InfopartyBalance.VoucherTypeId = decPaymentVoucherTypeId;
                    InfopartyBalance.InvoiceNo = strInvoiceNo;
                    if (!isAutomatic)
                    {
                        InfopartyBalance.VoucherNo = txtVoucherNo.Trim();
                    }
                    else
                    {
                        InfopartyBalance.VoucherNo = strVoucherNo;
                    }
                }
                else
                {
                    InfopartyBalance.ExchangeRateId = Convert.ToDecimal(dtblPartyBalance.Rows[inJ]["OldExchangeRate"].ToString());
                    InfopartyBalance.AgainstInvoiceNo = strInvoiceNo;
                    if (!isAutomatic)
                    {
                        InfopartyBalance.AgainstVoucherNo = txtVoucherNo.Trim();
                    }
                    else
                    {
                        InfopartyBalance.AgainstVoucherNo = strVoucherNo;
                    }
                    InfopartyBalance.AgainstVoucherTypeId = decPaymentVoucherTypeId;
                    InfopartyBalance.VoucherTypeId = Convert.ToDecimal(dtblPartyBalance.Rows[inJ]["AgainstVoucherTypeId"].ToString());
                    InfopartyBalance.VoucherNo = dtblPartyBalance.Rows[inJ]["AgainstVoucherNo"].ToString();
                    InfopartyBalance.InvoiceNo = dtblPartyBalance.Rows[inJ]["AgainstInvoiceNo"].ToString();
                }
                if (dtblPartyBalance.Rows[inJ]["PartyBalanceId"].ToString() == "0")
                {
                    spPartyBalance.PartyBalanceAdd(InfopartyBalance);
                }
                else
                {
                    InfopartyBalance.PartyBalanceId = Convert.ToDecimal(dtblPartyBalance.Rows[inJ]["partyBalanceId"]);
                    spPartyBalance.PartyBalanceEdit(InfopartyBalance);
                }
            }
            catch (Exception ex)
            {
                PublicVariables.infoError.ErrorString = "PV15:" + ex;
            }
        }

        public void LedgerPosting(string cmbAccountLedger, string strVoucherNo, string txtAmount, string txtVoucherNo, string txtVoucherDate, string txtChequeDate, string txtChequeNo, decimal decDebitNoteVoucherTypeId, decimal decId, decimal decCredit, decimal decDebit, decimal decDetailsId, int inA)
        {
            LedgerPostingSP spLedgerPosting = new LedgerPostingSP();
            LedgerPostingInfo infoLedgerPosting = new LedgerPostingInfo();
            ExchangeRateSP SpExchangRate = new ExchangeRateSP();
            decimal decOldExchange = 0;
            decimal decNewExchangeRate = 0;
            decimal decNewExchangeRateId = 0;
            decimal decOldExchangeId = 0;
            try
            {
                if (txtAmount != null)
                {
                    infoLedgerPosting.Date = PublicVariables._dtCurrentDate;
                    infoLedgerPosting.VoucherTypeId = decDebitNoteVoucherTypeId;
                    infoLedgerPosting.VoucherNo = strVoucherNo;
                    infoLedgerPosting.DetailsId = decDetailsId;
                    infoLedgerPosting.YearId = Convert.ToDecimal(PublicVariables._decCurrentFinancialYearId.ToString());
                    infoLedgerPosting.InvoiceNo = txtVoucherNo.Trim();

                    if (txtChequeNo != null && txtChequeNo.ToString() != string.Empty)
                    {
                        infoLedgerPosting.ChequeNo = txtChequeNo.ToString();
                        if (txtChequeDate != null && txtChequeDate.ToString() != string.Empty)
                        {
                            infoLedgerPosting.ChequeDate = Convert.ToDateTime(txtChequeDate.ToString());
                        }
                        else
                            infoLedgerPosting.ChequeDate = DateTime.Now;

                    }
                    else
                    {
                        infoLedgerPosting.ChequeNo = string.Empty;
                        infoLedgerPosting.ChequeDate = DateTime.Now;
                    }


                    infoLedgerPosting.Extra1 = string.Empty;
                    infoLedgerPosting.Extra2 = string.Empty;
                    infoLedgerPosting.LedgerId = decId;
                    infoLedgerPosting.Credit = decCredit;
                    infoLedgerPosting.Debit = decDebit;

                    spLedgerPosting.LedgerPostingAdd(infoLedgerPosting);
                }
                else
                {
                    infoLedgerPosting.Date = PublicVariables._dtCurrentDate;
                    infoLedgerPosting.VoucherTypeId = decDebitNoteVoucherTypeId;
                    infoLedgerPosting.VoucherNo = strVoucherNo;
                    infoLedgerPosting.DetailsId = decDetailsId;
                    infoLedgerPosting.YearId = Convert.ToDecimal(PublicVariables._decCurrentFinancialYearId.ToString());
                    infoLedgerPosting.InvoiceNo = txtVoucherNo.Trim();

                    if (txtChequeNo != null && txtChequeNo != string.Empty)
                    {
                        infoLedgerPosting.ChequeNo = txtChequeNo.ToString();
                        if (txtChequeDate != null && txtChequeDate.ToString() != string.Empty)
                        {
                            infoLedgerPosting.ChequeDate = Convert.ToDateTime(txtChequeDate.ToString());
                        }
                        else
                            infoLedgerPosting.ChequeDate = DateTime.Now;

                    }
                    else
                    {
                        infoLedgerPosting.ChequeNo = string.Empty;
                        infoLedgerPosting.ChequeDate = DateTime.Now;
                    }


                    infoLedgerPosting.ExtraDate = DateTime.Now;
                    infoLedgerPosting.Extra1 = string.Empty;
                    infoLedgerPosting.Extra2 = string.Empty;
                    infoLedgerPosting.LedgerId = decId;
                    decimal decConvertRate = 0;
                    foreach (DataRow dr in dtblPartyBalance.Rows)
                    {
                        if (infoLedgerPosting.LedgerId == Convert.ToDecimal(dr["LedgerId"].ToString()))
                        {
                            decOldExchange = Convert.ToDecimal(dr["OldExchangeRate"].ToString());
                            decNewExchangeRateId = Convert.ToDecimal(dr["CurrencyId"].ToString());
                            var decSelectedCurrencyRate = SpExchangRate.GetExchangeRateByExchangeRateId(decOldExchange);
                            var decAmount = Convert.ToDecimal(dr["Amount"].ToString());
                            decConvertRate = decConvertRate + (decAmount * decSelectedCurrencyRate);

                        }
                    }

                    if (decCredit == 0)
                    {
                        infoLedgerPosting.Credit = 0;
                        infoLedgerPosting.Debit = decConvertRate;
                    }
                    else
                    {
                        infoLedgerPosting.Debit = 0;
                        infoLedgerPosting.Credit = decConvertRate;
                    }


                    spLedgerPosting.LedgerPostingAdd(infoLedgerPosting);

                    infoLedgerPosting.LedgerId = 12;
                    foreach (DataRow dr in dtblPartyBalance.Rows)
                    {
                        if (Convert.ToDecimal(cmbAccountLedger.ToString()) == Convert.ToDecimal(dr["LedgerId"].ToString()))
                        {
                            if (dr["ReferenceType"].ToString() == "Against")
                            {
                                decNewExchangeRateId = Convert.ToDecimal(dr["CurrencyId"].ToString());
                                decNewExchangeRate = SpExchangRate.GetExchangeRateByExchangeRateId(decNewExchangeRateId);
                                decOldExchangeId = Convert.ToDecimal(dr["OldExchangeRate"].ToString());
                                decOldExchange = SpExchangRate.GetExchangeRateByExchangeRateId(decOldExchangeId);
                                var decAmount = Convert.ToDecimal(dr["Amount"].ToString());
                                decimal decForexAmount = (decAmount * decNewExchangeRate) - (decAmount * decOldExchange);
                                if (dr["DebitOrCredit"].ToString() == "Dr")
                                {
                                    if (decForexAmount >= 0)
                                    {

                                        infoLedgerPosting.Debit = decForexAmount;
                                        infoLedgerPosting.Credit = 0;
                                    }
                                    else
                                    {
                                        infoLedgerPosting.Credit = -1 * decForexAmount;
                                        infoLedgerPosting.Debit = 0;
                                    }
                                }
                                else
                                {
                                    if (decForexAmount >= 0)
                                    {

                                        infoLedgerPosting.Credit = decForexAmount;
                                        infoLedgerPosting.Debit = 0;
                                    }
                                    else
                                    {
                                        infoLedgerPosting.Debit = -1 * decForexAmount;
                                        infoLedgerPosting.Credit = 0;
                                    }
                                }
                                spLedgerPosting.LedgerPostingAdd(infoLedgerPosting);
                            }
                        }

                    }

                }

            }
            catch (Exception ex)
            {
                PublicVariables.infoError.ErrorString = "DRNT22:" + ex.Message;
            }
        }

        public void LedgerPostingEdit(string cmbAccountLedger, string strVoucherNo, string txtAmount, string txtVoucherNo, string txtVoucherDate, string txtChequeDate, string txtChequeNo, decimal decDebitNoteVoucherTypeId, decimal decLedgerPostingId, decimal decLedgerId, decimal decCredit, decimal decDebit, decimal decDetailsId, int inA)
        {
            LedgerPostingSP spLedgerPosting = new LedgerPostingSP();
            LedgerPostingInfo infoLedgerPosting = new LedgerPostingInfo();
            ExchangeRateSP SpExchangRate = new ExchangeRateSP();
            decimal decOldExchange = 0;
            decimal decNewExchangeRate = 0;
            decimal decNewExchangeRateId = 0;
            decimal decOldExchangeId = 0;
            try
            {

                if (txtAmount != null)
                {
                    infoLedgerPosting.LedgerPostingId = decLedgerPostingId;
                    infoLedgerPosting.Date = PublicVariables._dtCurrentDate;
                    infoLedgerPosting.VoucherTypeId = decDebitNoteVoucherTypeId;
                    infoLedgerPosting.VoucherNo = strVoucherNo;
                    infoLedgerPosting.DetailsId = decDetailsId;
                    infoLedgerPosting.YearId = Convert.ToDecimal(PublicVariables._decCurrentFinancialYearId.ToString());
                    infoLedgerPosting.InvoiceNo = txtVoucherNo.Trim();

                    if (txtChequeNo != null && txtChequeNo.ToString() != string.Empty)
                    {
                        infoLedgerPosting.ChequeNo = txtChequeNo.ToString();
                        if (txtChequeDate != null && txtChequeDate.ToString() != string.Empty)
                        {
                            infoLedgerPosting.ChequeDate = Convert.ToDateTime(txtChequeDate.ToString());
                        }
                        else
                            infoLedgerPosting.ChequeDate = DateTime.Now;

                    }
                    else
                    {
                        infoLedgerPosting.ChequeNo = string.Empty;
                        infoLedgerPosting.ChequeDate = DateTime.Now;
                    }


                    infoLedgerPosting.Extra1 = string.Empty;
                    infoLedgerPosting.Extra2 = string.Empty;
                    infoLedgerPosting.LedgerId = decLedgerId;
                    infoLedgerPosting.Credit = decCredit;
                    infoLedgerPosting.Debit = decDebit;

                    spLedgerPosting.LedgerPostingEdit(infoLedgerPosting);
                }
                else
                {
                    infoLedgerPosting.LedgerPostingId = decLedgerPostingId;
                    infoLedgerPosting.Date = PublicVariables._dtCurrentDate;
                    infoLedgerPosting.VoucherTypeId = decDebitNoteVoucherTypeId;
                    infoLedgerPosting.VoucherNo = strVoucherNo;
                    infoLedgerPosting.DetailsId = decDetailsId;
                    infoLedgerPosting.YearId = Convert.ToDecimal(PublicVariables._decCurrentFinancialYearId.ToString());
                    infoLedgerPosting.InvoiceNo = txtVoucherNo.Trim();

                    if (txtChequeNo != null && txtChequeNo.ToString() != string.Empty)
                    {
                        infoLedgerPosting.ChequeNo = txtChequeNo.ToString();
                        if (txtChequeDate != null && txtChequeDate.ToString() != string.Empty)
                        {
                            infoLedgerPosting.ChequeDate = Convert.ToDateTime(txtChequeDate.ToString());
                        }
                        else
                            infoLedgerPosting.ChequeDate = DateTime.Now;

                    }
                    else
                    {
                        infoLedgerPosting.ChequeNo = string.Empty;
                        infoLedgerPosting.ChequeDate = DateTime.Now;
                    }


                    infoLedgerPosting.ExtraDate = DateTime.Now;
                    infoLedgerPosting.Extra1 = string.Empty;
                    infoLedgerPosting.Extra2 = string.Empty;
                    infoLedgerPosting.LedgerId = decLedgerId;
                    decimal decConvertRate = 0;

                    foreach (DataRow dr in dtblPartyBalance.Rows)
                    {
                        if (infoLedgerPosting.LedgerId == Convert.ToDecimal(dr["LedgerId"].ToString()))
                        {
                            decOldExchange = Convert.ToDecimal(dr["OldExchangeRate"].ToString());
                            decNewExchangeRateId = Convert.ToDecimal(dr["CurrencyId"].ToString());
                            var decSelectedCurrencyRate = SpExchangRate.GetExchangeRateByExchangeRateId(decOldExchange);
                            var decAmount = Convert.ToDecimal(dr["Amount"].ToString());
                            decConvertRate = decConvertRate + (decAmount * decSelectedCurrencyRate);

                        }
                    }

                    if (decCredit == 0)
                    {
                        infoLedgerPosting.Credit = 0;
                        infoLedgerPosting.Debit = decConvertRate;
                    }
                    else
                    {
                        infoLedgerPosting.Debit = 0;
                        infoLedgerPosting.Credit = decConvertRate;
                    }


                    spLedgerPosting.LedgerPostingEdit(infoLedgerPosting);

                    infoLedgerPosting.LedgerId = 12;
                    foreach (DataRow dr in dtblPartyBalance.Rows)
                    {
                        if (Convert.ToDecimal(cmbAccountLedger.ToString()) == Convert.ToDecimal(dr["LedgerId"].ToString()))
                        {
                            if (dr["ReferenceType"].ToString() == "Against")
                            {
                                decNewExchangeRateId = Convert.ToDecimal(dr["CurrencyId"].ToString());
                                decNewExchangeRate = SpExchangRate.GetExchangeRateByExchangeRateId(decNewExchangeRateId);
                                decOldExchangeId = Convert.ToDecimal(dr["OldExchangeRate"].ToString());
                                decOldExchange = SpExchangRate.GetExchangeRateByExchangeRateId(decOldExchangeId);
                                var decAmount = Convert.ToDecimal(dr["Amount"].ToString());
                                decimal decForexAmount = (decAmount * decNewExchangeRate) - (decAmount * decOldExchange);
                                if (dr["DebitOrCredit"].ToString() == "Dr")
                                {
                                    if (decForexAmount >= 0)
                                    {

                                        infoLedgerPosting.Debit = decForexAmount;
                                        infoLedgerPosting.Credit = 0;
                                    }
                                    else
                                    {
                                        infoLedgerPosting.Credit = -1 * decForexAmount;
                                        infoLedgerPosting.Debit = 0;
                                    }
                                }
                                else
                                {
                                    if (decForexAmount >= 0)
                                    {

                                        infoLedgerPosting.Credit = decForexAmount;
                                        infoLedgerPosting.Debit = 0;
                                    }
                                    else
                                    {
                                        infoLedgerPosting.Debit = -1 * decForexAmount;
                                        infoLedgerPosting.Credit = 0;
                                    }
                                }
                                spLedgerPosting.LedgerPostingAdd(infoLedgerPosting);
                            }
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                PublicVariables.infoError.ErrorString = "DRNT23:" + ex.Message;
            }

        }

    }

}