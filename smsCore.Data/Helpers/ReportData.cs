using Microsoft.EntityFrameworkCore;
using Models;
using SchoolManagementSystem.Helpers;
using System.Data;
using System.Data.SqlClient;
using Utilities;
using NetBarcode;

namespace smsCore.Data.Helpers
{
    public class LateFeeStructure
    {
        public int CampusId { get; set; }
        public string CampusName { get; set; }
        public bool ApplyDaily { get; set; }
        public decimal LateFeeAmount { get; set; }
    }

    public class ReportData
    {
        private readonly SchoolEntities db ;
        private readonly DatabaseAccessSql dba;
        private readonly ClsBussinessSetting setting;
        private readonly UtilityFunctions UtilityFunctions;
        private readonly CurrentUser _user;
        private readonly AttendanceHelper _attendanceHelper;

        public ReportData(SchoolEntities _db, DatabaseAccessSql _dba,ClsBussinessSetting _setting, UtilityFunctions utilityFunctions,CurrentUser user, AttendanceHelper attendanceHelper)
        {
            db = _db;
            dba = _dba;
            setting = _setting;
            UtilityFunctions = utilityFunctions;
            _user = user;
            _attendanceHelper = attendanceHelper;
        }
        public object StudentCardsPrint(int campusId = -1, string RegNo = "", int Class = -1, int Section = -1,
            int Validity = 12, string defaultImage = "", string byclass = "", string byregno = "", bool trans = false)
        {
            int[] Ids = { };

            var camIds = campusId == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] {campusId};
            var clsIds = Class == -1 ? db.Classes.Select(s => s.ID).ToArray() : new[] {Class};
            var sectionIds = Section == -1 ? db.Sections.Select(s => s.ID).ToArray() : new[] {Section};
            if (!string.IsNullOrEmpty(RegNo) && byregno == "true")
                Ids = UtilityFunctions.ParseAdmIDs(RegNo, camIds, false);

            else if (byclass == "true")
                Ids = db.Admissions
                    .Where(w => camIds.Contains(w.CampuseID) && w.IsExpell == false &&
                                clsIds.Contains(w.ClassSection.ClassID) &&
                                sectionIds.Contains(w.ClassSection.SectionID)).Select(s => s.ID).ToArray();
            else if (trans)
                Ids = db.Admissions.Where(w => w.Student.StudentsTransports.Count() > 0).Select(s => s.ID).ToArray();
            else
                Ids = db.Admissions.Where(w => camIds.Contains(w.CampuseID) && w.IsExpell == false).Select(s => s.ID)
                    .ToArray();

            var Signature = db.SchoolLogoes.Where(w => w.ID == 3).FirstOrDefault();
            var defaultImageByte = UtilityFunctions.ImageToByteArraybyImageConverter(System.Drawing.Image.FromFile(defaultImage));
            var data = db.Admissions.Where(w => Ids.Contains(w.ID)).ToList().Select(s => new
            {
                AdmissionID = s.Student.RegistrationNo,
                s.Student.StudentName,
                s.ClassSection.Class.ClassName,
                s.Student.FName,
                PostalAddress = s.Student.StudentsTransports.Count() == 0 ? "No" : "Yes",
                EntryDate = DateTime.Now.AddMonths(Validity),
                MobileNo = s.Student.StudentMobiles.Where(w => w.IsDefault).Select(ss => ss.MobileNo).DefaultIfEmpty()
                    .FirstOrDefault(),
                StudentImage = s.Student.StudentPhotos.Select(ss => ss.StudentImage).FirstOrDefault() == null
                    ? defaultImageByte
                    : s.Student.StudentPhotos.Select(ss => ss.StudentImage).FirstOrDefault(),
                BarCode = new Barcode(s.Student.RegistrationNo.ToString(), NetBarcode.Type.Code128, false, 95, 22, null).GetByteArray(),
                //BarCode = RenderBarcode(s.Student.RegistrationNo.ToString()),
                c30 = Signature
            });
            return data;
            //return GenerateReport("~/Reports/EmployeeCard.rdlc", data);
        }

        public object EmployeeCardsPrint(int campusId = -1, string empcode = "", int Validity = 12, string defaultImage = "")
        {
            //decimal Ids = 0;
            var camIds = campusId == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] {campusId};
            var emp = db.tbl_Employee.Where(w => camIds.Contains(w.CampusID) && (w.LeavedStaffs==null || w.LeavedStaffs.Count()==0));

            if (!string.IsNullOrEmpty(empcode))
            {
                emp = emp.Where(w => w.employeeCode == empcode);
            }
            //var Signature = db.SchoolLogoes.Where(w => w.ID == 3).FirstOrDefault();
            var defaultImageByte = UtilityFunctions.ImageToByteArraybyImageConverter(System.Drawing.Image.FromFile(defaultImage));
            var data = emp.OrderBy(o => o.Id).ToList().Select(s => new
            {
                AdmissionID = s.Id,
                StaffName = s.employeeName,
                FName = s.FatherName,
                PostalAddress = s.address,
                ValidUpto = DateTime.Now.AddMonths(Validity),
                RegistrationNo = s.employeeCode,
                StudentImage = s.Photo == null ? defaultImageByte : s.Photo,
                AdmissionDate = s.joiningDate.Value.ToString("dd MMM yyyy"),
                MobileNo = s.mobileNumber,
                Col7 = s.tbl_Designation.designationName,
                BarCode = new Barcode(s.employeeCode.ToString(),NetBarcode.Type.Code128, false, 95, 22, null).GetByteArray()
            });

            return data;
        }

        public object ServiceCardsPrint(int campusId = -1, string Empcode = "", int Validity = 12, string defaultImage = "")
        {
            decimal Ids = 0;
            var camIds = campusId == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] { campusId };
            var emp = db.tbl_Employee.Where(w => camIds.Contains(w.CampusID) && (w.LeavedStaffs==null || w.LeavedStaffs.Count()==0));

            if (!string.IsNullOrEmpty(Empcode))
            {
                emp = emp.Where(w => w.employeeCode == Empcode);
            }

            var defaultImageByte = UtilityFunctions.ImageToByteArraybyImageConverter(System.Drawing.Image.FromFile(defaultImage));
            var data = emp.OrderBy(o=>o.Id).ToList().Select(s => new
            {
                AdmissionID = s.Id,
                StaffName = s.employeeName,
                FName = s.FatherName,
                Status = s.tbl_Designation == null ? string.Empty : s.tbl_Designation.designationName,
                c1 = s.CNIC,
                PostalAddress = s.address,
                ValidUpto = DateTime.Now.AddMonths(Validity),
                Col7 = s.employeeCode,
                StudentImage = s.Photo == null ? defaultImageByte : s.Photo,
                DueDate = s.joiningDate.Value.ToString("dd MMM yyyy"),
                MobileNo = s.mobileNumber
            });
            return data;
        }

        public object BirthCertificate(string RegNo = "", string remarks = "", string defaultImage = "")
        {
            var camIds = _user.GetCampusIds();
            var ids = UtilityFunctions.ParseAdmIDs(RegNo, camIds);
            var defaultImageByte = UtilityFunctions.ImageToByteArraybyImageConverter(System.Drawing.Image.FromFile(defaultImage));
            var data = db.Admissions
                .Where(w => camIds.Contains(w.CampuseID) && ids.Contains(w.ID) && w.IsExpell == false).ToList().Select(s => new
                {
                    AdmissionID = s.Student.RegistrationNo,
                    s.Student.StudentName,
                    s.Student.FName,
                    s.Session,
                    s.Student.PostalAddress,
                    s.Student.DOB,
                    s.Student.AdmittedClass,
                    PresentClass = s.ClassSection.Class.ClassName,
                    s.Student.AdmissionDate,
                    StudentImage = s.Student.StudentPhotos.Select(ss => ss.StudentImage).FirstOrDefault() == null
                        ? defaultImageByte
                        : s.Student.StudentPhotos.Select(ss => ss.StudentImage).FirstOrDefault()
                }).OrderBy(o => o.AdmissionID).Select(s => new
                {
                    AdmissionID = s.AdmissionID.ToString(),
                    s.AdmittedClass,
                    Col7 = s.DOB.ToString("dd/MM/yyyy"),
                    DOB = WrittenNumerics.DateToWritten(s.DOB),
                    s.FName,
                    s.Session,
                    s.PostalAddress,
                    ClassName = s.PresentClass,
                    s.StudentName,
                    AdmissionDate = s.AdmissionDate == null
                        ? "00/00/0000"
                        : Convert.ToDateTime(s.AdmissionDate).ToString("dd/MM/yyyy"),
                    s.StudentImage,
                    Remarks = remarks
                }).OrderBy(o => int.Parse(o.AdmissionID));
            return data;
        }

        //Registration Form Report
        public object RegistrationForm(int campusId = -1, string RegNo = "", int Class = -1, int Section = -1,
            string defaultImage = "", string byclass = "", string byregno = "")
        {
            int[] Ids = { };

            var camIds = campusId == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] {campusId};
            var clsIds = Class == -1 ? db.Classes.Select(s => s.ID).ToArray() : new[] {Class};
            var sectionIds = Section == -1 ? db.Sections.Select(s => s.ID).ToArray() : new[] {Section};

            if (!string.IsNullOrEmpty(RegNo) && byregno == "true")
                Ids = UtilityFunctions.ParseAdmIDs(RegNo, camIds, false);

            else if (byclass == "true")
                Ids = db.Admissions
                    .Where(w => camIds.Contains(w.CampuseID) && w.IsExpell == false &&
                                clsIds.Contains(w.ClassSection.ClassID) &&
                                sectionIds.Contains(w.ClassSection.SectionID)).Select(s => s.ID).ToArray();
            else
                Ids = db.Admissions.Where(w => camIds.Contains(w.CampuseID) && w.IsExpell == false).Select(s => s.ID).ToArray();
            var defaultImageByte = UtilityFunctions.ImageToByteArraybyImageConverter(System.Drawing.Image.FromFile(defaultImage));
            var data = db.Admissions.Where(w => Ids.Contains(w.ID)).ToList().Select(s => new
            {
                AdmissionID = s.Student.RegistrationNo,
                s.Student.StudentName,
                s.Student.FName,
                s.Session,
                c4 = s.Student.FatherProfession,
                s.Student.PermenantAddress,
                s.Student.PostalAddress,
                d1= s.Student.Domicile,
                s.Student.DOB,
                University = s.Student.LastIntitution,
                c1 = s.Student.CNIC,
                c2 = s.Student.StudentCNIC,
                MobileNo = s.Student.StudentMobiles.Where(w => w.IsDefault).Select(ss => ss.MobileNo).DefaultIfEmpty()
                    .FirstOrDefault(),
                PresentClass = s.ClassSection.Class.ClassName,
                StudentImage = s.Student.StudentPhotos.Select(ss => ss.StudentImage).FirstOrDefault() == null
                    ? defaultImageByte
                    : s.Student.StudentPhotos.Select(ss => ss.StudentImage).FirstOrDefault(),
                DriverName = s.Student.StudentsTransports.Select(ss => ss.Driver.DriverName).DefaultIfEmpty()
                    .FirstOrDefault(),
                TripNumber = s.Student.StudentsTransports.Select(ss => ss.TripNumber).DefaultIfEmpty().FirstOrDefault(),
                TimeTo = s.Student.StudentsTransports.Select(ss => ss.Closed).DefaultIfEmpty().FirstOrDefault(),
                TimeFrom = s.Student.StudentsTransports.Select(ss => ss.Started).DefaultIfEmpty().FirstOrDefault(),
                c3 = s.Student.StudentsTransports.Select(ss => ss.Fare).DefaultIfEmpty().FirstOrDefault(),
                PickPoint = s.Student.StudentsTransports.Select(ss => ss.PickPoint).DefaultIfEmpty().FirstOrDefault(),
                c5 = s.StudentPreviousEducations.Select(ss => ss.PreviousBoard).DefaultIfEmpty().FirstOrDefault(),
                c7 = s.StudentPreviousEducations.Select(ss => ss.ObtainedMarks).DefaultIfEmpty().FirstOrDefault(),
                ClassName = s.Student.AdmittedClass,
                c6 = s.StudentPreviousEducations.Select(zz => zz.YearOfPassing).DefaultIfEmpty().FirstOrDefault(),
                c10 = s.StudentPreviousEducations.Select(zz => zz.Grade).DefaultIfEmpty().FirstOrDefault(),
                GroupName = s.StudentPreviousEducations.Select(ss => ss.PreviousGroup).DefaultIfEmpty().FirstOrDefault(),
                AdmissionDate = s.StudentPreviousEducations.Select(ss => ss.AdmissionDate).DefaultIfEmpty().FirstOrDefault(),
                c11 = s.StudentPreviousEducations.Select(ss => ss.RollNo).DefaultIfEmpty().FirstOrDefault(),
                Discounts = s.Student.FeeDiscounts.Select(d => new {d.FeeType.TypeName, d.Discount, d.DiscountInAmount})
            }).OrderBy(o => o.AdmissionID).Select(s => new
            {
                AdmissionID = s.AdmissionID.ToString(),
                DOB = s.DOB.ToString("dd/MM/yyyy"),
                s.FName,
                s.Session,
                s.c1,
                s.c2,
                s.c3,
                s.c4,
                s.c5,
                s.c6,
                s.c7,
                s.GroupName,
                s.c11,
                s.d1,
                AdmissionDate = s.AdmissionDate.ToString("dd MMM yyyy"),
                s.TimeFrom,
                s.TimeTo,
                s.PickPoint,
                s.PermenantAddress,
                s.University,
                s.MobileNo,
                s.c10,
                s.PostalAddress,
                s.PresentClass,
                s.ClassName,
                s.StudentName,
                s.DriverName,
                s.TripNumber,
                s.StudentImage,
                TuitionFe =
                    s.Discounts.Where(w => w.TypeName == "Tuition Fee").Select(f => f.Discount).FirstOrDefault(),
                TransportFee = s.Discounts.Where(w => w.TypeName == "Transport Fee").Select(f => f.Discount)
                    .FirstOrDefault(),
                Exa = s.Discounts.Where(w => w.TypeName == "Exam Fee").Select(f => f.Discount).FirstOrDefault(),
                c12 = s.Discounts.Where(w => w.TypeName == "IT Fee").Select(f => f.Discount).FirstOrDefault()
            }).OrderBy(o => int.Parse(o.AdmissionID));
            return data;
        }

        public object experienceCertificat(int campusId = -1, string Empcode = "")
        {
            decimal Ids = 0;
            var camIds = campusId == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] {campusId};
            if (!string.IsNullOrEmpty(Empcode))
                Ids = db.tbl_Employee.Where(w => w.employeeCode == Empcode).Select(s => s.Id).FirstOrDefault();
            var data = db.tbl_Employee.Where(w => camIds.Contains(w.CampusID)).ToList().Select(s => new
            {
                AdmissionID = s.Id,
                StaffName = s.employeeName,
                s.employeeCode,
                FName = s.FatherName,
                Status = s.tbl_Designation == null ? string.Empty : s.tbl_Designation.designationName,
                ForMonthYear = s.joiningDate,
                EntryDate = s.terminationDate
            }).OrderBy(o => o.AdmissionID).Select(s => new
            {
                s.AdmissionID,
                s.StaffName,
                s.employeeCode,
                s.FName,
                s.Status,
                s.ForMonthYear,
                s.EntryDate
            });

            if (Empcode != "") data = data.Where(w => w.AdmissionID == Ids).ToList();
            return data;
        }

        public object CharacterCertificate(string RegNo = "")
        {
            var camp = _user.GetCampusIds();
            var ids = UtilityFunctions.ParseAdmIDs(RegNo, camp);
            var list = db.Students.Where(w => w.Admissions.Where(ww => ids.Contains(ww.ID)).Any()).ToList().Select(s => new
            {
                s.StudentName,
                s.FName,
                Session = s.Admissions.Select(k=> k.Session).FirstOrDefault(),
                ClassName = s.Admissions.Select(l=> l.ClassSection.Class.ClassName),
                SocBeh = s.Admissions.Select(n=> n.ExamRemarks.Select(i=> i.SocBeh)).FirstOrDefault(),
                EntryDate = DateTime.Now,
                c1= s.PermenantAddress,
                TelePhoneOff = s.StudentMobiles.Select(g=> g.MobileNo).FirstOrDefault()
            });
                return list;
        }
        public object Leavingcertificate(string RegNo = "", string remarks = "")
        {
            var camIds = _user.GetCampusIds();
            var ids = UtilityFunctions.ParseAdmIDs(RegNo, camIds);

            var data = db.ExpellDetails.Where(w => w.Student.Admissions.Where(ww => ids.Contains(ww.ID)).Any()).ToList()
                .Select(s => new
                {
                    s.StudentID,
                    AdmissionID = s.Student.RegistrationNo,
                    s.Student.RegistrationNo,
                    s.Student.StudentName,
                    s.Student.FName,
                    s.Student.DOB,
                    PresentClass = s.LastClass,
                    AppliedClass = s.Student.AdmittedClass,
                    s.Student.AdmissionDate,
                    s.Conduct,
                    StudentImage = s.Student.StudentPhotos.Where(w => w.IsReplaced == false).Select(p => p.StudentImage).FirstOrDefault(),
                    s.SLCNo,
                    s.Particular,
                    EntryDate = s.EntryDate.ToString("dd MMM yyyy"),
                    s.Student.Remarks,
                    s.Student.AdmittedClass
                }).ToList().OrderBy(o => o.StudentID).Select(s => new
                {
                    s.StudentID,
                    AdmissionID = s.AdmissionID.ToString(),
                    s.RegistrationNo,
                    DOB = s.DOB.ToString("dd/MM/yyyy"),
                    Col7 = WrittenNumerics.DateToWritten(s.DOB),
                    s.FName,
                    s.PresentClass,
                    s.StudentName,
                    c3=s.AdmittedClass,
                    AdmissionDate = s.AdmissionDate == null
                        ? "00/00/0000"
                        : Convert.ToDateTime(s.AdmissionDate).ToString("dd/MM/yyyy"),
                    s.AppliedClass,
                    s.EntryDate,
                    s.StudentImage,
                    c2 = remarks,
                    c1 = s.SLCNo
                }).OrderBy(o => int.Parse(o.AdmissionID));
            return data;
        }

        public object PrintStudentMonthlyAttandance(DateTime SelectedDate, int ClassId, int CampusId)
        {
            var q =
                $@"with a(AttendanceDate,Code,RegistrationNo,StudentName,Fname,ClassName) as (SELECT        DAY(dbo.StudentAttendences.AttendanceDate) AS d, dbo.StudentAttendanceTypes.Code, dbo.Students.RegistrationNo, dbo.Students.StudentName, 
dbo.Students.FName, dbo.Classes.ClassName FROM dbo.StudentAttendences INNER JOIN
dbo.StudentAttendanceTypes ON dbo.StudentAttendences.AttendanceTypeID = dbo.StudentAttendanceTypes.ID INNER JOIN dbo.Admissions ON dbo.StudentAttendences.AdmissionID = dbo.Admissions.ID INNER JOIN
dbo.Students  ON dbo.Admissions.StudentID = dbo.Students.ID INNER JOIN dbo.ClassSections ON dbo.Admissions.ClassSectionID = dbo.ClassSections.ID INNER JOIN dbo.Classes ON dbo.ClassSections.ClassID = dbo.Classes.ID INNER JOIN
dbo.Campuses ON dbo.Admissions.CampuseID ={CampusId} and dbo.ClassSections.ClassID = {ClassId} 
WHERE  Month(StudentAttendences.AttendanceDate)=" + SelectedDate.Month +
                " AND Year(StudentAttendences.AttendanceDate)=" + SelectedDate.Year +
                " ) select * from a Pivot(Max(Code) for AttendanceDate IN([1],[2],[3],[4],[5],[6],[7],[8],[9],[10],[11],[12],[13],[14],[15],[16],[17],[18],[19],[20],[21],[22],[23],[24],[25],[26],[27],[28],[29],[30],[31]))as pt";

            
            var tab = dba.CreateTable(q);
            var con = "";
            tab.Columns.Add("Month"); //present
            tab.Columns.Add("Year"); //absent
            tab.Columns.Add("FName"); //leave
            tab.Columns.Add("holiday"); //holidy
            var day = 0;
            var holidays = new int[31];
            var j = 0;
            foreach (DataColumn d in tab.Columns)
                if (tab.Rows.Count > 0)
                {
                    try
                    {
                        day = int.Parse(d.ColumnName);
                    }
                    catch
                    {
                        day = 0;
                    }

                    if (day != 0)
                    {
                        var holiday = db.SchoolLeaveSchedules.Where(w =>
                                w.date.Year == SelectedDate.Year && w.date.Month == SelectedDate.Month &&
                                w.date.Day == day && w.IsHoliday)
                            .FirstOrDefault();
                        if (holiday != null)
                            try
                            {
                                tab.Rows[j]["holiday"] = holiday.date.Day + " : " + holiday.holidayName;
                                holidays[j] = holiday.date.Day;
                                j++;
                            }
                            catch
                            {
                                var dr = tab.NewRow();
                                dr["holiday"] = holiday.date.Day + " : " + holiday.holidayName;
                                tab.Rows.Add(dr);
                                j++;
                            }
                    }
                }
        
            foreach (DataRow dr in tab.Rows)
            {
                int present = 0, leave = 0, absent = 0, latecoming = 0;
                var dd = 0;
                foreach (DataColumn dc in tab.Columns)
                {
                    if (dc.ColumnName == "RegistrationNo" || dc.ColumnName == "StudentName" ||
                        dc.ColumnName == "FName" || dc.ColumnName == "ClassName")
                        continue;

                    try
                    {
                        con = dr[dc].ToString().ToLower().Trim();
                    }
                    catch
                    {
                    }

                    try
                    {
                        dd = int.Parse(dc.ColumnName);
                    }
                    catch
                    {
                        dd = 0;
                    }

                    if (con.Trim().ToLower() == "p")
                        present++;
                    else if (con.Trim().ToLower() == "a")
                        absent++;
                    else if (con.Trim().ToLower() == "l")
                        leave++;
                    else if (con.Trim() == "" && !holidays.Contains(dd))
                        dr[dc] = "-";
                    else if (con.Trim() == "" && holidays.Contains(dd)) dr[dc] = "X";
                }
                dr["Month"] = present.ToString();
                dr["Year"] = absent.ToString();
                dr["FName"] = leave.ToString();
                dr["ClassName"] = latecoming.ToString();
            }

            foreach (DataColumn d in tab.Columns)
            {
                var res = 0;
                if (int.TryParse(d.ColumnName, out res)) d.ColumnName = "c" + d.ColumnName;
            }

            tab.Columns["RegistrationNo"].ColumnName = "AdmissionID";
            return tab;
        }
        public class AwardList
        {
            public string ExamName { get; set; }
            public int AdmissionID { get; set; }
            public string StudentName { get; set; }
            public string FName { get; set; }
            public double ObtainedMarks { get; set; }
            public string ClassName { get; set; }
            public string SectionName { get; set; }
            public string SubjectName { get; set; }
            public double TotalMarks { get; set; }
            public double PassPercentage { get; set; }
            public double Percentage
            {
                get
                {

                    return ObtainedMarks/TotalMarks*100;
                }
            }
            public bool Pass
            {
                get
                {
                    return Percentage>=PassPercentage;
                }
            }
        }
        public List<AwardList> GetAwardList(int Classes, int Sections, int campus, int exams,
            int Subjec)
        {
            var secid = db.ClassSections.Where(w => w.CampusID == campus & w.ClassID == Classes && w.SectionID == Sections).Select(s => new { s.ID, s.Class.ClassName, s.Section.SectionName })
                    .FirstOrDefault();
            var exam = db.ExamHelds.Where(w => w.ID==exams).ToList().Select(s => s.Exam.ExamName+" "+ s.EntryDate.ToString("MMMM, yyyy")).FirstOrDefault();
            var subjName = db.Subjects.Where(w => w.ID==Subjec).Select(s => s.SubjectName).FirstOrDefault();
            var classSubjectID = db.ClassSubjects.Where(w => w.SubjectID == Subjec && w.ClassID == Classes)
                .Select(s => s.ID).FirstOrDefault();
            try
            {
                var data = db.Results.AsNoTracking().OrderBy(o=>o.Admission.Student.RegistrationNo).
                    Where(w => w.Admission.ClassSectionID == secid.ID
                    && w.Admission.CampuseID == campus &&
                    w.ExamHeldID == exams &&
                    w.ClassSubjectID == classSubjectID).ToList().Select(s => new AwardList
                    {
                        ExamName=exam,
                        AdmissionID = s.Admission.Student.RegistrationNo,
                        StudentName = s.Admission.Student.StudentName,
                        FName = s.Admission.Student.FName,
                        ObtainedMarks = s.ObtainedMarks,
                        ClassName =secid.ClassName,
                        SectionName= secid.SectionName,
                        TotalMarks = s.ExamHeld.ExamDates.Where(w => w.SubjectID==Subjec && w.ClassSectionID==secid.ID).Select(t => t.TotalMarks).FirstOrDefault(),
                        PassPercentage = s.ExamHeld.ExamsRules.Select(t => t.AtLeastPercentage).FirstOrDefault(),
                        SubjectName= subjName
                    });
                return data.ToList();
            }
            catch (Exception aa)
            {
                return new List<AwardList>();
            }
        }

        public object GetAwardListblank(int Classes = -1, int Sections = -1, int campus = -1, int exams = -1,
        int Subject = -1)
        {
            var secid = db.ClassSections.Where(w => w.CampusID == campus & w.ClassID == Classes && w.SectionID == Sections).Select(s => new { s.ID, s.Class.ClassName, s.Section.SectionName })
                     .FirstOrDefault();

            var exam = db.ExamHelds.Where(w => w.ID==exams).ToList().Select(s => s.Exam.ExamName+" "+ s.EntryDate.ToString("MMMM, yyyy") ).FirstOrDefault();
            var subjName = db.Subjects.Where(w => w.ID==Subject).Select(s => s.SubjectName).FirstOrDefault();

            var classSubjectID = db.ClassSubjects.Where(w => w.SubjectID == Subject && w.ClassID == Classes)
                .Select(s => s.ID).FirstOrDefault();
            try
            {
                var data = db.Admissions.AsNoTracking().OrderBy(o=>o.Student.RegistrationNo).
                    Where(w => w.ClassSectionID == secid.ID
                    && w.CampuseID == campus && !w.IsExpell).Select(s => new
                    {
                        ExamName=exam,
                        AdmissionID = s.Student.RegistrationNo,
                        s.Student.StudentName,
                        s.Student.FName,
                        secid.ClassName,
                        secid.SectionName,
                        SubjectName= subjName
                    });
                
                return data;
            }
            catch (Exception aa)
            {
                return new DataTable();
            }
        }

        public object PrintstaffMonthlyAttandance(DateTime SelectedDate)
        {
            var q =
                @"with a(AttendanceDate,Code,RegistrationNo,StudentName,Fname) as (SELECT        DAY(EmployeeAttendance.AttendanceDate) AS d, EmployeeAttendanceType.Code,
                         tbl_Employee.Id AS RegistrationNo, tbl_Employee.EmployeeName AS StudentName, tbl_Employee.FatherName AS FName FROM tbl_Employee INNER JOIN EmployeeAttendance ON tbl_Employee.Id = EmployeeAttendance.StaffID INNER JOIN
                         EmployeeAttendanceType ON EmployeeAttendance.AttendanceTypeID = EmployeeAttendanceType.ID 
                        WHERE  Month(EmployeeAttendance.AttendanceDate)=" + SelectedDate.Month +
                " AND Year(EmployeeAttendance.AttendanceDate)=" + SelectedDate.Year +
                " ) select * from a Pivot(Max(Code) for AttendanceDate IN([1],[2],[3],[4],[5],[6],[7],[8],[9],[10],[11],[12],[13],[14],[15],[16],[17],[18],[19],[20],[21],[22],[23],[24],[25],[26],[27],[28],[29],[30],[31]))as pt";
            var tab = dba.CreateTable(q);
            var con = "";
            tab.Columns.Add("Month"); //present
            tab.Columns.Add("Year"); //absent
            tab.Columns.Add("FName"); //leave
            tab.Columns.Add("ClassName"); //Latecoming
            tab.Columns.Add("holiday"); //holidy
            var day = 0;
            var holidays = new int[31];
            var j = 0;
            foreach (DataColumn d in tab.Columns)
                if (tab.Rows.Count > 0)
                {
                    try
                    {
                        day = int.Parse(d.ColumnName);
                    }
                    catch
                    {
                        day = 0;
                    }

                    if (day != 0)
                    {
                        var holiday = db.SchoolLeaveSchedules.Where(w =>
                                w.date.Year == SelectedDate.Year && w.date.Month == SelectedDate.Month &&
                                w.date.Day == day && w.IsHoliday)
                            .FirstOrDefault();
                        if (holiday != null)
                            try
                            {
                                tab.Rows[j]["holiday"] = holiday.date.Day + " : " + holiday.holidayName;
                                holidays[j] = holiday.date.Day;
                                j++;
                            }
                            catch
                            {
                                var dr = tab.NewRow();
                                dr["holiday"] = holiday.date.Day + " : " + holiday.holidayName;
                                tab.Rows.Add(dr);
                                j++;
                            }
                    }
                }

            foreach (DataRow dr in tab.Rows)
            {
                int present = 0, leave = 0, absent = 0, latecoming = 0;
                var d = 0;

                foreach (DataColumn dc in tab.Columns)
                {
                    if (dc.ColumnName == "RegistrationNo")
                        continue;
                    try
                    {
                        con = dr[dc].ToString().ToLower().Trim();
                    }
                    catch
                    {
                    }

                    try
                    {
                        d = int.Parse(dc.ColumnName);
                    }
                    catch
                    {
                        d = 0;
                    }

                    if (con.Trim().ToLower() == "p")
                        present++;
                    else if (con.Trim().ToLower() == "a")
                        absent++;
                    else if (con.Trim().ToLower() == "l")
                        leave++;
                    else if (con.Trim() == "" && !holidays.Contains(d))
                        dr[dc] = "-";
                    else if (con.Trim() == "" && holidays.Contains(d)) dr[dc] = "X";
                }

                foreach (DataColumn dd in tab.Columns)
                {
                    var res = 0;
                    if (int.TryParse(dd.ColumnName, out res)) dd.ColumnName = "c" + dd.ColumnName;
                }

                dr["Month"] = present.ToString();
                dr["Year"] = absent.ToString();
                dr["FName"] = leave.ToString();
                dr["ClassName"] = latecoming.ToString();
            }

            return tab;
        }

        public object Printfeeslip(DateTime SelectedDate, string ClassId, string regno, int campusId, bool showAdvance)
        {
            int[] admId = { };
            int[] arrearAdmId = { };
            var camIds = campusId == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] {campusId};
            if (regno != "" && regno != null)
            {
                admId = UtilityFunctions.ParseAdmIDs(regno, camIds);
                arrearAdmId = admId;
                if (admId.Count() == 0) return "";
            }
            else
            {
                int.TryParse(ClassId, out var cid);
                var reg = db.Admissions.Where(w => w.ClassSection.ClassID == cid && camIds.Contains(w.CampuseID))
                    .Select(s => s.Student.RegistrationNo).ToArray();
                admId = db.Admissions.Where(w => w.ClassSection.ClassID == cid && camIds.Contains(w.CampuseID))
                    .Select(s => s.ID).ToArray();

                arrearAdmId = UtilityFunctions.ParseAdmIDs(string.Join(",", reg), camIds);

                if (admId.Count() == 0) return "";
            }

            var tab = new DataTable();
            var startdate = new SqlParameter("month", SelectedDate.Month);
            var enddate = new SqlParameter("year", SelectedDate.Year);
            var admissionIds = new SqlParameter("id", string.Join(",", admId));
            var collection = new List<SqlParameter>();
            collection.Add(startdate);
            collection.Add(enddate);
            collection.Add(admissionIds);

            tab = dba.ExecuteSP("[dbo].[FeeSystemProc]", collection);
            var tabArrears = new DataTable();
            if (arrearAdmId.Count() > 0)
            {
                var collection2 = new List<SqlParameter>();

                var startdate2 = new SqlParameter("month", SelectedDate.Month);
                var enddate2 = new SqlParameter("year", SelectedDate.Year);
                var admissionIds2 = new SqlParameter("id", string.Join(",", arrearAdmId));

                collection2.Add(startdate2);
                collection2.Add(enddate2);
                collection2.Add(admissionIds2);

                tabArrears = dba.ExecuteSP("[dbo].[FeeSystemProcArrears]", collection2);
            }
            if (tab.Rows.Count > 0)
            {
                tab.Columns.Add("d1");
                tab.Columns["LateFee"].ColumnName = "d2";
                tab.Columns.Add("BarCode", typeof(byte[]));
                var distinctCampus = tab.AsEnumerable().Select(s => new {name = s["CampusName"].ToString()}).ToList()
                    .Distinct();

                foreach (var cid in distinctCampus)
                {
                    var cId = db.Campuses.Where(w => w.CampusName == cid.name).FirstOrDefault().ID;
                    setting.CampusId = cId;
                    var dailyLateFee = (bool) setting.ReadWithType("ApplyDailyLateFee", typeof(bool));
                    var LateFeeAmount = (decimal)setting.ReadWithType("LateReceivedFeeFine", typeof(decimal));
                    var studentsInCampus =
                        tab.AsEnumerable().Where(w => w["CampusName"].ToString() == cid.name).ToList();
                    foreach (var item in studentsInCampus)
                    {
                        item["d1"] = dailyLateFee ? 1 : 0;
                        if(!decimal.TryParse( item["d2"].ToString(),out decimal late) || late==0)
                        {
                            item["d2"] = LateFeeAmount;
                        }
                        //item["BarCode"] = RenderBarcode(item["RegistrationNo"].ToString());
                        item["BarCode"] = new Barcode(item["RegistrationNo"].ToString(), NetBarcode.Type.Code128, false, 95, 22, null).GetByteArray();
                    }
                }


                var disctingsRegNo = tab.AsEnumerable().Select(s => new
                    {
                        regno = int.Parse(s["RegistrationNo"].ToString()), id = int.Parse(s["AdmissionID"].ToString())
                })
                    .ToList().Distinct();

                if (tabArrears.Rows.Count > 0)
                {
                    foreach (var regNo in disctingsRegNo)
                    {

                        var meArrears = tabArrears.AsEnumerable().Where(w => int.Parse(w["RegistrationNo"].ToString()) == regNo.regno);
                        
                        if (meArrears.Count() > 0)
                        {
                            if (showAdvance)
                            {
                                var date = new DateTime(SelectedDate.Year, SelectedDate.Month, 28);
                                var arrears = meArrears.Where(w => (DateTime)w["ForMonth"] <= date.Date);
                                if (arrears.Count() > 0)
                                {
                                    var row = tab.NewRow();
                                    row["RegistrationNo"] = regNo.regno;
                                    row["AdmissionID"] = regNo.id;
                                    row["TypeName"] = "Arrears " + string.Join(", ", arrears.Select(s => ((DateTime)s["ForMonth"]).ToString("MMM yyyy")).ToArray());
                                    row["Amount"] = arrears.Sum(s => decimal.Parse(s["Balance"].ToString()));

                                    tab.Rows.Add(row);
                                }
                                var advance = meArrears.Where(w => (DateTime)w["ForMonth"] >= date.Date);
                                if (advance.Count() > 0)
                                {
                                    var row1 = tab.NewRow();
                                    row1["RegistrationNo"] = regNo.regno;
                                    row1["AdmissionID"] = regNo.id;
                                    row1["TypeName"] = "Advance " + string.Join(", ", advance.Select(s => ((DateTime)s["ForMonth"]).ToString("MMM yyyy")).ToArray());
                                    row1["Amount"] = advance.Sum(s => decimal.Parse(s["Balance"].ToString()));

                                    tab.Rows.Add(row1);
                                }
                            }
                            else
                            {
                                var row = tab.NewRow();
                                row["RegistrationNo"] = regNo.regno;
                                row["AdmissionID"] = regNo.id;
                                row["TypeName"] = "Arrears " + string.Join(", ", meArrears.Select(s => ((DateTime)s["ForMonth"]).ToString("MMM yyyy")).ToArray());
                                row["Amount"] = meArrears.Sum(s => decimal.Parse(s["Balance"].ToString()));

                                tab.Rows.Add(row);
                            }
                        }
                    }
                }
            }
             return tab;
        }

        public object DateSheetPrint(string RegNo = "", int Class = -1, int Section = -1, int campus = -1,
            int exams = -1, string remarks = "", string byregno = "", string byclass = "")
        {
            var tab = new DataTable();
            var examid = int.Parse(exams.ToString());
            int[] ids = { };

            var ClassId = 0;
            var CampusId = 0;
            var ClassSecitonId = 0;

            #region ids by class

            if (byclass == "true")
            {
                ClassId = Class;
                CampusId = campus;
                ClassSecitonId = db.ClassSections.Where(w =>w.CampusID==CampusId && w.SectionID == Section && w.ClassID == ClassId)
                    .Select(s => s.ID).FirstOrDefault();
                ids = db.Admissions.Where(w => !w.IsExpell && w.CampuseID == CampusId && w.ClassSectionID==ClassSecitonId).Select(s=>s.ID).ToArray();

                //ids = db.Admissions
                //    .Where(w => w.ClassSectionID == ClassSecitonId).Select(s => s.ID).ToArray();
                //int pause = 0;
            }

            #endregion

            #region ids by admno

            else if (byregno == "true")
            {
                var CampusIds = campus == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] {campus};
                ids = UtilityFunctions.ParseAdmIDs(RegNo, CampusIds);
                if (ids.Count() == 0)
                {
                    ids = new[] {0};
                    var tables = new DataTable();
                    return tables;
                }

                var firstAdm = db.Admissions.Where(w => ids.Contains(w.ID))
                    .Select(s => new {s.CampuseID, s.ClassSectionID, s.ClassSection.ClassID}).FirstOrDefault();
                ClassId = firstAdm.ClassID;
                CampusId = firstAdm.CampuseID;
                ClassSecitonId = firstAdm.ClassSectionID;
            }

            #endregion

            //dba.GetCon();, dbo.StudentPhotos.StudentImage, dbo.StudentPhotos.IsReplaced
            var query =
                $@"SELECT TOP (100) PERCENT dbo.Students.RegistrationNo, dbo.Students.PostalAddress, dbo.Students.StudentName, dbo.Students.FName, dbo.Classes.ClassName,dbo.Campuses.CampusName, CONVERT(varchar(3), dbo.Campuses.ID) AS CampuseID, dbo.Exams.ExamName, 
                  DATENAME(month, dbo.ExamHelds.EntryDate) + ' , ' + DATENAME(year, dbo.ExamHelds.EntryDate) AS ForMonthYear, dbo.Subjects.SubjectName, DATENAME(dw, dbo.ExamDates.ExamDate) + ' ' + CONVERT(VARCHAR(25), 
                  dbo.ExamDates.ExamDate, 106) AS EntryDate, dbo.ExamDates.TimeFrom AS c1, dbo.ExamDates.ToTime AS c2, dbo.ExamDates.TotalMarks, dbo.Sections.SectionName, 
                  dbo.ExamDates.ExamHeldID,dbo.StudentPhotos.StudentImage
FROM     dbo.ExamDates INNER JOIN
                  dbo.ExamHelds ON dbo.ExamDates.ExamHeldID = dbo.ExamHelds.ID INNER JOIN
                  dbo.Exams ON dbo.ExamHelds.ExamID = dbo.Exams.ID INNER JOIN
                  dbo.Campuses ON dbo.ExamDates.CampusID = dbo.Campuses.ID INNER JOIN
                  dbo.ClassSections ON dbo.ExamDates.ClassSectionID = dbo.ClassSections.ID INNER JOIN
                  dbo.Classes ON dbo.ClassSections.ClassID = dbo.Classes.ID INNER JOIN
                  dbo.Sections ON dbo.ClassSections.SectionID = dbo.Sections.ID INNER JOIN
                  dbo.Subjects ON dbo.ExamDates.SubjectID = dbo.Subjects.ID INNER JOIN
                  dbo.Admissions ON dbo.Campuses.ID = dbo.Admissions.CampuseID AND dbo.ClassSections.ID = dbo.Admissions.ClassSectionID INNER JOIN
                  dbo.Students ON dbo.Admissions.StudentID = dbo.Students.ID LEFT OUTER JOIN
                  dbo.StudentPhotos ON dbo.Students.ID = dbo.StudentPhotos.StudentID
WHERE  (dbo.ExamDates.ExamHeldID = {examid}) AND (dbo.ExamDates.CampusID = {CampusId}) AND (dbo.ExamDates.ClassSectionID = {ClassSecitonId}) AND  dbo.Admissions.IsExpell = 0 AND dbo.Admissions.ID IN({string.Join(",", ids)}) AND (dbo.StudentPhotos.IsReplaced IS NULL OR
                  dbo.StudentPhotos.IsReplaced = 0) ORDER BY dbo.Exams.ExamName, dbo.ExamDates.ExamDate";

            tab = dba.CreateTable(query);

            tab.Columns.Add("c4");
            tab.Columns.Add("c3");

            tab.Columns.Add("BarCode", typeof(byte[]));
            tab.Columns.Add("Logo", typeof(byte[]));

            var Signature = db.SchoolLogoes.Where(w => w.ID == 3).FirstOrDefault();

            var disitingRegNo = tab.AsEnumerable().Select(s => int.Parse(s["RegistrationNo"].ToString())).Distinct();
            foreach (var reg in disitingRegNo)
            {
                var sno = 1;
                foreach (var s in tab.AsEnumerable().Where(w => w["RegistrationNo"].ToString() == reg.ToString()))
                {
                    s["c3"] = sno;
                    sno++;
                    if (Signature != null) s["Logo"] = Signature.LOGO;
                    s["BarCode"] = null;
                    // b.GetImageData(BarcodeLib.SaveTypes.JPG);
                }
            }
            return tab;
        }

        public object ProgressReport(string RegNo = "", int Class = -1, int Section = -1, int campus = -1,
            int exams = -1, string byregno = "", string byclass = "", int year = -1)
        {
            
            var examid = int.Parse(exams.ToString());
            int[] ids = { };

            #region ids by class

            if (byclass == "true")
            {
                var clsID = Class == -1 ? db.Classes.Select(s => s.ID).ToArray() : new[] {Class};
                var campid = campus == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] {campus};
                var secid = Section == -1
                    ? db.ClassSections.Select(s => s.ID).ToArray()
                    : new[]
                    {
                        db.ClassSections.Where(w => w.ClassID == Class && w.SectionID == Section).Select(s => s.ID)
                            .FirstOrDefault()
                    };

                //int[] secid = new int[] { };
                if (Section == -1)
                {
                    if (Class > 0)
                        secid = db.ClassSections.Where(w => w.ClassID == Class).Select(s => s.ID).ToArray();
                    else
                        secid = db.ClassSections.Select(s => s.ID).ToArray();
                }

                var admissionid = db.Admissions.Where(w => secid.Contains(w.ClassSection.ID)).Select(s => s.ID)
                    .ToArray();
                ids = db.Results
                    .Where(w => admissionid.Contains(w.AdmissionID) && campid.Contains(w.Admission.CampuseID) &&
                                w.ExamHeldID == examid).Select(s => s.ID).ToArray();
            }

            #endregion

            #region ids by admno

            else if (byregno == "true")
            {
                var CampusIds = campus == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] {campus};
                ids = UtilityFunctions.ParseAdmIDs(RegNo, CampusIds);
                if (ids.Count() == 0)
                {
                    var tables = new DataTable();
                    return tables;
                }
            }

            #endregion

            string[] cols =
            {
                "SocBeh", "Discpline", "OralExp", "CoTeamWork", "Gender", "Col7", "ExamHeldID", "Confid", "Manners",
                "Punctuality", "PhyFit", "Neatness", "_Year", "RegistrationNo", "StudentName", "FName", "ClassName",
                "ExamName", "SectionName", "StudentID", "CampuseID", "Exa", "EntryDate", "c21", "c22", "c23", "c24",
                "c25", "c26", "c27", "c28", "c29", "c30", "StudentImage", "BarCode"
            };
            var rs = new ResultSystem(
                "(dbo.Results.ID IN (" + string.Join(",", ids.Select(s => s.ToString().Trim())) +
                " )) AND (YEAR(ExamHelds.EntryDate) = " + year + ")", "ExamHelds.EntryDate,");
            var tab = rs.GetData();
            if (tab.Rows.Count < 1)
                return "";
            var pt = new Pivot(tab);
            var dt = pt.PivotData("ObtainedMarks", AggregateFunction.First,
                new[]
                {
                    "AdmissionID", "StudentID", "ExamHeldID", "ExamName", "StudentName", "FName", "ClassName",
                    "RegistrationNo", "SectionName", "TotalObtained", "ClassPos", "SecPos", "GrantTotal", "TotalStatus",
                    "TotalPercentage", "_Year", "Exa", "CampusID", "_Month", "EntryDate"
                }, new[] {"SubjectName"});

            dt.Columns.Add("Attendance");
            dt.Columns.Add("Teacher's Signature");
            dt.Columns.Add("Parent's Signature");
            var lastordinal = dt.Columns.Count;
            dt.Columns.Add("Manners");
            dt.Columns.Add("Confid");
            dt.Columns.Add("SocBeh");
            dt.Columns.Add("Discpline");
            dt.Columns.Add("OralExp");
            dt.Columns.Add("CoTeamWork");
            dt.Columns.Add("Punctuality");
            dt.Columns.Add("PhyFit");
            dt.Columns.Add("Neatness");
            dt.Columns.Add("Gender");
            dt.Columns.Add("Col7");


            dt.Columns.Add("c21");
            dt.Columns.Add("c22");
            dt.Columns.Add("c23");
            dt.Columns.Add("c24");
            dt.Columns.Add("c25");
            dt.Columns.Add("c26");
            dt.Columns.Add("c27");
            dt.Columns.Add("c28");
            dt.Columns.Add("c29");
            dt.Columns.Add("c30");
            //dt.Columns.Add("Col7");

            dt.Columns["GrantTotal"].SetOrdinal(lastordinal - 1);
            dt.Columns["TotalObtained"].SetOrdinal(lastordinal - 1);
            dt.Columns["TotalPercentage"].SetOrdinal(lastordinal - 1);
            dt.Columns["SecPos"].SetOrdinal(lastordinal - 1);
            dt.Columns["ClassPos"].SetOrdinal(lastordinal - 1);
            dt.Columns["Attendance"].SetOrdinal(lastordinal - 1);
            dt.Columns["TotalStatus"].SetOrdinal(lastordinal - 1);
            dt.Columns["Teacher's Signature"].SetOrdinal(lastordinal - 1);
            dt.Columns["Parent's Signature"].SetOrdinal(lastordinal - 1);

            if (dt.Columns.Contains("GroupName"))
                dt.Columns["GroupName"].ColumnName = " Group";
            dt.Columns["TotalObtained"].ColumnName = "Obtained";
            dt.Columns["GrantTotal"].ColumnName = "Total";
            dt.Columns["TotalStatus"].ColumnName = "Remarks";
            dt.Columns["CampusID"].ColumnName = "CampuseID";
            dt.Columns["TotalPercentage"].ColumnName = "Percentage";

            dt.Columns.Add("StudentImage", typeof(byte[]));
            dt.Columns.Add("BarCode", typeof(byte[]));
            double percentage = 0;
            var remtab = new DataTable();

            foreach (DataRow dr in dt.Rows)
            {
                var q = "";
                try
                {
                    var examDate = DateTime.Parse(dr["EntryDate"].ToString());
                    dr["Attendance"] = _attendanceHelper.GetMonthlyAttendance(int.Parse(dr["StudentID"].ToString()),
                        examDate.Month, examDate.Year, 'p');
                }
                catch
                {
                    dr["Attendance"] = ".";
                }

                dr["Teacher's Signature"] = ".";
                dr["Parent's Signature"] = ".";

                #region First Term Exam Attndance and Remarks

                var firstTermExam = db.ExamHelds.Where(w => w.Exam.ID == 5).ToList()
                    .Where(w => w.EntryDate.Year == year).FirstOrDefault();
                if (firstTermExam != null)
                {
                    var firTermExamDate = firstTermExam.EntryDate.Date;
                    var days = DateTime.DaysInMonth(firTermExamDate.Year, firTermExamDate.Month);

                    //firTermExamDate = DateTime.ParseExact(days.ToString("00") + "/" + firTermExamDate.Month.ToString("00") + "/" + firTermExamDate.Year, "dd/mm/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    //MessageBox.Show(firTermExamDate.ToString());
                    try
                    {
                        var attendance = _attendanceHelper.GetYearlyAttendance(int.Parse(dr["StudentID"].ToString()), firTermExamDate,
                            'P');
                        percentage = _attendanceHelper.getpercentage();
                        dr["Punctuality"] = UtilityFunctions.GetPunctualityRemarks(percentage);
                        dr["Attendance"] = attendance; //c17 is the attendance field of first term in report
                    }
                    catch
                    {
                        dr["Punctuality"] = "";
                        dr["Attendance"] = "";
                    }

                    q =
                        @"SELECT ExamRemarks.SocBeh, ExamRemarks.Discpline, ExamRemarks.Manners, ExamRemarks.OralExp, ExamRemarks.CoTeamWork, ExamRemarks.Confid, 
                         ExamRemarks.Punctuality, ExamRemarks.PhyFit, ExamRemarks.Neatness, ExamRemarks.Attendance, ExamRemarks.Remarks,ExamRemarks.ExamHeldID FROM Admissions INNER JOIN
                         ExamRemarks ON Admissions.ID = ExamRemarks.AdmissionID INNER JOIN Students ON Admissions.StudentID = Students.ID
                         WHERE (Admissions.ID = " + dr["AdmissionID"] + ") AND (ExamRemarks.ExamHeldID =" +
                        firstTermExam.ID + ")";

                    var firstTerm = dba.CreateTable(q).AsEnumerable()
                        .Where(w => w["ExamHeldID"].ToString() == firstTermExam.ID.ToString())
                        .FirstOrDefault(); // 5 is the id of 1st term exam
                    if (firstTerm != null)
                    {
                        dr["SocBeh"] = firstTerm["SocBeh"];
                        dr["Manners"] = firstTerm["Manners"];
                        dr["Confid"] = firstTerm["Confid"];
                        dr["Discpline"] = firstTerm["Discpline"];
                        dr["OralExp"] = firstTerm["OralExp"];
                        dr["CoTeamWork"] = firstTerm["CoTeamWork"];
                        dr["Punctuality"] = firstTerm["Punctuality"];
                        dr["PhyFit"] = firstTerm["PhyFit"];
                        dr["Neatness"] = firstTerm["Neatness"];
                        dr["Attendance"] = firstTerm["Attendance"];
                    }
                }

                #endregion

                #region Final Term Exam Attendance and Reamarks

                var finalTermExam = db.ExamHelds.Where(w => w.Exam.ID == 1).Where(w => w.EntryDate.Year == year)
                    .FirstOrDefault();
                if (finalTermExam != null)
                {
                    try
                    {
                        var finalTermExamDate = finalTermExam.EntryDate.Date;
                        var attendance = _attendanceHelper.GetYearlyAttendance(int.Parse(dr["StudentID"].ToString()),
                            finalTermExamDate, 'P');
                        percentage = _attendanceHelper.getpercentage();
                        dr["c23"] = UtilityFunctions
                            .GetPunctualityRemarks(percentage); //c23 is the punctuality field of final term in report
                        dr["c24"] = attendance; //c24 is the attendance field of final term in report
                    }
                    catch
                    {
                        dr["c23"] = "";
                        dr["c24"] = "";
                    }

                    q =
                        @"SELECT ExamRemarks.SocBeh, ExamRemarks.Discpline, ExamRemarks.Manners, ExamRemarks.OralExp, ExamRemarks.CoTeamWork, ExamRemarks.Confid, 
                         ExamRemarks.Punctuality, ExamRemarks.PhyFit, ExamRemarks.Neatness, ExamRemarks.Attendance, ExamRemarks.Remarks,ExamRemarks.ExamHeldID FROM Admissions INNER JOIN
                         ExamRemarks ON Admissions.ID = ExamRemarks.AdmissionID INNER JOIN Students ON Admissions.StudentID = Students.ID
                         WHERE (Admissions.ID = " + dr["AdmissionID"] + ") AND (ExamRemarks.ExamHeldID =" +
                        finalTermExam.ID + ")";
                    var finalTerm = dba.CreateTable(q).AsEnumerable()
                        .Where(w => w["ExamHeldID"].ToString() == finalTermExam.ID.ToString())
                        .FirstOrDefault(); // 1 is the id of Annual exam
                    if (finalTerm != null)
                    {
                        dr["c21"] = finalTerm["SocBeh"];
                        dr["c27"] = finalTerm["Manners"];
                        dr["c28"] = finalTerm["Confid"];
                        dr["c25"] = finalTerm["Discpline"];
                        dr["c22"] = finalTerm["OralExp"];
                        dr["c26"] = finalTerm["CoTeamWork"];
                        dr["c23"] = finalTerm["Punctuality"];
                        dr["c30"] = finalTerm["PhyFit"];
                        dr["c29"] = finalTerm["Neatness"];
                        dr["c23"] = finalTerm["Punctuality"];
                        dr["c24"] = finalTerm["Attendance"];
                        dr["Remarks"] = finalTerm["Remarks"];
                    }
                }

                #endregion


                //NetBarcode.Barcode b = new NetBarcode.Barcode();
                //System.Drawing.Image img = b.Encode(BarcodeLib.TYPE.CODE128, dr["RegistrationNo"].ToString());
                //foreach (var t in dt.AsEnumerable().Where(w => w["RegistrationNo"].ToString() == dr["RegistrationNo"].ToString()))
                //    t["BarCode"] = b.GetImageData(BarcodeLib.SaveTypes.JPG);
                foreach (var t in tab.AsEnumerable().Where(w => w["RegistrationNo"].ToString() == RegNo))
                    t["BarCode"] = null; //b.GetImageData(BarcodeLib.SaveTypes.JPG);
                var a = dba.CreateTable(
                    "SELECT dbo.StudentPhotos.StudentImage,dbo.StudentPhotos.PhotoYear FROM dbo.StudentPhotos INNER JOIN dbo.Students ON dbo.StudentPhotos.StudentID = dbo.Students.ID WHERE (dbo.Students.RegistrationNo = " +
                    dr["RegistrationNo"] + ")");
                if (a.Rows.Count > 0)
                {
                    var photo = a.AsEnumerable()
                        .Where(w => w["PhotoYear"].ToString().Trim() == dr["_Year"].ToString().Trim())
                        .Select(s => s["StudentImage"]).FirstOrDefault();

                    if (photo != null)
                        //   MessageBox.Show(r["Photoyear"].ToString());
                        dr["StudentImage"] = photo;
                    else
                        // MessageBox.Show(a.Rows[i]["Photoyear"].ToString()) ?>
                        dr["StudentImage"] = a.Rows[a.Rows.Count - 1][0]; //logo;
                }
            }

            return "";
        }
        
        public DataTable markSheetPrint(string RegNo = "", int Class = -1, int Section = -1, int campus = -1,
            int exams = -1, string byregno = "", string byclass = "", string subject = "",
            string omit = "", string summary = "", bool showposition=false, bool showdob=false)
        {
            var examid = int.Parse(exams.ToString());
            int[] ids = { };

                var campid = campus == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] {campus};
            #region ids by class

            if (byclass == "true")
            {
                var clsID = Class == -1 ? db.Classes.Select(s => s.ID).ToArray() : new[] {Class};
                var secid = Section == -1
                    ? db.ClassSections.Where(w=> campid.Contains(w.CampusID)).Select(s => s.ID).ToArray()
                    : new[]
                    {
                        db.ClassSections.Where(w => w.ClassID == Class && w.SectionID == Section & campid.Contains(w.CampusID)).Select(s => s.ID)
                            .FirstOrDefault()
                    };

                //int[] secid = new int[] { };
                if (Section == -1)
                {
                    if (Class > 0)
                        secid = db.ClassSections.Where(w => w.ClassID == Class & campid.Contains(w.CampusID)).Select(s => s.ID).ToArray();
                    else
                        secid = db.ClassSections.Where(w=> campid.Contains(w.CampusID)).Select(s => s.ID).ToArray();
                }

                var admissionid = db.Admissions.Where(w => secid.Contains(w.ClassSection.ID) & campid.Contains(w.CampuseID)).Select(s => s.ID)
                    .ToArray();
                ids = db.Results
                    .Where(w => admissionid.Contains(w.AdmissionID) && campid.Contains(w.Admission.CampuseID) &&
                                w.ExamHeldID == examid).Select(s => s.ID).ToArray();
            }

            #endregion

            #region ids by admno

            else if (byregno == "true")
            {
                var CampusIds = campus == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] {campus};
                var admissionid = UtilityFunctions.ParseAdmIDs(RegNo, CampusIds);

                ids = db.Results
                    .Where(w => admissionid.Contains(w.AdmissionID) && campid.Contains(w.Admission.CampuseID) &&
                                w.ExamHeldID == examid).Select(s => s.ID).ToArray();
                if (ids.Count() == 0)
                {
                    var tables = new DataTable();
                    return tables;
                }
            }

            #endregion
            if(ids.Count()==0)
            {
                var tables = new DataTable();
                return tables;
                //throw new Exception("No Result found.");
            }
            #region load results

            var rl = new ResultSystem(" (dbo.Results.ID IN (" +
                                                                     string.Join(",",
                                                                         ids.Select(s => s.ToString().Trim())
                                                                             .Distinct()) + " ))",ShowPosition: showposition);
            var tab = rl.GetData();
            tab.Columns.Add("ForMonthYear");
            tab.Columns.Add("SubjectPos");
            tab.Columns.Add("StudentPhoto");
            tab.Columns.Add("c3");
            if (tab.Rows.Count == 0)
                return tab;

            var exam = tab.Rows[0]["ExamName2"].ToString();
            var ExYear = db.ExamHelds.Where(w => w.ID == examid).FirstOrDefault().EntryDate;
            

            #endregion

            #region Subject position

            var distinctCampus = tab.AsEnumerable().Select(s => new
                {
                    campus = int.Parse(s["CampusID"].ToString()), classSub = int.Parse(s["ClassSubjectID"].ToString())
                })
                .DistinctBy(d => d.classSub);
            

            #endregion

            tab.Columns.Add("Logo", typeof(byte[]));
            tab.Columns.Add("StudentImage", typeof(byte[]));
            tab.Columns.Add("BarCode", typeof(byte[]));
            tab.Columns.Add("CampuseID", typeof(string));
            //var logo = db.SchoolLogoes.DefaultIfEmpty().FirstOrDefault().LOGO;
            float percentage = 0;

            var sig = db.SchoolLogoes.Where(w => w.ID == 3).Select(s => s.LOGO).FirstOrDefault();

            tab.Columns.Add("posArrange", typeof(int));
            for (var j = 0; j < tab.Rows.Count; j++)
            {
                var r = tab.Rows[j];
                if (sig != null) r["Logo"] = sig;

                if (omit == "true")
                    if (r["Attendance"].ToString().Trim() == string.Empty || r["Attendance"].ToString().Trim() == "" ||
                        r["Attendance"] == null)
                        try
                        {
                            var attendance = _attendanceHelper.GetYearlyAttendance(int.Parse(r["StudentID"].ToString()), ExYear, 'p');
                            r["Attendance"] = attendance;
                            //MessageBox.Show(tab.Rows[0]["Attendance"].ToString() + " " + tab.Rows.Count.ToString());
                            percentage = _attendanceHelper.getpercentage();
                            var remarksDet = db.ExamRemarksDetails.ToList();
                            if (percentage >= 95 && percentage <= 100)
                                r["Punctuality"] = remarksDet.Where(w => w.Code == 1).FirstOrDefault().Remarks;
                            if (percentage >= 90 && percentage < 95)
                                r["Punctuality"] = remarksDet.Where(w => w.Code == 2).FirstOrDefault().Remarks;
                            if (percentage >= 80 && percentage < 90)
                                r["Punctuality"] = remarksDet.Where(w => w.Code == 3).FirstOrDefault().Remarks;
                            if (percentage >= 70 && percentage < 80)
                                r["Punctuality"] = remarksDet.Where(w => w.Code == 4).FirstOrDefault().Remarks;
                            if (percentage < 70)
                                r["Punctuality"] = remarksDet.Where(w => w.Code == 5).FirstOrDefault().Remarks;
                        }
                        catch
                        {
                        }

                r["CampuseID"] = r["CampusID"].ToString();
                var reg = int.Parse(r["RegistrationNo"].ToString());
                
                r["ForMonthYear"] = exam.Remove(0, tab.Rows[0]["ExamName"].ToString().Trim().Length).Trim();
                if (showposition)
                {
                    var result = 0;
                    var pos = r["SecPos"].ToString().Split('/')[0];
                    if (int.TryParse(pos, out result))
                    {
                        r["posArrange"] = result==0 ? 500000 : result;
                        r["SecPos"]=result.ToPosition();
                    }
                    else r["posArrange"] = 5000000;
                }
            }

            if (!tab.Columns.Contains("c17"))
                tab.Columns.Add("c17"); // for serial no
            tab.Columns.Add("c18"); // for grade
            var disitingRegNo = tab.AsEnumerable().Select(s =>new { regno = int.Parse(s["RegistrationNo"].ToString()), stdid= int.Parse(s["StudentID"].ToString()) }).DistinctBy(b=>b.regno);
            //int ii = 0;

            var stdIds = tab.AsEnumerable().Select(s => int.Parse(s["StudentID"].ToString())).Distinct();

            var photTable = dba.CreateTable(
                "SELECT dbo.StudentPhotos.StudentImage,dbo.StudentPhotos.PhotoYear,dbo.StudentPhotos.StudentID FROM dbo.StudentPhotos  WHERE ( dbo.StudentPhotos.StudentID in (" +
               string.Join(",", stdIds) + ")) ");
            var rule = tab.AsEnumerable().FirstOrDefault();

            foreach (var student in disitingRegNo)
            {
                var sno = 1;
                foreach (var s in tab.AsEnumerable().Where(w => w["RegistrationNo"].ToString() == student.regno.ToString())
                    .OrderBy(o => int.Parse(o["ClassSubjectID"].ToString())))
                {
                    s["c17"] = sno;
                    sno++;
                }

                double Perc = 0;
                double.TryParse(tab.AsEnumerable().Where(w => w["RegistrationNo"].ToString() == student.regno.ToString()).FirstOrDefault()
                            ["TotalPercentage"].ToString(), out Perc);
                

                //var a = dba.CreateTable(
                //    "SELECT dbo.StudentPhotos.StudentImage,dbo.StudentPhotos.PhotoYear FROM dbo.StudentPhotos INNER JOIN dbo.Students ON dbo.StudentPhotos.StudentID = dbo.Students.ID WHERE (dbo.Students.RegistrationNo = " +
                //    reg + ") ");
                if (photTable.Rows.Count > 0)
                {
                    foreach (var r in tab.AsEnumerable().Where(w => w["RegistrationNo"].ToString() == student.regno.ToString()))
                    {
                        var year = r["_Year"].ToString().Trim();
                        //MessageBox.Show(year.ToString());
                        var photo = photTable.AsEnumerable().Where(w => int.Parse(w["StudentID"].ToString())==student.stdid && w["PhotoYear"].ToString().Trim().Contains(year))
                            .Select(s => s["StudentImage"]).FirstOrDefault();
                        if (photo != null)
                        {
                            //   MessageBox.Show(r["Photoyear"].ToString());
                            r["StudentImage"] = photo;
                        }
                        else
                        {
                            var thisPic = photTable.AsEnumerable().Where(w => int.Parse(w["StudentID"].ToString())==student.stdid).Select(s => s["StudentImage"]).FirstOrDefault();
                            if(thisPic!=null)
                            // MessageBox.Show(a.Rows[i]["Photoyear"].ToString());
                            r["StudentImage"] = thisPic; //logo;
                        }
                    }
                }

                var grade = "";
                if (Perc >= double.Parse(rule["APlus"].ToString()))
                    grade = "A+";
                else if (Perc < double.Parse(rule["APlus"].ToString()) && Perc >= double.Parse(rule["A"].ToString()))
                    grade = "A";
                else if (Perc < double.Parse(rule["A"].ToString()) && Perc >= double.Parse(rule["BPlus"].ToString()))
                    grade = "B+";
                else if (Perc < double.Parse(rule["BPlus"].ToString()) && Perc >= double.Parse(rule["B"].ToString()))
                    grade = "B";
                else if (Perc < double.Parse(rule["B"].ToString()) && Perc >= double.Parse(rule["C"].ToString()))
                    grade = "C";
                else if (Perc < double.Parse(rule["C"].ToString()) && Perc >= double.Parse(rule["D"].ToString()))
                    grade = "D";
                else if (Perc < double.Parse(rule["D"].ToString()) && Perc >= double.Parse(rule["E"].ToString()))
                    grade = "E";
                else grade = "F";

                foreach (var g in tab.AsEnumerable().Where(w => w["RegistrationNo"].ToString() == student.regno.ToString()))
                    if (g["TotalStatus"].ToString().ToLower() == "fail")
                        g["c18"] = "F";
                    else g["c18"] = grade;

                foreach (var t in tab.AsEnumerable().Where(w => w["RegistrationNo"].ToString() == student.regno.ToString()))
                    t["BarCode"] = null; //b.GetImageData(BarcodeLib.SaveTypes.JPG);
            }

            try
            {
                if (byclass == "true" && summary == "true")
                {
                    foreach (var d in distinctCampus)
                    {
                        var results = db.Results.Where(w =>
                                w.ExamHeldID == examid && w.ClassSubjectID == d.classSub && w.Admission.CampuseID == d.campus)
                            .Select(s => new
                            {
                                s.ExamHeldID,
                                s.ClassSubjectID,
                                s.Admission.Student.RegistrationNo,
                                s.ObtainedMarks,
                                SubjectPos = "0/0"
                            }).OrderByDescending(o => o.ObtainedMarks).Distinct().AsDataTable();
                        foreach (var absent in results.AsEnumerable()
                            .Where(w => w["ObtainedMarks"].ToString().ToLower() == "2000"))
                            absent["SubjectPos"] = "0";
                        var position = 1;
                        for (var i = 0; i < results.Rows.Count; i++)
                        {
                            var row = results.Rows[i];
                            if (row["ObtainedMarks"].ToString() == "2000")
                            {
                                row["SubjectPos"] = "0";
                                continue;
                            }

                            if (i != 0)
                            {
                                var prow = results.Rows[i - 1];
                                double obt = 0;
                                if (row["ObtainedMarks"].ToString().ToLower() != "a")
                                {
                                    obt = Convert.ToDouble(row["ObtainedMarks"]);
                                }
                                else
                                {
                                    row["SubjectPos"] = "0";
                                    continue;
                                }

                                double tempobt = 0;
                                if (prow["ObtainedMarks"].ToString().ToLower() != "a")
                                    tempobt = Convert.ToDouble(prow["ObtainedMarks"]);
                                if (obt == tempobt)
                                {
                                    row["SubjectPos"] = prow["SubjectPos"];
                                }
                                else
                                {
                                    row["SubjectPos"] = position.ToString();
                                    position++;
                                }
                            }
                            else
                            {
                                if (row["ObtainedMarks"].ToString().ToLower() == "a")
                                {
                                    row["SubjectPos"] = "0";
                                    continue;
                                }

                                row["SubjectPos"] = position.ToString();
                                position++;
                            }
                        } //end of for loop

                        foreach (var dr in
                            tab.AsEnumerable().Where(w => w["ClassSubjectID"].ToString() == d.classSub.ToString()))
                            dr["SubjectPos"] = results.AsEnumerable()
                                .Where(w => w["RegistrationNo"].ToString() == dr["RegistrationNo"].ToString())
                                .Select(s => s["SubjectPos"].ToString()).FirstOrDefault();

                        var count = results.AsEnumerable().Where(w => w["ClassSubjectID"].ToString() == d.classSub.ToString())
                            .Select(s => int.Parse(s["SubjectPos"].ToString())).Max();
                        foreach (var dr in tab.AsEnumerable()
                            .Where(w => w["ClassSubjectID"].ToString() == d.classSub.ToString()))
                            dr["SubjectPos"] += "/" + count;
                    } //end of distinc campus for each
                    var rsum = db.ResultSummaries.Where(w =>
                        w.ExamHeldID == examid &&
                        w.CampusName == db.Campuses.Where(ww => ww.ID == campus).Select(s => s.CampusName)
                            .FirstOrDefault() &&
                        w.ClassName == db.Classes.Where(ww => ww.ID == Class).Select(s => s.ClassName)
                            .FirstOrDefault() && w.SectionName == db.Sections.Where(ww => ww.ID == Section)
                            .Select(s => s.SectionName).FirstOrDefault()).FirstOrDefault();
                    if (rsum == null)
                    {
                        rsum = new ResultSummary();
                        var rsID = 0;
                        try
                        {
                            rsID = db.ResultSummaries.Select(s => s.ID).Max();
                        }
                        catch
                        {
                        }

                        rsID++;
                        rsum.ID = rsID;
                        rsum.ExamHeldID = examid;
                        rsum.CampusName = db.Campuses.Where(ww => ww.ID == campus).Select(s => s.CampusName)
                            .FirstOrDefault();
                        rsum.ClassName = db.Classes.Where(ww => ww.ID == Class).Select(s => s.ClassName)
                            .FirstOrDefault();
                        rsum.SectionName = db.Sections.Where(ww => ww.ID == Section).Select(s => s.SectionName)
                            .FirstOrDefault();
                        db.ResultSummaries.Add(rsum);
                    }

                    var enrolled = disitingRegNo.Count();
                    rsum.Enrollment = enrolled;
                    rsum.Appeared = enrolled;
                    var totalFailed = tab.AsEnumerable().Where(w => w["TotalStatus"].ToString().ToLower() == "fail")
                        .Select(s => int.Parse(s["RegistrationNo"].ToString())).Distinct().Count();
                    var totalPass = tab.AsEnumerable().Where(w => w["TotalStatus"].ToString().ToLower() == "pass")
                        .Select(s => int.Parse(s["RegistrationNo"].ToString())).Distinct().Count();
                    rsum.Fail = totalFailed;
                    rsum.Success = totalPass;
                    rsum.Percentage = enrolled > 0 ? totalPass / enrolled * 100 : 0;
                    rsum.SB1 = tab.AsEnumerable().Where(r => r["SubjectPos"].ToString().Split('/')[0].Trim() == "1")
                        .Count();
                    rsum.SB2 = tab.AsEnumerable().Where(r => r["SubjectPos"].ToString().Split('/')[0].Trim() == "2")
                        .Count();
                    rsum.SB3 = tab.AsEnumerable().Where(r => r["SubjectPos"].ToString().Split('/')[0].Trim() == "3")
                        .Count();

                    db.SaveChanges();
                }
            }
            catch (Exception)
            {
            }

            //var sort = "RegistrationNo";

            //if (student.ToLower() == "RegistrationNo".ToLower())
            //    sort = "RegistrationNo";
            //else if (student.ToLower() == "ClassPosition".ToLower())
            //    sort = "ClassPos1";
            //else if (student.ToLower() == "SectionPosition".ToLower())
            //    sort = "SecPos1";

            //if (!string.IsNullOrEmpty(subject))
            //    if (subject.ToLower() == "SubjectName".ToLower())
            //        sort += ", SubjectName";

            try
            {
                if (showposition)
                {
                    tab.Columns["ClassPos"].ColumnName = "c2";
                    tab.Columns["SecPos"].ColumnName = "c1";
                }
                tab.Columns["AtLeastPercentage"].ColumnName = "PassMarks";
                tab.Columns["TotalObtained"].ColumnName = "Total";
                tab.Columns["GrantTotal"].ColumnName = "c14";
            }
            catch
            {
            }
            //else if (cmbSortSubject.Text == "Exam Date")
            //    sort += ", ExamDate";
            //DataView dv = tab.AsEnumerable().AsDataView();//.OrderBy(o => int.Parse(o["IDs"].ToString())).AsDataView();
            return tab;
            //return GenerateReport("~/Reports/IDCard.rdlc", data);
        }
        //public byte[] RenderBarcode(string text, int width = 280, int height = 80)
        //{
        //    Image img = null;
        //    using (var ms = new MemoryStream())
        //    {
        //        var writer = new ZXing.BarcodeWriter() { Format = BarcodeFormat.CODE_128 };
        //        writer.Options.Height = height;
        //        writer.Options.Width = width;
        //        writer.Options.PureBarcode = true;
        //        img = writer.Write(text);
        //        img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
        //        return ms.ToArray();
        //    }
        //}

    }
}