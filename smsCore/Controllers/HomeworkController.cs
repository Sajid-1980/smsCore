using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using smsCore.Data;
using smsCore.Data.Helpers;
using Syncfusion.EJ2.Base;
using System.IO;
using System.Web;

 

namespace smsCore.Controllers
{
    public class HomeworkController : BaseController
    {
        // GET: Homework
        private readonly SchoolEntities db ;
        private readonly CurrentUser _user;
        private readonly IWebHostEnvironment _env;

        public HomeworkController(SchoolEntities _db , CurrentUser user,IWebHostEnvironment env)
        {
            db = _db;
            _user = user;
            _env = env;
        }

       public IActionResult Create()
        {
            return View();
        }



        [HttpPost]
        public async Task<string> Create(Homework homework, IFormFile attachHw)
        {
            var classId = int.Parse(Request.Form["Class"].ToString());
            var sectionId = int.Parse(Request.Form["Section"].ToString());
            var subjectId = int.Parse(Request.Form["Subject"].ToString());
            var classsectionId = db.ClassSections.Where(w => w.ClassID == classId && w.SectionID == sectionId).Select(s => s.ID).FirstOrDefault();
            homework.ClassSectionId = classsectionId;
            homework.SubjectId = subjectId;
            homework.HomeworkDate = DateTimeHelper.ConvertDate(Request.Form["HomeworkDate"].ToString());
            homework.SubmissionDate = DateTimeHelper.ConvertDate(Request.Form["SubmissionDate"].ToString());
            homework.AttachDocument = await SaveToPhysicalLocation(attachHw);
            try
            {
                db.Homework.Add(homework);
                db.SaveChanges();
                return "success";
            }
            catch (Exception ex)
            {
                ModelState.Clear();
                return ex.Message;
                //return RedirectToAction("Create");
            }
        }


        private async Task<string> SaveToPhysicalLocation(IFormFile file)
        {
            string rootpath = _env.WebRootPath;
            if (file != null && file.Length > 0)
            {

                var fileName = Path.GetFileName(file.FileName);
                string filePath = "/Uploads/StudentTask/";

                if (!System.IO.Directory.Exists(rootpath + filePath))
                {
                    Directory.CreateDirectory(rootpath + filePath);
                }

                var path = Path.Combine(rootpath, filePath, fileName);

                using (var stream = System.IO.File.Create(path))
                {
                    await file.CopyToAsync(stream);
                }
                return filePath + fileName;
            }
            return string.Empty;
        }

        public ActionResult Edit(int id)
        {
            var editHome = db.Homework.Where(w => w.Id == id).FirstOrDefault();
            return View(editHome);
        }
        
        [HttpPost]
        public async Task<string> Edit(Homework homework, IFormFile attachHw)
        {
            var classId = int.Parse(Request.Form["Class"].ToString());
            var sectionId = int.Parse(Request.Form["Section"].ToString());
            var subjectId = int.Parse(Request.Form["Subject"].ToString());
            var classsectionId = db.ClassSections.Where(w => w.ClassID == classId && w.SectionID == sectionId).Select(s => s.ID).FirstOrDefault();
            homework.ClassSectionId = classsectionId;
            homework.SubjectId = subjectId;
            homework.HomeworkDate = DateTimeHelper.ConvertDate(Request.Form["HomeworkDate"].ToString());
            homework.SubmissionDate = DateTimeHelper.ConvertDate(Request.Form["SubmissionDate"].ToString());

            var item = db.Homework.Find(homework.Id);
            try
            {
                if (item != null)
                {
                    item.ClassSectionId = classsectionId;
                    item.SubjectId = subjectId;
                    item.HomeworkDate = homework.HomeworkDate;
                    item.SubmissionDate = homework.SubmissionDate;
                    item.AttachDocument = homework.AttachDocument;
                    item.description = homework.description;
                    item.AttachDocument =await SaveToPhysicalLocation(attachHw);

                    db.SaveChanges();
                    return "success";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "failed";
        }

        public ActionResult Delete(int id)
        {
            var dlthomework = db.Homework.Where(w => w.Id == id).FirstOrDefault();
            return View(dlthomework);
        }

        [HttpPost]
        public string Delete(Homework homework)
        {
            var homework1 = db.Homework.Find(homework.Id);
            db.Homework.Remove(homework1);
            db.SaveChanges();
            return "delete";
        }

        public ActionResult GeHomeworkList(DataManagerRequest dm, int campusId = -1, int classid = -1, int sectionid = -1, int subjectid = -1)
        {
            var classsectionId = db.ClassSections.Where(w => w.ClassID == classid && w.CampusID == campusId && w.SectionID == sectionid)
                .Select(s => s.ID).FirstOrDefault();
            var hw = db.Homework.AsNoTracking().Where(w => w.ClassSectionId == classsectionId && w.SubjectId == subjectid).Select(s => new
            {
                s.Id,
                s.ClassSection.Class.ClassName,
                s.ClassSection.Campus.CampusName,
                HomeworkDate = s.HomeworkDate,
                SubmissionDate =s.SubmissionDate,
                Subject= s.Subject.SubjectName,
                s.description
            });
            DataOperations operation = new DataOperations();
            if (dm.Search != null && dm.Search.Count > 0)
            {
                hw = operation.PerformSearching(hw, dm.Search);  //Search
            }
            if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
            {
                hw = operation.PerformSorting(hw, dm.Sorted);
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                hw = operation.PerformFiltering(hw, dm.Where, dm.Where[0].Operator);
            }
            int count = hw.Count();
            if (dm.Skip != 0)
            {
                hw = operation.PerformSkip(hw, dm.Skip);   //Paging
            }
            if (dm.Take != 0)
            {
                hw = operation.PerformTake(hw, dm.Take);
            }
            if (dm.RequiresCounts)
            {
                return Json(new { result = hw, count = hw.Count() } );
            }
            else
            {
                return Json(new { result = hw, count = hw.Count() } );
            }
        }
        public ActionResult Evaluate()
        {
            return View();
        }        
        public ActionResult GetsearchHomework (DataManagerRequest dm, string hwDate, int classid = -1, int sectionId= -1, int campusId= -1, int subjectId= -1)
        {
            DateTime data = DateTimeHelper.ConvertDate(hwDate);
            var classSectionId = db.ClassSections.Where(x => x.ClassID == classid && x.CampusID == campusId && x.SectionID == sectionId)
                .Select(g => g.ID).FirstOrDefault();
            var eval = db.Homework.Where(w=>w.ClassSectionId==classSectionId & w.SubjectId==subjectId & w.HomeworkDate==data).Select(s => new
            {
                s.Id,
                HomeworkDate = s.HomeworkDate,
                SubmissionDate = s.SubmissionDate,
                Subject= s.Subject.SubjectName,
                s.ClassSection.Class.ClassName,
                s.ClassSection.Campus.CampusName
            });
            
            DataOperations operation = new DataOperations();
            if (dm.Search != null && dm.Search.Count > 0)
            {
                eval = operation.PerformSearching(eval, dm.Search);  //Search
            }
            if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
            {
                eval = operation.PerformSorting(eval, dm.Sorted);
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                eval = operation.PerformFiltering(eval, dm.Where, dm.Where[0].Operator);
            }
            int count = eval.Count();
            if (dm.Skip != 0)
            {
                eval = operation.PerformSkip(eval, dm.Skip);   //Paging
            }
            if (dm.Take != 0)
            {
                eval = operation.PerformTake(eval, dm.Take);
            }
            if (dm.RequiresCounts)
            {
                return Json(new { result = eval, count = eval.Count() } );
            }
            else
            {
                return Json(new { result = eval, count = eval.Count() } );
            }
        }
        
        public ActionResult Complaints()
        {
            return View();
        }

     //   [ValidateInput(false)]
        [HttpPost]
        public string Complaints(StudentComplaign complaign)
        {
            var userId = _user.UserID; // To get current user
            var empId = db.UserDefinitions.Where(w => w.UserId == userId && w.UserType == "E").Select(s => s.FK)
                .FirstOrDefault();

            if (!int.TryParse(Request.Form["RegistrationNo"], out var RegNo)) return "Invalid Registration Number";
            complaign.Particular = complaign.Particular == null ? string.Empty : complaign.Particular;
            var stdID = db.Students.Where(w => w.RegistrationNo == RegNo).Select(s => s.ID).FirstOrDefault();
            complaign.Ctype = string.Empty;

            if (db.StudentComplaigns.Count() > 0)
                complaign.ID = db.StudentComplaigns.Max(n => n.ID) + 1;
            else
                complaign.ID = 1;
            complaign.EntryDate = DateTime.Now;
            complaign.EmployeeId = empId;
            complaign.StudentID = stdID;
            db.StudentComplaigns.Add(complaign);
            try
            {
                db.SaveChanges();
                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}