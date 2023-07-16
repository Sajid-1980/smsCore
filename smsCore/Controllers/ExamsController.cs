using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using smsCore.Data;
using smsCore.Data.Helpers;
using smsCore.Data.Models.ViewModels;
using Syncfusion.EJ2.Base;
using System.Data;
using System.Linq.Dynamic;

namespace smsCore.Controllers
{
    //[CompressContent]
    [Authorize]
    public class ExamsController : BaseController
    {
        private readonly int[] _campusIds;//= Helpers.CurrentUser.GetCampusIds();
        private readonly SchoolEntities db;
        private readonly CurrentUser _user;


        public ExamsController(SchoolEntities _db , CurrentUser user)
        {
            db = _db;
            _user = user;
            _campusIds = user.GetCampusIds();
        }


        //
        //public ActionResult SearchExam()
        //{
        //    ViewBag.ExamsList = new SelectList(db.Exams.Select(s => new {s.ID, s.ExamName}), "ID", "ExamName");
        //    ViewBag.ClassList = new SelectList(db.Classes.Select(s => new {s.ID, s.ClassName}), "ID", "ClassName");
        //    return View();
        //}


        public IActionResult NewExamHelds(int? ExamID)
        {
            //var model = db.Examsrules.Where(w => w.Examheld.ExamID == ExamID).FirstOrDefault();
            ViewBag.ExamID = ExamID;
            ViewBag.ExamHeldID = new SelectList(db.Exams.ToList(), "ID", "ExamName");
            return View("NewExamHelds");
        }

        public JsonResult GetHeldExams(int ExamID)
        {
            var result = Json(new
            {
                Data = db.ExamHelds.Where(w => w.ExamID == ExamID).ToList()
                .Select(s => new { s.ID, ExamName = s.EntryDate.ToString("MMMM, yyyy") }).ToList()
            });
            return result;
            //result.Data = db.ExamHelds.Where(w => w.ExamID == ExamID).ToList()
            //    .Select(s => new {s.ID, ExamName = s.EntryDate.ToString("MMMM, yyyy")}).ToList();
            // return result;
        }
        //GetExamListForJq
        [HttpPost]
        public JsonResult ExamHeldList(DataManagerRequest dm)
        {
            var types = db.ExamHelds.Where(w => _campusIds.Contains(w.CampusID) && w.ExamsRules.Count() > 0)
                .OrderByDescending(o => o.EntryDate).Select(x => new
                {
                    x.ID,
                    x.Exam.ExamName,
                    x.EntryDate,
                    StartDate = x.ExamDates.Select(s => s.ExamDate1).DefaultIfEmpty().Min(),
                    EndDate = x.ExamDates.Select(s => s.ExamDate1).DefaultIfEmpty().Max(),
                }).ToList()
                        .Select(s => new
                         {

                             s.ID,
                             s.ExamName,
                             s.EntryDate,
                             StartDate = s.StartDate ,
                             EndDate = s.EndDate 
                         });

            return Json(new {result= types, count=types.Count() });


        }
        
        
        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult NewExamHelds(ExamsRule model)
        {
            string message = "";
            var examid = int.Parse(Request.Form["examId"].ToString());
            if (Request.Form["Campus"].ToString() == null) { message= "Campus is not Selected";
                return Json(new { status = message=="success", message = message=="success" ? "Record uploaded successfully." : message });
            }
            if (Request.Form["ExamHeld.EntryDate"].ToString() == null) { message= "Please Select a month.";
                return Json(new { status = message=="success", message = message=="success" ? "Record uploaded successfully." : message });
            }
            //var date = DateTimeHelper.ConvertDate(model.ExamHeld.EntryDate.ToString("dd/MM/yyyy"), "dd/MM/yyyy");

            var examDate = DateTimeHelper.ConvertDate(Request.Form["ExamHeld.EntryDate"].ToString());

            var campusId = _user.SelectedCampusId;
            var ExamHeldPk = 0;
            var examHeld = db.ExamHelds.Where(w => w.ExamID == examid).ToList().Where(ww =>
                ww.EntryDate.Month == examDate.Month &&
                ww.EntryDate.Year == examDate.Year).ToList().FirstOrDefault();
            if (examHeld == null)
            {
                examHeld = new ExamHeld
                    {EntryDate = examDate, CampusID = campusId, ExamID = examid, VeiwAble = true};
                var examRules = new ExamsRule ();
                examRules.A = model.A;
                examRules.APlus = model.APlus;
                examRules.AtLeastPercentage = model.AtLeastPercentage;
                examRules.B = model.B;
                examRules.BPlus = model.BPlus;
                examRules.C = model.C;
                examRules.CoreFail = model.CoreFail;
                examRules.CoreWithNoneCoreFail = model.CoreWithNoneCoreFail;
                examRules.D = model.D;
                examRules.E = model.E;
                examRules.NoneCoreFail = model.NoneCoreFail;

                examHeld.ExamsRules.Add(examRules);

                db.ExamHelds.Add(examHeld);
                try
                {
                    db.SaveChanges();
                    message = "success";
                }
                catch (Exception aa)
                {
                    message = aa.Message;
                }
            }
            else
            {
                message = "Exam already exist for selected month.";
            }


            return Json(new { status = message=="success", message = message=="success" ? "Record uploaded successfully." : message });
        }

        
        public ActionResult ExamHeldEdit(int id)
        {
            var rule = new ExamsRule();
            var examheld = db.ExamHelds.Find(id);
            if (examheld != null)
            {
                rule = db.ExamsRules.Where(w => w.ExamHeldID == id).FirstOrDefault();
                if (rule == null)
                    rule = new ExamsRule
                    {
                        ExamHeldID = id,
                        ExamHeld = examheld
                    };
                ViewBag.Exams = new SelectList(db.Exams.ToList(), "ID", "ExamName", rule.ExamHeld.ExamID);
            }


            return View(rule);
        }

        
        public IActionResult NewExam()
        {
            return View();
        }
        
        
        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult ExamHeldEdit(ExamsRule model)
        {
            var message = "";
            if (Request.Form["Campus"].ToString() == null)
            {
                message = "Campus is not Selected";
                return Json(new { status = message == "success", message = message == "success" ? "Record uploaded successfully." : message });
            }
            var campusId = int.Parse(Request.Form["Campus"].ToString());

            if (Request.Form["ExamHeld.EntryDate"].ToString() == null)
            {
                message = "Please Select a month.";
                return Json(new { status = message == "success", message = message == "success" ? "Record uploaded successfully." : message });
            }
            var examDate = DateTimeHelper.ConvertDate(Request.Form["ExamHeld.EntryDate"].ToString());

            //var alreadyExam = db.ExamHelds.Where(w=>DbFunctions.TruncateTime(w.EntryDate)== DbFunctions.TruncateTime(date) && w.ID!= model.ExamHeldID 
            //&& w.CampusID==campusId).FirstOrDefault();

            //if (alreadyExam!=null)
            //{
            //    return "An exam already exists on current date";
            //}
            var examHeld = db.ExamHelds.Where(w => w.ID == model.ExamHeldID).FirstOrDefault();
            if (User.IsInRole("Admin"))
            {
                examHeld.CampusID = campusId;
                examHeld.ExamID = model.ExamHeld.ExamID;
            }

            examHeld.EntryDate = examDate;

            var examRules = db.ExamsRules.Find(examHeld.ID);
            if (examRules == null)
            {
                examRules = new ExamsRule();
                db.ExamsRules.Add(examRules);
                examRules.ExamHeldID = examHeld.ID;

            }

            examRules.A = model.A;
            examRules.APlus = model.APlus;
            examRules.AtLeastPercentage = model.AtLeastPercentage;
            examRules.B = model.B;
            examRules.BPlus = model.BPlus;
            examRules.C = model.C;
            examRules.CoreFail = model.CoreFail;
            examRules.CoreWithNoneCoreFail = model.CoreWithNoneCoreFail;
            examRules.D = model.D;
            examRules.E = model.E;
            examRules.NoneCoreFail = model.NoneCoreFail;
            try
            {
                db.SaveChanges();
                message = "success";
            }
            catch (Exception aa)
            {
                message = aa.Message;
            }


            return Json(new { status = message == "success", message = message == "success" ? "Record uploaded successfully." : message });
        }


        [HttpPost]
        public string NewExam(Exam model)
        {
            db.Exams.Add(model);
            try
            {
                db.SaveChanges();
                return "Successfully Stored data.";
            }
            catch
            {
            }

            return "Unable to add data";
        }

        
        public IActionResult DateSheetEntry(int? examId, int? campusId, int? classId, int? sectionId)
        {
            ViewBag.examId = examId;
            ViewBag.campusId = campusId;
            ViewBag.classId = classId;
            ViewBag.sectionId = sectionId;

            return View();
        }

        public async Task<JsonResult> SaveDateSheet(List<ExamDateViewModel> changed)
        {
            foreach (var datesheets in changed)
            {

                ExamDate exist = null;
                //To Remove duplicate record for exam
                //query to get all existing datesheet reocord
                // then delete all record except 1st record
                var existAll = db.ExamDates.Where(w =>
                    w.CampusID == datesheets.CampusID && w.ClassSectionID == datesheets.ClassSectionID &&
                    w.ExamHeldID == datesheets.ExamHeldID && w.SubjectID == datesheets.SubjectID
                ).ToList();
                if (existAll.Count > 1)
                {
                    for (int i = 1; i < existAll.Count; i++)
                    {
                        db.ExamDates.Remove(existAll[i]);
                    }
                    db.SaveChanges();
                    exist = existAll.FirstOrDefault();
                }
                else
                {
                    exist = existAll.FirstOrDefault();
                }

                if (exist == null) //(exist == null)
                {
                    var examDate = new ExamDate();

                    //examDate.ClassID = datesheets.ClassID;
                    examDate.ExamDate1 = datesheets.ExamDate1;
                    examDate.ExamHeldID = datesheets.ExamHeldID;
                    examDate.SubjectID = datesheets.SubjectID;
                    examDate.TimeFrom = datesheets.TimeFrom ?? string.Empty;
                    examDate.TotalMarks = datesheets.TotalMarks;
                    examDate.ToTime = datesheets.ToTime ?? string.Empty; ;
                    examDate.CampusID = datesheets.CampusID;
                    examDate.ClassSectionID = datesheets.ClassSectionID;
                    db.ExamDates.Add(examDate);
                }
                else
                {
                    exist.ExamDate1 = datesheets.ExamDate1;
                    exist.ToTime = datesheets.ToTime ?? string.Empty; ;
                    exist.TimeFrom = datesheets.TimeFrom ?? string.Empty; ;
                    exist.TotalMarks = datesheets.TotalMarks;
                    db.Entry(exist).State = EntityState.Modified;
                }
            }
            try
            {
                await db.SaveChangesAsync();
                return Json(new { status = true, message = "Datesheet records saved successfully." });
            }
            catch (Exception a)
            {
                return Json(new { status = false, message = a.Message });
            }

        }



        [HttpPost]
        public async Task<string> DateSheetDelete(int id)
        {

            var del = db.ExamDates.Find(id);
                if (del != null)
                {
                    db.ExamDates.Remove(del);
                }
                else
                {
                    return "No record found to delete.";
                }           

            try
            {
                    await db.SaveChangesAsync();
                return "Success";
            }
            catch (Exception a)
            {
                return a.Message;
            }

            //return "Unable to add data";
        }

        public IActionResult DateSheetList(DataManagerRequest dm, int examId = -1, int ClassID = -1,
            int SectionID = -1, int CampusID = -1)
        {
             var ClassSectionID = db.ClassSections.Where(w => w.CampusID == CampusID && w.ClassID == ClassID && w.SectionID == SectionID)
                .Select(s => s.ID).FirstOrDefault();

            var dates = db.ExamDates.Where(w => w.CampusID == CampusID && w.ExamHeldID == examId &&
                                              w.ClassSectionID == ClassSectionID
                                              //&& w.ClassSection.Class.ClassSubjects.Where(ww =>
                                              //      ww.IsDeleted != true && ww.ClassID == ClassID &&
                                              //      ww.SubjectID == w.SubjectID).Count() > 0
                                              ).Select(s => new
                                              {
                                                  s.ExamHeldID,
                                                  s.CampusID,
                                                  SectionID = s.ClassSection.SectionID,
                                                  s.ClassSection.ClassID,
                                                  ClassSectionID = s.ClassSectionID,
                                                  s.SubjectID,
                                                  s.Subject.SubjectName,
                                                  s.ID,
                                                  s.ExamDate1,
                                                  s.TimeFrom,
                                                  s.ToTime,
                                                  s.TotalMarks
                                              })
                .ToList();

            var subjects = db.ClassSubjects.Where(w => (!w.IsDeleted.HasValue || !w.IsDeleted.Value) && w.ClassID == ClassID && w.CampusID == CampusID).ToList();

            foreach (var s in subjects)
            {
                var ex = dates.Where(w => w.SubjectID == s.SubjectID).Any();
                if (!ex)
                {
                    dates.Add(new
                    {
                        ExamHeldID = examId,
                        s.CampusID,
                        SectionID,
                        s.ClassID,
                        ClassSectionID,
                        s.SubjectID,
                        s.Subject.SubjectName,
                        ID = 0,
                        ExamDate1 = dates.Where(w => w.SubjectID == s.SubjectID).Select(d => d.ExamDate1).Any() ? dates.Where(w => w.SubjectID == s.SubjectID).Select(d => d.ExamDate1).FirstOrDefault() : DateTime.Now,
                        TimeFrom = dates.Where(w => w.SubjectID == s.SubjectID).Select(d => d.TimeFrom).FirstOrDefault(),
                        ToTime = dates.Where(w => w.SubjectID == s.SubjectID).Select(d => d.ToTime).FirstOrDefault(),
                        TotalMarks = dates.Where(w => w.SubjectID == s.SubjectID).Select(d => d.TotalMarks).FirstOrDefault()
                    });
                }
            }

            //var types = subjects.Select(s => new
            //{
            //    ExamHeldID = examId,
            //    s.CampusID,
            //    SectionID,
            //    s.ClassID,
            //    ClassSectionID,
            //    s.SubjectID,
            //    s.Subject.SubjectName,
            //    ID = dates.Where(w => w.SubjectID == s.SubjectID).Select(d => d.ID).FirstOrDefault(),
            //    ExamDate1 = dates.Where(w => w.SubjectID == s.SubjectID).Select(d => d.ExamDate1).Any() ? dates.Where(w => w.SubjectID == s.SubjectID).Select(d => d.ExamDate1).FirstOrDefault() : DateTime.Now,
            //    TimeFrom = dates.Where(w => w.SubjectID == s.SubjectID).Select(d => d.TimeFrom).FirstOrDefault(),
            //    ToTime = dates.Where(w => w.SubjectID == s.SubjectID).Select(d => d.ToTime).FirstOrDefault(),
            //    TotalMarks = dates.Where(w => w.SubjectID == s.SubjectID).Select(d => d.TotalMarks).FirstOrDefault()
            //});

            return Json(new { result = dates, count = dates.Count() });
        }


        public IActionResult RemarkConfiguration()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RemarkConfiguration(ExamRemarksDetail examRemarks)
        {
            var message = "";
            var exam = new ExamRemarksDetail();
            try
            {
                 
                db.ExamRemarksDetails.Add(examRemarks);
                db.SaveChanges();
                message = "success";
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return Json(new { status = message == "success", message = message == "success" ? "Record uploaded successfully." : message });
        }

        public string RemarksDelete(int id)
        {
            var remarks = db.ExamRemarksDetails.Find(id);
            try
            {
                db.ExamRemarksDetails.Remove(remarks);
                db.SaveChanges();
                return "Success";
            }
            catch
            {
                return "Failed";
            }

        }

        public IActionResult RemarksList(DataManagerRequest dm)
        {

            var types = db.ExamRemarksDetails.Select(s => new
            {
                s.ID,
                s.Code,
                s.Remarks,

            });
            return Json(new { result = types, count = types.Count() } );
          
        }
        
        
        public IActionResult ResultmissingStudent()
        {
            return View();
        }

        public IActionResult ResultMissingStudentList(DataManagerRequest dm, int classId = 0, int subjectid = 0,
            int campusId = 0, int examid = 0)
        {
            var classSubjectId = db.ClassSubjects.Where(w => (!w.IsDeleted.HasValue || !w.IsDeleted.Value) && w.ClassID == classId & w.SubjectID == subjectid && w.CampusID == campusId).FirstOrDefault().ID;

            var types = db.Admissions.Where(w => !w.IsExpell & w.CampuseID == campusId &
            w.ClassSection.ClassID == classId
            & !w.Results.Any(r => r.ExamHeldID == examid & r.ClassSubject.SubjectID == subjectid & r.ClassSubject.ClassID == classId)
            ).Select(s => new
            {
                s.Student.StudentName,
                s.Student.RegistrationNo,
                s.Campus.CampusName,
                s.ClassSection.Class.ClassName,
                s.ID,
                ClassSubjectId = classSubjectId,
                ExamHeldId = examid,
                Marks = 0
            });

            return Json(new { result = types, count = types.Count() });
            //var q =
            //    @"SELECT  Students.StudentName,Students.RegistrationNo , Campuses.CampusName as Campus,Classes.ClassName as Class, Sections.SectionName as Section,  Subjects.SubjectName as Subject FROM            Campuses INNER JOIN
            //             Sections INNER JOIN
            //             Admissions INNER JOIN
            //             Students ON Admissions.StudentID = Students.ID INNER JOIN
            //             ClassSections ON Admissions.ClassSectionID = ClassSections.ID ON Sections.ID = ClassSections.SectionID ON Campuses.ID = Admissions.CampuseID INNER JOIN
            //             Classes ON ClassSections.ClassID = Classes.ID INNER JOIN
            //             ClassSubjects ON Classes.ID = ClassSubjects.ClassID INNER JOIN
            //             Subjects ON ClassSubjects.SubjectID = Subjects.ID
            //            WHERE (Admissions.IsExpell = 0) AND (Classes.ID IN (" + string.Join(",", classIDs) +
            //    ")) AND(ClassSubjects.ID IN(" + string.Join(",", classSubjectIds) + ")) AND (Campuses.ID IN (" +
            //    string.Join(",", campusIDs) + ")) AND  dbo.MissingResult(Admissions.ID," + examid +
            //    ",ClassSubjects.ID)=0";


        }

        [HttpPost]
        public async Task<JsonResult> ExamHeldDelete(int id)
        {
            var exam =await db.ExamHelds.FindAsync(id);
            if(exam!=null)
            {
                if(exam.Results.Any())
                {
                    return Json(new {status=false, message="This exam has result data. It can't be deleted before deleteing the results." });
                }
                db.ExamHelds.Remove(exam);
                try
                {
                   await db.SaveChangesAsync();
                    return Json(new { status = true, message = "Selected exam record has been deleted successfully." });

                }
                catch(Exception ex) {
                    return Json(new { status = false, message = ex.InnerException == null ? ex.Message : ex.InnerException.Message });

                }
            }
            return Json(new { status = false, message = "No record found." });


        }
    }
}