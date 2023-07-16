using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using smsCore.Data;
using smsCore.Data.Helpers;
using Syncfusion.EJ2.Base;
using System.Data;
using System.Security.Claims;

namespace smsCore.Controllers
{
    //[CompressContent]
    public class HomeController : Controller
    {
        //private readonly int[] _campusIds;//= Helpers.CurrentUser.GetCampusIds();

        private readonly SchoolEntities objdb;
        private readonly CurrentUser user;
        private readonly Extensions1 _methods;
        public HomeController(SchoolEntities db, CurrentUser _user, Extensions1 extenstionMethods)
        {
            objdb = db;
            user = _user;
            //_campusIds = user.GetCampusIds();
            _methods = extenstionMethods;
        }
        [HttpGet]
        [Route("pages/{name}")]
        public IActionResult Page(string name)
        {
            var model = objdb.PublicPages.Where(w => w.PageName==name).FirstOrDefault();
            return View(model);
        }
        public IActionResult Index()
        {
            if (!objdb.Database.CanConnect())
            {
                return RedirectToAction("Install","Account");
            }
            var model = objdb.PublicPages.Where(w => w.PageName=="home").FirstOrDefault();
            return View(model);
        }

        public IActionResult About()
        {
           var model = objdb.PublicPages.Where(w => w.PageName=="about").FirstOrDefault();
            
            return View(model);
        }

        public IActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        [Authorize]
        public IActionResult Dashboard()
        {
            return View();
        }
        
        [Authorize]
        public ActionResult FeeList(DataManagerRequest dm)
        {            
            try
            {
                var _campusIds = user.GetCampusIds();
                var data = objdb.FeeSlips.AsNoTracking().Where(w => _campusIds.Contains(w.Admission.CampuseID)).Select(s => new
                {
                    Month = s.ForMonth,
                    Total = s.FeeSlipDetails.Select(fs=>fs.Amount).DefaultIfEmpty(0).Sum(fs => fs),
                    Received = s.FeeSlipReceipts.Select(fr=>fr.Amount).DefaultIfEmpty(0).Sum(fs => fs),
                    s.Admission.Campus.CampusName
                }).GroupBy(g => new { g.Month.Month, g.Month.Year, g.CampusName }).Select(s => new
                {
                    s.FirstOrDefault().Month,
                    s.Key.CampusName,
                    Total = s.Sum(t => t.Total),
                    Received = s.Sum(t => t.Received),
                    Balance = s.Sum(t => t.Total) - s.Sum(t => t.Received)
                });
                DataOperations operation = new DataOperations();
                if (dm.Sorted != null && dm.Sorted.Count > 0)
                {
                    data = operation.PerformSorting(data, dm.Sorted);
                }
                
                if (dm.Skip != 0)
                {
                    if (dm.Sorted.Count == 0)
                    {
                        List<Sort> sort = new List<Sort>() { new Sort { Name = "Month", Direction = "descending" } };
                        data = operation.PerformSorting(data, sort);
                    }
                    data = operation.PerformSkip(data, dm.Skip);
                }
                if (dm.Take != 0)
                {
                    data = operation.PerformTake(data, dm.Take);
                }
                return Json(new { result = data, count = data.Count() });
            }            
            catch (Exception)
            {
                return Json(new { }) ;
            }
        }


        [Authorize]
        public async Task<IActionResult> AttendanceBySelectedClassList(DataManagerRequest request, string datetime = "", int CampusID = -1)
        {
            User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            //DateTime date = Request["Time"] != null ? DateTime.Parse(Request["Time"]).Date : DateTime.Today.Date;
            // if (datetime != "")
            // {
            //     date = DateTime.Parse(datetime);
            // }
            if (CampusID == -1)
            {
                var _campusIds = user.GetCampusIds();
                CampusID = _campusIds[0];
            }
            var classes =await objdb.ClassSections
    .AsNoTracking()
    .Where(w => w.CampusID == CampusID && !w.Admissions.Any(ww => ww.IsExpell))
    .OrderBy(o => o.ClassID)
    .ThenBy(o => o.SectionID)
    .ToListAsync();// objdb.ClassSections.AsNoTracking().Where(w => w.CampusID == CampusID && w.Admissions.Where(ww=>!ww.IsExpell).Any()).OrderBy(o => new { o.ClassID, o.SectionID }).ToListAsync();
            int present = (int)PublicVariables.AttendanceType.Present;
            int absent = (int)PublicVariables.AttendanceType.Absent;
            int leave = (int)PublicVariables.AttendanceType.Leave;
            int late = (int)PublicVariables.AttendanceType.LateComing;
            DateTime today = DateTime.Now.Date;// DateTimeHelper.ConvertDate(datetime, false, "dd/MM/yyyy hh:mm tt").Date;
            var data = classes.Select(s =>
                 new
                 {
                     s.ID,
                     s.CampusID,
                     s.Class.ClassName,
                     s.Section.SectionName,
                     Present = objdb.StudentAttendences.Count(c => c.Admission.ClassSectionID == s.ID & (c.AttendanceDate.Date) == today & c.AttendanceTypeID == present),
                     Absent = objdb.StudentAttendences.Count(c => c.Admission.ClassSectionID == s.ID & (c.AttendanceDate.Date) == today & c.AttendanceTypeID == absent),
                     Leave = objdb.StudentAttendences.Count(c => c.Admission.ClassSectionID == s.ID & (c.AttendanceDate.Date) == today & c.AttendanceTypeID == leave),
                     LateComing = objdb.StudentAttendences.Count(c => c.Admission.ClassSectionID == s.ID & (c.AttendanceDate.Date) == today & c.AttendanceTypeID == late),
                     NoAttendance = s.Admissions.Count(c => !c.IsExpell & !c.StudentAttendences.Any(ct => (ct.AttendanceDate.Date) == today)),
                     Total = s.Admissions.Count(c => s.CampusID == CampusID & !c.IsExpell)
                 });

            return Json(new { result = data, count = data.Count() });
        }

        [Authorize]
        public async Task<JsonResult> DashboardData()
        {
            var _campusIds = user.GetCampusIds();
            var lblStudentTotal = (await objdb.Admissions.Where(w => _campusIds.Contains(w.CampuseID) && !w.IsExpell).CountAsync()).ToString();

            DateTime date = DateTime.Today.Date;

            var presentId = _methods.GetForStudentId(PublicVariables.AttendanceType.Present);//.GetForStudentId();
            var lateId = _methods.GetForStudentId(PublicVariables.AttendanceType.LateComing);//.GetForStudentId();
            var absentId = _methods.GetForStudentId(PublicVariables.AttendanceType.Absent);//.GetForStudentId();
            var leaveId = _methods.GetForStudentId(PublicVariables.AttendanceType.Leave);//.GetForStudentId();
            var lblStdPresent = await objdb.Admissions.CountAsync(w => _campusIds.Contains(w.CampuseID) && !w.IsExpell && w.StudentAttendences.Any(s => s.AttendanceDate.Date == date && (s.AttendanceTypeID == presentId || s.AttendanceTypeID == lateId)));
            var lblStdLeave = await objdb.Admissions.CountAsync(w => _campusIds.Contains(w.CampuseID) && !w.IsExpell && w.StudentAttendences.Any(s => s.AttendanceDate.Date == date && s.AttendanceTypeID == leaveId));
            var lblStdAbsent = await objdb.Admissions.CountAsync(w => _campusIds.Contains(w.CampuseID) && !w.IsExpell && w.StudentAttendences.Count(s => s.AttendanceDate.Date == date && s.AttendanceTypeID == absentId) > 0);

            var lblEmployeeTotal = await objdb.tbl_Employee.CountAsync(w => _campusIds.Contains(w.CampusID) && w.LeavedStaffs.Count == 0);
            var classes = await objdb.Classes.Where(w => w.ClassSections.Where(c => _campusIds.Contains(c.CampusID)).Any()).CountAsync();
            presentId = _methods.GetForEmployeeId(PublicVariables.AttendanceType.Present);//.GetForEmployeeId();
            lateId = _methods.GetForEmployeeId(PublicVariables.AttendanceType.LateComing);//.GetForEmployeeId();
            absentId = _methods.GetForEmployeeId(PublicVariables.AttendanceType.Absent);//.GetForEmployeeId();
            leaveId = _methods.GetForEmployeeId(PublicVariables.AttendanceType.Leave);//GetForEmployeeId();
            var lblEmployeePresent = await objdb.tbl_Employee.CountAsync(w => _campusIds.Contains(w.CampusID) && w.EmployeeAttendances.Any(a => a.AttendanceDate.Date == date && (a.AttendanceTypeID == presentId || a.AttendanceTypeID == lateId)));
            var lblEmployeeLeave = await objdb.tbl_Employee.CountAsync(w => _campusIds.Contains(w.CampusID) && w.EmployeeAttendances.Any(a => a.AttendanceDate.Date == date && a.AttendanceTypeID == leaveId));
            var lblEmployeeAbsent = await objdb.tbl_Employee.CountAsync(w => _campusIds.Contains(w.CampusID) && w.EmployeeAttendances.Any(a => a.AttendanceDate.Date == date && a.AttendanceTypeID == absentId));

            var lblPrentsTotal = objdb.Admissions
    .Where(w => (_campusIds.Contains(w.CampuseID)) && (!w.IsExpell))
    .Select(s => new { s.StudentID, CNIC = s.Student != null ? s.Student.CNIC : null })
    .Distinct()
    .Count(); // objdb.Admissions.Where(w => _campusIds.Contains(w.CampuseID) && !w.IsExpell).Select(s => new { s.StudentID, s.Student.CNIC }).DistinctBy(d => d.CNIC).Count();
            var freeAdmission = await objdb.Admissions.CountAsync(w => _campusIds.Contains(w.CampuseID) && !w.IsExpell && w.Student.FreeAdmissions.Count > 0);


            return Json(new
            {
                classes,
                lblStudentTotal,
                lblStdPresent,
                lblStdLeave,
                lblStdAbsent,
                lblEmployeeTotal,
                lblEmployeePresent,
                lblEmployeeLeave,
                lblEmployeeAbsent,
                lblPrentsTotal,
                freeAdmission
            });
        }

        [Authorize]
        public async Task<JsonResult> BirthDays()
        {
            var _campusIds = user.GetCampusIds();
            DateTime today = DateTime.Today.Date;
            var stds = objdb.Students.Where(w => w.DOB.Month == today.Date.Month && w.DOB.Day == today.Day && w.Admissions.Where(a => _campusIds.Contains(a.CampuseID)).Any()).ToList().Select(s => new
            {
                Photo = _methods.GetPhoto(s.StudentPhotos.Select(p => p.StudentImage).LastOrDefault()),
                s.StudentName,
                DOB = s.DOB.ToString("dd/MM/yyyy"),
                Admission = s.Admissions.Select(a => new
                {
                    a.Campus.CampusName,
                    a.ClassSection.Class.ClassName
                })
            });
            return Json(stds);
        }
    }
}