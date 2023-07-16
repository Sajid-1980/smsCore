using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using smsCore.Data;
using smsCore.Data.Helpers;

namespace smsCore.Controllers
{
    [Authorize]
    public class ParentsController : Controller
    {
        private readonly SchoolEntities db; 
        private readonly CurrentUser _user;
        private readonly FeeSystemHelper _fl;



        public ParentsController(SchoolEntities _db  , CurrentUser user, FeeSystemHelper fl)
        {
            db = _db;
            _user = user;
            _fl = fl;
        }

       
        // GET: Parents
        public IActionResult Index(int id)
        {
            var CampusIds = _user.GetCampusIds();

            var student = db.Students.Where(w => w.ID == id).FirstOrDefault();

           // var fl = new FeeSystemHelper();

           //  FeeSystemHelper fl = new FeeSystemHelper();

            var fathercnic = db.Students.Where(w => w.ID == id).Select(s => s.CNIC).FirstOrDefault();
            var admissionId = db.Admissions.Where(w => w.Student.CNIC == fathercnic).Select(s => s.ID).ToArray();
            ViewBag.feeList = _fl.GetFeeOfStudent(2019, admissionId, CampusIds);
            return View(student);
        }
    }
}