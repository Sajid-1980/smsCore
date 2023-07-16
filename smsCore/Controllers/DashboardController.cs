using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
using smsCore.Data.Helpers;
using System.Data;

namespace smsCore.Controllers
{
    //[CompressContent]
    [Authorize]
    public class DashboardController : BaseController
    {
        private SchoolEntities db;
        MenuHelper _helper;
        SeedDataHelper _seeder;
        public DashboardController(SchoolEntities _db, MenuHelper helper, SeedDataHelper seeder)
        {
            db = _db;
            _helper = helper;
            _seeder = seeder;
        }
        //
        // GET: /Admin/Dashboard/
        public void ResetSession()
        {
            //just to reset the session and prevent from auto expire in the browser
        }

        public async Task<IActionResult> SeedData()
        {
            await _seeder.SeedData();
            return new JsonResult( "Data seeded succesfully.");
        }

        public IActionResult GetMenu()
        {
            var all = _helper.GetMenuItems();
            //return new JsonResult { Data=all, JsonRequestBehavior=JsonRequestBehavior.AllowGet };
            return View("~/Views/Shared/MenuPartial/_LeftMenu.cshtml", all);
        }
        public IActionResult Index()
        {
            var comp = db.Company.FirstOrDefault();
            if (comp == null) return RedirectToAction("Install", "School");
            ViewBag.TotalStudent = db.Admissions.Where(w => !w.IsExpell).Count().ToString();

            ViewBag.TotalEmployee = db.tbl_Employee.Where(w => w.LeavedStaffs.Count == 0).Count().ToString();
            ViewBag.AbsentEmployee = db.tbl_Employee.Where(w =>
                w.EmployeeAttendances.Where(a =>
                    a.AttendanceDate.Date == DateTime.Now.Date &&
                    a.EmployeeAttendanceType.AttendanceName == "Absent").Count() > 0).Count().ToString();

            ViewBag.TotalParent = db.Admissions.Where(w => !w.IsExpell).Select(s => s.Student.CNIC).Distinct().Count()
                .ToString();
            ViewBag.NoOfCampuse = db.Campuses.Count();
            return View();
        }

        public ActionResult WebCam(string preview = "preview", string txtValue = "txtValue")
        {
            ViewBag.txtValue = txtValue;
            ViewBag.preview = preview;
            return View("~/Views/Dashboard/WebCam.cshtml");
        }

        //GetfeeGroupInClass
        public JsonResult GetfeeGroupInClass(int classID)
        {
            var result = new JsonResult(null);
            var list = db.FeeGroups.Select(s => new {s.ID, s.FeeGroupName}).ToList();
            var id = db.ClassFeeGroups.Where(w => w.ClassID == classID).Select(s => s.FeeGroupID).FirstOrDefault();
            result = new JsonResult( new {list, ID = id});
            //result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return result;
        }

        //GetAmountinfeeGroup
        public JsonResult GetAmountInFeegroup(int feeGroupId)
        {

            var feeStructure = db.FeeTypes.ToList().Select(s => new
            {
                s.Id,
                s.TypeName,
                Amount = s.FeeStructures.Where(w => w.FeeGroupId == feeGroupId && !w.Revised &&
                                                    w.IsActive && !w.IsDeleted).ToList().Select(ss => ss.Amount)
                    .FirstOrDefault()
            });
            var result = new JsonResult(feeStructure);

            //result.Data = feeStructure;
            //result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return result;
        }

        //GetFeeSlipDetailinEditFee
        public JsonResult GetFeeSlipDetail(int id, DateTime month)
        {
            var  Data = db.FeeSlipDetails.Where(w => w.FeeSlip.Admission.Student.RegistrationNo == id).ToList()
                .Where(w => w.FeeSlip.ForMonth.Month == month.Month && w.FeeSlip.ForMonth.Year == month.Year).Select(
                    s => new
                    {
                        s.Id, s.FeeSlipId, s.FeeType.TypeName, s.Amount, s.Discount, s.DiscountInAmount,
                        Receipts = s.FeeSlip.FeeSlipReceipts.Select(ss => new
                        {
                            ss.Id, ss.FeeSlipId, ss.FeeSlipPaymentMethod.PaymentMethodName, ss.Amount, ss.UserId,
                            ss.EditBy, ss.EditeOn, EntryDate = ss.EntryDate.ToString("dd, MMM, yyyy")
                        })
                    });
            var result = new JsonResult(Data);

            return result;
        }

        //GetFeeSlipReceiptEditFee
        public JsonResult GetFeeSlipreceipt(int id, DateTime month)
        {
            var Data = db.FeeSlipReceipts.Where(w => w.FeeSlip.Admission.Student.RegistrationNo == id).ToList()
                .Where(w => w.FeeSlip.ForMonth.Month == month.Month && w.FeeSlip.ForMonth.Year == month.Year).Select(
                    s => new
                    {
                        s.Id, s.FeeSlipId, s.FeeSlipPaymentMethod.PaymentMethodName, s.Amount, s.UserId, s.EditBy,
                        s.EditeOn, EntryDate = s.EntryDate.ToString("dd, MMM, yyyy")
                    });
            
            var result = new JsonResult(Data);
            return result;
        }

        public JsonResult cmbClassSectiononchange(int classId, int classSectionId)
        {

            var existing = db.TeachingSubjects.Where(w => w.ClassSectionID == classSectionId).ToList()
                .Where(w => w.CloseDate == null).ToList();


            var data = db.ClassSubjects.Where(w => w.ClassID == classId).ToList().Select(s => new
            {
                ID = existing.Where(w => w.SubjectId == s.SubjectID).Select(t => t.ID).FirstOrDefault(),
                SubjectId = s.SubjectID,
                s.Subject.SubjectName,
                StaffID = existing.Where(w => w.SubjectId == s.SubjectID && w.CloseDate == null).Select(t => t.StaffID)
                    .FirstOrDefault(),
                StartDate = existing.Where(w => w.SubjectId == s.SubjectID && w.CloseDate == null)
                    .Select(t => t.StartDate).FirstOrDefault().ToString("MM/dd/yyyy h:mm tt"),

                CloseDate = ""
            }).ToList();

            var result = new JsonResult(data);
            return result;
        }

        public JsonResult TimeTableInmanual(int classId, int classSectionsId, int campusid)
        {
            var result = new JsonResult(null);

            if (int.TryParse(classSectionsId.ToString(), out var clsSectionId) && clsSectionId > 0)
            {
                var classid = classId;
                var classSectionId = db.ClassSections.Where(w => w.ClassID == classid && w.SectionID == clsSectionId)
                    .Select(s => s.ID).FirstOrDefault();
                var subjects = db.ClassSubjects.Where(w => w.ClassID == classid)
                    .Select(s => new {s.SubjectID, s.Subject.SubjectName}).ToList();
                var timetableExisting = db.TimeTables.ToList();

                var timeTable = new List<dynamic>();

                foreach (var data in subjects)
                {
                    var Teacher = db.TeachingSubjects
                        .Where(w => w.ClassSectionID == classSectionId && w.SubjectId == data.SubjectID &&
                                    w.CloseDate == null).Select(s =>
                            s.tbl_Employee.employeeName + "(" + s.tbl_Employee.employeeCode + ")").FirstOrDefault();

                    var current = timetableExisting.Where(w =>
                            w.ClassID == classid && w.SectionID == clsSectionId && w.SubjectID == data.SubjectID)
                        .FirstOrDefault();
                    if (current != null)
                        timeTable.Add(new
                        {
                            current.ID, SubjectId = data.SubjectID, current.Pno,
                            SubjectName = data.SubjectName + " [" + Teacher + "]", current.StaffID,
                            TimeFrom = current.TimeFrom.ToString("hh:mm tt"),
                            TimeTo = current.TimeTo.ToString("hh:mm tt")
                        });
                    else
                        timeTable.Add(new
                        {
                            ID = 0, SubjectId = data.SubjectID, Pno = 0,
                            SubjectName = data.SubjectName + " [" + Teacher + "]", StaffID = current, TimeFrom = "",
                            TimeTo = ""
                        });
                }

                if (timeTable.Count == 0)
                {
                    timeTable.Add(new
                        {ID = 0, SubjectId = 0, Pno = 0, SubjectName = "", StaffID = 0, TimeFrom = "", TimeTo = ""});

                    result = new JsonResult(timeTable);
                }
                else
                {
                    result = new JsonResult(timeTable);
                }
            }

            return result;
        }

        public JsonResult GetStudentByClass()
        {
            var list = db.ClassSections.Select(s => new
            {
                s.ID,
                s.Section.SectionName
            }).ToList();
            var result = new JsonResult(list);
            return result;
        }

        public JsonResult GetSectionClass(int classID)
        {
                var list = db.ClassSections.Where(w => w.ClassID == classID)
                    .Select(s => new {s.ID, s.Section.SectionName}).ToList();
            var result = new JsonResult(list);
            return result;
        }

        

        public JsonResult GetRatingtype(int RatingId)
        {
            var result = new JsonResult(null);

            //try
            //{
            var MasterID = RatingId;
            var ratingtype = db.RatingMasters.Where(w => w.ID == MasterID).FirstOrDefault().RatingType.Trim();
            if (ratingtype == "Employee Rating")
            {
                //Employee Rating Event Rating

                var DataSource = db.tbl_Employee.Where(w => w.LeavedStaffs.Count < 1).Select(s =>
                    new {s.Id, StaffName = s.employeeName + " s/d/o " + s.FatherName}).ToList();
                var list = new SelectList(DataSource, "Id", "StaffName");
                result = new JsonResult(list);
            }

            return result;
        }

        public JsonResult RatingEntryRegNoChange(int regno)
        {
            var result = new JsonResult(null);

            var admid = db.Admissions.Where(w => w.Student.RegistrationNo == 0 && w.IsExpell == false).Select(s => s.ID)
                .FirstOrDefault();
            if (admid > 0)
            {
                var AdmisssionID = admid;
                var data = db.Admissions.Where(w => w.ID == admid).Select(s => new
                {
                    s.Student.RegistrationNo, s.Student.StudentName, s.Campus.CampusName,
                    s.ClassSection.Class.ClassName, s.ClassSection.Section.SectionName,
                    s.Student.StudentPhotos.Where(w => w.IsReplaced == false).FirstOrDefault().StudentImage
                }).ToList();
                var lblRegNo = data.FirstOrDefault().RegistrationNo.ToString();
                var lblName = data.FirstOrDefault().StudentName;
                var lblCampus = data.FirstOrDefault().CampusName;
                var lblClass = data.FirstOrDefault().ClassName;
                var lblSection = data.FirstOrDefault().SectionName;
                var img = data.FirstOrDefault().StudentImage;

                var source = db.Admissions.Select(s => new
                {
                    lblRegNo,
                    lblName,
                    lblCampus,
                    lblClass,
                    lblSection,
                    img
                }).FirstOrDefault();
                result = new JsonResult(source);
            }

            return result;
        }


        #region Photo

        //public JsonResult Rebind()
        //{
        //    var path = Session["path"].ToString(); // "http://localhost:55694/WebImages/" + Session["val"].ToString();
        //    return Json(path, JsonRequestBehavior.AllowGet);
        //}

        //public ActionResult Capture()
        //{
        //    if (Session["path"] != null)
        //    {
        //        var path = Server.MapPath(Session["path"].ToString());
        //        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        //    }

        //    var stream = Request.InputStream;
        //    string dump;
        //    using (var reader = new StreamReader(stream))
        //    {
        //        dump = reader.ReadToEnd();
        //        var nm = DateTime.Now;
        //        var date = nm.ToString("yyyymmddMMss");
        //        var path = Server.MapPath("~/Images/temp/" + date + "test.jpg");
        //        System.IO.File.WriteAllBytes(path, String_To_Bytes2(dump));
        //        Session["path"] = "/Images/temp/" + date + "test.jpg";
        //    }

        //    return Json(Session["path"].ToString(), JsonRequestBehavior.AllowGet);
        //}

        private byte[] String_To_Bytes2(string strInput)
        {
            var numBytes = strInput.Length / 2;
            var bytes = new byte[numBytes];
            for (var x = 0; x < numBytes; ++x) bytes[x] = Convert.ToByte(strInput.Substring(x * 2, 2), 16);
            return bytes;
        }

        #endregion
    }
}