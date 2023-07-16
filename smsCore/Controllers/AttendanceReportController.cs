using System.ComponentModel;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using smsCore.Data;
using smsCore.Data.Helpers;
using Syncfusion.EJ2.Navigations;
using Utilities;

namespace smsCore.Controllers
{
    public class AttendanceReportController : BaseController
    {
        // GET: AttendanceReport
        // private readonly SchoolEntities db = new SchoolEntities();
        private readonly SchoolEntities db;
        private readonly CurrentUser CurrentUser;
        private readonly DatabaseAccessSql dba;
        private readonly UtilityFunctions UtilityFunctions;
        public AttendanceReportController(SchoolEntities _db, CurrentUser currentUser, DatabaseAccessSql _dba, UtilityFunctions _UtilityFunctions)
        {
            db = _db;
            CurrentUser = currentUser;
            dba = _dba;
            UtilityFunctions = _UtilityFunctions;
        }

        public IActionResult MonthlyAttendance(int classId = -1, int campus = 0)
        {
            return View();
        }

        
        public IActionResult AttendanceSheetByClass()  
        {
            return View();
        }

        public JsonResult GetAttendanceSheetByClass(string date, int campus = 0, string rdobyMonth = "")
        {
            var dt = DateTimeHelper.ConvertDate(date);//dd/MM/yyyy
            var camp = db.Campuses.Find(campus);
            var campName = "";
            if (camp != null)
            {
                campName = camp.CampusName;
            }
            else
            {
                var dat = new DataTable();
                var classArray = dat.AsEnumerable()
                    .Select(s => int.TryParse(s["ClassID"].ToString().Trim(), out var r) ? r : 0).Distinct()
                    .ToArray();

                var classNames = db.Classes.AsNoTracking().Where(w => classArray.Contains(w.ID))
                    .Select(s => s.ClassName).ToList();
              //  var sheetList = new JsonResult {Data = dat, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
               // return sheetList;
                var sheetList = Json(new
                {
                    Data = dat,
                    

                });
                return sheetList;
            }

            var criteria = "";
            if (rdobyMonth == "false")
                criteria = "AND DATEPART(Day, dbo.StudentAttendences.AttendanceDate)=" + dt.Day + "";

            var c = db.Classes.Select(s => s.ClassName).ToArray();
            var colss = new string[c.Count()];
            var colsss = new string[c.Count()];
            var n = 0;
            foreach (var s in c)
            {
                colss[n] = "ISNULL([" + s + "],0) as [" + s + "]";
                colsss[n] = " [" + s + "]";
                n++;
            }
            var q =
                @"With d(Attendance, EntryDate, ClassName, Total, CampusName) as (SELECT StudentAttendanceTypes.AttendanceName AS Attendance, CONVERT(varchar(10), StudentAttendences.AttendanceDate, 103) AS EntryDate, Classes.ClassName, 
                         COUNT(StudentAttendences.AdmissionID) AS Total, Campuses.CampusName
                         FROM StudentAttendanceTypes INNER JOIN
                         StudentAttendences ON StudentAttendanceTypes.ID = StudentAttendences.AttendanceTypeID INNER JOIN
                         Admissions ON StudentAttendences.AdmissionID = Admissions.ID INNER JOIN
                         ClassSections ON Admissions.ClassSectionID = ClassSections.ID INNER JOIN
                         Classes ON ClassSections.ClassID = Classes.ID INNER JOIN
                         Campuses ON Admissions.CampuseID = Campuses.ID WHERE (Admissions.IsExpell = 0) AND DATEPART(month, dbo.StudentAttendences.AttendanceDate)=" +
                dt.Month + " " + criteria + " AND DATEPART(Year, dbo.StudentAttendences.AttendanceDate)=" +
                dt.Year + @"
                        GROUP BY StudentAttendanceTypes.AttendanceName, CONVERT(varchar(10), StudentAttendences.AttendanceDate, 103), Classes.ClassName, Campuses.CampusName HAVING dbo.Campuses.CampusName='" +
                campName + @"') 
                        select Attendance, EntryDate, CampusName, " + string.Join(",", colss) +
                " from d Pivot(Max(Total) for ClassName IN(" + string.Join(",", colsss) + ") ) as pt";

            DataTable tab = null;
            tab = dba.CreateTable(q);

            var atSheet =  Json(new
            { Data = tab.AsEnumerable() });
            return atSheet;

          

        }

        
        public ActionResult AttendancesOnLeave()
        {
            ViewBag.UserList = new SelectList(db.Users.ToList(), "Id", "UserName");
            ViewBag.AttendanceType = new SelectList(db.StudentAttendanceTypes, "ID", "AttendanceName");
            return View();
        }

        public JsonResult GetAttendancesOnLeaveReport(string user, string dates = "", string attendancetype = "")
        {
            string[] userID = {"0"};
            int[] attdntypeid = {0};
            try
            {
                userID[0] = user;
            }
            catch
            {
                userID = db.Users.Select(s => s.Id).ToArray();
            }

            try
            {
                attdntypeid[0] = int.Parse(attendancetype);
            }
            catch
            {
                attdntypeid = db.StudentAttendanceTypes.Select(s => s.ID).ToArray();
            }

            var date = DateTimeHelper.ConvertDate(dates);
            var data = db.StudentAttendences
                .Where(w => userID.Contains(w.UserID) && attdntypeid.Contains(w.AttendanceTypeID) && w.manaual != false)
                .Where(w =>  w.AttendanceDate.Date == date.Date).ToList().Select(s => new
                {
                    s.Admission.Student.RegistrationNo,
                    s.Admission.Student.StudentName,
                    AttendanceDate=s.AttendanceDate.ToString("dd/MMM/yyyy"),
                    Attendancetype = s.StudentAttendanceType.AttendanceName.Trim(),
                    Class = s.Admission.ClassSection.Class.ClassName.Trim() + "(C-" + s.Admission.CampuseID + ")",
                    Section = s.Admission.ClassSection.Section.SectionName.Trim(),
                    UserName = s.AspNetUser.UserName.Trim()
                }).ToList();
            var stdList =Json( new  
                        {Data = data});
            return stdList;
        }

        
        public ActionResult DailyAttendanceTime()
        {
            ViewBag.UserList = new SelectList(db.Users.ToList(), "Id", "UserName");
            ViewBag.AttendanceType = new SelectList(db.StudentAttendanceTypes, "ID", "AttendanceName");
            return View();
        }

        public JsonResult GetDailyAttendanceTimeDatatable(int classId = 0, int campusId = 0, int sectionId = 0,
            string dates = "", string attendancetype = "", string checktime = "", string timefrom = "",
            string timeto = "")
        {
            int[] ids = { };
            var clsID = classId == 0 ? db.Classes.Select(s => s.ID).ToArray() : new[] {classId};
            var campid = campusId == 0 ? db.Campuses.Select(s => s.ID).ToArray() : new[] {campusId};
            var secid = sectionId == 0
                ? db.ClassSections.Select(s => s.ID).ToArray()
                : new[]
                {
                    db.ClassSections.Where(w => w.ClassID == classId && w.SectionID == sectionId).Select(s => s.ID)
                        .FirstOrDefault()
                };
            //int[] secid = new int[] { };
            if (sectionId == 0)
            {
                if (classId > 0)
                    secid = db.ClassSections.Where(w => w.ClassID == classId).Select(s => s.ID).ToArray();
                else
                    secid = db.ClassSections.Select(s => s.ID).ToArray();
            }

            var data = new DataTable();
            var li = new Dictionary<string, object>();
            var criteria = "";
            if (checktime == "true")
            {
                var fromdates = DateTimeHelper.ConvertDate(timefrom);
                var todates = DateTimeHelper.ConvertDate(timeto);

                var fhour = fromdates.Hour;
                var fminiut = fromdates.Minute;
                var fsecond = fromdates.Second;
                var lhour = todates.Hour;
                var lminiut = todates.Minute;
                var lsecond = todates.Second;
                criteria = @"AND (CONVERT(time, StudentAttendences.AttendanceDate, 108) BETWEEN N'" +
                           fromdates.ToString("HH:mm:ss") + "' AND N'" + todates.ToString("HH:mm:ss") + "')";
            }

            int[] attID = {0};
            try
            {
                attID[0] = int.Parse(attendancetype);
            }
            catch
            {
                attID = db.StudentAttendanceTypes.Select(s => s.ID).ToArray();
            }

            var query =
                $@"SELECT TOP (100) PERCENT dbo.Students.StudentName, dbo.Students.FName, CONVERT(varchar(10), dbo.StudentAttendences.AttendanceDate, 103) AS EntryDate, dbo.StudentAttendences.AttendanceTypeID, 
                  dbo.Students.RegistrationNo AS AdmissionID, RIGHT(CONVERT(CHAR(20), dbo.StudentAttendences.AttendanceDate, 22), 11) AS PickTime, dbo.Classes.ClassName + ' ' + dbo.Sections.SectionName + '( C-' + CONVERT(varchar(2), 
                  dbo.Campuses.ID) + ')' AS ClassName, dbo.StudentAttendences.ID, dbo.StudentAttendanceTypes.AttendanceName
FROM     dbo.Admissions INNER JOIN
                  dbo.StudentAttendences ON dbo.Admissions.ID = dbo.StudentAttendences.AdmissionID INNER JOIN
                  dbo.Students ON dbo.Admissions.StudentID = dbo.Students.ID INNER JOIN
                  dbo.ClassSections ON dbo.Admissions.ClassSectionID = dbo.ClassSections.ID INNER JOIN
                  dbo.Classes ON dbo.ClassSections.ClassID = dbo.Classes.ID INNER JOIN
                  dbo.Campuses ON dbo.Admissions.CampuseID = dbo.Campuses.ID INNER JOIN
                  dbo.Sections ON dbo.ClassSections.SectionID = dbo.Sections.ID INNER JOIN
                  dbo.StudentAttendanceTypes ON dbo.StudentAttendences.AttendanceTypeID = dbo.StudentAttendanceTypes.ID
WHERE  (dbo.Admissions.IsExpell = 0) AND (dbo.Admissions.ClassSectionID IN ({string.Join(",", secid)})) AND (dbo.Admissions.CampuseID IN ({string.Join(",", campid)})) AND (CONVERT(Date, dbo.StudentAttendences.AttendanceDate, 103) = CONVERT(Date, '{DateTimeHelper.ConvertDate(dates).ToString("dd/MMM/yyyy")}', 103)) AND 
                  (dbo.StudentAttendences.AttendanceTypeID IN ({string.Join(",", attID)})) {criteria} ORDER BY AdmissionID, dbo.Classes.ID, EntryDate, PickTime";

            data = dba.CreateTable(query);
            var dList = Json(new
                {Data = data.AsEnumerable()});
            return dList;
        }

        
        public ActionResult AttendanceRecord()
        {
            ViewBag.AttendanceType = new SelectList(db.StudentAttendanceTypes, "ID", "AttendanceName");
            return View();
        }

        public JsonResult GetAttendanceRecordDatatable(string regno = "", string attendancetype = "",
            string checktime = "", string timefrom = "", string timeto = "", string checkdate = "",
            string datefrom = "", string dateto = "")
        {
            int[] ids = { };
            var camIds = CurrentUser.GetCampusIds();
            ids = UtilityFunctions.ParseAdmIDs(regno, camIds);
            var data = new DataTable();

            var criteria = "";
            if (checkdate == "true")
                criteria = "AND CONVERT(Date, StudentAttendences.AttendanceDate, 103)  >=((CONVERT(DATE, '" +
                           DateTimeHelper.ConvertDate(datefrom).Date.ToString("dd/MM/yyyy") +
                           "', 103))) AND CONVERT(Date, StudentAttendences.AttendanceDate, 103)  <=(CONVERT(DATE, '" +
                           DateTimeHelper.ConvertDate(dateto).Date.ToString("dd/MM/yyyy") + "', 103))";
            if (checktime == "true")
                criteria += @" AND (CONVERT(time, StudentAttendences.AttendanceDate, 108) BETWEEN N'" +
                            DateTime.Parse(timefrom).ToString("HH:mm:ss") + "' AND N'" +
                            DateTime.Parse(timeto).ToString("HH:mm:ss") + "')";

            int[] attID = {0};
            if (attendancetype == "0")
                attID = db.StudentAttendanceTypes.Select(s => s.ID).ToArray();
            else
                attID[0] = int.Parse(attendancetype);
            data = dba.CreateTable(
                @"SELECT        Students.StudentName, Students.FName, CONVERT(Date, StudentAttendences.AttendanceDate, 103) AS EntryDat, StudentAttendences.AttendanceTypeID, Students.RegistrationNo AS AdmissionID, 
                         RTRIM(StudentAttendanceTypes.AttendanceName) AS PresentClass, RIGHT(CONVERT(CHAR(20), StudentAttendences.AttendanceDate, 22), 11) AS PickTime, StudentAttendences.UserID, 
                         StudentAttendences.manaual, Classes.ClassName, Sections.SectionName, Campuses.ID as CampusID
FROM            Admissions INNER JOIN
                         StudentAttendences ON Admissions.ID = StudentAttendences.AdmissionID INNER JOIN
                         Students ON Admissions.StudentID = Students.ID INNER JOIN
                         StudentAttendanceTypes ON StudentAttendences.AttendanceTypeID = StudentAttendanceTypes.ID INNER JOIN
                         ClassSections ON Admissions.ClassSectionID = ClassSections.ID INNER JOIN
                         Classes ON ClassSections.ClassID = Classes.ID INNER JOIN
                         Sections ON ClassSections.SectionID = Sections.ID INNER JOIN
                         Campuses ON Admissions.CampuseID = Campuses.ID
                        WHERE (Admissions.IsExpell = 0) AND (Admissions.ID IN (" + string.Join(",", ids) +
                "))  AND (StudentAttendences.AttendanceTypeID IN (" + string.Join(",", attID) + "))" + criteria +
                " ORDER BY EntryDat, PickTime, AdmissionID");
            data.Columns.Add("EntryDate", typeof(string));
            var record = Json(new
                {Data = data.AsEnumerable()});
            return record;

         
        }

        
        public ActionResult ConsecutiveAttendance()
        {
            ViewBag.AttendanceType = new SelectList(db.StudentAttendanceTypes, "ID", "AttendanceName");
            return View();
        }

        public JsonResult GetConsecutiveAttendanceDatatable(int classId = 0, int campusId = 0, int sectionId = 0,
            string regno = "", string attendancetype = "")
        {
            int[] ids = { };
            var clsID = classId == 0 ? db.Classes.Select(s => s.ID).ToArray() : new[] {classId};
            var campid = campusId == 0 ? db.Campuses.Select(s => s.ID).ToArray() : new[] {campusId};
            var secid = sectionId == 0
                ? db.ClassSections.Select(s => s.ID).ToArray()
                : new[]
                {
                    db.ClassSections.Where(w => w.ClassID == classId && w.SectionID == sectionId).Select(s => s.ID)
                        .FirstOrDefault()
                };
            //int[] secid = new int[] { };
            if (sectionId == 0)
            {
                if (classId > 0)
                    secid = db.ClassSections.Where(w => w.ClassID == classId).Select(s => s.ID).ToArray();
                else
                    secid = db.ClassSections.Select(s => s.ID).ToArray();
            }

            var attendanceCode = attendancetype.Trim().ToLower() == "leave" ? "L" : "A";

            var li = new Dictionary<string, object>();
            li.Add("ReportType", attendanceCode == "L" ? "Leave" : "Absent");
            li.Add("Column", attendanceCode == "L" ? "CL" : "CA");
            var month = DateTime.Today.Month;
            var year = DateTime.Today.Year;
            object data = null;
            if (!string.IsNullOrEmpty(regno))
            {
                var idss = UtilityFunctions.ParseAdmIDs(regno, campid);
                data = db.Admissions.Where(w => w.IsExpell == false && idss.Contains(w.ID)).ToList().Select(s => new
                {
                    s.ID,
                    s.CampuseID,
                    AdmissionID = s.Student.RegistrationNo,
                    s.Student.StudentName,
                    s.Student.FName,
                    s.ClassSection.Class.ClassName,
                    s.ClassSection.Section.SectionName,
                    s.Campus.CampusName,
                    C3 = s.StudentAttendences.Where(w =>
                        w.AttendanceDate.Month == month && w.AttendanceDate.Year == year &&
                        w.StudentAttendanceType.Code == "A").Count(),
                    C2 = s.StudentAttendences.Where(w =>
                        w.AttendanceDate.Month == month && w.AttendanceDate.Year == year &&
                        w.StudentAttendanceType.Code == "L").Count()
                    //C1 = attendanceCode == "L" ? s.StudentAttendences.Where(w => w.AttendanceDate.Month == month && w.AttendanceDate.Year == year && w.StudentAttendanceType.Code == "L").OrderByDescending(o => o.AttendanceDate).FirstOrDefault() == null ? 0 : GetConsecutive(s.ID, s.StudentAttendences.Where(w => w.AttendanceDate.Month == month && w.AttendanceDate.Year == year && w.StudentAttendanceType.Code == "L").OrderByDescending(o => o.AttendanceDate).FirstOrDefault().AttendanceDate, "L") :
                    //s.StudentAttendences.Where(w => w.AttendanceDate.Month == month && w.AttendanceDate.Year == year && w.StudentAttendanceType.Code == "A").OrderByDescending(o => o.AttendanceDate).FirstOrDefault() == null ? 0 : GetConsecutive(s.ID, s.StudentAttendences.Where(w => w.AttendanceDate.Month == month && w.AttendanceDate.Year == year && w.StudentAttendanceType.Code == "A").OrderByDescending(o => o.AttendanceDate).FirstOrDefault().AttendanceDate, "A")
                }).Where(w => w.C3 > 0 || w.C2 > 0).Select(s => new
                {
                    s.ID,
                    s.StudentName,
                    s.FName,
                    s.AdmissionID,
                    ClassName = s.ClassName + "-" + s.SectionName.Trim() + "(C-" + s.CampuseID + ")",
                    s.C2,
                    s.C3,
                    C1 = attendanceCode == "L"
                        ? GetConsecutive(s.ID, DateTime.Today, "L")
                        : GetConsecutive(s.ID, DateTime.Today, "A")
                }).Where(w => w.C1 > 0).ToList();
            }
            else
            {
                data = db.Admissions
                    .Where(w => w.IsExpell == false && secid.Contains(w.ClassSectionID) && campid.Contains(w.CampuseID))
                    .ToList().Select(s => new
                    {
                        s.ID,
                        s.CampuseID,
                        AdmissionID = s.Student.RegistrationNo,
                        s.Student.StudentName,
                        s.Student.FName,
                        s.ClassSection.Class.ClassName,
                        s.ClassSection.Section.SectionName,
                        s.Campus.CampusName,
                        C3 = s.StudentAttendences.Where(w =>
                            w.AttendanceDate.Month == month && w.AttendanceDate.Year == year &&
                            w.StudentAttendanceType.Code == "A").Count(),
                        C2 = s.StudentAttendences.Where(w =>
                            w.AttendanceDate.Month == month && w.AttendanceDate.Year == year &&
                            w.StudentAttendanceType.Code == "L").Count()
                        //C1 = attendanceCode == "L" ? s.StudentAttendences.Where(w => w.AttendanceDate.Month == month && w.AttendanceDate.Year == year && w.StudentAttendanceType.Code == "L").OrderByDescending(o => o.AttendanceDate).FirstOrDefault() == null ? 0 : GetConsecutive(s.ID, s.StudentAttendences.Where(w => w.AttendanceDate.Month == month && w.AttendanceDate.Year == year && w.StudentAttendanceType.Code == "L").OrderByDescending(o => o.AttendanceDate).FirstOrDefault().AttendanceDate, "L") :
                        //s.StudentAttendences.Where(w => w.AttendanceDate.Month == month && w.AttendanceDate.Year == year && w.StudentAttendanceType.Code == "A").OrderByDescending(o => o.AttendanceDate).FirstOrDefault() == null ? 0 : GetConsecutive(s.ID, s.StudentAttendences.Where(w => w.AttendanceDate.Month == month && w.AttendanceDate.Year == year && w.StudentAttendanceType.Code == "A").OrderByDescending(o => o.AttendanceDate).FirstOrDefault().AttendanceDate, "A")
                    }).Where(w => w.C3 > 0 || w.C2 > 0).Select(s => new
                    {
                        s.ID,
                        s.StudentName,
                        s.FName,
                        s.AdmissionID,
                        ClassName = s.ClassName + "-" + s.SectionName.Trim() + "(C-" + s.CampuseID + ")",
                        s.C2,
                        s.C3,
                        C1 = GetConsecutive(s.ID, DateTime.Today, attendanceCode)
                    }).Where(w => w.C1 > 0).ToList();
            }

            //   var conList = new JsonResult {Data = data};
            var conlist = Json(new
            {
                Data = data
            });
            return conlist;
        }

        private int GetConsecutive(int admID, DateTime date, string AttendanceCode)
        {
            var consectives = 0;
            var month = date.Month;
            var year = date.Year;
            var day = date.Day;
            var q =
                @"SELECT        dbo.StudentAttendences.AttendanceDate, dbo.StudentAttendences.AdmissionID, dbo.StudentAttendanceTypes.Code, DAY(dbo.StudentAttendences.AttendanceDate) AS day
FROM            dbo.StudentAttendences INNER JOIN
                         dbo.StudentAttendanceTypes ON dbo.StudentAttendences.AttendanceTypeID = dbo.StudentAttendanceTypes.ID
WHERE     dbo.StudentAttendences.AdmissionID=" + admID + " AND   (MONTH(dbo.StudentAttendences.AttendanceDate) = " +
                month + ") AND (YEAR(dbo.StudentAttendences.AttendanceDate) = " + year +
                ") AND (dbo.StudentAttendanceTypes.Code = '" + AttendanceCode + "') ";
            var tabAt = dba.CreateTable(q);
            if (tabAt.Rows.Count > 0)
                for (var i = day; i > 0; i--)
                {
                    var today = tabAt.AsEnumerable().Where(w => w["day"].ToString() == i.ToString()).ToList();
                    if (today.Count > 0)
                    {
                        consectives++;
                    }
                    else
                    {
                        var thisDay = new DateTime(year, month, i);
                        if (thisDay.DayOfWeek != DayOfWeek.Sunday) break;
                    }
                }

            //var attendance = objdb.StudentAttendences.Where(w => w.AttendanceDate.Month == month && w.AttendanceDate.Year == year && w.AdmissionID == admID && w.StudentAttendanceType.Code == AttendanceCode).ToList();
            //for (int i = day; i > 0; i--)
            //{
            //    if (attendance.Where(w => w.AttendanceDate.Day == day).FirstOrDefault() != null)
            //    {
            //        MessageBox.Show("Day: '" + day + "' Total Attendance with Code '" + AttendanceCode + "' this attendance attribute '" + attendance.Where(w => w.AttendanceDate.Day == day).FirstOrDefault().StudentAttendanceType.AttendanceName);
            //        consectives++;
            //    }
            //    else break;
            //}
            return consectives;
        }

        
        public ActionResult MonthlyAttendanceRegister()
        {
            return View();
        }

        
        public ActionResult ShortLeave()
        {
            var staff = db.tbl_Employee.Where(w => w.LeavedStaffs.Count < 1)
                .Select(s => new {ID = s.Id, StaffName = s.employeeName + " - " + s.employeeCode})
                .Select(s => new {s.ID, s.StaffName}).ToList();
            //var staff = db.tbl_Employee.Select(s => new { s.Id, s.employeeName }).ToList();
            staff.Insert(0, new {ID = 0, StaffName = "---select---"});
            var list = new SelectList(staff, "ID", "StaffName");

            ViewBag.stafflist = list;

            return View();
        }

        public JsonResult GetShortLeaveDatatable(string StaffId = "", string checkmonth = "", string monthselect = "",
            string checkdate = "", string datefrom = "", string dateto = "")
        {
            int[] staffid = { 0 };

            if (StaffId != null)
                staffid = new[] { int.Parse(StaffId) };
            else if (StaffId == null || StaffId == "0")
                staffid = db.tbl_Employee.ToList().Select(s => s.Id).ToArray();

            var datefrom1 = DateTimeHelper.ConvertDate(datefrom);
            var dateto1 = DateTimeHelper.ConvertDate(dateto);
            var monthdt = DateTimeHelper.ConvertDate(monthselect);
            int month = monthdt.Month;
            int year = monthdt.Year;
            //
            var data = db.EmployeeShortLeaves.Where(w => staffid.Contains(w.StaffID));
            if (checkdate == "true")
                data = data.Where(w =>
                        w.OutTime.Date >= datefrom1 &&
                        w.OutTime.Date <= dateto1);

            if (checkmonth == "true")
                data = data.Where(w =>
                    w.OutTime.Year == year &&
                    w.OutTime.Month == month);

            var result = data.ToList().Select(s =>
                          new
                          {
                              s.ID,
                              s.tbl_Employee.employeeName,
                              OutTime = s.OutTime.ToString("dd/MM/yyyy"),
                              LeaveTime = s.OutTime.ToString("hh:mm tt"),
                              ArrivalTime = s.InTime == null ? "" : s.InTime.Value.ToString("hh:mm tt")
                          });
            // data = data.Where(w => w.OutTime.Year ==DateTime.Parse(monthselect).Year && w.OutTime.Date.Month == DateTime.Parse(monthselect).Date.Month).Select(s => new { s.ID, s.employeeName, date = s.OutTime.Date.ToString("dd-MMM-yyyy"), LeaveTime = s.OutTime.ToString("hh:mm tt"), ArrivalTime = s.InTime == null ? "" : s.InTime.Value.ToString("hh:mm tt") });
            var staffA = Json(new
            {
                Data = result,
               
            });
            return staffA;
        }

        
        public ActionResult StaffAttendanceRecord()
        {
            var staff = db.tbl_Employee.Where(w => w.LeavedStaffs.Count < 1).ToList()
                .Select(s => new {ID = s.Id, StaffName = s.employeeName + " s/d/o " + s.FatherName}).ToList()
                .Select(s => new {s.ID, s.StaffName}).ToList();
            //var staff = db.tbl_Employee.Select(s => new { s.Id, s.employeeName }).ToList();
            staff.Insert(0, new {ID = 0, StaffName = "---select---"});
            var list = new SelectList(staff, "ID", "StaffName");

            ViewBag.stafflist = list;

            return View();
        }

        public JsonResult GetStaffAttendanceRecordDatatable(string StaffId = "", string checkmonth = "",
            string monthselect = "", string checkdate = "", string datefrom = "", string dateto = "")
        {
            int[] staffid = {0};

            if (StaffId != null)
                staffid = new[] {int.Parse(StaffId)};
            else if (StaffId == null || StaffId == "0")
                staffid = db.tbl_Employee.Where(w => w.LeavedStaffs.Count < 1).Select(s => s.Id).ToArray();


            var criteria = "";
            //string shortcriteria = "";
            var data = db.EmployeeAttendances.Where(w => staffid.Contains(w.StaffID)).Select(s =>
                    new {s.ID, s.tbl_Employee.employeeName, s.AttendanceDate, s.EmployeeAttendanceType.AttendanceName})
                .ToList();
            if (checkdate == "true")
               criteria = "AND CONVERT(Datetime, attendanceDate, 103)  >=((CONVERT(DATETIME, '" +
                           DateTimeHelper.ConvertDate(datefrom) +
                           "', 103))) AND CONVERT(Datetime, attendanceDate, 103)  <=(CONVERT(DATETIME, '" +
                           DateTimeHelper.ConvertDate(datefrom) + "', 103)) ";

            if (checkmonth == "true")
                criteria = "AND Month(attendancedate)=" + DateTimeHelper.ConvertDate(monthselect).Month +
                           " AND Year(attendancedate)=" + DateTimeHelper.ConvertDate(monthselect).Year + "";
             var q =
                @"WITH d(ID, StaffName, typee,date,ForMonth) AS (SELECT tbl_Employee.Id, tbl_Employee.employeename as StaffName, EmployeeAttendanceType.AttendanceName,attendancedate,CONVERT(CHAR(4), attendancedate, 100) + CONVERT(CHAR(4), attendancedate, 120)
FROM tbl_Employee INNER JOIN EmployeeAttendance ON tbl_Employee.Id = EmployeeAttendance.StaffID INNER JOIN
EmployeeAttendanceType ON EmployeeAttendance.AttendanceTypeID = EmployeeAttendanceType.ID Where tbl_Employee.Id in (" +
                string.Join(",", staffid) + ")  " + criteria +
                ") SELECT *,0 as Total FROM d pivot(count(date) for typee in(Present,Absent,Leave,[Late coming],[Short leave]))  as pt";
            var tab = dba.CreateTable(q);

            string[] exCols = {"ID", "StaffName", "Total"};
            for (var i = 0; i < tab.Rows.Count; i++)
            {
                double total = 0;
                for (var j = 0; j < tab.Columns.Count; j++)
                    try
                    {
                        if (!exCols.Contains(tab.Columns[j].ColumnName))
                            total += Convert.ToDouble(tab.Rows[i][j].ToString());
                    }
                    catch
                    {
                    }

                tab.Rows[i]["Total"] = total;
                var dt = DateTime.Parse(tab.Rows[i]["ForMonth"].ToString());
                //MessageBox.Show(dt.ToString());
                var StaffID = int.Parse(tab.Rows[i]["ID"].ToString());
                //string qq = "SELECT COUNT(ID) AS Expr1 FROM ShortLeave WHERE (StaffID =" + tab.Rows[i]["ID"].ToString() + ") AND (" + shortcriteria + ") GROUP BY StaffID";
                //if (rdoMonth.Checked)
                tab.Rows[i]["Short leave"] = db.EmployeeShortLeaves.Where(w => w.StaffID == StaffID)
                    .Where(w => w.OutTime.Month == dt.Month && w.OutTime.Year == dt.Year).ToList().Count();
                
            }

            var StafRec = Json(new
            { Data = data.AsEnumerable() });
            return StafRec;
        }

        
        public ActionResult DailyAttendanceTimeVeiwer()
        {
            var staff = db.tbl_Employee.Where(w => w.LeavedStaffs.Count < 1).ToList()
                .Select(s => new {ID = s.Id, StaffName = s.employeeName + " s/d/o " + s.FatherName}).ToList()
                .Select(s => new {s.ID, s.StaffName}).ToList();
            //var staff = db.tbl_Employee.Select(s => new { s.Id, s.employeeName }).ToList();
            var list = new SelectList(staff, "ID", "StaffName");

            ViewBag.stafflist = list;
            return View();
        }

        public JsonResult GetDailyAttendanceTimeVeiwerDatatable(string staffId = "", string attendancetype = "",
            string checktime = "", string timefrom = "", string timeto = "", string checkdate = "",
            string datefrom = "", string dateto = "")
        {
            int[] ids = { };
            int[] StaffIDs = { };
            var stf = new List<tbl_Employee>();
            int[] attendanceid = {0};
            try
            {
                attendanceid[0] = int.Parse(attendancetype);
            }
            catch
            {
                attendanceid = db.EmployeeAttendanceTypes.Select(s => s.ID).ToArray();
            }

            if (staffId != null && int.TryParse(staffId, out var id))
                ids = new[] {id};
            else
                ids = db.tbl_Employee.Where(w => w.LeavedStaffs.Count == 0).Select(s => s.Id).ToArray();

            var c = "";
            if (checkdate == "true")
                c = "from " + DateTimeHelper.ConvertDate(datefrom) + " to " + DateTimeHelper.ConvertDate(dateto);
            c = "Attendance History " + c;
            var criteria = "";
            if (checktime == "true")
            {
                var fhour = DateTime.Parse(timefrom).Hour;
                var fminiut = DateTime.Parse(timefrom).Minute;
                var fsecond = DateTime.Parse(timefrom).Second;
                var lhour = DateTime.Parse(timeto).Hour;
                var lminiut = DateTime.Parse(timeto).Minute;
                var lsecond = DateTime.Parse(timeto).Second;

                var timefroms = DateTime.Parse(timefrom).TimeOfDay;
                var timeTos = DateTime.Parse(timeto).TimeOfDay;

                criteria = @"AND (CONVERT(time, EmployeeAttendance.AttendanceDate, 108) BETWEEN N'" +
                           DateTime.Parse(timefrom).ToString("HH:mm:ss") + "' AND N'" +
                           DateTime.Parse(timeto).ToString("HH:mm:ss") + "')";
            }

            if (checkdate == "true")
                criteria = "AND CONVERT(Date, EmployeeAttendance.AttendanceDate, 103)  >=((CONVERT(DATE, '" +
                           datefrom +
                           "', 103))) AND CONVERT(Date, EmployeeAttendance.AttendanceDate, 103)  <=(CONVERT(DATE, '" +
                           dateto + "', 103))";
             var q =
                @"SELECT        tbl_Employee.employeeName AS StudentName, tbl_Employee.FatherName AS FName, CONVERT(Varchar(10), EmployeeAttendance.AttendanceDate, 103) AS EntryDat, EmployeeAttendance.AttendanceTypeID, tbl_Employee.Id AS AdmissionID, 
                         RIGHT(CONVERT(CHAR(20), EmployeeAttendance.AttendanceDate, 22), 11) AS PickTime, DATEPART(Hour, EmployeeAttendance.AttendanceDate) AS h, DATEPART(Minute, EmployeeAttendance.AttendanceDate) AS m, 
                         DATEPART(Second, EmployeeAttendance.AttendanceDate) AS s, CONVERT(varchar(2), EmployeeAttendance.UserID) AS Remarks, EmployeeAttendanceType.AttendanceName as Misc
FROM            EmployeeAttendanceType INNER JOIN
                         EmployeeAttendance ON EmployeeAttendanceType.ID = EmployeeAttendance.AttendanceTypeID INNER JOIN
                         tbl_Employee ON EmployeeAttendance.StaffID = tbl_Employee.Id
                        WHERE (EmployeeAttendance.AttendanceTypeID IN (" + string.Join(",", attendanceid) +
                ")) AND (tbl_Employee.Id IN (" + string.Join(",", ids) + "))  " + criteria +
                " ORDER BY AdmissionID, CONVERT(Date, EmployeeAttendance.AttendanceDate, 103)";

            var dtab = dba.CreateTable(q);
            dtab.Columns.Add("col7");
            dtab.Columns.Add("EntryDate", typeof(string));
            var atTime = db.SMSApplicationProperties.Where(w => w.PropertyName == "ATstaff").DefaultIfEmpty()
                .FirstOrDefault().PropertyValue.Trim();
            var sartTime = (DateTime) new DateTimeConverter().ConvertFrom(atTime);
            var athour = sartTime.Hour;
            var atminut = sartTime.Minute;
            var atsecond = sartTime.Second;

            var attendancetime = athour * 3600 + atminut * 60 + atsecond;

            foreach (DataRow dr in dtab.Rows)
            {
                dr["EntryDate"] = dr["EntryDat"].ToString();

                if (dr["Misc"].ToString().Trim().ToLower() == "absent" ||
                    dr["Misc"].ToString().Trim().ToLower() == "leave")
                {
                    dr["PickTime"] = "";
                    dr["col7"] = "By ";
                }
                else
                {
                    var reachhour = int.Parse(dr["h"].ToString() == null ? "0" : dr["h"].ToString());
                    var reachminute = int.Parse(dr["m"].ToString() == null ? "0" : dr["m"].ToString());
                    var reachsecond = int.Parse(dr["s"].ToString() == null ? "0" : dr["s"].ToString());

                    int tminut = 0, thour = 0, tsecond = 0;
                    if (atsecond > reachsecond)
                    {
                        atminut--;
                        reachsecond += 60;
                        tsecond = reachsecond - atsecond;
                    }
                    else
                    {
                        tsecond = reachsecond - atsecond;
                    }

                    if (atminut > reachminute)
                    {
                        athour--;
                        reachminute += 60;
                        tminut = reachminute - atminut;
                    }
                    else if (atminut > reachminute)
                    {
                        reachminute += 60;
                    }
                    else
                    {
                        tminut = reachminute - atminut;
                    }

                    thour = reachhour - athour;

                    dr["col7"] = thour + ":" + tminut + ":" + tsecond;
                }
            }

            var Alist = Json( new{
                Data = dtab.AsEnumerable()
            });
            return Alist;
        }
    }
}

