using LazZiya.ImageResize;
 using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using smsCore.Controllers;
using smsCore.Data;
using smsCore.Data.Helpers;
using smsCore.Data.Models.ViewModels;
using Syncfusion.EJ2.Base;
using System.Collections;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
 using smsCore.Data.Models;
using HttpPostAttribute = Microsoft.AspNetCore.Mvc.HttpPostAttribute;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using AuthorizeAttribute = Microsoft.AspNetCore.Authorization.AuthorizeAttribute;

namespace sms.Controllers
{
    //[CompressContent]F
    [Authorize]
    public class PayrollController : BaseController
    {
        #region Private Members

        private readonly SchoolEntities db; 
        private readonly CurrentUser _user; 
        private readonly SelectListHelper _selectListHelper;
        private readonly DatabaseAccessSql dba;



        int[] CampusIds;
        #endregion
        public PayrollController(SchoolEntities _db , CurrentUser user , DatabaseAccessSql _dba , SelectListHelper selectListHelper)
        {
            db = _db;
           _user = user;
            dba = _dba;
            _selectListHelper = selectListHelper;
         //   CampusIds = CurrentUser.GetCampusIds();
        }

        #region Designation


        public IActionResult Designation()
        {
            return View();
        }

        [HttpPost]
        public IActionResult DesignationList(DataManagerRequest request)
        {
            var designationslist = db.tbl_Designation.Select(s =>

                    new
                    {
                        s.Id,
                        Name = s.designationName,
                        Narration = s.narration,
                        AdvanceAmount = s.advanceAmount,
                        LeaveDays = s.leaveDays,
                        Employees = s.tbl_Employee.Count
                    });

            return Json(new { result = designationslist, count = designationslist.Count() });
        }


        [HttpPost]
        public string Designation(tbl_Designation designation)
        {
            if (db.tbl_Designation.ToList()
                    .Where(w => w.designationName.ToLower() == designation.designationName.ToLower())
                    .FirstOrDefault() !=
                null) return "Designation already exist";
            decimal.TryParse(Request.Form["ClInMonth"], out var ClInMonth);

            if (ClInMonth > 30)
            {
                return " Maximum CL in a month is 30 ";
            }

            try
            {
                designation.advanceAmount = designation.advanceAmount == null ? 0 : designation.advanceAmount;
                designation.leaveDays = ClInMonth;
                designation.extra1 = string.Empty;
                designation.extra2 = string.Empty;
                designation.extraDate = DateTime.Now;
                db.tbl_Designation.Add(designation);
                db.SaveChanges();
                return "success";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return ModelState.Where(w => w.Value.Errors.Count > 0).FirstOrDefault().Value.Errors.FirstOrDefault()
                .ErrorMessage;
        }


        public IActionResult DesignationEdit(decimal id)
        {
            var designationlist = db.tbl_Designation.Where(w => w.Id == id).FirstOrDefault();
            return View(designationlist);
        }


        [HttpPost]
        public string DesignationEdit(tbl_Designation designation)
        {
            decimal.TryParse(Request.Form["ClInMonth"].ToString(), out var ClInMonth);
            if (ClInMonth > 30)
            {
                return " Maximum CL in a month is 30 ";
            }

            var existdesignation = db.tbl_Designation.Where(w => w.Id == designation.Id)
                .FirstOrDefault();
            if (existdesignation == null) return "failed";
            try
            {
                existdesignation.designationName = designation.designationName;
                existdesignation.narration = designation.narration;
                existdesignation.advanceAmount = designation.advanceAmount == null ? 0 : designation.advanceAmount;
                existdesignation.leaveDays = ClInMonth;
                existdesignation.extra1 = string.Empty;
                existdesignation.extra2 = string.Empty;
                existdesignation.extraDate = DateTime.Now;
                db.SaveChanges();
                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }


        public IActionResult DesignationDelete(decimal id, bool? saveChangesError = false)
        {
            var designationlist = db.tbl_Designation.Where(w => w.Id == id).FirstOrDefault();
            return View(designationlist);
        }


        [HttpPost]
        public string DesignationDelete(decimal id)
        {
            try
            {
                var existdesignation = db.tbl_Designation.Where(w => w.Id == id).FirstOrDefault();
                if (existdesignation == null) return "Already Deleted";
                if (existdesignation.tbl_Employee.Any())
                    return "Employee exist under selected designation. It can't be deleted.";
                db.tbl_Designation.Remove(existdesignation);
                db.SaveChanges();
                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        #endregion

        #region Employee Creation
        public IActionResult EmployeeCreation()
        {
            ViewBag.DriverList = new SelectList(db.Drivers.Select(s => new { s.ID, s.DriverName }), "ID", "DriverName");
            ViewBag.designationList = new SelectList(db.tbl_Designation.Select(s => new { s.Id, s.designationName }).ToList(), "Id", "designationName");
            ViewBag.SalaryPackageList = new SelectList(db.SalaryPackages.Where(w => w.isActive).ToList(), "salaryPackageId",
                "salaryPackageName");
            var model = new EmployeeCreationViewModel();
            model.EmloyeeSubjects = db.Subjects.Select(s => new EmployeeSubject { SubjectId = s.ID, SubjectName = s.SubjectName, Select = false }).ToList();
            return View(model);
        }


        [HttpPost]
        public IActionResult EmployeeCreation(EmployeeCreationViewModel model)
        {
            if (_user.BasicUserType == EnumManager.BasicUserType.Campus)
                model.Employee.CampusID = _user.GetCampusIds().FirstOrDefault();

            if (db.tbl_Employee.Any(w => w.employeeCode == model.Employee.employeeCode))
                return BadRequest("Employee code already exist");

            model.Employee = (tbl_Employee)StudentsController.Validate(model.Employee, new[] { "employeeName", "employeeCode" });

            var employeePhotoFile = Request.Form.Files["EmployeePhotoFile"];
            byte[] bytedata;
            if (employeePhotoFile != null)
            {
                using (var stream = new MemoryStream())
                {
                    var images = Image.FromStream(employeePhotoFile.OpenReadStream());
                    var resized = images.ScaleAndCrop(350, 450);
                    resized.Save(stream, ImageFormat.Jpeg);
                    bytedata = stream.ToArray();
                }

                model.Employee.Photo = bytedata;
            }
            model.Employee.isActive ??= true;
            model.Employee.joiningDate = DateTimeHelper.ConvertDate(Request.Form["Employee.joiningDate"].ToString());
            model.Employee.dob = DateTimeHelper.ConvertDate(Request.Form["Employee.dob"].ToString());

            if (!string.IsNullOrEmpty(Request.Form["Employee.terminationDate"]))
                model.Employee.terminationDate = DateTimeHelper.ConvertDate(Request.Form["Employee.terminationDate"].ToString());

            if (!string.IsNullOrEmpty(Request.Form["Employee.passportExpiryDate"].ToString()))
                model.Employee.passportExpiryDate = DateTimeHelper.ConvertDate(Request.Form["Employee.passportExpiryDate"].ToString());

            if (!string.IsNullOrEmpty(Request.Form["Employee.visaExpiryDate"].ToString()))
                model.Employee.visaExpiryDate = DateTimeHelper.ConvertDate(Request.Form["Employee.visaExpiryDate"].ToString());

            if (!string.IsNullOrEmpty(Request.Form["Employee.labourCardExpiryDate"].ToString()))
                model.Employee.labourCardExpiryDate = DateTimeHelper.ConvertDate(Request.Form["Employee.labourCardExpiryDate"].ToString());

            if (model.Qualifications != null)
                foreach (var qualification in model.Qualifications)
                {
                    if (string.IsNullOrEmpty(qualification.QualificationType) && string.IsNullOrEmpty(qualification.University)) continue;
                    model.Employee.Qualifications.Add(new Qualification
                    {
                        Grade = qualification.Grade,
                        QualificationType = qualification.QualificationType,
                        University = qualification.University,
                        Year = qualification.Year,
                        UserID = _user.UserID
                    });
                }

            if (model.EmployeeExpriences != null)
                foreach (var experience in model.EmployeeExpriences)
                {
                    if (string.IsNullOrEmpty(experience.Institution) || string.IsNullOrEmpty(experience.Desgnition)) continue;

                    model.Employee.EmployeeExpriences.Add(new EmployeeExprience
                    {
                        Desgnition = experience.Desgnition,
                        From = experience.From,
                        To = experience.To,
                        Institution = experience.Institution
                    });
                }

            if (model.EmloyeeSubjects != null)
                foreach (var item in model.EmloyeeSubjects)
                {
                    if (item.Select)
                        model.Employee.EmployeeExperinceSubjects.Add(new EmployeeExperinceSubject { SubjectId = item.SubjectId });
                }

            try
            {
                db.tbl_Employee.Add(model.Employee);
                db.SaveChanges();
                return Ok("success");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        #endregion


        #region Employee Register
        public IActionResult EmployeeRegister()
        {
            ViewBag.designationList = new SelectList(db.tbl_Designation.Select(s => new { s.Id, s.designationName }).ToList(), "Id", "designationName");
            return View();
        }

        public IActionResult EmployeeListInCampus(DataManagerRequest dm, int CampusId = 0)
        {
            int[] campusId = new int[] { CampusId };
            string Userid = _user.UserID;
            if (CampusId == 0)
                campusId = new LoggedInUserHelper(Userid).CampusId;
            var today = DateTime.Today;
            var data = db.tbl_Employee.Where(w => campusId.Contains(w.CampusID)).Select(s => new
            {
                s.Id,
                s.employeeCode,
                s.employeeName,
                s.FatherName,
                s.CNIC,
                s.tbl_Designation.designationName,
                s.salaryType,
                s.mobileNumber,
                s.address,
                s.joiningDate,
                s.terminationDate,// = s.terminationDate.HasValue ? s.terminationDate.Value : DateTime.MinValue,
                Left = s.terminationDate.HasValue && s.terminationDate.Value <= today
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
                    srt.Add(new Sort { Name = "employeeCode", Direction = "ascending" });
                    data = operation.PerformSorting(data, srt);
                }
                data = operation.PerformSkip(data, dm.Skip);   //Paging
            }
            if (dm.Take != 0)
            {
                data = operation.PerformTake(data, dm.Take);
            }
            var m = data.ToList();
            var response = data.ToList().Select(s => new
            {
                s.Id,
                s.employeeCode,
                s.employeeName,
                s.FatherName,
                s.CNIC,
                s.designationName,
                s.salaryType,
                s.mobileNumber,
                s.address,
                terminationDate = s.terminationDate.HasValue ? s.terminationDate.Value.ToString("dd/MM/yyyy") : "",
                joiningDate = s.joiningDate.HasValue ? s.joiningDate.Value.ToString("dd/MM/yyyy") : "",
                s.Left
            });
            return Json(new { result = response, count = count });
        }
        #endregion


        #region Employee Details
        public IActionResult EmployeeDetails(int id)
        {
            var data = db.tbl_Employee.AsNoTracking().Where(w => w.Id == id).FirstOrDefault();
            return View(data);
        }
        #endregion

        #region Employee Edit
        public IActionResult EditEmployee(int id)
        {
            var emp = db.tbl_Employee.Find(id);
            if (emp == null)
                return NotFound();

            EmployeeCreationViewModel model = new EmployeeCreationViewModel();
            model.Employee = emp;
            model.EmployeeExpriences = emp.EmployeeExpriences.ToList();
            model.Qualifications = emp.Qualifications.ToList();
            model.EmloyeeSubjects = db.Subjects.Select(s => new EmployeeSubject { Select = s.EmployeeExperinceSubjects.Where(w => w.EmployeeId == emp.Id).Any(), SubjectId = s.ID, SubjectName = s.SubjectName }).ToList();

            ViewBag.designationList = new SelectList(db.tbl_Designation.Select(s => new { s.Id, s.designationName }).ToList(), "Id", "designationName",
                emp.designationId);
            ViewBag.SalaryPackageList = new SelectList(db.SalaryPackages.Where(w => w.isActive).ToList(), "salaryPackageId",
                "salaryPackageName", emp.defaultPackageId);

            return View(model);
        }


        [HttpPost]
        public string EditEmployee(EmployeeCreationViewModel model)
        {
            var EmployeePhotoFile = Request.Form.Files["EmployeePhotoFile"];
            byte[] bytedata;

            if (EmployeePhotoFile != null)
            {
                var Images = Image.FromStream(EmployeePhotoFile.OpenReadStream());
                var resized = Images.ScaleAndCrop(350, 450);
                using (var Stream = new MemoryStream())
                {
                    resized.Save(Stream, ImageFormat.Jpeg);
                    bytedata = Stream.ToArray();
                }

                model.Employee.Photo = bytedata;
            }

            try
            {
                var em = db.tbl_Employee.Find(model.Employee.Id);
                if (em != null)
                {
                    em.address = model.Employee.address ?? string.Empty;
                    em.bankAccountNumber = model.Employee.bankAccountNumber ?? string.Empty;
                    em.bankName = model.Employee.bankName ?? string.Empty;
                    em.bloodGroup = model.Employee.bloodGroup ?? string.Empty;
                    em.branchCode = model.Employee.branchCode ?? string.Empty;
                    em.branchName = model.Employee.branchName ?? string.Empty;
                    em.CampusID = model.Employee.CampusID;
                    em.CNIC = model.Employee.CNIC ?? string.Empty;
                    em.dailyWage = model.Employee.dailyWage ?? 0;
                    em.defaultPackageId = model.Employee.defaultPackageId;
                    em.designationId = model.Employee.designationId;
                    em.email = model.Employee.email ?? string.Empty;
                    em.employeeCode = model.Employee.employeeCode;
                    em.Id = model.Employee.Id;
                    em.employeeName = model.Employee.employeeName;
                    em.esiNumber = model.Employee.esiNumber ?? string.Empty;
                    em.extra1 = string.Empty;
                    em.extra2 = string.Empty;
                    em.extraDate = DateTime.Now;
                    em.FatherName = model.Employee.FatherName ?? string.Empty;
                    em.gender = model.Employee.gender ?? string.Empty;
                    em.isActive = model.Employee.isActive ?? true;
                    em.joiningDate = model.Employee.joiningDate;
                    em.dob = model.Employee.dob;
                    em.terminationDate = model.Employee.terminationDate;

                    if (!string.IsNullOrEmpty(Request.Form["Employee.terminationDate"].ToString()))
                        em.terminationDate = model.Employee.terminationDate;



                    if (!string.IsNullOrEmpty(Request.Form["Employee.passportExpiryDate"].ToString()))
                        model.Employee.passportExpiryDate =
                        DateTimeHelper.ConvertDate(Request.Form["Employee.passportExpiryDate"].ToString());

                    if (!string.IsNullOrEmpty(Request.Form["Employee.visaExpiryDate"]))
                        model.Employee.visaExpiryDate = DateTimeHelper.ConvertDate(Request.Form["Employee.visaExpiryDate"].ToString());

                    if (!string.IsNullOrEmpty(Request.Form["Employee.labourCardExpiryDate"]))
                        model.Employee.labourCardExpiryDate = DateTimeHelper.ConvertDate(Request.Form["Employee.labourCardExpiryDate"].ToString());

                    model.Employee.isActive = !string.IsNullOrEmpty(Request.Form["IsActive"]);
                    if (model.Employee.salaryType == "Daily wage")
                    {
                        if (model.Employee.dailyWage.ToString().Trim() != string.Empty)
                            model.Employee.dailyWage = Convert.ToDecimal(model.Employee.dailyWage);
                    }
                    else
                    {
                        decimal.TryParse(Request.Form["SalaryPackageId"], out decimal package);
                        model.Employee.defaultPackageId = package;
                    }
                    em.labourCardNumber = model.Employee.labourCardNumber ?? string.Empty;
                    em.maritalStatus = model.Employee.maritalStatus ?? string.Empty;
                    em.mobileNumber = model.Employee.mobileNumber ?? string.Empty;
                    em.narration = model.Employee.narration ?? string.Empty;
                    em.panNumber = model.Employee.panNumber ?? string.Empty;
                    em.passportNo = model.Employee.passportNo ?? string.Empty;
                    em.pfNumber = model.Employee.pfNumber ?? string.Empty;
                    em.phoneNumber = model.Employee.phoneNumber ?? string.Empty;
                    em.Photo = model.Employee.Photo;
                    em.qualification = model.Employee.qualification ?? string.Empty;
                    em.salaryType = model.Employee.salaryType ?? string.Empty;
                    em.Title = model.Employee.Title ?? string.Empty;
                    em.visaNumber = model.Employee.visaNumber ?? string.Empty;
                }
                else
                {
                    return "No record found.";
                }
                //StudentsController studentsController = new StudentsController();
                //em = (tbl_Employee)studentsController.Validate(em, new string[] { "employeeName", "employeeCode" });

                db.Entry(em).State = EntityState.Modified;

                int decEmployeeId = model.Employee.Id;
                if(model.Qualifications !=null)
                for (var i = 0; i < model.Qualifications.Count; i++)
                {
                    if (string.IsNullOrEmpty(model.Qualifications[i].QualificationType) && string.IsNullOrEmpty(model.Qualifications[i].University)) continue;
                    var qual = db.Qualifications.Find(model.Qualifications[i].ID);
                    if (qual == null)
                    {
                        qual = new Qualification();
                        qual.StaffID = decEmployeeId;
                            qual.UserID = _user.UserID;
                        db.Qualifications.Add(qual);
                    }
                    qual.Grade = model.Qualifications[i].Grade;
                    qual.QualificationType = model.Qualifications[i].QualificationType;
                    qual.University = model.Qualifications[i].University;
                    qual.Year = model.Qualifications[i].Year;
                }

                if(model.EmployeeExpriences !=null)
                for (var i = 0; i < model.EmployeeExpriences.Count() - 1; i++)
                {
                    if (string.IsNullOrEmpty(model.EmployeeExpriences[i].Institution) || string.IsNullOrEmpty(model.EmployeeExpriences[i].Desgnition)) continue;
                    var exp = db.EmployeeExpriences.Find(model.EmployeeExpriences[i].ID);
                    if (exp == null)
                    {
                        exp = new EmployeeExprience() { StfID = decEmployeeId };
                        db.EmployeeExpriences.Add(exp);
                    }
                    exp.Desgnition = model.EmployeeExpriences[i].Desgnition;
                    exp.From = model.EmployeeExpriences[i].From;
                    exp.To = model.EmployeeExpriences[i].To;
                    exp.Institution = model.EmployeeExpriences[i].Institution;
                }
                var existings = db.EmployeeExperinceSubjects.Where(w => w.EmployeeId == decEmployeeId);
                db.EmployeeExperinceSubjects.RemoveRange(existings);
                if(model.EmloyeeSubjects !=null )
                foreach (var item in model.EmloyeeSubjects)
                {
                    if (item.Select)
                        db.EmployeeExperinceSubjects.Add(new EmployeeExperinceSubject { EmployeeId = decEmployeeId, SubjectId = item.SubjectId });
                }

                try
                {
                    db.SaveChanges();
                }
                catch (Exception aa)
                {
                    return aa.InnerException==null ? aa.Message : aa.InnerException.Message;
                }

                return "success";
            }
            catch (Exception aa)
            {
                return aa.InnerException==null ? aa.Message : aa.InnerException.Message;
            }
        }
        #endregion

        #region employee delete
        public IActionResult DeleteEmployee(int id)
        {
            var model = new LeavedStaff { StaffID=id };
            return View(model);
        }
        [HttpPost]
        public IActionResult DeleteEmployee(LeavedStaff employee)
        {
            var rm = db.tbl_Employee.Where(w => w.Id == employee.StaffID && w.LeavedStaffs.Count==0).FirstOrDefault();
            if (rm != null)
            {
                var leavingdate = DateTimeHelper.ConvertDate(Request.Form["LeavingDate"].ToString(), true);
                //var employeeExperience = db.EmployeeExpriences.Where(w => w.StfID == rm.Id).ToList();
                //var experienceSubjects =
                //    db.EmployeeExperinceSubjects.Where(w => w.EmployeeId == rm.Id).ToList();
                //var qualification = db.Qualifications.Where(w => w.StaffID == rm.Id).ToList();
                //if (employeeExperience != null)
                //    foreach (var item in employeeExperience)
                //        db.EmployeeExpriences.Remove(item);
                //if (experienceSubjects != null)
                //    foreach (var item in experienceSubjects)
                //        db.EmployeeExperinceSubjects.Remove(item);
                //if (qualification != null)
                //    foreach (var item in qualification)
                //        db.Qualifications.Remove(item);
                //db.tbl_Employee.Remove(rm);
                rm.terminationDate=leavingdate;
                var leaved = new LeavedStaff { LeavingDate=leavingdate, Reason=employee.Reason, StaffID=employee.StaffID, Remarks=employee.Remarks, UserID=_user.UserID };
                db.LeavedStaffs.Add(leaved);
            }
            else
            {
                return Json(new { status = false, message = "Employee record not found." });
            }

            try
            {
                db.SaveChanges();
                return Json(new { status = true, message = "Employee record deleted successfully." });
            }
            catch (Exception aa)
            {
                return Json(new
                {
                    status = false,
                    message = (aa.InnerException==null ? aa.Message : aa.InnerException.Message)
                });
            }
        }
        #endregion

        #region Payhead

        public IActionResult Payhead()
        {
            return View();
        }

        public IActionResult PayHeadList(DataManagerRequest request)
        {
            var payheadlist = db.PayHeads;


            var data = payheadlist.ToList().Select(s => new
            {
                s.payHeadId,
                Name = s.payHeadName,
                Narration = s.narration,
                Type = s.type
            });
            return Json(new { result = data, count = data.Count() });
        }


        [HttpPost]
        public string Payhead(tbl_PayHead payHead)
        {
            if (db.PayHeads.Where(w => w.payHeadName.ToLower() == payHead.payHeadName.ToLower())
                .FirstOrDefault() != null) return "Payhead name already exist";
            payHead.narration = payHead.narration == null ? string.Empty : payHead.narration;
            payHead.UserId = _user.UserID;
            payHead.EntryDate = DateTime.Now;
            try
            {
                db.PayHeads.Add(payHead);
                db.SaveChanges();
                return "success";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return ModelState.Where(w => w.Value.Errors.Count > 0).FirstOrDefault().Value.Errors.FirstOrDefault()
                .ErrorMessage;
        }


        public IActionResult PayheadEdit(decimal id)
        {
            var payhead = db.PayHeads.Where(w => w.payHeadId == id).FirstOrDefault();
            return View(payhead);
        }


        [HttpPost]
        public string PayheadEdit(tbl_PayHead tbl_PayHead)
        {
            var payheadexist = db.PayHeads.Where(w => w.payHeadId == tbl_PayHead.payHeadId).FirstOrDefault();
            if (payheadexist == null) return "failed";
            try
            {
                payheadexist.payHeadName = tbl_PayHead.payHeadName;
                payheadexist.narration = tbl_PayHead.narration;
                payheadexist.type = tbl_PayHead.type;
                payheadexist.UserId = _user.UserID;
                payheadexist.EntryDate = DateTime.Now;
                db.SaveChanges();
                return "success";
            }
            catch (Exception aa)
            {
                ModelState.AddModelError("", aa.InnerException==null ? aa.Message : aa.InnerException.Message);
            }

            return ModelState.Where(w => w.Value.Errors.Count > 0).FirstOrDefault().Value.Errors.FirstOrDefault()
                .ErrorMessage;
        }


        public IActionResult PayheadDelete(decimal id)
        {
            var payheads = db.PayHeads.Where(w => w.payHeadId == id).FirstOrDefault();
            return View(payheads);
        }


        [HttpPost]
        public string PayheadDelete(tbl_PayHead tbl_PayHead)
        {
            var payheads = db.PayHeads.Where(w => w.payHeadId == tbl_PayHead.payHeadId).FirstOrDefault();
            if (payheads == null) return "Already Deleted";
            try
            {
                db.PayHeads.Remove(payheads);
                db.SaveChanges();
                return "delete";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return ModelState.Where(w => w.Value.Errors.Count > 0).FirstOrDefault().Value.Errors.FirstOrDefault()
                .ErrorMessage;
        }

        #endregion

        #region Salary Package

        public IActionResult SalaryPackageCreation()
        {
            return View();
        }

        [HttpPost]
        public string SalaryPackageCreation(SalaryPackageViewModel model)
        {

            var exist = db.SalaryPackages.Where(w => w.salaryPackageName == model.PackageName).Any();
            if (exist)
                return "Salary package with same name already exist.";

            if (!model.PayHeads.Where(w => w.Amount > 0).Any())
            {
                return "Please define atleast one pay head amount.";
            }
            var package = new tbl_SalaryPackage();
            package.isActive = model.IsActive;
            package.narration = model.Narration;
            package.salaryPackageName = model.PackageName;
            package.EntryDate = DateTime.Now;
            package.UserId = _user.UserID;
            foreach (var t in model.PayHeads)
            {
                if (t.Amount > 0)
                {
                    package.SalaryPackageDetails.Add(new tbl_SalaryPackageDetails
                    {
                        amount = t.Amount,
                        payHeadId = t.Id,
                        narration = string.Empty
                    });
                }
            }
            try
            {
                db.SalaryPackages.Add(package);
                db.SaveChanges();
                return "success";
            }
            catch (Exception aa)
            {
                return aa.InnerException==null ? aa.Message : aa.InnerException.Message;
            }
        }

        public IActionResult GetPayheadsInPackage(DataManagerRequest dm,int id)
        {
            var data = db.PayHeads.AsNoTracking().Select(s => new
            {
                s.payHeadId,
               Name = s.payHeadName,
                Type=s.type,
                Amount = s.SalaryPackageDetails.DefaultIfEmpty().Where(w => w.salaryPackageId==id).Select(t => t.amount)
            });


            return Json(new { result = data, count = data.Count() });
        }

        public IActionResult SalaryPackageEdit(int id)
        {
            var exist = db.SalaryPackages.Find(id);
            var salaryPackageVM = new SalaryPackageViewModel
            {
                Id = exist.salaryPackageId,
                Narration = exist.narration,
                PackageName = exist.salaryPackageName,
                IsActive = exist.isActive
            };
            ViewBag.packageId = id;
            return View(salaryPackageVM);
        }

        public decimal GetSalaryEmployee(int EmployeeId)
        {
            var pkgId = db.tbl_Employee.AsNoTracking()
                .Where(w => w.Id == EmployeeId)
                .DefaultIfEmpty()
                .Select(s => s.defaultPackageId)
                .FirstOrDefault();

            if (pkgId > 0)
            {
                var calculatedAmount = db.SalaryPackageDetails
            .Where(w => w.salaryPackageId == pkgId && w.SalaryPackage.isActive).Count() > 0 ?
            db.SalaryPackageDetails.Where(w => w.salaryPackageId == pkgId && w.SalaryPackage.isActive)
            .Sum(s => s.amount) : 0;

                return calculatedAmount;
            }

            return 0;
        }


        [HttpPost]
        public string SalaryPackageEdit(SalaryPackageViewModel model)
        {
            if (!model.PayHeads.Where(w => w.Amount > 0).Any())
            {
                return "Please define atleast one pay head amount.";
            }
            var existInDB = db.SalaryPackages.Find(model.Id);
            if (existInDB != null)
            {
                existInDB.isActive = model.IsActive;
                existInDB.narration = model.Narration;
                existInDB.salaryPackageName = model.PackageName;
                existInDB.EntryDate = DateTime.Now;
                existInDB.UserId = _user.UserID;
            }
            else return "No Salary package record found to edit.";


            foreach (var t in model.PayHeads)
            {
                var exist = db.SalaryPackageDetails.FirstOrDefault(w => w.payHeadId==t.Id && w.salaryPackageId==existInDB.salaryPackageId);
                if (t.Amount==0)
                {
                    if (exist!=null)
                        db.SalaryPackageDetails.Remove(exist);
                }
                else if (t.Amount > 0)
                {
                    if (exist==null)
                    {
                        exist=new tbl_SalaryPackageDetails
                        {
                            salaryPackageId=existInDB.salaryPackageId
                        };
                        db.SalaryPackageDetails.Add(exist);
                    }
                    exist.amount = t.Amount;
                    exist.payHeadId = t.Id;
                    exist.narration = string.Empty;
                }
            }
            try
            {
                db.Entry(existInDB).State = EntityState.Modified;
                db.SaveChanges();
                return "success";
            }
            catch (Exception aa)
            {
                return aa.InnerException==null ? aa.Message : aa.InnerException.Message;
            }
        }
    
        [HttpPost]
        public async Task<IActionResult> SalarypackageDelete(int id = 0)
        {
            var packagesalary = await db.SalaryPackages.FindAsync(id);

            if (packagesalary == null)
            {
                return Json(new { status = true, message = "No record found to delete.!! " });
            }
            else
            {
                try
                {
                    var existMonthlyPkg = await db.MonthlySalaries.FirstOrDefaultAsync(w => w.SalaryPackageId == id);
                    var existEmplSalPkg = await db.tbl_Employee.FirstOrDefaultAsync(w => w.defaultPackageId == id);

                    if (existEmplSalPkg != null)
                    {
                        return Json(new { status = true, message = "Can't delete Salary Package.Refrence record exist in Employees" });
                    }

                    if (existMonthlyPkg != null)
                    {
                        return Json(new { status = true, message = "Can't delete Salary Package. Refrence record exist in Monthly Salaries" });
                    }
                    else
                    {
                        db.SalaryPackages.Remove(packagesalary);
                        await db.SaveChangesAsync();
                        return Json(new { status = true, message = "Selected record has been deleted successfully." });
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { status = false, message = ex.InnerException == null ? ex.Message : ex.InnerException.Message });
                }
            }
        }

        public IActionResult SalaryPackageRegister()
        {
            //SalaryPackageSP sp = new SalaryPackageSP();
            // var dtinfo = sp.SalaryPackageregisterSearch("", "All");
            return View();
        }

        [HttpPost]

        public JsonResult SalaryPackageList(DataManagerRequest dm, string packagename,string Active)
        {
            if (Active == "Yes")
                Active = "true";
            else if (Active == "No")
                Active = "false";
            else
                Active = "All";
            var data = db.SalaryPackages.Include(i=>i.SalaryPackageDetails).AsNoTracking().AsQueryable();
            if (!string.IsNullOrEmpty(packagename)) 
                data = data.Where(w => w.salaryPackageName.Contains(packagename));
            if (Active == "true")
                data = data.Where(w => w.isActive);
            else if (Active == "false") data = data.Where(w => !w.isActive);

            var data2 = data.Select(s =>
                new
                {
                    Id = s.salaryPackageId,
                    Amount = s.SalaryPackageDetails.Select(t=>t.amount).DefaultIfEmpty().Sum(),
                    name = s.salaryPackageName,
                    s.isActive
                });
            return Json(new { result=data2, count=data2.Count() });
        }

        public  IActionResult PayHeadListByPacka(int id)
        {
            var payheadlist = db.PayHeads;
            var data = payheadlist.ToList().Select(s => new
            {
                s.payHeadId,
                Name = s.payHeadName,
                Narration = s.narration,
                Amount = s.SalaryPackageDetails.Where(w=>w.salaryPackageId==id).Select(t=>t.amount).FirstOrDefault(),
                Type = s.type
            });
            return Json(new { result = data, count = data.Count() });
        }
        #endregion

        #region Monthly Salary Setting
        public IActionResult MonthlySalarySetting()
        {
            ViewBag.Packages = new SelectList(db.SalaryPackages.Where(w => w.isActive).Select(s => new { value = s.salaryPackageId, text = s.salaryPackageName }), "value", "text");
            return View();
        }
        [HttpPost]
        public JsonResult MonthlySalaryList(DataManagerRequest request, string month , int campusId )
        {

             var months = DateTimeHelper.ConvertDate(month, false);
            DateTime today = DateTime.Now.Date;
            var data = db.tbl_Employee.Where(w => w.CampusID == campusId & w.salaryType == "Monthly" & (!w.terminationDate.HasValue || w.terminationDate.Value > today)).Select(s => new
            {
                s.Id,
                s.employeeCode,
                s.employeeName,
                s.defaultPackageId
            });

           

            int count = data.Count();
            DataOperations operation = new DataOperations();
            if (request.Skip != 0)
            {
                if (request.Sorted == null)
                {
                    List<Sort> srt = new List<Sort>() { new Sort { Direction = "ascending", Name = "employeeCode" } };
                    data = operation.PerformSorting(data, srt);
                }
                data = operation.PerformSkip(data, request.Skip);
            }
            if (request.Take != 0)
            {
                data = operation.PerformTake(data, request.Take);
            }

            return Json(new { result = data, count = count });

        }

        [HttpPost]
        public async Task<string> MonthlySalarySetting(List<MonthlySalarySettingViewModel> monthlySalary)
        {
            try
            {
                var month = monthlySalary[0]._Month;
                int _month = month.Month;
                int _year = month.Year;
                foreach (var emp in monthlySalary)
                {
                    var packageId = emp.SalaryPackageId;
                    var packages = db.SalaryPackageDetails.Include(i=>i.PayHead).Where(w => w.salaryPackageId == packageId);
                    if (packages.Count() == 0)
                        continue;
                    var empId = emp.EmployeeId;
                    var exist = await db.MonthlySalaries.Include(i=>i.tbl_MonthlySalaryDetails).FirstOrDefaultAsync(w => w.ForMonth.Month == _month && w.ForMonth.Year == _year & w.EmployeeId == empId);
                    bool isNew = false;
                    if (exist == null)
                    {
                        exist = new tbl_MonthlySalary
                        {
                            EmployeeId = emp.EmployeeId,
                            SalaryPackageId = packageId,
                            EntryDate = DateTime.Now,
                            ForMonth = month,
                            UserId = _user.UserID
                        };
                        db.MonthlySalaries.Add(exist);
                        isNew = true;
                    }
                        exist.ModifiedBy = _user.UserID;
                        exist.ModifiedDate = DateTime.Now;
                        exist.SalaryPackageId = packageId;
                    
                    foreach (var package in packages)
                    {
                        if (isNew)
                        {
                            exist.tbl_MonthlySalaryDetails.Add(new tbl_MonthlySalaryDetails { Amount = package.amount, PayHeadId = package.payHeadId, Notes = package.PayHead.payHeadName,type=package.PayHead.type });
                        }
                        else
                        {
                            var payHeadId = package.payHeadId;
                            var salaryExist = exist.tbl_MonthlySalaryDetails.FirstOrDefault(w => w.PayHeadId == payHeadId);
                            if (salaryExist == null)
                            {
                                salaryExist = new tbl_MonthlySalaryDetails() { Amount = package.amount, PayHeadId = package.payHeadId, MonthlySalaryId = exist.Id, Notes = package.PayHead.payHeadName, type = package.PayHead.type };
                                db.MonthlySalaryDetails.Add(salaryExist);
                            }
                            else
                            {
                                salaryExist.Amount = package.amount;
                            }
                        }
                    }
                }
                await db.SaveChangesAsync();
                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        #endregion


        #region Monthly Salary Register
        public IActionResult MonthlySalaryRegister()
        {
            return View();
        }

        [HttpPost]
        public JsonResult MonthlySalaryRegisterList(DataManagerRequest request,string month,int campusId)
        {
            var Month = DateTimeHelper.ConvertDate(month);
            int _month = Month.Month;
            int _year = Month.Year;
            var data = db.MonthlySalaries.Include(i=>i.Employee).Where(w => w.Employee.CampusID==campusId & w.ForMonth.Month == _month & w.ForMonth.Year == _year).Select(s => new
            {
                s.Id,
                s.Employee.employeeCode, 
                s.Employee.employeeName,
                s.SalaryPackage.salaryPackageName,
                Amount = s.tbl_MonthlySalaryDetails.Select(t=>new { t.Amount, t.type }).DefaultIfEmpty().Sum(t=>t.Amount*t.type),
                IsPayed=false
            });
            return Json(new {result=data, count=data.Count() });
        }

        public IActionResult MonthlySalaryRegisterEdit(int id=0)
        {
           ViewBag.PaymentId = _selectListHelper.GetPaymentSelectList();
            ViewBag.id = id;
            return View();
        }
        public IActionResult MonthlySalaryAmountEdit(int? id)
        {
            var monthlySalary = db.MonthlySalaryDetails.Where(s => s.Id == id).FirstOrDefault();
            if (monthlySalary == null) return NotFound();
           
            return View(monthlySalary);
        }
        [HttpPost]
        public string MonthlySalaryAmountEdit(tbl_MonthlySalaryDetails model)
        {
            var message = string.Empty;
            var exist = db.MonthlySalaryDetails.Find(model.Id);
            if (exist != null)
            {
                exist.Amount = model.Amount;
                db.Entry(exist).State = EntityState.Modified;
            }
                try
                {
                    db.SaveChanges();
                    message = "success";
                }
                catch (Exception ss)
                {
                    message = ss.InnerException == null ? ss.Message : ss.InnerException.Message;
                }

                return message;
        }

        [HttpPost]
         public async Task<IActionResult> MonthlySalaryDelete(int id = 0)
        {
            var month_salary = await db.MonthlySalaries.FindAsync(id);

            try
            {
                if (month_salary == null)
                {
                    return Json(new { status = true, message = "No record found to delete.!! " });
                }
                else
                {
                    db.MonthlySalaries.Remove(month_salary);
                    await db.SaveChangesAsync();
                    return Json(new { status = true, message = "Selected record has been deleted successfully." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.InnerException == null ? ex.Message : ex.InnerException.Message });
            }
        }
        #endregion

        #region Bonus Deduction


        public IActionResult BonusDeduction()
        {

            //ViewBag.EmployeeId = _selectListHelper.GetEmployeeList();
            ViewBag.PayHeads = new SelectList(db.PayHeads.Select(s => new { id = s.payHeadId, text = s.payHeadName }), "id", "text");
            return View();
        }


        [HttpPost]
        public JsonResult MonthlySalaryDetails(DataManagerRequest dm, int employeeId, string month)
        {
            var Month = DateTimeHelper.ConvertDate(month, false);
            if (Month == DateTime.MinValue)
            {
                return Json(new { result = "Invalid Month Selected", count = 0 });
            }
            int _month = Month.Month;
            int _year = Month.Year;
            var data = db.MonthlySalaryDetails.Include(i=>i.MonthlySalary).Where(w => w.MonthlySalary.EmployeeId == employeeId &
            w.MonthlySalary.ForMonth.Month == _month & w.MonthlySalary.ForMonth.Year == _year).Select(s=>new { 
            s.Notes,s.Amount,
            s.type,s.Id
            
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
                    srt.Add(new Sort { Name = "Id", Direction = "ascending" });
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
                return Json(new { result = data, count = count });
            }
            else
            {
                return Json(new { result = data, count = count });
            }
        }

        [HttpPost]
        public async Task<string> BonusDeduction(tbl_MonthlySalaryDetails model)
        {
            try
            {
                if (model.Amount == 0)
                {
                    return "Please enter a valid amount to continue";
                }
                if (!decimal.TryParse(Request.Form["EmployeeId"].ToString(), out decimal employeeId))
                {
                    return "Please select an employee.";
                }
                var Month = DateTimeHelper.ConvertDate(Request.Form["Month"].ToString(), false);
                if (Month == DateTime.MinValue)
                {
                    return "Invalid Month Selected";
                }
                int _month = Month.Month;
                int _year = Month.Year;
                var data = await db.MonthlySalaries.FirstOrDefaultAsync(w => w.EmployeeId == employeeId &&
                w.ForMonth.Month == _month && w.ForMonth.Year == _year);
                if (data == null)
                {
                    return "Please add monthly salary for this employee before continue.";
                }
                var payHead = await db.PayHeads.FirstOrDefaultAsync(a => a.payHeadId == model.PayHeadId);
                db.MonthlySalaryDetails.Add(new tbl_MonthlySalaryDetails
                {
                    Amount = model.Amount,
                    MonthlySalaryId = data.Id,
                    Notes = model.Notes ?? payHead.payHeadName,
                    PayHeadId = model.PayHeadId,
                    type = payHead.type
                });

                await db.SaveChangesAsync();
                return "success";
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. " +
                    "Try again, and if the problem persists, " +
                    "see your system administrator.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            if (ModelState.IsValid) return "success";
            return ModelState.Where(w => w.Value.Errors.Count > 0).FirstOrDefault().Value.Errors.FirstOrDefault()
                .ErrorMessage;
        }

        #endregion

        #region Pay slip
        public IActionResult PaySlips()
        {
            return View();
        }


        [HttpPost]
        public  IActionResult PaySlips(SalaryVoucherMasterInfo masterInfo)
        {
            return View();
        }
        #endregion

        #region Salary Payment

        public IActionResult SalaryPayment()
        {
            var bank_id = (int)PublicVariables.EnumAccountGroup.Bank_Account;
            var cash_id = (int)PublicVariables.EnumAccountGroup.Cash_in_Hand;
            ViewBag.LedgerLists =
                new SelectList(
                    db.AccountLedgers.Where(s => s.AccountGroupId == cash_id || s.AccountGroupId == bank_id)
                    .Select(s => new { val = s.Id, text = s.LedgerName }).ToList(),
                    "val", "text");
            return View();
        }

        //public variable

        [HttpPost]
        public async Task<string> SalaryPayment(tbl_SalaryPayment model)
        {
            var message = "";
            try
            {
                if (model.Amount <= 0)
                {
                    return "Please enter a valid amount to continue.";
                }
                int decPaymentVoucherTypeId = (int)PublicVariables.EnumVoucherTypeDefaults.Payment_Voucher;
                if (Request.Form["PaymentType"] == "advance")
                {
                    decPaymentVoucherTypeId = (int)PublicVariables.EnumVoucherTypeDefaults.Advance_Payment;
                }
                else
                {
                    decPaymentVoucherTypeId = (int)PublicVariables.EnumVoucherTypeDefaults.Payment_Voucher;
                }

                var month = DateTimeHelper.ConvertDate(Request.Form["Month"]);
                if (month == DateTime.MinValue)
                {
                    return "Please select a month to continue.";
                }
                int _month = month.Month;
                int _year = month.Year;
                if (!Decimal.TryParse(Request.Form["EmployeeId"], out decimal employeeId))
                {
                    return "Please select an employee to continue.";
                }
                var salary = await db.MonthlySalaries.Include(i => i.tbl_MonthlySalaryDetails).Include(i => i.SalaryPayments).FirstOrDefaultAsync(f => f.ForMonth.Month == _month & f.ForMonth.Year == _year & f.EmployeeId == employeeId);
                if (salary == null)
                {
                    return "The selected employee has not monthly salary added for selected month.";
                }

                var totalAmount = salary.tbl_MonthlySalaryDetails.Select(s => s.Amount * s.type).DefaultIfEmpty(0).Sum();
                var paidAmount = salary.SalaryPayments.Select(s => s.Amount).DefaultIfEmpty(0).Sum();
                if (model.Amount > (totalAmount - paidAmount))
                {
                    return $"Please enter an amount less then the remaining amount({totalAmount - paidAmount}) of the salary";
                }

                var advancePayment = new tbl_SalaryPayment
                {
                    Amount = model.Amount,
                    ChequeDate = model.ChequeDate == DateTime.MinValue ? DateTime.Now : model.ChequeDate,
                    Chequenumber = model.Chequenumber ?? string.Empty,
                    EntryDate = DateTime.Now,
                    LedgerId = model.LedgerId,
                    ModifiedBy = _user.UserID,
                    ModifiedOn = DateTime.Now,
                    MonthlySalaryId = salary.Id,
                    Narration = model.Narration ?? string.Empty,
                    UserId = _user.UserID,
                    VoucherNo = model.VoucherNo ?? string.Empty,
                    VoucherTypeId = decPaymentVoucherTypeId
                };

                db.SalaryPayments.Add(advancePayment);
                await db.SaveChangesAsync();
                message = "success";
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return message;
        }


        public IActionResult SalaryPaymentEdit(decimal id)
        {
            var data = db.SalaryPayments.Find(id);
            var bank_id = (int)PublicVariables.EnumAccountGroup.Bank_Account;
            var cash_id = (int)PublicVariables.EnumAccountGroup.Cash_in_Hand;
            ViewBag.LedgerLists =
                new SelectList(
                    db.AccountLedgers.Where(s => s.AccountGroupId == cash_id || s.AccountGroupId == bank_id)
                    .Select(s => new { val = s.Id, text = s.LedgerName }).ToList(),
                    "val", "text",data.LedgerId);
            return View(data);
        }


        [HttpPost]
        public async Task<string> SalaryPaymentEdit(tbl_SalaryPayment model)
        {
            string message;
            try
            {

                if (model.Amount <= 0)
                {
                    return "Please enter a valid amount to continue.";
                }

                int decPaymentVoucherTypeId = (int)PublicVariables.EnumVoucherTypeDefaults.Payment_Voucher;
                if (Request.Form["PaymentType"].ToString() == "advance")
                {
                    decPaymentVoucherTypeId = (int)PublicVariables.EnumVoucherTypeDefaults.Advance_Payment;
                }
                else
                {
                    decPaymentVoucherTypeId = (int)PublicVariables.EnumVoucherTypeDefaults.Payment_Voucher;
                }

                var month = DateTimeHelper.ConvertDate(Request.Form["Month"].ToString());
                if (month == DateTime.MinValue)
                {
                    return "Please select a month to continue.";
                }

                int _month = month.Month;
                int _year = month.Year;
                if (!decimal.TryParse(Request.Form["EmployeeId"].ToString(), out decimal employeeId))
                {
                    return "Please select an employee to continue.";
                }

                var exist = await db.SalaryPayments.FindAsync(model.Id);
                if (exist == null)
                {
                    return "No such record found.";
                }

                var salary = await db.MonthlySalaries
                    .Include(i => i.tbl_MonthlySalaryDetails)
                    .Include(i => i.SalaryPayments)
                    .FirstOrDefaultAsync(f => f.ForMonth.Month == _month && f.ForMonth.Year == _year && f.EmployeeId == employeeId);

                if (salary == null)
                {
                    return "The selected employee has no monthly salary added for the selected month.";
                }

                var totalAmount = salary.tbl_MonthlySalaryDetails.Select(s => s.Amount * s.type).DefaultIfEmpty(0).Sum();
                var paidAmount = salary.SalaryPayments.Where(w => w.Id != model.Id).Select(s => s.Amount).DefaultIfEmpty(0).Sum();

                if (model.Amount > (totalAmount - paidAmount))
                {
                    return $"Please enter an amount less than the remaining amount ({totalAmount - paidAmount}) of the salary";
                }

                exist.Amount = model.Amount;
                exist.ChequeDate = model.ChequeDate == DateTime.MinValue ? DateTime.Now : model.ChequeDate;
                exist.Chequenumber = model.Chequenumber ?? string.Empty;
                exist.LedgerId = model.LedgerId;
                exist.ModifiedBy = _user.UserID;
                exist.ModifiedOn = DateTime.Now;
                exist.MonthlySalaryId = salary.Id;
                exist.Narration = model.Narration ?? string.Empty;
                exist.VoucherNo = model.VoucherNo ?? string.Empty;
                exist.VoucherTypeId = decPaymentVoucherTypeId;

                await db.SaveChangesAsync();
                message = "success";

            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return message;
        }

        [HttpPost]
        public async Task<IActionResult> SalaryPaymentDelete(int id)
        {
            string message = "";
            try
            {
                var exist = await db.SalaryPayments.FindAsync(id);
                if (exist == null)
                {
                    message = "No record found to delete.";
                }
                else
                {
                    db.SalaryPayments.Remove(exist);
                    await db.SaveChangesAsync();
                    message = "success";
                }
            }
            catch (DbUpdateException ex)
            {
                message = ex.InnerException?.Message ?? ex.Message;
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return Json(new { message });
        }
        public IActionResult SalaryPaymentRegister()
        {
            return View();
        }

        public IActionResult SalaryPaymentList(DataManagerRequest dm, string month = "")
        {
            var months = DateTimeHelper.ConvertDate(month);
            if(months==DateTime.MinValue)
            {

            }
            int _month = months.Month;
            int _year = months.Year;
            var data = db.SalaryPayments.Include(i => i.MonthlySalary).Include(i=>i.MonthlySalary.Employee).
                Where(w => w.MonthlySalary.ForMonth.Month == _month & w.MonthlySalary.ForMonth.Year == _year).
                Select(s => new
                {
                    s.Id,
                    s.MonthlySalary.Employee.employeeCode,
                    s.MonthlySalary.Employee.employeeName,
                    s.EntryDate,
                    s.MonthlySalary.ForMonth,
                    s.Amount,
                    s.VoucherTypeId
                });
            int count = data.Count();
            DataOperations operation = new DataOperations();
            if (dm.Skip != 0)
            {
                if (dm.Sorted == null)
                {
                    List<Sort> srt = new List<Sort>() { new Sort { Direction = "ascending", Name = "employeeCode" } };
                    data = operation.PerformSorting(data, srt);
                }
                data = operation.PerformSkip(data, dm.Skip);
            }
            if (dm.Take != 0)
            {
                data = operation.PerformTake(data, dm.Take);
            }
            return Json(new { result = data, count = data.Count() });
        }


        #endregion
        
        private void LedgerPosting(int cmbCashOrBankAcc, string txtVoucherNo, string lblTotalAmount,
            string dtpVoucherDate)
        {
            try
            {
                int decMonthlyVoucherTypeId = (int)PublicVariables.EnumVoucherTypeDefaults.Monthly_Salary_Voucher;
                bool isAutomatic = false;
                string strVoucherNo = "", strInvoiceNo="";
                var infoLedgerPostingcredit = new tbl_LedgerPosting();
                infoLedgerPostingcredit.debit = 0;
                infoLedgerPostingcredit.credit = Convert.ToDecimal(lblTotalAmount);
                infoLedgerPostingcredit.voucherTypeId = decMonthlyVoucherTypeId;
                if (isAutomatic)
                    infoLedgerPostingcredit.voucherNo = strVoucherNo;
                else
                    infoLedgerPostingcredit.voucherNo = txtVoucherNo;
                infoLedgerPostingcredit.EntryDate =
                    DateTimeHelper.ConvertDate(dtpVoucherDate);
                infoLedgerPostingcredit.ledgerId = cmbCashOrBankAcc;
                infoLedgerPostingcredit.detailsId = 0;
                if (isAutomatic)
                    infoLedgerPostingcredit.invoiceNo = strInvoiceNo;
                else
                    infoLedgerPostingcredit.invoiceNo = txtVoucherNo;
                infoLedgerPostingcredit.chequeNo = string.Empty;
                infoLedgerPostingcredit.chequeDate = DateTime.Now;
                infoLedgerPostingcredit.yearId = PublicVariables._decCurrentFinancialYearId;
                infoLedgerPostingcredit.UserId = _user.UserID;
                infoLedgerPostingcredit.ModifiedBy = _user.UserID;
                infoLedgerPostingcredit.ModifiedOn = DateTime.Now;
                db.LedgerPostings.Add(infoLedgerPostingcredit);

                var infoLedgerPostingdebit = new tbl_LedgerPosting();

                infoLedgerPostingdebit.debit = Convert.ToDecimal(lblTotalAmount);
                infoLedgerPostingdebit.credit = 0;
                infoLedgerPostingdebit.voucherTypeId = decMonthlyVoucherTypeId;
                if (isAutomatic)
                {
                    infoLedgerPostingdebit.voucherNo = strVoucherNo;
                    infoLedgerPostingdebit.invoiceNo = strInvoiceNo;
                }
                else
                {
                    infoLedgerPostingdebit.voucherNo = txtVoucherNo;
                    infoLedgerPostingdebit.invoiceNo = txtVoucherNo;
                }

                infoLedgerPostingdebit.EntryDate = DateTime.Now;
                infoLedgerPostingdebit.ledgerId = 4; //ledgerId of salarys
                infoLedgerPostingdebit.detailsId = 0;

                infoLedgerPostingdebit.yearId = PublicVariables._decCurrentFinancialYearId;
                infoLedgerPostingdebit.UserId = _user.UserID;
                infoLedgerPostingdebit.ModifiedBy = _user.UserID;
                infoLedgerPostingdebit.ModifiedOn = DateTime.Now;
                db.LedgerPostings.Add(infoLedgerPostingdebit);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                 
            }
        }

        public void LedgerPostingAdvanepayment(int decAdvancePaymentId, string txtAdvanceVoucherNo,
            int cmbCashOrBank, string txtAmount)
        {
            try
            {
                //add ledgerposting for credit
                int decPaymentVoucherTypeId = (int)PublicVariables.EnumVoucherTypeDefaults.Advance_Payment;
                bool isAutomatic = false;
                string strVoucherNo = "", strInvoiceNo = "";
                var infoLedgerPosting = new tbl_LedgerPosting();

                if (isAutomatic)
                {
                    infoLedgerPosting.voucherNo = strVoucherNo;
                    infoLedgerPosting.invoiceNo = strInvoiceNo;
                }
                else
                {
                    infoLedgerPosting.voucherNo = txtAdvanceVoucherNo.Trim();
                    infoLedgerPosting.invoiceNo = txtAdvanceVoucherNo.Trim();
                }

                infoLedgerPosting.debit = 0;
                infoLedgerPosting.credit = Convert.ToDecimal(txtAmount);
                infoLedgerPosting.ledgerId = cmbCashOrBank;

                infoLedgerPosting.voucherTypeId = decPaymentVoucherTypeId;
                infoLedgerPosting.EntryDate = DateTime.Now;
                infoLedgerPosting.detailsId = decAdvancePaymentId;
                infoLedgerPosting.yearId = PublicVariables._decCurrentFinancialYearId;
                infoLedgerPosting.chequeNo = string.Empty;
                infoLedgerPosting.chequeDate = DateTime.Now;
                infoLedgerPosting.ModifiedOn = DateTime.Now;
                infoLedgerPosting.UserId = _user.UserID;
                infoLedgerPosting.ModifiedBy = _user.UserID;

                db.LedgerPostings.Add(infoLedgerPosting);

                //add ledgerposting for debit
                var infoLedgerPostingdebit = new tbl_LedgerPosting();

                if (isAutomatic)
                {
                    infoLedgerPostingdebit.voucherNo = strVoucherNo;
                    infoLedgerPostingdebit.invoiceNo = strInvoiceNo;
                }
                else
                {
                    infoLedgerPostingdebit.invoiceNo = txtAdvanceVoucherNo.Trim();
                    infoLedgerPostingdebit.voucherNo = txtAdvanceVoucherNo.Trim();
                }

                infoLedgerPostingdebit.debit = Convert.ToDecimal(txtAmount);
                infoLedgerPostingdebit.credit = 0;
                infoLedgerPostingdebit.ledgerId = 3;

                infoLedgerPostingdebit.voucherTypeId = decPaymentVoucherTypeId;
                infoLedgerPostingdebit.EntryDate = DateTime.Now;
                infoLedgerPostingdebit.detailsId = decAdvancePaymentId;
                infoLedgerPostingdebit.yearId = PublicVariables._decCurrentFinancialYearId;
                infoLedgerPostingdebit.chequeNo = string.Empty;
                infoLedgerPostingdebit.chequeDate = DateTime.Now;
                infoLedgerPosting.ModifiedOn = DateTime.Now;
                infoLedgerPosting.UserId = _user.UserID;
                infoLedgerPosting.ModifiedBy = _user.UserID;

                db.LedgerPostings.Add(infoLedgerPostingdebit);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                // PublicVariables.infoError.ErrorString = "AP6:" + ex.Message;
            }
        }

        #region rating

        
        public IActionResult RatingMasterEntry()
        {
            return View();
        }

        //public IActionResult GetRatingMasterListForJq(JqGridRequest request)
        //{
        //    var ratingmaster = db.RatingMasters.ToList();

        //    var totalRecords = ratingmaster.Count();
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
        //    IEnumerable<JqGridRecord> data = null;

        //    data = ratingmaster.Skip(skip).Take(perpage).ToList().Select(s => new JqGridRecord(s.ID.ToString(),
        //        new
        //        {
        //            s.ID, s.Description, StartDate = s.StartDate.ToString("MMMM,yyyy"),
        //            CloseDate = s.CloseDate.ToString("MMMM,yyyy"), s.RatingParticipants, s.RatingType, s.Year,
        //            action =
        //                $"<a title=\"Delete\" href=\"{Url.Action("DeleteRatingMaster", "Payroll", new {id = s.ID})}\" class=\"edit ajax ml-1\"><i class=\"fa fa-trash\"></i></a> "
        //        }));

        //    foreach (var d in data) response.Records.Add(d);
        //    //response.Records = data;
        //    response.Reader.RepeatItems = false;
        //    //return new JqGridJsonResult {Data = response};

        //    return Json(new
        //    {
        //        Data = response
        //    });
        //    //return db.Companies.ToList();
        //}

        
        [HttpPost]

        public IActionResult RatingMasterEntry(RatingMaster ratingMaster)
        {
            if (Request.Form["Campus"].ToString() == null)
            {
                return BadRequest("Campus Not Selected");
            }

            var campusid = int.Parse(Request.Form["Campus"]);

            try
            {
               // ratingMaster.ID = 1;

                if (db.RatingMasters.Count() != 0)
                {
                  //  ratingMaster.ID = db.RatingMasters.Select(s => s.ID).Max() + 1;
                }

                ratingMaster.CampusID = campusid;
                db.RatingMasters.Add(ratingMaster);
                db.SaveChanges();

                return Ok("success");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            if (ModelState.IsValid)
            {
                return Ok("success");
            }

            return BadRequest(ModelState.Where(w => w.Value.Errors.Count > 0).FirstOrDefault().Value.Errors.FirstOrDefault().ErrorMessage);
        }





        //public string RatingMasterEntry(RatingMaster ratingMaster)
        //{
        //    if (Request.Form["Campus"].ToString() == null) return "Campus Not Selected";
        //    var campusid = int.Parse(Request.Form["Campus"]);
        //    try
        //    {
        //        ratingMaster.ID = 1;
        //        if (db.RatingMasters.Count() != 0) ratingMaster.ID = db.RatingMasters.Select(s => s.ID).Max() + 1;
        //        ratingMaster.CampusID = campusid;
        //        db.RatingMasters.Add(ratingMaster);
        //        db.SaveChanges();
        //        return "success";
        //    }


        //    catch (Exception ex)
        //    {
        //        ModelState.AddModelError("", ex.Message);
        //    }

        //    if (ModelState.IsValid) return "success";
        //    return ModelState.Where(w => w.Value.Errors.Count > 0).FirstOrDefault().Value.Errors.FirstOrDefault()
        //        .ErrorMessage;
        //}


        public IActionResult DeleteRatingMaster(int id)
        {
            var list = db.RatingMasters.Where(w => w.ID == id).FirstOrDefault();

            return View(list);
        }

        
        [HttpPost]
        public string DeleteRatingMaster(RatingMaster ratingMaster)
        {
            try
            {
                var rm = db.RatingMasters.Where(w => w.ID == ratingMaster.ID).FirstOrDefault();
                if (rm != null)
                    db.RatingMasters.Remove(rm);
                db.SaveChanges();
            }
            //catch (DbEntityValidationException invalids)
            //{
            //    foreach (var inv in invalids.EntityValidationErrors)
            //    foreach (var field in inv.ValidationErrors)
            //        ModelState.AddModelError(field.PropertyName, field.ErrorMessage);
            //}
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            if (ModelState.IsValid) return "delete";
            return ModelState.Where(w => w.Value.Errors.Count > 0).FirstOrDefault().Value.Errors.FirstOrDefault()
                .ErrorMessage;
        }

        
        public IActionResult RatingEntry()
        {
            //tmr.Interval = 100;
            //tmr.Tick += new EventHandler(tmr_Tick);
            //var dba = new DatabaseAccessSql();

            //string q = @"SELECT ActiveRating.StaffID,ActiveRating.campusID,ActiveRating.RatingMasterID,ActiveRating.ClassID,Staffs.StaffName, Campuses.CampusName, Classes.ClassName, RatingMaster.Description, Staffs.Photo FROM ActiveRating INNER JOIN 
            //            Staffs ON ActiveRating.StaffID = Staffs.ID INNER JOIN Campuses ON ActiveRating.campusID = Campuses.ID INNER JOIN Classes ON 
            //            ActiveRating.ClassID = Classes.ID INNER JOIN RatingMaster ON ActiveRating.RatingMasterID = RatingMaster.ID";
            //DataTable tab = dba.CreateTable(q);

            return View();
        }

        
        public IActionResult ActiveRating()
        {
            var DataSource = db.tbl_Employee.Where(w => w.LeavedStaffs.Count < 1)
                .Select(s => new {s.Id, StaffName = s.employeeName + " s/d/o " + s.FatherName}).ToList();
            //DataSource.Insert(0, new { employeeId = 0, StaffName = "--Select --" });
            var list = new SelectList(DataSource, "employeeId", "StaffName");

            ViewBag.staff = list;


            var rating = db.RatingMasters.Select(s => new {s.ID, s.Description}).ToList();
            var lists = new SelectList(rating, "ID", "Description");

            ViewBag.Ratinglist = lists;
            return View();
        }

        
        [HttpPost]
        public string ActiveRating(RatingMaster ratingMaster)
        {
            try
            {
                //var rating = db.RatingMasters.Select(s => new { s.ID, s.Description }).ToList();
                ////DataSource.Insert(0, new { ID = 0, Description = "--Select --" });
                //SelectList lists = new SelectList(rating, "ID", "Description");

                //ViewBag.Ratinglist = lists;

                var MasterID = int.Parse(Request.Form["RatingId"].ToString());
                var classID = int.Parse(Request.Form["classid"].ToString());
                var campusID = int.Parse(Request.Form["compusid"].ToString());
                int? staffID = null;

                try
                {
                    staffID = int.Parse(Request.Form["staffId"].ToString());
                }
                catch
                {
                }

                //if (cmbStaff.Enabled == false) 
                //{
                //    staffID = 0;
                //}
                var Quries = new ArrayList();
                Quries.Add("Delete from ActiveRating");

                Quries.Add("Insert into ActiveRating(RatingMasterID,StaffID,ClassID,CampusID) values (" + MasterID +
                           "," + staffID + "," + classID + "," + campusID + ")");

                if (dba.Insert(Quries)) return "success";
            }
            //catch (DbEntityValidationException invalids)
            //{
            //    foreach (var inv in invalids.EntityValidationErrors)
            //    foreach (var field in inv.ValidationErrors)
            //        ModelState.AddModelError(field.PropertyName, field.ErrorMessage);
            //}
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            if (ModelState.IsValid) return "success";
            return ModelState.Where(w => w.Value.Errors.Count > 0).FirstOrDefault().Value.Errors.FirstOrDefault()
                .ErrorMessage;
        }

        #endregion

        #region salary And Payments

       
      
        public void LedgerUpdate(string txtAdvanceVoucherNo, decimal txtAmount, string cmbCashOrBank)
        {
            try
            {
                //decimal decLedgerPostingId = 0;
                //var spLedgerPosting = new LedgerPostingSP();
                //var infoLedgerPosting = new LedgerPostingInfo();
                //var dtbl = new DataTable();
                //dtbl = spLedgerPosting.GetLedgerPostingIds(strVoucherNo, decAdvancePaymentId);
                //var ini = 0;
                //foreach (DataRow dr in dtbl.Rows)
                //{
                //    ini++;
                //    if (ini == 2)
                //    {
                //        decLedgerPostingId = Convert.ToDecimal(dr.ItemArray[0].ToString());
                //        infoLedgerPosting.LedgerPostingId = decLedgerPostingId;
                //        infoLedgerPosting.Date = PublicVariables._dtCurrentDate;
                //        if (isAutomatic)
                //            infoLedgerPosting.VoucherNo = strVoucherNo;
                //        else
                //            infoLedgerPosting.VoucherNo = txtAdvanceVoucherNo.Trim();
                //        infoLedgerPosting.Debit = Convert.ToDecimal(txtAmount.ToString());
                //        infoLedgerPosting.Credit = 0;
                //        infoLedgerPosting.VoucherTypeId = decPaymentVoucherTypeId;
                //        infoLedgerPosting.LedgerId = 3;
                //        infoLedgerPosting.DetailsId = decAdvancePaymentId;
                //        if (isAutomatic)
                //            infoLedgerPosting.InvoiceNo = strInvoiceNo;
                //        else
                //            infoLedgerPosting.InvoiceNo = txtAdvanceVoucherNo.Trim();

                //        infoLedgerPosting.ChequeNo = string.Empty;
                //        infoLedgerPosting.ChequeDate = DateTime.Now;

                //        infoLedgerPosting.YearId = PublicVariables._decCurrentFinancialYearId;
                //        infoLedgerPosting.Extra1 = string.Empty;
                //        infoLedgerPosting.Extra2 = string.Empty;

                //        spLedgerPosting.LedgerPostingEdit(infoLedgerPosting);
                //    }

                //    if (ini == 1)
                //    {
                //        decLedgerPostingId = Convert.ToDecimal(dr.ItemArray[0].ToString());
                //        infoLedgerPosting.LedgerPostingId = decLedgerPostingId;
                //        infoLedgerPosting.Date = PublicVariables._dtCurrentDate;
                //        if (isAutomatic)
                //            infoLedgerPosting.VoucherNo = strVoucherNo;
                //        else
                //            infoLedgerPosting.VoucherNo = txtAdvanceVoucherNo.Trim();
                //        infoLedgerPosting.Debit = 0;
                //        infoLedgerPosting.Credit = Convert.ToDecimal(txtAmount.ToString());
                //        infoLedgerPosting.VoucherTypeId = decPaymentVoucherTypeId;
                //        infoLedgerPosting.LedgerId = Convert.ToDecimal(cmbCashOrBank);
                //        infoLedgerPosting.DetailsId = decAdvancePaymentId;
                //        if (isAutomatic)
                //            infoLedgerPosting.InvoiceNo = strInvoiceNo;
                //        else
                //            infoLedgerPosting.InvoiceNo = txtAdvanceVoucherNo.Trim();

                //        infoLedgerPosting.ChequeNo = string.Empty;
                //        infoLedgerPosting.ChequeDate = DateTime.Now;

                //        infoLedgerPosting.YearId = PublicVariables._decCurrentFinancialYearId;
                //        infoLedgerPosting.Extra1 = string.Empty;
                //        infoLedgerPosting.Extra2 = string.Empty;
                //        spLedgerPosting.LedgerPostingEdit(infoLedgerPosting);
                //    }
                //}
            }
            catch (Exception ex)
            {
               // PublicVariables.infoError.ErrorString = "AP7:" + ex.Message;
            }
        }

        
        
        
        public IActionResult MonthlySalaryEdit(int id)

        {
            //var salarySP = new MonthlySalarySP();
            //var sP = new SalaryVoucherMasterSP();
            //var datas = sP.SalaryVoucherMasterView(id);
            //var spMaster = new DailySalaryVoucherMasterSP();
            //var Ledgers = spMaster.DailySalaryVoucherCashOrBankLedgersComboFill().AsEnumerable().Select(s => new
            //{
            //    LedgerId = (decimal) s["LedgerId"],
            //    LedgerName = s["LedgerName"].ToString()
            //});
            //ViewBag.LedgerLists = new SelectList(Ledgers, "LedgerId", "LedgerName");

            return View();
        }

        
        [HttpPost]
        public string MonthlySalaryEdit(SalaryVoucherMasterInfo salary)
        {
            string message = "";
            return message;
        }

        public void LedgerUpdate(string txtVoucherNo, string strVoucherNo, decimal decMonthlyVoucherTypeId,
            string dtpVoucherDate, string lblTotalAmount, string cmbCashOrBankAcc)
        {
            try
            {
                //decimal decLedgerPostingId = 0;
                //var spLedgerPosting = new LedgerPostingSP();
                //var infoLedgerPosting = new LedgerPostingInfo();
                //var dtbl = new DataTable();
                //dtbl = spLedgerPosting.GetLedgerPostingIds(strVoucherNo, decMonthlyVoucherTypeId);
                //var ini = 0;
                //foreach (DataRow dr in dtbl.Rows)
                //{
                //    ini++;

                //    if (ini == 2)
                //    {
                //        decLedgerPostingId = Convert.ToDecimal(dr.ItemArray[0].ToString());
                //        infoLedgerPosting.LedgerPostingId = decLedgerPostingId;
                //        infoLedgerPosting.Date = Convert.ToDateTime(dtpVoucherDate);
                //        if (isAutomatic)
                //            infoLedgerPosting.VoucherNo = strVoucherNo;
                //        else
                //            infoLedgerPosting.VoucherNo = txtVoucherNo;
                //        infoLedgerPosting.Debit = Convert.ToDecimal(lblTotalAmount);
                //        infoLedgerPosting.Credit = 0;
                //        infoLedgerPosting.VoucherTypeId = decMonthlyVoucherTypeId;
                //        infoLedgerPosting.LedgerId = 4;
                //        infoLedgerPosting.DetailsId = 0;
                //        if (isAutomatic)
                //            infoLedgerPosting.InvoiceNo = strInvoiceNo;

                //        else
                //            infoLedgerPosting.InvoiceNo = txtVoucherNo;

                //        infoLedgerPosting.ChequeNo = string.Empty;
                //        infoLedgerPosting.ChequeDate = DateTime.Now;

                //        infoLedgerPosting.YearId = PublicVariables._decCurrentFinancialYearId;
                //        infoLedgerPosting.Extra1 = string.Empty;
                //        infoLedgerPosting.Extra2 = string.Empty;

                //        spLedgerPosting.LedgerPostingEdit(infoLedgerPosting);
                //    }

                //    if (ini == 1)
                //    {
                //        decLedgerPostingId = Convert.ToDecimal(dr.ItemArray[0].ToString());
                //        infoLedgerPosting.LedgerPostingId = decLedgerPostingId;
                //        infoLedgerPosting.Date = PublicVariables._dtCurrentDate;
                //        if (isAutomatic)
                //            infoLedgerPosting.VoucherNo = strVoucherNo;
                //        else
                //            infoLedgerPosting.VoucherNo = txtVoucherNo;
                //        infoLedgerPosting.Debit = 0;
                //        infoLedgerPosting.Credit = Convert.ToDecimal(lblTotalAmount);
                //        infoLedgerPosting.VoucherTypeId = decMonthlyVoucherTypeId;
                //        infoLedgerPosting.LedgerId = Convert.ToDecimal(cmbCashOrBankAcc);
                //        infoLedgerPosting.DetailsId = 0;
                //        if (isAutomatic)
                //            infoLedgerPosting.InvoiceNo = strInvoiceNo;

                //        else
                //            infoLedgerPosting.InvoiceNo = txtVoucherNo;
                //        infoLedgerPosting.YearId = PublicVariables._decCurrentFinancialYearId;
                //        infoLedgerPosting.Extra1 = string.Empty;
                //        infoLedgerPosting.Extra2 = string.Empty;
                //        infoLedgerPosting.ChequeNo = string.Empty;
                //        infoLedgerPosting.ChequeDate = DateTime.Now;
                //        spLedgerPosting.LedgerPostingEdit(infoLedgerPosting);
                //    }
                //}
            }
            catch (Exception ex)
            {
                //PublicVariables.infoError.ErrorString = "MSV5:" + ex.Message;
            }
        }

        [HttpPost]
        public IActionResult MonthlysalaryGridFill(DateTime dtpMonth)
        {
            try
            {

                return RedirectToAction("MonthlySalaryVoucher");
            }
            catch (Exception ex)
            {
               // PublicVariables.infoError.ErrorString = "MSV1:" + ex.Message;
            }

            return View();
        }

        
       
        
        public IActionResult Deletemonthlysalary(int id)
        {
            return View();
        }

        
        [HttpPost]
        public string Deletemonthlysalary(MonthlySalaryInfo salaryInfo)
        {
            if (Request.Form["masterid"].ToString()  == null) return "failed";
            var message = "";
            try
            {
                message = "success";
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return message;
        }

      
        
        
        public IActionResult DailySalaryRegister()
        {
            return View();
        }

        //public ActionResult GetDailysalaryregisteredForJq(JqGridRequest request, string txtSalaryDateTo = "",
        //    string txtSalaryDateFrom = "", string txtVoucherDateFrom = "", string txtVoucherDateTo = "",
        //    string txtVoucherNo = "")
        //{
            
        //    //return db.Companies.ToList();
        //}

        
        

        
        public IActionResult DailySalaryVoucher()
        {
            //var Ledgers = spMaster.DailySalaryVoucherCashOrBankLedgersComboFill().AsEnumerable().Select(s => new
            //{
            //    LedgerId = (decimal) s["LedgerId"],
            //    LedgerName = s["LedgerName"].ToString()
            //});
            //ViewBag.LedgerLists = new SelectList(Ledgers, "LedgerId", "LedgerName");
            
            return View();
        }

        //public ActionResult GetdailysalaryvoucherForJq(JqGridRequest request)
        //{
        //    var dtpSalaryDate = DateTime.Now;
        //    var isEditmode = true;
        //    var spDetails = new DailySalaryVoucherDetailsSP();
        //    var datatTable =
        //        spDetails.DailySalaryVoucherDetailsGridViewAll(dtpSalaryDate.ToString(), isEditmode,
        //            strVoucherNoforEdit);
        //    IEnumerable<JqGridRecord> data = null;
        //    var pno = request.PageIndex;
        //    var perpage = request.RecordsCount;
        //    var skip = perpage * pno;
        //    data = datatTable.AsEnumerable().Skip(skip).Take(perpage).Select(x =>
        //        new JqGridRecord(x["DetailsId"].ToString(), new
        //        {
        //            Id = x["DetailsId"].ToString(),
        //            SINo = (decimal) (x["Sl.No"] == null ? "" : x["Sl.No"]),
        //            masterId = x["MasterId"].ToString(),
        //            EmployeeId = (decimal) (x["employeeId"] == null ? "" : x["employeeId"]),
        //            EmployeeName = (string) (x["employeeName"] == null ? "" : x["employeeName"]),
        //            EmployeeCode = (string) (x["employeecode"] == null ? "" : x["employeecode"]),
        //            Wage = (decimal) (x["dailyWage"] == null ? "" : x["dailyWage"]),
        //            Attendance = (decimal) (x["attendance"] == null ? "" : x["attendance"]),
        //            Status =
        //                $"<select name=\"status\" class=\"status\" onchange=\"statuscheck()\" ><option value=\"Pending\">Pending</option><option value=\"Paid\">Paid</option></select><Input type=\"hidden\" name=\"detailsId\" value=\"{x["DetailsId"]}\"/><Input type=\"hidden\" name=\"employeeId\" value=\"{x["employeeId"]}\"/><Input type=\"hidden\" name=\"masterId\" value=\"{x["MasterId"]}\"/><Input type=\"hidden\" name=\"dailWage\" class=\"td-Salary\" value=\"{x["dailWage"]}\"/><Input type=\"hidden\" name=\"attendance\" value=\"{x["attendance"]}\"/>"
        //        }));
        //    var totalRecords = datatTable.Rows.Count;
        //    //Prepare JqGridData instance
        //    var response = new JqGridResponse
        //    {
        //        //Total pages count
        //        TotalPagesCount = (int) Math.Ceiling(totalRecords / (float) request.RecordsCount),

        //        //Page number
        //        PageIndex = request.PageIndex,
        //        //Total records count
        //        TotalRecordsCount = totalRecords
        //    };

        //    //IEnumerable<JqGridRecord> data = null;

        //    foreach (var d in data) response.Records.Add(d);
        //    //response.Records = data;
        //    response.Reader.RepeatItems = false;
        //    return new JqGridJsonResult {Data = response};
        //    //return db.Companies.ToList();
        //}

        //[HttpPost]
        //public string DailySalaryVoucher(DailySalaryVoucherDetailsInfo info)
        //{
        //    var txtEmployeeId = Request.Form["EmployeeId"].Split(',');
        //    var cmbStatus = Request.Form["Status"].Split(',');
        //    var txtWage = Request.Form["Wage"].Split(',');
        //    var txtMasterId = Request.Form["MasterId"].Split(',');

        //    var cmbCashorBankAccount = Request.Form["LegerId"];
        //    var txtVoucherNo = Request.Form["VoucherNo"];
        //    var dtpSalaryDate = Request.Form["SalaryDate"];
        //    var dtpDate = Request.Form["Date"];
        //    var txtNarration = Request.Form["Narration"];
        //    var lblTotalAmount = Request.Form["TotalAmount"];
        //    try
        //    {
        //        var infoMaster = new DailySalaryVoucherMasterInfo();
        //        var spMaster = new DailySalaryVoucherMasterSP();
        //        var infoDetails = new DailySalaryVoucherDetailsInfo();
        //        var spDetails = new DailySalaryVoucherDetailsSP();

        //        //-------------In multi user case check whether salary is paying for the same persone--------------//
        //        var inCounts = txtEmployeeId.Count();
        //        var incont = 0;
        //        decimal decVal = 0;
        //        for (var i = 0; i < inCounts; i++)
        //        {
        //            decVal = Convert.ToDecimal(txtEmployeeId[i]);
        //            if (spDetails.CheckWhetherDailySalaryAlreadyPaid(decVal, DateTime.Parse(dtpSalaryDate)) != "0")
        //            {
        //                strEployeeNames = strEployeeNames +
        //                                  spDetails.CheckWhetherDailySalaryAlreadyPaid(decVal,
        //                                      DateTime.Parse(dtpSalaryDate)) + ",";
        //                foreach (var ch in strEployeeNames)
        //                    if (ch == ',')
        //                        incont++;
        //                if (incont == 15)
        //                {
        //                    incont = 0;
        //                    strEployeeNames = strEployeeNames + Environment.NewLine;
        //                }
        //            }
        //        }

        //        if (spDetails.CheckWhetherDailySalaryAlreadyPaid(decVal, DateTime.Parse(dtpSalaryDate)) != "0")
        //        {
        //            return ("Salary already paid for - " + " " + strEployeeNames);
        //            //DailySalaryVoucherDetailsGridfill(isEditmode);
        //        }
        //        else
        //        {
        //            if (isAutomatic)
        //                infoMaster.VoucherNo = strVoucherNo;
        //            else
        //                infoMaster.VoucherNo = txtVoucherNo.Trim();
        //            infoMaster.Date = DateTime.Parse(dtpDate);
        //            infoMaster.SalaryDate = DateTime.Parse(dtpSalaryDate);
        //            infoMaster.LedgerId = decimal.Parse(cmbCashorBankAccount);
        //            infoMaster.Narration = txtNarration.Trim();
        //            infoMaster.TotalAmount = Convert.ToDecimal(lblTotalAmount);
        //            infoMaster.Extra1 = string.Empty; // Fields not in design//
        //            infoMaster.Extra2 = string.Empty; // Fields not in design//
        //            if (isAutomatic)
        //                infoMaster.InvoiceNo = strInvoiceNo;
        //            else
        //                infoMaster.InvoiceNo = txtVoucherNo.Trim();
        //            infoMaster.SuffixPrefixId = decDailySuffixPrefixId;
        //            infoMaster.VoucherTypeId = decDailyVoucherTypeId;
        //            infoMaster.FinancialYearId = PublicVariables._decCurrentFinancialYearId;

        //            var inval = 0;
        //            var inCount = txtEmployeeId.Count();
        //            for (var i = 0; i < inCount; i++)
        //                if (cmbStatus[i] == "Paid")
        //                    inval++;
        //            if (inval >= 0)
        //            {
        //                //-------------checks Voucher No. repeating in Multi user case----------//
        //                var dtbl = new DataTable();
        //                dtbl = spMaster.DailySalaryVoucherMasterAddWithIdentity(infoMaster, isAutomatic);
        //                foreach (DataRow dr in dtbl.Rows)
        //                {
        //                    decMasterId = Convert.ToDecimal(dr.ItemArray[0].ToString());
        //                    strUpdatedVoucherNo = dr.ItemArray[1].ToString();
        //                    strUpdatedInvoiceNo = dr.ItemArray[2].ToString();
        //                }

        //                if (!isAutomatic) strVoucherNo = txtVoucherNo;
        //                if (isAutomatic)
        //                    if (Convert.ToDecimal(strUpdatedVoucherNo) != Convert.ToDecimal(strVoucherNo))
        //                    {
        //                        //Messages.InformationMessage("Voucher number changed from  " + strInvoiceNo + "  to  " + strUpdatedInvoiceNo);
        //                        strVoucherNo = strUpdatedVoucherNo;
        //                        strInvoiceNo = strUpdatedInvoiceNo;
        //                    }

        //                //-------------------------------------//
        //                LedgerPosting(Convert.ToDecimal(cmbCashorBankAccount), txtVoucherNo, lblTotalAmount, dtpDate);

        //                infoDetails.DailySalaryVocherMasterId = decMasterId;
        //                infoDetails.Extra1 = string.Empty; // Fields not in design//
        //                infoDetails.Extra2 = string.Empty; // Fields not in design//
        //                var inRowCount = txtEmployeeId.Count();
        //                for (var i = 0; i < inRowCount; i++)
        //                {
        //                    if (txtEmployeeId[i] != null && txtEmployeeId[i] != string.Empty)
        //                        infoDetails.EmployeeId = Convert.ToDecimal(txtEmployeeId[i]);
        //                    if (txtWage[i] != null && txtWage[i] != string.Empty)
        //                        infoDetails.Wage = Convert.ToDecimal(txtWage[i]);
        //                    if (cmbStatus[i] != null && cmbStatus[i] != string.Empty) infoDetails.Status = cmbStatus[i];

        //                    if (cmbStatus[i] == "Paid" && txtMasterId[i] == string.Empty)
        //                    {
        //                        infoDetails.DailySalaryVocherMasterId = decMasterId;
        //                        spDetails.DailySalaryVoucherDetailsAdd(infoDetails);
        //                    }
        //                }

        //                return "success";
        //            }

        //            strVoucherNo = spMaster.DailySalaryVoucherMasterGetMax(decDailyVoucherTypeId);
        //            ModelState.AddModelError("", "Can't save without atleast one employee");
        //        }
        //    }
        //    catch (DbEntityValidationException invalids)
        //    {
        //        foreach (var inv in invalids.EntityValidationErrors)
        //        foreach (var field in inv.ValidationErrors)
        //            ModelState.AddModelError(field.PropertyName, field.ErrorMessage);
        //    }
        //    catch (Exception ex)
        //    {
        //        ModelState.AddModelError("", ex.Message);
        //    }

        //    if (ModelState.IsValid) return "success";
        //    return ModelState.Where(w => w.Value.Errors.Count > 0).FirstOrDefault().Value.Errors.FirstOrDefault()
        //        .ErrorMessage;
        //}

        
        #endregion
    }
}