using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using sms.Helpers;
using sms.Models;
using smsCore.Data;
using smsCore.Data.Helpers;
using Syncfusion.EJ2.Base;
using Utilities;

namespace smsCore.Controllers
{
    [Authorize]
    public class StudentReportsController : BaseController
    {
        private SignInManager<ApplicationUser> _signInManager;
        private UserManager<ApplicationUser> _userManager;
        private readonly SchoolEntities db ;
        private RoleManager<IdentityRole> roleManager;
        private readonly UtilityFunctions UtilityFunctions;
        DatabaseAccessSql dba;
        CurrentUser CurrentUser;
        public StudentReportsController(SignInManager<ApplicationUser> signin, UserManager<ApplicationUser> _user, SchoolEntities _db,RoleManager<IdentityRole> _rol, UtilityFunctions utilityFunctions, CurrentUser _CurrentUser, DatabaseAccessSql _sql)
        {
            _signInManager = signin;
            _userManager = _user;
            db = _db;
            roleManager = _rol;
            UtilityFunctions = utilityFunctions;
            CurrentUser = _CurrentUser;
            dba = _sql;
        }

        public ActionResult ClassWiseList()
        {
            return View();
        }

        public JsonResult GetClassWiseListInfo(DataManagerRequest dm,int clasId = -1, int sectionid = -1, int campusId = -1)
        {
            IQueryable<Admission> students;

            int[] ids = { };
            var campid = campusId == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] {campusId};
            students = db.Admissions.Where(w =>!w.IsExpell && campid.Contains(w.CampuseID));
            if (clasId > 0)
                students = students.Where(w => w.ClassSection.ClassID == clasId);

            if (sectionid > 0)
                students = students.Where(w => w.ClassSection.SectionID == sectionid);


            var data = students.Select(s => new
            {
                AdmissionID = s.Student.RegistrationNo,
                s.Student.StudentName,
                s.Student.FName,
                s.ClassSection.Class.ClassName,
                MobileNo = s.Student.StudentMobiles.FirstOrDefault().MobileNo,
                Col7 = s.Student.CNIC
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

        
        public ActionResult StudentIdentification()
        {
            return View();
        }

        public JsonResult GetClassStudentIdentification(DataManagerRequest dm,int clasId = -1, int campusId = -1, int sectionid = -1,
            string rdoByClass = "", string rdoByAdm = "", string regno = "")
        {
            int[] ids = { };
            var classids = clasId == -1 ? db.Classes.Select(s => s.ID).ToArray() : new[] {clasId};
            var campid = campusId == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] {campusId};
            var secid = sectionid == -1
                ? db.ClassSections.Select(s => s.ID).ToArray()
                : new[]
                {
                    db.ClassSections.Where(w => w.ClassID == clasId && w.SectionID == sectionid).Select(s => s.ID)
                        .FirstOrDefault()
                };

            //int[] secid = new int[] { };
            if (sectionid == -1)
            {
                if (clasId > 0)
                    secid = db.ClassSections.Where(w => w.ClassID == clasId).Select(s => s.ID).ToArray();
                else
                    secid = db.ClassSections.Select(s => s.ID).ToArray();
            }

            if (rdoByClass == "true")
                ids = db.Admissions
                    .Where(w => w.IsExpell == false && secid.Contains(w.ClassSectionID) && campid.Contains(w.CampuseID))
                    .Select(s => s.ID).ToArray();
            else if (rdoByAdm == "true") ids = UtilityFunctions.ParseAdmIDs(regno, campid, false);
            var data = db.Admissions.Where(w => w.IsExpell == false && ids.Contains(w.ID)).AsEnumerable().Select(s => new
            {
                AdmissionID = s.Student.RegistrationNo,
                s.Student.StudentName,
                s.Student.FName,
                PermenantAddress = s.Student.PostalAddress,
                s.ClassSection.Class.ClassName,
                s.ClassSection.Section.SectionName,
                // StudentImage = s.Student.StudentPhotos.Where(w => w.IsReplaced == false).FirstOrDefault() == null ? null : s.Student.StudentPhotos.Where(w => w.IsReplaced == false).FirstOrDefault().StudentImage,
                MobileNo = string.Join(", ", s.Student.StudentMobiles.Select(ss => ss.MobileNo).ToArray()),
                Transport = s.Student.StudentsTransports.Where(w => w.IsClosed == false).FirstOrDefault() == null
                    ? "NO"
                    : "Yes"
                //Transport = s.Student.StudentsTransports.DefaultIfEmpty().FirstOrDefault().IsClosed == false ? " Yes " : "No"
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

        
        public ActionResult PresentStudentsInformation()
        {
            return View();
        }

        public JsonResult GetPresentStudentsInformation(DataManagerRequest dm,int clasId = -1, int campusId = -1, string rdoByClass = "false",
            string rdoByAdm = "false", string regno = "")
        {
            int[] ids = { };
            var campid = campusId == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] {campusId};
            var clsid = clasId == -1 ? db.Classes.Select(s => s.ID).ToArray() : new[] {clasId};
            if (rdoByClass == "true")
                ids = db.Admissions
                    .Where(w => w.IsExpell == false && w.ClassSection.ClassID == clasId && campid.Contains(w.CampuseID))
                    .Select(s => s.ID).ToArray();

            else if (rdoByAdm == "true") ids = UtilityFunctions.ParseAdmIDs(regno, campid, false);

            var data = db.Admissions.Where(w => w.IsExpell == false && ids.Contains(w.ID)).ToList().Select(s => new
            {
                AdmissionID = s.Student.RegistrationNo,
                s.Student.StudentName,
                s.Student.FName,
                s.Student.MotherTongue,
                s.Student.PostalAddress,
                DOB = s.Student.DOB.ToString("dd MMM yyyy"),
                Gender = s.Student.Sex,
                AppliedClass = s.Student.AdmittedClass,
                PresentClass = s.ClassSection.Class.ClassName,
                AdmissionDate = s.Student.AdmissionDate.ToString("dd MMM yyyy")
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

        
        public ActionResult WithdrewStudent()
        {
            return View();
        }

        public JsonResult GetWithdrawStudentsList(DataManagerRequest dm, int clasId = -1, int campusId = -1, int yearId=-1,
            string rdoByClass = "false", string rdoByAdm = "false", string regno = "")
        {
            var campid = campusId == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] { campusId };
            int[] sessions = yearId == -1 ? db.FinancialYears.Select(s => s.financialYearId).ToArray() : new int[] { yearId };
            var students = db.Admissions.Distinct()
                        .Where(w => w.IsExpell && sessions.Contains(w.Session) & w.Student.ExpellDetails.Any());

            if (rdoByClass == "true")
            {
                var clsid = clasId == -1 ? db.Classes.Select(s => s.ID).ToArray() : new[] { clasId };
                students = students.Where(w => clsid.Contains(w.ClassSection.ClassID) &&
                                          campid.Contains(w.CampuseID));
            }
            else
            {

                var ids = UtilityFunctions.ParseRegNos(regno, campid);
                students = students.Where(w => campid.Contains(w.CampuseID) &&
                                  ids.Contains(w.Student.RegistrationNo));
            }
            var data = students.ToList().Select(s => new
            {
                AdmissionID = s.Student.RegistrationNo,
                s.Student.StudentName,
                s.Student.FName,
                s.Student.MotherTongue,
                DOB = s.Student.DOB,
                AppliedClass = s.ClassSection.Class.ClassName,
                PresentClass = s.Student.AdmittedClass,
                EntryDate=s.Student.ExpellDetails.Select(t=>t.EntryDate).OrderByDescending(o=>o).FirstOrDefault()
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


        public ActionResult ReportByGender()
        {
            return View();
        }

        public JsonResult GetReportByGender(int clasId = -1, int campusId = -1)
        {
            var classid = clasId == -1 ? db.Classes.Select(s => s.ID).ToArray() : new[] {clasId};
            var campid = campusId == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] {campusId};
            var q =
                "SELECT TOP (100) PERCENT dbo.Classes.ID,dbo.Classes.ClassName, dbo.Campuses.CampusName, LTRIM(RTRIM(dbo.Students.Sex)) AS Sex FROM dbo.Admissions INNER JOIN dbo.Students ON dbo.Admissions.StudentID = dbo.Students.ID INNER JOIN dbo.ClassSections ON dbo.Admissions.ClassSectionID = dbo.ClassSections.ID INNER JOIN dbo.Classes ON dbo.ClassSections.ClassID = dbo.Classes.ID INNER JOIN dbo.Campuses ON dbo.Admissions.CampuseID = dbo.Campuses.ID WHERE        (dbo.Admissions.IsExpell = 0) AND (dbo.Classes.ID IN (" +
                string.Join(",", classid) + ")) AND (dbo.Campuses.ID IN(" + string.Join(",", campid) +
                ")) ORDER BY dbo.Classes.ID,dbo.Campuses.ID ASC";
            var tab = dba.CreateTable(q);

            var pt = new Pivot(tab);
           var data = pt.PivotData("Sex", AggregateFunction.Count, new[] {"ID", "ClassName", "CampusName"}, new[] {"Sex"});
            data.Columns.Add("total");
            var result = data.AsEnumerable().Select(s => new { ClassName = s["ClassName"].ToString(),
            CampusName=s["CampusName"],Male=s["Male"],Female=s["Female"]
            });
            return Json(new { result = result, count = result.Count() });
        }

        
        public ActionResult ListofFreeAdmissions()
        {
            return View();
        }

        public JsonResult GetListofFreeAdmissions(DataManagerRequest dm,int clasId = -1, int campusId = -1)
        {
            var clsid = clasId == -1 ? db.Classes.Select(s => s.ID).ToArray() : new[] {clasId};
            var campusIds = campusId == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] {campusId};
            var data = db.Admissions.Where(w =>
                    w.IsExpell == false && clsid.Contains(w.ClassSection.ClassID) && campusIds.Contains(w.CampuseID) && w.Student.FreeAdmissions.Any())
                .Select(s => new
                {
                    s.Student.RegistrationNo,
                    s.Campus.CampusName,
                    s.ClassSection.Class.ClassName,
                    s.Student.StudentName,
                    s.Student.FName,
                    Reason = s.Student.FreeAdmissions.Select(f => f.Reason).FirstOrDefault()
                }).GroupBy(o => new { o.CampusName, o.ClassName, o.RegistrationNo, o.StudentName, o.FName }).Select(s => new
                {
                    s.Key.CampusName,
                    s.Key.ClassName,
                    s.Key.RegistrationNo,
                    s.Key.StudentName,
                    s.Key.FName,
                    s.FirstOrDefault().Reason
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

        
        public ActionResult OrphanList()
        {
            return View();
        }

        public JsonResult GetOrphanList(DataManagerRequest dm,int clasId = -1, int campusId = -1, int sectionid = -1, string fatherlive = "")
        {
            int[] ids = { };
            var clsID = clasId == -1 ? db.Classes.Select(s => s.ID).ToArray() : new[] {clasId};
            var campid = campusId == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] {campusId};
            var secid = sectionid == -1
                ? db.ClassSections.Select(s => s.ID).ToArray()
                : new[]
                {
                    db.ClassSections.Where(w => w.ClassID == clasId && w.SectionID == sectionid).Select(s => s.ID).FirstOrDefault()
                };

            if (sectionid == -1)
            {
                if (clasId > 0)
                    secid = db.ClassSections.Where(w => w.ClassID == clasId).Select(s => s.ID).ToArray();
                else
                    secid = db.ClassSections.Select(s => s.ID).ToArray();
            }
            var data = db.Admissions.Where(w =>
                 secid.Contains(w.ClassSectionID) && campid.Contains(w.CampuseID) &&
                 w.Student.FatherAlive.Trim().ToLower() == fatherlive.Trim().ToLower() && w.IsExpell == false).Select(
                 s => new
                 {
                     AdmissionID = s.Student.RegistrationNo,
                     s.Student.StudentName,
                     s.Student.FName,
                     ClassName = s.ClassSection.Class.ClassName.Trim() + "-" +
                     s.ClassSection.Section.SectionName.Trim() + "(C-" + s.CampuseID + ")",
                     s.Student.PostalAddress
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

        #region Student Transport Report

        
        public ActionResult ClassWise()
        {
            return View();
        }

        public JsonResult GetClassWiseList(DataManagerRequest dm, int clasId = -1, int campusId = -1)
        {
            var clsID = clasId == -1 ? db.Classes.Select(s => s.ID).ToArray() : new[] { clasId };
            var cid = campusId == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] { campusId };
            var data = db.Admissions
                .Where(w => w.IsExpell == false && clsID.Contains(w.ClassSection.ClassID) &&
                            cid.Contains(w.CampuseID) &&
                            w.Student.StudentsTransports.Any(ww => ww.IsClosed == false)).Select(s => new
                            {
                                AdmissionID = s.Student.RegistrationNo,
                                s.Student.StudentName,
                                s.Student.FName,
                                s.ClassSection.Class.ClassName,
                                DriverName = s.Student.StudentsTransports.Where(w => w.IsClosed == false).Select(t => t.Driver.DriverName).FirstOrDefault(),
                                s.Student.StudentsTransports.Where(w => w.IsClosed == false).FirstOrDefault().PickPoint,
                                s.Student.StudentsTransports.Where(w => w.IsClosed == false).FirstOrDefault().PickTime,
                                s.Student.StudentsTransports.Where(w => w.IsClosed == false).FirstOrDefault().TripNumber
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

            if (dm.Sorted != null && dm.Sorted.Count>0)
            {
                data = operation.PerformSorting(data, dm.Sorted);
            }

            if (dm.Skip!=0)
            {
                if (dm.Sorted.Count == 0)
                {
                    List<Sort> sort = new List<Sort>() { new Sort { Name= "AdmissionID", Direction="ascending" } };
                    data = operation.PerformSorting(data,sort);
                }

                data = operation.PerformSkip(data, dm.Skip);
            }

            if (dm.Take != 0)
            {
                data = operation.PerformTake(data, dm.Take);
            }

            return Json(new { result = data, count = data.Count() });
        }

        
        public ActionResult DriverWise()
        {
            var CampusIds = CurrentUser.GetCampusIds();
            ViewBag.driverlist = new SelectList(db.Drivers.Where(w => CampusIds.Contains(w.CampusID)).ToList(), "ID",
                "DriverName");
            return View();
        }

        public JsonResult GetDriverWiseList(DataManagerRequest dm,int driverId = -1, int campusId = -1)
        {
            var cid = campusId == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] { campusId };
            var data = db.Admissions
                .Where(w => w.IsExpell == false && cid.Contains(w.CampuseID) &&
                w.Student.StudentsTransports.Where(ww=> ww.IsClosed == false ).FirstOrDefault().DriverID == driverId &&
                            w.Student.StudentsTransports.Any(ww => ww.IsClosed == false)).Select(s => new
                            {
                                s.ID,
                                AdmissionID = s.Student.RegistrationNo,
                                s.Student.StudentName,
                                s.Student.FName,
                                s.ClassSection.Class.ClassName,
                                DriverName = s.Student.StudentsTransports.Where(w => w.IsClosed == false).Select(t => t.Driver.DriverName).FirstOrDefault(),
                                s.Student.StudentsTransports.Where(w => w.IsClosed == false).FirstOrDefault().PickPoint,
                                s.Student.StudentsTransports.Where(w => w.IsClosed == false).FirstOrDefault().PickTime,
                                s.Student.StudentsTransports.Where(w => w.IsClosed == false).FirstOrDefault().Fare
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

        
        public ActionResult TripWise()
        {
            var CampusIds = CurrentUser.GetCampusIds();
            ViewBag.driverlist = new SelectList(db.Drivers.Where(w => CampusIds.Contains(w.CampusID)).ToList(), "ID",
                "DriverName");
            ViewBag.triplist =
                new SelectList(
                    db.StudentsTransports.Where(w =>
                        CampusIds.Contains(w.Student.Admissions.Select(s => s.CampuseID).FirstOrDefault())),
                    "TripNumber", "TripNumber");
            return View();
        }

        public JsonResult GetTripWiseList(DataManagerRequest dm,int driverId = -1, int tripnumber = -1, int campusId = -1)
        {
            var campusid = campusId == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] {campusId};
            var data = db.Admissions.Where(w =>
                    w.IsExpell == false && campusid.Contains(w.CampuseID) &&
                    w.Student.StudentsTransports.Where(ww => ww.IsClosed == false).FirstOrDefault().DriverID ==
                    driverId &&
                    w.Student.StudentsTransports.Where(ww => ww.IsClosed == false).FirstOrDefault().TripNumber ==
                    tripnumber.ToString() && w.Student.StudentsTransports.Any(ww => ww.IsClosed == false))
                .ToList().Select(s => new
                {
                    AdmissionID = s.Student.RegistrationNo,
                    s.Student.StudentName,
                    s.Student.FName,
                    s.ClassSection.Class.ClassName,
                    DriverName = s.Student.StudentsTransports.Where(w => w.IsClosed == false).Select(dn => dn.Driver.DriverName).FirstOrDefault(),
                    s.Student.StudentsTransports.Where(w => w.IsClosed == false).FirstOrDefault().PickPoint,
                    s.Student.StudentsTransports.Where(w => w.IsClosed == false).FirstOrDefault().PickTime,
                    s.Student.StudentsTransports.Where(w => w.IsClosed == false).FirstOrDefault().TripNumber
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

        #endregion

        #region Student List

        
        public ActionResult PhoneList()
        {
            return View();
        }

        public JsonResult GetPhoneList(DataManagerRequest dm, int clasId = -1, int campus = -1)
        {
            var classids = clasId == -1 ? db.Classes.Select(s => s.ID).ToArray() : new[] {clasId};
            var campusids = campus == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] {campus};
            var data = db.Admissions
                .Where(w => w.IsExpell == false && campusids.Contains(w.CampuseID) &&
                            classids.Contains(w.ClassSection.ClassID)).ToList().Select(s => new
                            {
                                AdmissionID = s.Student.RegistrationNo,
                                s.Student.StudentName,
                                s.Student.FName,
                                TelephoneResidance = s.Student.ResidanceTelephone,
                                Class = s.ClassSection.Class.ClassName,
                                MobileNo = string.Join(", ",
                        s.Student.StudentMobiles.Select(ss => ss.MobileNo.Trim().Replace(",", "").ToString()).ToArray())
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

        
        public ActionResult MissingDocuments()
        {
            return View();
        }

        public JsonResult GetMissingDocuments(DataManagerRequest dm,int clasId = -1, int campusID = -1, string category = "")
        {
            var classids = clasId == -1 ? db.Classes.Select(s => s.ID).ToArray() : new[] {clasId};
            var campid = campusID == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] {campusID};
            IEnumerable<object> data=null;
            if (category.Trim().ToLower() == "missingdocument")
            {
            data = db.Admissions.Where(w =>
                       w.IsExpell == false && classids.Contains(w.ClassSection.ClassID) && campid.Contains(w.CampuseID) &&
                       w.Student.MissingDocuments != "no" && w.Student.MissingDocuments != "" &&
                       w.Student.MissingDocuments != "yes" && w.Student.MissingDocuments.Trim().ToLower() != "done" &&
                       !string.IsNullOrEmpty(w.Student.MissingDocuments)).Select(s => new
                       {
                           AdmissionID = s.Student.RegistrationNo,
                           s.Student.StudentName,
                           FName = s.Student.FName,
                           EntryDate = s.Student.DateForSubmission,
                           s.ClassSection.Class.ClassName,
                           s.Campus.CampusName
                       }).ToList();
                
            }
            else if (category.Trim().ToLower() == "missingfatherphoto")
            {
                data = db.Admissions.Where(w =>
                     w.IsExpell == false && classids.Contains(w.ClassSection.ClassID) && campid.Contains(w.CampuseID) &&
                     w.Student.FatherPhoto == null).Select(s => new
                     {
                         AdmissionID = s.Student.RegistrationNo,
                         s.Student.StudentName,
                         FName = s.Student.FName,
                         EntryDate = s.Student.DateForSubmission,
                         s.ClassSection.Class.ClassName,
                         s.Campus.CampusName
                     }).ToList();
            }
            else if (category.Trim().ToLower() == "studentphoto")
            {
                data = db.Admissions.Where(w =>
                     w.IsExpell == false && classids.Contains(w.ClassSection.ClassID) && campid.Contains(w.CampuseID) &&
                     w.Student.StudentPhotos.FirstOrDefault() == null).Select(s => new
                     {
                         AdmissionID = s.Student.RegistrationNo,
                         s.Student.StudentName,
                         FName = s.Student.FName,
                         EntryDate = s.Student.DateForSubmission,
                         s.ClassSection.Class.ClassName,
                         s.Campus.CampusName
                     }).ToList();
            }
            else if (category.Trim().ToLower() == "studentcnic")
            {
              data = db.Admissions.Where(w =>
                    w.IsExpell == false && classids.Contains(w.ClassSection.ClassID) && campid.Contains(w.CampuseID) &&
                    w.Student.StudentCNIC == null).Select(s => new
                    {
                        AdmissionID = s.Student.RegistrationNo,
                        s.Student.StudentName,
                        FName = s.Student.FName,
                        EntryDate = s.Student.DateForSubmission,
                        s.ClassSection.Class.ClassName,
                        s.Campus.CampusName
                    }).ToList();
            }
            else if(category.Trim().ToLower() == "studentmobile")
            {
                data = db.Admissions.Where(m => m.IsExpell == false && classids.Contains(m.ClassSection.ClassID) && campid.Contains(m.CampuseID) &&
                    m.Student.StudentMobiles.FirstOrDefault().MobileNo == null).Select(s => new
                    {
                        AdmissionID = s.Student.RegistrationNo,
                        s.Student.StudentName,
                        FName = s.Student.FName,
                        EntryDate = s.Student.DateForSubmission,
                        s.ClassSection.Class.ClassName,
                        s.Campus.CampusName
                    }).ToList();
            }
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

        
        public ActionResult ParentsList()
        {
            return View();
        }

        public JsonResult GetParentsList(DataManagerRequest dm, int campusId = -1, string regnos = "", string cbxAll = "")
        {
            //int[] CampusIds = Helpers.CurrentUser.GetCampusIds();
            var campusIds = campusId == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] { campusId };

            var regNo2 = UtilityFunctions.ParseRegNosAsItis(regnos);
            int[] regNos = regNo2.Item1;
            string method = regNo2.Item2;
            bool all = cbxAll == "true";
            IEnumerable<object> data = null;
            if (regNos.Count() > 0)
            {
                var query = db.Students.AsNoTracking().Where(w =>
                           regNos.Contains(w.RegistrationNo));
                if (!all)
                    query = query.Where(w => w.Admissions.Any(a => !a.IsExpell));

                data = query.DistinctBy(d => d.CNIC)
                      .ToList().Select(s => new
                      {
                          AdmissionID = string.Join(",",
                           db.Students.Where(ww => ww.CNIC == s.CNIC).Select(ss => ss.RegistrationNo).ToArray()),
                          StudentName = s.FName,
                          s.PermenantAddress,
                          s.FatherNatureOfJob,
                          s.CNIC,
                          ChildCount = db.Students.Where(ww => ww.CNIC == s.CNIC).Count(),
                          MobileNo =
                              s.StudentMobiles.Where(w => w.MobileHolder.Trim().ToLower() == "father")
                                  .Select(ss => ss.MobileNo).FirstOrDefault() == null
                                  ? s.StudentMobiles.Where(w => w.IsDefault).Select(ss => ss.MobileNo).FirstOrDefault()
                                  : s.StudentMobiles.Where(w => w.MobileHolder.Trim().ToLower() == "father")
                                      .Select(ss => ss.MobileNo).FirstOrDefault(),
                        //    action =
                        //        $"<a title=\"Credentials\" href=\"javascript:void()\" title=\"AddParent User\" onclick=\"showpop({s.ID})\" class=\"edit ajax ml-1\"><i class=\"fa fa-lock\"></i>"
                    });
            }
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

        
        public async Task<ActionResult> parentuser(int id)
        {
            ViewBag.studentId = id;
            var std = (await db.Students.FindAsync(id));
            if(std==null)
            {

            }
            var cnic = std.CNIC;
            var all = db.Students.Where(w => w.CNIC==cnic).Select(s=>s.ID);
            var checkexist = db.UserDefinitions.Where(w => all.Contains(w.FK) && w.UserType == "P").FirstOrDefault();
            string user = checkexist.UserId;
            return View();
        }

        
        //[HttpPost]
        //public string parentuser(RegisterViewModel model)
        //{
        //    if (Request.Form["StudentId"].FirstOrDefault() == null) return "The Data is Not Valid";
        //    var StudentId = int.Parse(Request.Form["StudentId"]);
        //    var checkexist = db.UserDefinitions.Where(w => w.FK == StudentId && w.UserType == "P").ToList();
        //    if (checkexist.Count() > 0) return "User already Exist";

        //    try
        //    {
        //        var user = new ApplicationUser
        //        { UserName = model.Email, Email = model.Email, PhoneNumber = "", EmailConfirmed = false };

        //        var result = UserManager.Create(user, model.Password);
        //        if (result.Errors.Count() > 0) return result.Errors.FirstOrDefault();

        //        try
        //        {
        //            UserManager.AddToRole(user.Id, "Parent");
        //        }
        //        catch
        //        {
        //            return "An error occured while trying to create the user account.";
        //        }

        //        if (result.Succeeded)
        //            try
        //            {
        //                var definition = new UserDefinition();
        //                definition.FK = StudentId;
        //                definition.UserId = user.Id;
        //                definition.UserType = "P";
        //                db.UserDefinitions.Add(definition);
        //                db.SaveChanges();
        //                return "success";
        //            }
        //            catch (Exception ex)
        //            {
        //                try
        //                {
        //                    UserManager.Delete(user);
        //                }
        //                catch
        //                {
        //                }

        //                return ex.Message;
        //            }

        //        ModelState.AddModelError("", result.Errors.FirstOrDefault());
        //    }
        //    catch (Exception e)
        //    {
        //        return e.Message;
        //    }

        //    return ModelState.Where(w => w.Value.Errors.Count > 0).FirstOrDefault().Value.Errors.FirstOrDefault()
        //        .ErrorMessage;
        //}

        
        public ActionResult SubWiseList()
        {
            return View();
        }

        public JsonResult GetSubWiseList(int clasId = -1, int campusID = -1, string attendane = "", string issuefee = "", string receivefee = "", string result = "")
        {
            string criteri = "", head = "";
            var clsID = clasId == -1 ? db.Classes.Select(s => s.ID).ToArray() : new[] {clasId};
            var campid = campusID == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] {campusID};
            if (attendane == "true")
            {
                head = "Attendance";
                criteri = " AND (ComSubs.Attendance = 1)";
            }

            if (result == "true")
            {
                head += head == "" ? "Result" : ", Result";
                criteri += " AND (ComSubs.Result = 1)";
            }

            if (issuefee == "true")
            {
                head += head == "" ? "Issued Fee" : ", Issued Fee";
                criteri += " AND (ComSubs.IssuedFee = 1)";
            }

            if (receivefee == "true")
            {
                head += head == "" ? "Received Fee" : ", Received Fee";
                criteri += " AND (ComSubs.ReceiveFee = 1)";
            }

            var data = new DataTable();
            var li = new Dictionary<string, object>();
            var q =
                @"SELECT        Students.RegistrationNo, Students.StudentName, Students.FName, Classes.ClassName, Campuses.CampusName, ComSubs.Attendance, ComSubs.Result, ComSubs.IssuedFee, ComSubs.ReceiveFee, 
                         Campuses.ID AS CampusID
FROM            ComSubs INNER JOIN
                         Students ON ComSubs.StudentID = Students.ID INNER JOIN
                         Admissions ON Students.ID = Admissions.StudentID INNER JOIN
                         ClassSections ON Admissions.ClassSectionID = ClassSections.ID INNER JOIN
                         Classes ON ClassSections.ClassID = Classes.ID INNER JOIN
                         Campuses ON Admissions.CampuseID = Campuses.ID
                         WHERE (Admissions.IsExpell = 0)  AND (Classes.ID IN (" + string.Join(",", clsID) +
                ")) AND (Campuses.ID IN (" + string.Join(",", campid) + @")) " + criteri +
                " ORDER BY Classes.ID, Students.RegistrationNo";
            data = dba.CreateTable(q);
            data.Columns.Add("CampuseID");
            li.Add("Heading", head + " SMS Subscription List");
            var attendacnechages = double.Parse(db.SMSApplicationProperties
                .Where(w => w.PropertyName == "AttendanceCharges").FirstOrDefault().PropertyValue.Trim());
            var resultCharges = double.Parse(db.SMSApplicationProperties.Where(w => w.PropertyName == "ResultCharges")
                .FirstOrDefault().PropertyValue.Trim());
            var FeeIssuedCharges = double.Parse(db.SMSApplicationProperties
                .Where(w => w.PropertyName == "FeeIssuedCharges").FirstOrDefault().PropertyValue.Trim());
            var FeeReceivedCharges = double.Parse(db.SMSApplicationProperties
                .Where(w => w.PropertyName == "FeeReceivedCharges").FirstOrDefault().PropertyValue.Trim());
            foreach (DataRow dr in data.Rows)
            {
                if (dr["Attendance"].ToString().Trim() == "1") dr["Attendance"] = attendacnechages;
                if (dr["Result"].ToString().Trim() == "1") dr["Result"] = resultCharges;
                if (dr["IssuedFee"].ToString().Trim() == "1") dr["IssuedFee"] = FeeIssuedCharges;
                if (dr["ReceiveFee"].ToString().Trim() == "1") dr["ReceiveFee"] = FeeReceivedCharges;
                dr["CampuseID"] = dr["CampusID"].ToString();
            }
            var sub = new JsonResult (data.ToDynamicList());
            return sub;
        }

        #endregion

        #region Certificate

        
        public ActionResult IDCard()
        {
            return View();
        }

        
        public ActionResult BirthCertificate()
        {
            return View();
        }

        public ActionResult LeavingCertificate(int id=0)
        {
            if (id>0)
            {
                ViewBag.studentId = id;
                ViewBag.RegNo = db.Students.Where(w => w.ID==id).Select(s => s.RegistrationNo).FirstOrDefault();
            }
            else
            {
                ViewBag.studentId =null;
            }
            return View();
        }

        public ActionResult CharacterCertificate()
        {
            return View();
        }

        public ActionResult StudentRegister()
        {
            return View();
        }

        #endregion
    }
}