using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Models;
using smsCore.Data.Helpers;

namespace smsCore.Controllers
{
    [Authorize]
    public class UploadDataController : BaseController
    {
        private readonly SchoolEntities db;
        private readonly ExcelSheetWorker _worker;

        public UploadDataController(SchoolEntities _db, ExcelSheetWorker worker)
        {
            db = _db;
            _worker = worker;
        }

        public object ValidateObj(object student)
        {
            var p = student.GetType().GetFields(BindingFlags.Public |
                                                BindingFlags.NonPublic |
                                                BindingFlags.Instance);
            foreach (var member in p)
            {
                var val = member.GetValue(student);
                if (member.FieldType == typeof(DateTime))
                    if (val == null || string.IsNullOrEmpty(val.ToString()) ||
                        !DateTime.TryParse(val.ToString(), out var re) || re > DateTime.Now.AddDays(5000).Date)
                        member.SetValue(student, DateTime.Now);

                if (member.FieldType == typeof(string))
                    if (val == null || string.IsNullOrEmpty(val.ToString()))
                        member.SetValue(student, string.Empty);

                if (member.FieldType == typeof(int) || member.FieldType == typeof(decimal) ||
                    member.FieldType == typeof(double))
                    if (val == null || string.IsNullOrEmpty(val.ToString()))
                        member.SetValue(student, 0);
                try
                {
                    if (member.FieldType == typeof(DateTime))
                        if (((DateTime) val).Year == 1)
                            member.SetValue(student, DateTime.Now);
                }
                catch
                {
                }
            }

            return student;
        }

        
        public string SaveDataEmployee(List<tbl_Employee> employee, int CampusId)
        {
            var message = "";
 
            if (employee.Count > 0)
            {
                

                for (var i = 0; i < employee.Count; i++)
                {
                    var dr = new tbl_Employee();

                    dr = (tbl_Employee) ValidateObj(employee[i]);
                    if (string.IsNullOrEmpty(dr.employeeName))
                        continue;
                    var dr2 = db.tbl_Employee.Where(w => w.employeeCode == dr.employeeCode).FirstOrDefault();
                    if (dr2 == null)
                    {
                        dr.CampusID = CampusId;

                        db.tbl_Employee.Add(dr);
                    }
                    else
                    {
                        return "Employee Code  Already Exist";
                    }
                }

                try
                {
                    db.SaveChanges();
                    message = "success";
                }
                catch (Exception ex)
                {
                    message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                }
            }

            return message;
        }

        // GET: UploadData
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> UploadResultData(int CampusId, int ClassId, int ExamHeldId)
        {
            var message = "";
            var ExcelFile = Request.Form["StudentExcelSheet"].ToString();
            if (!string.IsNullOrEmpty(ExcelFile))
            {
                var Students = await _worker.GetResults(new MemoryStream(Encoding.UTF8.GetBytes(ExcelFile)), ClassId, CampusId, ExamHeldId);
                try
                {
                    if (!Students.Status)
                    {
                        message = Students.Error;
                        if (Students.Faild != null && Students.Faild.Count > 0)
                        {
                            message = "Unable to save " + Students.Faild.Count + " students result. " + string.Join(", ", Students.Faild.Select(s => s.Status).Distinct());
                        }
                    }
                    else if (Students.Response.Count == 0)
                    {
                        message = "No Student record added.";
                    }
                    else
                    {
                        message = "success";
                    }
                }
                catch (Exception ex)
                {
                    //    message = ex.InnerException == null ? ex.Message : ex.InnerException.InnerException.Message;
                }
                return Json(new { status = Students.Status, message = message == "success" ? "Record uploaded successfully." : message });
            }
            return Json(new { status = false, message = "Please upload an excel file to continue." });
        }


        [HttpPost]
        public JsonResult UploadStdData(int Session, int Campus, int Class = 0, int Section = 0, bool ClassSelected = false, bool SectionSelected = false)
        {
            var message = "";
            var ExcelFile = Request.Form.Files["StudentExcelSheet"];
            if (!ClassSelected)
                Class = 0;
            if (!SectionSelected)
                Section = 0;
            ClassSelected = false;
            SectionSelected = false;
            if (ExcelFile!=null)
            {
                var stream = ExcelFile.OpenReadStream();
               // var worker = new ExcelSheetWorker();
                var Students = _worker.GetStudents(stream, Session, Campus, Class, Section, ClassSelected,
                    SectionSelected);
                for (var i = 0; i < Students.Count; i++) Students[i] = (Student) ValidateObj(Students[i]);
                var classSectionId = db.ClassSections.Where(w => w.ClassID == Class && w.SectionID == Section)
                    .Select(s => s.ID).FirstOrDefault();

                 

                db.Students.AddRange(Students);
                try
                {
                    if (Students.Count == 0)
                    {
                        message = "No Student record added.";
                    }
                    else
                    {
                        db.SaveChanges();
                        message = "success";
                    }
                }
                catch (Exception ex)
                {
                 //   message = ex.InnerException == null ? ex.Message : ex.InnerException.InnerException.Message;
                }

            }
            return Json(new { status = message=="success", message = message=="success" ? "Record uploaded successfully." : message });
        }

        [HttpGet]
        public void DownloadSampleSheet()
        {
           // var worker = new ExcelSheetWorker();
            var bytes = _worker.DownloadStudentSampleSheet();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.Headers.Append(
                "content-disposition",
                string.Format("attachment;  filename={0}", "Student_Uplaod_Format.xlsx"));
            Response.WriteAsJsonAsync(bytes);
        }

        [HttpGet]
        public void DownloadResultSampleSheet()
        {
          //  var worker = new ExcelSheetWorker();
            var bytes = _worker.DownloadResultSampleSheet();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.Headers.Append(
                "content-disposition",
                string.Format("attachment;  filename={0}", "Student Result Upload Sheet.xlsx"));
            Response.WriteAsJsonAsync(bytes);
        }
        [HttpPost]
        public JsonResult UploadEmployeeData(int Campus)
        {
            var message = "";
            //var ExcelFile = Request.Files["EmployeeExcelSheet"];
            //if (ExcelFile != null)
            //{
            //    var worker = new ExcelSheetWorker();
            //    var employees = worker.GetEmployees(ExcelFile.InputStream);
            //    for (var i = 0; i < employees.Count; i++) employees[i] = (tbl_Employee) ValidateObj(employees[i]);
            //    message= SaveDataEmployee(employees, Campus);
            //}


            return Json(new { status = message=="success", message = message=="success" ? "Record uploaded successfully." : message });
        }

        [HttpGet]
        public void DownloadEmployeeSampleSheet()
        {
           // var worker = new ExcelSheetWorker();
            var bytes = _worker.DownloadEmployeeSampleSheet();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            //Response.AddHeader(
            //    "content-disposition",
            //    string.Format("attachment;  filename={0}", "Employee_Uplaod_Format.xlsx"));
            //Response.BinaryWrite(bytes);
        }
    }
}