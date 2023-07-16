using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using smsCore.Data;
using smsCore.Data.Helpers;
using smsCore.Data.Models;
using Syncfusion.EJ2.Base;

namespace smsCore.Controllers
{
    [Authorize]
    public class AccountsController : BaseController
    {
        int PaymentVoucherTypeId = (int)PublicVariables.EnumVoucherTypeDefaults.Payment_Voucher;
        int ExpenseVoucherTypeId = (int)PublicVariables.EnumVoucherTypeDefaults.Payment_Voucher;
        int ReceiptVoucherTypeId = (int)PublicVariables.EnumVoucherTypeDefaults.Receipt_Voucher;
        int JournalVoucherTypeId = (int)PublicVariables.EnumVoucherTypeDefaults.Journal_Voucher;


        int cashid = (int)PublicVariables.EnumAccountGroup.Cash_in_Hand;
        int bankid = (int)PublicVariables.EnumAccountGroup.Bank_Account;
        private SchoolEntities db ;
        private readonly StaticResources StaticResources;
        private readonly CurrentUser CurrentUser;
        private readonly LedgerPostingSP sp;
        public AccountsController(SchoolEntities _db, StaticResources _StaticResources, CurrentUser _CurrentUser, LedgerPostingSP _sp)
        {
            StaticResources = _StaticResources;
            db = _db;
            CurrentUser = _CurrentUser;
            sp = _sp;
        }
    
        #region Methods

        public  async Task<SelectList> GetBankOrCashLedgerList(bool search = false)
        {
            var Ledgers = await db.AccountLedgers.AsNoTracking().Where(w => w.AccountGroupId == cashid || w.AccountGroupId == bankid).Select(s => new
            {
                LedgerId = s.Id,
                LedgerName = s.LedgerName
            }).ToListAsync();
            if (search)
                Ledgers.Insert(0, new
                {
                    LedgerId = -1,
                    LedgerName = "All"
                });
            return new SelectList(Ledgers, "LedgerId", "LedgerName");
        }
        public async Task<SelectList> GetLedgerList(bool search = false)
        {

            var Ledgers = await db.AccountLedgers.AsNoTracking().Select(s => new
            {
                LedgerId = s.Id,
                LedgerName = s.LedgerName
            }).ToListAsync();
            if (search)
                Ledgers.Insert(0, new
                {
                    LedgerId = -1,
                    LedgerName = "All"
                });
            return new SelectList(Ledgers, "LedgerId", "LedgerName");
        }
        #endregion
        #region Payments
        public async Task<IActionResult> ExpenseRegister()
        {
            ViewBag.LedgerLists = await GetBankOrCashLedgerList(true);
            return View();
        }
        public async Task<ActionResult> ExpenseVoucher(int id = 0)
        {

            ViewBag.LedgerLists = await GetLedgerList();
            ViewBag.BankOrCash = await GetBankOrCashLedgerList();
            var model = await db.ExpenseMasters.Where(w => w.Id == id).Select(s => new PaymentMasterInfo
            {
                Date = s.VocherDate,
                LedgerId = s.LedgerId,
                Narration = s.Narration,
                InvoiceNo = s.invoiceNo,
                PaymentMasterId = s.Id,
                VoucherNo = s.voucherNo,
                VoucherTypeId = s.VoucherTypeId,
                ChequeDate = s.ChequeDate,
                ChequeNo = s.ChequeNo,
                PaymentDetails = s.ExpenseDetails.Select(t => new PaymentDetailsInfo { Amount = t.Amount, LedgerId = t.LedgerId, PaymentDetailsId = t.Id, PaymentMasterId = s.Id })
            }).FirstOrDefaultAsync();
            return View(model);
        }
        [HttpPost]
        public async Task<JsonResult> ExpenseVoucher(PaymentMasterInfo model)
        {
            try
            {
                bool isNew = false;
                var exist = await db.ExpenseMasters.FirstOrDefaultAsync(w => w.Id == model.PaymentMasterId);
                if (exist == null)
                {
                    isNew = true;
                    exist = new tbl_ExpenseMaster() { VoucherTypeId = ExpenseVoucherTypeId, EntryDate = DateTime.Now, UserId = CurrentUser.UserID };
                    db.ExpenseMasters.Add(exist);
                }
                if (!string.IsNullOrEmpty(model.Extra2))
                    exist.ChequeDate = DateTimeHelper.ConvertDate(model.Extra2);
                else exist.ChequeDate = DateTime.Now.Date;
                exist.VocherDate = DateTimeHelper.ConvertDate(model.Extra1);
                exist.ChequeNo = model.ChequeNo;
                //exist.EntryDate = DateTimeHelper.ConvertDate(model.Extra1);
                exist.invoiceNo = model.InvoiceNo;
                exist.LedgerId = model.LedgerId;
                exist.ModifiedBy = CurrentUser.UserID;
                exist.ModifiedOn = DateTime.Now;
                exist.Narration = model.Narration;
                exist.voucherNo = model.VoucherNo;
                exist.CampusId = CurrentUser.SelectedCampusId;


                foreach (var p in model.PaymentDetails)
                {
                    var detail = await db.ExpenseDetails.FirstOrDefaultAsync(w => w.Id == p.PaymentDetailsId);
                    if (detail == null)
                    {
                        detail = new tbl_ExpenseDetails();
                        if (isNew)
                        {
                            exist.ExpenseDetails.Add(detail);
                        }
                        else
                        {
                            db.ExpenseDetails.Add(detail);
                        }

                    }
                    detail.LedgerId = p.LedgerId;
                    detail.Amount = p.Amount;
                    if (!isNew)
                        detail.ExpenseMasterId = exist.Id;


                }
                await db.SaveChangesAsync();

                var master = await db.ExpenseMasters.FirstOrDefaultAsync(w => w.Id == exist.Id);
                foreach (var m in master.ExpenseDetails)
                {
                    await sp.LeddgerPosting(m.Amount, m.Id, m.LedgerId, false, master.EntryDate, ExpenseVoucherTypeId, master.ChequeDate, voucherNo: master.voucherNo, invoiceNo: master.invoiceNo, checkno: master.ChequeNo);
                    await sp.LeddgerPosting(m.Amount, m.Id, master.LedgerId, true, master.EntryDate, ExpenseVoucherTypeId, master.ChequeDate, voucherNo: master.voucherNo, invoiceNo: master.invoiceNo, checkno: master.ChequeNo);
                }

                return StaticResources.GetResult(true, "Info! Voucher Saved successfully.");
            }
            catch (Exception ex)
            {
                return StaticResources.GetResult(false, ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }
        }
        public async Task<JsonResult> ExpenseVoucherList(DataManagerRequest dm, string fromdate, string todate, int LedgerId, string VoucherNo)
        {
            int campusid = CurrentUser.SelectedCampusId;
            var data = db.ExpenseMasters.Where(w=>w.CampusId ==campusid ).Select(s => new
            {
                s.Id,
                s.VocherDate,
                s.voucherNo,
                s.Ledger.LedgerName,
                s.LedgerId,
                Amount = s.ExpenseDetails.Select(t => t.Amount).DefaultIfEmpty(0).Sum()

            }).Where(w => w.Id > 0);
            var date1 = DateTimeHelper.ConvertDate(fromdate);
            var date2 = DateTimeHelper.ConvertDate(todate);
            if (date1 != DateTime.MinValue)
            {
                data = data.Where(w => w.VocherDate >= date1);
            }
            if (date2 != DateTime.MinValue)
            {
                data = data.Where(w => w.VocherDate <= date2);
            }
            if (!string.IsNullOrEmpty(VoucherNo))
            {
                data = data.Where(w => w.voucherNo.Contains(VoucherNo));
            }
            if (LedgerId != -1)
            {
                data = data.Where(w => w.LedgerId == LedgerId);
            }
            DataOperations operation = new DataOperations();

            if (dm.Search != null && dm.Search.Count > 0)
            {
                data = operation.PerformSearching(data, dm.Search);  //Search
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                data = operation.PerformFiltering(data, dm.Where, dm.Where[0].Operator);
            }
            int count = data.Count();
            if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
            {
                data = operation.PerformSorting(data, dm.Sorted);
            }
            if (dm.Skip != 0)
            {
                if (dm.Sorted == null || dm.Sorted.Count == 0)
                {
                    List<Sort> srt = new List<Sort>() { };
                    srt.Add(new Sort { Name = "VocherDate", Direction = "ascending" });
                    data = operation.PerformSorting(data, srt);
                }

                data = operation.PerformSkip(data, dm.Skip);   //Paging
            }
            if (dm.Take != 0)
            {
                data = operation.PerformTake(data, dm.Take);
            }

            if (dm.RequiresCounts)
            {
                return Json(new { result = await data.ToListAsync(), count = count });
            }
            else
            {
                return Json(await data.ToListAsync());
            }
        }
        public async Task<JsonResult> ExpenseVoucherDetails(int id)
        {
            try
            {
                var data = await db.ExpenseDetails.AsNoTracking().Where(w => w.ExpenseMasterId == id).Select(s => new { s.Id, s.LedgerId, s.Amount }).ToListAsync();

                return Json(new { status = true, data = data });
            }
            catch (Exception ex)
            {
                return StaticResources.GetResult(false, ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }
        }
        public async Task<JsonResult> DeleteExpenseVoucher(int id)
        {
            try
            {
                var exist = await db.ExpenseMasters.FirstOrDefaultAsync(w => w.Id == id);
                if (exist != null)
                {
                    var toremoveids = exist.ExpenseDetails.Select(s => new { s.Id, s.LedgerId }).ToList();
                    db.ExpenseMasters.Remove(exist);
                    await db.SaveChangesAsync();
                    foreach (var s in toremoveids)
                        await sp.DeleteLedgerPosting(s.Id, ExpenseVoucherTypeId, s.LedgerId);
                }
                return StaticResources.GetResult(true, "success");
            }
            catch { }
            return StaticResources.GetResult(false, "failed");
        }
        public async Task<JsonResult> DeleteExpenseVoucherDetails(int id)
        {
            try
            {
                var exist = await db.ExpenseDetails.FirstOrDefaultAsync(w => w.Id == id);
                if (exist != null)
                {
                    db.ExpenseDetails.Remove(exist);
                    await db.SaveChangesAsync();

                    await sp.DeleteLedgerPosting(exist.Id, ExpenseVoucherTypeId, exist.LedgerId);
                }
                return StaticResources.GetResult(true, "success");
            }
            catch { }
            return StaticResources.GetResult(false, "failed");
        }

        #endregion


        #region Payments
        public async Task<ActionResult> PaymentRegister()
        {
            ViewBag.LedgerLists = await GetBankOrCashLedgerList(true);
            return View();
        }
        public async Task<ActionResult> PaymentVoucher(int id = 0)
        {

            ViewBag.LedgerLists = await GetLedgerList();
            ViewBag.BankOrCash = await GetBankOrCashLedgerList();
            var model = await db.PaymentMasters.Where(w => w.Id == id).Select(s => new PaymentMasterInfo
            {
                Date = s.VocherDate,
                LedgerId = s.LedgerId,
                Narration = s.Narration,
                InvoiceNo = s.invoiceNo,
                PaymentMasterId = s.Id,
                VoucherNo = s.voucherNo,
                VoucherTypeId = s.VoucherTypeId,
                ChequeDate = s.ChequeDate,
                ChequeNo = s.ChequeNo,
                PaymentDetails = s.PaymentDetails.Select(t => new PaymentDetailsInfo { Amount = t.Amount, LedgerId = t.LedgerId, PaymentDetailsId = t.Id, PaymentMasterId = s.Id })
            }).FirstOrDefaultAsync();
            return View(model);
        }
        [HttpPost]
        public async Task<JsonResult> PaymentVoucher(PaymentMasterInfo model)
        {
            try
            {
                bool isNew = false;
                var exist = await db.PaymentMasters.FirstOrDefaultAsync(w => w.Id == model.PaymentMasterId);
                if (exist == null)
                {
                    isNew = true;
                    exist = new tbl_PaymentMaster() { VoucherTypeId = PaymentVoucherTypeId, EntryDate = DateTime.Now, UserId = CurrentUser.UserID };
                    db.PaymentMasters.Add(exist);
                }

                if (!string.IsNullOrEmpty(model.Extra2))
                    exist.ChequeDate = DateTimeHelper.ConvertDate(model.Extra2);
                else 
                    exist.ChequeDate = DateTime.Now.Date;

                exist.VocherDate = DateTimeHelper.ConvertDate(model.Extra1);
                exist.ChequeNo = model.ChequeNo;
                //exist.EntryDate = DateTimeHelper.ConvertDate(model.Extra1);
                exist.invoiceNo = model.InvoiceNo;
                exist.LedgerId = model.LedgerId;
                exist.ModifiedBy = CurrentUser.UserID;
                exist.ModifiedOn = DateTime.Now;
                exist.Narration = model.Narration;
                exist.voucherNo = model.VoucherNo;
                exist.CampusId = CurrentUser.SelectedCampusId;

                foreach (var p in model.PaymentDetails)
                {
                    var detail = await db.PaymentDetails.FirstOrDefaultAsync(w => w.Id == p.PaymentDetailsId);
                    if (detail == null)
                    {
                        detail = new tbl_PaymentDetails();
                        if (isNew)
                        {
                            exist.PaymentDetails.Add(detail);
                        }
                        else
                        {
                            db.PaymentDetails.Add(detail);
                        }
                    }
                    detail.LedgerId = p.LedgerId;
                    detail.Amount = p.Amount;
                    if (!isNew)
                        detail.PaymentMasterId = exist.Id;
                }
                await db.SaveChangesAsync();

                var master = await db.PaymentMasters.FirstOrDefaultAsync(w => w.Id == exist.Id);
                foreach (var m in master.PaymentDetails)
                {
                    await sp.LeddgerPosting(m.Amount, m.Id, m.LedgerId, false, master.EntryDate, PaymentVoucherTypeId, master.ChequeDate, voucherNo: master.voucherNo, invoiceNo: master.invoiceNo, checkno: master.ChequeNo);
                    await sp.LeddgerPosting(m.Amount, m.Id, master.LedgerId, true, master.EntryDate, PaymentVoucherTypeId, master.ChequeDate, voucherNo: master.voucherNo, invoiceNo: master.invoiceNo, checkno: master.ChequeNo);
                }

                return StaticResources.GetResult(true, "Info! Voucher Saved successfully.");
            }
            catch (Exception ex)
            {
                return StaticResources.GetResult(false, ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }
        }
        public async Task<JsonResult> PaymentVoucherList(DataManagerRequest dm, string fromdate, string todate, int LedgerId, string VoucherNo)
        {
            int campusid = CurrentUser.SelectedCampusId;
            var data = db.PaymentMasters.Where(w => w.CampusId == campusid).Select(s => new
            {
                s.Id,
                s.VocherDate,
                s.voucherNo,
                s.Ledger.LedgerName,
                s.LedgerId,
                Amount = s.PaymentDetails.Select(t => t.Amount).DefaultIfEmpty(0).Sum()

            }).Where(w => w.Id > 0);
            var date1 = DateTimeHelper.ConvertDate(fromdate);
            var date2 = DateTimeHelper.ConvertDate(todate);
            if (date1 != DateTime.MinValue)
            {
                data = data.Where(w => w.VocherDate >= date1);
            }
            if (date2 != DateTime.MinValue)
            {
                data = data.Where(w => w.VocherDate <= date2);
            }
            if (!string.IsNullOrEmpty(VoucherNo))
            {
                data = data.Where(w => w.voucherNo.Contains(VoucherNo));
            }
            if (LedgerId != -1)
            {
                data = data.Where(w => w.LedgerId == LedgerId);
            }
            DataOperations operation = new DataOperations();

            if (dm.Search != null && dm.Search.Count > 0)
            {
                data = operation.PerformSearching(data, dm.Search);  //Search
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                data = operation.PerformFiltering(data, dm.Where, dm.Where[0].Operator);
            }
            int count = data.Count();
            if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
            {
                data = operation.PerformSorting(data, dm.Sorted);
            }
            if (dm.Skip != 0)
            {
                if (dm.Sorted == null || dm.Sorted.Count == 0)
                {
                    List<Sort> srt = new List<Sort>() { };
                    srt.Add(new Sort { Name = "VocherDate", Direction = "ascending" });
                    data = operation.PerformSorting(data, srt);
                }

                data = operation.PerformSkip(data, dm.Skip);   //Paging
            }
            if (dm.Take != 0)
            {
                data = operation.PerformTake(data, dm.Take);
            }

            if (dm.RequiresCounts)
            {
                return Json(new { result = await data.ToListAsync(), count = count });
            }
            else
            {
                return Json(await data.ToListAsync());
            }
        }
        public async Task<JsonResult> PaymentVoucherDetails(int id)
        {
            try
            {
                var data = await db.PaymentDetails.AsNoTracking().Where(w => w.PaymentMasterId == id).Select(s => new { s.Id, s.LedgerId, s.Amount }).ToListAsync();

                return Json(new { status = true, data = data });
            }
            catch (Exception ex)
            {
                return StaticResources.GetResult(false, ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }
        }
        public async Task<JsonResult> DeletePaymentVoucher(int id)
        {
            try
            {
                var exist = await db.PaymentMasters.FirstOrDefaultAsync(w => w.Id == id);
                if (exist != null)
                {
                    var toremoveids = exist.PaymentDetails.Select(s => new { s.Id, s.LedgerId }).ToList();
                    db.PaymentMasters.Remove(exist);
                    await db.SaveChangesAsync();
                    foreach (var s in toremoveids)
                        await sp.DeleteLedgerPosting(s.Id, PaymentVoucherTypeId, s.LedgerId);
                }
                return StaticResources.GetResult(true, "success");
            }
            catch { }
            return StaticResources.GetResult(false, "failed");
        }
        public async Task<JsonResult> DeletePaymentVoucherDetails(int id)
        {
            try
            {
                var exist = await db.PaymentDetails.FirstOrDefaultAsync(w => w.Id == id);
                if (exist != null)
                {
                    db.PaymentDetails.Remove(exist);
                    await db.SaveChangesAsync();
                    await sp.DeleteLedgerPosting(exist.Id, ExpenseVoucherTypeId, exist.LedgerId);
                }
                return StaticResources.GetResult(true, "success");
            }
            catch { }
            return StaticResources.GetResult(false, "failed");
        }

        #endregion

        #region Receipt
        public async Task<ActionResult> ReceiptRegister()
        {
            ViewBag.LedgerLists = await GetBankOrCashLedgerList(true);
            return View();
        }
        public async Task<ActionResult> ReceiptVoucher(int id = 0)
        {

            ViewBag.LedgerLists = await GetLedgerList();
            ViewBag.BankOrCash = await GetBankOrCashLedgerList();
            var model = await db.ReceiptMasters.Where(w => w.Id == id).Select(s => new PaymentMasterInfo
            {
                Date = s.VocherDate,
                LedgerId = s.LedgerId,
                Narration = s.Narration,
                InvoiceNo = s.invoiceNo,
                PaymentMasterId = s.Id,
                VoucherNo = s.voucherNo,
                VoucherTypeId = s.VoucherTypeId,
                ChequeDate = s.ChequeDate,
                ChequeNo = s.ChequeNo,
                PaymentDetails = s.ReceiptDetails.Select(t => new PaymentDetailsInfo { Amount = t.Amount, LedgerId = t.LedgerId, PaymentDetailsId = t.Id, PaymentMasterId = s.Id })
            }).FirstOrDefaultAsync();
            return View(model);
        }
        [HttpPost]
        public async Task<JsonResult> ReceiptVoucher(PaymentMasterInfo model)
        {
            try
            {
                bool isNew = false;
                var exist = await db.ReceiptMasters.FirstOrDefaultAsync(w => w.Id == model.PaymentMasterId);
                if (exist == null)
                {
                    isNew = true;
                    exist = new tbl_ReceiptMaster() { VoucherTypeId = ReceiptVoucherTypeId, EntryDate = DateTime.Now, UserId = CurrentUser.UserID };
                    db.ReceiptMasters.Add(exist);
                }
                if (!string.IsNullOrEmpty(model.Extra2))
                    exist.ChequeDate = DateTimeHelper.ConvertDate(model.Extra2);
                else exist.ChequeDate = DateTime.Now.Date;
                exist.VocherDate = DateTimeHelper.ConvertDate(model.Extra1);
                exist.ChequeNo = model.ChequeNo;
                //exist.EntryDate = DateTimeHelper.ConvertDate(model.Extra1);
                exist.invoiceNo = model.InvoiceNo;
                exist.LedgerId = model.LedgerId;
                exist.ModifiedBy = CurrentUser.UserID;
                exist.ModifiedOn = DateTime.Now;
                exist.Narration = model.Narration;
                exist.voucherNo = model.VoucherNo;
                exist.CampusId = CurrentUser.SelectedCampusId;

                foreach (var p in model.PaymentDetails)
                {
                    var detail = await db.ReceiptDetails.FirstOrDefaultAsync(w => w.Id == p.PaymentDetailsId);
                    if (detail == null)
                    {
                        detail = new tbl_ReceiptDetails();
                        if (isNew)
                        {
                            exist.ReceiptDetails.Add(detail);
                        }
                        else
                        {
                            db.ReceiptDetails.Add(detail);
                        }
                    }
                    detail.LedgerId = p.LedgerId;
                    detail.Amount = p.Amount;
                    if (!isNew)
                        detail.ReceiptMasterId = exist.Id;
                }
                await db.SaveChangesAsync();

                var master = await db.ReceiptMasters.FirstOrDefaultAsync(w => w.Id == exist.Id);
                foreach (var m in master.ReceiptDetails)
                {
                    await sp.LeddgerPosting(m.Amount, m.Id, m.LedgerId, true, master.EntryDate, ReceiptVoucherTypeId, master.ChequeDate, voucherNo: master.voucherNo, invoiceNo: master.invoiceNo, checkno: master.ChequeNo);
                    await sp.LeddgerPosting(m.Amount, m.Id, master.LedgerId, false, master.EntryDate, ReceiptVoucherTypeId, master.ChequeDate, voucherNo: master.voucherNo, invoiceNo: master.invoiceNo, checkno: master.ChequeNo);
                }
                return StaticResources.GetResult(true, "Info! Voucher Saved successfully.");
            }
            catch (Exception ex)
            {
                return StaticResources.GetResult(false, ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }
        }
        public async Task<JsonResult> ReceiptVoucherList(DataManagerRequest dm, string fromdate, string todate, int LedgerId, string VoucherNo)
        {
            int campusid = CurrentUser.SelectedCampusId;
            var data = db.ReceiptMasters.Where(w => w.CampusId == campusid).Select(s => new
            {
                s.Id,
                s.VocherDate,
                s.voucherNo,
                s.Ledger.LedgerName,
                s.LedgerId,
                Amount = s.ReceiptDetails.Select(t => t.Amount).DefaultIfEmpty(0).Sum()

            }).Where(w => w.Id > 0);
            var date1 = DateTimeHelper.ConvertDate(fromdate);
            var date2 = DateTimeHelper.ConvertDate(todate);
            if (date1 != DateTime.MinValue)
            {
                data = data.Where(w => w.VocherDate >= date1);
            }
            if (date2 != DateTime.MinValue)
            {
                data = data.Where(w => w.VocherDate <= date2);
            }
            if (!string.IsNullOrEmpty(VoucherNo))
            {
                data = data.Where(w => w.voucherNo.Contains(VoucherNo));
            }
            if (LedgerId != -1)
            {
                data = data.Where(w => w.LedgerId == LedgerId);
            }
            DataOperations operation = new DataOperations();

            if (dm.Search != null && dm.Search.Count > 0)
            {
                data = operation.PerformSearching(data, dm.Search);  //Search
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                data = operation.PerformFiltering(data, dm.Where, dm.Where[0].Operator);
            }
            int count = data.Count();
            if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
            {
                data = operation.PerformSorting(data, dm.Sorted);
            }
            if (dm.Skip != 0)
            {
                if (dm.Sorted == null || dm.Sorted.Count == 0)
                {
                    List<Sort> srt = new List<Sort>() { };
                    srt.Add(new Sort { Name = "VocherDate", Direction = "ascending" });
                    data = operation.PerformSorting(data, srt);
                }

                data = operation.PerformSkip(data, dm.Skip);   //Paging
            }
            if (dm.Take != 0)
            {
                data = operation.PerformTake(data, dm.Take);
            }

            if (dm.RequiresCounts)
            {
                return Json(new { result = await data.ToListAsync(), count = count });
            }
            else
            {
                return Json(await data.ToListAsync());
            }
        }
        public async Task<JsonResult> ReceiptVoucherDetails(int id)
        {
            try
            {
                var data = await db.ReceiptDetails.AsNoTracking().Where(w => w.ReceiptMasterId == id).Select(s => new { s.Id, s.LedgerId, s.Amount }).ToListAsync();

                return Json(new { status = true, data = data });
            }
            catch (Exception ex)
            {
                return StaticResources.GetResult(false, ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }
        }
        public async Task<JsonResult> DeleteReceiptVoucher(int id)
        {
            try
            {
                var exist = await db.ReceiptMasters.FirstOrDefaultAsync(w => w.Id == id);
                if (exist != null)
                {
                    var toremoveids = exist.ReceiptDetails.Select(s => new { s.Id, s.LedgerId }).ToList();
                    db.ReceiptMasters.Remove(exist);
                    await db.SaveChangesAsync();
                    foreach (var s in toremoveids)
                        await sp.DeleteLedgerPosting(s.Id, ReceiptVoucherTypeId, s.LedgerId);
                }
                return StaticResources.GetResult(true, "success");
            }
            catch { }
            return StaticResources.GetResult(false, "failed");
        }
        public async Task<JsonResult> DeleteReceiptVoucherDetails(int id)
        {
            try
            {
                var exist = await db.ReceiptDetails.FirstOrDefaultAsync(w => w.Id == id);
                if (exist != null)
                {
                    db.ReceiptDetails.Remove(exist);
                    await db.SaveChangesAsync();
                    await sp.DeleteLedgerPosting(exist.Id, ReceiptVoucherTypeId, exist.LedgerId);
                }
                return StaticResources.GetResult(true, "success");
            }
            catch { }
            return StaticResources.GetResult(false, "failed");
        }

        #endregion


        #region Journal
        public async Task<ActionResult> JournalRegister()
        {
            ViewBag.LedgerLists = await GetBankOrCashLedgerList(true);
            return View();
        }
        public async Task<ActionResult> JournalVoucher(int id = 0)
        {

            ViewBag.LedgerLists = await GetLedgerList();
            ViewBag.BankOrCash = await GetBankOrCashLedgerList();
            var model = await db.JournalMasters.Where(w => w.Id == id).Select(s => new PaymentMasterInfo
            {
                Date = s.VocherDate,
                LedgerId = s.LedgerId,
                Narration = s.Narration,
                InvoiceNo = s.invoiceNo,
                PaymentMasterId = s.Id,
                VoucherNo = s.voucherNo,
                VoucherTypeId = s.VoucherTypeId,
                ChequeDate = s.ChequeDate,
                ChequeNo = s.ChequeNo,
                PaymentDetails = s.JournalDetails.Select(t => new PaymentDetailsInfo { Debit = t.Debit, Credit = t.Credit, LedgerId = t.LedgerId, PaymentDetailsId = t.Id, PaymentMasterId = s.Id })
            }).FirstOrDefaultAsync();
            return View(model);
        }
        [HttpPost]
        public async Task<JsonResult> JournalVoucher(PaymentMasterInfo model)
        {
            try
            {
                bool isNew = false;
                var exist = await db.JournalMasters.FirstOrDefaultAsync(w => w.Id == model.PaymentMasterId);
                if (exist == null)
                {
                    isNew = true;
                    exist = new tbl_JournalMaster() { VoucherTypeId = JournalVoucherTypeId, EntryDate = DateTime.Now, UserId = CurrentUser.UserID };
                    db.JournalMasters.Add(exist);
                }
                if (!string.IsNullOrEmpty(model.Extra2))
                    exist.ChequeDate = DateTimeHelper.ConvertDate(model.Extra2);
                else exist.ChequeDate = DateTime.Now.Date;
                exist.VocherDate = DateTimeHelper.ConvertDate(model.Extra1);
                exist.ChequeNo = model.ChequeNo;
                //exist.EntryDate = DateTimeHelper.ConvertDate(model.Extra1);
                exist.invoiceNo = model.InvoiceNo;
                exist.LedgerId = model.LedgerId;
                exist.ModifiedBy = CurrentUser.UserID;
                exist.ModifiedOn = DateTime.Now;
                exist.Narration = model.Narration;
                exist.voucherNo = model.VoucherNo;
                exist.CampusId = CurrentUser.SelectedCampusId;

                foreach (var p in model.PaymentDetails)
                {
                    var detail = await db.JournalDetails.FirstOrDefaultAsync(w => w.Id == p.PaymentDetailsId);
                    if (detail == null)
                    {
                        detail = new tbl_JournalDetails();
                        if (isNew)
                        {
                            exist.JournalDetails.Add(detail);
                        }
                        else
                        {
                            db.JournalDetails.Add(detail);
                        }
                    }
                    detail.LedgerId = p.LedgerId;
                    detail.Debit = p.Debit;
                    detail.Credit = p.Credit;
                    if (!isNew)
                        detail.JournalMasterId = exist.Id;
                }
                await db.SaveChangesAsync();

                var master = await db.JournalMasters.FirstOrDefaultAsync(w => w.Id == exist.Id);
                foreach (var m in master.JournalDetails)
                {
                    await sp.LeddgerPosting(m.Debit, m.Credit, m.Id, m.LedgerId, master.EntryDate, JournalVoucherTypeId, master.ChequeDate, voucherNo: master.voucherNo, invoiceNo: master.invoiceNo, checkno: master.ChequeNo);
                    await sp.LeddgerPosting(m.Debit,m.Credit, m.Id, master.LedgerId,  master.EntryDate, JournalVoucherTypeId, master.ChequeDate, voucherNo: master.voucherNo, invoiceNo: master.invoiceNo, checkno: master.ChequeNo);
                }

                return StaticResources.GetResult(true, "Info! Voucher Saved successfully.");
            }
            catch (Exception ex)
            {
                return StaticResources.GetResult(false, ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }
        }
        public async Task<JsonResult> JournalVoucherList(DataManagerRequest dm, string fromdate, string todate, int LedgerId, string VoucherNo)
        {
            int campusid = CurrentUser.SelectedCampusId;
            var data = db.JournalMasters.Where(w => w.CampusId == campusid).Select(s => new
            {
                s.Id,
                s.VocherDate,
                s.voucherNo,
                s.Ledger.LedgerName,
                s.LedgerId,
                Debit = s.JournalDetails.Select(t => t.Debit).DefaultIfEmpty(0).Sum(),
                Credit = s.JournalDetails.Select(t => t.Credit).DefaultIfEmpty(0).Sum()

            }).Where(w => w.Id > 0);
            var date1 = DateTimeHelper.ConvertDate(fromdate);
            var date2 = DateTimeHelper.ConvertDate(todate);
            if (date1 != DateTime.MinValue)
            {
                data = data.Where(w => w.VocherDate >= date1);
            }
            if (date2 != DateTime.MinValue)
            {
                data = data.Where(w => w.VocherDate <= date2);
            }
            if (!string.IsNullOrEmpty(VoucherNo))
            {
                data = data.Where(w => w.voucherNo.Contains(VoucherNo));
            }
            if (LedgerId != -1)
            {
                data = data.Where(w => w.LedgerId == LedgerId);
            }
            DataOperations operation = new DataOperations();

            if (dm.Search != null && dm.Search.Count > 0)
            {
                data = operation.PerformSearching(data, dm.Search);  //Search
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                data = operation.PerformFiltering(data, dm.Where, dm.Where[0].Operator);
            }
            int count = data.Count();
            if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
            {
                data = operation.PerformSorting(data, dm.Sorted);
            }
            if (dm.Skip != 0)
            {
                if (dm.Sorted == null || dm.Sorted.Count == 0)
                {
                    List<Sort> srt = new List<Sort>() { };
                    srt.Add(new Sort { Name = "VocherDate", Direction = "ascending" });
                    data = operation.PerformSorting(data, srt);
                }

                data = operation.PerformSkip(data, dm.Skip);   //Paging
            }
            if (dm.Take != 0)
            {
                data = operation.PerformTake(data, dm.Take);
            }

            if (dm.RequiresCounts)
            {
                return Json(new { result = await data.ToListAsync(), count = count });
            }
            else
            {
                return Json(await data.ToListAsync());
            }
        }
        public async Task<JsonResult> JournalVoucherDetails(int id)
        {
            try
            {
                var data = await db.JournalDetails.AsNoTracking().Where(w => w.JournalMasterId == id).Select(s => new { s.Id, s.LedgerId, s.Debit, s.Credit }).ToListAsync();

                return Json(new { status = true, data = data });
            }
            catch (Exception ex)
            {
                return StaticResources.GetResult(false, ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }
        }
        public async Task<JsonResult> DeleteJournalVoucher(int id)
        {
            try
            {
                var exist = await db.JournalMasters.FirstOrDefaultAsync(w => w.Id == id);
                if (exist != null)
                {
                    var toremoveids = exist.JournalDetails.Select(s => new{ s.Id,s.LedgerId}).ToList();
                    db.JournalMasters.Remove(exist);
                    await db.SaveChangesAsync();
                    foreach (var s in toremoveids)
                        await sp.DeleteLedgerPosting(s.Id, JournalVoucherTypeId, s.LedgerId);

                }
                return StaticResources.GetResult(true, "success");
            }
            catch { }
            return StaticResources.GetResult(false, "failed");
        }
        public async Task<JsonResult> DeleteJournalVoucherDetails(int id)
        {
            try
            {
                var exist = await db.JournalDetails.FirstOrDefaultAsync(w => w.Id == id);
                if (exist != null)
                {
                    db.JournalDetails.Remove(exist);
                    await db.SaveChangesAsync();
                    await sp.DeleteLedgerPosting(exist.Id, JournalVoucherTypeId, exist.LedgerId);
                }
                return StaticResources.GetResult(true, "success");
            }
            catch { }
            return StaticResources.GetResult(false, "failed");
        }

        #endregion


        //#region Bank Reconciliation

        public ActionResult BankReconciliation()
        {
            int bankid = (int)PublicVariables.EnumAccountGroup.Bank_Account;
            var Ledgers =
                db.AccountLedgers.Where(w => w.AccountGroupId == bankid).AsNoTracking().Select(s => new
                {
                    s.Id,
                    s.LedgerName
                });
            ViewBag.bank = new SelectList(Ledgers, "Id", "LedgerName");
            return View();
        }
    }
}