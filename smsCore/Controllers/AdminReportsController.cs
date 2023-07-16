using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;

namespace smsCore.Controllers
{
    [Authorize]
    [CompressContent]
    public class AdminReportsController : BaseController
    {
       // SchoolEntities _context = new SchoolEntities();
        private readonly SchoolEntities _context;
         public AdminReportsController(SchoolEntities context)
        {
            _context= context;
        }

        // GET: AdminReports
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult DailyCashReport()
        {
            return View();
        }

        public JsonResult DailyCashReportData(DateTime dt,int campusId=-1)
        {
            if(campusId==-1)
            { 
            //campusId=
            }
            var fee = _context.FeeSlipDetails.Where(w => w.FeeSlip.Admission.CampuseID == campusId && w.FeeSlip.FeeSlipReceipts.Where(r => r.EntryDate== dt).Any()).GroupBy(g => new { g.FeeType.TypeName }).Select(s => new
            {
                s.Key,
                Amount = s.Sum(m => m.Amount)
            });
            return Json(new { count = fee.Count(), result = fee });
        }

        
        public ActionResult IncomeMap()
        {
            return View();
        }

        public ActionResult UnwantedClients()
        {
            return View();
        }
    }
}