using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Syncfusion.EJ2.Base;

namespace smsCore.Controllers
{
    public class EmployeeReportsController : BaseController
    {
        private readonly SchoolEntities db;
        public  EmployeeReportsController(SchoolEntities db)
        {
            this.db = db;
        }

        // GET: EmployeeReports
        public IActionResult Index()
        {
            return View();
        }

        //
        public IActionResult EmployeeCard()
        {
            return View();
        }

        public IActionResult ServiceCard()
        {
            return View();
        }

        public IActionResult StaffStatement()
        {
            return View();
        }

        public JsonResult GetStaffStaments(DataManagerRequest dm,int campusId, string empcode = "")
           
        {
            var campusIds = campusId == -1 ? db.Campuses.Select(t => t.ID).ToArray() : new[] {campusId};
            var staffData= db.tbl_Employee.AsNoTracking().Where(t => t.CampusID == campusId).ToList().Select(e =>
                new
                {
                    StaffID = e.employeeCode,
                    Name = e.employeeName,
                    FName = e.FatherName,
                    CNo = e.mobileNumber,
                    CNIC = e.CNIC,
                    PAddress = e.address,
                    Designation = e.tbl_Designation == null ? string.Empty : e.tbl_Designation.designationName
                });
            DataOperations operation = new DataOperations();

            if (dm.Search != null && dm.Search.Count > 0)
            {
                staffData = operation.PerformSearching(staffData, dm.Search);  //Search
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                staffData = operation.PerformFiltering(staffData, dm.Where, dm.Where[0].Operator);
            }

            if (dm.Sorted != null && dm.Sorted.Count > 0)
            {
                staffData = operation.PerformSorting(staffData, dm.Sorted);
            }

            if (dm.Skip != 0)
            {
                if (dm.Sorted.Count == 0)
                {
                    List<Sort> sort = new List<Sort>() { new Sort { Name = "employeeName", Direction = "ascending" } };
                    staffData = operation.PerformSorting(staffData, sort);
                }
                staffData = operation.PerformSkip(staffData, dm.Skip);
            }

            if (dm.Take != 0)
            {
                staffData = operation.PerformTake(staffData, dm.Take);
            }

            if (empcode != null && empcode != "") staffData = staffData.Where(w => w.StaffID == empcode).ToList();
            var state = Json(new { Data = staffData });
            return state;
           }

        public IActionResult ExperienceCertificate()
        {
            return View();
        }

        public IActionResult AllEmployeeList()
        {
            return View();
        }

        public JsonResult GetEmployeeInformatoin(DataManagerRequest dm,int campusId)
        {
            var campusIds = campusId == -1 ? db.Campuses.Select(t => t.ID).ToArray() : new[] { campusId };
            var empData = db.tbl_Employee.AsNoTracking().Where(w =>w.CampusID==campusId ).ToList().Select(s => new
            {
                s.Id,
                EmpCode = s.employeeCode,
                EmployeeName = s.employeeName,
                FName = s.FatherName,
                DOB = s.dob,
                MobileNo = s.mobileNumber,
                JoiningDate = s.joiningDate,
                Designation = s.tbl_Designation == null ? string.Empty : s.tbl_Designation.designationName,
                Qualififcation = s.qualification
            });
            
            
            DataOperations operation = new DataOperations();

            if (dm.Search != null && dm.Search.Count > 0)
            {
                empData = operation.PerformSearching(empData, dm.Search);  //Search
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                empData = operation.PerformFiltering(empData, dm.Where, dm.Where[0].Operator);
            }

            if (dm.Sorted != null && dm.Sorted.Count > 0)
            {
                empData = operation.PerformSorting(empData, dm.Sorted);
            }

            if (dm.Skip != 0)
            {
                if (dm.Sorted.Count == 0)
                {
                    List<Sort> sort = new List<Sort>() { new Sort { Name = "RegistrationNo", Direction = "ascending" } };
                    empData = operation.PerformSorting(empData, sort);
                }

                empData = operation.PerformSkip(empData, dm.Skip);
            }

            if (dm.Take != 0)
            {
                empData = operation.PerformTake(empData, dm.Take);
            }
            return Json(new { result = empData, count = empData.Count() });
        }

        public ActionResult NoObjectionCertificate()
        {
            return View();
        }

        public ActionResult EmployeePhoneList()
        {
            return View();
        }

        public JsonResult GetEmployeePhoneList(int campusId = -1)
        {
            var campusIds = campusId == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] {campusId};
            var phoneList = db.tbl_Employee.AsNoTracking().Where(w => campusIds.Contains(w.CampusID)).Select(
                s => new
                {
                    EmpCode = s.employeeCode,
                    Name = s.employeeName,
                    FName = s.FatherName,
                    MobileNo = s.mobileNumber,
                    Email = s.email,
                    PhoneNo = s.phoneNumber
                }).ToList();
            var list = Json(new {Data = phoneList});
            return list;
        }

        public ActionResult QualificationWiseList()
        {
            return View();
        }

        public JsonResult GetQualificationList(int campusId = -1, string empcode = "")
        {
            var sno = 1;
            var campusIds = campusId == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] {campusId};
            var qualList = db.tbl_Employee.AsNoTracking().Where(t => campusIds.Contains(t.CampusID)).ToList().Select(
                l => new
                {
                    SNo = sno++,
                    StaffID = l.employeeCode,
                    Name = l.employeeName,
                    FName = l.FatherName,
                    Qualification = l.qualification,
                    Grade = l.Qualifications.Select(s => s.Grade).DefaultIfEmpty("").FirstOrDefault(),
                    Year = l.Qualifications.Select(s => s.Year).DefaultIfEmpty().FirstOrDefault(),
                    Designation = l.tbl_Designation == null ? string.Empty : l.tbl_Designation.designationName
                });
            if (empcode != null && empcode != "") qualList = qualList.Where(w => w.StaffID == empcode).ToList();
            var qual = Json(new {Data = qualList});
            return qual;
        }

        public ActionResult DailySalaryRecord(int campusId = -1, string empcode = "")
        {
            var campId = campusId == -1 ? db.Campuses.Select(c => c.ID).ToArray() : new[] {campusId};
            var dsalary = db.tbl_Employee.AsNoTracking().Where(e => campId.Contains(e.CampusID)).ToList().Select(w =>
                new
                {
                });
            return View();
        }

        public ActionResult MonthlySalaryRecord()
        {
            return View();
        }

        public ActionResult PayHeadReport()
        {
            return View();
        }

        public ActionResult SalaryPackage()
        {
            return View();
        }

        public ActionResult AdvancePaymentReport()
        {
            return View();
        }
    }
}