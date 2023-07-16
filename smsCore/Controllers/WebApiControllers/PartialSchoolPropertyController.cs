using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Newtonsoft.Json;
using smsCore.Data;

namespace sms.WebApiControllers
{
    [Authorize]
    [Route("api/selector")]
    
    public class PartialSchoolPropertyController : Controller
    {

        private readonly SchoolEntities db;
        private readonly CurrentUser _user;
        public PartialSchoolPropertyController(SchoolEntities _db,CurrentUser Cuser)
        {
            db = _db;
            _user = Cuser;
        }

        [HttpGet]
        [Route("sessions")]
        public string GetSessions(bool search)
        {
            
            var sessions = db.FinancialYears.ToList().Select(s => new { id = s.financialYearId, text = s.fromDate.ToString("MMM, yyyy") + " - " + s.toDate.ToString("MMM, yyyy"), selected=s.IsCurrent }).ToList();

            if (search) sessions.Insert(0, new { id = -1, text = "All", selected = false });
            return JsonConvert.SerializeObject(sessions);
        }

        [HttpGet]
        [Route("campuses")]
        public string GetCampus(bool search)
        {
            
            var campuses = db.Campuses.Select(s => new {id = s.ID, text = s.CampusName}).ToList();
            if (search) campuses.Insert(0, new {id = -1, text = "All"});
            return JsonConvert.SerializeObject(campuses);
        }

        [HttpGet]
        [Route("classes/{search}")]
        public string GetClasses(bool search, int employeeId = -1, bool classteacher=true)
        {
            
            if (employeeId != -1)
            {
                if (!classteacher)
                {
                    var data = db.TeachingSubjects.Where(w => w.StaffID == employeeId)
                        .Select(s => new { id = s.ClassSection.ClassID, text = s.ClassSection.Class.ClassName }).Distinct()
                        .ToList();
                    if (search) data.Insert(0, new { id = -1, text = "All" });
                    return JsonConvert.SerializeObject(data);
                }
                else
                {
                var data = db.TeachingClasses.Where(w => w.StaffID == employeeId)
                    .Select(s => new {id = s.ClassSection.ClassID, text = s.ClassSection.Class.ClassName}).Distinct()
                    .ToList();
                if (search) data.Insert(0, new {id = -1, text = "All"});
                return JsonConvert.SerializeObject(data);

                }
            }

            var campuses = db.Classes.Select(s => new {id = s.ID, text = s.ClassName}).ToList();
            if (search) campuses.Insert(0, new {id = -1, text = "All"});


            return JsonConvert.SerializeObject(campuses);
        }

        [HttpGet]
        [Route("sections/{classid}/{campusId}")]
        public string GetSections(int classid, int campusId, bool search = false, int employeeId = -1, bool classteacher = true)
        {
            if (employeeId != -1)
            {
                if (!classteacher) { 
                var data = db.TeachingSubjects.Where(w => w.StaffID == employeeId && w.ClassSection.ClassID==classid).Select(s =>
                    new {id = s.ClassSection.SectionID, text = s.ClassSection.Section.SectionName}).Distinct().ToList();
                //if (search) data.Insert(0, new {id = -1, text = "All"});
                return JsonConvert.SerializeObject(data);
                }
                else
                {
                    var data = db.TeachingClasses.Where(w => w.StaffID == employeeId  && w.ClassSection.ClassID==classid).Select(s =>
                    new { id = s.ClassSection.SectionID, text = s.ClassSection.Section.SectionName }).Distinct().ToList();
                    //if (search) data.Insert(0, new {id = -1, text = "All"});
                    return JsonConvert.SerializeObject(data);
                }
            }

            var campuses = db.ClassSections.Where(w => w.CampusID == campusId && w.ClassID == classid)
                .Select(s => new {id = s.Section.ID, text = s.Section.SectionName}).ToList();

            if (search) campuses.Insert(0, new {id = -1, text = "All"});
            return JsonConvert.SerializeObject(campuses);
        }

        [HttpGet]
        [Route("subjects/{classid}/{campusId}")]
        public string GetSubjects(bool search, int classid, int campusId, int employeeId = -1)
        {
            
            if (employeeId != -1)
            {
                var data = db.TeachingSubjects.AsNoTracking().Where(w => w.StaffID == employeeId && w.ClassSection.ClassID==classid)
                    .Select(s => new {id = s.SubjectId, text = s.Subject.SubjectName}).Distinct().ToList();
                //if (search) data.Insert(0, new {id = -1, text = "All"});
                return JsonConvert.SerializeObject(data);
            }
            var campuses = db.ClassSubjects.AsNoTracking().Where(w => (!w.IsDeleted.HasValue || !w.IsDeleted.Value) && w.CampusID == campusId && w.ClassID == classid)
                .Select(s => new {id = s.SubjectID, text = s.Subject.SubjectName}).ToList();
            if (search) campuses.Insert(0, new {id = -1, text = "All"});
            return JsonConvert.SerializeObject(campuses);
        }

        [HttpGet]
        [Route("feegroups/{classid}/{campusId}")]
        public string GetFeeGrups(bool search, int classid, int campusId,bool showFree=true)
        {
            //var db = new ();
            var campuses = db.ClassFeeGroups.AsNoTracking().Where(w => w.CampusID == campusId && w.ClassID == classid)
                .Select(s => new {id = s.FeeGroupID, text = s.FeeGroup.FeeGroupName}).ToList();
            if (search) campuses.Insert(0, new {id = -1, text = "All"});
            if (showFree) campuses.Add( new { id = -2, text = "F (Free)" });
            
            return JsonConvert.SerializeObject(campuses);
        }


        [Route("exams/{search}/{campusId}")]
        [HttpGet]
        public string Getexams(bool search, int campusId,bool loadlist=false)
        {
            if (loadlist)
            {
                var data = db.Exams.Select(s => new { id=s.ID, text=s.ExamName }).ToList();
                if (search) data.Insert(0, new { id = -1, text = "All" });
                return JsonConvert.SerializeObject(data);
            }

            var campuses = db.ExamHelds.AsNoTracking().Where(w =>w.VeiwAble & w.CampusID == campusId).Include(i=>i.Exam).ToList().Select(s =>
                new {id = s.ID, text = s.Exam.ExamName + " (" + s.EntryDate.ToString("MMMM, yyyy") + ")"}).ToList();
            if (search) campuses.Insert(0, new {id = -1, text = "All"});
            return JsonConvert.SerializeObject(campuses);
        }

        [Route("employee/{search}/{campusId}")]
        [HttpGet]
        public string GetEmployee(bool search, int campusId)
        {
            var campuses = db.tbl_Employee.AsNoTracking().Where(w => w.CampusID == campusId).ToList().Select(s =>
                new { id = s.Id, text = s.employeeName }).ToList();
            if (search) campuses.Insert(0, new { id = -1, text = "All" });
            return JsonConvert.SerializeObject(campuses);
        }
    }
}