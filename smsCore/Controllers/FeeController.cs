using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using smsCore.BackgroundTasks;
using smsCore.Data;
using smsCore.Data.Helpers;
using smsCore.Data.Models;
using smsCore.Data.Models.ViewModels;
using Syncfusion.EJ2.Base;
using Utilities;

namespace smsCore.Controllers
{
    [Authorize]
    public class FeeController : BaseController
    {
        private readonly SchoolEntities db;
        private readonly CurrentUser _user;
        private readonly SelectListHelper _selectListHelper;
        private readonly StaticResources _resource;
        private readonly MessagingSystem _ms;
        private readonly DatabaseAccessSql dba;
        private readonly LedgerPostingSP _ledgerPosting;
        private readonly int[] _campusIds;//= Helpers.CurrentUser.GetCampusIds();
        AddFeeBackgroundTask _backgroundTask;
        ClsBussinessSetting _setting;
        FeeSystemHelper _helper;


        public FeeController(SchoolEntities _db, CurrentUser user, StaticResources resources, SelectListHelper selectListHelper, MessagingSystem ms, LedgerPostingSP ledgerPosting, DatabaseAccessSql databaseAccessSql, AddFeeBackgroundTask backgroundTask, ClsBussinessSetting setting, FeeSystemHelper helper)
        {
            db = _db;
            _user = user;
            _resource = resources;
            _campusIds = user.GetCampusIds();
            _selectListHelper = selectListHelper;
            _ms = ms;
            _ledgerPosting = ledgerPosting;
            dba = databaseAccessSql;
            _backgroundTask = backgroundTask;
            _setting = setting;
            _helper = helper;
        }
        public IActionResult ClassfeeIndex()
        {
            ViewBag.FeeGroups = _selectListHelper.GetSectionFeeGroupList();
            ViewBag.ClassId = _selectListHelper.GetClassSelectList();
            return View();
        }


        [HttpPost]
        public async Task<string> AddClassFeeGroups(ClassFeeGroup model)
        {
            var message = "failed";
            var fclg = db.ClassFeeGroups.Where(w => w.CampusID == model.CampusID && w.ClassID == model.ClassID)
                .FirstOrDefault();
            if (fclg == null)
            {
                fclg = new ClassFeeGroup();

                fclg.ClassID = model.ClassID;
                fclg.CampusID = model.CampusID;
                db.ClassFeeGroups.Add(fclg);
            }

            fclg.FeeGroupID = model.FeeGroupID;
            try
            {
                await db.SaveChangesAsync();
                message = "success";
            }

            catch (Exception aa)
            {
                message = aa.InnerException != null ? aa.InnerException.Message : aa.Message;
                //ModelState.AddModelError("", aa.Message);
            }

            return message;
        }

        public IActionResult ClassfeeDelete(int? id, bool? saveChangesError = false)
        {
            if (saveChangesError.GetValueOrDefault())
                ViewBag.ErrorMessage =
                    "Delete failed. Try again, and if the problem persists see your system administrator.";
            var clsfee = db.ClassFeeGroups.Find(id);
            if (clsfee == null) return NotFound();
            return View(clsfee);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<string> ClassfeeDelete(int id)
        {
            var message = string.Empty;
            try
            {
                var clsfee = db.ClassFeeGroups.Find(id);
                db.ClassFeeGroups.Remove(clsfee);
                await db.SaveChangesAsync();
                message = "success";
            }
            catch (Exception aa)
            {
                message = aa.Message;
                //ModelState.AddModelError("", aa.Message);
            }

            return message;
        }


        [HttpPost]
        public async Task<JsonResult> ClassFeeGroupList(DataManagerRequest dm, int campusId)
        {
            var types = db.ClassFeeGroups.Include(i=>i.FeeGroup.FeeStructures).Include(i=>i.Class).
                Where(w => w.CampusID == campusId).Select(s => new
            {
                s.ID,
                s.Class.ClassName,
                s.FeeGroup.FeeGroupName,
                Amount = s.FeeGroup.FeeStructures.Where(w => !w.Revised && w.CampusID == campusId && w.FeeGroupId == s.FeeGroupID).Select(t => t.Amount).Sum(),
                s.FeeGroupID,
                s.CampusID
            }).AsEnumerable();

            DataOperations operation = new DataOperations();
            if (dm.Search != null && dm.Search.Count > 0)
            {
                types = operation.PerformSearching(types, dm.Search);  //Search
            }
            if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
            {
                types = operation.PerformSorting(types, dm.Sorted);
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                types = operation.PerformFiltering(types, dm.Where, dm.Where[0].Operator);
            }
            int count = types.Count();
            if (dm.Skip != 0)
            {
                types = operation.PerformSkip(types, dm.Skip);   //Paging
            }
            if (dm.Take != 0)
            {
                types = operation.PerformTake(types, dm.Take);
            }
            //if (dm.RequiresCounts)
            {
                return Json(new { result = types, count = count });
            }
            //else
            {
              //  return Json(types);
            }

        }

        //Class group fee end
        //***********Fee Structure Start********

        public IActionResult FeestructIndex(int campId = 0, int feegroupid = 0)
        {
            if (campId == 0 && feegroupid == 0)
            {
                var CampusId = -1;
                if (_user.BasicUserType == EnumManager.BasicUserType.Campus)
                    CampusId = _user.GetCampusIds().FirstOrDefault();
                else
                    CampusId = CampusId == -1 ? db.Campuses.Select(s => s.ID).FirstOrDefault() : CampusId;
                ViewBag.CampusId = CampusId;
                var select = db.FeeGroups.Select(s => new { s.ID, s.FeeGroupName }).ToList();
                ViewBag.feegrouplist = new SelectList(select, "ID", "FeeGroupName");

                return View();
            }

            ViewBag.CampusId = campId;
            var feegrop = db.FeeGroups.Find(feegroupid);
            ViewBag.feegrouplist = new SelectList(db.FeeGroups.ToList(), "ID", "FeeGroupName", feegroupid);
            ViewBag.FeeGroupId = feegroupid;
            return View();
        }


        public IActionResult FeestructEdit(int id, int feeGroupId, int campusId)
        {
            var feeGroup = db.FeeGroups.Where(w => w.ID == feeGroupId).Select(s => s.FeeGroupName).FirstOrDefault();
            var feetypeModel = db.FeeTypes.Where(w => w.Id == id).FirstOrDefault();
            if (feetypeModel == null || string.IsNullOrEmpty(feeGroup))
                throw new Exception("Inavlid fee type selected.");
            ViewBag.campusId = campusId;
            ViewBag.FeeGroupId = feeGroupId;
            ViewBag.FeeGroupName = feeGroup;
            return View(feetypeModel);
        }

        [HttpPost]
        public async Task<string> FeestructEdit(FeeType model)
        {
            var message = string.Empty;

            var FeeGroupId = int.Parse(Request.Form["FeeGroupId"].ToString());
            var campusId = int.Parse(Request.Form["campusId"].ToString());

            var Feestructure = db.FeeStructures.ToList().Where(w =>
                w.CampusID == campusId && w.FeeTypeId == model.Id && w.FeeGroupId == FeeGroupId && !w.Revised);
            if (Feestructure.Count() > 0 && Feestructure != null)
                foreach (var fee in Feestructure)
                    fee.Revised = true;

            var feeStructures = new FeeStructure();

            feeStructures.Amount = decimal.Parse(Request.Form["Amount"].ToString());
            feeStructures.Deduction = false;
            feeStructures.EffectiveFrom = DateTimeHelper.ConvertDate(Request.Form["EffectiveFrom"].ToString());
            feeStructures.EntryDate = DateTime.Now;
            feeStructures.FeeGroupId = FeeGroupId;
            feeStructures.FeeTypeId = model.Id;
            feeStructures.IsActive = !string.IsNullOrEmpty(Request.Form["IsActive"]);
            feeStructures.IsDeleted = !string.IsNullOrEmpty(Request.Form["IsDeleted"]);
            feeStructures.Revised = false;
            feeStructures.UserId = _user.UserID;
            feeStructures.CampusID = campusId;
            db.FeeStructures.Add(feeStructures);

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


        public IActionResult FeestructDelete(int? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
            }

            if (saveChangesError.GetValueOrDefault())
                ViewBag.ErrorMessage =
                    "Delete failed. Try again, and if the problem persists see your system administrator.";
            var feestruct = db.FeeStructures.Find(id);
            if (feestruct == null) return NotFound();
            return View(feestruct);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult FeestructDelete(int id)
        {
            try
            {
                var feestruct = db.FeeStructures.Find(id);
                db.FeeStructures.Remove(feestruct);
                db.SaveChanges();
            }
            catch
            {
            }

            return RedirectToAction("FeestructIndex");
        }
        public IActionResult FeeTypeList(DataManagerRequest dm, int classID, int campusId, int feegroupid)
        {
            string[] excludes = { "Transport Fee", "SMS charges", "AttendanceFine", "Late Fee", "Arears" };
            var types = db.FeeTypes.AsNoTracking().Where(w => !excludes.Contains(w.TypeName)).ToList().Select(s =>
                new
                {
                    s.IsOptional,
                    s.Id,
                    s.TypeName,
                    Amount =
                        s.FeeStructures.Where(w => w.CampusID == campusId && !w.Revised && w.FeeGroupId == feegroupid)
                            .Count() > 0
                            ? s.FeeStructures
                                .Where(w => w.CampusID == campusId && !w.Revised && w.FeeGroupId == feegroupid)
                                .DefaultIfEmpty().Select(ss => ss.Amount).FirstOrDefault()
                            : 0,
                    classID,
                    campusId,
                    feegroupid
                });

            DataOperations operation = new DataOperations();
            if (dm.Search != null && dm.Search.Count > 0)
            {
                types = operation.PerformSearching(types, dm.Search);  //Search
            }
            if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
            {
                types = operation.PerformSorting(types, dm.Sorted);
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                types = operation.PerformFiltering(types, dm.Where, dm.Where[0].Operator);
            }
            int count = types.Count();
            if (dm.Skip != 0)
            {
                types = operation.PerformSkip(types, dm.Skip);   //Paging
            }
            if (dm.Take != 0)
            {
                types = operation.PerformTake(types, dm.Take);
            }
            if (dm.RequiresCounts)
            {
                return Json(new { result = types, count = count });
            }
            else
            {
                return Json(types);
            }

            //return new JsonResult { Data = new { Items=types.OrderBy(o=>o.SortOrder), count=types.Count() }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public IActionResult FeeStructure()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> FeeStructure(FeeStructureViewModel model)
        {
            string groupName = model.FeeGroupName.Trim().ToLower();
            if (string.IsNullOrEmpty(groupName))
            {
                return Json(new { status = false, message = "Fee Structure name may not be empty." });
            }

            var classname = await db.Classes.FirstOrDefaultAsync(w => w.ID == model.ClassId);
            if (classname == null)
            {
                return Json(new { status = false, message = "Please select a class before continue." });
            }

            var ExistingClassFeeGroup = await db.ClassFeeGroups.FirstOrDefaultAsync(w => w.CampusID == model.CampusId && w.ClassID == model.ClassId);
            model.FeeGroupId = ExistingClassFeeGroup == null ? 0 : ExistingClassFeeGroup.FeeGroupID;

            var FeeGroup = await db.FeeGroups.FirstOrDefaultAsync(w => w.ID != model.FeeGroupId && w.FeeGroupName.Trim().ToLower() == groupName);

            if (FeeGroup != null)
            {
                return Json(new { status = false, message = "Fee Structure name already exist." });
            }

            FeeGroup = db.FeeGroups.Find(model.FeeGroupId);
            if (FeeGroup == null)
            {
                FeeGroup = new FeeGroup { FeeGroupName = model.FeeGroupName.Trim() };
                db.FeeGroups.Add(FeeGroup);
                await db.SaveChangesAsync();
                ExistingClassFeeGroup = new ClassFeeGroup { CampusID = model.CampusId, ClassID = model.ClassId, FeeGroupID = FeeGroup.ID };
                db.ClassFeeGroups.Add(ExistingClassFeeGroup);
                await db.SaveChangesAsync();
                model.FeeGroupId = FeeGroup.ID;
            }
            else if (FeeGroup.FeeGroupName != model.FeeGroupName)
            {
                FeeGroup.FeeGroupName = model.FeeGroupName;
            }
            var currentStructure = await db.FeeStructures.Where(w => w.CampusID == model.CampusId && w.FeeGroupId == model.FeeGroupId && !w.Revised).ToListAsync();
            foreach (var i in model.FeeStructureTypes)
            {
                var exist = currentStructure.FirstOrDefault(w => w.FeeTypeId == i.FeeTypeId);
                if (exist != null)
                {
                    if (i.Amount != exist.Amount)
                        exist.Revised = true;
                }
                if (exist == null || (exist != null && i.Amount != exist.Amount))
                {
                    if (i.Amount <= 0)
                        continue;
                    var structure = new FeeStructure
                    {
                        Amount = i.Amount,
                        FeeTypeId = i.FeeTypeId,
                        IsActive = true,
                        CampusID = model.CampusId,
                        Deduction = false,
                        EffectiveFrom = DateTime.Now,
                        EntryDate = DateTime.Now,
                        FeeGroupId = model.FeeGroupId,
                        IsDeleted = false,
                        LastEditBy = _user.UserID,
                        LastEditedOn = DateTime.Now,
                        Revised = false,
                        UserId = _user.UserID
                    };
                    db.FeeStructures.Add(structure);
                }
            }
            await db.SaveChangesAsync();
            return Json(new { status = true, message = "Fee Structure saved successfully." });
        }
        [HttpPost]
        public JsonResult FeeStructureList(DataManagerRequest dm, int feeGroupId, int campusId)
        {
            string[] excludes = { "Transport Fee", "SMS charges", "AttendanceFine", "Late Fee", "Arears" };
            int i = 1;
            var types = db.FeeTypes.AsNoTracking().Where(w => !excludes.Contains(w.TypeName)).ToList().Select(s =>
                new
                {
                    SNO = i++,
                    FeeTypeId = s.Id,
                    s.TypeName,
                    Amount =
                        s.FeeStructures.Where(w => w.CampusID == campusId && !w.Revised && w.FeeGroupId == feeGroupId)
                            .Count() > 0
                            ? s.FeeStructures
                                .Where(w => w.CampusID == campusId && !w.Revised && w.FeeGroupId == feeGroupId)
                                .DefaultIfEmpty().Select(ss => ss.Amount).FirstOrDefault()
                            : 0,
                    feeGroupId,
                    campusId
                });

            DataOperations operation = new DataOperations();
            if (dm.Search != null && dm.Search.Count > 0)
            {
                types = operation.PerformSearching(types, dm.Search);  //Search
            }
            if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
            {
                types = operation.PerformSorting(types, dm.Sorted);
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                types = operation.PerformFiltering(types, dm.Where, dm.Where[0].Operator);
            }
            int count = types.Count();
            if (dm.Skip != 0)
            {
                types = operation.PerformSkip(types, dm.Skip);   //Paging
            }
            if (dm.Take != 0)
            {
                types = operation.PerformTake(types, dm.Take);
            }
            //if (dm.RequiresCounts)
            {
                return Json(new { result = types, count = count });
            }
            //else
            {
              //  return Json(types);
            }

            //return new JsonResult { Data = new { Items=types.OrderBy(o=>o.SortOrder), count=types.Count() }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public async Task<JsonResult> GetFeeGroupByClass(int campusId, int classId)
        {
            var ExistingClassFeeGroup = await db.ClassFeeGroups.FirstOrDefaultAsync(w => w.CampusID == campusId && w.ClassID == classId);
            if (ExistingClassFeeGroup != null)
            {
                var fee = await db.FeeGroups.FirstOrDefaultAsync(w => w.ID == ExistingClassFeeGroup.FeeGroupID);
                if (fee != null)
                {
                    return Json(new { status = true, message = new { fee.ID, fee.FeeGroupName } });
                }
            }
            return Json(new { status = false, message = "No fee structure found." });
        }

        //Class Structure end

        //***************Add Fee Type******************

        public IActionResult FeeTypeIndex()
        {
            return View();
        }


        public IActionResult AddFeeType()
        {
            return View();
        }

        [HttpPost]
        public async Task<string> AddFeeType(FeeType feeType)
        {
            if (Request.Form["TypeName"].ToString() == null || Request.Form["TypeName"].ToString() == "") return "Invalid Fee Type Name";
            var feetypeName = Request.Form["TypeName"].ToString();
            var note = "";
            if (Request.Form["Notes"].ToString() != null || Request.Form["Notes"] != "") note = Request.Form["Notes"].ToString();
            var sortorder = int.Parse(Request.Form["SortOrder"].ToString());
            var Allodiscount = !string.IsNullOrEmpty(Request.Form["AllowDiscount"].ToString());
            var IsOptional = !string.IsNullOrEmpty(Request.Form["IsOptional"].ToString());
            var FeeType = db.FeeTypes.ToList().Where(w => w.TypeName.ToLower().Trim() == feetypeName.ToLower().Trim())
                .FirstOrDefault();
            if (FeeType != null) return "Fee Type already exist.";
            try
            {
                FeeType = new FeeType();
                FeeType.Notes = note;
                FeeType.TypeName = feetypeName;
                FeeType.AllowDiscount = Allodiscount;
                FeeType.AllowEdit = true;
                FeeType.SortOrder = sortorder;
                FeeType.IsOptional = IsOptional;
                db.FeeTypes.Add(FeeType);

                await db.SaveChangesAsync();
                return "success";
            }
            catch (Exception cc)
            {
                return cc.Message;
            }
        }

        public IActionResult GetFeeTypes(DataManagerRequest dm)
        {
            var types = db.FeeTypes.AsNoTracking().ToList().Select(s => new
            {
                s.Id,
                s.TypeName,
                s.AllowDiscount,
                s.IsOptional,
                s.AllowEdit,
                s.SortOrder,
                s.Notes,
            });

            DataOperations operation = new DataOperations();
            if (dm.Search != null && dm.Search.Count > 0)
            {
                types = operation.PerformSearching(types, dm.Search);  //Search
            }
            if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
            {
                types = operation.PerformSorting(types, dm.Sorted);
            }
            else
            {
                types = operation.PerformSorting(types, new List<Sort>() { new Sort { Name = "SortOrder", Direction = "ascending" } });
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                types = operation.PerformFiltering(types, dm.Where, dm.Where[0].Operator);
            }
            int count = types.Count();
            if (dm.Skip != 0)
            {
                types = operation.PerformSkip(types, dm.Skip);   //Paging
            }
            if (dm.Take != 0)
            {
                types = operation.PerformTake(types, dm.Take);
            }
            if (dm.RequiresCounts)
            {
                return Json(new { result = types, count = count });
            }
            else
            {
                return Json(types);
            }
        }
        //*************Transport Fee Edit************
        public IActionResult TransportFeeEdit()
        {
            ViewBag.stop = new SelectList(db.StudentsTransports.AsNoTracking().Select(s => new { ID = s.PickPoint, s.PickPoint }).Distinct(), "ID", "PickPoint");
            return View();
        }


        [HttpPost]
        public async Task<string> TransportFeeEdit(int CampusId, string PickPoint, double Fare)
        {
            try
            {
                var all = db.StudentsTransports.Where(w => w.Student.Admissions.Where(a => a.CampuseID == CampusId && !a.IsExpell).Any() && w.PickPoint == PickPoint && !w.IsClosed);
                await all.ForEachAsync((e) =>
                 {
                     e.Fare = Fare;
                 });
                await db.SaveChangesAsync();
                return "successfull";
            }
            catch (Exception ee)
            {
                return ee.InnerException == null ? ee.Message
                    : ee.InnerException.Message;
            }
        }
        //*************Fee Type Edit************

        public ActionResult FeeTypeEdit(int id)
        {
            var feeType = db.FeeTypes.Where(w => (w.Id == id) & (w.AllowEdit == true)).FirstOrDefault();
            ViewBag.AllowDiscount = feeType.AllowDiscount;
            return View(feeType);
        }

        [HttpPost]
        public async Task<string> FeeTypeEdit(FeeType feeType)
        {
            var exist = db.FeeTypes.Find(feeType.Id);
            if (exist.AllowEdit.HasValue & !exist.AllowEdit.Value)
            {
                return "You are not allowed to edit this fee type.";
            }
            var isoptional = !string.IsNullOrEmpty(Request.Form["IsOptional"].ToString());
            if (exist.AllowEdit.HasValue && exist.AllowEdit.Value)
                try
                {
                    var Allodiscount = !string.IsNullOrEmpty(Request.Form["AllowDiscount"].ToString());
                    exist.AllowDiscount = Allodiscount;
                    exist.Notes = feeType.Notes;
                    exist.SortOrder = feeType.SortOrder;
                    exist.TypeName = feeType.TypeName;
                    exist.IsOptional = isoptional;
                    await db.SaveChangesAsync();
                    return "success";
                }
                catch (Exception aa)
                {
                    return aa.Message;
                }
            return "This fee type cannot be modified.";
        }

        //***********Fee type Delete************

        public IActionResult FeeTypeDelete(int? id, bool? saveChangesError = false)
        {
            if (saveChangesError.GetValueOrDefault())
                ViewBag.ErrorMessage =
                    "Delete failed. Try again, and if the problem persists see your system administrator.";
            var feeType = db.FeeTypes.Find(id);

            if (feeType == null) return NotFound();
            if (feeType.AllowEdit.HasValue && feeType.AllowEdit.Value)
            {
            }

            return View(feeType);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public string FeeTypeDelete(int id)
        {
            try
            {
                var feeType = db.FeeTypes.Find(id);
                if (feeType.AllowEdit.HasValue && feeType.AllowEdit.Value)
                {
                    db.FeeTypes.Remove(feeType);
                    db.SaveChanges();

                    return "success";
                }

                return "This fee type cannot be deleted.";
            }
            catch (Exception a)
            {
                return a.Message;
            }
        }

        //***************Monthly Fee Slip Start*************

        public IActionResult FeeSlipIndex()
        {
            var jobs = JobStorage.Current.GetMonitoringApi().ProcessingJobs(0, 1000);
            //var jobs = jobStorage.Current.GetMonitoringApi().ProcessingJobs(0, 1000);  

            var exist = jobs.Find(f => f.Value.Job.Method.Name == "AddFeeSlip");
            ViewBag.exist = exist;
            return View();
        }


        [HttpPost]
        public string AddFeeSlip(FeeSlipViewModel feeSlip)
        {

            var jobs = JobStorage.Current.GetMonitoringApi().ProcessingJobs(0, 1000);
            var exist = jobs.Find(f => f.Value.Job.Method.Name == "AddFeeSlip");
            if (exist.Key != null & exist.Value != null)
            {
                return "Already running this task from somewhere. Please wait to complete the task execution.";
            }
            var str = new StringBuilder();
            var message = _resource.QeuedMessage;

            int[] classids;
            int[] campusids;

            if (feeSlip.CampusId == -1)
                campusids = _user.GetCampusIds();
            else
                campusids = new[] { feeSlip.CampusId };

            if (feeSlip.ClassId == -1)
                classids = db.Classes.Select(s => s.ID).ToArray();
            else
                classids = new[] { feeSlip.ClassId };

            str.AppendLine("Month; " + feeSlip.ForMonth);

            var month = feeSlip._ForMonth.Month;
            var year = feeSlip._ForMonth.Year;

            string id = BackgroundJob.Enqueue(() =>
            _backgroundTask.AddFeeSlip(feeSlip.OverWrite, campusids, classids, month, year, _user.UserID, feeSlip._DueDate, feeSlip._LastFineDate, feeSlip.AddTransport, feeSlip.AddSmsCharges, feeSlip.Fee, feeSlip.SendMessage)
            );

            return message;
        }


        public IActionResult AdvanceFee()
        {
            return View();
        }


        [HttpPost]
        public string AdvanceFee(FeeSlipViewModel feeSlips)
        {
            var message = string.Empty;
            if (!int.TryParse(Request.Form["regno"].ToString(), out var regno)) return "Invalid Registration Number";
            var AdmissionIDs = db.Admissions.Where(w => !w.IsExpell && w.Student.RegistrationNo == regno)
                .Select(s => s.ID).FirstOrDefault();

            var month = Request.Form["month"].ToString().Split(',');

            var year = int.Parse(Request.Form["year"].ToString());
            //DatabaseAccessSql sql = new DatabaseAccessSql(System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);

            for (var i = 0; i < month.Count(); i++)
            {

                var date = $"1-{month[i]}-{year}";

                var forMonth = new DateTime(year, int.Parse(month[i]), 1);
                var dueDate = feeSlips.DueDate;
                var lastFineDate = feeSlips.LastFineDate;
                var formonths = DateTime.Now;
                var firstdate = new DateTime(forMonth.Date.Year, forMonth.Date.Month, 1);
                var lastdate = new DateTime(forMonth.Date.Year, forMonth.Date.Month, 1).AddMonths(1).AddDays(-1);

                var startdate = new SqlParameter("startDate", firstdate);
                var enddate = new SqlParameter("endDate", lastdate);
                var admissionIds = new SqlParameter("admissionIds", string.Join(",", AdmissionIDs));
                var collection = new List<SqlParameter>();
                collection.Add(startdate);
                collection.Add(enddate);
                collection.Add(admissionIds);


                var data = dba.ExecuteSP("GetFeeEntryStrucure", collection);

                if (data.Rows.Count == 0)
                {
                    message = "No Fee Strucure Found ";
                    return message;
                }

                var ids = data.AsEnumerable().Select(s => s["AdmissionID"]).Distinct().ToArray();
                var OverWrite = bool.Parse(Request.Form["OverWrite"].ToString());
                //var spLedgerPosting = new LedgerPostingSP();
                //decimal VoucherTypeId = (int) PublicVariables.EnumVoucherTypeDefaults.Fee_Issued_Voucher;
                var editMode = false;
                editMode = feeSlips != null && OverWrite;
                var id = int.Parse(ids[0].ToString().ToString().Trim());
                var rows = data.AsEnumerable().Where(w => w["AdmissionID"].ToString().Trim() == id.ToString().Trim());
                var fineId = (int)PublicVariables.FeeTypes.AttendanceFine;
                var feeslip = db.FeeSlips.FirstOrDefault(w => w.AdmissionId == AdmissionIDs & w.ForMonth.Month == forMonth.Month & w.ForMonth.Year == forMonth.Year);
                if (feeslip != null)
                {
                    if (!feeSlips.OverWrite)
                        continue;
                }
                else
                {
                    feeslip = new FeeSlip();
                    feeslip.EntryDate = DateTime.Now;
                    feeslip.AdmissionId = AdmissionIDs;
                    db.FeeSlips.Add(feeslip);
                }

                feeslip.UserId = _user.UserID;
                feeslip.ForMonth = forMonth;
                feeslip.DueDate = feeSlips._DueDate;
                feeslip.LastFineDate = feeSlips._DueDate;

                var AttendanceFine = decimal.Parse(rows.FirstOrDefault()["AttendanceFine"].ToString());
                var FinesSlip = feeslip.FeeSlipDetails.Where(w => w.FeeTypeId == fineId).FirstOrDefault();
                var addFine = false;
                if (AttendanceFine > 0)
                {

                    if (FinesSlip == null)
                    {
                        FinesSlip = new FeeSlipDetail();
                        feeslip.FeeSlipDetails.Add(FinesSlip);
                        addFine = true;
                    }
                    else if (OverWrite)
                    {
                        addFine = true;
                    }

                    if (addFine)
                    {

                        FinesSlip.Amount = AttendanceFine;
                        FinesSlip.Discount = 0;// decimal.Parse(rows.FirstOrDefault()["Discount"].ToString());
                        FinesSlip.DiscountInAmount = false;
                        //bool.Parse(rows.FirstOrDefault()["DiscountInAmount"].ToString());
                        FinesSlip.FeeTypeId = (int)PublicVariables.FeeTypes.AttendanceFine;
                    }

                }

                if (feeSlips.AddTransport)
                {
                    var transportAmount = decimal.Parse(rows.FirstOrDefault()["TransportFare"].ToString());
                    var trasnId = (int)PublicVariables.FeeTypes.Transport;
                    var TransportSlip = feeslip.FeeSlipDetails.Where(w => w.FeeTypeId == trasnId).FirstOrDefault();
                    if (transportAmount > 0)
                    {
                        addFine = false;
                        if (TransportSlip == null)
                        {
                            TransportSlip = new FeeSlipDetail();
                            feeslip.FeeSlipDetails.Add(TransportSlip);
                            addFine = true;
                        }
                        else if (OverWrite)
                        {
                            addFine = true;
                        }

                        if (addFine)
                        {
                            TransportSlip.Amount = decimal.Parse(rows.FirstOrDefault()["TransportFare"].ToString());

                            TransportSlip.Discount = decimal.Parse(rows.FirstOrDefault()["Discount"].ToString());
                            TransportSlip.DiscountInAmount =
                                bool.Parse(rows.FirstOrDefault()["DiscountInAmount"].ToString());
                            TransportSlip.FeeTypeId = (int)PublicVariables.FeeTypes.Transport;
                        }
                    }
                }

                if (feeSlips.AddSmsCharges)
                {
                    var amount = decimal.Parse(rows.FirstOrDefault()["SMSRate"].ToString());
                    var smsid = (int)PublicVariables.FeeTypes.SMSCharges;

                    var SMSCharges = feeslip.FeeSlipDetails.Where(w => w.FeeTypeId == smsid).FirstOrDefault();
                    if (amount > 0 || SMSCharges != null)
                    {
                        //decimal smsCharges = decimal.Parse(rows.FirstOrDefault()["SMSRate"].ToString());


                        addFine = false;
                        if (SMSCharges == null)
                        {
                            SMSCharges = new FeeSlipDetail();
                            feeslip.FeeSlipDetails.Add(SMSCharges);
                            addFine = true;
                        }
                        else if (OverWrite)
                        {
                            addFine = true;
                        }

                        if (addFine)
                        {
                            SMSCharges.Amount = amount;
                            SMSCharges.Discount = decimal.Parse(rows.FirstOrDefault()["Discount"].ToString());
                            SMSCharges.DiscountInAmount =
                                bool.Parse(rows.FirstOrDefault()["DiscountInAmount"].ToString());
                            SMSCharges.FeeTypeId = (int)PublicVariables.FeeTypes.SMSCharges;
                        }
                    }
                }

                for (var item = 0; item < feeSlips.Fee.Where(w => w.Select).Count(); item++)
                {
                    decimal amount = 0;
                    var optionalexistings = feeslip.FeeSlipDetails.ToList()
                        .Where(w => w.FeeTypeId == feeSlips.Fee[item].Id).FirstOrDefault();
                    addFine = false;
                    if (optionalexistings == null)
                    {
                        optionalexistings = new FeeSlipDetail();
                        feeslip.FeeSlipDetails.Add(optionalexistings);
                        addFine = true;
                        amount = feeSlips.Fee[item].Amount;
                        addFine = true;
                    }
                    else if (OverWrite)
                    {
                        addFine = true;
                    }

                    if (addFine)
                    {
                        optionalexistings.Amount = amount;
                        optionalexistings.Discount = 0;
                        optionalexistings.DiscountInAmount = false;
                        optionalexistings.FeeTypeId = feeSlips.Fee[item].Id;
                    }
                }

                foreach (var row in rows)
                {
                    var amount = decimal.Parse(row["ActualAmount"].ToString());
                    var feeTypeId = int.Parse(row["FeeTypeId"].ToString());
                    var continues = false;
                    for (var it = 0; it < feeSlips.Fee.Where(w => w.Select).Count(); it++)
                        if (feeTypeId == feeSlips.Fee[it].Id ||
                            feeTypeId == (int)PublicVariables.FeeTypes.Transport ||
                            feeTypeId == (int)PublicVariables.FeeTypes.SMSCharges ||
                            feeTypeId == (int)PublicVariables.FeeTypes.AttendanceFine)
                        {
                            continues = true;
                            break;
                        }

                    if (continues) continue;
                    var existings = feeslip.FeeSlipDetails.Where(w => w.FeeTypeId == feeTypeId).FirstOrDefault();
                    if (amount > 0)
                    {
                        addFine = false;
                        if (existings == null)
                        {
                            existings = new FeeSlipDetail();
                            feeslip.FeeSlipDetails.Add(existings);
                            addFine = true;
                        }
                        else if (OverWrite)
                        {
                            addFine = true;
                        }

                        if (addFine)
                        {
                            existings.Amount = amount;
                            existings.Discount = decimal.Parse(row["Discount"].ToString());
                            existings.DiscountInAmount = bool.Parse(row["DiscountInAmount"].ToString());
                            existings.FeeTypeId = feeTypeId;
                        }
                    }
                }

                //if (editMode)
                //    spLedgerPosting.FeeLedgerPostingF(feeslip.FeeSlipDetails.Sum(s => s.Amount), feeSlips.Id,
                //        feeSlips._EntryDate, VoucherTypeId);

                int VoucherTypeId = (int)PublicVariables.EnumVoucherTypeDefaults.Fee_Issued_Voucher;
                _ledgerPosting.FeeLedgerPostingF(feeslip.FeeSlipDetails.Sum(s => s.Amount), feeslip.Id,
                   feeslip.EntryDate, VoucherTypeId);
            }

            try
            {
                db.SaveChanges();
                message = "success";
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return message;
        }

        //************Edit fee slip****************

        public IActionResult FeeSlipEdit(int id)
        {
            var select = db.Admissions.Select(s => new { s.ID, s.Campus.CampusName }).ToList();
            var list = new SelectList(select, "ID", "CampusName");
            ViewBag.Admision = list;
            ViewBag.datetime = DateTime.Now;
            ViewBag.Userid = _user.UserID;
            return View();
        }


        [HttpPost]
        public string FeeSlipEdit(FeeSlip feeSlip)
        {
            var message = string.Empty;
            var select = db.Admissions.Select(s => new { s.ID, s.Campus.CampusName }).ToList();
            var list = new SelectList(select, "ID", "CampusName");
            ViewBag.Admision = list;
            ViewBag.datetime = DateTime.Now;
            ViewBag.Userid = _user.UserID;
            db.Entry(feeSlip).State = EntityState.Modified;
            try
            {
                db.SaveChanges();

                int VoucherTypeId = (int)PublicVariables.EnumVoucherTypeDefaults.Fee_Issued_Voucher;
                _ledgerPosting.FeeLedgerPostingF(feeSlip.FeeSlipDetails.Sum(s => s.Amount), feeSlip.Id,
                   feeSlip.EntryDate, VoucherTypeId);

                message = "success";
            }
            catch (Exception aa)
            {
                message = aa.Message;
            }

            return message;
        }


        public IActionResult FeeSlipDeleteView(int id)
        {

            var feeslip = db.FeeSlips.Find(id);
            if (feeslip == null)
            {
                return NotFound();
            }
            return View(feeslip);
        }

        [HttpPost]
        public string FeeSlipDelete(int id)
        {
            var message = string.Empty;
            try
            {
                var feeslip = db.FeeSlips.Find(id);
                var receitps = db.FeeSlipReceipts.Where(w => w.FeeSlipId == id);
                if ((receitps != null) & (receitps.Count() > 0))
                {
                    db.FeeSlipReceipts.RemoveRange(receitps);
                    foreach (var rec in receitps)
                    {
                        //To Do ledger posting remove
                    }
                }
                var details = db.FeeSlipDetails.Where(w => w.FeeSlipId == id);
                if ((details != null) & (details.Count() > 0)) db.FeeSlipDetails.RemoveRange(details);
                db.FeeSlips.Remove(feeslip);

                db.SaveChanges();



                message = "success";
            }
            catch (Exception ss)
            {
                message = ss.Message;
            }

            return message;
        }


        public IActionResult MonthlyFeeList(DataManagerRequest dm, string month, int campusId, int classid)
        {
            var _month = DateTimeHelper.ConvertDate(month.Trim(), true);
            var monthint = _month.Month;
            var year = _month.Year;
            int[] cids = campusId == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new int[] { campusId };
            int[] clsids = classid == -1 ? db.Classes.Select(s => s.ID).ToArray() : new int[] { classid };
            var types = db.FeeSlips
                .Where(w => clsids.Contains(w.Admission.ClassSection.ClassID) & cids.Contains(w.Admission.CampuseID) && w.ForMonth.Month == monthint && w.ForMonth.Year == year)
                .Select(s => new
                {
                    s.Id,
                    s.Admission.Student.RegistrationNo,
                    s.Admission.Student.StudentName,
                    s.ForMonth,
                    Amount = s.FeeSlipDetails.Sum(ss => ss.Amount),
                });
            DataOperations operation = new DataOperations();
            if (dm.Search != null && dm.Search.Count > 0)
            {
                types = operation.PerformSearching(types, dm.Search);  //Search
            }
            if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
            {
                types = operation.PerformSorting(types, dm.Sorted);
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {

                types = operation.PerformFiltering(types, dm.Where, dm.Where[0].Operator);
            }
            int count = types.Count();
            if (dm.Sorted == null || dm.Sorted.Count == 0)
            {
                Sort srt = new Sort();
                srt.Name = "RegistrationNo";
                srt.Direction = "ascending";
                types = operation.PerformSorting(types, new List<Sort>() { srt });
            }
            if (dm.Skip != 0)
            {

                types = operation.PerformSkip(types, dm.Skip);   //Paging
            }
            if (dm.Take != 0)
            {
                types = operation.PerformTake(types, dm.Take);
            }
            if (dm.RequiresCounts)
            {
                return Json(new { result = types, count = count });
            }
            else
            {
                return Json(types);
            }

        }

        public IActionResult MonthlyFeeSlipOfStudent(DataManagerRequest dm, int regno)
        {
            var campusId = _user.GetCampusIds();

            var year = db.FinancialYears.Where(w => w.IsCurrent).FirstOrDefault().toDate.Year;

            var adId = db.Admissions.Where(w => w.Student.RegistrationNo == regno && w.IsExpell == false)
                .Select(s => s.ID).FirstOrDefault();

            var types = db.FeeSlips.Where(w => w.AdmissionId == adId && w.ForMonth.Year == year).Select(s => new
                {
                    s.Id,
                    s.Admission.Student.RegistrationNo,
                    s.Admission.Student.StudentName,
                    s.ForMonth,
                    Amount = s.FeeSlipDetails.Sum(ss => ss.Amount)
                });
            DataOperations operation = new DataOperations();
            if (dm.Search != null && dm.Search.Count > 0)
            {
                types = operation.PerformSearching(types, dm.Search);  //Search
            }
            if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
            {
                types = operation.PerformSorting(types, dm.Sorted);
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                types = operation.PerformFiltering(types, dm.Where, dm.Where[0].Operator);
            }
            int count = types.Count();
            if (dm.Skip != 0)
            {
                types = operation.PerformSkip(types, dm.Skip);   //Paging
            }
            if (dm.Take != 0)
            {
                types = operation.PerformTake(types, dm.Take);
            }
            if (dm.RequiresCounts)
            {
                return Json(new { result = types.OrderBy(o => o.ForMonth), count = count });
            }
            else
            {
                return Json(types.OrderBy(o => o.ForMonth));
            }
        }

        //Monthly Fee Slip end

        //*******************Add Fee Group******************

        public IActionResult AddFeeGroup()
        {
            return View();
        }


        [HttpPost]
        public string AddFeeGroup(FeeGroup fee)
        {
            var message = string.Empty;
            var feegroupName = Request.Form["FeeGroupName"].ToString();
            if (db.FeeGroups.Any(w => w.FeeGroupName == feegroupName) || feegroupName.ToLower() == "F".ToLower())
                return "Fee group name is already exist";



            fee = new FeeGroup();
            fee.FeeGroupName = feegroupName;
            db.FeeGroups.Add(fee);

            try
            {
                db.SaveChanges();
                message = "success";
            }
            catch (Exception aa)
            {
                message = aa.Message;
                //ModelState.AddModelError("", aa.Message);
            }

            return message;
        }

        //*************Fee group Index***********

        public IActionResult FeeGroupIndex()
        {
            ViewBag.FeeGroups = _selectListHelper.GetSectionFeeGroupList();
            return View();
        }

        public IActionResult FeeGroupList(DataManagerRequest dm)
        {
            var types = db.FeeGroups.AsNoTracking().ToList().Select(s => new
            {
                s.ID,
                s.FeeGroupName
            });

            DataOperations operation = new DataOperations();
            if (dm.Search != null && dm.Search.Count > 0)
            {
                types = operation.PerformSearching(types, dm.Search);  //Search
            }
            if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
            {
                types = operation.PerformSorting(types, dm.Sorted);
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                types = operation.PerformFiltering(types, dm.Where, dm.Where[0].Operator);
            }
            int count = types.Count();
            if (dm.Skip != 0)
            {
                types = operation.PerformSkip(types, dm.Skip);   //Paging
            }
            if (dm.Take != 0)
            {
                types = operation.PerformTake(types, dm.Take);
            }
            if (dm.RequiresCounts)
            {
                return Json(new { result = types, count = count });
            }
            else
            {
                return Json(types);
            }

            //return new JsonResult { Data = new { Items=types.OrderBy(o=>o.SortOrder), count=types.Count() }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public IActionResult EditFeeGroup(int id)
        {
            var fee = db.FeeGroups.Where(w => w.ID == id).FirstOrDefault();
            return View(fee);
        }
        [HttpPost]
        public string EditFeeGroup(FeeGroup feeGroup)
        {
            var message = string.Empty;
            db.Entry(feeGroup).State = EntityState.Modified;
            try
            {
                db.SaveChanges();
                message = "success";
            }
            catch (Exception aa)
            {
                message = aa.Message;
            }

            return message;
        }

        //********************Delete Fee Group**********************

        public IActionResult FeeGroupDelete(int? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
            }

            if (saveChangesError.GetValueOrDefault())
                ViewBag.ErrorMessage =
                    "Delete failed. Try again, and if the problem persists see your system administrator.";
            var feeGroup = db.FeeGroups.Find(id);
            if (feeGroup == null) return NotFound();
            return View(feeGroup);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult FeeGroupDelete(int id)
        {
            try
            {
                var feeGroup = db.FeeGroups.Find(id);
                db.FeeGroups.Remove(feeGroup);
                db.SaveChanges();
            }
            catch
            {
            }

            var model = db.FeeGroups.ToList();

            return RedirectToAction("FeeGroupIndex", model);
        }

        /// <summary>
        ///     Student Edit Fee Start
        /// </summary>
        /// <returns></returns>

        public IActionResult EditFeeIndex(int id = 0)
        {
            if (id > 0)
            {
                ViewBag.regNo = db.FeeSlips.Where(w => w.Id == id).Select(s => s.Admission.Student.RegistrationNo).FirstOrDefault();
                ViewBag.FeeSlipId = id;
            }
            ViewBag.PaymentId = _selectListHelper.GetPaymentSelectList();
            return View();
        }
        [HttpPost]
        public IActionResult EditFeeIndex(FeeSlipDetail feeSlipDetail)
        {
            return View();
        }

        /// <summary>
        ///     End of Student fee or Fee Edits
        /// </summary>
        /// <param name="id"></param>
        /// <param name="saveChangesError"></param>
        /// <returns></returns>

        public IActionResult FeeSlipDetailDelete(int? id, bool? saveChangesError = false)
        {
            if (saveChangesError.GetValueOrDefault())
                ViewBag.ErrorMessage =
                    "Delete failed. Try again, and if the problem persists see your system administrator.";
            var FeeSlipDetail = db.FeeSlipDetails.Find(id);
            if (FeeSlipDetail == null) return NotFound();
            return View(FeeSlipDetail);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public string FeeSlipDetailDelete(int id)
        {
            var message = string.Empty;
            try
            {
                var feeslip = db.FeeSlipDetails.Find(id);
                db.FeeSlipDetails.Remove(feeslip);
                db.SaveChanges();
                var spLedgerPosting = _ledgerPosting;
                var FeeSlip = db.FeeSlips.Find(feeslip.FeeSlipId);
                spLedgerPosting.FeeLedgerPostingF(FeeSlip.FeeSlipDetails.Sum(s => s.Amount), FeeSlip.Id,
                    FeeSlip.EntryDate, (int)PublicVariables.EnumVoucherTypeDefaults.Fee_Issued_Voucher);
                message = "success";
            }
            catch (Exception aa)
            {
                message = aa.InnerException == null ? aa.Message : aa.InnerException.Message;
            }
            return message;
        }


        public IActionResult FeeSlipDetailEdit(int? id)
        {
            var FeeSlipDetail = db.FeeSlipDetails.Find(id);

            if (FeeSlipDetail == null) return NotFound();
            return View(FeeSlipDetail);
        }


        [HttpPost]
        public string FeeSlipDetailEdit(FeeSlipDetail feeSlipDetail)
        {
            var message = string.Empty;
            var detailtid = Request.Form["id"];
            var id = int.Parse(detailtid);
            var amount = feeSlipDetail.Amount;
            var discount = feeSlipDetail.Discount;
            var feeSlipDetails = db.FeeSlipDetails.Where(w => w.Id == id).FirstOrDefault();
            if (feeSlipDetails != null)
            {
                feeSlipDetails.Amount = amount;
                feeSlipDetails.Discount = discount;
                feeSlipDetails.DiscountInAmount =
                    !string.IsNullOrEmpty(Request.Form["DiscountInAmount"].ToString()); //feeSlipDetail.DiscountInAmount;
                db.Entry(feeSlipDetails).State = EntityState.Modified;
            }

            try
            {

                db.SaveChanges();
                // var spLedgerPosting = new LedgerPostingSP();
                var FeeSlip = db.FeeSlips.Find(feeSlipDetails.FeeSlipId);
                _ledgerPosting.FeeLedgerPostingF(FeeSlip.FeeSlipDetails.Sum(s => s.Amount), FeeSlip.Id,
                    FeeSlip.EntryDate, (int)PublicVariables.EnumVoucherTypeDefaults.Fee_Issued_Voucher);
                message = "success";
            }
            catch (Exception aa)
            {
                message = aa.InnerException == null ? aa.Message : aa.InnerException.Message;
            }

            return message;
        }


        public IActionResult FeeSlipReceiptDelete(int? id, bool? saveChangesError = false)
        {
            if (id == null) return NotFound();
            if (saveChangesError.GetValueOrDefault())
                ViewBag.ErrorMessage =
                    "Delete failed. Try again, and if the problem persists see your system administrator.";
            var FeeSlipReceipt = db.FeeSlipReceipts.Find(id);
            if (FeeSlipReceipt == null) return NotFound();
            return View(FeeSlipReceipt);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public string FeeSlipReceiptDelete(int id)
        {
            var message = string.Empty;
            try
            {
                var feeslip = db.FeeSlipReceipts.Find(id);
                db.FeeSlipReceipts.Remove(feeslip);
                db.SaveChanges();
                // var spLedgerPosting = new LedgerPostingSP();
                var FeeSlip = db.FeeSlips.Find(feeslip.FeeSlipId);
                _ledgerPosting.FeeLedgerPostingF(FeeSlip.FeeSlipReceipts.Sum(s => s.Amount), FeeSlip.Id,
                    FeeSlip.EntryDate, (int)PublicVariables.EnumVoucherTypeDefaults.Fee_Receipt_Voucher);

                message = "success";
            }
            catch (Exception ss)
            {
                message = ss.InnerException == null ? ss.Message : ss.InnerException.Message;
            }

            return message;
        }


        public IActionResult FeeSlipReceiptEdit(int? id)
        {
            var FeeSlipReceipt = db.FeeSlipReceipts.Where(w => w.Id == id).FirstOrDefault();
            ViewBag.PaymentId = _selectListHelper.GetPaymentSelectList(FeeSlipReceipt.PaymentMethodId);

            if (FeeSlipReceipt == null) return NotFound();
            return View(FeeSlipReceipt);
        }


        [HttpPost]
        public string FeeSlipReceiptEdit(FeeSlipReceipt feeSlipReceipt)
        {
            var message = string.Empty;
            ViewBag.PaymentId = _selectListHelper.GetPaymentSelectList();
            var reiceptid = Request.Form["id"].ToString();
            var id = int.Parse(reiceptid);
            var Amount = feeSlipReceipt.Amount;
            //int id = int.Parse(dr.Cells["ReceiptId"].Value.ToString());

            var feePayments = db.FeeSlipReceipts.Where(w => w.Id == id).FirstOrDefault();
            if (feePayments != null)
            {
                feePayments.Amount = Amount;
                feePayments.EditBy = _user.UserID;
                feePayments.EditeOn = DateTime.Now;
                feePayments.PaymentMethodId = feeSlipReceipt.PaymentMethodId;
                db.Entry(feePayments).State = EntityState.Modified;
            }

            try
            {
                db.SaveChanges();
                // var spLedgerPosting = new LedgerPostingSP();
                var FeeSlip = db.FeeSlips.Find(feePayments.FeeSlipId);
                _ledgerPosting.FeeLedgerPostingF(FeeSlip.FeeSlipReceipts.Sum(s => s.Amount), FeeSlip.Id,
                    FeeSlip.EntryDate, (int)PublicVariables.EnumVoucherTypeDefaults.Fee_Receipt_Voucher);

                message = "success";
            }
            catch (Exception ss)
            {
                message = ss.InnerException == null ? ss.Message : ss.InnerException.Message;
            }

            return message;
        }

        public IActionResult DiscountorFine()
        {
            return View();
        }
        [HttpPost]
        public IActionResult FeedetailsForDiscountOrFine(DataManagerRequest dm, int campusId = -1, int classid = -1, int typeId = 0, string formonth = "", string amounts = "0")
        {
            var campusIds = campusId <= 0 ? db.Campuses.Select(s => s.ID).ToArray() : new[] { campusId };
            var classIds = classid == -1 ? db.Classes.Select(s => s.ID).ToArray() : new[] { classid };
            var months = DateTimeHelper.ConvertDate(formonth.Trim()); ;
            var types = db.FeeSlips.Where(ww => !ww.Admission.IsExpell &  campusIds.Contains(ww.Admission.CampuseID) &
                    ww.ForMonth.Month == months.Month && ww.ForMonth.Year == ww.ForMonth.Year &&
                    classIds.Contains(ww.Admission.ClassSection.ClassID)).AsNoTracking().Select(s => new
                    {
                        Amount = amounts,
                        s.Id,
                        s.Admission.Student.StudentName,
                        s.Admission.ClassSection.Class.ClassName,
                        s.Admission.Campus.CampusName,
                        s.Admission.Student.RegistrationNo,
                        s.Admission.Student.FName
                    });

            DataOperations operation = new DataOperations();
            if (dm.Search != null && dm.Search.Count > 0)
            {
                types = operation.PerformSearching(types, dm.Search);  //Search
            }
            if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
            {
                types = operation.PerformSorting(types, dm.Sorted);
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                types = operation.PerformFiltering(types, dm.Where, dm.Where[0].Operator);
            }
            int count = types.Count();
            if (dm.Skip != 0)
            {
                if (dm.Sorted == null || dm.Sorted.Count == 0)
                {
                    List<Sort> sort = new List<Sort>() { new Sort { Name = "Id", Direction = "ascending" } };
                    types = operation.PerformSorting(types, sort);
                }

                types = operation.PerformSkip(types, dm.Skip);
            }

            if (dm.Take != 0)
            {
                types = operation.PerformTake(types, dm.Take);
            }
            //if (dm.RequiresCounts)
            //{
                return Json(new { result = types, count = count });
            //}
            //else
            //{
            //    return Json(types);
            //}

        }

        public async Task<string> fineordiscountsave(int[] id, decimal[] amount, int feetype)
        {
            var splitId = id;
            var splitamount = amount;
            try
            {
                for (var i = 0; i < splitId.Count(); i++)
                {
                    var feedetails = new FeeSlipDetail();
                    var existing = db.FeeSlipDetails.ToList()
                            .Where(w => w.FeeTypeId == feetype && w.FeeSlipId == splitId[i])
                            .FirstOrDefault();
                    if (existing == null)
                    {
                        feedetails.Amount = (splitamount[i]);
                        feedetails.FeeSlipId = (splitId[i]);
                        feedetails.FeeTypeId = feetype;
                        feedetails.Discount = 0;
                        feedetails.DiscountInAmount = false;
                        db.FeeSlipDetails.Add(feedetails);
                    }
                    else
                    {
                        existing.Amount = (splitamount[i]);
                        //db.Entry(existing).State = EntityState.Modified;
                    }

                }
                await db.SaveChangesAsync();
                for (var i = 0; i < splitId.Count(); i++)
                {

                    // var spLedgerPosting = new LedgerPostingSP();
                    var FeeSlip = db.FeeSlips.Find(splitId[i]);
                    _ledgerPosting.FeeLedgerPostingF(FeeSlip.FeeSlipDetails.Sum(s => s.Amount), FeeSlip.Id,
                        FeeSlip.EntryDate, (int)PublicVariables.EnumVoucherTypeDefaults.Fee_Issued_Voucher);

                }



                return "success";
            }
            catch (Exception ex)
            {
                return ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            }
        }

        public class ReceiveFeeFromStudentGridSource
        {
            private decimal _received;
            public int? pk { get; set; }
            public int RegNo { get; set; }
            public bool Select { get; set; }
            public string Month { get; set; }
            public decimal ReceiveableAmount { get; set; }


            public decimal LateFee { get; set; }
            public decimal Balance { get; set; }

            public decimal Received
            {
                get => _received;
                set
                {
                    _received = value;
                    Balance = ReceiveableAmount - _received;
                    Select = _received > 0;
                }
            }
        }

        #region

        //******Receive Fee************ 

        public IActionResult RecieveFee()
        {
            ViewBag.receiveData = DateTime.Now.ToString("MM/dd/yyyy");
            return View();
        }

        //*****

        [HttpPost]
        public async Task<JsonResult> RecieveFee(List<FeeReceiptViewModel> slip, bool cbxSend, bool cbxPrint,
            bool cbxDetail, string receivedate)
        {
            DateTime receiveDate = DateTimeHelper.ConvertDate(receivedate);
            // ClsBussinessSetting setting = new ClsBussinessSetting();
            ////Helpers.MessagingSystem ms = new MessagingSystem();
            //var feeReceivedMessageTemplate = string.Empty;
            //    if (cbxSend)
            //    {
            //        var messageTemplate = setting.Read("FeeReceivedMessageTemplate");
            //        if (messageTemplate == null)
            //        {
            //           var PropertyValue =
            //                "Dear {0}({1}),Your Fees Payment of Rs.{2} has been received on {3} at {4}.";
            //            messageTemplate.WriteorUpdate("FeeReceivedMessageTemplate", PropertyValue);
            //        }

            //        feeReceivedMessageTemplate = setting.Read("FeeReceivedMessageTemplate").PropertyValue;
            //    }

            var infoLedgerPost = new List<LedgerPostingInfo>();
            var Messages = new Dictionary<int, string>();
            var paymentMetho = db.FeeSlipPaymentMethods.FirstOrDefault();
            if (paymentMetho == null)
            {
                var url = Url.Action("feepaymentmethod", "School");
                return Json(new
                {
                    Data = new
                    {
                        type = "failede",
                        message =
                            $"Fee payment method is not defined. Please go to this <a href=\"{url}\">link</a> to add a fee payment method."
                    }
                });
            }

            foreach (var r in slip)
            {
                var ThisReceivedAmount = r.Received;
                var regno = r.RegistrationNo;
                var TotalAmount = r.Total;

                var outstandingFees = db.FeeSlips.Where(w =>
                    w.Admission.Student.RegistrationNo == regno && (w.FeeSlipReceipts.Count == 0 ||
                                                                    w.FeeSlipDetails.Select(ss => ss.Amount).Sum() -
                                                                    w.FeeSlipReceipts.Select(ss => ss.Amount).Sum() >
                                                                    0)).OrderBy(o => o.ForMonth).ToList().Select(s =>
                    new
                    {
                        s.Id,
                        Balance = s.FeeSlipDetails.Select(ss => ss.Amount).Sum() -
                                  s.FeeSlipReceipts.Select(ss => ss.Amount).Sum(),
                        s.Admission.CampuseID
                    });

                if (outstandingFees.Count() > 0)
                {
                    if (TotalAmount == ThisReceivedAmount)
                    {
                        var tab = new DataTable();

                        foreach (var dr in outstandingFees)
                        {
                            var receipt = new FeeSlipReceipt();
                            receipt.FeeSlipId = dr.Id; // int.Parse(dr["ID"].ToString());
                            receipt.UserId = _user.UserID;
                            receipt.Amount = dr.Balance;
                            receipt.EntryDate = DateTime.Now;
                            receipt.PaymentMethodId = paymentMetho.Id;

                            db.FeeSlipReceipts.Add(receipt);

                            infoLedgerPost.Add(new LedgerPostingInfo
                            { DetailsId = receipt.FeeSlipId, Debit = receipt.Amount });
                        }
                    }

                    else if (ThisReceivedAmount < TotalAmount)
                    {
                        foreach (var dr in outstandingFees)
                        {
                            var total = dr.Balance; // Convert.ToDecimal(dr["Balance"].ToString());

                            if (total == ThisReceivedAmount || ThisReceivedAmount < total)
                            {
                                //rec.sendmessage = sendmessage;
                                var receipt = new FeeSlipReceipt();
                                receipt.FeeSlipId = dr.Id; // int.Parse(dr["ID"].ToString());
                                receipt.UserId = _user.UserID; // PublicVariables._decCurrentUserId.ToString();
                                receipt.Amount = ThisReceivedAmount;
                                receipt.EntryDate = DateTime.Now;
                                receipt.PaymentMethodId = paymentMetho.Id;

                                db.FeeSlipReceipts.Add(receipt);

                                infoLedgerPost.Add(new LedgerPostingInfo
                                { DetailsId = receipt.FeeSlipId, Debit = receipt.Amount });

                                break;
                            }

                            if (ThisReceivedAmount > total)
                            {
                                var receipt = new FeeSlipReceipt();
                                receipt.FeeSlipId = dr.Id; // int.Parse(dr["ID"].ToString());
                                receipt.UserId = PublicVariables._decCurrentUserId;
                                receipt.Amount = dr.Balance; //  decimal.Parse(dr["Balance"].ToString());
                                receipt.EntryDate = DateTime.Now;
                                receipt.PaymentMethodId = paymentMetho.Id;
                                ;

                                db.FeeSlipReceipts.Add(receipt);

                                infoLedgerPost.Add(new LedgerPostingInfo
                                { DetailsId = receipt.FeeSlipId, Debit = receipt.Amount });
                                ThisReceivedAmount -= total;
                            }
                        }
                    } //end of ThisReceivedAmount < TotalAmount condition

                    var student = db.Students.Where(w => w.RegistrationNo == regno)
                        .Select(s => new { s.ID, s.StudentName }).FirstOrDefault();
                    if (cbxSend)
                    {
                        var substd =
                            dba.CreateTable(
                                "SELECT ReceiveFee, StudentID FROM dbo.ComSubs WHERE (ReceiveFee = 1) AND (StudentID = " +
                                student.ID + ")");
                        if (substd.Rows.Count > 0)
                        {
                            _setting.CampusId = outstandingFees.FirstOrDefault().CampuseID;
                            var feeReceivedMessageTemplate = _setting.Read("FeeReceivedMessageTemplate")?.PropertyValue;
                            if (!string.IsNullOrEmpty(feeReceivedMessageTemplate))
                            {
                                var message = string.Format(feeReceivedMessageTemplate, student.StudentName, regno,
                                  ThisReceivedAmount, DateTime.Now.ToString("dd/MM/yyyy"),
                                  DateTime.Now.ToString("hh:mm:ss tt"));
                                Messages.Add(regno, message);
                            }
                        }
                    }
                }
            }

            ////if(save)
            //byte[] SchoolReceiptReportBytes = null;
            try
            {
                db.SaveChanges();
                // var spLedgerPosting = new LedgerPostingSP();
                foreach (var info in infoLedgerPost)
                    await _ledgerPosting.ReceiveFeeLedgerPostingF(info.Debit, info.DetailsId, DateTime.Now,
                         (int)PublicVariables.EnumVoucherTypeDefaults.Fee_Receipt_Voucher);
                if (cbxSend)
                {
                    var ms = _ms;
                    foreach (var msg in Messages) await ms.sendMessage(msg.Key, msg.Value, false);
                }

                if (cbxPrint || cbxDetail)
                {
                    if (cbxPrint)
                    {
                        //ReportViewer.ReceiptViewer rcp = new ReportViewer.ReceiptViewer(FeeTable, 0);
                        //rcp.StartPosition = FormStartPosition.CenterScreen;
                        //rcp.Show();
                    }

                    if (cbxDetail)
                    {
                        //ReportViewer.ReceiptViewer rcp = new ReportViewer.ReceiptViewer(FeeTable, 1);
                        //rcp.StartPosition = FormStartPosition.CenterScreen;
                        //rcp.Show();
                    }
                }
                return Json(new
                {
                    Data = new { type = "success", message = "Sucessfully stored data." }
                });
                //  return Json(new JsonResult {Data = new {type = "success", message = "Sucessfully stored data."}});
            }
            catch (Exception aa)
            {
                //Log.LogError("Receive by students group", aa);
                //Log.LogError("Receive by students group", aa.InnerException);
                return Json(new
                {
                    Data = new { type = "failede", message = aa.Message }
                });
                //  return Json(new JsonResult {Data = new {type = "failede", message = aa.Message}});

                //ModelState.AddModelError("", aa.Message);
            }

            //return View(slip);
        }

        //Recieve Fee By Indidual Student GetIndividualStdJQ

        public ActionResult RecieveFeeByIndividualStd()
        {
            return View();
        }


        [HttpPost]
        public async Task<string> RecieveFeeByIndividualStd(FeeSlipReceipt feee)
        {
            var campusId = int.Parse(Request.Form["CampusID"].ToString());
            var message = "";
            var cbxSend = !string.IsNullOrEmpty(Request.Form["sendmessage"].ToString());

            var infoLedgerPost = new List<LedgerPostingInfo>();
            var paymentMetho = db.FeeSlipPaymentMethods.FirstOrDefault();
            if (paymentMetho == null)
            {
                var url = Url.Action("feepaymentmethod", "School");
                return
                    $"Fee payment method is not defined. Please go to this <a href=\"{url}\">link</a> to add a fee payment method.";
            }

            decimal totalReceived = 0;
            if (Request.Form["selectrow"].ToString() == null) return "No Row are Selected";
            var selectrow = Request.Form["selectrow"].ToString().Split(',');
            var receivedfee = Request.Form["Received"].ToString().Split(',');
            var pk = Request.Form["pk"].ToString().Split(',');
            var lateFee = Request.Form["LateFee"].ToString().Split(',');

            int latefeeId = await db.FeeTypes.Where(w => w.TypeName == "Late Fee").Select(s => s.Id).FirstOrDefaultAsync();

            for (var i = 0; i < selectrow.Count(); i++)
            {
                if (selectrow[i] == "0") continue;
                if (decimal.TryParse(receivedfee[i], out var receivefee))
                {
                    var id = int.Parse(pk[i]);
                    if (decimal.TryParse(lateFee[i], out var lfee))
                    {
                        var feeSlip = await db.FeeSlipDetails.FirstOrDefaultAsync(w => w.FeeSlipId == id && w.FeeTypeId == latefeeId);
                        if (feeSlip == null)
                            db.FeeSlipDetails.Add(new FeeSlipDetail
                            { Amount = lfee, Discount = 0, DiscountInAmount = false, FeeSlipId = id, FeeTypeId = latefeeId });
                        else
                            feeSlip.Amount = lfee;
                    }

                    var receipt = new FeeSlipReceipt();
                    receipt.FeeSlipId = id; // int.Parse(dr["ID"].ToString());
                    receipt.UserId = _user.UserID;
                    receipt.Amount = receivefee;
                    receipt.EntryDate = DateTimeHelper.ConvertDate(Request.Form["RecievedDate"].ToString(), false, "dd-MM-yyyy");
                    receipt.PaymentMethodId = paymentMetho.Id;
                    db.FeeSlipReceipts.Add(receipt);
                    totalReceived += receipt.Amount;
                    infoLedgerPost.Add(new LedgerPostingInfo { DetailsId = receipt.FeeSlipId, Debit = receipt.Amount });
                }
            }

            ////if(save)
            try
            {
                db.SaveChanges();
                // var spLedgerPosting = new LedgerPostingSP();
                foreach (var info in infoLedgerPost)
                    await _ledgerPosting.ReceiveFeeLedgerPostingF(info.Debit, info.DetailsId, DateTime.Now,
                         (int)PublicVariables.EnumVoucherTypeDefaults.Fee_Receipt_Voucher);
                if (cbxSend)
                {
                    // var dba = new DatabaseAccessSql(true);
                    var regno = int.Parse(Request.Form["regNo"].ToString());
                    var stdid = db.Admissions.Where(w => !w.IsExpell & w.Student.RegistrationNo == regno).Select(s => new { s.StudentID, s.CampuseID }).FirstOrDefault();
                    if (stdid != null)
                    {
                        var substd =
                          dba.CreateTable(
                              "SELECT ReceiveFee, StudentID FROM dbo.ComSubs WHERE (ReceiveFee = 1) AND (StudentID = " +
                              stdid + ")");
                        if (substd.Rows.Count > 0)
                        {
                            var Messages = new ArrayList();
                            //MessagingSystem ms = new MessagingSystem();
                            _setting.CampusId = stdid.CampuseID;
                            var feeReceivedMessageTemplate = _setting.Read("FeeReceivedMessageTemplate")?.PropertyValue;
                            if (!string.IsNullOrEmpty(feeReceivedMessageTemplate))
                            {
                                var messages = string.Format(feeReceivedMessageTemplate, Request.Form["name"], regno.ToString(),
                                  totalReceived, DateTime.Now.ToString("dd/MM/yyyy"), DateTime.Now.ToString("hh:mm:ss tt"));
                                //objdb.SMSApplicationProperties.Where(w => w.PropertyName == "FeeIssuedMesg").FirstOrDefault().PropertyValue.Trim();
                                //  MessagingSystem ms = new MessagingSystem();

                                var ms = _ms;
                                if (ms.error != string.Empty)
                                    await ms.sendMessage(regno, messages, cbxSend);
                            }
                        }
                    }
                }

                message = "success";
            }
            catch (Exception aa)
            {
                message = aa.InnerException == null ? aa.Message : aa.InnerException.Message;
            }
            return message;
        }


        //public IActionResult GetIndividualStdJQ(JqGridRequest request, int regno, string ReceivedDate)
        //{
        //    var campusIds = _user.GetCampusIds();
        //   var receivedate= DateTimeHelper.ConvertDate(ReceivedDate);
        //    var FeeSlipDetail = db.FeeSlips.Where(w =>
        //            campusIds.Contains(w.Admission.CampuseID) && w.Admission.Student.RegistrationNo == regno &&
        //            (w.FeeSlipReceipts.Count == 0 ||
        //             w.FeeSlipDetails.Sum(s => s.Amount) > w.FeeSlipReceipts.Sum(s => s.Amount)))
        //        .OrderBy(o => o.ForMonth).ToList().Select(s => new ReceiveFeeFromStudentGridSource
        //        {
        //            ReceiveableAmount = s.FeeSlipDetails.Sum(su => su.Amount) - s.FeeSlipReceipts.Sum(su => su.Amount),
        //            pk = s.Id,
        //            Month = s.ForMonth.ToString("MMMM, yyyy"),
        //            Received = s.FeeSlipReceipts.Sum(ss => ss.Amount),
        //            Select = false,
        //            LateFee = _helper.GetLateReceivedFeeFine(s.Id, receivedate)
        //        });
        //    ;
        //    var totalRecords = FeeSlipDetail.Count();
        //    //Prepare JqGridData instance
        //    var response = new JqGridResponse
        //    {
        //        //Total pages count
        //        TotalPagesCount = (int) Math.Ceiling(totalRecords / (float) request.RecordsCount),

        //        //Page number
        //        PageIndex = request.PageIndex,
        //        //Total records counts
        //        TotalRecordsCount = totalRecords
        //        //Footer data
        //        //UserData = new { QuantityPerUnit = "Avg Unit Price:", UnitPrice = _productsRepository.GetProducts(filterExpression).Average(p => p.UnitPrice) }
        //    };

        //    IEnumerable<JqGridRecord> data = null;

        //    data = FeeSlipDetail.Select(s => new JqGridRecord(s.pk.ToString(), new
        //    {
        //        s.pk,
        //        s.ReceiveableAmount,
        //        balance =
        //            $"<input type=\"text\" name=\"balance\" id=\"balance\" value=\"{s.ReceiveableAmount + s.LateFee}\" readonly /><input type=\"hidden\" name=\"pk\" id=\"pk\" value=\"{s.pk}\" /><input type=\"hidden\" name=\"receiveable\" id=\"receiveable\" value=\"{s.ReceiveableAmount}\" />",
        //        received = "<input type=\"text\" name=\"Received\" id=\"received\" value=\"0\" readonly />",
        //        s.Month,
        //        LateFee =
        //            $"<input type=\"text\" name=\"LateFee\" id=\"lateFee\" onchange=\"lateFees(this)\" value=\"{s.LateFee}\"  />",
        //        action =
        //            $"<input type=\"checkbox\" name=\"selectrow\" onchange=\"receivebalance(this)\" value=\"0\" id=\"selectss\" {(s.Select ? "checked" : "")} />"
        //    }));
        //    //else
        //    //    data = _context.Employee.OrderByDescending(request.SortingName).Skip(skip).Take(perpage).ToList().Select(s => new JqGridRecord(s.Id.ToString(), new { s.Id, s.EmployeeName, s.Cnic, s.Address, s.MobileNo, s.Email, action = $"<a title=\"Placement\" href=\"{Url.Action("CreatePlacement", "Employees", new { employeeId = s.Id })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-plus\"></i></a> <a title=\"Edit\" href=\"{Url.Action("Edit", "Employees", new { id = s.Id })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-edit\"></i></a> <a title=\"Delete\" href=\"{Url.Action("Delete", "Employees", new { id = s.Id })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-trash\"></i></a>" }));

        //    foreach (var d1 in data) response.Records.Add(d1);
        //    //response.Records = data;
        //    response.Reader.RepeatItems = false;
        //   // return new JqGridJsonResult {Data = response};

        //    var d = Json(new
        //    {
        //        Data = response
        //    });
        //    return d;
        //    //return db.Companies.ToList();
        //}
        public IActionResult OptionalFeeList(DataManagerRequest dm)
        {
            var types = db.FeeTypes.Where(w => w.IsOptional == true).Select(s => new
            {
                s.Id,
                s.TypeName,
                Amount = 0,
            });

            DataOperations operation = new DataOperations();
            if (dm.Search != null && dm.Search.Count > 0)
            {
                types = operation.PerformSearching(types, dm.Search);  //Search
            }
            if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
            {
                types = operation.PerformSorting(types, dm.Sorted);
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                types = operation.PerformFiltering(types, dm.Where, dm.Where[0].Operator);
            }
            int count = types.Count();
            if (dm.Skip != 0)
            {
                types = operation.PerformSkip(types, dm.Skip);   //Paging
            }
            if (dm.Take != 0)
            {
                types = operation.PerformTake(types, dm.Take);
            }
            if (dm.RequiresCounts)
            {
                return Json(new { result = types, count = count });
            }
            else
            {
                return Json(types);
            }
        }


        #endregion
    }
}