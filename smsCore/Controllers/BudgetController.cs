using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using sms.Controllers;
using smsCore.Data;
using smsCore.Data.Helpers;
using smsCore.Data.Models;
using Syncfusion.EJ2.Base;
using System.Data;

namespace smsCore.Controllers
{
   // [CompressContent]
    [Authorize]
    public class BudgetController : BaseController
    {
        private SchoolEntities db;
        private readonly StaticResources _resource;
        private readonly CurrentUser _user;


        public BudgetController (SchoolEntities  _db , StaticResources resources  , CurrentUser user)
        {
            db = _db;
            _resource = resources;
            _user = user;       
        }


        // GET: TblBudgetDetails


        #region Payments

        public async Task<JsonResult> GetLedgers()
        {
            var ledgers =await db.AccountLedgers.Select(s => new { id = s.Id, text = s.LedgerName }).ToListAsync();
            return Json(new { status = true, data = ledgers });
        }
        public async Task<JsonResult> GetGroups()
        {
            var ledgers = await db.AccountGroups.Select(s => new { id = s.Id, text = s.AccountGroupName }).ToListAsync();
            return Json(new { status = true, data = ledgers });
        }

        public async Task<IActionResult> Index()
        {
            //ViewBag.LedgerLists = await AccountsController.GetBankOrCashLedgerList(true);
            return View();
        }
        public async Task<IActionResult> Create(int id = 0)
        {
            var model = await db.BudgetMasters.Where(w => w.Id == id).Select(s => new BudgetMasterInfo
            {
                BudgetMasterId = s.Id,
                BudgetName = s.BudgetName,
                FromDate = s.FromDate,
                Narration = s.Narration,
                ToDate = s.ToDate,
                Type = s.Type,
                BudgetDetails = s.BudgetDetails.Select(t => new BudgetDetailsInfo { CrDr = t.CrDr, BudgetDetailsId = t.Id, BudgetMasterId = s.Id, Credit = t.CrDr == "Cr" ? t.Credit : t.Debit, LedgerId = t.LedgerId })
            }).FirstOrDefaultAsync();
            return View(model);
        }




        [HttpPost]
        public async Task<JsonResult> Create(BudgetMasterInfo model)
        {
            try
            {
                bool isNew = false;
                var exist = await db.BudgetMasters.FirstOrDefaultAsync(w => w.Id == model.BudgetMasterId);
                if (exist == null)
                {
                    isNew = true;
                    exist = new tbl_BudgetMaster() { EntryDate = DateTime.Now, UserId = _user.UserID };
                    db.BudgetMasters.Add(exist);
                }

                exist.BudgetName = model.BudgetName;
                exist.FromDate = DateTimeHelper.ConvertDate(model.Extra1);
                exist.ToDate = DateTimeHelper.ConvertDate(model.Extra2);
                exist.Type = model.Type;
                exist.ModifiedBy = _user.UserID;
                exist.ModifiedOn = DateTime.Now;
                exist.Narration = model.Narration;

                foreach (var p in model.BudgetDetails)
                {
                    var detail = await db.BudgetDetails.FirstOrDefaultAsync(w => w.Id == p.BudgetDetailsId);
                    if (detail == null)
                    {
                        detail = new tbl_BudgetDetails();
                        if (isNew)
                        {
                            exist.BudgetDetails.Add(detail);
                        }
                        else
                        {
                            db.BudgetDetails.Add(detail);
                        }
                    }
                    detail.LedgerId = p.LedgerId;
                    detail.CrDr = p.CrDr;
                    if (detail.CrDr.ToLower().Trim() == "dr")
                    {
                        detail.Credit = 0;
                        detail.Debit = p.Credit;
                    }
                    else
                    {
                        detail.Credit = p.Credit;
                        detail.Debit = 0;
                    }
                        if (!isNew)
                        detail.BudgetMasterId = exist.Id;
                }
                await db.SaveChangesAsync();
                return _resource.GetResult(true, "Info! Budget Saved successfully.");
            }
            catch (Exception ex)
            {
                return _resource.GetResult(false, ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }
        }
        [HttpPost]
        public async Task<JsonResult> BudgetList(DataManagerRequest dm, string type, string name)
        {
            var data = db.BudgetMasters.Include(i=>i.BudgetDetails).Select(s => new
            {
                s.Id,
                s.BudgetName,
                s.FromDate,
                s.ToDate,
                s.Type,
                Debit = s.BudgetDetails.Select(t=>t.Debit).DefaultIfEmpty().Sum(),
                Credit = s.BudgetDetails.Select(t=>t.Credit).DefaultIfEmpty().Sum()
            }).Where(w => w.Id > 0);
            if (!string.IsNullOrEmpty(name))
            {
                data = data.Where(w => w.BudgetName.Contains(name));
            }
            if (!string.IsNullOrEmpty(type))
            {
                data = data.Where(w => w.Type == type);
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
                    srt.Add(new Sort { Name = "FromDate", Direction = "ascending" });
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
                return Json(new { result = await data.ToListAsync(), count = count });
            }
        }
        public async Task<JsonResult> BudgetDetails(int id)
        {
            try
            {
                var data = await db.BudgetDetails.AsNoTracking().Where(w => w.BudgetMasterId == id).Select(s => new { s.Id, s.LedgerId, Amount = s.CrDr == "Cr" ? s.Credit : s.Debit, s.CrDr }).ToListAsync();

                return Json(new { status = true, data = data });
            }
            catch (Exception ex)
            {
                return _resource.GetResult(false, ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }
        }
        public async Task<JsonResult> DeleteBudget(int id)
        {
            try
            {
                var exist = await db.BudgetMasters.FirstOrDefaultAsync(w => w.Id == id);
                if (exist != null)
                {
                    db.BudgetMasters.Remove(exist);
                    await db.SaveChangesAsync();
                }
                return _resource.GetResult(true, "success");
            }
            catch { }
            return _resource.GetResult(false, "failed");
        }
        public async Task<JsonResult> DeleteBudgetDetails(int id)
        {
            try
            {
                var exist = await db.BudgetDetails.FirstOrDefaultAsync(w => w.Id == id);
                if (exist != null)
                {
                    db.BudgetDetails.Remove(exist);
                    await db.SaveChangesAsync();
                }
                return _resource.GetResult(true, "success");
            }
            catch { }
            return _resource.GetResult(false, "failed");
        }

        #endregion
        public IActionResult BudgetVariance(string budgetMasterId = "0")
        { 
            var list = db.BudgetMasters.AsNoTracking().Select(s => new
            {
                s.Id,
                s.BudgetName
            });
            ViewBag.budgetMasterId = new SelectList(list, "Id", "BudgetName");
            return View();
        }

        //public ActionResult GetBudgetvarianceListForJq(JqGridRequest request, string budgetMasterId = "0")
        //{
        //    if (budgetMasterId == "") budgetMasterId = "0";

        //    var spbudgetmaster = new BudgetMasterSP();
        //    var decId = Convert.ToDecimal(budgetMasterId);
        //    var datatable = spbudgetmaster.BudgetVariance(decId);

        //    IEnumerable<JqGridRecord> data = null;
        //    data = datatable.AsEnumerable().Select(x => new JqGridRecord(x["Sl NO"].ToString(), new
        //    {
        //        sno = x["Sl NO"].ToString(),
        //        Name = (string) (x["particular"] == null ? "" : x["particular"]),
        //        budgetDr = (decimal) (x["BudgetDr"] == null ? "" : x["BudgetDr"]),
        //        actualDr = (decimal) (x["ActualDr"] == null ? "" : x["ActualDr"]),
        //        varianceDr = (decimal) (x["VarianceDr"] == null ? "" : x["VarianceDr"]),
        //        budgetCr = (decimal) (x["BudgetCr"] == null ? "" : x["BudgetCr"]),
        //        actualCr = (decimal) (x["ActualCr"] == null ? "" : x["ActualCr"]),
        //        varianceCr = (decimal) (x["VarianceCr"] == null ? "" : x["VarianceCr"])
        //    }));
        //    var totalRecords = data.Count();
        //    //Prepare JqGridData instance
        //    var response = new JqGridResponse
        //    {
        //        //Total pages count
        //        TotalPagesCount = (int) Math.Ceiling(totalRecords / (float) request.RecordsCount),

        //        //Page number
        //        PageIndex = request.PageIndex,
        //        //Total records count
        //        TotalRecordsCount = totalRecords
        //        //Footer data
        //        //UserData = new { QuantityPerUnit = "Avg Unit Price:", UnitPrice = _productsRepository.GetProducts(filterExpression).Average(p => p.UnitPrice) }
        //    };
        //    var pno = request.PageIndex;
        //    var perpage = request.RecordsCount;
        //    var skip = perpage * pno;
        //    //IEnumerable<JqGridRecord> data = null;
        //    //var n = _context.Sector.OrderBy(.OrderByDescending(ToLambda<T>(propertyName));
        //    //            if (request.SortingOrder == JqGridSortingOrders.Asc)
        //    //.OrderBy(request.SortingName)
        //    //data = data.Skip(skip).Take(perpage).ToList().Select(s => new JqGridRecord(s.Id.ToString(), new {s.Id, action = $"<a title=\"Edit\" href=\"{Url.Action("Designations", "Payroll", new { id = s.Id })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-edit\"></i></a> <a title=\"Delete\" href=\"{Url.Action("Designations", "Payroll", new { id = s.Id })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-trash\"></i></a> " }));
        //    //else
        //    //    data = _context.Employee.OrderByDescending(request.SortingName).Skip(skip).Take(perpage).ToList().Select(s => new JqGridRecord(s.Id.ToString(), new { s.Id, s.EmployeeName, s.Cnic, s.Address, s.MobileNo, s.Email, action = $"<a title=\"Placement\" href=\"{Url.Action("CreatePlacement", "Employees", new { employeeId = s.Id })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-plus\"></i></a> <a title=\"Edit\" href=\"{Url.Action("Edit", "Employees", new { id = s.Id })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-edit\"></i></a> <a title=\"Delete\" href=\"{Url.Action("Delete", "Employees", new { id = s.Id })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-trash\"></i></a>" }));

        //    foreach (var d in data) response.Records.Add(d);
        //    //response.Records = data;
        //    response.Reader.RepeatItems = false;
        //    return new JqGridJsonResult {Data = response};
        //    //return db.Companies.ToList();
        //}

        //// GET: TblBudgetDetails/Create
        

        //// POST: TblBudgetDetails/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        
       
    }
}