using Models;
using Newtonsoft.Json;
using smsCore.Data;
using smsCore.Data.Helpers;
using Syncfusion.EJ2.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace sms.WebApiControllers
{
    [Route("api/students")]
    [Authorize]
    public class StudentsApiController : Controller
    {
        private readonly SchoolEntities db;
        private readonly CurrentUser _user;
        private readonly Extensions1 _method;

        public StudentsApiController(SchoolEntities _db,CurrentUser currentUser, Extensions1 method)
        {
            db = _db;
            _user = currentUser;
            _method = method;

        }

        [HttpGet]
        [Route("drivers/{campusid}")]
        public string GetDrivers(int campusid)
        {
            //var db = new SchoolEntities();
            var campuses = db.Drivers.Where(w => w.CampusID == campusid).Select(s => new { id = s.ID, text = s.DriverName }).ToList();
            return JsonConvert.SerializeObject(campuses);
        }
        [HttpGet]
        [Route("get-student-info/{regno}")]
        public string GetStudentInfo(int regno)
        {
            var campusIds = _user.GetCampusIds();
            var std = db.Admissions.Include(i=>i.Student).ThenInclude(i=>i.StudentPhotos).Include(i=>i.Campus).Include(i=>i.ClassSection).ThenInclude(i=>i.Section).Include(i=>i.ClassSection.Class).Where(w => campusIds.Contains(w.CampuseID) && w.Student.RegistrationNo == regno)
                .ToList().Select(s => new
                {
                    s.Student.StudentName, s.Campus.CampusName, s.ClassSection.Class.ClassName,
                    s.ClassSection.Section.SectionName,s.CampuseID,
                    photo = _method.GetPhoto(s.Student.StudentPhotos.Select(p => p.StudentImage).LastOrDefault())

                }).LastOrDefault();
            return JsonConvert.SerializeObject(std);
        }
        [HttpGet]
        [Route("get-student-timeline/{studentId}")]
        public JsonResult TimeLine(int studentId)
        {
            var student = db.Students.Find(studentId);
            var admission = student.Admissions.Select(s => new { s.Session, s.ClassSection.Class.ClassName, s.ClassSection.Section.SectionName, s.Campus.CampusName }).OrderBy(o => o.Session).ToList().Select(s => new
            {
                sortdate = DateTimeHelper.ConvertDate($"01/01/{s.Session}",false,"dd/MM/yyyy"),
                date = s.Session.ToString(),
                content = $"Got Admission in Campus: {s.CampusName} Class: {s.ClassName} Section: {s.SectionName}"
            });

            var Fee = db.FeeSlips.Where(w => w.Admission.StudentID == studentId).Select(s => new { s.ForMonth, Amount = s.FeeSlipDetails.Sum(f => f.Amount) }).ToList().Select(s => new
            {
                sortdate = s.ForMonth,
                date=s.ForMonth.ToString("MMM, yyyy"),
                content = $"Issued Fee slip . Amount {s.Amount}"
            });

            var FeeReceived = db.FeeSlips.Where(w => w.Admission.StudentID == studentId & w.FeeSlipReceipts.Count>0).
                Select(s => new { s.ForMonth, Amount = s.FeeSlipReceipts.Sum(f=>f.Amount), date = s.FeeSlipReceipts.Select(f => f.EntryDate).FirstOrDefault() }).ToList().Select(s => new
            {
                sortdate = s.date,
                date = s.date.ToString("dd MMM, yyyy"),
                content = $"Fee Received . Amount {s.Amount} for month {s.ForMonth.ToString("MMM, yyyy")}"
            });

           var data = admission.Union(Fee).Union(FeeReceived).OrderBy(o => o.sortdate).Select(s=>new {s.date,s.content });
           return  Json(new { data });
        }

        [HttpGet]
        [Route("getdataforrole")]
        public JsonResult GetDataForRole(string role, string keyword)
        {
            
             DataOperations operation = new DataOperations();
            if (role=="Parent")
            {
                var data = db.Students.OrderBy(o=>o.ID).Select(s => new {s.ID, s.CNIC, s.FName, s.StudentName, s.RegistrationNo, s.Email }).DistinctBy(db => db.CNIC);
               
                if (!string.IsNullOrEmpty(keyword))
                {
                    SearchFilter filter = new SearchFilter() { Fields=new List<string> { "CNIC", "FName", "StudentName", "RegistrationNo", "Email" }, Key=keyword, Operator="contains" };
                    data = operation.PerformSearching(data, new List<SearchFilter> { filter });  //Search
                }
                return Json(new { data });
            }
            else if (role == "Teacher")
            {

                var data = db.tbl_Employee.Select(s => new {s.Id, s.employeeCode, s.employeeName, s.FatherName, s.CNIC,s.email });
                if (!string.IsNullOrEmpty(keyword))
                {
                    SearchFilter filter = new SearchFilter() { Fields=new List<string> { "employeeCode", "employeeName", "FatherName","CNIC", "email" }, Key=keyword, Operator="contains" };
                    data = operation.PerformSearching(data, new List<SearchFilter> { filter });  //Search
                }
                return Json(new { data });
            }
            else if (role=="Student")
            {
                var data = db.Students.Select(s => new {s.ID, s.RegistrationNo, s.StudentName, s.FName,s.Email });
                if (!string.IsNullOrEmpty(keyword))
                {
                    SearchFilter filter = new SearchFilter() { Fields=new List<string> { "StudentName", "FName", "RegistrationNo", "Email" }, Key=keyword, Operator="contains" };
                    data = operation.PerformSearching(data, new List<SearchFilter> { filter });  //Search
                }
                return Json(new { data });
            }
            return Json(new { });
        }

    }
}