using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using SchoolManagementSystem.Helpers;
using Syncfusion.EJ2.Base;
using smsCore.Data.Helpers;

namespace smsCore.Controllers
{
    [Authorize]
    public class ExamsReportController : BaseController
    {
        private readonly SchoolEntities db ;
        private readonly DatabaseAccessSql dba;
        public ExamsReportController(SchoolEntities _db, DatabaseAccessSql _dba)
        {
            db = _db;
            dba = _dba;
        }

        // GET: ExamsReport
        public IActionResult Index()
        {
            return View();
        }

        
        public IActionResult DateSheet()
        {
            return View();
        }

        
        public IActionResult MarkSheet()
        {
            return View();
        }

        
        public IActionResult ResultGazzete()
        {
            return View();
        }

        public JsonResult GetResultGazzeteforDatatable(int Classes = -1, int Sections = -1, int campus = -1,
            int exams = -1, string dob = "", string summary = "")
        {
            var classids = Classes == -1 ? 0 : Classes;
            var campusids = campus == -1 ? 0 : campus;
            var secid = Sections == -1
                ? 0
                : db.ClassSections.Where(w => w.ClassID == Classes && w.SectionID == Sections).Select(s => s.ID)
                    .FirstOrDefault();

            var AdmIds = db.Results
                .Where(w => w.ExamHeldID == exams && w.Admission.CampuseID == campusids &&
                            w.Admission.ClassSectionID == secid).Select(s => s.ID).ToArray();

            var rs = new ResultSystem("(dbo.Results.ID IN (" +
                                      string.Join(",", AdmIds.Select(s => s.ToString().Trim())) + " ))");
            var tab = rs.GetData();
            if (tab != null)
            {
                string[] cols = {"posArrange", "RegistrationNo", "StudentName", "FName", "SectionName"};
                var pt = new Pivot(tab);
                var dt = new DataTable();
                if (dob == "true")
                    dt = pt.PivotData("ObtainedMarks", AggregateFunction.First,
                        new[]
                        {
                            "RegistrationNo", "StudentName", "DOB",
                            tab.Columns.Contains("GroupName") ? "GroupName" : "FName", "SectionName", "TotalObtained",
                            "ClassPos", "SecPos", "GrantTotal", "TotalPercentage", "TotalStatus"
                        }, new[] {"SubjectName"});
                else
                    dt = pt.PivotData("ObtainedMarks", AggregateFunction.First,
                        new[]
                        {
                            "RegistrationNo", "StudentName", tab.Columns.Contains("GroupName") ? "GroupName" : "FName",
                            "SectionName", "TotalObtained", "ClassPos", "SecPos", "GrantTotal", "TotalPercentage",
                            "TotalStatus"
                        }, new[] {"SubjectName"});
                var lastordinal = dt.Columns.Count;

                dt.Columns["TotalObtained"].SetOrdinal(lastordinal - 1);
                dt.Columns["GrantTotal"].SetOrdinal(lastordinal - 1);
                dt.Columns["TotalPercentage"].SetOrdinal(lastordinal - 1);
                dt.Columns["SecPos"].SetOrdinal(lastordinal - 1);
                dt.Columns["ClassPos"].SetOrdinal(lastordinal - 1);
                dt.Columns["TotalStatus"].SetOrdinal(lastordinal - 1);
                if (dt.Columns.Contains("GroupName"))
                    dt.Columns["GroupName"].ColumnName = " Group";
                dt.Columns["TotalObtained"].ColumnName = "Obtained";
                dt.Columns["GrantTotal"].ColumnName = "Total";
                dt.Columns["TotalStatus"].ColumnName = "Remarks";
                dt.Columns["TotalPercentage"].ColumnName = "%age";
                var par = new Dictionary<string, object>();
                var exname = db.ExamHelds.Where(w => w.ID == exams).Select(s => s.Exam.ExamName).FirstOrDefault();
                var heldin = db.ExamHelds.Where(w => w.ExamID == exams).Select(s => s.ID).FirstOrDefault().ToString();
                par.Add("examName", exname.Split(':')[0].Trim());
                par.Add("heldIn", heldin);
                par.Add("className", db.Classes.Where(w => w.ID == classids).Select(s => s.ClassName).FirstOrDefault());
                par.Add("sectionName",
                    db.Sections.Where(w => w.ID == secid).Select(s => s.SectionName).FirstOrDefault());
                par.Add("campusName",
                    db.Campuses.Where(w => w.ID == campusids).Select(s => s.CampusName).FirstOrDefault());
                double totalap = dt.AsEnumerable().Where(w => w["Remarks"].ToString().ToLower() != "absent").Count();
                double totalab = dt.AsEnumerable().Where(w => w["Remarks"].ToString().ToLower() == "absent").Count();

                double pass = dt.AsEnumerable().Where(w => w["Remarks"].ToString().ToLower() == "pass").Count();
                var ptt = pass / totalap * 100;
                var percentage = decimal.Round(decimal.Parse(ptt.ToString()), 2).ToString();
                var fail = dt.AsEnumerable().Where(w => w["Remarks"].ToString().ToLower() == "fail").Count().ToString();
                par.Add("status",
                    "Total Enrollment: " + dt.Rows.Count + "  Total Appeared: " + totalap + "\n PASS: " + pass +
                    "     FAIL: " + fail + "     Absent: " + totalab);
                par.Add("passpercentage", "Pass percentage: " + percentage);

                #region summary prepration

                if (summary == "true")
                {
                    var rsum = db.ResultSummaries.Where(w =>
                            w.ExamHeldID == exams &&
                            w.CampusName.Trim().ToLower() == db.Campuses.Where(ww => ww.ID == campusids)
                                .Select(s => s.CampusName).FirstOrDefault().ToLower() &&
                            w.ClassName.Trim().ToLower() == db.Classes.Where(ww => ww.ID == classids)
                                .Select(s => s.ClassName).FirstOrDefault().ToLower() &&
                            w.SectionName.Trim().ToLower() ==
                            db.Sections.Where(ww => ww.ID == secid).Select(s => s.SectionName).FirstOrDefault()
                                .ToLower())
                        .FirstOrDefault();
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
                        rsum.ExamHeldID = int.Parse(exams.ToString());
                        rsum.CampusName = db.Campuses.Where(ww => ww.ID == campusids).Select(s => s.CampusName)
                            .FirstOrDefault();
                        rsum.ClassName = db.Classes.Where(ww => ww.ID == classids).Select(s => s.ClassName)
                            .FirstOrDefault();
                        rsum.SectionName = db.Sections.Where(ww => ww.ID == secid).Select(s => s.SectionName)
                            .FirstOrDefault();
                        db.ResultSummaries.Add(rsum);
                    }

                    rsum.Enrollment = dt.Rows.Count;
                    rsum.Appeared = int.Parse(totalap.ToString());
                    rsum.Fail = int.Parse(fail);
                    rsum.Percentage = double.Parse(percentage);
                    rsum.Success = int.Parse(pass.ToString());
                    rsum.C1 = dt.AsEnumerable().Where(r => r["ClassPos"].ToString().Split('/')[0] == "1").Count();
                    rsum.C2 = dt.AsEnumerable().Where(r => r["ClassPos"].ToString().Split('/')[0] == "2").Count();
                    rsum.C3 = dt.AsEnumerable().Where(r => r["ClassPos"].ToString().Split('/')[0] == "3").Count();
                    rsum.S1 = dt.AsEnumerable().Where(r => r["SecPos"].ToString().Split('/')[0] == "1").Count();
                    rsum.S2 = dt.AsEnumerable().Where(r => r["SecPos"].ToString().Split('/')[0] == "2").Count();
                    rsum.S3 = dt.AsEnumerable().Where(r => r["SecPos"].ToString().Split('/')[0] == "3").Count();
                    try
                    {
                        db.SaveChanges();
                    }
                    catch
                    {
                    }
                }

                #endregion

                dt.Columns.Add("posArrange");
                foreach (DataRow r in dt.Rows)
                {
                    var result = 0;
                    var pos = r["SecPos"].ToString().Split('/')[0];
                    if (int.TryParse(pos, out result))
                        r["posArrange"] = r["SecPos"].ToString().Split('/')[0];
                    else r["posArrange"] = "5000000";
                }

                dt.Columns["SecPos"].ColumnName = "Sec_Pos";
                dt.Columns["ClassPos"].ColumnName = "C_Pos";
                //"posArrange", "", "StudentName", "FName", "SectionName"
                dt.Columns["RegistrationNo"].ColumnName = "Reg";
                dt.Columns["StudentName"].ColumnName = "Name";
                dt.Columns["SectionName"].ColumnName = "Section";
                dt.Columns.Remove("posArrange");
                //Converting to PIVOT
                var table_source = dt.AsEnumerable();

                var subjectsArray = tab.AsEnumerable()
                    .Select(s => int.TryParse(s["ClassSubjectID"].ToString().Trim(), out var r) ? r : 0).Distinct()
                    .ToArray();

                var subjectNames = db.ClassSubjects.AsNoTracking().Where(w => subjectsArray.Contains(w.ID))
                    .Select(s => s.Subject.SubjectName).Distinct().ToList();

                return Json(new {Items = dt.ToDynamicList(), dt.Rows.Count, subjectNames}
                    );
            }

            return Json("");
        }

        
        public IActionResult AwardList()
        {
            return View();
        }

        public JsonResult GetAwardList(DataManagerRequest dm,int Classes, int Sections, int campus , int exams,
            int Subjec)
        {
            var secid = db.ClassSections.Where(w => w.CampusID == campus & w.ClassID == Classes && w.SectionID == Sections).Select(s => s.ID)
                    .FirstOrDefault();

                var classSubjectID = db.ClassSubjects.Where(w => w.SubjectID == Subjec && w.ClassID == Classes)
                    .Select(s => s.ID).FirstOrDefault();
                try
                {
                var data = db.Results.AsNoTracking().
                    Where(w => w.Admission.ClassSectionID == secid
                    && w.Admission.CampuseID == campus &&
                    w.ExamHeldID == exams &&
                    w.ClassSubjectID == classSubjectID).Select(s => new
                    {
                        RegNo = s.Admission.Student.RegistrationNo,
                        s.Admission.Student.StudentName,
                        FatherName = s.Admission.Student.FName,
                        ObtainMarks = s.ObtainedMarks
                    });


                DataOperations operation = new DataOperations();

                if (dm.Search != null && dm.Search.Count > 0)
                {
                    data = operation.PerformSearching(data, dm.Search);  //Search
                }
                if (dm.Where != null && dm.Where.Count > 0) //Filtering
                {
                    data = operation.PerformFiltering(data, dm.Where, dm.Where[0].Operator);
                }

                if (dm.Sorted != null && dm.Sorted.Count > 0)
                {
                    data = operation.PerformSorting(data, dm.Sorted);
                }

                if (dm.Skip != 0)
                {
                    if (dm.Sorted.Count == 0)
                    {
                        List<Sort> sort = new List<Sort>() { new Sort { Name = "RegNo", Direction = "ascending" } };
                        data = operation.PerformSorting(data, sort);
                    }

                    data = operation.PerformSkip(data, dm.Skip);
                }

                if (dm.Take != 0)
                {
                    data = operation.PerformTake(data, dm.Take);
                }

                return Json(new { result = data, count = data.Count() });

            }
                catch (Exception aa)
                {
                    return Json(new{Data = aa.Message});
                }
          

        }

        
        public IActionResult AwardListblank()
        {
            return View();
        }

        [Authorize]
        public JsonResult GetAwardListblank(DataManagerRequest dm, int Classes = -1, int Sections = -1, int campus = -1, int exams = -1,
            int Subject = -1)
        {
            var secid = db.ClassSections.Where(w => w.CampusID == campus & w.ClassID == Classes && w.SectionID == Sections).Select(s => s.ID)
                   .FirstOrDefault();

            var classSubjectID = db.ClassSubjects.Where(w => w.SubjectID == Subject && w.ClassID == Classes)
                .Select(s => s.ID).FirstOrDefault();
            try
            {
                var data = db.Results.AsNoTracking().
                    Where(w => w.Admission.ClassSectionID == secid
                    && w.Admission.CampuseID == campus &&
                    w.ExamHeldID == exams &&
                    w.ClassSubjectID == classSubjectID).Select(s => new
                    {
                        RegNo = s.Admission.Student.RegistrationNo,
                        s.Admission.Student.StudentName,
                        FatherName = s.Admission.Student.FName,
                        ObtainMarks ="",
                        Signature = "_____________________"
                    });
                DataOperations operation = new DataOperations();

                if (dm.Search != null && dm.Search.Count > 0)
                {
                    data = operation.PerformSearching(data, dm.Search);  //Search
                }
                if (dm.Where != null && dm.Where.Count > 0) //Filtering
                {
                    data = operation.PerformFiltering(data, dm.Where, dm.Where[0].Operator);
                }

                if (dm.Sorted != null && dm.Sorted.Count > 0)
                {
                    data = operation.PerformSorting(data, dm.Sorted);
                }

                if (dm.Skip != 0)
                {
                    if (dm.Sorted.Count == 0)
                    {
                        List<Sort> sort = new List<Sort>() { new Sort { Name = "RegNo", Direction = "ascending" } };
                        data = operation.PerformSorting(data, sort);
                    }

                    data = operation.PerformSkip(data, dm.Skip);
                }

                if (dm.Take != 0)
                {
                    data = operation.PerformTake(data, dm.Take);
                }

                return Json(new { result = data, count = data.Count() });

            }
            catch (Exception aa)
            {
                return Json(new { Data = aa.Message });
            }
        }
        public IActionResult weeklyAwardListblank()
        {
            return View();
        }

        //public JsonResult GetweeklyAwardListblank(int Classes, int Sections, int campus, string group = "")
        //{
          
        //    ///Need to do from start
        //    var classids = Classes == -1 ? 0 : Classes;
        //    var campusids = campus == -1 ? 0 : campus;
        //    var secid = Sections == -1
        //        ? db.ClassSections.Select(s => s.ID).ToArray()
        //        : new[]
        //        {
        //            db.ClassSections
        //                .Where(w => w.ClassID == classids && w.SectionID == Sections && w.CampusID == campusids)
        //                .Select(s => s.ID).FirstOrDefault()
        //        };

        //    //int[] secid = new int[] { };
        //    if (Sections == 0)
        //    {
        //        if (classids > 0)
        //            secid = db.ClassSections.Where(w => w.ClassID == classids && w.CampusID==campusids).Select(s => s.ID).ToArray();
        //        else
        //            secid = db.ClassSections.Select(s => s.ID).ToArray();
        //    }

        //    var tab = new DataTable();

        //    #region awardlist codes

        //    tab = db.Admissions
        //        .Where(w => w.IsExpell == false && secid.Contains(w.ClassSection.Section.ID) &&
        //                    w.ClassSection.ClassID == classids && w.CampuseID == campusids).ToArray()
        //        .Select(s => new
        //            {c2 = s.Student.RegistrationNo, c3 = s.Student.StudentName, s.ClassSection.Class.ClassName})
        //        .OrderBy(o => o.c2).ToList().AsDataTable();
        //    string[] grps = {group};
        //    if (group != "")
        //    {
        //        tab = new DataTable();
        //        if (group == "0" || group.ToLower() == "all" || group == string.Empty)
        //            //MessageBox.Show("in dec");
        //            grps = new[] {"Pre Medical", "Pre Engineering", "Computer Science"};
        //        tab = db.Admissions
        //            .Where(w => w.IsExpell == false && secid.Contains(w.ClassSection.Section.ID) &&
        //                        w.ClassSection.ClassID == classids && w.CampuseID == campusids &&
        //                        w.StudentPreviousEducations.Where(ww => grps.Contains(ww.PreviousGroup)).Count() > 0)
        //            .Select(s => new
        //                {c2 = s.Student.RegistrationNo, c3 = s.Student.StudentName, s.ClassSection.Class.ClassName})
        //            .OrderBy(o => o.c2).ToList().AsDataTable();
        //    }

        //    #endregion

        //    if (tab.Rows.Count < 1) return new JsonResult {Data = ""};
        //    tab.Columns.Add("c1");
        //    var sno = 1;
        //    foreach (DataRow r in tab.Rows)
        //    {
        //        r["c1"] = sno.ToString();
        //        sno++;
        //    }

        //    tab.Columns["c1"].SetOrdinal(0);
        //    tab.Columns.Add("MonthName");
        //    tab.Columns.Add("CampusName");
        //    tab.Columns.Add("SectionName");
        //    tab.Columns.Add("ExamName");
        //    tab.Columns.Add("GroupName");

        //    var weeklyList = new JsonResult
        //        {Data = tab.ToDynamicList(), JsonRequestBehavior = JsonRequestBehavior.AllowGet};
        //    return weeklyList;
        //}

        
        public IActionResult RemarksEntrySheet()
        {
            return View();
        }

        
        public IActionResult RemarksSheet()
        {
            return View();
        }

        public JsonResult GetRemarksSheet(DataManagerRequest dm,int Classes, int Sections, int campus, int exams, string group = "",
            string remarks = "false")
        {
            var secid =new[]
                {
                    db.ClassSections
                        .Where(w => w.ClassID == Classes && w.SectionID == Sections && w.CampusID == campus)
                        .Select(s => s.ID).FirstOrDefault()
                };

            //int[] secid = new int[] { };
            if (Sections == 0)
            {
                if (Classes > 0)
                    secid = db.ClassSections.Where(w => w.ClassID == Classes ).Select(s => s.ID).ToArray();
                else
                    secid = db.ClassSections.Select(s => s.ID).ToArray();
            }
            string[] grps = {group};
            //if (group != "")
            //{

            //    if (group == "0" || group.ToLower() == "all" || group == string.Empty)
            //    {
            //        //MessageBox.Show("in dec");
            //        grps = new[] {"Pre Medical", "Pre Engineering", "Computer Science"};
            //    }

            //    tab = db.Admissions
            //        .Where(w => w.IsExpell == false && secid.Contains(w.ClassSection.Section.ID) &&
            //                    w.ClassSection.ClassID == Classes && w.CampuseID ==campus &&
            //                    w.StudentPreviousEducations.Where(ww => grps.Contains(ww.PreviousGroup)).Count() > 0)
            //        .Select(s => new
            //            {c2 = s.Student.RegistrationNo, c3 = s.Student.StudentName, s.ClassSection.Class.ClassName})
            //        .OrderBy(o => o.c2).ToList().AsDataTable();
            //}
            //else
            //{
            //    secid = new[] {int.Parse(Sections.ToString())};
            //    //sectionid[0] = int.Parse(cmbSection.SelectedValue.ToString());
            //}


            var stuData = db.Admissions.Where(w => w.IsExpell == false && secid.Contains(w.ClassSection.SectionID) && w.ClassSection.ClassID == Classes && w.CampuseID == campus).Select(s => new
            {
               s.Student.RegistrationNo,
               s.Student.StudentName,
               s.Student.FName,
                SocialBhv = s.ExamRemarks.Where(w => w.ExamHeldID == exams).DefaultIfEmpty().FirstOrDefault().SocBeh,
                Discpline = s.ExamRemarks.Where(w => w.ExamHeldID == exams).DefaultIfEmpty().FirstOrDefault().Discpline,
                Manners = s.ExamRemarks.Where(w => w.ExamHeldID == exams).DefaultIfEmpty().FirstOrDefault().Manners,
                OralExp = s.ExamRemarks.Where(w => w.ExamHeldID == exams).DefaultIfEmpty().FirstOrDefault().OralExp,
                Confid = s.ExamRemarks.Where(w => w.ExamHeldID == exams).DefaultIfEmpty().FirstOrDefault().Confid,
                Punctuality = s.ExamRemarks.Where(w => w.ExamHeldID == exams).DefaultIfEmpty().FirstOrDefault().Punctuality,
                PhyFit = s.ExamRemarks.Where(w => w.ExamHeldID == exams).DefaultIfEmpty().FirstOrDefault().PhyFit,
                Neatness = s.ExamRemarks.Where(w => w.ExamHeldID == exams).DefaultIfEmpty().FirstOrDefault().Neatness,
                Attendance = s.ExamRemarks.Where(w => w.ExamHeldID == exams).DefaultIfEmpty().FirstOrDefault().Attendance,
                Remarks = s.ExamRemarks.Where(w => w.ExamHeldID == exams).DefaultIfEmpty().FirstOrDefault().Remarks
            });
            DataOperations operation = new DataOperations();

            if (dm.Search != null && dm.Search.Count > 0)
            {
                stuData = operation.PerformSearching(stuData, dm.Search);  //Search
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                stuData = operation.PerformFiltering(stuData, dm.Where, dm.Where[0].Operator);
            }

            if (dm.Sorted != null && dm.Sorted.Count > 0)
            {
                stuData = operation.PerformSorting(stuData, dm.Sorted);
            }

            if (dm.Skip != 0)
            {
                if (dm.Sorted.Count == 0)
                {
                    List<Sort> sort = new List<Sort>() { new Sort { Name = "RegistrationNo", Direction = "ascending" } };
                    stuData = operation.PerformSorting(stuData, sort);
                }

                stuData = operation.PerformSkip(stuData, dm.Skip);
            }

            if (dm.Take != 0)
            {
                stuData = operation.PerformTake(stuData, dm.Take);
            }

            return Json(new { result = stuData, count = stuData.Count() });

        }

        
        public IActionResult subjectlistwithnoresult()
        {
            return View();
        }

        public JsonResult Getsubjectlistwithnoresultblank(int campusid = -1, int exams = -1, string cbxSection = "",
            string cbxgreater = "")
        {
            if (exams == -1) return Json(new { Data = "" });
            //int[] CampusIds=campusid=0?db.Campuses.Select(s=>s.ID).ToArray():new { int[] campusid };
            var q =
                "SELECT DISTINCT TOP (100) PERCENT dbo.Classes.ID as ClassID, dbo.Campuses.ID as CampusID,dbo.Campuses.CampusName, dbo.Classes.ClassName, dbo.Sections.SectionName,dbo.ClassSections.ID as ClassSectionID, dbo.Subjects.SubjectName,  dbo.ClassSubjects.ID AS ClassSubjectID, dbo.Admissions.ID AS AdmissionID FROM            dbo.ClassSections INNER JOIN dbo.Admissions ON dbo.ClassSections.ID = dbo.Admissions.ClassSectionID INNER JOIN dbo.Campuses ON dbo.Admissions.CampuseID = dbo.Campuses.ID INNER JOIN dbo.Classes ON dbo.ClassSections.ClassID = dbo.Classes.ID INNER JOIN dbo.Sections ON dbo.ClassSections.SectionID = dbo.Sections.ID INNER JOIN dbo.ClassSubjects ON dbo.Classes.ID = dbo.ClassSubjects.ClassID INNER JOIN dbo.Subjects ON dbo.ClassSubjects.SubjectID = dbo.Subjects.ID WHERE    (dbo.Campuses.CampusName <> '0') AND (dbo.Admissions.IsExpell = 0) ORDER BY dbo.Campuses.CampusName, ClassSubjectID, dbo.Classes.ClassName, dbo.Subjects.SubjectName";
            var tab = dba.CreateTable(q);
            var tabReq = new DataTable();
            tabReq.Columns.Add("S No");
            tabReq.Columns.Add("Campus Name");
            tabReq.Columns.Add("Class Name");
            tabReq.Columns.Add("Section Name");
            tabReq.Columns.Add("Subject Name");
            tabReq.Columns.Add("Teacher Name");
            tabReq.Columns.Add("Delay");
            if (cbxSection == "true")
            {
                var SubjectIDs = tab.AsEnumerable().Select(s => new
                {
                    ClassID = int.Parse(s["ClassID"].ToString()),
                    ClassSectionID = int.Parse(s["ClassSectionID"].ToString()),
                    ClassSubjectID = int.Parse(s["ClassSubjectID"].ToString()),
                    CampusID = int.Parse(s["CampusID"].ToString()), CampusName = s["CampusName"].ToString(),
                    ClassName = s["ClassName"].ToString(), SectionName = s["SectionName"].ToString()
                }).Distinct();
                var i = 1;
                foreach (var r in SubjectIDs)
                {
                    var AdmissionIDs = tab.AsEnumerable().Where(w =>
                            int.Parse(w["ClassSubjectID"].ToString()) == r.ClassSubjectID &&
                            w["CampusName"].ToString() == r.CampusName && w["ClassName"].ToString() == r.ClassName &&
                            w["SectionName"].ToString() == r.SectionName)
                        .Select(s => int.Parse(s["AdmissionID"].ToString()));
                    var results = db.Results.Where(w =>
                        w.ExamHeldID == exams && AdmissionIDs.Contains(w.AdmissionID) &&
                        w.ClassSubjectID == r.ClassSubjectID).Select(s => s.ID);

                    if (results.Count() < 1)
                    {
                        var classID = db.ClassSubjects.Where(w => w.ID == r.ClassSubjectID).Select(s => s.ClassID)
                            .FirstOrDefault();
                        var SubjectID = db.ClassSubjects.Where(w => w.ID == r.ClassSubjectID).Select(s => s.SubjectID)
                            .FirstOrDefault();

                        var examDate = db.ExamDates.Where(w =>
                                w.ExamHeldID == exams && w.ClassSectionID == r.ClassSectionID &&
                                w.SubjectID == SubjectID)
                            .FirstOrDefault();
                        if (examDate != null)
                        {
                            var delay = DateTime.Today.Date.Subtract(examDate.ExamDate1).Days;
                            if (cbxgreater == "true" && delay > 0)
                            {
                                var dr = tabReq.NewRow();
                                tabReq.Rows.Add(dr);
                                dr["S No"] = i;
                                dr["Campus Name"] = r.CampusName;
                                dr["Class Name"] = r.ClassName;
                                dr["Section Name"] = r.SectionName;
                                dr["Subject Name"] = db.ClassSubjects.Where(w => w.ID == r.ClassSubjectID)
                                    .Select(s => s.Subject.SubjectName).FirstOrDefault();
                                dr["Teacher Name"] = db.TeachingSubjects
                                    .Where(w => w.ClassSectionID == r.ClassSectionID && w.CampusID == r.CampusID)
                                    .Select(s => s.tbl_Employee.employeeName).DefaultIfEmpty().FirstOrDefault();
                                dr["Delay"] = delay;
                                i++;
                            }

                            if (cbxgreater == "false")
                            {
                                var dr = tabReq.NewRow();
                                tabReq.Rows.Add(dr);
                                dr["S No"] = i;
                                dr["Campus Name"] = r.CampusName;
                                dr["Class Name"] = r.ClassName;
                                dr["Section Name"] = r.SectionName;
                                dr["Subject Name"] = db.ClassSubjects.Where(w => w.ID == r.ClassSubjectID)
                                    .Select(s => s.Subject.SubjectName).FirstOrDefault();
                                dr["Teacher Name"] = db.TeachingSubjects
                                    .Where(w => w.ClassSectionID == r.ClassSectionID && w.CampusID == r.CampusID)
                                    .Select(s => s.tbl_Employee.employeeName).DefaultIfEmpty().FirstOrDefault();
                                dr["Delay"] = delay;
                                i++;
                            }
                        }
                    }
                }
            }
            else if (cbxSection == "false")
            {
                tabReq.Columns.Remove("Section Name");
                var SubjectIDs = tab.AsEnumerable().Select(s => new
                {
                    ClassID = int.Parse(s["ClassID"].ToString()),
                    ClassSectionID = int.Parse(s["ClassSectionID"].ToString()),
                    ClassSubjectID = int.Parse(s["ClassSubjectID"].ToString()),
                    CampusID = int.Parse(s["CampusID"].ToString()), CampusName = s["CampusName"].ToString(),
                    ClassName = s["ClassName"].ToString()
                }).Distinct();
                var i = 1;
                foreach (var r in SubjectIDs)
                {
                    var AdmissionIDs = tab.AsEnumerable()
                        .Where(w => int.Parse(w["ClassSubjectID"].ToString()) == r.ClassSubjectID &&
                                    w["CampusName"].ToString() == r.CampusName &&
                                    w["ClassName"].ToString() == r.ClassName)
                        .Select(s => int.Parse(s["AdmissionID"].ToString()));
                    var results = db.Results.Where(w =>
                        w.ExamHeldID == exams && AdmissionIDs.Contains(w.AdmissionID) &&
                        w.ClassSubjectID == r.ClassSubjectID).Select(s => s.ID);

                    if (results.Count() < 1)
                    {
                        var classID = db.ClassSubjects.Where(w => w.ID == r.ClassSubjectID).Select(s => s.ClassID)
                            .FirstOrDefault();
                        var SubjectID = db.ClassSubjects.Where(w => w.ID == r.ClassSubjectID).Select(s => s.SubjectID)
                            .FirstOrDefault();

                        var examDate = db.ExamDates.Where(w =>
                                w.ExamHeldID == exams && w.ClassSectionID == r.ClassSectionID &&
                                w.SubjectID == SubjectID)
                            .FirstOrDefault();
                        if (examDate != null)
                        {
                            var delay = DateTime.Today.Date.Subtract(examDate.ExamDate1).Days;
                            if (cbxgreater == "true" && delay > 0)
                            {
                                var dr = tabReq.NewRow();
                                tabReq.Rows.Add(dr);
                                dr["S No"] = i;
                                dr["Campus Name"] = r.CampusName;
                                dr["Class Name"] = r.ClassName;
                                dr["Subject Name"] = db.ClassSubjects.Where(w => w.ID == r.ClassSubjectID)
                                    .Select(s => s.Subject.SubjectName).FirstOrDefault();
                                dr["Teacher Name"] = db.TeachingSubjects
                                    .Where(w => w.ClassSectionID == r.ClassSectionID && w.CampusID == r.CampusID)
                                    .Select(s => s.tbl_Employee.employeeName).DefaultIfEmpty().FirstOrDefault();
                                dr["Delay"] = delay;
                                i++;
                            }

                            if (cbxgreater == "false")
                            {
                                var dr = tabReq.NewRow();
                                tabReq.Rows.Add(dr);
                                dr["S No"] = i;
                                dr["Campus Name"] = r.CampusName;
                                dr["Class Name"] = r.ClassName;
                                dr["Subject Name"] = db.ClassSubjects.Where(w => w.ID == r.ClassSubjectID)
                                    .Select(s => s.Subject.SubjectName).FirstOrDefault();
                                dr["Teacher Name"] = db.TeachingSubjects
                                    .Where(w => w.ClassSectionID == r.ClassSectionID && w.CampusID == r.CampusID)
                                    .Select(s => s.tbl_Employee.employeeName).DefaultIfEmpty().FirstOrDefault();
                                dr["Delay"] = delay;
                                i++;
                            }
                        }
                    }
                }
            }

            var noResult = Json(new
            { Data = tabReq.ToDynamicList() });
            return noResult;
        }

        
        public ActionResult ResultSummary()
        {
            return View();
        }

        public JsonResult GetResultSummaryblank(DataManagerRequest dm,int exams,int campusid)
        {
            string campName = db.Campuses.Where(s => s.ID == campusid).Select(ww => ww.CampusName).FirstOrDefault().ToString();            
            var data = db.ResultSummaries.AsNoTracking().Where(w => w.ExamHeldID == exams && w.CampusName.ToLower()==campName).Select(s => new
            {
                s.ClassName,
                s.SectionName,
                s.CampusName,
                s.Enrollment,
                s.Appeared,
                s.Success,
                s.Fail,
                s.Percentage
            });
            DataOperations operation = new DataOperations();

            if (dm.Search != null && dm.Search.Count > 0)
            {
                data = operation.PerformSearching(data, dm.Search);  //Search
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                data = operation.PerformFiltering(data, dm.Where, dm.Where[0].Operator);
            }

            if (dm.Sorted != null && dm.Sorted.Count > 0)
            {
                data = operation.PerformSorting(data, dm.Sorted);
            }

            if (dm.Skip != 0)
            {
                if (dm.Sorted.Count == 0)
                {
                    List<Sort> sort = new List<Sort>() { new Sort { Name = "RegistrationNo", Direction = "ascending" } };
                    data = operation.PerformSorting(data, sort);
                }

                data = operation.PerformSkip(data, dm.Skip);
            }

            if (dm.Take != 0)
            {
                data = operation.PerformTake(data, dm.Take);
            }

            return Json(new { result = data, count = data.Count() });

        }

        
        public IActionResult ResultSubjectSummary()
        {
            return View();
        }

        public JsonResult GetResultSubjectSummaryblank(DataManagerRequest dm,int exams ,int campusId)
        {
            string campName = db.Campuses.Where(ww => ww.ID==exams).Select(ss=>ss.CampusName).FirstOrDefault().ToString();
          
            var data = db.ResultSummaries.AsNoTracking().Where(w => w.ExamHeldID == exams && w.CampusName== campName).Select(s => new
            {
                s.CampusName,
                s.ClassName,
                s.SectionName,
                S1 = s.S1,
                S2 = s.S2,
                S3 = s.S3,
                C1 = s.C1,
                C2 = s.C2,
                C3 = s.C3,
                SB1 = s.SB1,
                SB2 = s.SB2,
                SB3 = s.SB3,
            });
           
            DataOperations operation = new DataOperations();

            if (dm.Search != null && dm.Search.Count > 0)
            {
                data = operation.PerformSearching(data, dm.Search);  //Search
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                data = operation.PerformFiltering(data, dm.Where, dm.Where[0].Operator);
            }

            if (dm.Sorted != null && dm.Sorted.Count > 0)
            {
                data = operation.PerformSorting(data, dm.Sorted);
            }

            if (dm.Skip != 0)
            {
                if (dm.Sorted.Count == 0)
                {
                    List<Sort> sort = new List<Sort>() { new Sort { Name = "RegistrationNo", Direction = "ascending" } };
                    data = operation.PerformSorting(data, sort);
                }

                data = operation.PerformSkip(data, dm.Skip);
            }

            if (dm.Take != 0)
            {
                data = operation.PerformTake(data, dm.Take);
            }

            return Json(new { result = data, count = data.Count() });

        }

        
        public IActionResult ProgressReport()
        {
            return View();
        }

        public JsonResult GetProgressReportblank(int Classes, int Sections, int campus, string regno = "",
            string year = "", int examsid = -1, string rdobyregno = "false", string rdobyclass = "false")
        {
            var classids = Classes == -1 ? 0 : Classes;
            var campusids = campus == -1 ? 0 : campus;
            var secid = Sections == -1
                ? db.ClassSections.Select(s => s.ID).ToArray()
                : new[]
                {
                    db.ClassSections
                        .Where(w => w.ClassID == classids && w.SectionID == Sections && w.CampusID == campusids)
                        .Select(s => s.ID).FirstOrDefault()
                };

            if (Sections == 0)
            {
                if (classids > 0)
                    secid = db.ClassSections.Where(w => w.ClassID == classids).Select(s => s.ID).ToArray();
                else
                    secid = db.ClassSections.Select(s => s.ID).ToArray();
            }

            if (year == "")
            {
                try
                {
                    var list = db.ResultSummaries.Where(w => w.ExamHeldID == examsid).Select(na => new
                    {
                        na.CampusName,
                        na.ClassName,
                        na.SectionName,
                        na.Enrollment,
                        na.Appeared,
                        na.Success,
                        na.Fail,
                        na.Percentage
                    }).Select(s => new
                    {
                        s.CampusName,
                        Class = s.ClassName,
                        Section = s.SectionName,
                        S1 = s.Enrollment == null ? 0 : Convert.ToDouble(s.Enrollment),
                        S2 = s.Appeared == null ? 0 : Convert.ToDouble(s.Appeared),
                        S3 = s.Success == null ? 0 : Convert.ToDouble(s.Success),
                        SB1 = s.Fail == null ? 0 : Convert.ToDouble(s.Fail),
                        SB2 = s.Percentage == null ? 0 : Convert.ToDouble(s.Percentage)
                    }).ToList();
                    var values = Json(new {Data = list});
                    return values;
                }
                catch
                {
                }
            }
            else
            {
                var listed = db.ResultSummaries.Where(w => w.ExamHeldID == examsid).Select(na => new
                {
                    na.CampusName,
                    na.ClassName,
                    na.SectionName,
                    na.Enrollment,
                    na.Appeared,
                    na.Success,
                    na.Fail,
                    na.Percentage
                }).ToList();
                var lists = Json(new {Data = listed});
                return lists;
            }

            return null;
        }

        public ActionResult ResultGazzete2()
        {
            return View();
        }
    }
}