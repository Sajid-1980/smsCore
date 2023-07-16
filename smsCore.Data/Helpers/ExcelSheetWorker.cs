using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Models;

using OfficeOpenXml;
using OfficeOpenXml.Style;
using Syncfusion.XlsIO;

namespace smsCore.Data.Helpers
{
    public class ExcelSheetWorker
    {
        private SchoolEntities objdb;
        private CurrentUser _user;
        public ExcelSheetWorker(SchoolEntities _db, CurrentUser user)
        {
            objdb = _db;
            _user = user;
        }

        public class ResultUpload
        {
            public int RegNo { get; set; }
            public string Subject { get; set; }
            public string Marks { get; set; }
            public string Status { get; set; }
        }
        public class UploadResult<T,F>
        {
            public F Faild { get; set; }
            public bool Status { get; set; }
            public string? Error { get; set; }
            public int SuccessCount { get; set; }
            public T Response { get; set; }
        }

        public async Task<UploadResult<List<Result>, List<ResultUpload>>> GetResults(Stream file, int classid, int campusid, int examheldid)
        {
            var response = new UploadResult<List<Result>, List<ResultUpload>>();
            using (var excelEngine = new ExcelEngine())
            {
                var application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Excel2013;

                var workbook = application.Workbooks.Open(file);

                var worksheet = workbook.Worksheets[0];
                var totalrows2 = worksheet.Rows.Count();
                var totalcols2 = worksheet.Columns.Count();
                var cols = worksheet.ExportDataTable(1, 1, totalrows2, totalcols2,ExcelExportDataTableOptions.ColumnNames);
                var header = worksheet.Rows[0];
                var subjectList = objdb.ClassSubjects.Where(w => w.ClassID==classid && w.CampusID==campusid).Select(s => new { s.Subject.SubjectName, s.SubjectID, s.ID }).ToList();
                
                List<ResultUpload> students = new List<ResultUpload>();
                foreach (DataRow r in cols.Rows)
                {
                    if (int.TryParse(r["RegNo"].ToString().Trim(), out int regno))
                    {

                        foreach (DataColumn col in cols.Columns)
                        {
                            if (col.ColumnName=="RegNo") continue;
                            //if (!subjectList.Select(s => s.SubjectName.Trim().ToLower()).Contains(col.ColumnName.Trim().ToLower())) continue;
                            var tr = new ResultUpload { RegNo = regno };
                            tr.Marks=r[col].ToString();
                            tr.Subject = col.ColumnName;
                            students.Add(tr);
                        }
                    }
                }
                //var students = worksheet.ExportData<ResultUpload>(1, 1, totalrows2, totalcols2);


                var results = new List<Result>();
                var notAdded = new List<ResultUpload>();

                var subjects = students.Select(s => s.Subject).Distinct();
                var notExist = subjects.Where(s => !subjectList.Where(sub => sub.SubjectName.Trim().ToLower()==s.Trim().ToLower()).Any());
                if (notExist.Any())
                {
                    string error = "Subject ("+string.Join(", ", notExist)+") does not exist in this class";
                    response.Error=error;
                    response.Status=false;
                    return response;
                }

                var existingRsults = objdb.Results.Where(w => w.ExamHeldID==examheldid && w.ClassSubject.ClassID==classid).ToList();
                foreach (ResultUpload student in students)
                {
                    try
                    {
                        var classSubjectId = subjectList.FirstOrDefault(f => f.SubjectName.Trim().ToLower()==student.Subject.ToLower().Trim()).ID;
                        var admissionId = objdb.Admissions.Where(w => !w.IsExpell && w.Student.RegistrationNo==student.RegNo && w.CampuseID==campusid && w.ClassSection.ClassID==classid).Select(s => s.ID).FirstOrDefault();
                        if (admissionId==0)
                        {
                            student.Status="Student record not found";
                            notAdded.Add(student);
                            continue;
                        }
                        var exist = existingRsults.Where(w => w.ClassSubjectID==classSubjectId && w.AdmissionID==admissionId).FirstOrDefault();
                        if (exist==null)
                        {
                            exist = new Result
                            {
                                AdmissionID=admissionId,
                                CheckedBy=string.Empty,
                                ClassSubjectID=classSubjectId,
                                ExamHeldID=examheldid, 
                                UserID= _user.UserID
                            };
                            results.Add(exist);
                        }
                        if (double.TryParse(student.Marks, out var marks))
                        {
                            exist.ObtainedMarks=marks;
                        }
                        else
                        {
                            if (student.Marks.ToLower().Trim()=="a")
                            {
                                exist.ObtainedMarks=marks;
                            }
                            else
                            {
                                student.Status="Student record not found";
                                notAdded.Add(student);
                                results.Remove(exist);
                                continue;
                            }
                        }
                    }
                    catch
                    {
                        notAdded.Add(student);
                    }
                }

                try
                {
                    if (notAdded.Count==0)
                    {
                        objdb.Results.AddRange(results);
                        await objdb.SaveChangesAsync();
                        response.Status=true;
                    }
                }
                catch (Exception ex)
                {
                    response.Status = false;
                    response.Error = ex.InnerException==null ? ex.Message : ex.InnerException.Message;
                }

                //Export worksheet data into CLR Objects
                response.Faild=notAdded;
                response.Response=results;
                return response; // worksheet.ExportData<Student>(1, 1, totalrows2, totalcols2);
            }
        }

        public List<Student> GetStudents(Stream file, int Session, int Campus, int classid, int sectionid,
            bool classSelected, bool sectionSelected)
        {
            using (var excelEngine = new ExcelEngine())
            {
                var application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Excel2013;

                var workbook = application.Workbooks.Open(file);

                var worksheet = workbook.Worksheets[0];
                var totalrows2 = worksheet.Rows.Count();
                var totalcols2 = worksheet.Columns.Count();
                var cols = worksheet.ExportDataTable(1, 1, totalrows2, totalcols2,
                    ExcelExportDataTableOptions.ColumnNames);
                var students = worksheet.ExportData<Student>(1, 1, totalrows2, totalcols2);

                var mobileId = 0;
                try
                {
                    mobileId = objdb.StudentMobiles.Select(s => s.ID).Max();
                }
                catch
                {
                    mobileId = 0;
                }

                mobileId += 1;

                var admId = 1;
                try
                {
                    admId = objdb.Admissions.Select(s => s.ID).Max();
                    admId++;
                }
                catch
                {
                    admId = 1;
                }

                var stdId = 1;
                try
                {
                    stdId = objdb.Students.Select(s => s.ID).Max();
                    stdId++;
                }
                catch
                {
                    stdId = 1;
                }

                var notAdded = new List<DataRow>();

                foreach (DataRow student in cols.Rows)
                    if (int.TryParse(student["RegistrationNo"].ToString(), out var RegNo))
                    {
                        var std = students.Where(w => w.RegistrationNo == RegNo).FirstOrDefault();

                        var regNoExist = objdb.Students.AsNoTracking().Where(w => w.RegistrationNo == RegNo).Any();

                        if (string.IsNullOrEmpty(std.StudentName) || regNoExist)
                        {
                            notAdded.Add(student);
                            students.Remove(std);
                        }

                        var section = student["Section"].ToString();
                        var className = student["Class"].ToString();
                        var classSectionId = 0;
                        if (!classSelected)
                            classid = objdb.Classes.Where(w => w.ClassName == className).Select(s => s.ID)
                                .FirstOrDefault();
                        if (!sectionSelected)
                            sectionid = objdb.Sections.Where(w => w.SectionName == section).Select(s => s.ID)
                                .FirstOrDefault();
                        classSectionId = objdb.ClassSections
                            .Where(w => w.ClassID == classid && w.SectionID == sectionid).Select(s => s.ID)
                            .FirstOrDefault();

                        std.ID = stdId;
                        stdId++;
                        var ad = new Admission();
                        ad.Session = Session;
                        ad.IsExpell = false;
                        ad.ClassSectionID = classSectionId;
                        ad.CampuseID = Campus;
                        ad.ID = admId;
                        admId++;
                        std.Admissions.Add(ad);

                        var moibles = new List<StudentMobile>();

                        var mobile = student["mobiles"].ToString();
                        if (string.IsNullOrEmpty(mobile))
                            continue;

                        if (mobile.Contains(","))
                        {
                            var nos = mobile.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                            var i = 0;
                            foreach (var mob in nos)
                                if (!string.IsNullOrEmpty(mob))
                                {
                                    std.StudentMobiles.Add(new StudentMobile
                                    {
                                        ID = mobileId, IsDefault = i == 0, MobileHolder = string.Empty, MobileNo = mob,
                                        Relation = string.Empty
                                    });
                                    mobileId++;
                                    i++;
                                }
                        }
                        else
                        {
                            std.StudentMobiles.Add(new StudentMobile
                            {
                                ID = mobileId, IsDefault = true, MobileHolder = string.Empty, MobileNo = mobile,
                                Relation = string.Empty
                            });
                            mobileId++;
                        }
                    }

                //Export worksheet data into CLR Objects
                return students; // worksheet.ExportData<Student>(1, 1, totalrows2, totalcols2);
            }
        }

        public List<tbl_Employee> GetEmployees(Stream file)
        {
            List<tbl_Employee> employees = null;
            using (var excelEngine = new ExcelEngine())
            {
                
                var application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Excel2013;

                var workbook = application.Workbooks.Open(file);

                var worksheet = workbook.Worksheets[0];

                //var exvelPckg2 = new OfficeOpenXml.ExcelPackage(file, true).Workbook.Worksheets.FirstOrDefault();
                //int totalrows2 = exvelPckg2.Dimension.End.Row;
                //int totalcols2 = exvelPckg2.Dimension.End.Column;

                var salaryPackage = objdb.SalaryPackages.FirstOrDefault();
                if (salaryPackage == null)
                {
                    salaryPackage = new tbl_SalaryPackage
                    {
                         isActive = true,
                        narration = string.Empty, salaryPackageName = "Default", UserId= _user.UserID, EntryDate=DateTime.Now
                    };
                    objdb.SalaryPackages.Add(salaryPackage);
                    objdb.SaveChanges();
                }

                var totalrows2 = worksheet.Rows.Count();
                var totalcols2 = worksheet.Columns.Count();
                var table = worksheet.ExportDataTable(1, 1, totalrows2, totalcols2,
                    ExcelExportDataTableOptions.ColumnNames);
                employees = worksheet.ExportData<tbl_Employee>(1, 1, totalrows2, totalcols2);

                foreach (DataRow d in table.Rows)
                {
                    var code = d["employeeCode"].ToString();
                    var emp = employees.FirstOrDefault(f => f.employeeCode == code);
                    if (emp != null)
                    {
                        var design = d["Designation"].ToString();
                        var id = objdb.tbl_Designation
                            .Where(w => w.designationName.ToLower().Trim() == design.ToLower().Trim()).FirstOrDefault();
                        if (id == null)
                        {
                            id = new tbl_Designation
                            {
                                designationName = design, narration = string.Empty, advanceAmount = 0,
                                extra1 = string.Empty, extra2 = string.Empty, extraDate = DateTime.Now, leaveDays = 0
                            };

                            objdb.tbl_Designation.Add(id);
                            objdb.SaveChanges();
                        }

                        emp.designationId = id.Id;
                        emp.defaultPackageId = salaryPackage.salaryPackageId;
                    }
                }
            }

            return employees;
        }

        public byte[] DownloadStudentSampleSheet()
        {
            //System.IO.FileInfo file = new System.IO.FileInfo(path);//+ DateTime.Now.ToString("yyyy-MM-dd--hh-mm-ss") + ".xlsx");
            var memoryStream = new MemoryStream();
            using (var pack = new ExcelPackage(memoryStream))
            {
                var sheet = pack.Workbook.Worksheets.Add("Students");
                //var students = SchoolEntities.Students.Take(1).AsDataTable(true);

                var rowNumber = 1;
                var colno = 1;
                foreach (var sub in Cols())
                {
                    sheet.Cells[rowNumber, colno].Value = sub;
                    colno++;
                }

                using (var range = sheet.Cells[rowNumber, 1, rowNumber, colno])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    range.Style.Font.Color.SetColor(Color.Black);
                    range.Style.ShrinkToFit = false;
                }

                rowNumber++;


                for (var i = 1; i <= colno; i++)
                    sheet.Column(i).AutoFit();


                // Set some document properties
                pack.Workbook.Properties.Title = "Student Entry Sheet Sample";
                pack.Workbook.Properties.Author = "Ejaz";
                pack.Workbook.Properties.Company = "SICo";
                pack.Workbook.Properties.Subject = "Student Entry Sheet Sample";
                // save our new workbook and we are done!
                //pack.Save();

                return pack.GetAsByteArray();
                //-------- Now leaving the using statement
            } // Outside the using statement

            //byte[] bytesInStream = new byte[memoryStream.Length];
            //memoryStream.Write(bytesInStream, 0, bytesInStream.Length);
            //memoryStream.Close();

            //return bytesInStream;
        }

        public byte[] DownloadResultSampleSheet()
        {
            //System.IO.FileInfo file = new System.IO.FileInfo(path);//+ DateTime.Now.ToString("yyyy-MM-dd--hh-mm-ss") + ".xlsx");
            var memoryStream = new MemoryStream();
            using (var pack = new ExcelPackage(memoryStream))
            {
                var sheet = pack.Workbook.Worksheets.Add("Student Results");
                //var students = SchoolEntities.Students.Take(1).AsDataTable(true);

                var rowNumber = 1;
                var colno = 1;
                sheet.Cells[rowNumber, colno].Value = "RegNo";
                colno++;

                var subjects = objdb.Subjects.Select(s => s.SubjectName.Trim()).ToList().ToArray();
                foreach (var sub in subjects)
                {
                    sheet.Cells[rowNumber, colno].Value = sub;
                    colno++;
                }

                using (var range = sheet.Cells[rowNumber, 1, rowNumber, colno])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    range.Style.Font.Color.SetColor(Color.Black);
                    range.Style.ShrinkToFit = false;
                }

                rowNumber++;


                for (var i = 1; i <= colno; i++)
                    sheet.Column(i).AutoFit();


                // Set some document properties
                pack.Workbook.Properties.Title = "Result Entry Sheet Sample";
                pack.Workbook.Properties.Author = "Ejaz";
                pack.Workbook.Properties.Company = "SICo";
                pack.Workbook.Properties.Subject = "Result Entry Sheet Sample";
                // save our new workbook and we are done!
                //pack.Save();

                return pack.GetAsByteArray();
                //-------- Now leaving the using statement
            } // Outside the using statement

            //byte[] bytesInStream = new byte[memoryStream.Length];
            //memoryStream.Write(bytesInStream, 0, bytesInStream.Length);
            //memoryStream.Close();

            //return bytesInStream;
        }

        public byte[] DownloadEmployeeSampleSheet()
        {
            //System.IO.FileInfo file = new System.IO.FileInfo(path);//+ DateTime.Now.ToString("yyyy-MM-dd--hh-mm-ss") + ".xlsx");
            var memoryStream = new MemoryStream();
            using (var pack = new ExcelPackage(memoryStream))
            {
                var sheet = pack.Workbook.Worksheets.Add("Employees");
                //var students = SchoolEntities.Students.Take(1).AsDataTable(true);

                var rowNumber = 1;
                var colno = 1;
                foreach (var sub in EmployeeCols())
                {
                    sheet.Cells[rowNumber, colno].Value = sub;
                    colno++;
                }

                using (var range = sheet.Cells[rowNumber, 1, rowNumber, colno])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    range.Style.Font.Color.SetColor(Color.Black);
                    range.Style.ShrinkToFit = false;
                }

                rowNumber++;


                for (var i = 1; i <= colno; i++)
                    sheet.Column(i).AutoFit();


                // Set some document properties
                pack.Workbook.Properties.Title = "Employee Entry Sheet Sample";
                pack.Workbook.Properties.Author = "Ejaz";
                pack.Workbook.Properties.Company = "SICo";
                pack.Workbook.Properties.Subject = "Employee Entry Sheet Sample";
                // save our new workbook and we are done!
                //pack.Save();

                return pack.GetAsByteArray();
                //-------- Now leaving the using statement
            } // Outside the using statement

            //byte[] bytesInStream = new byte[memoryStream.Length];
            //memoryStream.Write(bytesInStream, 0, bytesInStream.Length);
            //memoryStream.Close();

            //return bytesInStream;
        }

        public string[] Cols()
        {
            string[] col =
            {
                "RegistrationNo",
                "StudentName",
                "FName",
                "Sex",
                "StudentCNIC",
                "DOB", "FatherQualification", "FatherProfession", "FatherNatureOfJob", "FatherDepartment",
                "CNIC",
                "FatherAlive", "LiveWith", "MotherName", "MotherQualification", "MotherProfession",
                "MotherTongue", "GuardianName", "StdRelation",
                "PermenantAddress", "PostalAddress",
                "ResidanceTelephone",
                "OfficeTelephone", "Email",
                "Domicile", "Religion", "Nationality", "Games", "MissingDocuments", "DateForSubmission",
                "LastIntitution", "Remarks", "GeneralRemarks", "AdmittedClass", "AdmissionDate", "mobiles", "Class",
                "Section"
            };
            return col;
        }

        public string[] EmployeeCols()
        {
            string[] col =
            {
                "employeeCode", "employeeName", "FatherName", "CNIC",
                "dob", "maritalStatus", "gender", "qualification", "address", "phoneNumber", "mobileNumber", "email",
                "joiningDate", "narration", "bloodGroup", "passportNo", "passportExpiryDate", "labourCardNumber",
                "labourCardExpiryDate", "visaNumber", "visaExpiryDate", "bankName", "branchName", "branchCode",
                "bankAccountNumber", "panNumber", "pfNumber", "esiNumber", "Designation"
            };
            return col;
        }
    }
}