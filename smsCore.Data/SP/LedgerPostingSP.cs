//This is a source code or part of Models project
//Copyright (C) 2018  Models
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using smsCore.Data.Models;
using smsCore.Data;
using Microsoft.EntityFrameworkCore;

//<summary>    
//Summary description for LedgerPostingSP    
//</summary>    
namespace Models
{
    public class LedgerPostingSP : DBConnection
    {
        private readonly SchoolEntities db;// = new SchoolEntities();
        private readonly CurrentUser CurrentUser;
        public LedgerPostingSP(SchoolEntities _db,CurrentUser _currentUser)
        {
            db = _db;
            CurrentUser= _currentUser;
        }

        #region Function
        /// <summary>
        /// Function to insert values to LedgerPosting Table
        /// </summary>
        /// <param name="ledgerpostinginfo"></param>
        private async Task<int> LedgerPostingAdd(LedgerPostingInfo ledgerpostinginfo)
        {
            try
            {
                tbl_LedgerPosting ledger = new tbl_LedgerPosting();
                ledger.EntryDate = DateTime.Now;
                ledger.CampusId = ledgerpostinginfo.CampusId;
                ledger.chequeDate = ledgerpostinginfo.ChequeDate;
                ledger.chequeNo = ledgerpostinginfo.ChequeNo;
                ledger.credit = ledgerpostinginfo.Credit;
                ledger.debit = ledgerpostinginfo.Debit;
                ledger.detailsId = ledgerpostinginfo.DetailsId;
                ledger.invoiceNo = ledgerpostinginfo.InvoiceNo;
                ledger.ledgerId = ledgerpostinginfo.LedgerId;
                ledger.voucherNo = ledgerpostinginfo.VoucherNo;
                ledger.voucherTypeId = ledgerpostinginfo.VoucherTypeId;
                ledger.yearId = ledgerpostinginfo.YearId;
                ledger.CampusId = CurrentUser.SelectedCampusId;
                ledger.UserId = CurrentUser.UserID;
                ledger.yearId = ledgerpostinginfo.YearId;
                ledger.ModifiedOn = DateTime.Now;
                ledger.ModifiedBy = CurrentUser.UserID;

                db.LedgerPostings.Add(ledger);
                await db.SaveChangesAsync();
                return ledger.Id;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

            }
        }

        /// <summary>
        /// Function to Update values in LedgerPosting Table
        /// </summary>
        /// <param name="ledgerpostinginfo"></param>
        private async Task<int> LedgerPostingEdit(LedgerPostingInfo ledgerpostinginfo)
        {
            try
            {
                tbl_LedgerPosting ledger = db.LedgerPostings.Find(ledgerpostinginfo.LedgerPostingId);
                if (ledger == null)
                    throw new NullReferenceException();

                ledger.CampusId = CurrentUser.SelectedCampusId;
                ledger.chequeDate = ledgerpostinginfo.ChequeDate;
                ledger.chequeNo = ledgerpostinginfo.ChequeNo;
                ledger.credit = ledgerpostinginfo.Credit;
                ledger.debit = ledgerpostinginfo.Debit;
                ledger.detailsId = ledgerpostinginfo.DetailsId;
                ledger.invoiceNo = ledgerpostinginfo.InvoiceNo;
                ledger.ledgerId = ledgerpostinginfo.LedgerId;
                ledger.voucherNo = ledgerpostinginfo.VoucherNo;
                ledger.voucherTypeId = ledgerpostinginfo.VoucherTypeId;
                ledger.yearId = ledgerpostinginfo.YearId;
                ledger.ModifiedBy = CurrentUser.UserID;
                ledger.ModifiedOn = DateTime.Now;

                await db.SaveChangesAsync();
                return ledger.Id;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

            }
        }

        public async Task<List<tbl_LedgerPosting>> Find(int detailid, int voucherType, int ledgerid)
        {
            var exist =await db.LedgerPostings.Where(w =>w.detailsId==detailid && w.voucherTypeId==voucherType && w.ledgerId==ledgerid).ToListAsync();
            return exist.ToList();
        }

        public async Task<int> AddOrUpdate(LedgerPostingInfo info)
        {
            var exist = await this.Find(info.DetailsId, info.VoucherTypeId, info.LedgerId);
            if (exist == null || exist.Count==0)
            {
              return await  LedgerPostingAdd(info);
            }
            else
            {
                return await LedgerPostingEdit(info);
            }
        }

        public async Task DeleteLedgerPosting(int detailid, int VoucherTypeId,int ledgerId)
        {
            var exist =await Find(detailid, VoucherTypeId, ledgerId);
            if(exist!=null && exist.Count > 0)
            {
                db.LedgerPostings.RemoveRange(exist);
                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Ledger posting Save function
        /// </summary>
        public async void FeeLedgerPostingF(decimal Amount, int FeeSlipId, DateTime currentDate, int VoucherTypeId)
        {
            try
            {
                LedgerPostingInfo infoLedgerPosting = new LedgerPostingInfo();
                infoLedgerPosting.ExtraDate = currentDate;
                infoLedgerPosting.Extra1 = string.Empty;
                infoLedgerPosting.Extra2 = string.Empty;
                infoLedgerPosting.VoucherTypeId = VoucherTypeId;
                infoLedgerPosting.VoucherNo = FeeSlipId.ToString();
                infoLedgerPosting.Date = currentDate;
                infoLedgerPosting.DetailsId = FeeSlipId;
                infoLedgerPosting.LedgerId = (int)PublicVariables.EnumLedgerDefaults.School_Fee_Revenue;// Convert.ToDecimal(cmbAccountLedger.SelectedValue.ToString());
                infoLedgerPosting.YearId = PublicVariables._decCurrentFinancialYearId;
                infoLedgerPosting.ChequeDate = currentDate;
                infoLedgerPosting.InvoiceNo = FeeSlipId.ToString();
                infoLedgerPosting.ChequeNo = string.Empty;
                infoLedgerPosting.Debit = 0;
                infoLedgerPosting.Credit = Amount;
                await AddOrUpdate(infoLedgerPosting);

                infoLedgerPosting = new LedgerPostingInfo();
                infoLedgerPosting.ExtraDate = currentDate;
                infoLedgerPosting.Extra1 = string.Empty;
                infoLedgerPosting.Extra2 = string.Empty;
                infoLedgerPosting.VoucherTypeId = VoucherTypeId;
                infoLedgerPosting.Date = currentDate;
                infoLedgerPosting.DetailsId = FeeSlipId;
                infoLedgerPosting.VoucherNo = FeeSlipId.ToString();
                infoLedgerPosting.InvoiceNo = FeeSlipId.ToString();
                infoLedgerPosting.LedgerId = (int)PublicVariables.EnumLedgerDefaults.School_Fee_Receiveable;// Convert.ToDecimal(cmbAccountLedger.SelectedValue.ToString());
                infoLedgerPosting.YearId = PublicVariables._decCurrentFinancialYearId;
                infoLedgerPosting.ChequeDate = currentDate;
                infoLedgerPosting.ChequeNo = string.Empty;

                infoLedgerPosting.Debit = Amount;
                infoLedgerPosting.Credit = 0;
                await AddOrUpdate(infoLedgerPosting);


            }
            catch (Exception ex)
            {
                //PublicVariables.infoError.ErrorString = "PR11:" + ex.Message;
            }
        }
        /// <summary>
        /// Ledger posting Save function
        /// </summary>
        public async Task ReceiveFeeLedgerPostingF(decimal Amount, int FeeSliptId, DateTime currentDate, int VoucherTypeId)
        {
            LedgerPostingInfo infoLedgerPosting = new LedgerPostingInfo();

            try
            {
                infoLedgerPosting.ExtraDate = currentDate;
                infoLedgerPosting.Extra1 = string.Empty;
                infoLedgerPosting.Extra2 = string.Empty;
                infoLedgerPosting.VoucherTypeId = VoucherTypeId;
                infoLedgerPosting.VoucherNo = FeeSliptId.ToString();
                infoLedgerPosting.Date = currentDate;
                infoLedgerPosting.DetailsId = FeeSliptId;
                infoLedgerPosting.LedgerId = (int)PublicVariables.EnumLedgerDefaults.School_Fee_Receiveable;// Convert.ToDecimal(cmbAccountLedger.SelectedValue.ToString());
                infoLedgerPosting.YearId = PublicVariables._decCurrentFinancialYearId;
                infoLedgerPosting.ChequeDate = currentDate;
                infoLedgerPosting.InvoiceNo = FeeSliptId.ToString();
                infoLedgerPosting.ChequeNo = string.Empty;



                infoLedgerPosting.Debit = 0;
                infoLedgerPosting.Credit = Amount;

                await AddOrUpdate(infoLedgerPosting);
                infoLedgerPosting = new LedgerPostingInfo();
                infoLedgerPosting.ExtraDate = currentDate;
                infoLedgerPosting.Extra1 = string.Empty;
                infoLedgerPosting.Extra2 = string.Empty;
                infoLedgerPosting.VoucherTypeId = VoucherTypeId;
                infoLedgerPosting.Date = currentDate;
                infoLedgerPosting.DetailsId = FeeSliptId;
                infoLedgerPosting.VoucherNo = FeeSliptId.ToString();
                infoLedgerPosting.InvoiceNo = FeeSliptId.ToString();
                infoLedgerPosting.LedgerId = (int)PublicVariables.EnumLedgerDefaults.Cash;// Convert.ToDecimal(cmbAccountLedger.SelectedValue.ToString());
                infoLedgerPosting.YearId = PublicVariables._decCurrentFinancialYearId;
                infoLedgerPosting.ChequeDate = currentDate;
                infoLedgerPosting.ChequeNo = string.Empty;

                infoLedgerPosting.Debit = Amount;
                infoLedgerPosting.Credit = 0;
                await AddOrUpdate(infoLedgerPosting);
            }
            catch (Exception ex)
            {
                //PublicVariables.infoError.ErrorString = "PR11:" + ex.Message;
            }
        }


        public async Task LeddgerPosting(decimal Amount, int detailid,int ledgerid,bool cr, DateTime currentDate, int VoucherTypeId,DateTime checkdate,  string voucherNo="", string invoiceNo="", string checkno="", string extra1="", string extra2="")
        {
            try
            {
                LedgerPostingInfo infoLedgerPosting = new LedgerPostingInfo();
                infoLedgerPosting.ExtraDate = currentDate;
                infoLedgerPosting.Extra1 = extra1;
                infoLedgerPosting.Extra2 = extra2;
                infoLedgerPosting.VoucherTypeId = VoucherTypeId;
                infoLedgerPosting.VoucherNo = voucherNo;
                infoLedgerPosting.Date = currentDate;
                infoLedgerPosting.DetailsId = detailid;
                infoLedgerPosting.LedgerId = ledgerid;// Convert.ToDecimal(cmbAccountLedger.SelectedValue.ToString());
                infoLedgerPosting.YearId = PublicVariables._decCurrentFinancialYearId;
                infoLedgerPosting.ChequeDate = currentDate;
                infoLedgerPosting.InvoiceNo = invoiceNo;
                infoLedgerPosting.ChequeNo = checkno;
                infoLedgerPosting.ChequeDate = checkdate;
                infoLedgerPosting.Debit = 0;
                infoLedgerPosting.Credit = 0;
                if (cr)
                    infoLedgerPosting.Credit = Amount;
                else infoLedgerPosting.Debit = Amount;
                await AddOrUpdate(infoLedgerPosting);

            }
            catch (Exception ex)
            {
                //PublicVariables.infoError.ErrorString = "PR11:" + ex.Message;
            }
        }
        public async Task LeddgerPosting(decimal Debit,decimal Credit, int detailid,int ledgerid,DateTime currentDate, int VoucherTypeId,DateTime checkdate,  string voucherNo="", string invoiceNo="", string checkno="", string extra1="", string extra2="")
        {
            try
            {
                LedgerPostingInfo infoLedgerPosting = new LedgerPostingInfo();
                infoLedgerPosting.ExtraDate = currentDate;
                infoLedgerPosting.Extra1 = extra1;
                infoLedgerPosting.Extra2 = extra2;
                infoLedgerPosting.VoucherTypeId = VoucherTypeId;
                infoLedgerPosting.VoucherNo = voucherNo;
                infoLedgerPosting.Date = currentDate;
                infoLedgerPosting.DetailsId = detailid;
                infoLedgerPosting.LedgerId = ledgerid;// Convert.ToDecimal(cmbAccountLedger.SelectedValue.ToString());
                infoLedgerPosting.YearId = PublicVariables._decCurrentFinancialYearId;
                infoLedgerPosting.ChequeDate = currentDate;
                infoLedgerPosting.InvoiceNo = invoiceNo;
                infoLedgerPosting.ChequeNo = checkno;
                infoLedgerPosting.ChequeDate = checkdate;
                infoLedgerPosting.Debit = 0;
                infoLedgerPosting.Credit = 0;
                infoLedgerPosting.Credit = Credit;
                infoLedgerPosting.Debit = Debit;
                await AddOrUpdate(infoLedgerPosting);

            }
            catch (Exception ex)
            {
                //PublicVariables.infoError.ErrorString = "PR11:" + ex.Message;
            }
        }

        #endregion
    }
}
