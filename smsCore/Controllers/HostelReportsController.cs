using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using smsCore.Data.Helpers;

namespace smsCore.Controllers
{
    public class HostelReportsController : BaseController
    {
        private readonly SchoolEntities db;
        
        // GET: HostelReport
        public IActionResult Index()
        {
            return View();
        }

        // 
        public IActionResult AdmissionRecord
        {
            get
            {
                ViewBag.hstlename = new SelectList(db.Hostels.Select(s => new { s.Id, s.HostelName }), "Id", "HostelName");
                return View();
            }
        }

        public JsonResult GetAdmissionRecordList(int regno = -1, int hostel = -1)
        {
            var hosteLId = hostel == -1 ? db.Hostels.Select(s => s.Id).ToArray() : new[] {hostel};
            var list = db.HostelAdmissions.AsNoTracking().Where(w => hosteLId.Contains(w.HostelId)).ToList().Select(s =>
                new
                {
                    AdmissionDate = s.AdmissionDate.ToString(),
                    s.Admission.Student.StudentName,
                    AdmissionID = s.Admission.Student.RegistrationNo,
                    s.Admission.Student.FName,
                    s.Admission.ClassSection.Class.ClassName
                }).ToList();
            if (regno != -1) list = list.Where(w => w.AdmissionID == regno).ToList();
           // var hostelAdm = new JsonResult {Data = list, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
           // return hostelAdm;

            var hosteladm = Json(new
            {
                Data = list
            });
            return hosteladm;
        }

        public IActionResult RoomRecord()
        {
            ViewBag.hstlename = new SelectList(db.Hostels.Select(s => new {s.Id, s.HostelName}), "Id", "HostelName");
            return View();
        }

        public JsonResult GetRoomsRecordList(int hostel = -1)
        {
            var hostelID = hostel == -1 ? db.Hostels.Select(d => d.Id).ToArray() : new[] {hostel};
            var roomList = db.Rooms.AsNoTracking().Where(w => hostelID.Contains(w.HostelId)).Select(n => new
            {
                Roomno = n.RoomNo,
                n.HostelId,
                n.Hostel.HostelName,
                NoOfStudent = n.HostelAdmissions.Where(w => w.Isexpel == false).Count(),
                n.Floor,
                n.Capacity
            }).ToList();
            if (hostel != -1) roomList = roomList.Where(w => w.HostelId == hostel).ToList();
           // var room = new JsonResult {Data = roomList, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
           // return room;
            var room = Json(new
            {
                Data = roomList
            });
            return room;
        }

        public ActionResult VistorRecord()
        {
            ViewBag.hstlename = new SelectList(db.Hostels.Select(s => new {s.Id, s.HostelName}), "Id", "HostelName");
            return View();
        }

        public JsonResult GetVisitorsRecordList(string fdate="", int hostel = -1)
        {
            var hostelID = hostel == -1 ? db.Hostels.Select(d => d.Id).ToArray() : new[] {hostel};
            var fd = DateTimeHelper.ConvertDate(fdate);
            var visi = db.Visitors.AsNoTracking().Where(w => w.Date == fd)
                .Select(m => new
                {
                    vName = m.Name,
                    relationship = m.Relationship,
                    //student = m.HostelAdmission.Admission.Student.StudentName,
                    //mobile = m.HostelAdmission.Admission.Student.StudentMobiles.Select(s => s.MobileNo).FirstOrDefault(),
                    cnic = m.Cnic,
                    ondate = m.Date,
                    //timeIn = m.TimeIn.ToString(),
                    //timeOut = m.TimeOut.ToString()
                }).ToList();
           // var data = new JsonResult {Data = visi, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
           // return data;
            var data = Json(new
            {
                Data = visi
            });
            return data;
        }

        public ActionResult HostelAttendanceReport()
        {
            ViewBag.hstlename = new SelectList(db.Hostels.Select(n => new {n.Id, n.HostelName}), "Id", "HostelName");
            return View();
        }

        public JsonResult GetHostelAttendanceRecordList(int regno = -1, int hostel = -1, string month = "")
        {
            if (string.IsNullOrEmpty(month)) month = DateTime.Now.ToString("MMMM");
            var date = $"01-{month}-{DateTime.Now.Year}";
            var firstDate = DateTime.ParseExact(date, "dd-MMMM-yyyy", CultureInfo.InvariantCulture);
            var Lastdate = DateTime.Now.AddMonths(1);
            var hostelId = hostel == -1 ? db.Hostels.Select(e => e.Id).ToArray() : new[] {hostel};
            var attend = db.HostelAdmissions.AsNoTracking().Where(v =>
                    v.Isexpel == false && v.Admission.Student.RegistrationNo == regno && hostelId.Contains(v.HostelId))
                .ToList().Select(h => new
                {
                    RegNo = h.Admission.Student.RegistrationNo,
                    Name = h.Admission.Student.StudentName,
                    h.Admission.Student.FName,
                    present = h.HostelAttendances.Where(w =>
                        w.Date >= firstDate && w.Date <= Lastdate &&
                        w.StudentAttendanceType.AttendanceName.Trim() == "Present").Count(),
                    absent = h.HostelAttendances.Where(w =>
                        w.Date >= firstDate && w.Date <= Lastdate &&
                        w.StudentAttendanceType.AttendanceName.Trim() == "Absent").Count()
                });
          //  return new JsonResult {Data = attend, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
            return Json(new
            {
                Data = attend
            });
           
        }
    }
}