using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using smsCore.Data;
using smsCore.Data.Helpers;
using smsCore.Data.Models.ViewModels;
using Syncfusion.EJ2.Base;
using Utilities;

namespace smsCore.Controllers
{
    [Authorize]
    public class AttendancesController : BaseController
    {
        private readonly SchoolEntities db;
        private readonly CurrentUser CurrentUser;
        private readonly ClsBussinessSetting setting;
        private readonly MessagingSystem ms;
        private readonly Extensions1 _extension;

        public AttendancesController(SchoolEntities _db, CurrentUser user, ClsBussinessSetting _setting, MessagingSystem _ms, Extensions1 extension)
        {
            db = _db;
            CurrentUser = user;
            setting = _setting;
            ms = _ms;
            _extension = extension; ;
        }

        #region Student Attendance / Attendance by RegNo  
        public IActionResult StudentAttendance()
        {
            ViewBag.AttendanceType =
                new SelectList(db.StudentAttendanceTypes.Select(s => new { s.ID, s.AttendanceName }).ToList(), "ID",
                    "AttendanceName");
            return View();
        }

        public IActionResult AttendanceByRegNos()
        {
            ViewBag.AttendanceType =
                new SelectList(db.StudentAttendanceTypes.Select(s => new { s.ID, s.AttendanceName }).ToList(), "ID",
                    "AttendanceName");
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> SaveAttendance(int regno, string time, int type = 0, int campusId = -1)
        {

            int[] campusIds = campusId == -1 ? CurrentUser.GetCampusIds() : new int[] { campusId };
            var datetime = DateTimeHelper.ConvertDate(time, false, "dd-MM-yyyy HH:mm");
            var date = datetime.Date;
            var at = new StudentAttendence();
            var admission = await db.Admissions.Include(i => i.Student).Include(i => i.Campus).Include(i => i.ClassSection.Section).Include(i => i.ClassSection.Class).Where(w => w.Student.RegistrationNo == regno && !w.IsExpell & campusIds.Contains(w.CampuseID)).Select(s => new
            {
                s.ID,
                s.Student.RegistrationNo,
                s.Student.FName,
                s.Student.StudentName,
                s.CampuseID,
                s.Campus.CampusName,
                s.ClassSection.Class.ClassName,
                s.ClassSection.Section.SectionName,
                Status = "",
                Time = datetime
            }).FirstOrDefaultAsync();
            if (admission == null)
            {
                return Json(new { status = false, data = new { RegistrationNo = regno, Status = "Not Found" } });
            }
            setting.CampusId = admission.CampuseID;

            var existing = db.StudentAttendences.Any(w => w.AdmissionID == admission.ID & w.AttendanceDate.Date == date);
            if (!existing)
            {
                var attendanceType = PublicVariables.AttendanceType.Present;
                if (type == 0)
                {
                    var schooltime = setting.Read(PublicVariables.EnumConfigurations.ATstudent.ToString())?.PropertyValue;
                    if (!string.IsNullOrEmpty(schooltime))
                    {
                        time = date.ToString("dd/MM/yyyy") + " " + schooltime;
                        var schoolOpeningTime = DateTimeHelper.ConvertDate(time, false, "dd/MM/yyyy hh:mm tt");
                        if (schoolOpeningTime != DateTime.MinValue && admission.Time > schoolOpeningTime)
                            attendanceType = PublicVariables.AttendanceType.LateComing;
                    }
                }
                else
                {
                    attendanceType = (PublicVariables.AttendanceType)type;
                }
                at.AdmissionID = admission.ID;
                at.AttendanceTypeID = (int)attendanceType;
                at.FineAmount = 0;
                at.manaual = false;
                at.sendmessage = false;
                at.UserID = CurrentUser.UserID;
                at.AttendanceDate = datetime;
                db.StudentAttendences.Add(at);
                try
                {
                    await db.SaveChangesAsync();
                    try
                    {
                        if (string.IsNullOrEmpty(ms.error))
                        {
                            string messageTempalte = ms.AttendanceMessageTempalte(admission.CampuseID, attendanceType);
                            messageTempalte = string.Format(messageTempalte,
                            admission.RegistrationNo,
                            admission.StudentName,
                            admission.FName,
                             admission.Time.ToString("hh:mm tt"),
                             admission.Time.ToString("dd/MM/yyyy"),
                             attendanceType.ToString(),
                             admission.ClassName);
                            if (await ms.sendMessage(regno, messageTempalte, true))
                            {
                                at.sendmessage = true;
                                await db.SaveChangesAsync();
                            }
                        }
                    }
                    catch { }

                    return Json(new { status = true, data = new { Photo = _extension.GetPhoto(regno), admission.RegistrationNo, admission.StudentName, admission.FName, admission.ClassName, admission.CampusName, admission.SectionName, admission.Time, Status = attendanceType.ToString() } });
                }
                catch
                {
                    return Json(new { status = true, data = new { Photo = _extension.GetPhoto(regno), admission.RegistrationNo, admission.StudentName, admission.FName, admission.ClassName, admission.CampusName, admission.SectionName, admission.Time, Status = "Error" } });
                }
            }
            else
            {
                return Json(new { status = true, data = new { Photo = _extension.GetPhoto(regno), admission.RegistrationNo, admission.StudentName, admission.FName, admission.ClassName, admission.CampusName, admission.SectionName, admission.Time, Status = "Already added" } });
            }
        }

        #endregion

        #region Attendance by class   
        public IActionResult AttendanceByClass()
        {
            ViewBag.AttendanceType = new SelectList(db.StudentAttendanceTypes.Select(s => new { s.ID, s.AttendanceName }).ToList(), "ID", "AttendanceName");
            return View();
        }


        [HttpPost]
        public JsonResult GetStudentInSection(DataManagerRequest request, int campus, int classId, int sectionid,
            string selectdate = "")
        {
            var clasfeeGroup = db.Admissions.Where(w =>
                !w.IsExpell && w.CampuseID == campus && w.ClassSection.SectionID == sectionid &&
                w.ClassSection.ClassID == classId);

            var today = DateTimeHelper.ConvertDate(selectdate).Date;
            var data = clasfeeGroup.Select(s => new
            {
                s.ID,
                s.Student.StudentName,
                s.Student.RegistrationNo,
                Status = s.StudentAttendences
                         .Where(ww =>
                            ww.AttendanceDate.Date == today)
                         .Select(ss => ss.StudentAttendanceType.AttendanceName).FirstOrDefault()
            });

            return Json(new { result = data, count = data.Count() });
        }

        [HttpPost]
        public async Task<JsonResult> SaveAttendanceByClass(List<AttendanceByClassViewModel> data)
        {
            try
            {
                int attendanceType = data[0].AttendanceType;
                var today = DateTimeHelper.ConvertDate(data[0].Date);
                var fine = db.StudentAttendanceTypes.ToList()
                                .Where(w => w.ID == attendanceType).Select(s => s.Fine).FirstOrDefault();
                
                for (var i = 0; i < data.Count(); i++)
                {
                    var attendance = new StudentAttendence();
                    int id = data[i].ID;
                    var existing = db.StudentAttendences.FirstOrDefault(w => w.AdmissionID == id &
                   w.AttendanceDate.Date == today);
                    if (existing == null)
                    {
                        attendance.FineAmount = fine;
                        attendance.manaual = true;
                        attendance.sendmessage = false;
                        attendance.AttendanceTypeID = attendanceType;
                        attendance.AttendanceDate = today;
                        attendance.UserID = CurrentUser.UserID;
                        attendance.AdmissionID = id;
                        db.StudentAttendences.Add(attendance);

                    }
                    else
                    {
                        existing.AttendanceTypeID = attendanceType;
                    }

                }
                await db.SaveChangesAsync();
                return Json(new { status = true, message = "Attendance successfully saved for selected students." });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.InnerException == null ? ex.Message : ex.InnerException.Message });
            }
        }

        #endregion

        #region Edit Student Attendance
        public IActionResult EditStudentAttendance()
        {
            ViewBag.AttendanceType = new SelectList(db.StudentAttendanceTypes.Select(s => new { s.ID, s.AttendanceName }).ToList(), "ID", "AttendanceName");
            return View();
        }

        public IActionResult StudentAttendanceByDate(DataManagerRequest request, string date, int campid = -1, int secid = -1,
            int clsid = -1)
        {
            var dt = DateTimeHelper.ConvertDate(date, false, "dd/MM/yyyy hh:mm tt");
            var _date = dt.Date;
            var
            data = db.Admissions.Where(w =>
                !w.IsExpell && w.CampuseID == campid && w.ClassSection.SectionID == secid &&
                w.ClassSection.ClassID == clsid).Select(s =>
                        new
                        {
                            s.ID,
                            s.Student.RegistrationNo,
                            s.Student.StudentName,
                            s.Student.FName,
                            Status = s.StudentAttendences.Where(w => w.AttendanceDate.Date == _date).Select(t => t.StudentAttendanceType.AttendanceName).FirstOrDefault()
                            //GetstaffAttendanceStatus(s.Id, DateTimeHelper.ConvertDate(date, true, "dd/MM/yyyy hh:mm tt"))
                        });

            return Json(new { result = data, count = data.Count() });
        }

        public async Task<JsonResult> EditStudentAttendanceSave(List<AttendanceByClassViewModel> data)
        {
            try
            {
                int type = data[0].AttendanceType;
                var attendate = DateTimeHelper.ConvertDate(data[0].Date, false, "dd/MM/yyyy hh:mm tt");
                var _date = attendate.Date;
                foreach (var st in data)
                {
                    int id = st.ID;

                    var at = db.StudentAttendences
                 .FirstOrDefault(w => w.AdmissionID == id && w.AttendanceDate.Date == _date);
                    //     var at = db.StudentAttendences.Where(w => w.ID == regno).FirstOrDefault();
                    if (at == null)
                    {
                        var studentAttendance = new StudentAttendence();
                        studentAttendance.AttendanceDate = attendate;
                        studentAttendance.AttendanceTypeID = type;
                        studentAttendance.AdmissionID = id;
                        studentAttendance.sendmessage = false;
                        studentAttendance.FineAmount = 0;
                        studentAttendance.UserID = CurrentUser.UserID;
                        db.StudentAttendences.Add(studentAttendance);
                    }
                    else
                    {
                        at.AttendanceTypeID = type;
                    }
                }

                await db.SaveChangesAsync();
                return Json(new { status = true, message = "Attendance successfully saved for selected students." });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.InnerException == null ? ex.Message : ex.InnerException.Message });
            }
        }

        #endregion

        #region Atendance By Selected class


        public IActionResult AttendanceBySelectedClass()
        {
            ViewBag.AttendanceType = new SelectList(db.StudentAttendanceTypes.Select(s => new { s.ID, s.AttendanceName }).ToList(), "ID", "AttendanceName");
            return View();
        }


        [HttpPost]
        public JsonResult AttendanceBySelectedClassList(DataManagerRequest request, string selectdate = "", int CampusID = -1)
        {
            //var classes = db.ClassSections.Where(w => w.CampusID == CampusID).AsEnumerable().OrderBy(o => new { o.ClassID, o.SectionID }).AsNoTracking();

           // var classes = db.ClassSections.Where(w => w.CampusID == CampusID).OrderBy(o => new { o.ClassID, o.SectionID }).AsNoTracking();
            var classes = db.ClassSections.Where(w => w.CampusID == CampusID && w.CampusID != null).OrderBy(o => o.ClassID).ThenBy(o => o.SectionID).AsNoTracking();

            int present = (int)PublicVariables.AttendanceType.Present;
            int absent = (int)PublicVariables.AttendanceType.Absent;
            int leave = (int)PublicVariables.AttendanceType.Leave;
            int late = (int)PublicVariables.AttendanceType.LateComing;
            DateTime today = DateTimeHelper.ConvertDate(selectdate, false, "dd/MM/yyyy hh:mm tt").Date;
            var data = classes.Select(s =>
                 new
                 {
                     s.ID,
                     s.CampusID,
                     s.Class.ClassName,
                     s.Section.SectionName,
                     Present = db.StudentAttendences.Count(c => c.Admission.ClassSectionID == s.ID & c.AttendanceDate.Date == today & c.AttendanceTypeID == present),
                     Absent = db.StudentAttendences.Count(c => c.Admission.ClassSectionID == s.ID & c.AttendanceDate.Date == today & c.AttendanceTypeID == absent),
                     Leave = db.StudentAttendences.Count(c => c.Admission.ClassSectionID == s.ID & c.AttendanceDate.Date == today & c.AttendanceTypeID == leave),
                     Late = db.StudentAttendences.Count(c => c.Admission.ClassSectionID == s.ID & c.AttendanceDate.Date == today & c.AttendanceTypeID == late),
                     NoAttendance = s.Admissions.Count(c => !c.IsExpell & !c.StudentAttendences.Any(ct => ct.AttendanceDate.Date == today)),
                     Total = s.Admissions.Count(c => s.CampusID == CampusID & !c.IsExpell)
                 });

            return Json(new { result = data, count = data.Count() });
        }

        [HttpPost]
        public async Task<JsonResult> AttendanceBySelectedClass(List<AttendanceByClassViewModel> data)
        {
            try
            {
                int[] clsSectionIds = data.Select(s => s.ID).ToArray();
                int campusId = data[0].CampusID;
                DateTime datetime = DateTimeHelper.ConvertDate(data[0].Date, false, "dd/MM/yyyy hh:mm tt");
                if (datetime == DateTime.MinValue)
                {
                    return Json(new { status = false, message = "Attendance date and time is invalid." });
                }
                var date = datetime.Date;
                var type = data[0].AttendanceType;

                var Admissions = db.Admissions.Where(w =>
                    w.CampuseID == campusId && !w.IsExpell && clsSectionIds.Contains(w.ClassSectionID) && !w.StudentAttendences
                        .Any(ww => ww.AttendanceDate.Date == date)).Select(s => s.ID);
                if (Admissions.Count() > 0)
                {
                    foreach (var adm in Admissions)
                    {
                        var attendance = new StudentAttendence();
                        attendance.AdmissionID = adm;
                        attendance.FineAmount = 0;
                        attendance.AttendanceDate = datetime;
                        attendance.manaual = true;
                        attendance.sendmessage = false;
                        attendance.UserID = CurrentUser.UserID;
                        attendance.AttendanceTypeID = type;
                        db.StudentAttendences.Add(attendance);
                    }
                    await db.SaveChangesAsync();
                    return Json(new { status = true, message = "Attendance successfully saved for selected students." });
                }
                else
                {
                    return Json(new { status = false, message = "No student found in selected class." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.InnerException == null ? ex.Message : ex.InnerException.Message });
            }
        }
        #endregion

        #region Staff Attendance
        public ActionResult StaffAttendance()
        {
            ViewBag.AttendanceType =
                new SelectList(db.EmployeeAttendanceTypes.Select(s => new { s.ID, s.AttendanceName }).ToList(), "ID", "AttendanceName");
            return View();
        }
        [HttpPost]
        public async Task<JsonResult> SaveStaffAttendance(string code, string time, int campusId = -1, int type = 0)
        {
            int[] campusIds = campusId == -1 ? CurrentUser.GetCampusIds() : new int[] { campusId };
            var datetime = DateTimeHelper.ConvertDate(time, false, "dd-MM-yyyy HH:mm");
            var date = datetime.Date;
            var at = new EmployeeAttendance();
            var staff = await db.tbl_Employee.Where(w => w.employeeCode == code && !w.LeavedStaffs.Any() & campusIds.Contains(w.CampusID))
                .Select(s => new
                {
                    s.Id,
                    s.employeeCode,
                    s.employeeName,
                    Time = datetime,
                    s.CampusID
                }).FirstOrDefaultAsync();
            if (staff == null)
            {
                return Json(new { status = false, data = new { Code = code, Status = "Not Found" } });
            }
            setting.CampusId = (staff.CampusID);

            var existing = db.EmployeeAttendances.Any(w => w.StaffID == staff.Id & w.AttendanceDate.Date == date);
            string status = "";
            if (!existing)
            {
                var attendanceType = PublicVariables.AttendanceType.Present;
                if (type == 0)
                {
                    var schooltime = setting.Read(PublicVariables.EnumConfigurations.ATstaff.ToString())?.PropertyValue;
                    if (!string.IsNullOrEmpty(schooltime))
                    {
                        time = date.ToString("dd/MM/yyyy") + " " + schooltime;
                        var schoolOpeningTime = DateTimeHelper.ConvertDate(time, false, "dd/MM/yyyy hh:mm tt");
                        if (schoolOpeningTime != DateTime.MinValue && staff.Time > schoolOpeningTime)
                            attendanceType = PublicVariables.AttendanceType.LateComing;
                    }
                }
                else
                {
                    attendanceType = (PublicVariables.AttendanceType)type;
                }
                at.StaffID = staff.Id;
                at.AttendanceTypeID = (int)attendanceType;
                at.UserID = CurrentUser.UserID;
                at.AttendanceDate = datetime;
                db.EmployeeAttendances.Add(at);


                try
                {

                    await db.SaveChangesAsync();
                    try
                    {
                        var send = (bool)setting.ReadWithType(PublicVariables.EnumConfigurations.SendStaffAttendance.ToString(), typeof(bool));
                        var tosendMobile = (string)setting.ReadWithType(PublicVariables.EnumConfigurations.StaffAttendanceMobileNo.ToString(), typeof(string));
                        if (send & !string.IsNullOrEmpty(tosendMobile))

                        {
                            if (string.IsNullOrEmpty(ms.error))
                            {
                                string messageTempalte = "Mr/Ms {0} - {1} has reached school at {2}";
                                messageTempalte = string.Format(messageTempalte,
                                staff.employeeName,
                                staff.employeeCode,

                                 staff.Time.ToString("dd/MM/yyyy hh:mm tt"));

                                await ms.sendMessage(messageTempalte, new string[] { tosendMobile });
                            }
                        }
                    }
                    catch { }
                    status = attendanceType.ToString();
                }
                catch
                {
                    status = attendanceType.ToString();
                }
            }
            else
            {
                status = "Already added";
            }
            return Json(new
            {
                status = true,
                data = new
                {
                    staff.Time,
                    Status = status
                }
            });

        }

        #endregion
        public ActionResult AttendanceByStaffCodes()
        {
            ViewBag.AttendanceType =
                new SelectList(db.EmployeeAttendanceTypes.Select(s => new { s.ID, s.AttendanceName }).ToList(), "ID",
                    "AttendanceName");
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> SaveAttendanceofStaff(string codeno, string time, int type = 0, int campusId = -1)
        {

            int[] campusIds = campusId == -1 ? CurrentUser.GetCampusIds() : new int[] { campusId };
            var datetime = DateTimeHelper.ConvertDate(time, false, "dd-MM-yyyy HH:mm");
            var date = datetime.Date;
            var at = new EmployeeAttendance();
            var employee = await db.tbl_Employee.Where(w => w.employeeCode == codeno & campusIds.Contains(w.CampusID)).Select(s => new
            {
                s.Id,
                s.employeeCode,
                s.employeeName,
                s.CampusID,
                s.Campus.CampusName,
                Status = "",
                Time = datetime
            }).FirstOrDefaultAsync();
            if (employee == null)
            {
                return Json(new { status = false, data = new { employeeCode = codeno, Status = "Not Found" } });
            }
            setting.CampusId = (employee.CampusID);
            var exist = db.EmployeeAttendances.Any(x => x.StaffID == employee.Id & x.AttendanceDate.Date == date);
            if (!exist)
            {
                var attendanceType = PublicVariables.AttendanceType.Present;
                if (type == 0)
                {
                    var schooltime = setting.Read(PublicVariables.EnumConfigurations.ATstaff.ToString())?.PropertyValue;
                    if (!string.IsNullOrEmpty(schooltime))
                    {
                        time = date.ToString("dd/MM/yyyy") + " " + schooltime;
                        var schoolOpeningTime = DateTimeHelper.ConvertDate(time, false, "dd/MM/yyyy hh:mm tt");
                        if (schoolOpeningTime != DateTime.MinValue && employee.Time > schoolOpeningTime)
                            attendanceType = PublicVariables.AttendanceType.LateComing;
                    }
                }
                else
                {
                    attendanceType = (PublicVariables.AttendanceType)type;
                }
                at.StaffID = employee.Id;
                at.AttendanceTypeID = (int)attendanceType;
                at.UserID = CurrentUser.UserID;
                at.AttendanceDate = datetime;
                db.EmployeeAttendances.Add(at);


                try
                {
                    await db.SaveChangesAsync();

                    return Json(new { status = true, data = new { Photo = _extension.GetPhoto(codeno), employee.employeeCode, employee.employeeName, employee.CampusName, employee.Time, Status = attendanceType.ToString() } });
                }
                catch
                {
                    return Json(new { status = true, data = new { Photo = _extension.GetPhoto(codeno), employee.employeeCode, employee.employeeName, employee.CampusName, employee.Time, Status = "Error" } });
                }
            }
            else
            {
                return Json(new { status = true, data = new { Photo = _extension.GetPhoto(codeno), employee.employeeCode, employee.employeeName, employee.CampusName, employee.Time, Status = "Already added" } });
            }
        }

        #region Edit Staff Attendance
        public IActionResult EditStaffAttendance()
        {
            ViewBag.AttendanceType =
                new SelectList(db.EmployeeAttendanceTypes.Select(s => new { s.ID, s.AttendanceName }).ToList(), "ID",
                    "AttendanceName");
            return View();
        }

        public IActionResult StaffAttendanceByDate(DataManagerRequest request, string date, int campus)
        {
            var dt = DateTimeHelper.ConvertDate(date, false, "dd/MM/yyyy hh:mm tt");
            var _date = dt.Date;
            var
            data = db.tbl_Employee.Where(w => w.CampusID == campus).Select(s =>
                        new
                        {
                            s.Id,
                            s.employeeCode,
                            s.employeeName,
                            s.tbl_Designation.designationName,
                            Status = s.EmployeeAttendances.Where(w => w.AttendanceDate.Date == _date).Select(t => t.EmployeeAttendanceType.AttendanceName).FirstOrDefault()
                            //GetstaffAttendanceStatus(s.Id, DateTimeHelper.ConvertDate(date, true, "dd/MM/yyyy hh:mm tt"))
                        });

            return Json(new { result = data, count = data.Count() });
        }

        public async Task<JsonResult> EditStaffAttendanceSave(List<StaffAttendanceViewModel> data)
        {
            try
            {
                int type = data[0].AttendanceType;
                var attendate = DateTimeHelper.ConvertDate(data[0].Date, false, "dd/MM/yyyy hh:mm tt");
                foreach (var st in data)
                {
                    decimal id = st.ID;
                    var at = db.EmployeeAttendances
                      .FirstOrDefault(w => w.StaffID == id && w.AttendanceDate.Date == attendate.Date);

                    if (at == null)
                    {
                        var employeeAttendance = new EmployeeAttendance();
                        employeeAttendance.AttendanceDate = attendate;
                        employeeAttendance.AttendanceTypeID = type;
                        employeeAttendance.UserID = CurrentUser.UserID;
                        employeeAttendance.StaffID = st.ID;

                        db.EmployeeAttendances.Add(employeeAttendance);
                    }
                    else
                    {
                        at.AttendanceTypeID = type;
                    }
                }

                await db.SaveChangesAsync();
                return Json(new { status = true, message = "Attendance successfully saved for selected employees." });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.InnerException == null ? ex.Message : ex.InnerException.Message });
            }
        }
        #endregion

        public IActionResult StaffShortLeave()
        {
            //ViewBag.StaffList = new SelectList(db.tbl_Employee.ToList().Select(s => new { s.Id, StaffName = s.employeeName + s.gender.ToLower() == "male" ? " S/O " : "D/O" + s.FatherName + "(" + s.EmployeeDesignations.Where(w=>w.IsActive).Select(ss=>ss.EmployeeDesignationList.DesignationName).FirstOrDefault() + ")" }));
            var staff = db.tbl_Employee.Select(s => new { s.Id, s.employeeName }).ToList();
            staff.Insert(0, new { Id = -1, employeeName = "---Select---" });
            ViewBag.StaffList = new SelectList(staff, "Id", "employeeName");
            ViewBag.AttendanceType =
                new SelectList(db.EmployeeAttendanceTypes.Select(s => new { s.ID, s.AttendanceName }).ToList(), "ID",
                    "AttendanceName");
            return View();
        }

        public JsonResult GetStaffForAttendance(int id, int campusId = -1)
        {
            var campusIds = campusId == -1 ? db.Campuses.Select(w => w.ID).ToArray() : new[] { campusId };
            object? Data;
            var jsonResult = new JsonResult(null);
            if (CurrentUser.BasicUserType == EnumManager.BasicUserType.Campus)
            {
                campusIds = CurrentUser.GetCampusIds();
                Data = db.tbl_Employee.ToList().Where(w => w.Id == id && campusIds.Contains(w.CampusID)).Select(
                    s => new
                    {
                        s.employeeName,
                        FName = s.FatherName,
                        s.CNIC,
                        PostalAddress = s.address,
                        Designation = s.tbl_Designation.designationName,
                        Photo = s.Photo == null ? "" : Convert.ToBase64String(s.Photo)
                    }).FirstOrDefault();
            }
            else
            {
                //campusId = CurrentUser.GetCampusIds().FirstOrDefault();
                Data = db.tbl_Employee.Include(i => i.tbl_Designation).ToList().Where(w => w.Id == id && campusIds.Contains(w.CampusID)).Select(
                    s => new
                    {
                        s.employeeName,
                        FName = s.FatherName,
                        s.CNIC,
                        PostalAddress = s.address,
                        //Designation = s.tbl_Designation.designationName,
                        Designation = s.tbl_Designation.designationName,
                        Photo = s.Photo == null ? "" : Convert.ToBase64String(s.Photo)
                    }).FirstOrDefault();
            }

            jsonResult = new JsonResult(Data);
            return jsonResult;
        }


        public JsonResult SaveStaffShortLeave(int id)
        {
            var today = ExtenstionMethods.Now();
            var message = "";
            var result = new JsonResult(null);
            //var existing = db.EmployeeShortLeaves.Where(w => w.StaffID == id).ToList().Where(w => w.OutTime.Date == DateTime.Now.Date).LastOrDefault();
            var existing = db.EmployeeShortLeaves.Where(w => w.StaffID == id).ToList()
                .Where(w => w.OutTime.Date == today.Date && w.InTime == null).FirstOrDefault();

            if (existing != null)
            {
                existing.InTime = today;
            }
            else
            {
                var at = new EmployeeShortLeave();
                //StaffAttendance at = new StaffAttendance();
                var leaveid = 1;
                if (db.EmployeeShortLeaves.Count() != 0) leaveid = db.EmployeeShortLeaves.Max(w => w.ID) + 1;
                at.StaffID = id; // db.tbl_Employee.Where(w => w.Id == id).First().emp;
                //at.ID = leaveid;
                at.UserID = CurrentUser.UserID;
                at.OutTime = today;
                db.EmployeeShortLeaves.Add(at);
            }

            try
            {
                db.SaveChanges();
                message = "success";
                result = new JsonResult(message);
                return result;
            }

            catch (Exception ex)
            {
                message = ex.Message;
                result = new JsonResult(message);
                return result;
            }
        }
    }
}