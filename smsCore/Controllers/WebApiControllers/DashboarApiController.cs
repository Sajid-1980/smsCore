using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Models;
using sms.Helpers;
using System.Threading.Tasks;
using System.Data.Entity;
using smsCore.Data;
using Utilities;
using Microsoft.AspNetCore.Mvc;

namespace sms.WebApiControllers
{
    [Route("api/dashboard")]
    public class DashboarApiController : Controller
    {
        private readonly SchoolEntities db;
        private readonly CurrentUser _user;
        private readonly AttendanceHelper attendanceHelper;
        public DashboarApiController(SchoolEntities _db,CurrentUser Cuser, AttendanceHelper _attendanceHelper)
        {
            db = _db;
            _user = Cuser;
            attendanceHelper = _attendanceHelper;
        }

       
        [HttpGet]
        [Route("student-chart-data")]
        public object StudentsChart(string campusIds)
        {
            var campsId = _user.GetCampusIds();

            var iData = new List<object>();
            //var objdb = new SchoolEntities();
            //var students = objdb.Admissions.Where(w => !w.IsExpell).OrderBy(o => o.ClassSection.ClassID).Select(s => new { s.ClassSection.Class.ClassName, s.ClassSection.Section.SectionName, s.ID, s.ClassSection.ClassID }).ToList();
            var students = db.Admissions.Where(w => !w.IsExpell & campsId.Contains(w.CampuseID))
                .GroupBy(g => new {g.ClassSection.Class.ClassName, g.ClassSection.Section.SectionName}).Select(s => new
                {
                    s.Key.ClassName,
                    s.Key.SectionName,
                    Value = s.Count()
                }).ToList();

            //var students = objdb.ClassSections.Where(w =>
            //campsId.Contains(w.Admissions.Select(ss => ss.CampuseID).FirstOrDefault())).OrderBy(o => o.ClassID).
            //Select(s => new
            //{
            //    s.Class.ClassName,
            //    s.Section.SectionName,
            //    Value = s.Admissions.Where(w => campsId.Contains(w.CampuseID) && !w.IsExpell).Count()
            //}).ToList();

            var subData = new List<object>();

            iData.Add((from student in students select student.ClassName).Distinct());
            var rnd = new Random();

            foreach (var s in students.Select(s => s.SectionName).Distinct())
                subData.Add(new
                {
                    label = s,
                    backgroundColor = "#" + Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256)).Name,
                    borderColor = "#" + Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256)).Name,
                    borderWidth = 1,
                    data = students.Where(w => w.SectionName == s).Select(ss => ss.Value).ToArray()
                });
            iData.Add(subData);

            //Source data returned as JSON  
            return Json(iData);
        }

        [HttpGet]
        [Route("session-chart-data")]
        public async Task<object> SessionChart(int campusId = -1)
        {
            campusId = campusId == -1 ? _user.GetCampusIds().FirstOrDefault() : campusId;
            var iData = new List<object>();

            var year =await db.FinancialYears.Where(w=>w.IsCurrent).FirstOrDefaultAsync();
            iData.Add(new
            {
                lblSessionInfId = year.financialYearId.ToString(),
                lblSessionStartDate = year.fromDate.ToString("dd - MMMM - yyyy"),
                lblSessionEndDate = year.toDate.ToString("dd - MMMM - yyyy"),
                lblSessiontotalDays = (year.toDate - year.fromDate).TotalDays.ToString()
            });

            iData.Add(new[] {"Total", "Working", "Remaining", "Holidays"});
            //var attendanceHelper = new AttendanceHelper(year.Id, year.toDate);
            

            //iData.Add(new[]
            //{
            //    (decimal) (year.toDate - year.fromDate).TotalDays,
            //    attendanceHelper.GetTotalWorkingDaysInSession(campusId),
            //    attendanceHelper.GetTotalRemainingWorkingDaysInSession(campusId, DateTime.Now.Date),
            //    attendanceHelper.GetTotalHolidaysInSession(campusId)
            //});
 
            return Json(iData);
        }

        [HttpGet]
        [Route("notifications")]
        public async Task<object> Notification()
        {
           var ns = db.Notifications.Select(s => new { });
            return ns;
        }
    }
}