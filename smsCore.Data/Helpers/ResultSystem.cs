using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Models;
using MoreLinq;

namespace SchoolManagementSystem.Helpers
{

    //SocBeh, Discpline, 
    //Manners, OralExp, CoTeamWork, Confid, Punctuality, PhyFit, 
    //Neatness, Attendance, Remarks
    public class ResultSystem
    {
        private readonly DataTable tab = new DataTable();

        private readonly DatabaseAccessSql obj;
        private readonly SchoolEntities objdb;
        public ResultSystem(DatabaseAccessSql _obj,SchoolEntities _objdb)
        {
            obj = _obj;
            objdb = _objdb;
        }
        /// <summary>
        ///     the only constructor of Result system that will perform all the required calculation of result System
        /// </summary>
        /// <param name="filter">any filter for SQL query excluding Where for WHERE clause</param>
        public ResultSystem(string filter, string sort = "", bool ShowPosition=true)
        {
            var q = "";
            q =
                @"SELECT TOP (100) PERCENT Results.ID AS ResultID, Results.ClassSubjectID,Admissions.ID AS AdmissionID,ClassSubjects.SubjectID, Classes.ClassName,Classes.ID as ClassID,Sections.ID AS SectionID,dbo.ClassSections.ID as ClassSectionID,
                Students.PostalAddress, CONVERT(varchar(10), Students.DOB, 103) AS DOB, Students.StudentName, Admissions.CampuseID AS CampusID,  Students.RegistrationNo, Subjects.SubjectName, 
                ExamDates.TotalMarks, 
                CASE WHEN dbo.Results.ObtainedMarks = 2000 THEN 'A' ELSE CAST(dbo.Results.ObtainedMarks AS varchar(5)) END AS ObtainedMarks, 
                CASE WHEN dbo.Results.ObtainedMarks = 2000 THEN 'Absent' ELSE CASE WHEN CASE WHEN dbo.Results.ObtainedMarks = 2000 THEN 0 ELSE dbo.Results.ObtainedMarks
                END / dbo.ExamDates.TotalMarks * 100 >= dbo.ExamsRules.AtLeastPercentage THEN 'PASS' ELSE 'FAIL' END END AS Status, 
                CASE WHEN dbo.Results.ObtainedMarks = 2000 THEN 0 ELSE dbo.Results.ObtainedMarks END / ExamDates.TotalMarks * 100 AS Percentage,
                Results.CheckedBy, Students.FName,  Sections.SectionName, Exams.ExamName, 
                Exams.ExamName + ' ' + DATENAME(month, ExamHelds.EntryDate) + ' ' + CONVERT(char(4), YEAR(ExamHelds.EntryDate)) AS ExamName2,
                Exams.ExamName + ' ' + DATENAME(month, ExamHelds.EntryDate) AS Exa, 
                YEAR(ExamHelds.EntryDate) AS _Year, 
                ExamHelds.EntryDate, MONTH(ExamHelds.EntryDate) AS _Month,
                ExamsRules.AtLeastPercentage, ExamsRules.APlus, ExamsRules.BPlus,ExamsRules.A, ExamsRules.B, ExamsRules.C, ExamsRules.D, ExamsRules.E, Results.ExamHeldID, Subjects.IsCoreSubject, Classes.ID AS ClassID, ExamRemarks.SocBeh, ExamRemarks.Discpline, 
                ExamRemarks.Manners, ExamRemarks.OralExp, ExamRemarks.CoTeamWork, ExamRemarks.Confid, ExamRemarks.Punctuality, ExamRemarks.PhyFit, 
                ExamRemarks.Neatness, ExamRemarks.Attendance, ExamRemarks.Remarks, Students.ID AS StudentID,
                (SELECT     SUM(CASE WHEN ResultTotal.ObtainedMarks = 2000 THEN 0 ELSE ResultTotal.ObtainedMarks END) AS Expr1
                FROM          dbo.Results AS ResultTotal
                WHERE      (ExamHeldID = dbo.ExamHelds.ID) AND (AdmissionID = dbo.Admissions.ID)) AS TotalObtained,
                (SELECT     SUM(TotalMarks) AS Expr1
                FROM          dbo.ExamDates AS dates
                WHERE      (ClassSectionID = dbo.ClassSections.ID) AND (ExamHeldID = dbo.ExamHelds.ID)) AS GrantTotal, ROUND
                ((SELECT     SUM(CASE WHEN ResultTotal.ObtainedMarks = 2000 THEN 0 ELSE ResultTotal.ObtainedMarks END) AS Expr1
                FROM         dbo.Results AS ResultTotal
                WHERE     (ExamHeldID = dbo.ExamHelds.ID) AND (AdmissionID = dbo.Admissions.ID)) /
                (SELECT     SUM(TotalMarks) AS Expr1
                FROM          dbo.ExamDates AS dates
                WHERE      (ClassSectionID = dbo.ClassSections.ID) AND (ExamHeldID = dbo.ExamHelds.ID)) * 100, 2) AS TotalPercentage
                FROM Subjects INNER JOIN
                Admissions INNER JOIN
                    Students ON Admissions.StudentID = Students.ID INNER JOIN
                    Results ON Admissions.ID = Results.AdmissionID INNER JOIN
                    ClassSubjects ON Results.ClassSubjectID = ClassSubjects.ID ON Subjects.ID = ClassSubjects.SubjectID INNER JOIN
                    ClassSections ON Admissions.ClassSectionID = ClassSections.ID INNER JOIN
                    Classes ON ClassSections.ClassID = Classes.ID INNER JOIN
                    Sections ON ClassSections.SectionID = Sections.ID INNER JOIN
                    ExamHelds ON Results.ExamHeldID = ExamHelds.ID INNER JOIN
                    Exams ON ExamHelds.ExamID = Exams.ID INNER JOIN
                    ExamsRules ON ExamHelds.ID = ExamsRules.ExamHeldID INNER JOIN
                    ExamDates ON Subjects.ID = ExamDates.SubjectID AND ClassSections.ID = ExamDates.ClassSectionID AND ExamHelds.ID = ExamDates.ExamHeldID LEFT OUTER JOIN
                    ExamRemarks ON Admissions.ID = ExamRemarks.AdmissionID AND ExamHelds.ID = ExamRemarks.ExamHeldID  WHERE  " +
                filter + " ORDER BY " + sort + " dbo.Students.RegistrationNo ,Results.ClassSubjectID";
            tab = obj.CreateTable(q);
            if (tab.Rows.Count < 1)
                return;
            //tab.Columns.Add("TotalObtained");
            //tab.Columns.Add("GrantTotal");
            //tab.Columns.Add("TotalPercentage");
            tab.Columns.Add("TotalStatus");


            //with distinctExams we calculate the positions of students for each distinct exams, class, campus
            var distinctExams = tab.AsEnumerable().DistinctBy(d => d["ExamHeldID"]).Select(s =>
                new
                {
                    examid = s["ExamHeldID"].ToString(), ClassSectionId = s["ClassSectionID"].ToString(),
                    clsid = s["ClassID"].ToString(), 
                    campusid = s["CampusID"].ToString(),
                    SecId = s["SectionID"].ToString()
                });
            foreach (var exID in distinctExams)
            {
                var examID = exID.examid;
                var ClassID = exID.clsid;
                var CampusID = exID.campusid;
                var SectionID = exID.SecId;

                var ExamDistinct = tab.AsEnumerable().Where(w =>
                    w["ClassID"].ToString() == ClassID && w["CampusID"].ToString() == CampusID &&
                    w["ExamHeldID"].ToString() == examID);

                //var subjectIds = tab.AsEnumerable().Where(w =>
                //    w["ClassID"].ToString() == ClassID && w["CampusID"].ToString() == CampusID &&
                //    w["ExamHeldID"].ToString() == examID).Select(s => s["SubjectID"].ToString());

                var ruleQuery =
                    @"SELECT SUM(dbo.ExamDates.TotalMarks) AS TotalMarks, dbo.ExamsRules.APlus, dbo.ExamsRules.B, dbo.ExamsRules.C, dbo.ExamsRules.D, dbo.ExamsRules.E, 
                                        dbo.ExamsRules.CoreFail, dbo.ExamsRules.NoneCoreFail, dbo.ExamsRules.CoreWithNoneCoreFail, dbo.ExamsRules.AtLeastPercentage, dbo.ExamsRules.A,  
                                        dbo.ExamsRules.BPlus FROM            dbo.ExamDates INNER JOIN dbo.ExamHelds ON dbo.ExamDates.ExamHeldID = dbo.ExamHelds.ID INNER JOIN dbo.ExamsRules 
                                        ON dbo.ExamHelds.ID = dbo.ExamsRules.ExamHeldID GROUP BY dbo.ExamDates.ClassSectionID, dbo.ExamDates.ExamHeldID, dbo.ExamsRules.APlus, dbo.ExamsRules.B, 
                                        dbo.ExamsRules.C, dbo.ExamsRules.D, dbo.ExamsRules.E, dbo.ExamsRules.CoreFail, dbo.ExamsRules.NoneCoreFail, dbo.ExamsRules.CoreWithNoneCoreFail, 
                                        dbo.ExamsRules.AtLeastPercentage, dbo.ExamsRules.A,  dbo.ExamsRules.BPlus HAVING dbo.ExamDates.ExamHeldID=" +
                    examID + " AND dbo.ExamDates.ClassSectionID=" + exID.ClassSectionId + "";
                //System.Windows.MessageBox.Show(ruleQuery);
                var RuleTable = obj.CreateTable(ruleQuery);
                var GrantTotal = RuleTable.Rows[0][0].ToString();

                var passPercentage = Convert.ToDouble(RuleTable.Rows[0]["AtLeastPercentage"].ToString());
                int FailInCore = short.Parse(RuleTable.Rows[0]["CoreFail"].ToString());
                //int FailInNoneCore = Int16.Parse(RuleTable.Rows[0]["NoneCoreFail"].ToString());
                //int CorewithNoneCore = Int16.Parse(RuleTable.Rows[0]["CoreWithNoneCoreFail"].ToString());
                var posQuery =
                    ""; //SUM(CASE WHEN dbo.Results.ObtainedMarks = 2000 THEN 0 ELSE dbo.Results.ObtainedMarks END) AS ObtainedMarks
                posQuery =
                    "SELECT TOP (100) PERCENT dbo.Students.RegistrationNo, dbo.Results.ExamHeldID, SUM(dbo.Results.ObtainedMarks) AS ObtainedMarks, dbo.ClassSections.ClassID, dbo.ClassSections.SectionID, dbo.Admissions.CampuseID FROM dbo.Admissions INNER JOIN dbo.Students ON dbo.Admissions.StudentID = dbo.Students.ID INNER JOIN dbo.Results ON dbo.Admissions.ID = dbo.Results.AdmissionID INNER JOIN dbo.ClassSections ON dbo.Admissions.ClassSectionID = dbo.ClassSections.ID GROUP BY dbo.Students.RegistrationNo, dbo.Results.ExamHeldID, dbo.ClassSections.ClassID, dbo.ClassSections.SectionID, dbo.Admissions.CampuseID HAVING (dbo.Results.ExamHeldID = " +
                    examID + ") AND (dbo.ClassSections.ClassID = " + ClassID + ") AND (dbo.Admissions.CampuseID=" +
                    exID.campusid + ") ORDER BY ObtainedMarks DESC";
                var distinctAdm = ExamDistinct.DistinctBy(d => d["RegistrationNo"]);
                //region to calculate the total obtained, total percentage and status of each distinct admission

                #region Marks operation

                foreach (var r in distinctAdm)
                {
                    var thisStudentResult = tab.AsEnumerable().Where(w =>
                        w["RegistrationNo"].ToString() == r["RegistrationNo"].ToString() &&
                        w["ExamHeldID"].ToString() == examID);

                    var TotalObtained = decimal.Parse(thisStudentResult.FirstOrDefault()["TotalObtained"].ToString());
                    var totalPercentage =
                        decimal.Parse(thisStudentResult.FirstOrDefault()["TotalPercentage"].ToString());

                    var status = "PASS";
                    var failincore = thisStudentResult.Where(w => w["Status"].ToString().Trim() == "FAIL").Count();


                    //var failinNonecore = thisStudentResult.Where(w =>
                    //    w["Status"].ToString().Trim() == "FAIL" && w["IsCoreSubject"].ToString() == "False").Count();

                    //|| (failinNonecore >= FailInNoneCore || failincore >= CorewithNoneCore)
                    //totalPercentage < decimal.Parse(passPercentage.ToString()) ||
                    //    (failincore > 0 && failincore >= FailInCore)
                    if (failincore > 0 && failincore >= FailInCore)
                        status = "FAIL";

                    foreach (var tot in thisStudentResult)
                        if (tot["ObtainedMarks"].ToString().ToLower() == "a")
                        {
                            foreach (var t in thisStudentResult) t["TotalStatus"] = "Absent";
                            break;
                        }
                        else
                        {
                            tot["TotalStatus"] = status;
                        }


                } //end of disting admission 

                #endregion

                if (ShowPosition)
                {
                    var ClassposTable = obj.CreateTable(posQuery);

                    ClassposTable.Columns.Add("ClassPos");
                    ClassposTable.Columns.Add("SecPos");
                    ClassposTable.Columns.Add("ClassPos1", typeof(int));
                    ClassposTable.Columns.Add("SecPos1", typeof(int));
                    //this region will calculate the position of student in his class

                    #region Find Class Position

                    var position = 1;
                    var distinctmarksInClass = ClassposTable.AsEnumerable()
                        .Select(s => new
                        { regNo = s["RegistrationNo"].ToString(), obt = Convert.ToDouble(s["ObtainedMarks"].ToString()) })
                        .DistinctBy(d => d.obt).OrderByDescending(o => o.obt);
                    foreach (var s in distinctmarksInClass)
                        if (s.obt > 1900)
                        {
                            foreach (var c in ClassposTable.AsEnumerable()
                                .Where(w => Convert.ToDouble(w["ObtainedMarks"].ToString()) > 1900))
                            {
                                c["ClassPos"] = "0";
                                c["ClassPos1"] = 50000;
                            }
                        }
                        else
                        {
                            foreach (var c in ClassposTable.AsEnumerable()
                                .Where(w => Convert.ToDouble(w["ObtainedMarks"].ToString()) == s.obt))
                            {
                                c["ClassPos"] = position; // +"/" + distinctmarksInClass.Count().ToString();
                                c["ClassPos1"] = position;
                            }

                            position++;
                        }

                    #endregion

                    //this region will calculate the position of student in his section

                    #region Find Section Position

                    //var distinctSection = ClassposTable.AsEnumerable().DistinctBy(d => d["SectionID"]).Select(s => int.Parse(s["SectionID"].ToString()));


                    //foreach (var id in distinctSection)
                    {
                        position = 1;
                        var thisSection = ClassposTable.AsEnumerable().Where(w => w["SectionID"].ToString() == SectionID)
                            .OrderByDescending(o => Convert.ToDouble(o["ObtainedMarks"].ToString())).ToArray();
                        var DistinctMarksinThisSection = thisSection
                            .Select(s => new
                            {
                                regNo = s["RegistrationNo"].ToString(),
                                obt = Convert.ToDouble(s["ObtainedMarks"].ToString())
                            }).DistinctBy(d => d.obt).OrderByDescending(o => o.obt);
                        foreach (var s in DistinctMarksinThisSection)
                            if (s.obt > 1900)
                            {
                                foreach (var c in ClassposTable.AsEnumerable()
                                    .Where(w => Convert.ToDouble(w["ObtainedMarks"].ToString()) > 1900))
                                {
                                    c["SecPos"] = "0";
                                    c["SecPos1"] = 10000;
                                }
                            }
                            else
                            {
                                foreach (var c in ClassposTable.AsEnumerable().Where(w =>
                                    Convert.ToDouble(w["ObtainedMarks"].ToString()) == s.obt))
                                {
                                    c["SecPos"] = position; // +"/" + distinctmarksInClass.Count().ToString();
                                    c["SecPos1"] = position;
                                }

                                position++;
                            }
                    } //end of distinct section   Where(w => w["SectionID"].ToString() == id.ToString()).
                    var thisSecMaxPos = ClassposTable.AsEnumerable().Select(s =>
                        int.Parse(s["SecPos"].ToString() == string.Empty ? "0" : s["SecPos"].ToString())).DefaultIfEmpty().Max();
                    var thisClassMaxPos = ClassposTable.AsEnumerable()
                        .Where(w => Convert.ToDouble(w["ObtainedMarks"]) < 1900)
                        .Select(s => int.Parse(s["ClassPos"].ToString())).DefaultIfEmpty().Max();
                    foreach (DataRow rr in ClassposTable.Rows)
                    {
                        rr["SecPos"] += "/" + thisSecMaxPos;
                        rr["ClassPos"] += "/" + thisClassMaxPos;
                    }

                    #endregion

                    tab.Columns.Add("ClassPos");
                    tab.Columns.Add("ClassPos1", typeof(int));
                    tab.Columns.Add("SecPos");
                    tab.Columns.Add("SecPos1", typeof(int));
                    //this loop will assign the position to the rest of all the records of eachs student after FirstOrDefault
                    foreach (var r in distinctAdm)
                        if (ClassposTable.Rows.Count > 0)
                        {
                            var ClassPosition =
                                tab.AsEnumerable().Where(w =>
                                        w["RegistrationNo"].ToString() == r["RegistrationNo"].ToString() &&
                                        w["ExamHeldID"].ToString() == examID).FirstOrDefault()["TotalStatus"].ToString()
                                    .ToLower() == "absent"
                                    ? "Absent"
                                    : ClassposTable.AsEnumerable()
                                        .Where(w => w["RegistrationNo"].ToString() == r["RegistrationNo"].ToString())
                                        .FirstOrDefault()["ClassPos"].ToString();

                            var ClassPostionSort =
                                int.Parse(ClassposTable.AsEnumerable()
                                    .Where(w => w["RegistrationNo"].ToString() == r["RegistrationNo"].ToString())
                                    .FirstOrDefault()["ClassPos1"].ToString());
                            foreach (var tot in tab.AsEnumerable().Where(w =>
                                w["RegistrationNo"].ToString() == r["RegistrationNo"].ToString() &&
                                w["ExamHeldID"].ToString() == examID))
                            {
                                tot["ClassPos"] = ClassPosition;
                                tot["ClassPos1"] = ClassPostionSort;
                            }

                            ClassPosition = "";
                            ClassPosition =
                                tab.AsEnumerable().Where(w =>
                                        w["RegistrationNo"].ToString() == r["RegistrationNo"].ToString() &&
                                        w["ExamHeldID"].ToString() == examID).FirstOrDefault()["TotalStatus"].ToString()
                                    .ToLower() == "absent"
                                    ? "Absent"
                                    : ClassposTable.AsEnumerable().Where(w =>
                                        w["RegistrationNo"].ToString() == r["RegistrationNo"].ToString() &&
                                        w["SectionID"].ToString() == SectionID).FirstOrDefault()["SecPos"].ToString();
                            ClassPostionSort = int.Parse(ClassposTable.AsEnumerable().Where(w =>
                                w["RegistrationNo"].ToString() == r["RegistrationNo"].ToString() &&
                                w["SectionID"].ToString() == SectionID).FirstOrDefault()["SecPos1"].ToString());
                            foreach (var tot in tab.AsEnumerable().Where(w =>
                                w["RegistrationNo"].ToString() == r["RegistrationNo"].ToString() &&
                                w["ExamHeldID"].ToString() == examID))
                            {
                                tot["SecPos"] = ClassPosition;
                                tot["SecPos1"] = ClassPostionSort;
                            }
                        }

                }
            } //end of disting exmas
        }

        public ResultSystem()
        {
        }

        public List<Results> GetResults(int[] ids)
        {
            var results = objdb.Results.Where(w => ids.Contains(w.ID)).ToList().Select(s => new Results
            {
                _Month = s.ExamHeld.EntryDate.ToString("MMMM"),
                ExamName2 = s.ExamHeld.Exam.ExamName + " " + s.ExamHeld.EntryDate.ToString("MMMM, yyyy"),
                StudentName = s.Admission.Student.StudentName,
                FName = s.Admission.Student.FName,
                SubjectName = s.ClassSubject.Subject.SubjectName,
                _Year = s.ExamHeld.EntryDate.Year,
                AdmissionID = s.AdmissionID,
                AtLeastPercentage = s.ExamHeld.ExamsRules.FirstOrDefault().AtLeastPercentage,
                Attendance = 0,
                Status = string.Empty,
                CampusID = s.Admission.CampuseID,
                CampusName = s.Admission.Campus.CampusName,
                CheckedBy = s.CheckedBy,
                ClassID = s.ClassSubject.ClassID,
                SecPos = string.Empty,
                ClassName = s.ClassSubject.Class.ClassName,
                ClassPos = string.Empty,
                ClassSubjectID = s.ClassSubjectID,
                DOB = s.Admission.Student.DOB.ToString(),
                EntryDate = s.ExamHeld.EntryDate.ToString(),
                Exa = string.Empty,
                ExamHeldID = s.ExamHeldID,
                ExamName = s.ExamHeld.Exam.ExamName,
                GrantTotal = s.ExamHeld.ExamDates
                    .Where(w => w.ExamHeldID == s.ExamHeldID && w.ClassSection.ClassID == s.ClassSubject.ClassID)
                    .Sum(ss => ss.TotalMarks),
                ObtainedMarks = s.ObtainedMarks == 2000 ? "A" : s.ObtainedMarks.ToString(),
                Percentage = (s.ObtainedMarks == 2000 ? 0 : s.ObtainedMarks) /
                    s.ExamHeld.ExamDates
                        .Where(w => w.ExamHeldID == s.ExamHeldID && w.SubjectID == s.ClassSubject.SubjectID &&
                                    w.ClassSection.ClassID == s.ClassSubject.ClassID).FirstOrDefault().TotalMarks * 100,
                PostalAddress = s.Admission.Student.PostalAddress,
                RegistrationNo = s.Admission.Student.RegistrationNo,
                Remarks = string.Empty,
                ResultID = s.ID,
                SectionID = s.Admission.ClassSection.SectionID,
                SectionName = s.Admission.ClassSection.Section.SectionName,
                StudentID = s.Admission.StudentID,
                TotalMarks = s.ExamHeld.ExamDates
                    .Where(w => w.ExamHeldID == s.ExamHeldID && w.SubjectID == s.ClassSubject.SubjectID &&
                                w.ClassSection.ClassID == s.ClassSubject.ClassID).FirstOrDefault().TotalMarks,
                Pass = false
            }).ToList();
            var distinctRegNos = results.Select(s => s.RegistrationNo).Distinct();

            foreach (var reg in distinctRegNos)
            {
                //if (reg == 3749)
                //{
                //    bool strop = false;
                //}
                var studentResult = results.Where(w => w.RegistrationNo == reg);

                foreach (var result in studentResult)
                    try
                    {
                        result.Status = result.ObtainedMarks == "A" ? "Absent" :
                            result.Percentage >= result.AtLeastPercentage ? "Pass" : "Fail";
                        result.TotalObtained =
                            studentResult.Sum(s => s.ObtainedMarks == "A" ? 0 : double.Parse(s.ObtainedMarks));
                        result.TotalPercentage = result.TotalObtained / result.GrantTotal * 100;
                    }
                    catch
                    {
                        // SchoolSystemCommon.ErrorLogging.Error("Result Logic Data contains invalid data.");
                    }

                foreach (var result in studentResult)
                {
                    result.TotalStatus = result.TotalPercentage < result.AtLeastPercentage ? "Fail" :
                        studentResult.Where(w => w.Status == "Fail" || w.Status == "Absent").Count() > 0 ? "Fail" :
                        "Pass";

                    result.Pass = result.TotalStatus == "Pass";
                }
            }

            return results.ToList();
        }

        /// <summary>
        ///     table with all the results records
        public List<Results> GetResultsbyAdmissionId(int id)
        {
            var results = objdb.Results.Where(w => w.AdmissionID == id).ToList().Select(s => new Results
            {
                _Month = s.ExamHeld.EntryDate.ToString("MMMM"),
                ExamName2 = s.ExamHeld.Exam.ExamName + " " + s.ExamHeld.EntryDate.ToString("MMMM, yyyy"),
                StudentName = s.Admission.Student.StudentName,
                FName = s.Admission.Student.FName,
                SubjectName = s.ClassSubject.Subject.SubjectName,
                _Year = s.ExamHeld.EntryDate.Year,
                AdmissionID = s.AdmissionID,
                AtLeastPercentage = s.ExamHeld.ExamsRules.FirstOrDefault().AtLeastPercentage,
                Attendance = 0,
                Status = string
                    .Empty, // s.ObtainedMarks==2000?"Absent":(((s.ObtainedMarks==2000?0:s.ObtainedMarks)/s.ExamHeld.ExamDates.Where(w=>w.ExamHeldID==s.ExamHeldID && w.SubjectID==s.ClassSubject.SubjectID && w.ClassID==s.ClassSubject.ClassID).FirstOrDefault().TotalMarks)*100) >40?"PASS":"FAIL",
                CampusID = s.Admission.CampuseID,
                CampusName = s.Admission.Campus.CampusName,
                CheckedBy = s.CheckedBy,
                ClassID = s.ClassSubject.ClassID,
                SecPos = string.Empty,
                ClassName = s.ClassSubject.Class.ClassName,
                ClassPos = string.Empty,
                ClassSubjectID = s.ClassSubjectID,
                DOB = s.Admission.Student.DOB.ToString(),
                EntryDate = s.ExamHeld.EntryDate.ToString(),
                Exa = string.Empty,
                ExamHeldID = s.ExamHeldID,
                ExamName = s.ExamHeld.Exam.ExamName,
                GrantTotal = s.ExamHeld.ExamDates
                    .Where(w => w.ExamHeldID == s.ExamHeldID && w.ClassSection.ClassID == s.ClassSubject.ClassID)
                    .Sum(ss => ss.TotalMarks),
                ObtainedMarks = s.ObtainedMarks == 2000 ? "A" : s.ObtainedMarks.ToString(),
                Percentage = (s.ObtainedMarks == 2000 ? 0 : s.ObtainedMarks) /
                    s.ExamHeld.ExamDates
                        .Where(w => w.ExamHeldID == s.ExamHeldID && w.SubjectID == s.ClassSubject.SubjectID &&
                                    w.ClassSection.ClassID == s.ClassSubject.ClassID).FirstOrDefault().TotalMarks * 100,
                PostalAddress = s.Admission.Student.PostalAddress,
                RegistrationNo = s.Admission.Student.RegistrationNo,
                Remarks = string.Empty,
                ResultID = s.ID,
                SectionID = s.Admission.ClassSection.SectionID,
                SectionName = s.Admission.ClassSection.Section.SectionName,
                StudentID = s.Admission.StudentID,
                TotalMarks = s.ExamHeld.ExamDates
                    .Where(w => w.ExamHeldID == s.ExamHeldID && w.SubjectID == s.ClassSubject.SubjectID &&
                                w.ClassSection.ClassID == s.ClassSubject.ClassID).FirstOrDefault().TotalMarks,
                Pass = false
            }).ToList();
            var distinctRegNos = results.Select(s => s.RegistrationNo).Distinct();

            foreach (var reg in distinctRegNos)
            {
                //if (reg == 3749)
                //{
                //    bool strop = false;
                //}
                var studentResult = results.Where(w => w.RegistrationNo == reg);

                foreach (var result in studentResult)
                    try
                    {
                        result.Status = result.ObtainedMarks == "A" ? "Absent" :
                            result.Percentage >= 40 ? "Pass" : "Fail";
                        result.TotalObtained =
                            studentResult.Sum(s => s.ObtainedMarks == "A" ? 0 : double.Parse(s.ObtainedMarks));
                        result.TotalPercentage = result.TotalObtained / result.GrantTotal * 100;
                    }
                    catch
                    {
                        // SchoolSystemCommon.ErrorLogging.Error("Result Logic Data contains invalid data.");
                    }

                foreach (var result in studentResult)
                {
                    result.TotalStatus = result.TotalPercentage < 40 ? "Fail" :
                        studentResult.Where(w => w.Status == "Fail" || w.Status == "Absent").Count() > 0 ? "Fail" :
                        "Pass";

                    result.Pass = result.TotalStatus == "Pass";
                }
            }

            return results.ToList();
        }


        /// <summary>
        ///     table with all the results records
        public List<Results> GetResultsbyStudent(int studentid, int session)
        {
            var results = objdb.Results.Where(w => w.Admission.StudentID == studentid & w.Admission.Session==session).ToList().Select(s => new Results
            {
                _Month = s.ExamHeld.EntryDate.ToString("MMMM"),
                ExamName2 = s.ExamHeld.Exam.ExamName + " " + s.ExamHeld.EntryDate.ToString("MMMM, yyyy"),
                StudentName = s.Admission.Student.StudentName,
                FName = s.Admission.Student.FName,
                SubjectName = s.ClassSubject.Subject.SubjectName,
                _Year = s.ExamHeld.EntryDate.Year,
                AdmissionID = s.AdmissionID,
                AtLeastPercentage = s.ExamHeld.ExamsRules.FirstOrDefault().AtLeastPercentage,
                CoreFail = s.ExamHeld.ExamsRules.FirstOrDefault().CoreFail,
                Attendance = 0,
                Status = string
                    .Empty, // s.ObtainedMarks==2000?"Absent":(((s.ObtainedMarks==2000?0:s.ObtainedMarks)/s.ExamHeld.ExamDates.Where(w=>w.ExamHeldID==s.ExamHeldID && w.SubjectID==s.ClassSubject.SubjectID && w.ClassID==s.ClassSubject.ClassID).FirstOrDefault().TotalMarks)*100) >40?"PASS":"FAIL",
                CampusID = s.Admission.CampuseID,
                CampusName = s.Admission.Campus.CampusName,
                CheckedBy = s.CheckedBy,
                ClassID = s.ClassSubject.ClassID,
                SecPos = string.Empty,
                ClassName = s.ClassSubject.Class.ClassName,
                ClassPos = string.Empty,
                ClassSubjectID = s.ClassSubjectID,
                DOB = s.Admission.Student.DOB.ToString(),
                EntryDate = s.ExamHeld.EntryDate.ToString(),
                Exa = string.Empty,
                ExamHeldID = s.ExamHeldID,
                ExamName = s.ExamHeld.Exam.ExamName,
                GrantTotal = s.ExamHeld.ExamDates
                    .Where(w => w.ExamHeldID == s.ExamHeldID && w.ClassSection.ClassID == s.ClassSubject.ClassID)
                    .Sum(ss => ss.TotalMarks),
                ObtainedMarks = s.ObtainedMarks == 2000 ? "A" : s.ObtainedMarks.ToString(),
                Percentage = (s.ObtainedMarks == 2000 ? 0 : s.ObtainedMarks) /
                    s.ExamHeld.ExamDates
                        .Where(w => w.ExamHeldID == s.ExamHeldID && w.SubjectID == s.ClassSubject.SubjectID &&
                                    w.ClassSection.ClassID == s.ClassSubject.ClassID).FirstOrDefault().TotalMarks * 100,
                PostalAddress = s.Admission.Student.PostalAddress,
                RegistrationNo = s.Admission.Student.RegistrationNo,
                Remarks = string.Empty,
                ResultID = s.ID,
                SectionID = s.Admission.ClassSection.SectionID,
                SectionName = s.Admission.ClassSection.Section.SectionName,
                StudentID = s.Admission.StudentID,
                TotalMarks = s.ExamHeld.ExamDates
                    .Where(w => w.ExamHeldID == s.ExamHeldID && w.SubjectID == s.ClassSubject.SubjectID &&
                                w.ClassSection.ClassID == s.ClassSubject.ClassID).FirstOrDefault().TotalMarks,
                Pass = false
            }).ToList();
            var distinctRegNos = results.Select(s => s.RegistrationNo).Distinct();
            var distinctExm = results.Select(s => s.ExamHeldID).Distinct();
            foreach(var exam in distinctExm)
            foreach (var reg in distinctRegNos)
            {
                //if (reg == 3749)
                //{
                //    bool strop = false;
                //}
                var studentResult = results.Where(w => w.RegistrationNo == reg && w.ExamHeldID==exam);

                foreach (var result in studentResult)
                    try
                    {
                        result.Status = result.ObtainedMarks == "A" ? "Absent" :
                            result.Percentage >= 40 ? "Pass" : "Fail";
                        result.TotalObtained =
                            studentResult.Sum(s => s.ObtainedMarks == "A" ? 0 : double.Parse(s.ObtainedMarks));
                        result.TotalPercentage = result.TotalObtained / result.GrantTotal * 100;
                    }
                    catch
                    {
                        // SchoolSystemCommon.ErrorLogging.Error("Result Logic Data contains invalid data.");
                    }

                foreach (var result in studentResult)
                {
                    result.TotalStatus = result.TotalPercentage < result.AtLeastPercentage ? "Fail" :
                        studentResult.Where(w => w.Status == "Fail" || w.Status == "Absent").Count() >= result.CoreFail ? "Fail" :
                        "Pass";

                    result.Pass = result.TotalStatus == "Pass";
                }
            }

            return results.ToList();
        }

        /// </summary>
        /// <returns>DataTable</returns>
        public DataTable GetData()
        {
            return tab;
        }

        public List<Results> GetData(bool list)
        {
            var results = new List<Results>();
            foreach (DataRow dr in tab.Rows)
            {
                var result = new Results
                {
                    _Month = dr["_Month"].ToString(),
                    //_Year = int.Parse(String.IsNullOrEmpty(dr["_Year"].ToString()) ? "0" : dr["_Year"].ToString()),
                    //AdmissionID = int.Parse(String.IsNullOrEmpty(dr["AdmissionID"].ToString()) ? "0" : dr["AdmissionID"].ToString()),
                    //AtLeastPercentage = double.Parse(String.IsNullOrEmpty(dr["AtLeastPercentage"].ToString()) ? "0" : dr["AtLeastPercentage"].ToString()),
                    //Attendance = double.Parse(String.IsNullOrEmpty(dr["Attendance"].ToString()) ? "0" : dr["Attendance"].ToString()),
                    CampusID = int.Parse(string.IsNullOrEmpty(dr["CampusID"].ToString())
                        ? "0"
                        : dr["CampusID"].ToString()),
                    ClassID =
                        int.Parse(string.IsNullOrEmpty(dr["ClassID"].ToString()) ? "0" : dr["ClassID"].ToString()),
                    ClassName = dr["ClassName"].ToString(),
                    //ClassSubjectID = int.Parse(String.IsNullOrEmpty(dr["ClassSubjectID"].ToString()) ? "0" : dr["ClassSubjectID"].ToString()),
                    DOB = dr["DOB"].ToString(),
                    EntryDate = dr["EntryDate"].ToString(),
                    Exa = dr["Exa"].ToString(),
                    ExamName = dr["ExamName"].ToString(),
                    ExamName2 = dr["ExamName2"].ToString(),
                    FName = dr["FName"].ToString(),
                    //ObtainedMarks = double.Parse(String.IsNullOrEmpty(dr["ObtainedMarks"].ToString()) ? "0" : dr["ObtainedMarks"].ToString()),
                    PostalAddress = dr["PostalAddress"].ToString(),
                    RegistrationNo = int.Parse(string.IsNullOrEmpty(dr["RegistrationNo"].ToString())
                        ? "0"
                        : dr["RegistrationNo"].ToString()),
                    StudentID = int.Parse(string.IsNullOrEmpty(dr["StudentID"].ToString())
                        ? "0"
                        : dr["StudentID"].ToString()),
                    Remarks = dr["Remarks"].ToString(),
                    //ResultID = int.Parse(String.IsNullOrEmpty(dr["ResultID"].ToString()) ? "0" : dr["ResultID"].ToString()),
                    SectionID = int.Parse(string.IsNullOrEmpty(dr["SectionID"].ToString())
                        ? "0"
                        : dr["SectionID"].ToString()),
                    Status = dr["Status"].ToString(),
                    SectionName = dr["SectionName"].ToString(),
                    StudentName = dr["StudentName"].ToString(),
                    ExamHeldID = int.Parse(string.IsNullOrEmpty(dr["ExamHeldID"].ToString())
                        ? "0"
                        : dr["ExamHeldID"].ToString()),
                    //Percentage = double.Parse(String.IsNullOrEmpty(dr["Percentage"].ToString()) ? "0" : dr["Percentage"].ToString()),
                    SubjectName = dr["SubjectName"].ToString(),
                    //TotalMarks = double.Parse(String.IsNullOrEmpty(dr["TotalMarks"].ToString()) ? "0" : dr["TotalMarks"].ToString()),

                    ClassPos = dr["ClassPos"].ToString(),
                    GrantTotal = double.Parse(string.IsNullOrEmpty(dr["GrantTotal"].ToString())
                        ? "0"
                        : dr["GrantTotal"].ToString()),
                    SecPos = dr["SecPos"].ToString(),
                    TotalObtained = double.Parse(string.IsNullOrEmpty(dr["TotalObtained"].ToString())
                        ? "0"
                        : dr["TotalObtained"].ToString()),
                    TotalPercentage = double.Parse(string.IsNullOrEmpty(dr["TotalPercentage"].ToString())
                        ? "0"
                        : dr["TotalPercentage"].ToString()),
                    TotalStatus = dr["TotalStatus"].ToString(),
                    CampusName = dr["CampusName"].ToString()
                };
                results.Add(result);
            }

            return results;
        }
    }

    public class Results
    {
        public bool Pass { get; set; }
        public int ResultID { get; set; }
        public int ClassSubjectID { get; set; }

        public string PostalAddress { get; set; }
        public string DOB { get; set; }
        public string StudentName { get; set; }
        public int CampusID { get; set; }
        public string CampusName { get; set; }
        public int AdmissionID { get; set; }
        public int RegistrationNo { get; set; }
        public string SubjectName { get; set; }
        public double TotalMarks { get; set; }
        public string ObtainedMarks { get; set; }

        public string Status { get; set; }
        public double Percentage { get; set; }
        public string ClassName { get; set; }
        public string CheckedBy { get; set; }
        public string FName { get; set; }
        public int SectionID { get; set; }
        public string SectionName { get; set; }
        public string ExamName { get; set; }
        public string ExamName2 { get; set; }
        public string Exa { get; set; }
        public int _Year { get; set; }
        public string EntryDate { get; set; }
        public string _Month { get; set; }
        public double AtLeastPercentage { get; set; }
        public int CoreFail { get; set; }
        public int ExamHeldID { get; set; }

        //public bool IsCoreSubject { get; set; }
        public int ClassID { get; set; }
        public int SocBeh { get; set; }
        public int Discpline { get; set; }
        public int Manners { get; set; }
        public int OralExp { get; set; }
        public int CoTeamWork { get; set; }
        public int Confid { get; set; }
        public int Punctuality { get; set; }
        public int PhyFit { get; set; }
        public int Neatness { get; set; }
        public double Attendance { get; set; }
        public string Remarks { get; set; }
        public int StudentID { get; set; }

        public double TotalObtained { get; set; }
        public double GrantTotal { get; set; }
        public double TotalPercentage { get; set; }
        public string TotalStatus { get; set; }
        public string ClassPos { get; set; }
        public string SecPos { get; set; }
    }

    public class Results1
    {
        public double TotalObtained { get; set; }
        public double GrantTotal { get; set; }
        public double TotalPercentage { get; set; }
        public string TotalStatus { get; set; }
        public string ClassPos { get; set; }
        public string SecPos { get; set; }
    }

    public class ChartData
    {
        public int id { get; set; }
        public string Display { get; set; }
        public decimal Value { get; set; }
        public string GroupBy { get; set; }
    }

    public class ChartDatat
    {
        public int id { get; set; }
        public string Displayt { get; set; }
        public decimal Valuett { get; set; }
        public string GroupByt { get; set; }
    }
}