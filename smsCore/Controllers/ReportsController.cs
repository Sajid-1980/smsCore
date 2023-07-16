using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using smsCore.Data;
using smsCore.Data.Helpers;
using smsCore.Data.Models.ViewModels;
using Utilities;

namespace smsCore.Controllers
{
    [Authorize]
    [CompressContent]
    public class ReportsController : BaseController
    {
        // GET: AdminReports
        private readonly SchoolEntities db;
        private readonly IHttpContextAccessor _context;
        private readonly ReportData _reporetry;
        private readonly CurrentUser _user;
        private readonly ClsBussinessSetting  _clsBussinessSetting;
 
        private int[] _campusid;
        public ReportsController(SchoolEntities _db,IHttpContextAccessor context, ReportData reporetry, CurrentUser user   )
        {
            db = _db;
            _context = context;
            _reporetry = reporetry;
            _campusid = user.GetCampusIds();
           
        }
        public IActionResult Index()
        {
            return View();
        }

        //Admin Reports
        [HttpGet]
        public IActionResult AdminReports()
        {
            return View();
        }

        [HttpGet]
        public IActionResult FeeReports()
        {
            return View();
        }

        [HttpGet]
        public IActionResult StudentReports()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ExamReports()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AttendanceReports()
        {
            return View();
        }

        [HttpGet]
        public IActionResult PayrollReports()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AccountsReport()
        {
            return View();
        }

        [HttpGet]
        public IActionResult HostelReports()
        {
            return View();
        }

        [HttpGet]
        public IActionResult EmployeeReports()
        {
            return View();
        }

        //[HttpGet]
        //public ActionResult ReportDesigner()
        //{
        //    return View();
        //}
        [HttpGet]
        public IActionResult ReportDesigner(string reportname)
        {
            return View("ReportDesigner", reportname);
        }
        
        public IActionResult Viewer()
        {
            var report = string.Empty;
            var reportPath = string.Empty;
            object data = null;
            var ReportTitle = string.Empty;
            var remark = "";
            var bankdetail = "";
            var schoolcopys = "";
            var studentcopys = "";
            var bankcopys = "";

            string total = "";
            string pass = "";
            string fail = "";
            string absent = "";
            string percent = "";
            var school = db.Company.Select(s => new { CompanyName = s.companyName, Address = s.address, Phone = s.phone, Logo = s.logo }).FirstOrDefault();
            byte[] reportlogo = null;
            var Signature = db.SchoolLogoes.Where(w => w.ID == 3).FirstOrDefault();

            Dictionary<string, object> _params = new Dictionary<string, object>();

            if (_context.HttpContext.Request.Query["report"].FirstOrDefault() != null) 
                report = HttpContext.Request.Query["report"];




            if (report.ToLower() == "feeslip".ToLower())
            {
                schoolcopys = Convert.ToBoolean(HttpContext.Request.Query["schoolcopy"]).ToString();
                studentcopys = Convert.ToBoolean(HttpContext.Request.Query["studentcopy"]).ToString();
                bankcopys = Convert.ToBoolean(HttpContext.Request.Query["bankcopy"]).ToString();
                string showAdvance = Convert.ToBoolean(HttpContext.Request.Query["showadvance"]).ToString();
                reportPath = (HttpContext.Request.Query["template"]);

                remark = HttpContext.Request.Query["remark"];
                if (remark == null || remark == "") remark = ".";
                //reportPath = "feeslip";
                //if (orientation == "portrait")
                //    reportPath = "feeslipPortrait";

                var SelectedDate = DateTimeHelper.ConvertDate(HttpContext.Request.Query["formonth"]);
                int.TryParse(HttpContext.Request.Query["classid"], out var ClassId);
                int.TryParse(HttpContext.Request.Query["CampusId"], out var campusId);
                var regno = HttpContext.Request.Query["regno"];
                _clsBussinessSetting.CampusId = campusId;
                

                bankdetail = _clsBussinessSetting.ReadWithType("bankdetails", typeof(string)).ToString();
                //bankdetail=string.IsNullOrEmpty(bankdetail) ? "." : bankdetail;
                var reportdata = _reporetry;

                data = reportdata.Printfeeslip(SelectedDate, ClassId.ToString(), regno, campusId, showAdvance.ToLower() == "true");
            }
            ViewData["DataSource"] = data;
            var model = new BoldReportViewModel { ReportName = reportPath };
            model.Params.Add("SchoolName", school.CompanyName);
            model.Params.Add("schoolCopy", schoolcopys.ToLower());
            model.Params.Add("BankCopy", bankcopys.ToLower());
            model.Params.Add("studentCopy", studentcopys.ToLower());
            model.Params.Add("remarks", string.IsNullOrEmpty(remark) ? "." : remark);
            model.Params.Add("Contact", string.IsNullOrEmpty(school.Phone) ? "." : school.Phone);
            model.Params.Add("Address", string.IsNullOrEmpty(school.Address) ? "." : school.Address);
            model.Params.Add("BankDetail", string.IsNullOrEmpty(bankdetail) ? "." : bankdetail);
            var imagelogo = Convert.ToBase64String(school.Logo);
            model.Params.Add("ReportLogo", imagelogo);
            //model.Params.Add();

            return View(model);
        }
    }
}