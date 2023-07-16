using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using smsCore.Data;
using Syncfusion.EJ2.Base;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.AccessControl;
using LazZiya.ImageResize;
using smsCore.Data.Helpers;
using System.Security.Claims;
using System.Runtime.ConstrainedExecution;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace smsCore.Controllers
{
    [Authorize]
    public class StudentsController : BaseController
    {
        private SchoolEntities db ;
        private readonly CurrentUser _user;
        private readonly SelectListHelper _selectListHelper;

        public StudentsController(SchoolEntities _db, CurrentUser currentUser , SelectListHelper selectListHelper)
        {
            db = _db;
            _user = currentUser;
            _selectListHelper = selectListHelper;
        }

        public static object Validate(object o, string[] excludedProperties)
        {
            var ps = o.GetType().GetProperties();
            foreach (var p in ps)
            {
                if (p.GetValue(o, null) != null)
                    continue;
                if (!excludedProperties.Contains(p.Name))
                    if (p.CanWrite)
                    {
                        object value = null;
                        var type = p.PropertyType;
                        if (type == typeof(int) || type == typeof(long) || type == typeof(double) ||
                            type == typeof(decimal) || type == typeof(short) || type == typeof(int) ||
                            type == typeof(long))
                            value = 0;
                        else if (type == typeof(string))
                            value = string.Empty;
                        else if (type == typeof(bool))
                            value = false;
                        else if (type == typeof(DateTime))
                            value = DateTime.Now;
                        p.SetValue(o, value, null);
                    }
            }

            return o;
        }

        public bool CreateDirectory(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                //var info = new DirectoryInfo(path);
                //var security = info.GetAccessControl();
                //var accessRule = new FileSystemAccessRule(Environment.UserName, FileSystemRights.FullControl,
                //    AccessControlType.Allow);
                //security.AddAccessRule(accessRule);
                //info.SetAccessControl(security);
                return true;
            }
            catch
            {
                return false;
            }
        }
        ////[ETag]
        
        public IActionResult Create()
        {
            int regno = 1;
            try
            {
                regno = db.Students.AsNoTracking().Max(m => m.RegistrationNo) + 1;
            }
            catch { }
            var student = new Student {AdmissionDate = DateTime.Now, RegistrationNo = regno};
            student.FatherAlive = "true";
            student.StudentMobiles = new List<StudentMobile>();
            ViewBag.ClassId = _selectListHelper.GetClassSelectList();
            ViewBag.Sections = _selectListHelper.GetSectionSelectList();
            ViewBag.Fee = _selectListHelper.GetSectionFeeGroupList();
             return View(student);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Student student)
        {
            var message = string.Empty;

            var exit = db.Students.Any(w => w.RegistrationNo == student.RegistrationNo);
            if (exit) return  Json(new { status = false, message = "Registration no Already Exist." });// "Registration no Already Exist";
            student.DOB = DateTimeHelper.ConvertDate(Request.Form["DOB"].ToString());
            student.AdmissionDate = DateTimeHelper.ConvertDate(Request.Form["AdmissionDate"].ToString());
            if (student.DOB.Year < 1950) 
                return Json(new { status = false, message = "Date of Birth is invalid." });// "Date You Provide Is Invalid ";
            if (student.AdmissionDate.Year < 1950 ) 
                return Json(new { status = false, message = "Admission date is invalid." });// "Date You Provide Is Invalid ";
            student.LastEditedBy = _user.UserID;
            student.LastEditedOn = DateTime.Now.ToString();
            student.ApplyLateFee = Request.Form["ApplyLateFee"].ToString() == null ? 1 : 0;

            student = (Student) Validate(student, new[] {"StudentName", "RegistrationNo"});
            var data = Request;
            
            var admission = new Admission();
            admission.CampuseID = _user.SelectedCampusId; //int.Parse(Request.Form["Campus"].ToString());
            var classId = int.Parse(Request.Form["Class"].ToString());
            var sectionId = int.Parse(Request.Form["Section"].ToString());
            admission.ClassSectionID = db.ClassSections.Where(w => w.CampusID==admission.CampuseID & w.ClassID == classId && w.SectionID == sectionId)
                .Select(s => s.ID).FirstOrDefault();
            admission.IsExpell = false;
            

            admission.Session = int.Parse(Request.Form["Session"].ToString());
            if (!string.IsNullOrEmpty(Request.Form["examList.Results"].ToString()))
            {
                var result1 = new Result();
                //result1.ObtainedMarks = Request.Form["examList.ObtainedMarks"];
            }

            if (!string.IsNullOrEmpty(Request.Form["prevEdu.PreviousBoard"].ToString()))
            {
                var previousEdu = new StudentPreviousEducation();
                previousEdu.AdmissionDate = DateTimeHelper.ConvertDate(Request.Form["prevEdu.AdmissionDate"].ToString(),true);
                previousEdu.Grade = Request.Form["prevEdu.Grade"].ToString();
                previousEdu.ObtainedMarks = Request.Form["prevEdu.ObtainedMarks"].ToString();
                previousEdu.PreviousBoard = Request.Form["prevEdu.PreviousBoard"].ToString();
                previousEdu.PreviousGroup = Request.Form["prevEdu.PreviousGroup"].ToString();
                previousEdu.RegNo = Request.Form["prevEdu.RegNo"].ToString();
                previousEdu.RollNo = Request.Form["prevEdu.RollNo"].ToString();
                previousEdu.Session = Request.Form["prevEdu.Session"].ToString();
                previousEdu.YearOfPassing = int.Parse(Request.Form["prevEdu.YearOfPassing"].ToString());
                admission.StudentPreviousEducations.Add(previousEdu);
            }

            student.Admissions.Add(admission);
            var allowTransport = !string.IsNullOrEmpty(Request.Form["AllowTransport"]);
            if (allowTransport)
            {
                var transport = new StudentsTransport();
                int.TryParse(Request.Form["tranport.DriverID"].ToString(), out var DriverID);
                transport.DriverID = DriverID;
                double.TryParse(Request.Form["tranport.Fare"].ToString(), out var fair);
                transport.Fare = fair;

                transport.Closed =Request.Form["tranport.Closed"].ToString();
                transport.IsClosed = false;
                transport.PickPoint = Request.Form["tranport.PickPoint"].ToString();
                transport.PickTime = Request.Form["tranport.PickTime"].ToString();
                transport.Started = Request.Form["tranport.Started"].ToString();
                transport.TripNumber = Request.Form["tranport.TripNumber"].ToString();
                
                student.StudentsTransports.Add(transport);
            }

            
            var StudentPhotoFile = data.Form.Files["StudentPhotoFile"];
            var FatherPhotoFile = data.Form.Files["FatherPhotoFile"];
            var FatherSignFile = data.Form.Files["FatherSignFile"];
            var GaurdinaSignFile = data.Form.Files["GaurdinaSignFile"];
            if (
                StudentPhotoFile !=
                null) // || (Request.Form["StdPhotoValue"] != null && !string.IsNullOrEmpty(Request.Form["StdPhotoValue"])))
            {
                byte[] bytedata;
                var Images = Image.FromStream(StudentPhotoFile.OpenReadStream());
                var resized = Images.ScaleAndCrop(350, 450);
                using (var Stream = new MemoryStream())
                {
                    resized.Save(Stream, ImageFormat.Jpeg);
                    bytedata = Stream.ToArray();
                }

                var photo = new StudentPhoto();
                photo.IsReplaced = false;
                photo.PhotoYear = Request.Form["PhotoYear"];
                photo.StudentImage = bytedata;
                student.StudentPhotos.Add(photo);
            }

            if (FatherPhotoFile != null || 
                !string.IsNullOrEmpty(Request.Form["FatherPhotoValue"]))
            {
                byte[] bytedata;
                var Images = Image.FromStream(FatherPhotoFile.OpenReadStream());
                var resized = Images.ScaleAndCrop(350, 450);
                using (var Stream = new MemoryStream())
                {
                    resized.Save(Stream, ImageFormat.Jpeg);
                    bytedata = Stream.ToArray();
                }

                student.FatherPhoto = bytedata;
            }

            if (
                FatherSignFile != null ||
                GaurdinaSignFile !=
                null) // || (Request.Form["FatherSignValue"] != null && !string.IsNullOrEmpty(Request.Form["FatherSignValue"])) || (Request.Form["GaurdianSignValue"] != null && !string.IsNullOrEmpty(Request.Form["FatherSignValue"])))
            {
                var signs = new SignatureSpeciman();
                if (
                    FatherSignFile !=
                    null) // || (Request.Form["FatherSignValue"] != null && !string.IsNullOrEmpty(Request.Form["FatherSignValue"])))
                {
                    byte[] bytedata;
                    var Images = Image.FromStream(FatherSignFile.OpenReadStream());
                    var resized = Images.ScaleAndCrop(350, 450);
                    using (var Stream = new MemoryStream())
                    {
                        resized.Save(Stream, ImageFormat.Jpeg);
                        bytedata = Stream.ToArray();
                    }

                    signs.FatherSign = bytedata;
                }

                if (GaurdinaSignFile != null || 
                    !string.IsNullOrEmpty(Request.Form["GaurdianSignValue"]))
                {
                    byte[] bytedata;
                    var Images = Image.FromStream(GaurdinaSignFile.OpenReadStream());
                    var resized = Images.ScaleAndCrop(350, 450);
                    using (var Stream = new MemoryStream())
                    {
                        resized.Save(Stream, ImageFormat.Jpeg);
                        bytedata = Stream.ToArray();
                    }

                    signs.GaurdianSign = bytedata;
                }

                student.SignatureSpecimen.Add(signs);
            }

            var mobiles = Request.Form["mobile.MobileNo"].ToString().Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            var def = Request.Form["IsDefault"].ToString().Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            var relations = Request.Form["mobile.Relation"].ToString().Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            var holders = Request.Form["mobile.MobileHolder"].ToString().Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            

            for (var i = 0; i < mobiles.Count(); i++)
            {
                var mobile = new StudentMobile();

                mobile.IsDefault = def.Length > i ? def[i] == "1" : false;
                mobile.MobileHolder = holders.Length > i ? holders[i] : string.Empty;
                mobile.MobileNo = mobiles[i];
                mobile.Relation = relations.Length > i ? relations[i] : string.Empty;
                student.StudentMobiles.Add(mobile);
            }

            if (Request.Form["FeeGroup"] == "-2")
            {
                student.FreeAdmissions.Add(new FreeAdmission { Reason = Request.Form["Reason"] });
            }
            else
            {
                if (!string.IsNullOrEmpty(Request.Form["discountcheck"]))
                {
                    var discount = Request.Form["Discount"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var FeetypeId = Request.Form["feetypeId"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var DiscountInAmount = Request.Form["discountInAmounts"].ToString()
                        .Split(new[] { ',' });

                    for (var i = 0; i < discount.Count(); i++)
                    {
                        var feeDiscount = new FeeDiscount();
                            feeDiscount.FeeTypeId = int.Parse(FeetypeId[i]);
                            feeDiscount.Discount = int.Parse(discount[i]);
                        if (DiscountInAmount[i].Trim() == "1")
                            feeDiscount.DiscountInAmount = true;
                        else
                            feeDiscount.DiscountInAmount = false;
                            feeDiscount.UserId = _user.UserID;
                            feeDiscount.EntryDate = DateTime.Now;
                            student.FeeDiscounts.Add(feeDiscount);
                        }
                }
            }           
            var result = new JsonResult(null);
            //if (ModelState.IsValid)
            //{
            bool status = false;
            try
            {
                db.Students.Add(student);
                
                db.SaveChanges();
                message = "Student information has been saved successfully.";
                status=true;
            }
            catch (Exception validations)
            {
                message = validations.InnerException == null ? validations.Message : validations.InnerException.Message;
            }

            return Json(new { status = status, message = message }); ;
        }

        public string ResizePhoto()
        {
            var photos = db.StudentPhotos;
            foreach (var rp in photos)
            {
                byte[] bytedata;
                var Images = Image.FromStream(new MemoryStream(rp.StudentImage));
                var resized = Images.ScaleAndCrop(350, 450);
                using (var Stream = new MemoryStream())
                {
                    resized.Save(Stream, ImageFormat.Jpeg);
                    bytedata = Stream.ToArray();
                }

                rp.StudentImage = bytedata;
            }

            db.SaveChanges();
            return "Success fully resized photo.";
        }
        ////[ETag]
        
        public IActionResult Search(string keyword = "")
        {
            ViewBag.keyword = keyword;
            return View();
        }


        [HttpPost]
        public JsonResult searchResult(DataManagerRequest dm, int campusId = 0, int classid = 0,
            int sectionid = 0, int id = 0, string keyword = "", bool Isexpell = false)
        {
            IQueryable<Admission> studnts;
            var campusIds = campusId <= 0 ? db.Campuses.Select(s => s.ID).ToArray() : new[] { campusId };
            studnts = db.Admissions.Include(a => a.Student).ThenInclude(i=>i.ExpellDetails).Include(a=>a.ClassSection).Where(w => campusIds.Contains(w.CampuseID));
             


            if (classid > 0)
                studnts = studnts.Where(w => w.ClassSection.ClassID == classid);

            if (sectionid > 0) 
                studnts = studnts.Where(w => w.ClassSection.SectionID == sectionid);

            if (!Isexpell)
                studnts = studnts.Where(w => !w.IsExpell);

            var data = studnts.Select(s => new
            {
                StudentId = s.Student.ID,
                s.Student.RegistrationNo,
                s.Student.DOB,
                s.ClassSection.Class.ClassName,
                s.Student.Sex,
                s.Student.StudentName,
                s.Student.FName,
                s.IsExpell,
                ExpellDetailsId = s.Student.ExpellDetails.Select(t => t.ID).DefaultIfEmpty().Max()
            });


           
            DataOperations operation = new DataOperations();
            if (!string.IsNullOrEmpty(keyword))
            {
                if (dm.Search==null)
                    dm.Search=new List<SearchFilter>();

                dm.Search.Add(new SearchFilter
                {
                    Operator="contains",
                    Key=keyword,
                    Fields=new List<string> { "StudentId", "RegistrationNo",
                    "DOB","ClassName","Sex","StudentName","FName" }
                });
            }

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
                if (dm.Sorted == null || dm.Sorted.Count==0)
                {
                    List<Sort> srt = new List<Sort>() { };
                    srt.Add(new Sort { Name = "RegistrationNo", Direction = "ascending" });
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

        public IActionResult DeleteAllStudent()
        {
            return View();
        }

     public JsonResult GetStudentsListForDelete(DataManagerRequest dm, int clasId= -1, int sectionid = -1, int campusId = -1, int yearId = -1)
        {
            var camp = campusId == -1 ? db.Campuses.Select(g => g.ID).ToArray() : new[] { campusId };
            var clsId = clasId == -1 ? db.Classes.Select(k => k.ID).ToArray() : new[] { clasId };
            var secid = sectionid == -1 ? db.Sections.Select(k => k.ID).ToArray() : new[] { sectionid };
            int[] sess = yearId == -1 ? db.FinancialYears.Select(s => s.financialYearId).ToArray() : new int[] { yearId };
            var list = db.Admissions.Where(w => w.IsExpell == false && sess.Contains(w.Session) && clsId.Contains(w.ClassSectionID) && secid.Contains(w.ClassSection.SectionID));
            var data = list.ToList().Select(n => new
            {
                n.Student.RegistrationNo,
                n.Student.StudentName,
                n.Student.FName,
                Class = n.ClassSection.Class.ClassName,
                Section = n.ClassSection.Section.SectionName
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

            if (dm.Sorted != null && dm.Sorted.Count > 0)
            {
                data = operation.PerformSorting(data, dm.Sorted);
            }

            if (dm.Skip != 0)
            {
                if (dm.Sorted.Count == 0)
                {
                    List<Sort> sort = new List<Sort>() { new Sort { Name = "RegistrationNo", Direction = "ascending" } };
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
        public IActionResult SessionWiseSearch()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetSessionWiseStudentsList(DataManagerRequest dm, int clasId = -1, int campusId = -1, int yearId = -1)
        {
            var campid = campusId == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] { campusId };
            var clsid = clasId == -1 ? db.Classes.Select(s => s.ID).ToArray() : new[] { clasId };
            int[] sessions = yearId == -1 ? db.FinancialYears.Select(s => s.financialYearId).ToArray() : new int[] { yearId };
            var students = db.Admissions.Where(w => campid.Contains(w.CampuseID) && sessions.Contains(w.Session) && clsid.Contains(w.ClassSection.ClassID));
         
            var data = students.Select(s => new
            {
                AdmissionID = s.Student.RegistrationNo,
                s.Student.StudentName,
                s.Student.FName,
                Class = s.ClassSection.Class.ClassName
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
                    srt.Add(new Sort { Name = "AdmissionID", Direction = "ascending" });
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
        ////[ETag]
        public IActionResult Studentview(int id)
        {
            var CampusIds = _user.GetCampusIds();
            var model = db.Students.Where(w =>
                    CampusIds.Contains(w.Admissions.OrderByDescending(d => d.ID).FirstOrDefault().CampuseID) &&
                    w.ID == id)
                .FirstOrDefault();            

            if (model==null)
            {
                if (Request.IsAjaxRequest())
                    return Json(new { message = "Student Not found." });
                else return NotFound();
            }
            var driverid = 0;
            try
            {
                driverid = model.StudentsTransports.DefaultIfEmpty().Where(w => w.IsClosed == false).DefaultIfEmpty()
                    .FirstOrDefault().DriverID;
            }
            catch
            {
            }
            return View(model);
        }

        ////[ETag]
        public IActionResult Edit(int id = 0)
        {
            var student = db.Students.Where(s => s.Admissions.Any(w => !w.IsExpell && s.ID == id)).FirstOrDefault();
            if (student == null) return NotFound();

            ViewBag.DriverId = student.StudentsTransports.Select(s => s.DriverID).FirstOrDefault();
            return View(student);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Update(Student student)
        {;
            var message = "";
            var result = new JsonResult(null);
            var ErrorObject = new List<object>();
            var existing = db.Students.Find(student.ID);

            if (existing == null) return Json(new { status = false, message = "Invalid record found." }); //return "Invalid record found.";
            student = (Student)Validate(student, new[] { "StudentName", "RegistrationNo" });
            existing.GuardianName = student.GuardianName;
            existing.AdmissionDate = DateTimeHelper.ConvertDate(Request.Form["AdmissionDate"]);
            existing.AdmittedClass = student.AdmittedClass;
            existing.ApplyLateFee = student.ApplyLateFee;
            existing.CNIC = student.CNIC;
            existing.DateForSubmission = DateTimeHelper.ConvertDate(Request.Form["DateForSubmission"],true).ToString();
            existing.DOB = DateTimeHelper.ConvertDate(Request.Form["DOB"]);
            existing.Domicile = student.Domicile;
            existing.Email = student.Email;
            existing.FatherAlive = student.FatherAlive;
            existing.FatherDepartment = student.FatherDepartment;
            existing.FatherNatureOfJob = student.FatherNatureOfJob;
            existing.FatherPhoto = student.FatherPhoto;
            existing.FatherProfession = student.FatherProfession;
            existing.FatherQualification = student.FatherQualification;
            existing.FeeDiscount = student.FeeDiscount;
            existing.FName = student.FName;
            existing.Games = student.Games;
            existing.GeneralRemarks = student.GeneralRemarks;
            existing.GuardianName = student.GuardianName;
            existing.LastEditedBy = student.LastEditedBy;
            existing.LastEditedOn = student.LastIntitution;
            existing.LiveWith = student.LiveWith;
            existing.MissingDocuments = student.MissingDocuments;
            existing.MotherName = student.MotherName;
            existing.MotherProfession = student.MotherProfession;
            existing.MotherQualification = student.MotherQualification;
            existing.MotherTongue = student.MotherTongue;
            existing.Nationality = student.MotherTongue;
            existing.OfficeTelephone = student.OfficeTelephone;
            existing.PermenantAddress = student.PermenantAddress;
            existing.PostalAddress = student.PostalAddress;
            existing.RegistrationNo = student.RegistrationNo;
            existing.Religion = student.Religion;
            existing.Remarks = student.Remarks;
            existing.ResidanceTelephone = student.ResidanceTelephone;
            existing.Sex = student.Sex;
            existing.StdRelation = student.StdRelation;
            existing.StudentCNIC = student.StudentCNIC;
            existing.StudentName = student.StudentName;
            existing.LastEditedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
            existing.LastEditedOn = DateTime.Now.ToString();

            var FatherPhotoFile = Request.Form.Files["FatherPhotoFile"];
            var pat = Request.Form["FatherPhotoValue"];

            var admissionId = int.Parse(Request.Form["AdmissionId"].ToString());
            var admission = db.Admissions.Find(admissionId);
            if (admission != null)
            {
                db.Entry(admission).State = EntityState.Modified;
                admission.CampuseID = int.Parse(Request.Form["Campus"].ToString());
                var classId = int.Parse(Request.Form["Class"].ToString());
                var sectionId = int.Parse(Request.Form["Section"].ToString());
                admission.ClassSectionID = db.ClassSections.Where(w => w.CampusID == admission.CampuseID & w.ClassID == classId && w.SectionID == sectionId)
                    .Select(s => s.ID).FirstOrDefault();
                admission.IsExpell = false;
                admission.Session = int.Parse(Request.Form["Session"].ToString());
            }

            if (!string.IsNullOrEmpty(Request.Form["prevEdu.PreviousBoard"]))
            {
                var found = int.TryParse(Request.Form["prevEdu.ID"], out var prevId);
                var previousEdu = db.StudentPreviousEducations.Find(prevId);
                if (!found || previousEdu == null)
                {
                    previousEdu = new StudentPreviousEducation();
                    previousEdu.AdmissionID = admission.ID;
                    db.StudentPreviousEducations.Add(previousEdu);
                }
                else
                {
                    db.Entry(previousEdu).State = EntityState.Modified;
                }
                previousEdu.AdmissionDate =DateTimeHelper.ConvertDate(Request.Form["prevEdu.AdmissionDate"].ToString(),true).Date ;
                previousEdu.Grade = Request.Form["prevEdu.Grade"];
                previousEdu.ObtainedMarks = Request.Form["prevEdu.ObtainedMarks"];
                previousEdu.PreviousBoard = Request.Form["prevEdu.PreviousBoard"];
                previousEdu.PreviousGroup = Request.Form["prevEdu.PreviousGroup"];
                previousEdu.RegNo = Request.Form["prevEdu.RegNo"];
                previousEdu.RollNo = Request.Form["prevEdu.RollNo"];
                previousEdu.Session = Request.Form["prevEdu.Session"];
                previousEdu.YearOfPassing = int.Parse(Request.Form["prevEdu.YearOfPassing"]);

                try
                {
                    db.SaveChanges();
                    ErrorObject.Add(new { previousEduAdded = true });
                }
                catch (Exception validations)
                {
                    var errors = new List<string>() { validations.Message };
                    //foreach (var v in validations.EntityValidationErrors)
                    //    foreach (var vv in v.ValidationErrors)
                    //        errors.Add(vv.PropertyName + ": " + vv.ErrorMessage);

                    ErrorObject.Add(new { previousEduAdded = false, errors });
                }
            }

            var trandport = !string.IsNullOrEmpty(Request.Form["AllowTransport"]);
            if (trandport)
            {
                StudentsTransport transport = null;
                if (int.TryParse(Request.Form["tranport.ID"], out var transId))
                {
                    transport = db.StudentsTransports.Find(transId);
                    if (transport != null)
                        db.Entry(transport).State = EntityState.Modified;
                }

                if (transport == null)
                {
                    transport = new StudentsTransport();

                    if (db.StudentsTransports.Count() != 0)
                        transport.ID = db.StudentsTransports.Max(m => m.ID) + 1;
                    else
                        transport.ID = 1;
                    transport.StudentID = student.ID;
                    db.StudentsTransports.Add(transport);
                }

                transport.Closed = DateTimeHelper.ConvertDate(Request.Form["tranport.Closed"],true).Date.ToString();
                int.TryParse(Request.Form["tranport.DriverID"], out var DriverID);
                transport.DriverID = DriverID;
                double.TryParse(Request.Form["tranport.Fare"], out var fair);
                transport.Fare = fair;
                transport.IsClosed = false;
                transport.PickPoint = Request.Form["tranport.PickPoint"].ToString() ?? String.Empty;
                transport.PickTime = Request.Form["tranport.PickTime"].ToString() ?? string.Empty;
                transport.Started = DateTimeHelper.ConvertDate(Request.Form["tranport.Started"].ToString(), true).Date.ToString();
                transport.TripNumber = Request.Form["tranport.TripNumber"].ToString() ?? string.Empty;
            }

            var StudentPhotoFile = Request.Form.Files["StudentPhotoFile"];
            var FatherSignFile = Request.Form.Files["FatherSignFile"];
            var GaurdinaSignFile = Request.Form.Files["GaurdinaSignFile"];
            if (
                StudentPhotoFile !=
                null) // || (Request.Form["StdPhotoValue"] != null && !string.IsNullOrEmpty(Request.Form["StdPhotoValue"])))
            {
                byte[] data;
                var Images = Image.FromStream(StudentPhotoFile.OpenReadStream());
                var resized = Images.ScaleAndCrop(350, 450);
                using (var Stream = new MemoryStream())
                {
                    resized.Save(Stream, ImageFormat.Jpeg);
                    data = Stream.ToArray();
                }

                var existingphoto = db.StudentPhotos.Where(w => w.StudentID == student.ID).ToList().LastOrDefault();
                if (existingphoto != null)
                {
                    existingphoto.IsReplaced = false;
                    existingphoto.PhotoYear = Request.Form["PhotoYear"];
                    existingphoto.StudentImage = data;
                }
                else
                {
                    var photo = new StudentPhoto();
                    photo.IsReplaced = false;
                    photo.PhotoYear = Request.Form["PhotoYear"];
                    try
                    {
                        photo.ID = db.StudentPhotos.Max(m => m.ID) + 1;
                    }
                    catch
                    {
                        photo.ID = 1;
                    }

                    photo.StudentImage = data;
                    photo.StudentID = student.ID;
                    db.StudentPhotos.Add(photo);
                }
            }

            if (FatherPhotoFile != null || 
                !string.IsNullOrEmpty(Request.Form["FatherPhotoValue"]))
            {
                byte[] bytedata;
                var Images = Image.FromStream(FatherPhotoFile.OpenReadStream());
                var resized = Images.ScaleAndCrop(350, 450);
                using (var Stream = new MemoryStream())
                {
                    resized.Save(Stream, ImageFormat.Jpeg);
                    bytedata = Stream.ToArray();
                }

                student.FatherPhoto = bytedata;
            }
            else
            {
                var fphotosexist = db.Students.Where(w => w.ID == student.ID).ToList().FirstOrDefault();
                if (fphotosexist != null) student.FatherPhoto = fphotosexist.FatherPhoto;
            }

            if (
                FatherSignFile != null ||
                GaurdinaSignFile !=
                null) // || (Request.Form["FatherSignValue"] != null && !string.IsNullOrEmpty(Request.Form["FatherSignValue"])) || (Request.Form["GaurdianSignValue"] != null && !string.IsNullOrEmpty(Request.Form["FatherSignValue"])))
            {
                var signs = new SignatureSpeciman();
                //signs.ID = int.Parse(Request.Form["signaturespecimenID"]);
                var signexist = db.SignatureSpecimen.Where(w => w.StudentID == student.ID).FirstOrDefault();
                if (FatherSignFile != null || 
                    !string.IsNullOrEmpty(Request.Form["FatherSignValue"]))
                {
                    byte[] data;
                    var Images = Image.FromStream(FatherSignFile.OpenReadStream());
                    var resized = Images.ScaleAndCrop(350, 450);
                    using (var Stream = new MemoryStream())
                    {
                        resized.Save(Stream, ImageFormat.Jpeg);
                        data = Stream.ToArray();
                    }

                    if (signexist != null)
                        signexist.FatherSign = data;
                    else
                        signs.FatherSign = data;
                }

                if (
                    GaurdinaSignFile !=
                    null) // || (Request.Form["GaurdianSignValue"] != null && !string.IsNullOrEmpty(Request.Form["GaurdianSignValue"])))
                {
                    byte[] data;
                    var Images = Image.FromStream(GaurdinaSignFile.OpenReadStream());
                    var resized = Images.ScaleAndCrop(350, 450);
                    using (var Stream = new MemoryStream())
                    {
                        resized.Save(Stream, ImageFormat.Jpeg);
                        data = Stream.ToArray();
                    }

                    if (signexist != null)
                        signexist.GaurdianSign = data;
                    else
                        signs.GaurdianSign = data;
                }

                signs.StudentID = student.ID;
                if (signexist.ID > 0)
                {
                    signexist.ID = signexist.ID;
                }
                else
                {
                    if (db.SignatureSpecimen.Count() == 0)
                        signs.ID = 1;
                    else
                        signs.ID = db.SignatureSpecimen.Max(m => m.ID) + 1;
                    student.SignatureSpecimen.Add(signs);
                }
            }

            var IDs = Request.Form["mobile.ID"].ToString().Split(new[] { ',' }, StringSplitOptions.None);
            var mobiles = Request.Form["mobile.MobileNo"].ToString().Split(new[] { ',' }, StringSplitOptions.None);
            var def = Request.Form["IsDefault"].ToString().Split(new[] { ',' }, StringSplitOptions.None);
            var relations = Request.Form["mobile.Relation"].ToString().Split(new[] { ',' }, StringSplitOptions.None);
            var holders = Request.Form["mobile.MobileHolder"].ToString().Split(new[] { ',' }, StringSplitOptions.None);
            //student.StudentMobiles.Clear();


            for (var i = 0; i < mobiles.Count(); i++)
                if (!string.IsNullOrEmpty(mobiles[i]))
                {
                    var mobile = db.StudentMobiles.Find(int.Parse(IDs[i]));
                    if (mobile == null)
                    {
                        mobile = new StudentMobile();
                        db.StudentMobiles.Add(mobile);
                    }
                    else
                    {
                        db.Entry(mobile).State = EntityState.Modified;
                    }

                    mobile.StudentID = student.ID;
                    mobile.IsDefault = def[i] == "1";
                    mobile.MobileHolder = holders[i];
                    mobile.MobileNo = mobiles[i];
                    mobile.Relation = relations[i];
                }

            if (Request.Form["FeeGroup"] == "-2")
            {
                var exist = db.FreeAdmissions.Where(w => w.StudentID == student.ID).FirstOrDefault();
                if (exist == null)
                    db.FreeAdmissions.Add(new FreeAdmission { StudentID = student.ID, Reason = Request.Form["Reason"] });
                else
                {
                    exist.Reason = Request.Form["Reason"];
                }
            }
            else
            {
                var exist = db.FreeAdmissions.Where(w => w.StudentID == student.ID).FirstOrDefault();
                if (exist != null)
                {
                    db.FreeAdmissions.Remove(exist);
                }
                var notcheckdis = string.IsNullOrEmpty(Request.Form["discountcheck"]);
                if (notcheckdis == false)
                {
                    var discount = Request.Form["Discount"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var FeetypeId = Request.Form["feetypeId"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var DiscountInAmount = Request.Form["discountInAmounts"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    for (var i = 0; i < discount.Count(); i++)
                    {
                        var FeetypeIds = int.Parse(FeetypeId[i]);
                        var fee = db.FeeDiscounts.Where(w => w.StudentId == student.ID && w.FeeTypeId == FeetypeIds)
                            .FirstOrDefault();
                        if (fee != null)
                        {
                            fee.FeeTypeId = FeetypeIds;
                            fee.Discount = decimal.Parse(discount[i]);
                            if (DiscountInAmount[i] == "1")
                                fee.DiscountInAmount = true;
                            else
                                fee.DiscountInAmount = false;
                            fee.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                            fee.EntryDate = DateTime.Now;
                        }
                        else
                        {
                            if (decimal.Parse(discount[i]) <= 0) continue;
                            var feeDiscount = new FeeDiscount();
                            feeDiscount.FeeTypeId = int.Parse(FeetypeId[i]);
                            feeDiscount.Discount = decimal.Parse(discount[i]);
                            if (DiscountInAmount[i] == "1")
                                feeDiscount.DiscountInAmount = true;
                            else
                                feeDiscount.DiscountInAmount = false;
                            feeDiscount.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                            feeDiscount.EntryDate = DateTime.Now;
                            feeDiscount.StudentId = student.ID;
                            db.FeeDiscounts.Add(feeDiscount);
                        }
                    }
                }
            }
            bool status = false;
            try
            {
                db.SaveChanges();
                message = "Student record updated successfully.";
                status=true;
                ErrorObject.Add(new {mobileAdded = true});
            }
            catch (Exception ex)
            {
                message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            }

            return Json(new { status = status, message = message });  ;
        }

        
        public IActionResult ExpellDetail(int studentId, int expellDetailID = 0)
        {
            var model = new ExpellDetail {UserID = User.FindFirstValue(ClaimTypes.NameIdentifier), StudentID = studentId, ID = 0};
            if (expellDetailID > 0)
            {
                model = db.ExpellDetails.Where(w => w.ID == expellDetailID).FirstOrDefault();
            }
            else
            {
                var admission = db.Admissions.Where(w => w.IsExpell == false && w.StudentID == studentId)
                    .FirstOrDefault();
                if (admission != null)
                {
                    model.EntryDate = DateTime.Now;
                    model.LastCampus = admission.Campus.CampusName;
                    model.LastClass = admission.ClassSection.Class.ClassName;
                    model.LastSection = admission.ClassSection.Section.SectionName;
                }
            }

            return View(model);
        }
        //[ETag]
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ExpellDetail(ExpellDetail model)
        {
            string message = "";
            try
            {
                model.EntryDate= DateTimeHelper.ConvertDate(Request.Form["EntryDate"]);
                if (model.ID > 0)
                {
                    db.Entry((ExpellDetail) model.Validate(new List<string>())).State = EntityState.Modified;
                }
                else
                {
                    try
                    {
                        model.ID = db.ExpellDetails.Max(w => w.ID) + 1;
                    }
                    catch
                    {
                        model.ID = 1;
                    }

                    var admissions = db.Admissions.Where(w => w.StudentID == model.StudentID);
                    foreach (var adm in admissions) adm.IsExpell = true;
                    db.ExpellDetails.Add((ExpellDetail) model.Validate(new List<string>()));
                }

                db.SaveChanges();
                message= "success";
            }
            catch (Exception aa)
            {
                message = aa.InnerException == null ? aa.Message : aa.InnerException.Message;
            }

            return Json(new { status = message=="success", message = message=="success" ? "Record uploaded successfully." : message });
        }
        ////[ETag]
        
        public IActionResult UnExpell(int expellDetailID)
        {
            var model = db.ExpellDetails.Where(w => w.ID == expellDetailID).ToList().LastOrDefault();
            var admission = db.Admissions.Where(w => w.StudentID == model.StudentID).ToList().LastOrDefault();
            ViewBag.campusID = admission.CampuseID;
            ViewBag.classID = admission.ClassSection.ClassID;
            ViewBag.sectionID = admission.ClassSection.SectionID;
            ViewBag.Session = admission.Session;
            return View(model);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UnExpell(ExpellDetail model)
        {
            string message = "";
            //var keys = Request.Form.AllKeys;
            //string d = "";
            var studentID = int.Parse(Request.Form["StudentID"].ToString());
            try
            {
                var sectionId = int.Parse(Request.Form["Section"].ToString());
                var classId = int.Parse(Request.Form["Class"].ToString());
                var classsectionId = db.ClassSections.Where(w => w.ClassID == classId && w.SectionID == sectionId)
                    .FirstOrDefault()?.ID;
                var admission = db.Admissions.Where(w => w.StudentID == model.StudentID).ToList().LastOrDefault();
                var year = int.Parse(admission.Session.ToString());
                var ReAdmitSession = int.Parse(Request.Form["Session"].ToString());
                if (year == ReAdmitSession)
                {
                    admission.IsExpell = false;
                }
                else
                {
                    admission = new Admission();
                    admission.ID = db.Admissions.Max(m => m.ID) + 1;
                    admission.StudentID = model.StudentID;
                    admission.ClassSectionID = classsectionId.Value;
                    admission.CampuseID = int.Parse(Request.Form["Campus"].ToString());
                    admission.Session = ReAdmitSession;
                    admission.IsExpell = false;
                    db.Admissions.Add(admission);
                }

                var readmission = new ReAdmission();
                readmission.EntryDate = DateTime.Now;
                readmission.Particular = Request.Form["Reason"];
                readmission.UserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                readmission.AdmissionID = admission.ID;
                try
                {
                    readmission.ID = db.ReAdmissions.Max(m => m.ID) + 1;
                }
                catch
                {
                    readmission.ID = 1;
                }

                try
                {
                    admission.ReAdmissions.Add(readmission);
                    db.SaveChanges();
                    message= "success";
                }
                catch(Exception ee)
                {
                    message= ee.Message;
                }
            }
            catch (Exception aa)
            {
                //var errors = new List<string>();
                //foreach (var v in aa.EntityValidationErrors)
                //foreach (var vv in v.ValidationErrors)
                //    errors.Add(vv.PropertyName + ": " + vv.ErrorMessage);
                //result.Data = new {errors, type = "fail"};
                message= "Unable to Delete selected record.";
            }

            return Json(new { status = message=="success", message = message=="success" ? "Record uploaded successfully." : message });
        }

        ////[ETag]
        
        [ActionName("Promote")]
        public IActionResult PromoteStudent()
        {
            return View();
        }

        
        [HttpPost]
        public JsonResult PromoteStudent(List<int> RegistrationNos, int ClassID, int CampusID, int Session,int old_session)
        {
            string message = "";
            //if(Session==old_session)
            //{
            //    message= "You cannot promote student to same session(year).";
            //    return Json(new { status = message=="success", message = message=="success" ? "Record uploaded successfully." : message });
            //}
            if(RegistrationNos.Count==0)
            {
                message = "Please select atleast one student to continue.";
                return Json(new { status = message=="success", message = message=="success" ? "Record uploaded successfully." : message });
            }

            var oldadmissions = db.Admissions
                .Where(w => RegistrationNos.Contains(w.Student.RegistrationNo) && w.IsExpell == false);
            var j = 1;
            foreach (var adm in oldadmissions)
            {

                adm.IsExpell = true;
                var admission = new Admission();
                admission.IsExpell = false;
                admission.StudentID = adm.StudentID;
                admission.CampuseID = CampusID;
                admission.Session = Session;
                j++;
                var classSectionID = 0;
                var clsSection = db.ClassSections
                    .Where(w => w.CampusID == CampusID & w.ClassID == ClassID && w.SectionID == adm.ClassSection.SectionID).FirstOrDefault();
                if (clsSection != null)
                {
                    classSectionID = clsSection.ID;
                }
                else
                {
                    var sections = db.ClassSections.Where(w =>w.CampusID==CampusID & w.ClassID == ClassID).FirstOrDefault();
                    if (sections == null)
                    {
                        var sectionId = db.Sections.FirstOrDefault().ID;
                        sections = new ClassSection { CampusID = CampusID, ClassID = ClassID, IsDeleted = false, SectionID = sectionId };
                        db.ClassSections.Add(sections);
                        db.SaveChanges();
                    }
                    classSectionID = db.ClassSections.Where(w => w.CampusID == CampusID & w.ClassID == ClassID).FirstOrDefault().ID;
                }

                admission.ClassSectionID = classSectionID;
                db.Admissions.Add(admission);               
            }

            try
            {
                db.SaveChanges();
                message = "success";
            }
            catch (Exception exception)
            {
                //foreach (var validity in exception.EntityValidationErrors)
                //foreach (var v in validity.ValidationErrors)
                //    message = v.PropertyName + "" + v.ErrorMessage;
                message = exception.InnerException==null?exception.Message:exception.InnerException.Message;
            }


            return Json(new { status = message=="success", message = message=="success" ? "Record uploaded successfully." : message });
        }

        [HttpPost]
        public JsonResult SearchStudents(DataManagerRequest dm , int campus, int classID = 0, int session = 0)
        {
            int Sno = 0;
            var PromoteList = db.Admissions.Include(i=>i.Student).Where(w => w.Session == session && w.CampuseID == campus && w.ClassSection.ClassID == classID &&
                            w.IsExpell == false).ToList().Select(s=>new {
                                SNO=Sno++,
                                s.ID,
                                //StudentId= s.StudentID,
                                s.Student.StudentName,
                                s.Student.FName,
                                s.Student.RegistrationNo
                                //StudentName = s.Student.StudentName,
                                //FName = s.Student.FName,
                                //RegistrationNo = s.Student.RegistrationNo
                            });
            DataOperations operation = new DataOperations();
            if (dm.Search != null && dm.Search.Count > 0)
            {
                PromoteList = operation.PerformSearching(PromoteList, dm.Search);  //Search
            }
            if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
            {
                PromoteList = operation.PerformSorting(PromoteList, dm.Sorted);
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                PromoteList = operation.PerformFiltering(PromoteList, dm.Where, dm.Where[0].Operator);
            }
            int count = PromoteList.Count();
            if (dm.Skip != 0)
            {
                PromoteList = operation.PerformSkip(PromoteList, dm.Skip);   //Paging
            }
            if (dm.Take != 0)
            {
                PromoteList = operation.PerformTake(PromoteList, dm.Take);
            }
            if (dm.RequiresCounts)
            {
                return Json(new { result = PromoteList, count = count });
            }
            else
            {
                return Json(new { result = PromoteList, count = count });
            }
        }
        //[ETag]
        
        public IActionResult Details(int id = 0)
        {
            var student = db.Students.Find(id);
            if (student == null) return NotFound();
            return View(student);
        }
        //[ETag]
        
        public IActionResult Delete(int id = 0)
        {
            var student = db.Students.Find(id);
            if (student == null) return NotFound();
            return View(student);
        }

        public JsonResult SearchStudent(int campus, int classid, int section, bool forResult = false, int examid = 0,
            int registration = 0, int subject = 0)
        {
            var studnts = db.Admissions.Where(w =>
                w.CampuseID == campus && w.ClassSection.SectionID == section && w.ClassSection.ClassID == classid &&
                !w.IsExpell).ToList();
            if (forResult)
                studnts = studnts.Where(w =>
                    w.Results.Where(ww => ww.ExamHeldID == examid && ww.ClassSubject.SubjectID == subject).Count() <
                    1 && w.Student.RegistrationNo == registration).ToList();
            else
                studnts = studnts.Where(w => w.Student.RegistrationNo == registration).ToList();
            JsonResult Json;
            if (studnts.Count > 0)
                Json = new JsonResult
               ( new
                    {
                        Student = studnts.Select(s => new {s.ID, s.Student.StudentName, s.Student.RegistrationNo}),
                        type = "success"
                    }
                );
            else
                Json = new JsonResult
                ( new {message = "No data found to display.", type = "Registration Number is not valid"}
                );
            return Json;
        }

        
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var student = db.Students.Find(id);
            db.Students.Remove(student);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        
        
        //public IActionResult GetfeediscountForJq(JqGridRequest request, int studentId)
        //{
        //    var feeTypes = db.FeeTypes.Where(w => w.AllowDiscount.HasValue && w.AllowDiscount.Value)
        //        .Select(s => new {s.Id, s.TypeName});
        //    var fee = new List<dynamic>();
        //    var sno = 1;
        //    foreach (var s in feeTypes)
        //    {
        //        var stdDiscount = db.FeeDiscounts.Where(w => w.StudentId == studentId).Where(w => w.FeeTypeId == s.Id);
        //        fee.Add(new
        //        {
        //            sno, ID = stdDiscount.Select(ss => ss.Id).DefaultIfEmpty().FirstOrDefault(), FeeTypeId = s.Id,
        //            FeeTypeName = s.TypeName,
        //            DiscountAmount = stdDiscount.Select(ss => ss.Discount).DefaultIfEmpty().FirstOrDefault(),
        //            DiscountInAmount = stdDiscount.Select(ss => ss.DiscountInAmount).DefaultIfEmpty().FirstOrDefault()
        //        });
        //        sno++;
        //    }

        //    var Feediscount = fee;

        //    var totalRecords = Feediscount.Count();
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

        //    data = Feediscount.Skip(skip).Take(perpage).ToList().Select(s => new JqGridRecord(s.ID.ToString(),
        //        new
        //        {
        //            s.ID, s.sno, s.FeeTypeName, s.FeeTypeId, s.DiscountAmount, s.DiscountInAmount,
        //            action =
        //                $"<input type=\"checkbox\" {(s.DiscountInAmount.ToString().ToLower() == "true" ? "checked" : "")}  /> "
        //        }));

        //    foreach (var d in data) response.Records.Add(d);
        //    //response.Records = data;
        //    response.Reader.RepeatItems = false;
        //    return new JqGridJsonResult {Data = response};
        //    //return db.Companies.ToList();
        //}
        public IActionResult GetfeeclassgroupList(DataManagerRequest dm)
        {
            var feeTypes = db.FeeTypes.Where(w => w.AllowDiscount.HasValue && w.AllowDiscount.Value)
               .Select(s => new { s.Id, s.TypeName });
            var fee = new List<dynamic>();
            foreach (var s in feeTypes)
            {
                fee.Add(new { s.Id, s.TypeName, Discount = 0, DiscountInAmount = false });                
            }
            var Feediscount = fee;     
          
            var data = fee.AsEnumerable();
            data = Feediscount.ToList().Select(s => 
                new
                {
                    s.Id,
                    s.TypeName,
                    discountInAmount = true,
                    Discount = ""
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

            if (dm.Sorted != null && dm.Sorted.Count > 0)
            {
                data = operation.PerformSorting(data, dm.Sorted);
            }

            if (dm.Skip != 0)
            {
                if (dm.Sorted.Count == 0)
                {
                    List<Sort> sort = new List<Sort>() { new Sort { Name = "AdmissionID", Direction = "ascending" } };
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

        public IActionResult GetfeeclassgroupEditList(DataManagerRequest dm, int studentId)
        {
            var feeTypes = db.FeeTypes.Where(w => w.AllowDiscount.HasValue && w.AllowDiscount.Value).Select(s => new { s.Id, s.TypeName });
            var fee = new List<dynamic>();
            foreach (var s in feeTypes)
            {
                var stdDiscount = db.FeeDiscounts.Where(w => w.StudentId == studentId).Where(w => w.FeeTypeId == s.Id);
                fee.Add(new
                {
                    ID = stdDiscount.Select(ss => ss.Id).DefaultIfEmpty().FirstOrDefault(),
                    FeeTypeId = s.Id,
                    FeeTypeName = s.TypeName,
                    DiscountAmount = stdDiscount.Select(ss => ss.Discount).DefaultIfEmpty().FirstOrDefault(),
                    DiscountInAmount = stdDiscount.Select(ss => ss.DiscountInAmount).DefaultIfEmpty().FirstOrDefault()
                });
            }

            var data = fee.ToList().Select(s => new
            {
                s.ID,
                FeeTypeId = s.FeeTypeId,
                TypeName = s.FeeTypeName,
                discountInAmount = s.DiscountInAmount,                //$"<input type=\"checkbox\" name=\"discountInAmount\"   onchange=\"checkdiscount(this)\" {(s.DiscountInAmount.ToString().ToLower() == "true" ? "checked" : "")} /> <input type=\"hidden\" name=\"feetypeId\" value=\"{s.FeeTypeId}\" /><input type=\"hidden\" id=\"discount\" name=\"discountInAmounts\" value=\"{0}\" />",
                Discount = s.DiscountAmount                           // $"<input type=\"number\" name=\"Discount\" value=\"{s.DiscountAmount}\" /> "
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

            if (dm.Sorted != null && dm.Sorted.Count > 0)
            {
                data = operation.PerformSorting(data, dm.Sorted);
            }

            if (dm.Skip != 0)
            {
                if (dm.Sorted.Count == 0)
                {
                    List<Sort> sort = new List<Sort>() { new Sort { Name = "AdmissionID", Direction = "ascending" } };
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

        //[ETag]
        
        public IActionResult searchbycontact()
        {
            return View();
        }

        public JsonResult searchcontact(string mobile, int campusId = 0, string checkdelete = "true")
        {
            var campusIds = campusId <= 0 ? db.Campuses.Select(s => s.ID).ToArray() : new[] {campusId};

            var result = new JsonResult(null);
            var tableFound = "";
           // if (checkBox1.Checked)
            {
                var table = db.Admissions.Include(i=>i.ClassSection.Class).Include(i=>i.ClassSection.Section).Include(i=>i.Student).Include(i=>i.Campus).Where(w =>
                        campusIds.Contains(w.CampuseID) && w.IsExpell == false &&
                        w.Student.OfficeTelephone.Trim() == mobile.Trim() ||
                        w.Student.ResidanceTelephone.Trim() == mobile.Trim() || w.Student.StudentMobiles
                            .Select(s => s.MobileNo.Trim()).FirstOrDefault().Contains(mobile.Trim()))
                    .ToList().Select(s => new
                    {
                        s.Student.ID,
                        s.Student.RegistrationNo,
                        Name =
                            $"<a href=\"{Url.Action("Studentview", "Students", new {id = s.Student.ID})}\">{s.Student.StudentName}<a/>",
                        s.Student.FName,
                        s.Student.CNIC,
                        s.Student.PostalAddress,
                        s.ClassSection.Class.ClassName,
                        s.ClassSection.Section.SectionName,
                        s.Campus.CampusName,
                        s.Student.OfficeTelephone,
                        s.Student.ResidanceTelephone,
                        Photo = "", //s.Student.StudentPhotos.Where(w => w.IsReplaced == false).DefaultIfEmpty().FirstOrDefault().StudentImage,
                        FatherPhots = s.Student.FatherPhoto,
                        Group = "----"
                    }).AsDataTable();

                tableFound = "Students";
                if (table.Rows.Count < 1)
                {
                    table = new DataTable();
                    table.Columns.Clear();
                    //|| w.EmployeeMobiles.Select(s => s.MobileNo.Trim()).Contains(mobile.Trim())
                    table = db.tbl_Employee.ToList().Where(w =>
                            campusIds.Contains(w.CampusID) && w.mobileNumber.Trim() == mobile.Trim()).ToList()
                        .Select(s => new
                        {
                            ID = s.Id,
                            RegistrationNo = "-----",
                            Name =
                                $"<a href=\"{Url.Action("EditEmployee", "Payroll", new {id = s.Id})}\">{s.employeeName}<a/>",
                            FName = s.FatherName,
                            s.CNIC,
                            PostalAddress = s.address,
                            ClassName = "----",
                            SectionName = "-----",
                            CampusName = "-----",
                            OfficeTelephone = s.mobileNumber,
                            ResidanceTelephone = "----",
                            s.Photo,
                            FatherPhots = "----",
                            Group = "----"
                        }).AsDataTable();

                    tableFound = "Staff";
                }

                if (table.Rows.Count < 1)
                {
                    table = new DataTable();
                    table.Columns.Clear();
                    table = db.AddmissionWaitingLists
                        .Where(w => campusIds.Contains(w.CampusID) && w.Phone.Trim() == mobile.Trim())
                        .Select(s => new
                        {
                            s.ID,
                            RegistrationNo = "-----",
                            s.Name,
                            s.FName,
                            CNIC = "----",
                            PostalAddress = s.Address,
                            s.Class.ClassName,
                            SectionName = "-----",
                            CampusName = "-----",
                            OfficeTelephone = s.Phone,
                            ResidanceTelephone = "----",
                            Photo = "---",
                            FatherPhots = "----",
                            Group = "----"
                        }).AsDataTable();

                    tableFound = "Admission Waiting list";
                }

                if (table.Rows.Count < 1)
                {
                    table = new DataTable();
                    table.Columns.Clear();
                    table = db.EmployeeWaitingLists.Where(w =>
                            w.Phone.Trim() == mobile.Trim() || w.Phone2.Trim() == mobile.Trim())
                        .Select(s => new
                        {
                            s.ID,
                            RegistrationNo = "-----",
                            s.Name,
                            FName = "----",
                            CNIC = "----",
                            PostalAddress = s.Address,
                            ClassName = s.Subject,
                            SectionName = "-----",
                            CampusName = "-----",
                            OfficeTelephone = s.Phone,
                            ResidanceTelephone = "----",
                            s.Photo,
                            FatherPhots = "----",
                            Group = "----"
                        }).AsDataTable();

                    tableFound = "Staff Waiting List";
                }

                if (table.Rows.Count < 1)
                {
                    table = new DataTable();
                    table.Columns.Clear();
                    table = db.StudentAlumnis.Where(w =>
                            campusIds.Contains(w.Student.Admissions.Select(s => s.CampuseID).FirstOrDefault()) &&
                            w.OfficePhone.Trim() == mobile.Trim() || w.ResidancePhone.Trim() == mobile.Trim() ||
                            w.DefaultCellNo.Trim() == mobile.Trim() || w.CellNo.Trim() == mobile.Trim())
                        .Select(s => new
                        {
                            s.ID,
                            RegistrationNo = "-----",
                            Name = s.Student.StudentName,
                            s.Student.FName,
                            s.Student.CNIC,
                            PostalAddress = s.OfficeAddress,
                            ClassName = "----",
                            SectionName = "-----",
                            CampusName = "-----",
                            OfficeTelephone = s.OfficePhone,
                            ResidanceTelephone = s.ResidancePhone,
                            s.Photo,
                            FatherPhots = s.Student.FatherPhoto,
                            Group = "----"
                        }).AsDataTable();

                    tableFound = "Student StudentAlumnis";
                }

                if (table.Rows.Count < 1)
                {
                    table = new DataTable();
                    table.Columns.Clear();
                    table = db.PublicMobiles.Where(w => w.MobileNo.Trim() == mobile.Trim())
                        .Select(s => new
                        {
                            s.ID,
                            RegistrationNo = "-----",
                            s.Name,
                            FName = "----",
                            CNIC = "----",
                            PostalAddress = s.Address,
                            ClassName = "----",
                            SectionName = "-----",
                            CampusName = "-----",
                            OfficeTelephone = "----",
                            ResidanceTelephone = "-----",
                            Photo = "----",
                            FatherPhots = "----",
                            Group = s.GroupType
                        }).AsDataTable();

                    tableFound = "Public Contact";
                }

                if (table.Rows.Count > 0)
                {
                    var lblTableFound = tableFound;
                    var lblRegNo = table.Rows[0]["RegistrationNo"].ToString().Trim(); // data.RegistrationNo.ToString();
                    var lbladdress = table.Rows[0]["PostalAddress"].ToString().Trim(); // data.PostalAddress;
                    var lblcamp = table.Rows[0]["CampusName"].ToString().Trim(); //data.CampusName;
                    var lblclas = table.Rows[0]["ClassName"].ToString().Trim(); //data.ClassName;
                    var lblFname = table.Rows[0]["FName"].ToString().Trim(); //data.FName;
                    var lblgrp = table.Rows[0]["Group"].ToString().Trim();
                    var lblHometel = table.Rows[0]["ResidanceTelephone"].ToString().Trim(); //data.ResidanceTelephone;
                    var lblName = table.Rows[0]["Name"].ToString().Trim(); //data.StudentName;
                    var lblofficetel = table.Rows[0]["OfficeTelephone"].ToString().Trim(); //data.OfficeTelephone;
                    var lblsec = table.Rows[0]["SectionName"].ToString().Trim(); //data.SectionName;
                    var id = table.Rows[0]["ID"].ToString().Trim(); //data.SectionName;

                    var data = table.AsEnumerable().Select(s => new
                    {
                        regno = lblRegNo,
                        address = lbladdress,
                        campus = lblcamp,
                        classes = lblclas,
                        fname = lblFname,
                        group = lblgrp,
                        hometel = lblHometel,
                        name = lblName,
                        officetel = lblofficetel,
                        sections = lblsec,
                        foundtable = lblTableFound,
                        id
                    }).FirstOrDefault();
                    //try
                    //{
                    //    var BackgroundImage = new Bitmap(new MemoryStream((byte[]) table.Rows[0]["Photo"]), true);
                    //    //pictureBox1.BackgroundImageLayout = ImageLayout.Stretch;
                    //}
                    //catch
                    //{
                    //    //pictureBox1.BackgroundImage = null;
                    //}

                    //try
                    //{
                    //    var BackgroundImage = new Bitmap(new MemoryStream((byte[]) table.Rows[0]["FatherPhots"]), true);
                    //}
                    //catch
                    //{
                    //    //pictureBox2.BackgroundImage = null;
                    //}

                    result =new JsonResult( data);
                }
                else
                {
                    dynamic datas = new
                    {
                        regno = ".....",
                        address = ".....",
                        campus = ".....",
                        classes = ".....",
                        fname = ".....",
                        group = ".....",
                        hometel = ".....",
                        name = ".....",
                        officetel = ".....",
                        sections = ".....",
                        foundtable = ".....",
                        id = "....."
                    };
                    result =new JsonResult( datas);
                    //MessageBox.Show("No data found to display");
                }

                //result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return result;
            }
        }

        //public IActionResult Getstudentorstaff(JqGridRequest request, string tablefound = "", int id = 0)
        //{
        //    IEnumerable<JqGridRecord> mobileRecords = new List<JqGridRecord>();
        //    var pno = request.PageIndex;
        //    var perpage = request.RecordsCount;
        //    var skip = perpage * pno;

        //    if (tablefound == "Students")
        //        mobileRecords = db.StudentMobiles.Where(w => w.StudentID == id)
        //            .Select(s => new {s.ID, s.MobileHolder, s.MobileNo, s.IsDefault}).OrderBy(o => o.ID).Skip(skip)
        //            .Take(perpage).ToList().Select(s =>
        //                new JqGridRecord(s.ID.ToString(), new {s.MobileNo, s.IsDefault, s.MobileHolder}));
        //    else
        //        mobileRecords = db.EmployeeMobiles.Where(w => w.StfID == id)
        //            .Select(s => new {s.ID, s.MobileNo, s.IsDefault, MobileHolder = ""}).OrderBy(o => o.ID).Skip(skip)
        //            .Take(perpage).ToList().Select(s =>
        //                new JqGridRecord(s.ID.ToString(), new {s.MobileNo, s.IsDefault, s.MobileHolder}));
        //    var totalRecords = mobileRecords.Count();

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

        //    response.Records.AddRange(mobileRecords);

        //    response.Reader.RepeatItems = false;
        //    return new JqGridJsonResult {Data = response};
        //}

        
        //[ETag]
        [HttpGet]
        public IActionResult UploadStdData()
        {
            return View();
        }
    }
}