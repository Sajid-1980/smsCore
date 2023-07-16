using Microsoft.AspNetCore.Http;
using Models;
using SchoolManagementSystem.Helpers;
using smsCore.Data.Helpers;
using System.Data;
using System.Xml;

namespace SchoolManagementSystem
{
    public class ReportViewModel
    {
        public enum ReportFormat { PDF = 1, Word = 2, Excel = 3 }
        SchoolEntities db ;
        IHttpContextAccessor _context;
        IHostEnvironment _env;
        private readonly IWebHostEnvironment _hostingEnvironment;



        public ReportViewModel(SchoolEntities _db, IHttpContextAccessor context , IWebHostEnvironment web)
        {
            db = _db;
            _context = context;
            //initation for the data set holder
            ReportDataSets = new List<ReportDataSet>();
            _hostingEnvironment = web;
        }
        
        //Name of the report
        public string Name { get; set; }
        //Language of the report
        public string ReportLanguage { get; set; }

        //Reference to the RDLC file that contain the report definition
        public string FileName { get; set; }

        //The main title for the reprt
        public string ReportTitle { get; set; }
        //The right and left titles and sub title for the report
        //public string RightMainTitle { get; set; }
        //public string RightSubTitle { get; set; }
        //public string LeftMainTitle { get; set; }
        //public string LeftSubTitle { get; set; }
        //the url for the logo, 
        //public string ReportLogo { get; set; }
        //date for printing the report
        //public DateTime ReportDate { get; set; }
        //the user name that is printing the report
        //public string UserNamPrinting { get; set; }
        //dataset holder
        public List<ReportDataSet> ReportDataSets { get; set; }
        //report format needed
        public ReportFormat Format { get; set; }
        public bool ViewAsAttachment { get; set; }
        //an helper class to store the data for each report data set
        public class ReportDataSet
        {
            public string DatasetName { get; set; }
            public object Data { get; set; }
        }
        public string ReporExportFileName
        {
            get
            {
                return string.Format("attachment; filename={0}.{1}", this.ReportTitle, ReporExportExtention);
            }
        }
        public string ReporExportExtention
        {
            get
            {
                switch (this.Format)
                {
                    case ReportViewModel.ReportFormat.Word: return ".doc";
                    case ReportViewModel.ReportFormat.Excel: return ".xls";
                    default:
                        return ".pdf";
                }
            }
        }
        public string LastmimeType
        {
            get
            {
                return mimeType;
            }
        }
        private string mimeType;
        

        public string ResultGazzeteTemplate(string[] subjNames,bool dob, bool classpos)
        {
            int i = 1;
            string colValues = "<TablixCells xmlns=\"http://schemas.microsoft.com/sqlserver/reporting/2016/01/reportdefinition\" xmlns:rd=\"http://schemas.microsoft.com/SQLServer/reporting/reportdesigner\">";
            string colHeaders = "<TablixCells xmlns=\"http://schemas.microsoft.com/sqlserver/reporting/2016/01/reportdefinition\" xmlns:rd=\"http://schemas.microsoft.com/SQLServer/reporting/reportdesigner\">";
            string colSizes = "<TablixColumns xmlns=\"http://schemas.microsoft.com/sqlserver/reporting/2016/01/reportdefinition\" xmlns:rd=\"http://schemas.microsoft.com/SQLServer/reporting/reportdesigner\">";
            string members = "<TablixMembers xmlns=\"http://schemas.microsoft.com/sqlserver/reporting/2016/01/reportdefinition\" xmlns:rd=\"http://schemas.microsoft.com/SQLServer/reporting/reportdesigner\">";
            foreach (var s in subjNames)
            {
                colSizes+=@"<TablixColumn>
                                    <Width>0.5in</Width>
                                  </TablixColumn>";
                members+=@"<TablixMember />";
                colHeaders +=$@" <TablixCell>
                                        <CellContents>
                                          <Textbox Name='Textbox23{i}'>
                                            <CanGrow>true</CanGrow>
                                            <KeepTogether>true</KeepTogether>
                                            <Paragraphs>
                                              <Paragraph>
                                                <TextRuns>
                                                  <TextRun>
                                                    <Value>{s}</Value>
                                                    <Style>
                                                      <FontSize>9pt</FontSize>
                                                      <FontWeight>Bold</FontWeight>
                                                    </Style>
                                                  </TextRun>
                                                </TextRuns>
                                                <Style>
                                                  <TextAlign>Center</TextAlign>
                                                </Style>
                                              </Paragraph>
                                            </Paragraphs>
                                            <rd:DefaultName>Textbox23{i}</rd:DefaultName>
                                            <Style>
                                              <Border>
                                                <Style>Solid</Style>
                                              </Border>
                                              <VerticalAlign>Middle</VerticalAlign>
                                              <PaddingLeft>2pt</PaddingLeft>
                                              <PaddingRight>2pt</PaddingRight>
                                              <PaddingTop>2pt</PaddingTop>
                                              <PaddingBottom>2pt</PaddingBottom>
                                            </Style>
                                          </Textbox>
                                        </CellContents>
                                      </TablixCell>
                                     ";
                colValues +=
                $@" <TablixCell>
                                        <CellContents>
                                          <Textbox Name='Textbox24{i}'>
                                            <CanGrow>true</CanGrow>
                                            <KeepTogether>true</KeepTogether>
                                            <Paragraphs>
                                              <Paragraph>
                                                <TextRuns>
                                                  <TextRun>
                                                    <Value>=Fields!s{i}.Value</Value>
                                                    <Style>
                                                      <FontSize>9pt</FontSize>
                                                    </Style>
                                                  </TextRun>
                                                </TextRuns>
                                                <Style>
                                                  <TextAlign>Center</TextAlign>
                                                </Style>
                                              </Paragraph>
                                            </Paragraphs>
                                            <rd:DefaultName>Textbox24{i}</rd:DefaultName>
                                            <Style>
                                              <Border>
                                                <Style>Solid</Style>
                                              </Border>
                                              <VerticalAlign>Middle</VerticalAlign>
                                              <PaddingLeft>2pt</PaddingLeft>
                                              <PaddingRight>2pt</PaddingRight>
                                              <PaddingTop>2pt</PaddingTop>
                                              <PaddingBottom>2pt</PaddingBottom>
                                            </Style>
                                          </Textbox>
                                        </CellContents>
                                      </TablixCell>
                                     "
                                         ;
                i++;
            }

            colValues += "</TablixCells>";
            colHeaders += "</TablixCells>";
            colSizes += "</TablixColumns>";
            members += "</TablixMembers>";

            //string reportContents = System.IO.File.ReadAllText(_context.HttpContext.MapPath("~/Content/Reports/ResultGazzete.rdlc"));

            string reportPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Content/Reports/ResultGazzete.rdlc");
            string reportContents = System.IO.File.ReadAllText(reportPath);

            XmlDocument dc = new XmlDocument();
            dc.LoadXml(reportContents);
            XmlNamespaceManager xmlnsManager = new XmlNamespaceManager(dc.NameTable);
            xmlnsManager.AddNamespace("ns", "http://schemas.microsoft.com/sqlserver/reporting/2016/01/reportdefinition");
            xmlnsManager.AddNamespace("rd", "http://schemas.microsoft.com/SQLServer/reporting/reportdesigner");

            var xmlColSizes = dc.SelectNodes("//ns:Tablix[@Name='Tablix4']/ns:TablixBody/ns:TablixColumns", xmlnsManager);
            var xmlColHeaders = dc.SelectNodes("//ns:Tablix[@Name='Tablix4']/ns:TablixBody/ns:TablixRows/ns:TablixRow[1]/ns:TablixCells", xmlnsManager);
            var xmlColValues = dc.SelectNodes("//ns:Tablix[@Name='Tablix4']/ns:TablixBody/ns:TablixRows/ns:TablixRow[2]/ns:TablixCells", xmlnsManager);
            var xmlColMembers = dc.SelectNodes("//ns:Tablix[@Name='Tablix4']/ns:TablixColumnHierarchy/ns:TablixMembers", xmlnsManager);
            int index = 4;
            
            int classPosIndex = 7;

            if (!dob)
            {
               xmlColSizes[0].RemoveChild(xmlColSizes[0].ChildNodes[4]);
                xmlColHeaders[0].RemoveChild(xmlColHeaders[0].ChildNodes[4]);
                xmlColValues[0].RemoveChild(xmlColValues[0].ChildNodes[4]);
                xmlColMembers[0].RemoveChild(xmlColMembers[0].ChildNodes[4]);
                index=index-1;
                classPosIndex=classPosIndex-1;
            }
            if (!classpos)
            {
                xmlColSizes[0].RemoveChild(xmlColSizes[0].ChildNodes[classPosIndex]);
                xmlColHeaders[0].RemoveChild(xmlColHeaders[0].ChildNodes[classPosIndex]);
                xmlColValues[0].RemoveChild(xmlColValues[0].ChildNodes[classPosIndex]);
                xmlColMembers[0].RemoveChild(xmlColMembers[0].ChildNodes[classPosIndex]);

                //index=index-1;
            }
            XmlDocument doccolSizes = new XmlDocument();
            doccolSizes.LoadXml(colSizes); 
             var refchild = xmlColSizes[0].ChildNodes[index];
            foreach(XmlNode c in doccolSizes.ChildNodes[0].ChildNodes)
            {
                XmlNode importNode = dc.ImportNode(c, true);
                xmlColSizes[0].InsertAfter(importNode,refchild);
            }

            XmlDocument doccolHeaders = new XmlDocument();
            
            doccolHeaders.LoadXml(colHeaders);
            refchild = xmlColHeaders[0].ChildNodes[index];
            foreach (XmlNode c in doccolHeaders.ChildNodes[0].ChildNodes)
            {
                XmlNode importNode = dc.ImportNode(c, true);
                xmlColHeaders[0].InsertAfter(importNode, refchild);
            }

            XmlDocument doccolValues = new XmlDocument();
            doccolValues.LoadXml(colValues);
            refchild = xmlColValues[0].ChildNodes[index];
            foreach (XmlNode c in doccolValues.ChildNodes[0].ChildNodes)
            {
                XmlNode importNode = dc.ImportNode(c, true);
                xmlColValues[0].InsertAfter(importNode, refchild);
            }

            XmlDocument docmembers = new XmlDocument();
            docmembers.LoadXml(members);
            refchild = xmlColMembers[0].ChildNodes[index];
            foreach (XmlNode c in docmembers.ChildNodes[0].ChildNodes)
            {
                XmlNode importNode = dc.ImportNode(c, true);
                xmlColMembers[0].InsertAfter(importNode, refchild);
            }

            var _xmlColSizes = dc.SelectNodes("//ns:Tablix[@Name='Tablix4']/ns:TablixBody/ns:TablixColumns", xmlnsManager);
            int p = 0;

            float toltalwidth = 11.4f;
            float covered = 0;
            foreach(XmlNode n in _xmlColSizes[0].ChildNodes)
            {
                var width = n.ChildNodes[0].InnerText.Replace("in", "");
                float.TryParse(width, out float b);
                covered+=b;
            }

            if (covered<toltalwidth)
            {
                float rem = toltalwidth-covered;

                foreach (XmlNode n in _xmlColSizes[0].ChildNodes)
                {
                    var width = n.ChildNodes[0].InnerText.Replace("in", "");
                    if(float.TryParse(width, out float b))
                    {
                        var percent = b/covered*100;
                        var nval = rem*percent/100;
                        var val = b+nval;
                        n.ChildNodes[0].InnerText = val+"in";
                    }
                }
            }

            reportContents=dc.OuterXml; 
            return reportContents;
        }


        public  (DataTable,string,Dictionary<string,object>) GetResultGazzeteforDatatable(int Classes = -1, int Sections = -1, int campus = -1,
            int exams = -1,string summary = "",bool showposition=true,bool showdob=false)
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
                                      string.Join(",", AdmIds.Select(s => s.ToString().Trim())) + " ))",ShowPosition: showposition);
            var tab = rs.GetData();
            if (tab != null)
            {
                var examName = tab.Rows[0]["ExamName2"].ToString();
                string clsName = tab.Rows[0]["ClassName"].ToString();
                string secName = tab.Rows[0]["SectionName"].ToString();

                int.TryParse(tab.Rows[0]["ExamHeldID"].ToString(), out int examHeldId);

                 var subjectsArray = tab.AsEnumerable()
                    .Select(s =>new {name= s["SubjectName"].ToString().Trim(), id= int.TryParse(s["ClassSubjectID"].ToString().Trim(), out var r) ? r : 0 }).DistinctBy(b=>b.name)
                    .ToArray();

                var subjectids = subjectsArray.Select(s => s.id).ToArray();
                
                string reportTemplate = ResultGazzeteTemplate(subjectsArray.Select(s => s.name.Remove(1).ToUpper()+ (s.name.Length>6?s.name.Substring(1, 6).ToLower(): s.name.Substring(1, s.name.Length-1).ToLower())).ToArray(),showdob,showposition);


                List<string> cols1 =new List<string> { "RegistrationNo", "StudentName",  "FName","DOB",
                            "TotalObtained",  "GrantTotal", "TotalPercentage",
                            "TotalStatus" };
                if (showposition)
                {
                    cols1.Add("ClassPos");
                    cols1.Add("SecPos");
                }

                var pt = new Pivot(tab);
                var dt = new DataTable();
                List<string> cols = new List<string>() {
                "RegistrationNo", "StudentName",  "FName","DOB",
                            "TotalObtained", "ClassPos", "SecPos", "GrantTotal", "TotalPercentage",
                            "TotalStatus"
                };
                if(!showdob)
                {
                    cols.Remove("DOB");
                }
                if (!showposition)
                {
                    cols.Remove("ClassPos");
                    cols.Remove("SecPos");
                }
                dt = pt.PivotData("ObtainedMarks", AggregateFunction.First,
                    cols.ToArray(), new[] { "ClassSubjectID" });

                dt.Columns["RegistrationNo"].ColumnName = "AdmissionID";
                dt.Columns["GrantTotal"].ColumnName = "TotalMarks";
                dt.Columns["TotalObtained"].ColumnName = "ObtainedMarks";
                dt.Columns["TotalPercentage"].ColumnName = "TotalPercentage";
                if (dt.Columns.Contains("Grade"))
                    dt.Columns["Grade"].ColumnName = "Grade";

                dt.Columns["TotalStatus"].ColumnName = "TotalStatus";

                var par = new Dictionary<string, object>();
                par.Add("examName", examName);
                par.Add("className", clsName);
                par.Add("sectionName", secName);
                double totalap = dt.AsEnumerable().Where(w => w["TotalStatus"].ToString().ToLower() != "absent").Count();
                double totalab = dt.AsEnumerable().Where(w => w["TotalStatus"].ToString().ToLower() == "absent").Count();

                double pass = dt.AsEnumerable().Where(w => w["TotalStatus"].ToString().ToLower() == "pass").Count();
                var ptt = pass / totalap * 100;
                var percentage = decimal.Round(decimal.Parse(ptt.ToString()), 2).ToString();
                var fail = dt.AsEnumerable().Where(w => w["TotalStatus"].ToString().ToLower() == "fail").Count().ToString();
                par.Add("TotalStudents", dt.Rows.Count);
                par.Add("Appeared", totalap);
                par.Add("Absent", totalab);

                //par.Add("status",
                //    "Total Enrollment: " + dt.Rows.Count + "  Total Appeared: " + totalap + "\n PASS: " + pass +
                //    "     FAIL: " + fail + "     Absent: " + totalab);
                //par.Add("passpercentage", "Pass percentage: " + percentage);

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
                    //rsum.C1 = dt.AsEnumerable().Where(r => r["ClassPos"].ToString().Split('/')[0] == "1").Count();
                    //rsum.C2 = dt.AsEnumerable().Where(r => r["ClassPos"].ToString().Split('/')[0] == "2").Count();
                    //rsum.C3 = dt.AsEnumerable().Where(r => r["ClassPos"].ToString().Split('/')[0] == "3").Count();
                    //rsum.S1 = dt.AsEnumerable().Where(r => r["SecPos"].ToString().Split('/')[0] == "1").Count();
                    //rsum.S2 = dt.AsEnumerable().Where(r => r["SecPos"].ToString().Split('/')[0] == "2").Count();
                    //rsum.S3 = dt.AsEnumerable().Where(r => r["SecPos"].ToString().Split('/')[0] == "3").Count();
                    try
                    {
                        db.SaveChanges();
                    }
                    catch
                    {
                    }
                }

                #endregion

                dt.Columns.Add("Grade");
                dt.Columns.Add("posArrange",typeof(int));
                int i = 0;
                foreach (var pso in dt.AsEnumerable().OrderByDescending(r => double.TryParse(r["TotalPercentage"].ToString(), out double Perc) ? Perc : 50000))
                {
                    pso["posArrange"]=i;
                    i++;
                }
                var examRule = db.ExamsRules.FirstOrDefault(w => w.ExamHeldID==examHeldId);
                if (examRule==null)
                {
                    examRule=new ExamsRule();
                }
                foreach (DataRow r in dt.Rows)
                {
                    if (showposition)
                    {
                        var result = 0;
                        var pos = r["SecPos"].ToString().Split('/')[0];
                        if (int.TryParse(pos, out result))
                        {
                            r["posArrange"] = result==0 ? 500000 : result;
                            r["ClassPos"]=result.ToPosition();
                        }
                        else r["posArrange"] = 5000000;
                    }
                    else
                    {

                    }
                    double.TryParse(r["TotalPercentage"].ToString(), out double Perc);
                    var grade = "";
                    if (Perc >= examRule.APlus)
                        grade = "A+";
                    else if (Perc < examRule.APlus && Perc >= examRule.A)
                        grade = "A";
                    else if (Perc < examRule.A && Perc >= examRule.BPlus)
                        grade = "B+";
                    else if (Perc < examRule.BPlus && Perc >= examRule.B)
                        grade = "B";
                    else if (Perc < examRule.B && Perc >= examRule.C)
                        grade = "C";
                    else if (Perc < examRule.C && Perc >= examRule.D)
                        grade="D";
                    else if (Perc < examRule.D && Perc >= examRule.E)
                        grade="E";
                    else grade = "F";
                    if (r["TotalStatus"].ToString().ToLower() == "fail")
                        r["Grade"] = "F";
                    else r["Grade"] = grade;
                }

                int t = 1;
                foreach(var v in subjectids)
                {
                    if (dt.Columns.Contains(v.ToString()))
                    {
                        dt.Columns[v.ToString()].ColumnName="s"+t.ToString();
                        t++;
                    }
                }
                
                return (dt,reportTemplate,par);
            }

            return (new DataTable(),string.Empty,new Dictionary<string, object>());
        }


    }
}