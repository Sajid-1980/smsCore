using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using smsCore.Data.Helpers;
using Syncfusion.EJ2.Base;
using smsCore.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using smsCore.Data.Models;
using Newtonsoft.Json;

namespace smsCore.Controllers
{
    //[CompressContent]
    [Authorize]
    public class TimeTableController : BaseController
    {


        private readonly SchoolEntities db ;
        private readonly int[] campusIds;
        private readonly CurrentUser _user;
        private readonly ClsBussinessSetting setting;
        private readonly SelectListHelper SelectListHelper;


        public TimeTableController(SchoolEntities _db , CurrentUser  user, ClsBussinessSetting _setting, SelectListHelper _SelectListHelper)
        {
            db = _db;
            _user = user;
            campusIds = user.GetCampusIds(); 
            setting = _setting;
            SelectListHelper = _SelectListHelper;
        }


        public ActionResult Index()
        {
            return View();
        }

        public string GetStaff(decimal? id, int campusId)
        {
            var staff = db.tbl_Employee.Where(w => w.CampusID == campusId && w.LeavedStaffs.Count < 1).ToList()
                .Select(s => new {ID = s.Id, StaffName = s.employeeName + " - " + s.employeeCode}).ToList();
            staff.Insert(0, new {ID = 0, StaffName = "---select---"});
            var list = new SelectList(staff, "ID", "StaffName", decimal.Round(id.HasValue ? id.Value : 0,0));

            var select = "<select name=\"StaffIDs\" id=\"staffId\">";
            foreach (var itm in list)
            {
                var selected = "";
                if (itm.Selected) selected = "selected=\"true\"";
                select += $"<option value=\"{itm.Value}\" {selected}>{itm.Text}</option>";
            }

            select += "</select>";
            return select;
        }

        
        
        public IActionResult TimeTableAutoDesigner()
        {
            return View();
        }

        [HttpPost]
        public string TimeTableAutoDesigner(List<StaffTimeTable> finalTimeTable)
        {
            var message = "No data found to Save.";
            if (finalTimeTable.Count > 0)
            {
                var parentId = 1;
                try
                {
                    parentId = db.TimeTables.Select(s => s.ParentID).Max() + 1;
                }
                catch
                {
                }


                foreach (var t in finalTimeTable)
                {
                    var subject = db.Subjects.Find(t.SubjectID)?.SubjectName;
                    var employeename = db.tbl_Employee.Find(t.StaffID)?.employeeName;
                    var campusName = db.Campuses.Find(t.CampusID)?.CampusName;
                    var d = new TimeTable
                    {
                        CampusID = t.CampusID,
                        CampusName = campusName,
                        ClassID = t.ClassID,
                        ClassName = db.Classes.Find(t.ClassID).ClassName,
                        EmployeeName = employeename,
                        ParentID = parentId,
                        Pno = t.Period,
                        SectionID = t.SectionID,
                        SectionName = db.Sections.Find(t.SectionID).SectionName,
                        StaffID = t.StaffID,
                        SubjectID = t.SubjectID,
                        SubjectName = subject == null ? t.SubjectName : subject,
                        TimeFrom = t.TimeFromDate,
                        TimeTo = t.TimeToDate
                    };
                    db.TimeTables.Add(d);
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
            }

            return message;
        }

        public string btnSaveConfigurationClick(TimeTableConfig config)
        {
            setting.CampusId = (config.CampusID);
            setting.WriteorUpdate("FirstPeriodStart", config.SchoolStartTime.ToString());
            setting.WriteorUpdate("PeriodDuration", config.DurationofPeriod.ToString());
            setting.WriteorUpdate("OtherPeriods", JsonConvert.SerializeObject(config.otherPeriods));
            setting.WriteorUpdate("PerDayPeriod", config.PerDayPeriod.ToString());
            return "success";
        }


        public IActionResult TimeTableInManual()
        {
           var  TeachingSubjectList = db.TeachingSubjects.ToList();
            ViewBag.compusList = SelectListHelper.GetCampusSelectList();
            ViewBag.ClassList = SelectListHelper.GetClassSelectList();
            ViewBag.Section = SelectListHelper.GetSectionFeeGroupList();
            return View(TeachingSubjectList);
        }

        
        [HttpPost]
        public string TimeTableInManual(TeachingSubject teaching)
        {
            var message = "failed";
            string[] Id = { };
            if (Request.Form["Pk"].ToString() != null) Id = Request.Form["Pk"].ToString().Split(',');
            var dgvcmbStaffID = Request.Form["StaffID"].ToString().Split(',');
            var SubjectName = Request.Form["SubjectName"].ToString().Split(',');

            var SubjectId = Request.Form["SubjectID"].ToString().Split(',');
            var cmbClassSection = Request.Form["SectionId"];
            var Pno = Request.Form["Pno"].ToString().Split(',');
            var startime = Request.Form["TimeFrom"].ToString().Split(',');
            var endtime = Request.Form["TimeTo"].ToString().Split(',');
            var campusId = int.Parse(Request.Form["compusId"].ToString()   );
            var classId = int.Parse(Request.Form["ClassID"].ToString()  );
            var SectionId = int.Parse(Request.Form["SectionId"].ToString());
            var staffid = Request.Form["StaffID"].ToString().Split(',');
            for (var i = 0; i < SubjectId.Count(); i++)
            {
                if (!int.TryParse(SubjectId[i], out var SubjectIds) && !(SubjectIds > 0)) continue;

                if (!int.TryParse(Pno[i], out var pno) && !(pno > 0)) continue;

                if (!DateTime.TryParse("01/01/1901 " + startime[i], out var timeFrom)) continue;

                if (!DateTime.TryParse("01/01/1901 " + endtime[i], out var timeTo)) continue;

                var timeTable = new TimeTable();
                if (int.TryParse(Id[i], out var ids) && ids > 0)
                {
                    timeTable = db.TimeTables.Where(w => w.ID == ids).FirstOrDefault();
                    if (timeTable != null)
                    {
                        timeTable.CampusID = campusId;
                        timeTable.CampusName = db.Campuses.Where(w => w.ID == campusId).Select(s => s.CampusName)
                            .FirstOrDefault();
                        timeTable.ClassID = classId;
                        timeTable.ClassName = db.Classes.Where(w => w.ID == classId).Select(s => s.ClassName)
                            .FirstOrDefault();
                        timeTable.EmployeeName = string.Empty;
                        timeTable.ParentID = 1;
                        timeTable.SectionID = SectionId;
                        timeTable.SectionName = db.ClassSections.Where(w => w.ID == SectionId)
                            .Select(s => s.Section.SectionName).FirstOrDefault();
                        timeTable.SubjectID = int.Parse(SubjectId[i]);
                        var subjid = int.Parse(SubjectId[i]);

                        timeTable.SubjectName = SubjectName[i];
                    }
                }
                else
                {
                    db.TimeTables.Add(timeTable);
                    timeTable.CampusID = campusId;
                    timeTable.CampusName = db.Campuses.Where(w => w.ID == campusId).Select(s => s.CampusName)
                        .FirstOrDefault();
                    timeTable.ClassID = classId;
                    timeTable.ClassName =
                        db.Classes.Where(w => w.ID == classId).Select(s => s.ClassName).FirstOrDefault();
                    timeTable.EmployeeName = string.Empty;
                    timeTable.ParentID = 1;
                    timeTable.SectionID = SectionId;
                    timeTable.SectionName = db.ClassSections.Where(w => w.ID == SectionId)
                        .Select(s => s.Section.SectionName).FirstOrDefault();
                    timeTable.SubjectID = int.Parse(SubjectId[i]);
                    var subjid = int.Parse(SubjectId[i]);

                    timeTable.SubjectName = SubjectName[i];
                }

                try
                {
                    timeTable.StaffID = int.Parse(staffid[i]);
                }
                catch
                {
                    timeTable.StaffID = 0;
                }

                timeTable.TimeFrom = timeFrom;
                timeTable.TimeTo = timeTo;
                timeTable.Pno = pno;
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

        
        public ActionResult TimeTableInManualEdit(int SubjectId, int CampusId, int ClassId, int SectionId, int StaffId,
            string subjectName = "")
        {
            var SubjectName = SubjectId == 0 ? subjectName : db.Subjects.Find(SubjectId).SubjectName;
            var timeTable = db.TimeTables.Where(w =>
                w.CampusID == CampusId && w.ClassID == ClassId && w.SectionID == SectionId &&
                w.SubjectName == SubjectName).FirstOrDefault();
            if (timeTable == null)
            {
                timeTable = new TimeTable();
                timeTable.CampusID = CampusId;
                timeTable.ClassID = ClassId;
                timeTable.EmployeeName = string.Empty;
                timeTable.ParentID = 1;
                timeTable.SectionID = SectionId;
                timeTable.SubjectID = SubjectId;
                timeTable.SubjectName = SubjectName;
                timeTable.StaffID = StaffId;
            }

            return View(timeTable);
        }

        
        [HttpPost]
        public string TimeTableInManualEdit(TimeTable model)
        {
            var message = "";
            var timeTable = db.TimeTables.Where(w =>
                w.CampusID == model.CampusID && w.ClassID == model.ClassID && w.SectionID == model.SectionID &&
                w.SubjectName == model.SubjectName).FirstOrDefault();
            if (timeTable == null)
            {
                timeTable = new TimeTable();
                timeTable.CampusID = model.CampusID;
                timeTable.CampusName = db.Campuses.Where(w => w.ID == model.CampusID).Select(s => s.CampusName)
                    .FirstOrDefault();
                timeTable.ClassID = model.ClassID;
                timeTable.ClassName = db.Classes.Where(w => w.ID == model.ClassID).Select(s => s.ClassName)
                    .FirstOrDefault();
                timeTable.EmployeeName = string.Empty;
                timeTable.ParentID = 1;
                timeTable.SectionID = model.SectionID;
                timeTable.SectionName = db.ClassSections.Where(w => w.ID == model.SectionID)
                    .Select(s => s.Section.SectionName).FirstOrDefault();
                timeTable.SubjectID = model.SubjectID;
                timeTable.SubjectName = model.SubjectName; 
                timeTable.StaffID = model.StaffID;
                db.TimeTables.Add(timeTable);
            }

            timeTable.TimeFrom = model.TimeFrom;
            timeTable.TimeTo = model.TimeTo;
            timeTable.Pno = model.Pno;

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

        
        public ActionResult TimeTableView(TimeTableConfig config)
        {
            int.TryParse(Request.Form["Campus"], out var CampusID);
            config.CampusID = CampusID;
            var _finalTimeTable = AutoGenerateTimeTable(config);
            return View("_TimeTableView", _finalTimeTable);
        }

        public string GetAutoTimeTableJSON(TimeTableConfig config)
        {
            var _finalTimeTable = AutoGenerateTimeTable(config);
            return JsonConvert.SerializeObject(_finalTimeTable);
        }

        [HttpPost]
        public JsonResult TeachingSubjectsList(DataManagerRequest request, int classId, int sectionId,
            int campusid)
        {
            
            var existing = db.TeachingSubjects.Where(w =>
                w.CampusID == campusid && w.ClassSection.ClassID == classId &&
                w.ClassSection.SectionID == sectionId && w.CloseDate == null).ToList();
            //var data = db.ClassSubjects.Where(w => w.CampusID == campusid && w.ClassID == classId).ToList().Select(s =>
            //    new
            //    {
            //        ID = existing.Where(w => w.SubjectId == s.SubjectID).Select(t => t.ID).FirstOrDefault(),
            //        SubjectId = s.SubjectID,
            //        s.Subject.SubjectName,
            //        StaffID = existing.Where(w => w.SubjectId == s.SubjectID).Select(t => t.StaffID).FirstOrDefault(),
            //        StartDate = existing.Where(w => w.SubjectId == s.SubjectID).FirstOrDefault() == null
            //            ? ""
            //            : existing.Where(w => w.SubjectId == s.SubjectID).Select(t => t.StartDate).FirstOrDefault()
            //                .ToString("dd-MMM-yyyy")
            //    }).ToList();


            var data = db.ClassSubjects
    .Where(w => w.CampusID == campusid && w.ClassID == classId)
    .ToList()
    .Select(s => new
    {
        ID = existing?.Where(w => w.SubjectId == s.SubjectID).Select(t => t.ID).FirstOrDefault(),
        SubjectId = s.SubjectID,
        s.Subject.SubjectName,
        StaffID = existing?.Where(w => w.SubjectId == s.SubjectID).Select(t => t.StaffID).FirstOrDefault(),
        StartDate = existing?.Where(w => w.SubjectId == s.SubjectID).FirstOrDefault()?.StartDate
                        .ToString("dd-MMM-yyyy") ?? ""
    })
    .ToList();




            var dataa = data.ToList().Select(s => new
            {
                s.ID,
                s.SubjectName,
                s.StartDate,
                teacher = GetStaff(s.StaffID, campusid),
                s.StaffID,
                s.SubjectId
          });

            return Json(new { result = dataa, count = dataa.Count() });
        }

        public ActionResult TimeTableManualList(DataManagerRequest request, int classId, int SectionId, int campusid)
        {
            var config = new TimeTableConfig(campusid, true);
            var timeTables = new List<TimeTable>();
            var classSectionId = db.ClassSections
                .Where(w => w.CampusID == campusid && w.ClassID == classId && w.SectionID == SectionId)
                .Select(s => s.ID).FirstOrDefault();
            var subjects = db.ClassSubjects.Where(w => w.CampusID == campusid && w.ClassID == classId)
                .Select(s => new {s.SubjectID, s.Subject.SubjectName}).ToList();
            var timetableExisting = db.TimeTables
                .Where(w => w.CampusID == campusid && w.ClassID == classId && w.SectionID == SectionId).ToList();
            var teachingSubjectExisting = db.TeachingSubjects
                .Where(w => w.CampusID == campusid && w.ClassSectionID == classSectionId && w.CloseDate == null).Select(
                    s => new
                    {
                        s.SubjectId,
                        s.StaffID,
                        StaffName = s.tbl_Employee.employeeName + "(" + s.tbl_Employee.employeeCode + ")"
                    });

            foreach (var data in subjects)
            {
                var Teacher = teachingSubjectExisting.Where(w => w.SubjectId == data.SubjectID).FirstOrDefault();
                var current = timetableExisting.Where(w => w.SubjectID == data.SubjectID).FirstOrDefault();
                if (current != null)
                {
                    current.SubjectName=data.SubjectName + " [" + Teacher?.StaffName + "]";
                    timeTables.Add(current);
                }
                else
                    timeTables.Add(new TimeTable
                    {
                        ID = 0,
                        SubjectID = data.SubjectID,
                        Pno = 0,
                        SubjectName = data.SubjectName + " [" + Teacher?.StaffName + "]",
                        StaffID = Teacher == null ? 0 : (int)Teacher.StaffID,
                        TimeFrom = DateTime.Today.Date,
                        TimeTo = DateTime.Today.Date,
                        ClassID = classId,
                        SectionID = SectionId,
                        CampusID = campusid
                    });
            }

            if (config != null && config.otherPeriods != null)
                foreach (var other in config.otherPeriods)
                {
                    if (timeTables.Where(w => w.SubjectName == other.Description).Any())
                        continue;

                    var otherExist = timetableExisting.Where(w => w.SubjectName == other.Description).FirstOrDefault();
                    timeTables.Add(new TimeTable
                    {
                        ID = -1,
                        SubjectID = 0,
                        Pno = otherExist == null ? other.PeriodNo : otherExist.Pno,
                        SubjectName = other.Description,
                        StaffID = 0,
                        TimeFrom = otherExist == null ? DateTime.Today : otherExist.TimeFrom,
                        TimeTo = otherExist == null ? DateTime.Today : otherExist.TimeTo,
                        ClassID = classId,
                        SectionID = SectionId,
                        CampusID = campusid
                    });
                }

            timeTables = timeTables.OrderBy(o => o.Pno).ToList();

           
           var result = timeTables.Select(x =>new
            {
                x.ID,
                x.SubjectID,
                x.StaffID,
                x.SubjectName,
                Pno = x.Pno == 0 ? "" : x.Pno.ToString(),
                TimeFrom = x.TimeFrom.ToString("hh:mm tt"),
                TimeTo = x.TimeTo.ToString("hh:mm tt"),
                x.ClassID,
                x.CampusID,
                x.SectionID
            });

            return Json(new { result, count = result.Count() });
        }

        public ActionResult SavedTimeTableList(DataManagerRequest request)
        {
            var TimeTable = db.TimeTables.AsNoTracking().Where(w => campusIds.Contains(w.CampusID))
                .Select(s => new
                {
                    s.TimeFrom, s.TimeTo, s.Pno, s.ID, TableName = "TimeTables", s.StaffID, s.SubjectName, s.ClassName,
                    s.SectionName
                }).ToList().Select(time => new
                {
                    time.ID,
                    time.Pno,
                    time.ClassName,
                    time.SectionName,
                    ReminderTime = time.TimeFrom.ToString("hh:mm tt"),
                    EndTime = time.TimeTo.ToString("hh:mm tt"),
                    ReminderValue = $"{time.SubjectName} [{db.tbl_Employee.Find(time.StaffID)?.employeeName}]"
                });

            return Json(new { result = TimeTable, count = TimeTable.Count() });
        }

        private List<StaffTimeTable> AutoGenerateTimeTable(TimeTableConfig config)
        {
            //start Auto generationtimetable
            var finalTimeTable = new List<StaffTimeTable>();

            var tx = new timetableLogics(config);
            var TeachingSubjects = new List<TimeTablePreView>();

            var lists = db.ClassSections.Where(w => w.CampusID == config.CampusID).Select(s =>
                new {s.ID, s.Class.ClassName, s.ClassID, s.Section.SectionName, s.SectionID}).ToList();
            var campusName = db.Campuses.Find(config.CampusID)?.CampusName;
            foreach (var section in lists)
            {
                var subjects = db.TeachingSubjects.Where(w =>
                    w.CampusID == config.CampusID && w.ClassSectionID == section.ID && w.CloseDate == null).ToList();
                foreach (var subject in subjects)
                    TeachingSubjects.Add(new TimeTablePreView
                    {
                        ClassID = section.ClassID,
                        ClassName = section.ClassName.Trim(),
                        SectionID = section.SectionID,
                        SectionName = section.SectionName.Trim(),
                        SubjectID = subject.SubjectId,
                        SubjectName = subject.Subject.SubjectName.Trim(),
                        StaffID = subject.StaffID,
                        CampusID = config.CampusID,
                        CampusName = campusName
                    });
            }

            finalTimeTable = tx.MakeTimeTable(TeachingSubjects, config.CampusID).OrderBy(o => o.Period).ToList();
            foreach (var tt in finalTimeTable)
            {
                if (!config.otherPeriods.Select(s => s.Description).Contains(tt.SubjectName))
                    tt.SubjectName = tt.SubjectName + "<br />[" + tt.StaffName + "]";
                tt.PeriodNo = tt.Period.ToPosition() + "<br />" + tt.TimeFrom + " TO " + tt.TimeTo;
            }

            return finalTimeTable;
        }

        #region private member

        #endregion

        #region teaching Subject

        
        public ActionResult TeachingSubject()
        {
            var staff = db.tbl_Employee.Where(w => w.LeavedStaffs.Count < 1).ToList()
                .Select(s => new {ID = s.Id, StaffName = s.employeeName + " - " + s.employeeCode}).ToList()
                .Select(s => new {s.ID, s.StaffName}).ToList();
            var list = new SelectList(staff, "ID", "StaffName");
            ViewBag.staff = list;

            return View();
        }

        
        [HttpPost]
        public string TeachingSubject(TeachingSubject teaching)
        {
            if (Request.Form["Id"].ToString()== null) return "failed";
            var dgvtxtID = Request.Form["Id"].ToString().Split(',');
            if (Request.Form["StaffIDs"].ToString() == null) return "failed";
            var dgvcmbStaffID = Request.Form["StaffIDs"].ToString().Split(',');
            if (Request.Form["SubjectId"].ToString() == null) return "failed";
            var dgvtxtSubjectId = Request.Form["SubjectId"].ToString().Split(',');
            if (Request.Form["compusId"].ToString() == null) return "failed";
            var cmbCampus = Request.Form["compusId"].ToString();
            if (Request.Form["SectionId"].ToString() == null) return "failed";
            var cmbClassSection = Request.Form["SectionId"].ToString();
            if (dgvtxtID.Count() > 0)
            {
                for (var i = 0; i < dgvtxtID.Count(); i++)
                {
                    if (dgvtxtID[i] == null || int.TryParse(dgvtxtID[i], out var Id) && Id == 0)
                    {
                        if (!int.TryParse(dgvcmbStaffID[i], out var StaffId) || !(StaffId > 0))
                            //thisRow.DefaultCellStyle.BackColor = Color.Red;
                            //ErrorLogging.Error("No valid teacher selected to assign this subject.");
                            continue;

                        if (int.TryParse(dgvtxtSubjectId[i], out var SubjectId) && SubjectId > 0)
                        {
                            var campusId = int.Parse(cmbCampus);
                            var classSectionId = int.Parse(cmbClassSection);

                            db.TeachingSubjects.Add(new TeachingSubject
                            {
                                CampusID = campusId,
                                ClassSectionID = classSectionId,
                                SubjectId = SubjectId,
                                StaffID = StaffId,
                                StartDate = DateTime.Now
                            });
                        }
                    }

                    var pk = int.Parse(dgvtxtID[i]);
                    var teachingSubjectExisting = db.TeachingSubjects.Where(w => w.ID == pk).FirstOrDefault();

                    if (teachingSubjectExisting != null)
                    {
                        if (teachingSubjectExisting.StaffID == int.Parse(dgvcmbStaffID[i]))
                        {
                        }
                        else
                        {
                            teachingSubjectExisting.CloseDate = DateTime.Now;
                            db.Entry(teachingSubjectExisting).State = EntityState.Modified;
                            var campusId = int.Parse(cmbCampus);
                            var classSectionId = int.Parse(cmbClassSection);
                            var subjectId = int.Parse(dgvtxtSubjectId[i]);
                            var staffId = int.Parse(dgvcmbStaffID[i]);
                            db.TeachingSubjects.Add(new TeachingSubject
                            {
                                CampusID = campusId,
                                ClassSectionID = classSectionId,
                                SubjectId = subjectId,
                                StaffID = staffId,
                                StartDate = DateTime.Now
                            });
                            db.SaveChanges();
                        }
                    }
                }

                try
                {
                    db.SaveChanges();
                    return "success";
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("",ex.Message);
                }
            }

            return ModelState.Where(w => w.Value.Errors.Count > 0).FirstOrDefault().Value.Errors.FirstOrDefault()
                .ErrorMessage;
        }

        
        public JsonResult TeachingSubjectEdit(int campusId, int SectionId, int StaffId, int SubjectId, int ClassId)
        {
            var message = "";
            try
            {
                var classSectionId = db.ClassSections.Where(w => w.ClassID == ClassId && w.SectionID == SectionId)
                    .FirstOrDefault().ID;
                var teachingSubjectExisting = db.TeachingSubjects.Where(w =>
                    w.CampusID == campusId && w.ClassSectionID == classSectionId && w.SubjectId == SubjectId &&
                    w.CloseDate == null).FirstOrDefault();

                if (teachingSubjectExisting != null)
                {
                    if (teachingSubjectExisting.StaffID == StaffId)
                    {
                        message = "Already assigned to selected teacher.";
                    }
                    else
                    {
                        var closeDate = DateTime.Now;
                        teachingSubjectExisting.CloseDate = closeDate;
                        db.Entry(teachingSubjectExisting).State = EntityState.Modified;

                        db.TeachingSubjects.Add(new TeachingSubject
                        {
                            CampusID = campusId,
                            ClassSectionID = classSectionId,
                            SubjectId = SubjectId,
                            StaffID = StaffId,
                            StartDate = closeDate
                        });
                        db.SaveChanges();
                        message = "success";
                    }
                }
                else
                {
                    db.TeachingSubjects.Add(new TeachingSubject
                    {
                        CampusID = campusId,
                        ClassSectionID = classSectionId,
                        SubjectId = SubjectId,
                        StaffID = StaffId,
                        StartDate = DateTime.Now
                    });
                    db.SaveChanges();
                    message = "success";
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            if (ModelState.IsValid)
                message = "success";
            else
                message = ModelState.Where(w => w.Value.Errors.Count > 0).FirstOrDefault().Value.Errors.FirstOrDefault()
                    .ErrorMessage;

            return Json(new{ Data = new {message} });
           

        }

        #endregion

        #region teaching Class

        
        public ActionResult AssignTeacher()
        {
            var staff = db.tbl_Employee.Where(w => w.LeavedStaffs.Count < 1).ToList()
                .Select(s => new {ID = s.Id, StaffName = s.employeeName + " - " + s.employeeCode}).ToList()
                .Select(s => new {s.ID, s.StaffName}).ToList();
            var list = new SelectList(staff, "ID", "StaffName");
            ViewBag.staff = list;

            return View();
        }

         public JsonResult AssignTeacherEdit(int campusId, int ClassSectionId, int StaffId)
        {
            var message = "";
            try
            {
                var teachingClassExisting = db.TeachingClasses
                    .Where(w => w.ClassSection.CampusID == campusId && w.ClassSectionId == ClassSectionId)
                    .FirstOrDefault();

                if (teachingClassExisting != null)
                {
                    if (teachingClassExisting.StaffID == StaffId)
                    {
                        message = "Already assigned to selected teacher.";
                    }
                    else
                    {
                        teachingClassExisting.StaffID = StaffId;
                        db.Entry(teachingClassExisting).State = EntityState.Modified;
                        db.SaveChanges();
                        message = "success";
                    }
                }
                else
                {
                    var userId = _user.UserID;
                    db.TeachingClasses.Add(new TeachingClass
                    {
                        ClassSectionId = ClassSectionId,
                        StaffID = StaffId,
                        EntryDate = DateTime.Now,
                        IsActive = true,
                        UserId = _user.UserID
                    });
                    db.SaveChanges();
                    message = "success";
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            if (ModelState.IsValid)
                message = "success";
            else
                message = ModelState.Where(w => w.Value.Errors.Count > 0).FirstOrDefault().Value.Errors.FirstOrDefault()
                    .ErrorMessage;

            return Json(new { Data = new { message } });
         }

        [HttpPost]
        public JsonResult TeachingClassList(DataManagerRequest request, int classId, int campusid)
        {
            var existing = db.TeachingClasses.AsNoTracking()
                .Where(w => w.ClassSection.CampusID == campusid && w.ClassSection.ClassID == classId).ToList();
            var data = db.ClassSections.AsNoTracking().Where(w => w.CampusID == campusid && w.ClassID == classId).ToList()
                .Select(s => new
                {
                    ID = existing.Where(w => w.ClassSectionId == s.ID).Select(t => t.ID).FirstOrDefault(),
                    ClassSectionId = s.ID,
                    s.Section.SectionName,
                    s.Class.ClassName,
                    StaffID = existing.Where(w => w.ClassSectionId == s.ID).Select(t => t.StaffID).FirstOrDefault()
                }).ToList();



            var result = data.Select(s => new
            {
                s.ID,
                s.SectionName,
                s.ClassName,
                teacher = GetStaff(s.StaffID, campusid),
                s.StaffID,
                s.ClassSectionId
            });
            return Json(new { result, count = result.Count() } );
         }

        #endregion
    }
}