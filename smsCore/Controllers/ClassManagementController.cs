using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using NuGet.Common;
using smsCore.Data;
using smsCore.Data.Helpers;
using Syncfusion.EJ2.Base;

namespace smsCore.Controllers
{
    [Authorize]
    public class ClassManagementController : BaseController
    {
        private readonly SchoolEntities db ;
        private readonly CurrentUser _user;
        private readonly SelectListHelper _selectListHelper;

        public ClassManagementController(SchoolEntities _db , CurrentUser user , SelectListHelper selectListHelper)
        {
            db = _db;
            _user = user;
            _selectListHelper = selectListHelper;
        }

        //
        // GET: /Admin/ClassManagement/

        public IActionResult Index()
        {
            return View();
        }

        //
        // GET: /Admin/ClassManagement/Details/5

        public IActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Admin/ClassManagement/Create
        public async Task<JsonResult> CreateNewList(DataManagerRequest dm, EnumManager.newgroupEntry type, int campusId = -1)
        {
            IEnumerable<ViewModel> existings = new List<ViewModel>();
            switch (type)
            {
                case EnumManager.newgroupEntry.Campus:
                    existings = await db.Campuses.Select(s => new ViewModel { Id = s.ID, Name = s.CampusName }).ToListAsync();
                    break;
                case EnumManager.newgroupEntry.Classes:
                    existings = await db.Classes.Select(s => new ViewModel { Id = s.ID, Name = s.ClassName }).ToListAsync();
                    break;
                case EnumManager.newgroupEntry.Section:
                    existings = await db.Sections.Select(s => new ViewModel { Id = s.ID, Name = s.SectionName }).ToListAsync();
                    break;
                case EnumManager.newgroupEntry.Subject:
                    existings = db.Subjects.ToList()
                        .Select(s => new ViewModel
                        { Id = s.ID, OrigName=s.SubjectName, Name = s.SubjectName + " " + (s.IsCoreSubject ? "(Core)" : "(Non Core)") }).ToList();
                    break;
                case EnumManager.newgroupEntry.Driver:
                    existings = await db.Drivers.Where(w => w.CampusID == campusId)
                        .Select(s => new ViewModel { Id = s.ID, Name = s.DriverName }).ToListAsync();
                    break;
                case EnumManager.newgroupEntry.Exam:
                    existings = await db.Exams.Select(s => new ViewModel { Id = s.ID, Name = s.ExamName }).ToListAsync();
                    break;
            }
            
            DataOperations operation = new DataOperations();
            int count = existings.Count();
            if(dm.Skip!=0)
            {
                List<Sort> sort = new List<Sort>() { new Sort { Direction="ascending", Name="Id" } };
               existings= operation.PerformSorting(existings, sort);
                existings = operation.PerformSkip(existings, dm.Skip);
            }
            if(dm.Take!=0)
            {
                existings = operation.PerformTake(existings, dm.Take);
            }

            return Json(new { result = existings, count } );

        }
        public class ViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string OrigName { get; set; }
        }

        public ActionResult Create(EnumManager.newgroupEntry type, int campusId = -1)
        {
            
            ViewBag.type = type;
            
            return View();
        }

        //
        // POST: /Admin/ClassManagement/Create
       
        [HttpPost]
        public async Task<string> Create(EnumManager.newgroupEntry type)
        {
            var ids = 0;
            if (Request.Form["CreateName"].ToString() == null || Request.Form["CreateName"].ToString() == "") return "Invalid Name";
            var name = Request.Form["CreateName"].ToString();
            if (Request.Form["ID"].ToString() != null && Request.Form["ID"] != "") ids = int.Parse(Request.Form["ID"].ToString());
            var campusId = _user.SelectedCampusId;

            //if (_user.BasicUserType == EnumManager.BasicUserType.Campus)
            //{
            //    campusId = _user.GetCampusIds().FirstOrDefault();
            //}
            //else
            //{
            //    //change in datatype
                
            //    if (Request.Form["Campus"].ToString() != null) campusId = Int32.Parse(Request.Form["Campus"].ToString());
            //}

            var id = 0;
            switch (type)
            {
                case EnumManager.newgroupEntry.Campus:
                    if (ids != 0)
                    {
                        var existdata =await db.Campuses.Where(w => w.ID == ids).FirstOrDefaultAsync();
                        existdata.CampusName = name;
                        break;
                    }

                    id = 0;
                    try
                    {
                        id = (from sect in db.Campuses
                              select sect.ID).Max();
                    }
                    catch
                    {
                        id = 0;
                    }

                    var exist = await db.Campuses.Where(w => w.CampusName.ToLower() == name.ToLower())
                        .FirstOrDefaultAsync();
                    if (exist != null)
                        return "Campus already exist with same name.";

                    var se = new Campus();
                    se.ID = id + 1;
                    //id = se.ID;
                    se.CampusName = name;
                    db.Campuses.Add(se);
                    break;
                case EnumManager.newgroupEntry.Classes:
                    if (ids != 0)
                    {
                        var existcdata =await db.Classes.Where(w => w.ID == ids).FirstOrDefaultAsync();
                        existcdata.ClassName = name;
                        break;
                    }

                    var existc =await db.Classes.Where(w => w.ClassName.ToLower() == name.ToLower())
                        .FirstOrDefaultAsync();
                    if (existc != null)
                        return "Class already exist with same name.";
                    id = 0;
                    try
                    {
                        id = (from sect in db.Classes
                              select sect.ID).Max();
                    }
                    catch 
                    {
                        id = 0;
                    }

                    var mc = new Class();
                   // mc.ID = id + 1;
                    mc.ClassName = name;
                    db.Classes.Add(mc);
                    break;
                case EnumManager.newgroupEntry.Section:
                    if (ids != 0)
                    {
                        var existsdata = db.Sections.Where(w => w.ID == ids).FirstOrDefault();
                        existsdata.SectionName = name;
                        break;
                    }

                    var exists = db.Sections.ToList().Where(w => w.SectionName.ToLower() == name.ToLower())
                        .FirstOrDefault();
                    if (exists != null)
                        return "Section already exist with same name.";
                    id = 0;
                    try
                    {
                        id = (from sect in db.Sections
                              select sect.ID).Max();
                    }
                    catch
                    {
                        id = 0;
                    }

                    var sc = new Section();
                   
                    sc.SectionName = name;
                    db.Sections.Add(sc);
                    break;
                case EnumManager.newgroupEntry.Subject:
                    if (ids != 0)
                    {
                        var existssdata = db.Subjects.Where(w => w.ID == ids).FirstOrDefault();
                        existssdata.SubjectName = name;
                        existssdata.IsCoreSubject =Request.Form["IsCoreSubject"] == "True";

                        break;
                    }

                    var existss = db.Subjects.ToList().Where(w => w.SubjectName.ToLower() == name.ToLower())
                        .FirstOrDefault();
                    if (existss != null)
                        return "Subject already exist with same name.";
                    id = 0;
                    try
                    {
                        id = (from sect in db.Subjects
                              select sect.ID).Max();
                    }
                    catch
                    {
                        id = 0;
                    }

                    var sb = new Subject();
                    sb.SubjectName = name;
                     
                    sb.IsCoreSubject = Request.Form["IsCoreSubject"] == "True";

                    db.Subjects.Add(sb);
                    break;
                case EnumManager.newgroupEntry.Exam:
                    if (ids != 0)
                    {
                        var existxdata = db.Exams.Where(w => w.ID == ids).FirstOrDefault();
                        existxdata.ExamName = name;
                        break;
                    }

                    var existx = db.Exams.ToList().Where(w => w.ExamName.ToLower() == name.ToLower()).FirstOrDefault();
                    if (existx != null)
                        return "Exam already exist with same name.";
                    id = 0;
                    try
                    {
                        id = (from exam in db.Exams
                              select exam.ID).Max();
                    }
                    catch
                    {
                        id = 0;
                    }

                    var ex = new Exam();
                    
                    ex.ExamName = name;
                    db.Exams.Add(ex);
                    break;
                case EnumManager.newgroupEntry.Driver:
                    if (ids != 0)
                    {
                        var existddata = db.Drivers.Where(w => w.ID == ids).FirstOrDefault();
                        existddata.DriverName = name;
                        existddata.CampusID = campusId;
                        break;
                    }

                    var driverexists = db.Drivers.ToList()
                        .Where(w => w.DriverName.ToLower() == name.ToLower() && w.CampusID == campusId)
                        .FirstOrDefault();
                    if (driverexists != null)
                        return "Driver already exist with same name.";
                    id = 0;
                    try
                    {
                        id = (from sect in db.Drivers
                              select sect.ID).Max();
                    }
                    catch
                    {
                        id = 0;
                    }

                    var dr = new Driver();
                    
                    dr.DriverName = name;
                    dr.CampusID = campusId;
                    db.Drivers.Add(dr);
                    break;
            }

            if (ModelState.IsValid)
                try
                {
                    await db.SaveChangesAsync();
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

            return ModelState.FirstOrDefault(w => w.Value.Errors.Count > 0).Value.Errors.FirstOrDefault()
                .ErrorMessage;
        }


        public ActionResult ClassSection()
        {
            return View();
        }

        public JsonResult GetSectionInClass(int classid, int campusId)
        {
            return Json(new
            {
                existing = db.ClassSections.Include(i=>i.Section).Include(i=>i.Admissions).Where(w => w.ClassID == classid && w.CampusID == campusId)
                    .Select(s => new
                    {
                        s.ID,
                        SectionName = s.Section.SectionName + " (" + s.Admissions.Where(w => !w.IsExpell).Count() +
                                      ") Students"
                    }),
                newsection = db.Sections
                    .Where(w => w.ClassSections.Where(ww => ww.ClassID == classid && ww.CampusID == campusId).Count() ==
                                0).Select(s => new {s.ID, s.SectionName})
            } );
        }

        public JsonResult GetClasses()
        {
            return Json(new {existing = db.Classes.Select(s => new {s.ID, s.ClassName})});
        }

        //AddClassSection
        
        [HttpPost]
        public async Task<string> AddClassSectionAsync(int classid, int sectionid, int campusId)
        {
             
            var exist = db.ClassSections
                .Where(w => w.CampusID == campusId && w.SectionID == sectionid && w.ClassID == classid).Any();
            if (exist) return "Selected Section alread exist in selected class.";
            var cls = new ClassSection {ClassID = classid, SectionID = sectionid,  CampusID = campusId};
            if (cls != null)
            {
                db.ClassSections.Add(cls);
                try
                {
                   await db.SaveChangesAsync();
                    return "true";
                }
                catch//(Exception ex)
                {
                    return "false";
                }
            }

            return "invalid";
        }

        public JsonResult GetClassSections(int classes)
        {
             var list = db.ClassSections.Where(c => c.ClassID == classes)
                .Select(c => new {sectionId = c.ID, c.Section.SectionName}).ToList();
            var  result = Json(new { Data = list }); 
             return result;
        }

        
        [HttpPost]
        public async Task<string> RemoveClassSectionAsync(int classsectionid)
        {
             var cls = db.ClassSections.Find(classsectionid);
            if (cls != null)
            {
                db.ClassSections.Remove(cls);
                try
                {
                   await db.SaveChangesAsync();
                    return "true";
                }
                catch
                {
                    return "false";
                }
            }

            return "invalid";
        }

        //Class Subject
        
        public IActionResult ClassSubject()
        {
           
            return View();
        }

        public JsonResult GetSubjectInClass(int classid, int campusId)
        {
            return Json(
                new
                {
                    existing = db.ClassSubjects.ToList().Where(w =>(!w.IsDeleted.HasValue || !w.IsDeleted.Value) && w.CampusID == campusId && w.ClassID == classid)
                        .Select(s => new {s.ID, s.Subject.SubjectName}),
                    newsubject = db.Subjects
                        .Where(w => w.ClassSubjects.Where(ww => (!ww.IsDeleted.HasValue || !ww.IsDeleted.Value) && ww.CampusID == campusId && ww.ClassID == classid)
                            .Count() == 0).Select(s => new {s.ID, s.SubjectName})
                } );
        }

        //AddClassSection
        
        [HttpPost]
        public string AddClassSubject(int classid, int subjectid, int campusId)
        {
             
            var cls = new ClassSubject {ClassID = classid, SubjectID = subjectid,  CampusID = campusId, IsDeleted=false};
            if (cls != null)
            {
                db.ClassSubjects.Add(cls);
                try
                {
                    db.SaveChanges();
                    return "true";
                }
                catch
                {
                    return "false";
                }
            }

            return "invalid";
        }

        
        [HttpPost]
        public async Task<string> RemoveClassSubjectAsync(int ClassSubjectid)
        {
             var cls = db.ClassSubjects.Where(w => w.ID == ClassSubjectid).FirstOrDefault();
            if (cls != null)
            {
                cls.IsDeleted = true;
                //db.ClassSubjects.Remove(cls);
                try
                {
                    await db.SaveChangesAsync();
                    return "true";
                }
                catch
                {
                    return "false";
                }
            }

            return "invalid";
        }

        //
        // GET: /Admin/ClassManagement/Edit/5
        
        public IActionResult changeclasssections()
        {
            ViewBag.ClassId = _selectListHelper.GetClassSelectList();
            ViewBag.Sections = _selectListHelper.GetSectionSelectList();
            return View();
        }

        [HttpGet]
        public IActionResult SearchStudents(int campus, int classID = 0, int sections = 0)
        {
            var list = db.Admissions
                .Where(w => w.CampuseID == campus && w.ClassSection.ClassID == classID &&
                            w.ClassSection.SectionID == sections && w.IsExpell == false)
                .Select(s => new
                {
                    s.ID,
                    s.StudentID,
                    s.Student.StudentName,
                    s.Student.FName,
                    s.Student.RegistrationNo
                })
                .ToList();

            return Json(list);
        }
 
         


        public async Task<JsonResult> StudentSectionChangeAsync(List<int> RegistrationNos, int ClassID, int CampusID, int Sections)
        {
            var classsectionid = db.ClassSections
                .Where(w => w.ClassID == ClassID && w.SectionID == Sections && w.CampusID == CampusID).FirstOrDefault()
                .ID;
            for (var i = 0; i < RegistrationNos.Count; i++)
            {
                var Id = RegistrationNos[i];
                var std = db.Admissions.Where(w => !w.IsExpell && w.ID == Id).FirstOrDefault();
                if (std != null)
                {
                    std.ClassSectionID = classsectionid;
                    std.CampuseID = CampusID;
                }
                //}
            }

            try
            {
               await db.SaveChangesAsync();
            }
            
            catch (Exception ex){ }   
            //catch (DbEntityValidationException exception)
            //{
            //    foreach (var validity in exception.EntityValidationErrors)
            //    foreach (var v in validity.ValidationErrors)
            //    {
            //        var va = v.PropertyName + "" + v.ErrorMessage;
            //    }
            //}

            var result = Json(new { Data = "Save" });
             return result;
        }

        public IActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Admin/ClassManagement/Edit/5

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

        //
        // GET: /Admin/ClassManagement/Delete/5

        public IActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Admin/ClassManagement/Delete/5

        [HttpPost]
        public IActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}