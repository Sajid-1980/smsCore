using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Models;
using sms.Models;
using smsCore.Controllers;
using smsCore.Data;
using smsCore.Data.Helpers;
using Utilities;
using Messaging;
using Microsoft.AspNetCore.Mvc;

namespace sms.WebApiControllers
{

  
    public class FMds 
    {
      
        public int Id { get; set; }
        public string type { get; set; }
        public string impression { get; set; }
    }

    [Route("api/attendance")]
    public class AttendanceController : BaseApiController
    {
        // GET api/<controller>
        //private static readonly ILog Log = LogManager.GetLogger(typeof(AttendanceController));
        private readonly ILogger<AttendanceController> _logger;
        private readonly SchoolEntities db;
        private readonly CurrentUser _user;
        private readonly ClsBussinessSetting clsBussinessSetting;
        private readonly MessagingSystem messagingSystem;

        public AttendanceController(SchoolEntities _db, CurrentUser Cuser, IWebHostEnvironment webHostEnvironment, MessagingSystem _messagingSystem, ILogger<AttendanceController> Logger, UserManager<ApplicationUser> userManger, IHttpContextAccessor _httpContextAccessor, ClsBussinessSetting _clsBussinessSetting) : base(_db, Cuser, webHostEnvironment, userManger, _httpContextAccessor)
        {
            this.clsBussinessSetting = _clsBussinessSetting;
            this.db=_db;
            this._logger = Logger;
            this.messagingSystem = _messagingSystem;


        }

        //public AttendanceController(SchoolEntities _db, CurrentUser Cuser, ILogger<AttendanceController> logger) { 
        //     db = _db;
        //    _user = Cuser;
        //    _logger= logger;

        //}

        //public AttendanceController(SchoolEntities _db,CurrentUser Cuser)
        //{
        //    db = _db;
        //    _user = Cuser;

        //}
        [HttpPost]
        [Route("download")]
        public IActionResult DownloadData(int campusId)
        {
            _logger.LogInformation("Downloading data for campus: " + campusId);
            var data = db.EmployeeThumbImpressions.AsNoTracking().Where(w => w.tbl_Employee.CampusID == campusId && w.tbl_Employee.LeavedStaffs.Count == 0).ToList()
                .Select(s => new LocalImpressions
                {
                    Name = s.tbl_Employee.employeeName,
                    Impression = s.Thumb,
                    Campus = s.tbl_Employee.Campus.CampusName,
                    Class = string.Empty,
                    AType = "employee",
                    Designation = s.tbl_Employee.tbl_Designation.designationName,
                    CampusId = s.tbl_Employee.CampusID,
                    Section = string.Empty,
                    BasicId = (int) s.StaffID,
                    Photo = s.tbl_Employee.Photo
                });
            _logger.LogInformation("Downloading Employee Found data for campus: " + data.Count());

            //var stddata = db.Admissions.AsNoTracking().Where(w => w.CampuseID == campusId && !w.IsExpell).ToList()
            //    .Select(s => new LocalImpressions
            //    {
            //        Name = s.Student.StudentName,
            //        Impression = s.Student.StudentThumbImpressions.DefaultIfEmpty().Select(t => t.Thumb)
            //            .FirstOrDefault(),
            //        Campus = s.Campus.CampusName,
            //        Class = s.ClassSection.Class.ClassName,
            //        AType = "student",
            //        Designation = string.Empty,
            //        CampusId = s.CampuseID,
            //        Section = s.ClassSection.Section.SectionName,
            //        BasicId = s.ID,
            //        Photo = s.Student.StudentPhotos.OrderByDescending(o => o.ID).Select(i => i.StudentImage)
            //            .FirstOrDefault()
            //    });
            var stddata = db.Admissions.AsNoTracking()
                .Where(w => w.CampuseID == campusId && !w.IsExpell)                
                .ToList()
                .Select(s => new LocalImpressions
                {
                    Name = s.Student.StudentName,
                    Impression = s.Student.StudentThumbImpressions.DefaultIfEmpty().Select(t => t.Thumb).FirstOrDefault(),
                    Campus = s.Campus.CampusName,
                    Class = s.ClassSection.Class.ClassName,
                    AType = "student",
                    Designation = string.Empty,
                    CampusId = s.CampuseID,
                    Section = s.ClassSection.Section.SectionName,
                    BasicId = s.ID,
                    Photo = s.Student.StudentPhotos.OrderByDescending(o => o.ID).Select(i => i.StudentImage).FirstOrDefault()
                })
                .ToList();



            _logger.LogInformation("Downloading Student Found data for campus: " + stddata.Count());

            try
            {
                _logger.LogInformation("sEND Found data for campus: " + data.Count());

                var result = data.Union(stddata);
                _logger.LogInformation("Sending Found data for campus: " + result.Count());
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Download Data Failed: ", ex);
                _logger.LogInformation("Download Data fAILIED", ex.InnerException);
            }

            return Ok("");
        }

        [HttpPost]
        [Route("search")]
        public IActionResult GetInfo(string type, string Id)
        {
           // var db = new SchoolEntities();
            try
            {
                if (type == "student")
                {
                }
                else if (type == "employee")
                {
                    var employee = db.tbl_Employee.AsNoTracking().Where(f => f.employeeCode == Id).Select(s => new
                    {
                        Id = s.Id,
                        Code = s.employeeCode,
                        Name = s.employeeName,
                        s.Photo,
                        DesignationName = s.tbl_Designation.designationName,
                        s.Campus.CampusName,
                        ThumbRegistered = s.EmployeeThumbImpressions.Count > 0
                    }).FirstOrDefault();

                    if (employee != null) return Ok(employee);
                }

                return Ok("not foud");
            }
            catch (Exception ee)
            {
                return BadRequest(ee.Message);
            }
        }

        [HttpPost]
        [Route("enroll")]
        public IActionResult Enroll([FromBody] FMds fMds)
        {
            try
            {
                if (fMds.type == "student")
                {
                    var id = 0;
                    try
                    {
                        id = db.StudentThumbImpressions.Max(m => m.ID);
                    }
                    catch
                    {
                        id = 0;
                    }

                    id++;
                    decimal.TryParse(fMds.Id.ToString(), out var regno);
                    var studnet = db.Students.AsNoTracking().FirstOrDefault(f => f.RegistrationNo == regno);
                    if (studnet != null)
                    {
                        var thumb = studnet.StudentThumbImpressions.FirstOrDefault();
                        if (thumb == null)
                        {
                            thumb = new StudentThumbImpression();
                            thumb.ID = id;
                            thumb.StudentID = studnet.ID;
                            db.StudentThumbImpressions.Add(thumb);
                        }

                        thumb.Thumb = fMds.impression;
                        thumb.Finger = 1;
                        db.SaveChanges();
                    }
                }
                else if (fMds.type == "employee")
                {
                    var id = 0;
                    try
                    {
                        id = db.EmployeeThumbImpressions.Max(m => m.ID);
                    }
                    catch
                    {
                        id = 0;
                    }

                    id++;
                    decimal.TryParse(fMds.Id.ToString(), out var Id);
                    var employee = db.tbl_Employee.AsNoTracking().FirstOrDefault(f => f.Id == Id);
                    if (employee != null)
                    {
                        var thumb = employee.EmployeeThumbImpressions.FirstOrDefault();
                        if (thumb == null)
                        {
                            thumb = new EmployeeThumbImpression();
                            thumb.ID = id;
                            thumb.StaffID = employee.Id;
                            db.EmployeeThumbImpressions.Add(thumb);
                        }

                        thumb.Thumb = fMds.impression;
                        thumb.Finger = 1;
                        db.SaveChanges();
                    }
                }

                return Ok(true);
            }
            catch (Exception ee)
            {
                return BadRequest(ee.Message);
            }
        }

        [HttpPost]
        [Route("attendance")]
        public async Task<IActionResult> AddAttendance([FromBody] AddAttendanceModel model)
        {
            try
            {

                _logger.LogError("Attendance trying to add");
                clsBussinessSetting.CampusId = model.campusId;
                //var setting = new ClsBussinessSetting(new Models.databaseAccesSql { campusId = model.campusId });

                if (model.type == "employee")
                {
                    //var exist = db.EmployeeAttendances.Where(w =>
                    //        DbFunctions.TruncateTime(w.AttendanceDate) == model.time.Date && w.StaffID == model.Id)
                    //    .FirstOrDefault();
                    var exist = db.EmployeeAttendances.Where(w =>
                            w.AttendanceDate.Date == model.time.Date && w.StaffID == model.Id)
                        .FirstOrDefault();

                    if (exist != null)
                    {
                        _logger.LogError("Attendance already exist");
                        return Ok("alread exist");
                    }

                    var attendanceType = PublicVariables.AttendanceType.Present;
                    var time = clsBussinessSetting.Read(PublicVariables.EnumConfigurations.ATstaff.ToString())?.PropertyValue;
                    if (!string.IsNullOrEmpty(time))
                    {
                        time = DateTime.Today.Date.ToString("MM/dd/yyyy") + " " + time;
                        if (DateTime.TryParse(time, out var t) && model.time > t)
                            attendanceType = PublicVariables.AttendanceType.LateComing;
                    }

                    exist = new EmployeeAttendance
                    {
                        AttendanceTypeID = (int) attendanceType, AttendanceDate = model.time, StaffID = model.Id,
                        UserID = model.userId
                    };

                    db.EmployeeAttendances.Add(exist);
                    db.SaveChanges();
                    if (model.sendMessage)
                    {
                        var smsNo = clsBussinessSetting.Read(PublicVariables.EnumConfigurations.StaffAttendanceMobileNo.ToString())
                            .PropertyValue.Trim();
                        if (!string.IsNullOrEmpty(smsNo) && smsNo.Length >= 10)
                        {
                            var employe = db.tbl_Employee.AsNoTracking()
                                .FirstOrDefault(f => f.Id == exist.StaffID);
                            if (employe != null)
                            {
                               var messages = messagingSystem;

                                var message =
                                    string.Format("Mr/Ms {0}  has reached at institution.Attendance: {1} Time: {2}",
                                        employe.employeeName, attendanceType.ToString(),
                                        model.time.ToString("dd-MMM-yyy hh:mm tt"));
                                await messages.sendMessage(message, smsNo);
                            }
                        }
                    }

                    return Ok(true);
                }

                if (model.type == "student")
                {
                    var exist = db.StudentAttendences.Where(w =>w.AttendanceDate.Date == model.time.Date && w.AdmissionID == model.Id)
                        .FirstOrDefault();
                    if (exist != null) return Ok("alread exist");

                    var attendanceType = PublicVariables.AttendanceType.Present;
                    var time = clsBussinessSetting.Read(PublicVariables.EnumConfigurations.ATstudent.ToString())?.PropertyValue;
                    if (!string.IsNullOrEmpty(time))
                    {
                        time = DateTime.Today.Date.ToString("MM/dd/yyyy") + " " + time;
                        if (DateTime.TryParse(time, out var t) && model.time > t)
                            attendanceType = PublicVariables.AttendanceType.LateComing;
                    }

                    exist = new StudentAttendence
                    {
                        AttendanceTypeID = (int) attendanceType, AttendanceDate = model.time, AdmissionID = model.Id,
                        UserID = model.userId
                    };

                    db.StudentAttendences.Add(exist);
                    db.SaveChanges();
                    if (model.sendMessage)
                    {
                        var smsNo = exist.Admission.Student.StudentMobiles.Where(w => w.IsDefault).FirstOrDefault()
                            ?.MobileNo;
                        var messageTemplate = clsBussinessSetting.Read(PublicVariables.EnumConfigurations.PresentMessage.ToString())
                            ?.PropertyValue;
                        if (!string.IsNullOrEmpty(smsNo) && smsNo.Length >= 10 &&
                            !string.IsNullOrEmpty(messageTemplate))
                        {
                            var admission = db.Admissions.FirstOrDefault(f => f.ID == exist.AdmissionID);
                            if (admission != null)
                            {
                                var messages = messagingSystem;
                                var message = string.Format(messageTemplate, admission.Student.RegistrationNo,
                                    admission.Student.StudentName, admission.Student.FName,
                                    model.time.Date.ToString("hh:mm tt"), model.time.Date.ToString("dd-MM-yy"),
                                    attendanceType.ToString(), admission.ClassSection.Class.ClassName, 1);
                                await messages.sendMessage(message, smsNo);
                            }
                        }
                    }

                    return Ok(true);
                }
            }
            catch (Exception aa)
            {
                _logger.LogError("Attendance Entry", aa);
                _logger.LogError("Attendance Entry", aa.InnerException);
                return GetErrorResult(aa);
            }

            return Ok(true);
        }

        [HttpPost]
        [Route("upload-attendance")]
        public async Task<IActionResult> AddAttendanceUpload([FromBody] List<AddAttendanceModel> models)
        {
            try
            {
                //var setting = new  ClsBussinessSetting(models.FirstOrDefault().campusId);

                var attendanceType = PublicVariables.AttendanceType.Present;

                var messageTemplate = clsBussinessSetting.Read(PublicVariables.EnumConfigurations.PresentMessage.ToString())
                    ?.PropertyValue;

                var messageList = new List<SendSmsModel>();

                foreach (var model in models)
                    if (model.type == "employee")
                    {
                        var exist = db.EmployeeAttendances.Where(w =>
                                w.AttendanceDate.Date == model.time.Date && w.StaffID == model.Id)
                            .FirstOrDefault();
                        if (exist != null)
                        {
                        }
                        else
                        {
                            var time = clsBussinessSetting.Read(PublicVariables.EnumConfigurations.ATstaff.ToString())
                                ?.PropertyValue;
                            if (!string.IsNullOrEmpty(time))
                            {
                                time = DateTime.Today.Date.ToString("MM/dd/yyyy") + " " + time;
                                if (DateTime.TryParse(time, out var t) && model.time > t)
                                    attendanceType = PublicVariables.AttendanceType.LateComing;
                            }

                            exist = new EmployeeAttendance
                            {
                                AttendanceTypeID = (int) attendanceType, AttendanceDate = model.time,
                                StaffID = model.Id, UserID = model.userId
                            };

                            db.EmployeeAttendances.Add(exist);
                            db.SaveChanges();
                            if (model.sendMessage)
                            {
                                var smsNo = clsBussinessSetting
                                    .Read(PublicVariables.EnumConfigurations.StaffAttendanceMobileNo.ToString())
                                    .PropertyValue.Trim();
                                if (!string.IsNullOrEmpty(smsNo) && smsNo.Length >= 10)
                                {
                                    var employe = db.tbl_Employee.AsNoTracking()
                                        .FirstOrDefault(f => f.Id == exist.StaffID);
                                    if (employe != null)
                                    {
                                        var message =
                                            string.Format("Mr/Ms {0} Attendance Recorded. Attendance: {1} Time: {2}",
                                                employe.employeeName, attendanceType.ToString(),
                                                model.time.ToString("dd-MMM-yyy hh:mm tt"));
                                        messageList.Add(new SendSmsModel
                                        {
                                            Mask = string.Empty, Message = message, MobileNo = new[] {smsNo},
                                            testMode = false, Unicode = false
                                        });
                                        //await messages.sendMessage(message, smsNo, false);
                                    }
                                }
                            }
                        }
                    }
                    else if (model.type == "student")
                    {
                        var exist = db.StudentAttendences.Where(w =>
                                w.AttendanceDate.Date == model.time.Date &&
                                w.AdmissionID == model.Id)
                            .FirstOrDefault();
                        if (exist != null) return Ok("alread exist");

                        var time = clsBussinessSetting.Read(PublicVariables.EnumConfigurations.ATstudent.ToString())?.PropertyValue;
                        if (!string.IsNullOrEmpty(time))
                        {
                            time = DateTime.Today.Date.ToString("MM/dd/yyyy") + " " + time;
                            if (DateTime.TryParse(time, out var t) && model.time > t)
                                attendanceType = PublicVariables.AttendanceType.LateComing;
                        }

                        exist = new StudentAttendence
                        {
                            AttendanceTypeID = (int) attendanceType, AttendanceDate = model.time,
                            AdmissionID = model.Id, UserID = model.userId
                        };

                        db.StudentAttendences.Add(exist);
                        db.SaveChanges();
                        if (model.sendMessage)
                        {
                            var smsNo = exist.Admission.Student.StudentMobiles.Where(w => w.IsDefault).FirstOrDefault()
                                ?.MobileNo;
                            if (!string.IsNullOrEmpty(smsNo) && smsNo.Length >= 10 &&
                                !string.IsNullOrEmpty(messageTemplate))
                            {
                                var admission = db.Admissions.FirstOrDefault(f => f.ID == exist.AdmissionID);
                                if (admission != null)
                                    try
                                    {
                                        var message = string.Format(messageTemplate, admission.Student.RegistrationNo,
                                            admission.Student.StudentName, admission.Student.FName,
                                            model.time.Date.ToString("hh:mm tt"), model.time.Date.ToString("dd-MM-yy"),
                                            attendanceType.ToString(), admission.ClassSection.Class.ClassName, 1);
                                        messageList.Add(new SendSmsModel
                                        {
                                            Mask = string.Empty,
                                            Message = message,
                                            /*MobileNo = new[] {smsNo}*/
                                            MobileNo = new string[] { smsNo },
                                            testMode = false, Unicode = false
                                        });
                                    }
                                    catch
                                    {
                                    }
                            }
                        }
                    }

                if (messageList.Count > 0)
                    try
                    {
                        var messages = messagingSystem;
                        foreach (var sms in messageList)
                            await messages.sendMessage(sms.Message, sms.MobileNo, sms.Unicode);
                    }
                    catch
                    {
                    }
            }
            catch (Exception aa)
            {
                return GetErrorResult(aa);
            }


            return Ok(true);
        }
    }

    public class AddAttendanceModel
    {
        public string type { get; set; }
        public int Id { get; set; }
        public DateTime time { get; set; }
        public bool sendMessage { get; set; }
        public string userId { get; set; }
        public int campusId { get; set; }
    }

    public class LocalImpressions
    {
        public string Name { get; set; }
        public string Impression { get; set; }
        public string Campus { get; set; }
        public string Class { get; set; }
        public string AType { get; set; }
        public string Designation { get; set; }
        public int CampusId { get; set; }
        public string Section { get; set; }
        public int BasicId { get; set; }
        public byte[] Photo { get; set; }
    }
}