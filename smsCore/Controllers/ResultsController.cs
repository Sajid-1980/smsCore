using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using smsCore.Data;
using smsCore.Data.Helpers;
using smsCore.Data.Models.ViewModels;
using Syncfusion.EJ2.Base;

namespace smsCore.Controllers
{
    //[CompressContent]
    [Authorize]
    public class ResultsController : BaseController
    {
        private readonly SchoolEntities db;
        private readonly CurrentUser _user;


        public ResultsController(SchoolEntities  _db  , CurrentUser user)
        {
            db = _db;
            _user = user;

        }

        //
        // GET: /Admin/Results/

        public IActionResult Index()
        {
            return View();
        }

        //
        // GET: /Admin/Results/Details/5
        
        public IActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Admin/Results/Create
        
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Admin/Results/Create
        
        [HttpPost]
        public IActionResult Create(List<Result> results, ExamRemark remarks = null)
        {
            var j = 1;
            var id = 1;
            if (db.Results.Count() == 0)
                id = 1;
            else
                id = db.Results.Max(m => m.ID) + j;
            foreach (var result in results)
            {
                var exist = db.Results.FirstOrDefault(a =>
                    a.AdmissionID == result.AdmissionID &&
                    a.ExamHeldID == result.ExamHeldID &&
                    a.ClassSubjectID == result.ClassSubjectID);

                if (exist != null)
                {
                    exist.ObtainedMarks = result.ObtainedMarks;
                    exist.UserID = _user.UserID;
                    exist.CheckedBy = result.CheckedBy;
                }
                else
                {
                    result.UserID = _user.UserID;
                    result.ID = id;
                    db.Results.Add((Result) result.Validate(new List<string>()));
                    j++;
                    id++;
                }
            }

            try
            {
                db.SaveChanges();
                return Json(new {message = "Result records added successfully.", type = "success"});
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    exception = ex, message = ex.InnerException != null ? ex.InnerException.Message : ex.Message,
                    type = "exception"
                });
            }
        }

        public IActionResult BulkResultEntry()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> BulkResultEntry(List<BulkrResultViewModel> model)
        {
            string message = "";
            bool status = false;

            try
            {
                if (model == null)
                    return Json(new { status = false, message = "No record modified." });
                var id = 1;
                if (db.Results.Count() == 0)
                    id = 1;
                else
                    id = db.Results.Max(m => m.ID) + 1;
                foreach (var m in model)
                {
                    if(m.Marks.ToLower().Trim()!="a" && !double.TryParse(m.Marks, out double marks))
                    {
                        continue;
                    }

                    var exist = await db.Results.FirstOrDefaultAsync(w => w.ClassSubjectID == m.ClassSubjectId & w.ExamHeldID == m.ExamId & w.AdmissionID == m.AdmissionId);
                    if (exist == null)
                    {
                        exist = new Result() { ID = id, AdmissionID = m.AdmissionId, ClassSubjectID = m.ClassSubjectId, ExamHeldID = m.ExamId, UserID = _user.UserID, CheckedBy = string.Empty };
                        db.Results.Add(exist);
                        id++;
                    }
                    exist.ObtainedMarks = m.Marks.ToLower() == "a" ? 2000 : double.Parse(m.Marks.Trim());
                    //exist.ObtainedMarks = m.Marks.ToLower() == "0" ? 2000 : double.Parse(m.Marks.Trim());
                }

               await db.SaveChangesAsync();
                message = "Result record saved successfully.";
                status=true;
            }
            catch (Exception ex)
            {
                message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
               // return "An error occured while trying to save results.";
            }

           
            return Json(new { status = status, message = message });
        }

        [HttpPost]
        public async Task<JsonResult> GetBulkResultOfSubject(int myclass, int campus, int sectionId, int exam, int subject)
        {
            var clsSectionId = db.ClassSections.Where(w => w.CampusID == campus && w.ClassID == myclass && w.SectionID == sectionId)
                 .Select(s => s.ID).FirstOrDefault();

            var clsSubjectId = db.ClassSubjects.Where(w =>w.CampusID==campus & w.ClassID == myclass && w.SubjectID == subject)
                .Select(s => s.ID).FirstOrDefault();

            var maxNo =await db.ExamDates.Where(w =>w.ExamHeldID==exam & w.CampusID == campus & w.ClassSectionID == clsSectionId && w.SubjectID == subject).Select(s=>s.TotalMarks).ToListAsync();
            if(maxNo.Count==0)
            {
                return Json(new { Data = new { NoDateSheet = true } });

                // return new JsonResult { Data = new { NoDateSheet= true }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            else if (maxNo.Count>1)
            {
                return Json(new { Data = new { DateSheetDouble = true } });

                // return new JsonResult { Data = new { DateSheetDouble=true }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            double MaxMark = maxNo.FirstOrDefault();
            if(MaxMark==0)
            {
                return Json(new { Data = new { MaxMark = true } });

                // return new JsonResult { Data = new { MaxMark = true }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            var data =await db.Admissions.Where(w => w.CampuseID == campus & 
            w.ClassSectionID == clsSectionId & !w.IsExpell).Select(s => new
            {
                MaxMarks = MaxMark,
                ExamId=exam,
                ClassSubjectId = clsSubjectId,
                s.Student.RegistrationNo,
                AdmissionId= s.ID,  s.Student.StudentName, s.Student.FName, 
                Marks=s.Results.Where(w=>w.ExamHeldID==exam & w.ClassSubjectID== clsSubjectId).Select(t=>t.ObtainedMarks).FirstOrDefault()
            }).OrderBy(o=>o.RegistrationNo).ToListAsync();
            var d = Json(new
            {
                Data = data
            });
            return d;
        }

        

        
        // GET: /Admin/Results/Edit/5
        
        public IActionResult Edit()
        {
            return View();
        }

        //
        // POST: /Admin/Results/Edit/5
        
        [HttpPost]
        public IActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        
        public IActionResult EditStudentResults(int ExamHeldID = 0, int regNo = 0)
        {
            var remarksDetail = db.ExamRemarksDetails.Select(s => new {ID = s.Remarks, s.Remarks});
            ViewBag.RemarksDetail = new SelectList(remarksDetail, "ID", "Remarks",
                remarksDetail.FirstOrDefault() == null ? "" : remarksDetail.FirstOrDefault().ID);

            var model = db.Results.Where(w => w.ExamHeldID == 0 && w.Admission.Student.RegistrationNo == regNo)
                .ToList();
            return View(model);
        }


        [HttpPost]
        public IActionResult EditStudentResults(int id, double marks)
        {
            var result = db.Results.Find(id);
            if (result != null)
            {
                result.ObtainedMarks = marks;
                try
                {
                    db.SaveChanges();
                    return Json(new { message = "Result records modified successfully.", type = "success" });
                }
                catch
                {
                    return Json(new { message = "Unable to store data. please view the log file or contact admin.", type = "error" });
                }
            }
            return Json(new { message = "No result record found to edit.", type = "error" });
        }


        //
        // POST: /Admin/Results/Delete/5

        [HttpPost]
        public async Task<JsonResult> Delete(int id,int examheldid, int classSubjectId)
        {
            var result = await db.Results.FirstOrDefaultAsync(w=>w.AdmissionID==id && w.ExamHeldID==examheldid && w.ClassSubjectID==classSubjectId);
            if (result != null)
                try
                {
                    db.Results.Remove(result);
                    await db.SaveChangesAsync();
                    return Json(new { message = "Result records deleted successfully.", status=true });
                }
                catch
                {
                    return Json(new
                    { message = "Unable to delete data. please view the log file or contact admin.", status=false });
                }

            return Json(new { message = "No result record found to delete.", type = "error" });
        }
        [HttpPost]
        public JsonResult Delete(int id)
        {
            var result = db.Results.Find(id);
            if (result != null)
                try
                {
                    db.Results.Remove(result);
                    db.SaveChanges();
                    return Json(new {message = "Result records deleted successfully.", type = "success"});
                }
                catch
                {
                    return Json(new
                        {message = "Unable to store data. please view the log file or contact admin.", type = "error"});
                }

            return Json(new {message = "No result record found to delete.", type = "error"});
        }

        //AddExamRemarks
        
        public IActionResult AddExamRemarks()
        {
            var remarksDetail = db.ExamRemarksDetails.Select(s => new {s.ID, s.Remarks}).ToList();
            ViewBag.RemarksDetail = remarksDetail;
            return View();
        }

        
        [HttpPost]
        public async Task<JsonResult> AddExamRemarks(List<ExamRemarksViewModel> changed)
        {
            
            foreach (var remark in changed)
            {
                var exist = db.ExamRemarks.FirstOrDefault(w =>
                    w.AdmissionID == remark.ID && w.ExamHeldID == remark.ExamHeldId);
                if (exist == null)
                {
                    //remark.UserID = CurrentUser.UserID;
                    exist = new ExamRemark();
                    exist.AdmissionID = remark.ID;
                    exist.ExamHeldID = remark.ExamHeldId;
                    exist.UserID = _user.UserID;
                    db.ExamRemarks.Add(exist);
                }
                exist.Attendance = remark.Attendance ?? exist.Attendance ?? string.Empty;
                exist.Confid = remark.Confid ?? exist.Confid ?? string.Empty;
                exist.CoTeamWork = remark.CoTeamWork ?? exist.CoTeamWork ?? string.Empty;
                exist.Discpline = remark.Discpline ?? exist.Discpline ?? string.Empty;
                exist.Manners = remark.Manners ?? exist.Manners ?? string.Empty;
                exist.Neatness = remark.Neatness ?? exist.Neatness ?? string.Empty;
                exist.OralExp = remark.OralExp ?? exist.OralExp ?? string.Empty;
                exist.PhyFit = remark.Punctuality ?? exist.PhyFit ?? string.Empty;
                exist.Remarks = remark.Remarks ?? exist.Remarks ?? string.Empty;
                exist.SocBeh = remark.SocBeh ?? exist.SocBeh ?? string.Empty;
                exist.Punctuality = remark.Punctuality ?? exist.Punctuality ?? string.Empty;
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

        public async Task<JsonResult> StudentListForRemarks(DataManagerRequest dm, int campusId, int classId, int sectionId, int examid)
        {
            var types = await db.Admissions.Where(w => !w.IsExpell & w.CampuseID == campusId & w.ClassSection.ClassID == classId & w.ClassSection.SectionID == sectionId).Select(s => new
            {
                s.ID,
                s.Student.RegistrationNo,
                s.Student.StudentName,
                Attendance = s.ExamRemarks.Where(w=>w.ExamHeldID==examid).Select(f=>f.Attendance).FirstOrDefault(),
                Confid = s.ExamRemarks.Where(w => w.ExamHeldID == examid).Select(f => f.Confid).FirstOrDefault(),
                CoTeamWork = s.ExamRemarks.Where(w => w.ExamHeldID == examid).Select(f => f.CoTeamWork).FirstOrDefault(),
                Discpline = s.ExamRemarks.Where(w => w.ExamHeldID == examid).Select(f => f.Discpline).FirstOrDefault(),
                Manners = s.ExamRemarks.Where(w => w.ExamHeldID == examid).Select(f => f.Manners).FirstOrDefault(),
                Neatness = s.ExamRemarks.Where(w => w.ExamHeldID == examid).Select(f => f.Neatness).FirstOrDefault(),
                OralExp = s.ExamRemarks.Where(w => w.ExamHeldID == examid).Select(f => f.OralExp).FirstOrDefault(),
                PhyFit = s.ExamRemarks.Where(w => w.ExamHeldID == examid).Select(f => f.PhyFit).FirstOrDefault(),
                Punctuality = s.ExamRemarks.Where(w => w.ExamHeldID == examid).Select(f => f.Punctuality).FirstOrDefault(),
                SocBeh = s.ExamRemarks.Where(w => w.ExamHeldID == examid).Select(f => f.SocBeh).FirstOrDefault(),
                Remarks = s.ExamRemarks.Where(w => w.ExamHeldID == examid).Select(f => f.Remarks).FirstOrDefault(),
                ExamHeldId = examid
            }).ToListAsync();

            return Json(new { result = types, count = types.Count() });
        }

        //EditExamRemarks
        
        public IActionResult EditExamRemarks()
        {
            return View();
        }

        
        [HttpPost]
        public IActionResult EditExamRemarks(int id)
        {
            var remark = db.ExamRemarks.Where(w => w.ID == id).FirstOrDefault();
            if (remark != null)
            {
                if (remark.ID > 0)
                    db.Entry(remark).State = EntityState.Modified;
                else db.ExamRemarks.Add(remark);
            }

            try
            {
                db.SaveChanges();
                return Json(new {message = "Remarks records deleted successfully.", type = "success"});
            }
            catch
            {
                return Json(new
                    {message = "Unable to store data. please view the log file or contact admin.", type = "error"});
            }
        }

        
        [HttpPost]
        public IActionResult DeleteExamRemarks(int id)
        {
            var remark = db.ExamRemarks.Where(w => w.ID == id).FirstOrDefault();
            if (remark != null) db.ExamRemarks.Remove(remark);
            try
            {
                db.SaveChanges();
                return Json(new {message = "Remarks records deleted successfully.", type = "success"});
            }
            catch
            {
                return Json(new
                    {message = "Unable to store data. please view the log file or contact admin.", type = "error"});
            }
        }

        //GetSubjectDetail

        [HttpGet]
        public ActionResult GetSubjectDetail(int campus, int myclass, int exam, int subject, int sectionId)
        {
            var subjects = db.ExamDates
                .Where(w => w.ExamHeldID == exam && w.ClassSection.ClassID == myclass &&
                            w.ClassSection.SectionID == sectionId && w.SubjectID == subject).ToList().Select(s =>
                    new
                    {
                        ClassSubjectID = s.Subject.ClassSubjects.FirstOrDefault(w => w.ClassID == myclass).ID,
                        ID = s.SubjectID, s.TotalMarks, PassMarks = 0, s.ExamDate1
                    });
            if (subjects.Count() > 0)
                return Json(new {subject = subjects, type = "success"});
            return Json(
                new
                {
                    message =
                        "No Record found in datesheet. Please add datesheet for selected subject before adding Results.",
                    type = "error"
                });
        }

        [HttpGet]
        public IActionResult GetResultOfSubject(int campus, int myclass, int sectionId, int exam, int subject)
        {
            var results = db.Results
                .Where(w => w.Admission.CampuseID == campus && w.Admission.ClassSection.SectionID == sectionId &&
                            w.ExamHeldID == exam && w.ClassSubject.ClassID == myclass &&
                            w.ClassSubject.SubjectID == subject).ToList().Select(s => new
                {
                    s.ID, s.ObtainedMarks, s.ClassSubjectID, s.Admission.Student.StudentName,
                    s.Admission.Student.RegistrationNo, AdmissionID = s.Admission.ID,
                    s.ExamHeld.ExamDates
                        .Where(w => w.ClassSection.ClassID == myclass && w.ClassSection.SectionID == sectionId &&
                                    w.SubjectID == subject).FirstOrDefault().TotalMarks
                });
            if (results.Count() > 0) return Json(new {results, type = "success"} );
            return Json(
                new
                {
                    message =
                        "No Record found in datesheet. Please add datesheet for selected subject before adding Results.",
                    type = "error"
                });
        }

        [HttpGet]
        public IActionResult GetResultOfStuden(int exam, int regno)
        {
            //var results = db.Results.Where(w => w.ExamHeldID == exam && w.Admission.Student.RegistrationNo==regno).ToList().Select(s => new { s.ID, s.ObtainedMarks, s.ClassSubject.Subject.SubjectName, s.ExamHeld.ExamDates.Where(w => w.ClassID == s.ClassSubject.ClassID && w.SubjectID == s.ClassSubject.SubjectID).FirstOrDefault().TotalMarks });
            //var remarks = db.ExamRemarks.Where(w => w.ExamHeldID == exam && w.Admission.Student.RegistrationNo == regno).Select(s => new
            //{
            //    s.ID,
            //    s.Attendance,
            //    s.Confid,
            //    s.CoTeamWork,
            //    s.Discpline,
            //    s.Manners,
            //    s.Neatness,
            //    s.OralExp,
            //    s.PhyFit,
            //    s.Punctuality,
            //    s.Remarks,
            //    s.SocBeh
            //}).FirstOrDefault();
            //if (results.Count() > 0)
            //{
            //    return Json(new { results = results,remarks=remarks, type = "success" }, JsonRequestBehavior.AllowGet);
            //}
            return Json(
                new
                {
                    message =
                        "No Record found in datesheet. Please add datesheet for selected subject before adding Results.",
                    type = "error"
                });
        }
    }
}