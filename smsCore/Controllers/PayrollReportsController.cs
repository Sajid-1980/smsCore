using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models;

namespace smsCore.Controllers
{
    public class PayrollReportsController : BaseController
    {
        private readonly SchoolEntities db ;
        public PayrollReportsController(SchoolEntities db)
        {
            this.db = db;
        }
        

        // GET: PayrollReports
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Employee()
        {
            var designationlist = db.tbl_Designation.Select(s => new {s.Id, s.designationName}).ToList();
            designationlist.Insert(0, new {Id = 0, designationName = "All"});
            ViewBag.designationList = new SelectList(designationlist, "Id", "designationName");
            var employee = db.tbl_Employee.Select(s => new {s.Id, s.employeeCode}).ToList();
            employee.Insert(0, new {Id = 0, employeeCode = "All"});
            ViewBag.employeelist = new SelectList(employee, "Id", "employeeCode");
            return View();
        }

        public string GetEmployeeDatatable(string Id, string disignationId, string status)
        {
            //if (Id == "0") Id = "All";
            //if (disignationId == "0") disignationId = "All";
            //var spEmployee = new EmployeeSP();
            //var data = spEmployee.EmployeeViewAllEmployeeReport(Id, disignationId, status);
            //var jsonString = JsonConvert.SerializeObject(data);

            //return jsonString;
            return string.Empty;
        }

        public ActionResult Payhead()
        {
            //var spPayHead = new PayHeadSP();
            //decimal ii = -1;
            //var dtblPayHead = spPayHead.PayHeadViewAll();
            //var dr = dtblPayHead.NewRow();
            //dr[2] = "All";
            //dr[1] = ii;
            //dtblPayHead.Rows.InsertAt(dr, 0);

            //var data = dtblPayHead.AsEnumerable().Select(s => new
            //{
            //    PayHeadId = (decimal) s["PayHeadId"],
            //    PayHeadName = s["PayHeadName"].ToString()
            //});
            //ViewBag.payheadlist = new SelectList(data, "PayHeadId", "PayHeadName");
            return View();
        }

        public string GetPayheadDatatable(string payheadId, string type)
        {
            //if (payheadId == "-1") payheadId = "All";
            //var payHeadSP = new PayHeadSP();
            //var data = payHeadSP.PayHeadViewAllForPayHeadReport(payheadId, type);
            //var list = data.AsEnumerable().Select(x => new
            //{
            //    SlNo = x["SlNo"],
            //    payHeadName = (string) (x["payHeadName"] == null ? "" : x["payHeadName"]),
            //    type = (string) (x["type"] == null ? "" : x["type"])
            //});

            //var jsonString = JsonConvert.SerializeObject(list.OrderBy(s => s.SlNo));

            //return jsonString;
            return string.Empty;
        }

        public IActionResult SalaryPackage()
        {
        //    var spPayHead = new SalaryPackageSP();
        //    decimal ii = -1;
        //    var dtblPayHead = spPayHead.SalaryPackageViewAll();
        //    var dr = dtblPayHead.NewRow();
        //    dr[1] = "All";
        //    dr[0] = ii;
        //    dtblPayHead.Rows.InsertAt(dr, 0);
        //    var data = dtblPayHead.AsEnumerable().Select(s => new
        //    {
        //        SalaryPackageId = (decimal) s["SalaryPackageId"],
        //        SalaryPackageName = s["SalaryPackageName"].ToString()
        //    });
        //    ViewBag.packagelist = new SelectList(data, "SalaryPackageName", "SalaryPackageName");
            return View();
        }

        public string GetSalaryPackageDatatable(string packageId, string status)
        {
            //var payHeadSP = new SalaryPackageSP();
            //var data = payHeadSP.SalaryPackageViewAllForSalaryPackageReport(packageId, status);
            //var jsonString = JsonConvert.SerializeObject(data);
            //return jsonString;
            return string.Empty;
        }

        public IActionResult BonusDeduction()
        {
            //var spPayHead = new SalaryPackageSP();

            //decimal ii = -1;
            //var dtblPayHead = spPayHead.SalaryPackageViewAll();
            //var dr = dtblPayHead.NewRow();
            //dr[2] = "All";
            //dr[1] = ii;
            //dtblPayHead.Rows.InsertAt(dr, 0);

            //var data = dtblPayHead.AsEnumerable().Select(s => new
            //{
            //    PayHeadId = (decimal) s["SalaryPackageId"],
            //    PayHeadName = s["SalaryPackageName"].ToString()
            //});
            //ViewBag.packagelist = new SelectList(data, "SalaryPackageId", "SalaryPackageName");
            return View();
        }

        public string GetBonusDeductionDatatable(string packageId, string status)
        {
            //if (packageId == "-1") packageId = "All";
            //var payHeadSP = new SalaryPackageSP();
            //var data = payHeadSP.SalaryPackageViewAllForSalaryPackageReport(packageId, status);
            //var jsonString = JsonConvert.SerializeObject(data);

            //return jsonString;
            return string.Empty;
        }
    }
}