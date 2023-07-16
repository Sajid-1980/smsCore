//using Lib.Web.Mvc.JQuery.JqGrid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using smsCore.Data;
using smsCore.Data.Helpers;
using Syncfusion.EJ2.Base;
using System.Data;
using System.Linq.Dynamic;

namespace smsCore.Controllers
{
    //[CompressContent]
    [Authorize]
    public class AccountGroupController : BaseController
    {
        private readonly SchoolEntities db; 
        private readonly CurrentUser _user;

        
        public AccountGroupController (SchoolEntities _db , CurrentUser user)
        {
            db = _db;
            _user = user;
        }

        //private SchoolEntities db = new SchoolEntities();

        // GET: AccountGroup
        public IActionResult Index()
        {
            return View();
        }

        //Account Ledger F3 Start

        #region AccountLedger


        public async Task<IActionResult> AccountLedger()
        {
            var groupUnder = await
             db.AccountGroups.Select(s => new { s.Id, s.AccountGroupName }).ToListAsync();
            ViewBag.LedgerLists = new SelectList(groupUnder, "Id", "AccountGroupName");
            return View(new tbl_AccountLedger());
        }


        [HttpPost]
        public async Task<string> AccountLedger(tbl_AccountLedger ledgerInfo)
        {
            string message;

            try
            {
                if (string.IsNullOrEmpty(ledgerInfo.LedgerName))
                {
                    return "Invalid account ledger name.";
                }
                ledgerInfo = ledgerInfo.Validate(new List<string>() { "LedgerName" }) as tbl_AccountLedger;
                var name = ledgerInfo.LedgerName.ToLower().Trim();
                if (db.AccountLedgers.Any(a => a.LedgerName.ToLower().Trim() == name))
                    return "Account Ledger with same name already exist";

                ledgerInfo.IsDefault = false;

                ledgerInfo.EntryDate = DateTime.Now;
                ledgerInfo.UserId = _user.UserID;
                ledgerInfo.ModifiedBy = _user.UserID;
                ledgerInfo.ModifiedOn = DateTime.Now;
                db.AccountLedgers.Add(ledgerInfo);
                await db.SaveChangesAsync();
                message = "success";
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return message;
        }


        public async Task<IActionResult> AccountLedgerEdit(decimal id)
        {
           var groupUnder = await
             db.AccountGroups.Select(s => new { s.Id, s.AccountGroupName }).ToListAsync();
            ViewBag.LedgerLists = new SelectList(groupUnder, "Id", "AccountGroupName");
            var model = db.AccountLedgers.Find(id);
            return View(model);
        }

        
        [HttpPost]
        public async Task<string> AccountLedgerEdit(tbl_AccountLedger ledgerInfo)
        {
            string message;

            try
            {
                if (string.IsNullOrEmpty(ledgerInfo.LedgerName))
                {
                    return "Invalid account ledger name.";
                }
                var exist = db.AccountLedgers.Find(ledgerInfo.Id);
                if(exist == null)
                {
                    return "No record found to edit.";
                }
                ledgerInfo = ledgerInfo.Validate(new List<string>() { "LedgerName" }) as tbl_AccountLedger;

                var name = ledgerInfo.LedgerName.ToLower().Trim();
                if (db.AccountLedgers.Any(a =>a.Id!=ledgerInfo.Id && a.LedgerName.ToLower().Trim() == name))
                    return "Account Ledger with same name already exist";

                exist.AccountGroupId = ledgerInfo.AccountGroupId;
                exist.AccountNo = ledgerInfo.AccountNo;
                exist.Address = ledgerInfo.Address;
                exist.BranchCode = ledgerInfo.BranchCode;
                exist.BranchName = ledgerInfo.BranchName;
                exist.CreditLimit = ledgerInfo.CreditLimit;
                exist.CreditPeriod = ledgerInfo.CreditPeriod;
                exist.CrOrDr = ledgerInfo.CrOrDr;
                exist.Email = ledgerInfo.Email;
                exist.LedgerName = ledgerInfo.LedgerName;
                exist.MailingName = ledgerInfo.MailingName;
                exist.Mobile = ledgerInfo.Mobile;
                exist.Narration = ledgerInfo.Narration;
                exist.OpeningBalance = ledgerInfo.OpeningBalance;
                exist.Phone = ledgerInfo.Phone;


                exist.ModifiedBy = _user.UserID;
                exist.ModifiedOn = DateTime.Now;
                db.AccountLedgers.Add(ledgerInfo);
                await db.SaveChangesAsync();
                message = "success";
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return message;
        }
        
        
        public IActionResult AccountLedgerDelete(decimal id)
        {
            //var ledgerSP = new AccountLedgerSP();
            //var spAccountGroup = new AccountGroupSP();
            //var Ledgers = spAccountGroup.AccountGroupViewAllComboFillForAccountLedger().AsEnumerable().Select(s => new
            //{
            //    accountGroupId = (decimal) s["accountGroupId"],
            //    accountGroupName = s["accountGroupName"].ToString()
            //});
            //ViewBag.LedgerLists = new SelectList(Ledgers, "accountGroupId", "accountGroupName");
            //var data = ledgerSP.AccountLedgerView(id);
            ////var pricing = spPricingLevel.PricelistPricingLevelViewAllForComboBox().AsEnumerable().Select(s => new
            ////{
            ////    pricinglevelId = (decimal) s["pricinglevelId"],
            ////    pricinglevelName = s["pricinglevelName"].ToString()
            ////});
            ////ViewBag.PricingList = new SelectList(pricing, "pricinglevelId", "pricinglevelName");
            ////var Area = spArea.AreaViewAll().AsEnumerable().Select(s => new
            ////{
            ////    areaId = (decimal) s["areaId"],
            ////    pricinglevelName = s["areaName"].ToString()
            ////});
            ////ViewBag.AreaLists = new SelectList(Area, "areaId", "areaName");
            ////var rout = spRout.RouteViewByArea(Convert.ToDecimal("1")).AsEnumerable().Select(s => new
            ////{
            ////    routeId = (decimal) s["routeId"],
            ////    pricinglevelName = s["routeName"].ToString()
            ////});
            ////ViewBag.RouteLists = new SelectList(rout, "routeId", "routeName");
            return View();
        }

        
        //[HttpPost]
        //public string AccountLedgerDelete(AccountLedgerInfo ledgerInfo)
        //{
        //    string message="";
        //    //try
        //    //{
        //    //    if (!ledgerInfo.IsDefault)
        //    //    {
        //    //        var spAccountLedger = new AccountLedgerSP();
        //    //        spAccountLedger.PartyBalanceDeleteByVoucherTypeVoucherNoAndReferenceType(
        //    //            ledgerInfo.LedgerId.ToString(), 1);
        //    //        spAccountLedger.LedgerPostingDeleteByVoucherTypeAndVoucherNo(ledgerInfo.LedgerId.ToString(), 1);

        //    //        if (spAccountLedger.AccountLedgerCheckReferences(ledgerInfo.LedgerId) == -1)
        //    //            ModelState.AddModelError("", "");
        //    //    }
        //    //    else
        //    //    {
        //    //        return "Can't delete build in account ledger";
        //    //    }

        //    //    message = "success";
        //    //}

        //    //catch (Exception ex)
        //    //{
        //    //    message = ex.Message;
        //    //}

        //    return message;
        //}

        public void LedgerPostingAdd(decimal decOpeningBalances, decimal decLedgerId, string cmbOpeningBalanceCrOrDr)
        {
            string message = "";
            try
            {
                string strfinancialId;
                var decOpeningBalance = Convert.ToDecimal(decOpeningBalances);
                //var spLedgerPosting = new LedgerPostingSP();
                //var infoLedgerPosting = new LedgerPostingInfo();
                //var spFinancialYear = new FinancialYearSP();
                //var infoFinancialYear = new FinancialYearInfo();

                //var ids = PublicVariables._decCurrentFinancialYearId;
                //infoFinancialYear = spFinancialYear.FinancialYearViewForAccountLedger(ids);
                //strfinancialId = infoFinancialYear.FromDate.ToString("dd-MMM-yyyy");
                //infoLedgerPosting.VoucherTypeId = 1;

                //infoLedgerPosting.Date = Convert.ToDateTime(strfinancialId);
                ////infoLedgerPosting.Date =DateTime.Now;
                //////date no valid


                //infoLedgerPosting.LedgerId = decLedgerId;
                //infoLedgerPosting.VoucherNo = decLedgerId.ToString();

                //if (cmbOpeningBalanceCrOrDr == "Dr")
                //    infoLedgerPosting.Debit = decOpeningBalance;
                //else
                //    infoLedgerPosting.Credit = decOpeningBalance;

                //infoLedgerPosting.DetailsId = 0;
                //infoLedgerPosting.YearId = PublicVariables._decCurrentFinancialYearId;
                //infoLedgerPosting.InvoiceNo = decLedgerId.ToString();
                //infoLedgerPosting.ChequeNo = string.Empty;
                //infoLedgerPosting.ChequeDate = DateTime.Now;
                //infoLedgerPosting.Extra1 = string.Empty;
                //infoLedgerPosting.Extra2 = string.Empty;
                //infoLedgerPosting.ExtraDate = DateTime.Now;
                //spLedgerPosting.LedgerPostingAdd(infoLedgerPosting);
            }
            catch (Exception ex)
            {
                message = ex.Message;

                //  PublicVariables.infoError.ErrorString = "AL4:" + ex.Message;
            }
            //return message;
        }
        
       

        [HttpPost]
        public JsonResult GetAccountLedgerForJq(DataManagerRequest dm)
        {
            var data = db.AccountLedgers.Select(s => new
            {

                ledgerId = s.Id,
                ledgerName = s.LedgerName,
                accountGroupId = s.AccountGroupId,
                openingBalance = s.OpeningBalance,
                crOrDr = s.CrOrDr,
                isDefault = s.IsDefault
            });

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
                    srt.Add(new Sort { Name = "RegistrationNo", Direction = "ascending" });
                    data = operation.PerformSorting(data, srt);
                }

                data = operation.PerformSkip(data, dm.Skip);   //Paging
            }
            if (dm.Take != 0)
            {
                data = operation.PerformTake(data, dm.Take);

            }
            return Json(new { count = count, result = data });
        }

        //public IActionResult GetAccountLedgerForJq(JqGridRequest request)
        //{
        //    //AccountLedgerSP smm = new AccountLedgerSP();
        //    //var datatable = smm.AccountLedgerViewAll();
        //    var adata = db.AccountLedgers.Select(s => new
        //    {
        //        ledgerId=s.Id, ledgerName=s.LedgerName,
        //        accountGroupId = s.AccountGroupId,
        //        openingBalance = s.OpeningBalance, crOrDr =s.CrOrDr , isDefault=s.IsDefault
        //    });
        //    var totalRecords = adata.Count(); // datatable.Rows.Count;

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

        //    IEnumerable<JqGridRecord> data = null;

        //    //data = datatable.AsEnumerable().Skip(skip).Take(perpage).Select(x => new JqGridRecord(x["ledgerId"].ToString(), new
        //    //{
        //    //    Id = x["ledgerId"].ToString(),
        //    //    ledgerName = (string)(x["ledgerName"] == null ? "" : x["ledgerName"]),
        //    //    accountGroupId = (decimal)x["accountGroupId"],
        //    //    openingBalance = (decimal)(x["openingBalance"]),
        //    //    crOrDr = (string)(x["crOrDr"] == null ? "" : x["crOrDr"]),
        //    //    action = $"<a title=\"Edit\" href=\"{Url.Action("AccountLedgerEdit", "AccountGroup", new { id = x["ledgerId"] })}\" class=\"edit ml-1\"><i class=\"fa fa-edit\"></i></a> <a title=\"Delete\" href=\"{Url.Action("AccountLedgerdelete", "AccountGroup", new { id = x["ledgerId"] })}\" class=\"delete ajax ml-1\"><i class=\"fa fa-trash\"></i></a>"
        //    //}));

        //    data = adata.OrderBy(request.SortingName + " " + request.SortingOrder).Skip(skip).Take(perpage).ToList()
        //        .Select(x => new JqGridRecord(x.ledgerId.ToString(), new
        //        {
        //            x.ledgerId,
        //            x.ledgerName,
        //            accountGroupId = db.AccountGroups.Find(x.accountGroupId).AccountGroupName,
        //            x.openingBalance,
        //            x.crOrDr,
        //            action = x.isDefault
        //                ? ""
        //                : $"<a title=\"Edit\" href=\"{Url.Action("AccountLedgerEdit", "AccountGroup", new {id = x.ledgerId})}\" class=\"ajax edit ml-1\"><i class=\"fa fa-edit\"></i></a> <a title=\"Delete\" href=\"{Url.Action("AccountLedgerdelete", "AccountGroup", new {id = x.ledgerId})}\" class=\"delete ajax ml-1\"><i class=\"fa fa-trash\"></i></a>"
        //        }));

        //    response.Records.AddRange(data);
        //    //response.Records = data;
        //    response.Reader.RepeatItems = false;
        //    return new JsonResult(response);
        //    //return db.Companies.ToList();
        //}

        #endregion


        //Accout Group F2 Start

        #region Accountgroup

        #region Chart Of Account    
        public IActionResult ChartofAccounts()
        {
            return View();
        }

        public ActionResult GetAccountTypes(FormCollection postData)
        {
            var nodeId = string.Empty;
            if (postData.Keys.Contains("nodeid"))
                nodeId = postData["nodeid"];

            var n_level = string.Empty;
            if (postData.Keys.Contains("n_level"))
                n_level = postData["n_level"];

            var parentId = string.Empty;
            if (postData.Keys.Contains("parentid"))
                parentId = postData["parentid"];

            var data = GetTree(nodeId, n_level, parentId);
            //Preparing result
            var filesData = new
            {
                page = 1,
                total = 1,
                records = data.Count(),
                rows = (from child in data
                        select new
                        {
                            //table of cells values
                            cell = new object[]
                            {
                                child.id,
                                child.elementName,
                                child.transaction,
                                //Level - based on '\' count
                                child.level,
                                //Parent id
                                child.parent,
                                //Is not expandable
                                child.isLeaf,
                                //Is expanded
                                false
                            }
                        }
                    ).ToArray()
            };

            //Returning json data
            return Json(filesData);
        }

        public List<dynamic> GetTree(string nodeiId, string lvl, string parentid)
        {
            var dyn = new List<dynamic>();

            var types = db.AccountGroups.Select(s => s.Nature).Distinct().ToList();


            if (string.IsNullOrEmpty(nodeiId))
            {
                foreach (var t in types)
                {
                    var level = 0;
                    var accounts = db.AccountGroups.Where(w => w.GroupUnder == 0).ToList();
                    dyn.Add(new
                    {
                        id = "type_" + t,
                        elementName = t,
                        transaction = false,
                        level = level.ToString(),

                        parent = "",
                        isLeaf = accounts.Count > 0 ? false : true,
                        expanded = false,
                        loaded = true,
                        type = "type"
                    });
                }
            }
            else
            {
                var level = int.Parse(lvl) + 1;

                var id = nodeiId.Split(new[] {'_'}, StringSplitOptions.RemoveEmptyEntries);
                var nature = string.Empty;
                decimal? accountId = null;
                if (id[0] == "type")
                    nature = id[1];
                else
                    accountId = decimal.Parse(id[1]);

                parentid = nodeiId; // parentid == string.Empty ? nodeiId : parentid;

                var accountF = !string.IsNullOrEmpty(nature)
                    ? db.AccountGroups.Where(w => w.Nature == nature && (w.GroupUnder == 0))
                    : db.AccountGroups.Where(w => w.GroupUnder == accountId);

                foreach (var ac in accountF) dyn.AddRange(GetTree(ac, level, parentid));
            }

            return dyn;
        }

        public List<dynamic> GetTree(tbl_AccountGroup account, int level, string parentId)
        {
            var dyn = new List<dynamic>();
            var childAccounts = db.AccountGroups.Where(w => w.GroupUnder == account.Id).ToList();
            var childLagers = db.AccountLedgers.Where(w => w.AccountGroupId == account.Id).ToList();
            dyn.Add(new
            {
                id = "account_" + account.Id,
                elementName = account.AccountGroupName,
                transaction = false,
                level = level.ToString(),
                parent = parentId,
                isLeaf = childAccounts.Count > 0 || childLagers.Count > 0 ? false : true,
                expanded = false,
                loaded = true,
                type = "account"
            });

            foreach (var ledger in childLagers)
                dyn.AddRange(GetLedger(ledger, level + 1, "account_" + account.Id));
            return dyn;
        }

        public List<dynamic> GetLedger(tbl_AccountLedger account, int level, string parentId)
        {
            var dyn = new List<dynamic>();
            dyn.Add(new
            {
                id = "ledger_" + account.Id,
                elementName = $"<a href='#'>{account.LedgerName}</a>",
                transaction = true,
                level = level.ToString(),
                parent = parentId,
                isLeaf = true,
                expanded = false,
                loaded = true,
                type = "ledger"
            });

            return dyn;
        }

        #endregion

        public async Task<IActionResult> Accountgroup()
        {
            
            var groupUnder = await
            db.AccountGroups.Select(s => new { s.Id, s.AccountGroupName }).ToListAsync();
            ViewBag.groupUnder = new SelectList(groupUnder, "Id", "AccountGroupName");
            return View(new tbl_AccountGroup());
        }

        
        [HttpPost]
        public async Task<string> Accountgroup(tbl_AccountGroup model)
        {
            string message="";
            string name = model.AccountGroupName.ToLower().Trim();
            var exist = db.AccountGroups.AsNoTracking().Where(w => w.AccountGroupName.ToLower().Trim() == name).Any();
            if (exist)
                return "Account Group with same name already exist.";
            model.Nature = model.Nature??string.Empty;
            model.IsDefault = false;
            model.Narration = model.Narration ?? string.Empty;
            model.EntryDate = DateTime.Now;
            model.UserId = _user.UserID;
            model.ModifiedBy = _user.UserID;
            model.ModifiedOn = DateTime.Now;
            db.AccountGroups.Add(model);
            try
            {
                await db.SaveChangesAsync();
                message = "success";
            }
            catch (Exception aa)
            {
                message = aa.Message;
            }

            return message;
        }
        public JsonResult changeUndergroup(int groupid)
        {
            var result = new JsonResult(null);
           var infoAccountGroup = db.AccountGroups.Find(groupid);
            var strNature = infoAccountGroup.Nature;
            //var strIsAffectGrossProfit = infoAccountGroup.AffectGrossProfit;
            if (strNature != "NA")
            {
                var data = strNature;
                result=new JsonResult( data);
            }

            
            return result;
        }

        public IActionResult AccountgroupEdit(decimal id)
        {
            //var spAccountGroup = new AccountGroupSP();

            //var list = spAccountGroup.AccountGroupView(id);
            //var groupUnder = spAccountGroup.AccountGroupViewAllComboFill().AsEnumerable().Select(s => new
            //{
            //    accountGroupId = (decimal) s["accountGroupId"],
            //    accountGroupName = s["accountGroupName"].ToString()
            //});
            //ViewBag.GroupUnder = new SelectList(groupUnder, "accountGroupId", "accountGroupName", list.GroupUnder);
            return View();
        }

        
        //[HttpPost]
        //public string AccountgroupEdit(AccountGroupInfo groupInfo)
        //{
        //    string message;
        //    //var spAccountGroup = new AccountGroupSP();
        //    //var infoAccountGroup = new AccountGroupInfo();

        //    //infoAccountGroup.AccountGroupName = Request.Form["GroupName"].Trim();
        //    //infoAccountGroup.GroupUnder = Convert.ToDecimal(groupInfo.GroupUnder.ToString());
        //    //infoAccountGroup.Nature = Request.Form["Nature"];
        //    //infoAccountGroup.AffectGrossProfit = groupInfo.AffectGrossProfit;
        //    //infoAccountGroup.IsDefault = false;
        //    //infoAccountGroup.Narration = Request.Form["Narration"].Trim();
        //    //infoAccountGroup.Extra1 = string.Empty;
        //    //infoAccountGroup.Extra2 = string.Empty;

        //    //infoAccountGroup.AccountGroupId = groupInfo.AccountGroupId;

        //    try
        //    {
        //        //spAccountGroup.AccountGroupUpdate(infoAccountGroup);
        //        message = "success";
        //    }
        //    catch (Exception aa)
        //    {
        //        message = aa.Message;
        //    }

        //    return message;
        //}

        

        
        public IActionResult Accountgroupdelete(decimal id)
        {
            //var spAccountGroup = new AccountGroupSP();

            //var list = spAccountGroup.AccountGroupView(id);
            //var groupUnder = spAccountGroup.AccountGroupViewAllComboFill().AsEnumerable().Select(s => new
            //{
            //    accountGroupId = (decimal) s["accountGroupId"],
            //    accountGroupName = s["accountGroupName"].ToString()
            //});
            //ViewBag.groupUnder = new SelectList(groupUnder, "accountGroupId", "accountGroupName");
            return View();
        }


        //[HttpPost]
        //public string Accountgroupdelete(AccountGroupInfo groupInfo)
        //{
        //    string message="";

        //    if (groupInfo.IsDefault)
        //    {
        //        message = "Can't delete build in account group";
        //    }
        //    else
        //    {
        //        //var spAccountGroup = new AccountGroupSP();
        //        //try
        //        //{
        //        //    spAccountGroup.AccountGroupReferenceDelete(groupInfo.AccountGroupId);
        //        //    message = "success";
        //        //}
        //        //catch (Exception ex)
        //        //{
        //        //    message = ex.Message;
        //        //}
        //    }

        //    return message;
        //}

        //public IActionResult GetAccountGroupForJq(JqGridRequest request)
        //{
        //    //AccountGroupSP smm = new AccountGroupSP();
        //    //var datatable = smm.AccountGroupSearch("","All");
        //    var totalRecords = 0; // datatable.Rows.Count;

        //    var accountgroups = db.AccountGroups.Select(s => new
        //        {accountGroupId = s.Id, accountGroupName=s.AccountGroupName, groupUnder=s.GroupUnder, isDefault=s.IsDefault});
        //    totalRecords = accountgroups.Count();
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

        //    IEnumerable<JqGridRecord> data = null;
        //    //data = datatable.AsEnumerable().Skip(skip).Take(perpage).Select(x => new JqGridRecord(x["accountGroupId"].ToString(), new
        //    //{
        //    //    Id = x["accountGroupId"].ToString(),
        //    //    accountGroupName = (string)(x["AccountGroupName"] == null ? "" : x["AccountGroupName"]),
        //    //    groupUnder = (string)(x["Under"] == null ? "" : x["Under"]),
        //    //    action = $"<a title=\"Edit\" href=\"{Url.Action("AccountgroupEdit", "AccountGroup", new { id = x["accountGroupId"] })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-edit\"></i></a> <a title=\"Delete\" href=\"{Url.Action("Accountgroupdelete", "AccountGroup", new { id = x["accountGroupId"] })}\" class=\"delete ajax ml-1\"><i class=\"fa fa-trash\"></i></a>"
        //    //}));

        //    data = accountgroups.OrderBy(o => o.accountGroupId).Skip(skip).Take(perpage).ToList().Select(x =>
        //        new JqGridRecord(x.accountGroupId.ToString(), new
        //        {
        //            Id = x.accountGroupId,
        //            x.accountGroupName,
        //            groupUnder = db.AccountGroups.Where(w => w.Id == x.groupUnder)
        //                .Select(s => s.AccountGroupName).FirstOrDefault(),
        //            action = x.isDefault
        //                ? ""
        //                : $"<a title=\"Edit\" href=\"{Url.Action("AccountgroupEdit", "AccountGroup", new {id = x.accountGroupId})}\" class=\"edit ajax ml-1\"><i class=\"fa fa-edit\"></i></a> <a title=\"Delete\" href=\"{Url.Action("Accountgroupdelete", "AccountGroup", new {id = x.accountGroupId})}\" class=\"delete ajax ml-1\"><i class=\"fa fa-trash\"></i></a>"
        //        }));

        //    foreach (var d in data) response.Records.Add(d);
        //    //response.Records = data;
        //    response.Reader.RepeatItems = false;
        //    return new JsonResult (response);
        //    //return db.Companies.ToList();
        //}
        [HttpPost]
        public JsonResult GetAccountGroupForJq(DataManagerRequest dm)
        {

            var data = db.AccountGroups.Select(s => new
            {
                id = s.Id,
                accountGroupName = s.AccountGroupName,
                groupUnder = s.GroupUnder,

            });

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
                    srt.Add(new Sort { Name = "RegistrationNo", Direction = "ascending" });
                    data = operation.PerformSorting(data, srt);
                }

                data = operation.PerformSkip(data, dm.Skip);   //Paging
            }
            if (dm.Take != 0)
            {
                data = operation.PerformTake(data, dm.Take);

            }
            return Json(new { count = count, result = data });
        }


        #endregion
    }
}