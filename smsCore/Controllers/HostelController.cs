using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using Newtonsoft.Json;
using smsCore.Data;
using smsCore.Data.Helpers;
using Syncfusion.EJ2.Base;

namespace smsCore.Controllers
{
    [Authorize]
    public class HostelController : BaseController
    {
        // GET: Hostel
        private readonly SchoolEntities db;
        private readonly CurrentUser _user;

        public HostelController(SchoolEntities _db, CurrentUser user)
        {
            db = _db;
            _user = user;
        }

        #region Hostel

        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public string Create(Hostel hostel)
        {
            try
            {
                hostel.CampusId = _user.SelectedCampusId;
                db.Hostels.Add(hostel);
                db.SaveChanges();
                return "success";
            }
            catch (Exception ex)
            {
                ModelState.Clear();
                return ex.Message;
                //return RedirectToAction("Create");
            }
        }


        public IActionResult Edit(int id)
        {
            //HostelEntities db = new HostelEntities();
            var getdata = db.Hostels.Where(w => w.Id == id).FirstOrDefault();
            return View(getdata);
        }


        [HttpPost]
        public string Edit(Hostel hostel)
        {
            var hostelToUpdate = db.Hostels.Find(hostel.Id);
            if (hostelToUpdate != null)
            {
                hostelToUpdate.HostelName = hostel.HostelName;
                hostelToUpdate.TotalRooms = hostel.TotalRooms;
                hostelToUpdate.TotalStudents = hostel.TotalStudents;
                hostelToUpdate.Location = hostel.Location;
                hostelToUpdate.CampusId = hostel.CampusId;
            }
            else
            {
                return "failed";
            }

            try
            {
                db.SaveChanges();

                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        [HttpGet]
        public IActionResult Delete(int id)
        {
            var getdata = db.Hostels.Where(w => w.Id == id).FirstOrDefault();
            return View(getdata);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public string Delete(Hostel hostel)
        {
            var message = string.Empty;
            try
            {
                hostel = db.Hostels.Find(hostel.Id);
                db.Hostels.Remove(hostel);
                db.SaveChanges();
                message = "delete";
            }
            catch (Exception aa)
            {
                message = aa.Message;
                //ModelState.AddModelError("", aa.Message);
            }

            return message;
        }


        [HttpPost]
        public ActionResult GethostelList(DataManagerRequest dm)
        {

            var hostellist = db.Hostels.Select(s => new
            {
                s.Id,
                s.HostelName,
                s.Location,
                s.TotalRooms,
                s.TotalStudents,
                s.Campus.CampusName,
            });
            DataOperations operation = new DataOperations();
            if (dm.Search != null && dm.Search.Count > 0)
            {
                hostellist = operation.PerformSearching(hostellist, dm.Search);  //Search
            }
            if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
            {
                hostellist = operation.PerformSorting(hostellist, dm.Sorted);
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                hostellist = operation.PerformFiltering(hostellist, dm.Where, dm.Where[0].Operator);
            }
            int count = hostellist.Count();
            if (dm.Skip != 0)
            {
                hostellist = operation.PerformSkip(hostellist, dm.Skip);   //Paging
            }
            if (dm.Take != 0)
            {
                hostellist = operation.PerformTake(hostellist, dm.Take);
            }
            if (dm.RequiresCounts)
            {
                return Json(new { result = hostellist, count = hostellist.Count() });
            }
            else
            {
                return Json(new { result = hostellist, count = hostellist.Count() });
            }
        }

        #endregion
        #region Hostel Room

        public IActionResult HostelRoom()
        {
            ViewBag.hstlename = new SelectList(db.Hostels, "Id", "HostelName");
            return View();
        }

        [HttpPost]
        public string HostelRoom(Room room)
        {
            try
            {
                db.Rooms.Add(room);
                db.SaveChanges();
                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public IActionResult GetRoomList(DataManagerRequest dm)
        {
            var roomList = db.Rooms.Select(d => new
            {
                d.Id,
                d.Hostel.HostelName,
                d.RoomNo,
                d.Floor,
                d.Capacity
            });
            DataOperations operation = new DataOperations();
            if (dm.Search != null && dm.Search.Count > 0)
            {
                roomList = operation.PerformSearching(roomList, dm.Search);  //Search
            }
            if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
            {
                roomList = operation.PerformSorting(roomList, dm.Sorted);
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                roomList = operation.PerformFiltering(roomList, dm.Where, dm.Where[0].Operator);
            }
            int count = roomList.Count();
            if (dm.Skip != 0)
            {
                roomList = operation.PerformSkip(roomList, dm.Skip);   //Paging
            }
            if (dm.Take != 0)
            {
                roomList = operation.PerformTake(roomList, dm.Take);
            }
            if (dm.RequiresCounts)
            {
                return Json(new { result = roomList, count = roomList.Count() });
            }
            else
            {
                return Json(new { result = roomList, count = roomList.Count() });
            }
        }


        public IActionResult EditRooms(int id)
        {
            var getdata = db.Rooms.Where(w => w.Id == id).FirstOrDefault();
            ViewBag.hstlename = new SelectList(db.Hostels, "Id", "HostelName", getdata.HostelId);
            return View(getdata);
        }
        [HttpPost]
        public string EditRooms(Room room)
        {
            var roomToUpdate = db.Rooms.Find(room.Id);
            if (roomToUpdate != null)
            {
                roomToUpdate.RoomNo = room.RoomNo;
                roomToUpdate.Floor = room.Floor;
                roomToUpdate.Capacity = room.Capacity;
                roomToUpdate.HostelId = room.HostelId;
            }
            else
            {
                return "failed";
            }

            try
            {
                db.SaveChanges();
                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        public IActionResult DeleteRooms(int id)
        {
            var getdata = db.Rooms.Where(w => w.Id == id).FirstOrDefault();
            return View(getdata);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public string DeleteRooms(Room room)
        {
            var message = string.Empty;
            try
            {
                room = db.Rooms.Find(room.Id);
                db.Rooms.Remove(room);
                db.SaveChanges();
                message = "delete";
            }
            catch (Exception aa)
            {
                message = aa.Message;
                //ModelState.AddModelError("", aa.Message);
            }

            return message;
        }

        #endregion

        #region Hostel Admission
        public IActionResult HostelAdmission()
        {
            var campusId = _user.GetCampusIds();
            ViewBag.hostelname = new SelectList(db.Hostels.Select(s => new { s.Id, s.HostelName }), "Id", "HostelName");
            ViewBag.roomno = new SelectList(db.Rooms, "Id", "RoomNo");
            return View();
        }


        [HttpPost]
        public string HostelAdmissionCreate(HostelAdmission hadmission)
        {
            if (!int.TryParse(Request.Form["regNo"], out var regno)) return "Invalid reg No";
            var admissionid = db.Admissions.Where(w => w.Student.RegistrationNo == regno && !w.IsExpell)
                .Select(s => s.ID).FirstOrDefault();
            if (admissionid <= 0) return "regNo is Invalid";
            var hosteladmission = db.HostelAdmissions.Where(w => !w.Isexpel && w.AdmisionId == admissionid).ToList();
            if (hosteladmission.Count() > 0) return "Registration Number is Already Exist";
            try
            {
                var hostelAdm = new HostelAdmission()
                {
                    AdmisionId = admissionid,
                    AdmissionDate = DateTimeHelper.ConvertDate(Request.Form["AdmissionDate"].ToString(), true),
                    Fee = hadmission.Fee,
                    HostelId = hadmission.HostelId,
                    Isexpel = false,
                    RoomId = hadmission.RoomId
                };
                db.HostelAdmissions.Add(hostelAdm);
                db.SaveChanges();
                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        [HttpGet]
        public IActionResult HostelAdmissionEdit(int id)
        {
            ViewBag.hostelname = new SelectList(db.Hostels, "Id", "HostelName");
            ViewBag.roomno = new SelectList(db.Rooms, "Id", "RoomNo");
            var getdata = db.HostelAdmissions.Where(w => w.Id == id).FirstOrDefault();
            ViewBag.studentid = db.HostelAdmissions.Where(w => w.AdmisionId == getdata.AdmisionId)
                .Select(s => s.Admission.Student.RegistrationNo).FirstOrDefault();
            return View(getdata);
        }


        [HttpPost]
        public string HostelAdmissionEdit(HostelAdmission hadmission)
        {
            var id = int.Parse(Request.Form["StudentId"].ToString());
            hadmission.Isexpel = false;
            var hadmissionToUpdate = db.HostelAdmissions.Where(w => w.AdmisionId == hadmission.AdmisionId && !w.Isexpel)
                .FirstOrDefault();
            hadmissionToUpdate.AdmissionDate = DateTimeHelper.ConvertDate(Request.Form["AdmissionDate"].ToString(), true);

            hadmissionToUpdate.Fee = hadmission.Fee;
            hadmissionToUpdate.AdmisionId = hadmission.AdmisionId;
            hadmissionToUpdate.Isexpel = false;

            try
            {
                db.SaveChanges();

                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        public IActionResult HostelAdmissionDelete(int id)
        {
            var getdata = db.HostelAdmissions.Where(w => w.Id == id).FirstOrDefault();
            return View(getdata);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public string HostelAdmissionDelete(HostelAdmission hadmission)
        {
            var message = string.Empty;
            try
            {
                hadmission = db.HostelAdmissions.Find(hadmission.Id);
                db.HostelAdmissions.Remove(hadmission);
                db.SaveChanges();
                message = "delete";
            }
            catch (Exception aa)
            {
                message = aa.Message;
            }

            return message;
        }


        public IActionResult GetHostelAdmissionList(DataManagerRequest dm)
        {
            var hadmissionList = db.HostelAdmissions.Select(s => new
            {
                s.Id,
                RegNo = s.Admission.Student.RegistrationNo,
                s.Room.RoomNo,
                s.Admission.Student.StudentName,
                s.Fee,
                AdmissionDate = s.AdmissionDate.ToString()
            });

            DataOperations operation = new DataOperations();
            if (dm.Search != null && dm.Search.Count > 0)
            {
                hadmissionList = operation.PerformSearching(hadmissionList, dm.Search);  //Search
            }
            if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
            {
                hadmissionList = operation.PerformSorting(hadmissionList, dm.Sorted);
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                hadmissionList = operation.PerformFiltering(hadmissionList, dm.Where, dm.Where[0].Operator);
            }
            int count = hadmissionList.Count();
            if (dm.Skip != 0)
            {
                hadmissionList = operation.PerformSkip(hadmissionList, dm.Skip);   //Paging
            }
            if (dm.Take != 0)
            {
                hadmissionList = operation.PerformTake(hadmissionList, dm.Take);
            }
            if (dm.RequiresCounts)
            {
                return Json(new { result = hadmissionList, count = hadmissionList.Count() });
            }
            else
            {
                return Json(new { result = hadmissionList, count = hadmissionList.Count() });
            }
        }
        #endregion

        #region Hosetl Attendance
        public IActionResult HostelAttendance()

        {
            ViewBag.hstlename = new SelectList(db.Hostels, "Id", "HostelName");
            ViewBag.AttendanceType = new SelectList(db.StudentAttendanceTypes, "ID", "AttendanceName");
            ViewBag.Student = new SelectList(db.Students, "Id", "StudentName");
            //ViewBag.HostelAdmission = new SelectList(db.HostelAdmissions,"Id");
            var attendance = db.HostelAttendances.ToList();
            return View();
        }

        [HttpPost]
        public string HostelAttendanceCreate(HostelAttendance attendance)
        {
            if (!int.TryParse(Request.Form["regNo"], out var regno)) return "invalid regno";
            var hosteladsionid = db.HostelAdmissions.Where(w => w.Admission.Student.RegistrationNo == regno)
                .Select(s => s.Id).FirstOrDefault();
            if (hosteladsionid <= 0) return "Registration Not Found";
            var date = DateTimeHelper.ConvertDate(Request.Form["Date"].ToString());
            if (date == DateTime.MinValue)
                return "Please select an attendance date.";
            var exist = db.HostelAttendances.Any(w => w.HostleAdmId == hosteladsionid & w.Date.Date == date);
            if (exist)
            {
                return "Attendance already added for selected student.";
            }
            attendance.HostleAdmId = hosteladsionid;
            try
            {
                attendance.Date = date;
                db.HostelAttendances.Add(attendance);

                db.SaveChanges();
                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        [HttpGet]
        public IActionResult HostelAttendanceEdit(int id)
        {
            var getdata = db.HostelAttendances.Where(w => w.Id == id).FirstOrDefault();
            ViewBag.AttendanceType =
                new SelectList(db.StudentAttendanceTypes, "ID", "AttendanceName", getdata.AttendanceTypeId);
            return View(getdata);
        }


        [HttpPost]
        public string HostelAttendanceEdit(HostelAttendance attendance)
        {
            var attendanceToUpdate = db.HostelAttendances.Find(attendance.Id);
            if (attendanceToUpdate == null) return "No attendance record found.";
            attendanceToUpdate.Date = attendance.Date;
            attendanceToUpdate.AttendanceTypeId = attendance.AttendanceTypeId;
            try
            {
                db.SaveChanges();

                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        [HttpGet]
        public IActionResult HostelAttendanceDelete(int id)
        {
            //ViewBag.AttendanceType = new SelectList(db.AttendanceTypes, "TypeId", "TypeName");
            //ViewBag.Student = new SelectList(db.Students, "Id", "StudentName");
            var getdata = db.HostelAttendances.Where(w => w.Id == id).FirstOrDefault();
            return View(getdata);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public string HostelAttendanceDelete(HostelAttendance attendance)
        {
            var message = string.Empty;
            try
            {
                attendance = db.HostelAttendances.Find(attendance.Id);
                db.HostelAttendances.Remove(attendance);
                db.SaveChanges();
                message = "delete";
            }
            catch (Exception aa)
            {
                message = aa.Message;
            }

            return message;
        }

        public IActionResult GetAttendanceList(DataManagerRequest dm)
        {
            var attendanceList = db.HostelAttendances.Select(n => new
            {
                n.Id,
                Hostel = n.Hostel.HostelName,
                n.HostelAdmission.Admission.Student.StudentName,
                RegNo = n.HostelAdmission.Admission.Student.RegistrationNo,
                n.StudentAttendanceType.AttendanceName,
                Date = n.Date.ToString(),
            });

            DataOperations operation = new DataOperations();
            if (dm.Search != null && dm.Search.Count > 0)
            {
                attendanceList = operation.PerformSearching(attendanceList, dm.Search);  //Search
            }
            if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
            {
                attendanceList = operation.PerformSorting(attendanceList, dm.Sorted);
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                attendanceList = operation.PerformFiltering(attendanceList, dm.Where, dm.Where[0].Operator);
            }
            int count = attendanceList.Count();
            if (dm.Skip != 0)
            {
                attendanceList = operation.PerformSkip(attendanceList, dm.Skip);   //Paging
            }
            if (dm.Take != 0)
            {
                attendanceList = operation.PerformTake(attendanceList, dm.Take);
            }
            if (dm.RequiresCounts)
            {
                return Json(new { result = attendanceList, count = attendanceList.Count() });
            }
            else
            {
                return Json(new { result = attendanceList, count = attendanceList.Count() });
            }
        }
        #endregion

        #region Hostel Visitor

        public ActionResult HostelVisitor()
        {
            ViewBag.hstlename = new SelectList(db.Hostels, "Id", "HostelName");
            return View();
        }


        [HttpPost]
        public string VisitorCreate(Visitor visitor)
        {
            if (!int.TryParse(Request.Form["regNo"], out var regno)) return "Invalid registration No";
            //var data = db.Visitors.Select(n => new
            //{
            //    n.Name=visitor.Name,


            //});
            Visitor obj = new Visitor();
            obj.Name = visitor.Name;
            obj.Relationship = visitor.Relationship;
            obj.Cnic = visitor.Cnic;
            obj.CampusId = visitor.CampusId;
            obj.Date = visitor.Date;


            //visitor.StudentRegNo = regno;
            try
            {
                db.Visitors.Add(obj);
                db.SaveChanges();
                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<IActionResult> GetvisiorListForJq(DataManagerRequest dm)
        {
            var attendanceList = db.Visitors.Select(n => new
            {
                n.Id,
                n.Name,
                n.Relationship,
                n.StudentRegNo,
                n.Cnic,               
                Data = n.Date,


            }); ;

            DataOperations operation = new DataOperations();
            if (dm.Search != null && dm.Search.Count > 0)
            {
                attendanceList = operation.PerformSearching(attendanceList, dm.Search);  //Search
            }
            if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
            {
                attendanceList = operation.PerformSorting(attendanceList, dm.Sorted);
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                attendanceList = operation.PerformFiltering(attendanceList, dm.Where, dm.Where[0].Operator);
            }
            int count = await attendanceList.CountAsync();

            if (dm.Skip != 0)
            {
                attendanceList = operation.PerformSkip(attendanceList, dm.Skip);   //Paging
            }
            if (dm.Take != 0)
            {
                attendanceList = operation.PerformTake(attendanceList, dm.Take);
            }

            return Json(new { count, result = attendanceList });
        }



        public IActionResult VisitorEdit(int id)
        {
            ViewBag.hstlename = new SelectList(db.Hostels, "Id", "HostelName");
            var getdata = db.Visitors.Where(w => w.Id == id).FirstOrDefault();
            ViewBag.regno = getdata.StudentRegNo;
            return View(getdata);
        }


        [HttpPost]
        public string VisitorEdit(Visitor visitor)
        {
            var visitorToUpdate = db.Visitors.Find(visitor.Id);
            if (visitorToUpdate != null)
            {
                visitorToUpdate.Name = visitor.Name;
                visitorToUpdate.Relationship = visitor.Relationship;
                visitorToUpdate.Cnic = visitor.Cnic;
                visitorToUpdate.Date = visitor.Date;
            }
            else
            {
                return "failed";
            }

            try
            {
                db.SaveChanges();

                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        public IActionResult VisitorDelete(int id)
        {
            var getdata = db.Visitors.Where(w => w.Id == id).FirstOrDefault();
            return View(getdata);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public string VisitorDelete(Visitor visitor)
        {
            var message = string.Empty;
            try
            {
                visitor = db.Visitors.Find(visitor.Id);
                db.Visitors.Remove(visitor);
                db.SaveChanges();
                message = "delete";
            }
            catch (Exception aa)
            {
                message = aa.Message;
                //ModelState.AddModelError("", aa.Message);
            }

            return message;
        }

        #endregion
        public string Gethostel(int campusId)
        {
            var campus = campusId == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] { campusId };
            var hostel = db.Hostels.Where(w => campus.Contains(w.CampusId))
                .Select(s => new { id = s.Id, text = s.HostelName }).ToList();
            return JsonConvert.SerializeObject(hostel);
        }

        public string Getroom(int hostelId)
        {
            var hostel = hostelId == -1 ? db.Hostels.Select(s => s.Id).ToArray() : new[] { hostelId };
            var room = db.Rooms.Where(w => hostel.Contains(w.HostelId)).Select(s => new { id = s.Id, text = s.RoomNo })
                .ToList();
            return JsonConvert.SerializeObject(room);
        }

        public string Getavalaibleroom(int roomId, int regno = -1)
        {
            if (regno != -1)
            {
                var hosteladmission = db.HostelAdmissions
                    .Where(w => !w.Isexpel && w.RoomId == roomId && w.AdmisionId != regno).Count();
                var room = db.Rooms.ToList().Where(w => w.Id == roomId)
                    .Select(s => new { roomavailable = int.Parse(s.Capacity) - hosteladmission }).FirstOrDefault();
                return JsonConvert.SerializeObject(room);
            }
            else
            {
                var hosteladmission = db.HostelAdmissions.Where(w => !w.Isexpel && w.RoomId == roomId).Count();
                var room = db.Rooms.ToList().Where(w => w.Id == roomId)
                    .Select(s => new { roomavailable = int.Parse(s.Capacity) - hosteladmission }).FirstOrDefault();
                return JsonConvert.SerializeObject(room);
            }
        }

    }
}