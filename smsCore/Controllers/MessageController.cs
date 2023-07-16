using System.Collections;
using System.Data;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Models;
using SchoolManagementSystem.Helpers;
using sms.Hubs;
using smsCore;
using smsCore.Controllers;
using smsCore.Data.Helpers;
using smsCore.Data.Models.ViewModels;
using Syncfusion.EJ2.Base;
using Utilities;

namespace msCore.Controllers
{
    //[CompressContent]
   // [System.Web.Mvc.Authorize]
    public class MessageController : BaseController
    {
        #region private variable
        private readonly DatabaseAccessSql dba;
        private readonly SchoolEntities db;
        private readonly StaticResources _st;
        private readonly MessagingSystem ms;
        private readonly UtilityFunctions _utitlity;
        private readonly ClsBussinessSetting setting;
        private readonly IHubContext<SmsHub> hubContext;

        public MessageController(SchoolEntities _db, DatabaseAccessSql sq, StaticResources st,MessagingSystem _ms, UtilityFunctions utitlity, ClsBussinessSetting _setting,IHubContext<SmsHub> _hub)
        {
            db = _db;
            dba = sq;
            _st = st;
            ms = _ms;
            _utitlity = utitlity;
            setting = _setting;
            hubContext = _hub;
        }
        #endregion

        private static void SendNotification(IHubContext<SmsHub> context, object data)
        {
            //context.Clients.Group(HubSubscriptions.FeeEntry.ToString()).notification(data);
            context.Clients.All.SendCoreAsync("notification",new object[] { data });
        }

        #region MessageSubscribtion

        [HttpGet]
        public IActionResult MessageSubscribtion()
        {
            return View();
        }

        
        [HttpPost]
        public string MessageSubscribtion(MessageRecord message)
        {
            var msg = string.Empty;
            var data = new DataTable();
            var cmbClass = Request.Form["Classes"];
            var cmbCampus = Request.Form["Campus" +
                                         ""];
            var cmbSection = Request.Form["Sections"];

            var clsID = cmbClass == "all" ? 0 : int.Parse(cmbClass);

            int[] campid = {int.Parse(cmbCampus)};

            int[] secid = {0};

            try
            {
                secid[0] = int.Parse(cmbSection);
            }
            catch
            {
            }

            if (cmbSection == "0" || cmbSection== "all")
            {
                if (clsID > 0)
                    secid = db.ClassSections.Where(w => w.ClassID == clsID).Select(s => s.ID).ToArray();
                else
                    secid = db.ClassSections.Select(s => s.ID).ToArray();
            }

            if (cmbCampus == "0") campid = db.Campuses.Select(s => s.ID).ToArray();

            var q =
                "SELECT TOP (100) PERCENT dbo.Students.RegistrationNo, dbo.Students.StudentName, dbo.Students.FName, dbo.Classes.ClassName, dbo.Sections.SectionName, dbo.Campuses.CampusName, CONVERT(bit, dbo.ComSubs.Attendance) as Attendance , Convert(bit ,dbo.ComSubs.Result) as Result, Convert(bit,dbo.ComSubs.IssuedFee) as IssuedFee, Convert(bit, dbo.ComSubs.ReceiveFee) as ReceiveFee FROM dbo.Admissions INNER JOIN dbo.Students ON dbo.Admissions.StudentID = dbo.Students.ID INNER JOIN dbo.ClassSections ON dbo.Admissions.ClassSectionID = dbo.ClassSections.ID LEFT OUTER JOIN dbo.ComSubs ON dbo.Students.ID = dbo.ComSubs.StudentID INNER JOIN dbo.Campuses ON dbo.Admissions.CampuseID = dbo.Campuses.ID INNER JOIN dbo.Classes ON dbo.ClassSections.ClassID = dbo.Classes.ID INNER JOIN dbo.Sections ON dbo.ClassSections.SectionID = dbo.Sections.ID WHERE (dbo.Admissions.IsExpell = 0) AND (dbo.ClassSections.ID IN (" +
                string.Join(",", secid) + ")) AND ( dbo.Campuses.ID IN (" + string.Join(",", campid) +
                ")) ORDER BY dbo.Students.RegistrationNo";

            //var dba = new DatabaseAccessSql();

            data = dba.CreateTable(q);

            if (data.Rows.Count < 0) msg = "No data found to display";


            var total = data.Rows.Count + " out of " + db.Admissions.Where(w => w.IsExpell == false).Count();
            ViewBag.labeltotal = total;
            //return View(data);
            return msg;
        }

        public IActionResult MessageSubscribtionList(DataManagerRequest request, int campid, int secid, int clsid)
        {
            var q =
                @"SELECT dbo.Students.ID as StudentId, dbo.Students.RegistrationNo, dbo.Students.StudentName, dbo.Students.FName, dbo.Classes.ClassName, dbo.Sections.SectionName, 
dbo.Campuses.CampusName, CONVERT(bit, Isnull(dbo.ComSubs.Attendance,0)) as Attendance , Convert(bit ,Isnull(dbo.ComSubs.Result,0)) as Result, 
Convert(bit,Isnull(dbo.ComSubs.IssuedFee,0)) as IssuedFee, Convert(bit, Isnull(dbo.ComSubs.ReceiveFee,0)) as ReceiveFee 
FROM dbo.Admissions INNER JOIN dbo.Students ON dbo.Admissions.StudentID = dbo.Students.ID INNER JOIN 
dbo.ClassSections ON dbo.Admissions.ClassSectionID = dbo.ClassSections.ID LEFT OUTER JOIN dbo.ComSubs ON 
dbo.Students.ID = dbo.ComSubs.StudentID INNER JOIN dbo.Campuses ON dbo.Admissions.CampuseID = dbo.Campuses.ID INNER JOIN 
dbo.Classes ON dbo.ClassSections.ClassID = dbo.Classes.ID INNER JOIN dbo.Sections ON dbo.ClassSections.SectionID = dbo.Sections.ID 
WHERE (dbo.Admissions.IsExpell = 0) AND (dbo.ClassSections.ClassID IN (" + string.Join(",", clsid) +
                ")) AND(dbo.ClassSections.SectionID IN (" + string.Join(",", secid) + ")) AND ( dbo.Campuses.ID IN (" +
                string.Join(",", campid) + ")) " +
                "ORDER BY dbo.Students.RegistrationNo ";


            

            var table = dba.CreateTable(q).AsEnumerable().Select(s=>new { 
                RegistrationNo = s.Field<int>("RegistrationNo"),
                StudentName = s.Field<string>("StudentName"),
                FName = s.Field<string>("FName"),
                Attendance = s.Field<bool>("Attendance"),
                Result = s.Field<bool>("Result"),
                ReceiveFee = s.Field<bool>("ReceiveFee"),
                IssuedFee = s.Field<bool>("IssuedFee"),
                StudentId = s.Field<int>("StudentId")
            });
           
            return Json(new { result= table, count = table.Count() });

        }

        [HttpPost]
        public async Task<JsonResult> MessageSubscription(List<MessageSubscriptionViewModel> changed)
        {
            foreach (var e in changed)
            {
                var exist = db.ComSubs.FirstOrDefault(w => w.StudentID == e.StudentId);
                if (exist == null)
                {
                    exist = new ComSub() { StudentID = e.StudentId };
                    db.ComSubs.Add(exist);
                }

                exist.Attendance = e.Attendance ? 1 : 0;
                exist.IssuedFee = e.IssuedFee ? 1 : 0;
                exist.ReceiveFee = e.ReceiveFee ? 1 : 0;
                exist.Result = e.Result ? 1 : 0;

            }

            try
            {
                await db.SaveChangesAsync();
                return Json(new { status = true, message = "Data saved successfully." });
            }
            catch
            {
                return Json(new { status = false, message = "Failed to save data." });
            }

        }

     
        #endregion

        #region Send Message by Student Name

        public IActionResult SendMessagebyStudentName()
        {
            return View();
        }


        public void SendMessagebyStudentNamesTask(List<BulkMessageViewModel> records)
        {
            SendNotification(hubContext, _st.QeuedReturn("Send Message Task", StaticResources.ToastType.info, "Task Started.", 0, false));
            int total = records.Count;
            int per_page = 40;
            bool sent = false;
            for (int i = 0; i < total;)
            {
                var page = records.Skip(i).Take(per_page).Select(s => s.Mobile).ToArray();
                if (page.Count() > 0)
                    sent = ms.sendMessage(records[0].Message, page, records[0].IsRtl).Result;
                SendNotification(hubContext, _st.QeuedReturn("Send Message Task", StaticResources.ToastType.info, "Sending messages to students.", i , false));
                i += per_page;
            }

            SendNotification(hubContext, _st.QeuedReturn("Send Message Task", StaticResources.ToastType.info, "Task Completed.", 100, true));

        }

        [HttpPost]
        public JsonResult SendMessagebyStudentNames(List<BulkMessageViewModel> records)
        {
            var errormessage = string.Empty;

            if (!string.IsNullOrEmpty(ms.error)) return Json(new { status = false, message = ms.error });

            if (records.Count == 0)
                return Json(new { status = false, message = "No data found" });

            try
            {
                BackgroundJob.Enqueue(() => SendMessagebyStudentNamesTask(records));
                return Json(new { status = true, message = _st.QeuedMessage });
            }
            catch (Exception e)
            {
                errormessage = e.Message;
            }

            return Json(new { status = errormessage == "success", message = errormessage });
        }

        public ActionResult StudentListForBulkMessage(DataManagerRequest request, int regnoinput = -1,
            string mobileinput = "", int clsid = -1, int campid = -1)
        {
            var campus = campid == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] {campid};
            var classId = clsid == -1 ? db.Classes.Select(s => s.ID).ToArray() : new[] {clsid};

            var studentdetails = db.Admissions.Where(w =>
                !w.IsExpell && w.Student.StudentMobiles.Count() > 0 && campus.Contains(w.CampuseID) &&
                classId.Contains(w.ClassSection.ClassID)).Select(s=>new {
                    s.Student.RegistrationNo,
                    s.Student.StudentName,
                    Mobileno = s.Student.StudentMobiles.Where(w => w.IsDefault).Select(w => w.MobileNo).FirstOrDefault()
                });

            if (regnoinput != -1) studentdetails = studentdetails.Where(w => w.RegistrationNo == regnoinput);
            
            mobileinput = mobileinput.Trim();
            if (mobileinput != "")
                studentdetails = studentdetails.Where(w => w.Mobileno == mobileinput);
            studentdetails = studentdetails.OrderBy(o => o.RegistrationNo);
            int count = studentdetails.Count();
            DataOperations operation = new DataOperations();
            if(request.Skip!=0)
            {
                studentdetails= operation.PerformSkip(studentdetails, request.Skip);
            }
            if (request.Take != 0)
                studentdetails= operation.PerformTake(studentdetails, request.Take);
            
            return Json(new { result= studentdetails, count= count });
        }

        #endregion

        #region Send Result

        
        public ActionResult SentResult()
        {
            return View();
        }

        public void SendResultTask(int examId, string[] RegNo,int campusid, int classid, int sectionid, string remarks, bool save,bool showposition)
        {

            if(!string.IsNullOrEmpty(ms.error))
            {
                SendNotification(hubContext, new { title = "error", message = ms.error, completed = true });
            }
            var examID = db.ExamHelds.Where(w => w.ID == examId).Select(s => new {s.ID, s.Exam.ExamName, s.EntryDate }).FirstOrDefault();
            var dataTable = new DataTable();
            try
            {
                for (var j = 0; j < RegNo.Count(); j++)
                {
                    var regno = int.Parse(RegNo[j]);
                    var AdmIds = db.Results
                        .Where(w =>w.ExamHeldID==examId && w.Admission.IsExpell == false && w.Admission.Student.RegistrationNo == regno)
                        .Select(s => s.ID).ToArray();
                    var stdid = db.Students.Where(w => w.RegistrationNo == regno).FirstOrDefault().ID;
                    var substd =
                        dba.CreateTable(
                            "SELECT Result, StudentID FROM dbo.ComSubs WHERE (Result = 1) AND (StudentID = " + stdid +
                            ")");
                    if (substd.Rows.Count < 1)
                        continue;
                    var mob = db.StudentMobiles.Where(w => w.Student.RegistrationNo == regno && w.IsDefault)
                        .FirstOrDefault();
                    if (mob == null)
                    {
                        continue;
                    }
                    //Status[j]= "No mobile no OR default no defined.";
                    //return "Mobile Number Not Found";
                    var rs = new ResultSystem("(dbo.Results.ID IN (" +
                                              string.Join(",", AdmIds.Select(s => s.ToString().Trim())) + " ))",ShowPosition: showposition);
                    var tab = rs.GetData();
                    if (tab.Rows.Count < 1)
                        continue;
                        //return "Not Found";
                    List<string> cols =new List<string> {
                            "RegistrationNo", "StudentName", "TotalObtained", "ClassPos", "SecPos", "GrantTotal",
                            "TotalPercentage", "TotalStatus"
                        };
                    if (!showposition)
                    {
                        cols.Remove("ClassPos");
                        cols.Remove("SecPos");
                    }
                    var pt = new Pivot(tab); //tab.Columns.Contains("GroupName") ? "GroupName" : "FName", "SectionName",
                    var dt = pt.PivotData("ObtainedMarks", AggregateFunction.First,
                        cols.ToArray(), new[] { "SubjectName" });
                    var lastordinal = dt.Columns.Count;

                    var mobno = "";// mob.MobileNo.Trim();

                    var message = "Result of " + examID.ExamName + " (" + examID.EntryDate.ToString("MMM, yyyy") + ")";
                    foreach (DataRow dr in dt.Rows)
                    {
                        var k = 1;
                        message += "\nName: "+ dr["StudentName"] + " (" + dr["RegistrationNo"] + ")";
                        foreach (DataColumn colss in dt.Columns)
                        {
                            if (k > 8) message += "\n" + colss.ColumnName + "(" + dr[colss.ColumnName] + ")";
                            k++;
                        }
                        message += "\nObtained: " + dr["TotalObtained"] + "";
                        message += "\nTotal: " + dr["GrantTotal"] + "";
                        message += "\n%age: " + dr["TotalPercentage"] + "";
                        if (showposition)
                            message += "\nPosition: " + dr["SecPos"] + "";

                        message += remarks == string.Empty ? "" : ("\nRemarks: " + remarks.Trim());
                        _ = ms.sendMessage(regno, message, save);
                    }
                }

                db.SaveChanges();
            }
            catch (Exception aa)
            {
                //ModelState.AddModelError("", aa);
            }
        }
        
        [HttpPost]
        public JsonResult SentResult([FromForm] MessageRecord model)
        {
            if (!string.IsNullOrEmpty(ms.error))
                return Json(new { status = false, message = ms.error });

            if (string.IsNullOrEmpty(Request.Form["Regno"]))
                return Json(new { status = false, message = "No Registration No Found" });
            var RegNo = Request.Form["Regno"].ToString().Split(',');

            if (string.IsNullOrEmpty(Request.Form["Exam"]))
                return Json(new { status = false, message = "No Exam Found" });
            var cmbExam = int.Parse(Request.Form["Exam"].ToString());

            if (string.IsNullOrEmpty(Request.Form["Campus"]))
                return Json(new { status = false, message = "No Campus Found" });
            var campusid = int.Parse(Request.Form["Campus"].ToString());

            if (string.IsNullOrEmpty(Request.Form["Class"]))
                return Json(new { status = false, message = "No Section data Found" });
            var classid = int.Parse(Request.Form["Class"].ToString());

            if (string.IsNullOrEmpty(Request.Form["Section"]))
                return Json(new { status = false, message = "No Section data Found" });
            var sectionid = int.Parse(Request.Form["Section"].ToString());

            var remarks = Request.Form["Remarks"].ToString();
            var save = !string.IsNullOrEmpty(Request.Form["save"]);

            bool showPosition = !string.IsNullOrEmpty(Request.Form["showposition"]);

            BackgroundJob.Enqueue(() => SendResultTask(cmbExam, RegNo, campusid, classid, sectionid, remarks, save, showPosition));
            return Json(new { status = true, message = _st.QeuedMessage });
        }


        //partial view of result
        public IActionResult SendResultViewData(string cmbClass, string cmbCampus, string cmbSection, string cmbExam,
            string attendances, bool showposition)
        {
            var dt = new DataTable();
            var examID = int.Parse(cmbExam);
            var classID = int.Parse(cmbClass);
            var sectionid = int.Parse(cmbSection);
            var campusid = int.Parse(cmbCampus);
            var clasSectionId = db.ClassSections.Where(w =>w.CampusID==campusid && w.ClassID == classID && w.SectionID == sectionid)
                .Select(s => s.ID).FirstOrDefault();
            var cbxAttendance = true;
            var AdmIds = db.Results
                .Where(w => w.ExamHeldID == examID && w.Admission.CampuseID == campusid &&
                            w.Admission.ClassSectionID == clasSectionId).Select(s => s.ID).ToArray();
            if (AdmIds.Count() < 1) 
                return View("_MessagesendresultView");

            var rs = new ResultSystem("(dbo.Results.ID IN (" +
                                      string.Join(",", AdmIds.Select(s => s.ToString().Trim())) + " ))",ShowPosition:showposition);
            var tab = rs.GetData();
            if (tab.Rows.Count < 1)
                return View("_MessagesendresultView");
            
            List<string> cols =new List<string>()
                {
                    "RegistrationNo", "StudentName", "TotalObtained", "ClassPos", "SecPos", "GrantTotal",
                    "TotalPercentage", "TotalStatus"
                };
            if (!showposition)
            {
                cols.Remove("ClassPos");
                cols.Remove("SecPos");
            }
            var pt = new Pivot(tab); //tab.Columns.Contains("GroupName") ? "GroupName" : "FName", "SectionName",
            dt = pt.PivotData("ObtainedMarks", AggregateFunction.First,
                cols.ToArray(), new[] {"SubjectName"});
            var lastordinal = dt.Columns.Count;
            dt.Columns["TotalObtained"].SetOrdinal(lastordinal - 1);
            dt.Columns["GrantTotal"].SetOrdinal(lastordinal - 1);
            dt.Columns["TotalPercentage"].SetOrdinal(lastordinal - 1);
            if (showposition)
            {
                dt.Columns["SecPos"].SetOrdinal(lastordinal - 1);
                dt.Columns["ClassPos"].SetOrdinal(lastordinal - 1);
            }

            dt.Columns["TotalStatus"].SetOrdinal(lastordinal - 1);
            dt.Columns["TotalObtained"].ColumnName = "Obtained";
            dt.Columns["GrantTotal"].ColumnName = "Total";
            dt.Columns["TotalStatus"].ColumnName = "Remarks";
            dt.Columns["TotalPercentage"].ColumnName = "%age";
            if (showposition)
            {
                dt.Columns["ClassPos"].ColumnName = "C.Pos";
                dt.Columns["SecPos"].ColumnName = "Sec. Pos";
            }
            dt.Columns["StudentName"].ColumnName = "Student's Name";
            dt.Columns["RegistrationNo"].ColumnName = "Reg No.";
            if (cbxAttendance)
            {
                //var ga = new Utilities.AttendanceHelper();
                //dt.Columns.Add("Attendance").SetOrdinal(dt.Columns.Count - 1);
                //var exheld = db.ExamHelds.Where(w => w.ID == examID).FirstOrDefault();
                //var examdate = exheld.EntryDate;
                //var examinationID = exheld.Exam.ID;
                //foreach (DataRow r in dt.Rows)
                //{

                //    var stdid = db.Students.Where(w => w.RegistrationNo == r.Field<int>("ID"))
                //        .FirstOrDefault().ID;

                //    if (r["Attendance"].ToString().Trim() == string.Empty ||
                //        r["Attendance"].ToString().Trim() == "" || r["Attendance"] == null)
                //        try
                //        {
                //            var attendance = ga.GetYearlyAttendance(stdid, examdate, 'p');
                //            r["Attendance"] = attendance;
                //        }
                //        catch
                //        {
                //        }
                //}

            }

            //lblcount.Text = dataGridView1.Rows.Count.ToString() + " out of " + objdb.Admissions.Where(w => w.IsExpell == false).Count();
            return View("_MessagesendresultView ", dt);
        }

        #endregion

        #region SentComplaints

        
        public IActionResult SentComplaints()
        {
            return View();
        }

        public void SendComplaintsTask(List<StudentComplaintMessageViewModel> records)
        {
            SendNotification(hubContext, _st.QeuedReturn("Send Complaints Task", StaticResources.ToastType.info, "Task Started.", 0, false));
            for (var i = 0; i < records.Count(); i++)
            {
                if (records[i].Mobile == null)
                {
                    records[i].Status = "Invalid or No default Mobile no found.";
                    continue;
                }
                var message = string.Format("{0} {1}-{2} ", records[i].Name,
                    records[i].RegNo + "-" + records[i].Class, records[i].Message);

                if (ms.sendMessage(message, records[i].Mobile, records[i].AdmissionID, false).Result)
                {
                    records[i].Status = "Message sent successfully.";
                }
                SendNotification(hubContext, _st.QeuedReturn("Send Complaints Task", StaticResources.ToastType.info, "Sending messages to students.", i + 1, false));

            }
            SendNotification(hubContext, _st.QeuedReturn("Send Complaints Task", StaticResources.ToastType.info, "Task Completed.", 100, true));

        }

        [HttpPost]
        public JsonResult SentComplaints(List<StudentComplaintMessageViewModel> records)
        {
            try
            {
                //string running = StaticResources.GetTaskRunning("SendComplaintsTask");
                if (!string.IsNullOrEmpty(ms.error))
                    return Json(new { status = false, message = ms.error });
                BackgroundJob.Enqueue(() => SendComplaintsTask(records));
                return Json(new { status = true, message = _st.QeuedMessage });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message, exception=ex.ToString() });
            }
        }

        public JsonResult StudenInfoForComplaints(int regno)
        {
            var data = db.Admissions.Where(w => w.Student.RegistrationNo == regno && !w.IsExpell & w.Student.StudentMobiles.Any(t => t.IsDefault)).Select(s => new
            {
                s.ID,s.StudentID,
                regno = s.Student.RegistrationNo, stdName = s.Student.StudentName,
                clsName = s.ClassSection.Class.ClassName, cmpName = s.Campus.CampusName,
                Mobile = s.Student.StudentMobiles.Where(w=>w.IsDefault).Select(t=>t.MobileNo).FirstOrDefault()
            }).FirstOrDefault();


            return Json(data );
        }

        #endregion

        #region SentPublicmessage

        public IActionResult SentPublicmessage()
        {
            return View();
        }

        public void SendPublicMessageTask(List<StaffMessageViewModel> data)
        {
            SendNotification(hubContext, _st.QeuedReturn("Send Public Message Task", StaticResources.ToastType.info, "Task Started.", 0, false));
            string message = data[0].Message;
            if (ms.sendMessage(message, data.Select(s => s.Mobile).ToArray()).Result)
            {
                SendNotification(hubContext, _st.QeuedReturn("Send Public Message Task", StaticResources.ToastType.info, "Task Completed.", 100, true));
            }
            else
            {
                SendNotification(hubContext, _st.QeuedReturn("Send Public Message Task", StaticResources.ToastType.error, "Task Completed with error.", 100, true));
            }
        }
        [HttpPost]
        public JsonResult SendPublicmessage(List<StaffMessageViewModel> data)
        {
            var errormessage = string.Empty;

            if (!string.IsNullOrEmpty(ms.error)) 
                return Json(new { status = false, message = ms.error });

            if (data.Count == 0)
                return Json(new { status = false, message = "No data found" });

            try
            {
                BackgroundJob.Enqueue(() => SendPublicMessageTask(data));

                return Json(new { status = true, errormessage = _st.QeuedMessage });
            }
            catch (Exception e)
            {
                return Json(new { status = false, errormessage = e.Message });
            }
        }
        public string Addrecords(string name, string mobileno, string address, string grouptype, string remarks)
        {
            var pbmoble = new PublicMobile();
            var message = string.Empty;

            var id = 0;
            try
            {
                id = db.PublicMobiles.Max(w => w.ID);
            }
            catch
            {
            }

            pbmoble.ID = id + 1;
            pbmoble.Name = name;
            pbmoble.MobileNo = mobileno;
            pbmoble.Address = address;
            pbmoble.GroupType = grouptype;
            pbmoble.Remarks = remarks;
            try
            {
                db.PublicMobiles.Add(pbmoble);
                db.SaveChanges();
                message = "success";
            }
            catch (Exception aa)
            {
                message = aa.Message;
            }

            return message;
        }

        [HttpPost]
        public string RemovePublicContact(int id = 0)
        {
            var pbmobile = db.PublicMobiles.Find(id);
            string message;
            try
            {
                db.PublicMobiles.Remove(pbmobile);
                db.SaveChanges();
                message = "delete";
            }
            catch (Exception aa)
            {
                message = aa.Message;
            }

            return message;
        }

        public IActionResult PublicContactList(DataManagerRequest request, string mobileno = "", string name = "",
            string grouptype = "")
        {
            var totalRecords = db.PublicMobiles.Count();
         

            var data = db.PublicMobiles.Select(s => new
                {
                    s.ID,
                    s.Name,
                    s.MobileNo,
                    s.Address,
                    s.GroupType,
                    s.Remarks
                });

            if (!string.IsNullOrEmpty(mobileno) || !string.IsNullOrEmpty(name) || !string.IsNullOrEmpty(grouptype))
                data = data.Where(w => w.MobileNo == mobileno || w.Name == name || w.GroupType == grouptype);

            return Json(new { result=data, count=data.Count() });
        }

        #endregion

        #region SendDateSheet

        
        public IActionResult SendDateSheet()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> SendDateSheet(int campusid, int classid,int examid, int sectionid,string toExcludeRegNos="")
        {
            var errormessage = string.Empty;
            
            //if (!string.IsNullOrEmpty(ms.error)) return Json(new { status = false, message = ms.error });

            int[] ToExcludeAdmssionIds = { };
            var regNocheckbox = !string.IsNullOrEmpty(Request.Form["regNocheck"]);
            if (regNocheckbox)
            {
                var adms = Request.Form["RegNo"];
                ToExcludeAdmssionIds = _utitlity.ParseAdmIDs(adms);
            }
            var Mobiles = db.Admissions.Where(w => !ToExcludeAdmssionIds.Contains(w.ID) & !w.IsExpell & w.CampuseID == campusid & w.ClassSection.ClassID == classid & w.ClassSection.SectionID == sectionid & w.Student.StudentMobiles.Any(a => a.IsDefault)).
                Select(s => new {
                    Mobile = s.Student.StudentMobiles.Where(w => w.IsDefault).Select(t => t.MobileNo).FirstOrDefault()
                }).ToArray();
            if (Mobiles.Count()==0)
                return Json(new { status = false, message = "No valid/default mobile found" });

            
            var message = string.Empty;

            var exam = db.ExamHelds.Find(examid);
            if (exam == null || !exam.ExamDates.Any())
                return Json(new { status = false, message = "No datesheet data found" });
            
            message = "Date Sheet of " + exam.Exam.ExamName + " \n";

            foreach (var dateSheet in exam.ExamDates)
            {
                message += $"{dateSheet.Subject.SubjectName}  {dateSheet.ExamDate1.ToString("dd-MM-yyyy")} {dateSheet.TimeFrom} {dateSheet.ToTime} \n";
            }
            try
            {
                if (await ms.sendMessage(message, Mobiles))
                    errormessage = "success";
                else errormessage = ms.error;
            }
            catch (Exception e)
            {
                errormessage = e.Message;
            }

            return Json(new { status = errormessage == "success", errormessage = errormessage });

        }

        public IActionResult DateSheetList(DataManagerRequest request,int campusid, int classid , 
            int examid ,int sectionid)
        {
            var exams = db.ExamDates.Where(w =>w.CampusID==campusid & w.ExamHeldID == examid && w.ClassSection.ClassID == classid && w.ClassSection.SectionID == sectionid).
                Select(s => new
                {
                    s.Subject.SubjectName,
                    s.ExamDate1,
                    s.TimeFrom,
                    s.ToTime
                });

            return Json(new { result = exams, count = exams.Count() });
        }

        #endregion

        #region SendIssuedFee

        
        public ActionResult SendmessageIssuedFee()
        {
            return View();
        }

        
        [HttpPost]
        public JsonResult SendImessagessuedFee(List<IssueMessageViewModel> data, int campusId, bool remark, string month)
        {
            var errormessage = string.Empty;
            
            if (!string.IsNullOrEmpty(ms.error))
                return Json(new { status = false, message = ms.error });

            if (data.Count == 0)
                return Json(new { status = false, message = "No data found" });
            var forMonth = DateTimeHelper.ConvertDate(month, false, "MMMM,yyyy");
            if (forMonth == DateTime.MinValue)
            {
                return Json(new { status = false, message = "Select a month to continue." });
            }
            try
            {
                //BackgroundJob.Enqueue(() => BackgroundTasks.AddFeeBackgroundTask.SendIssuedMessageTask(data.Select(s=>s.RegistrationNo).ToArray(), campusId, remark, forMonth));

                return Json(new { status = true, errormessage = _st.QeuedMessage });
            }
            catch (Exception e)
            {
                return Json(new { status = false, errormessage = e.Message });
            }
        }


        public IActionResult IssuedFeeList(DataManagerRequest request, int classse, int section, int campus,
            string Month)
        {
            var dtpforMonth = DateTimeHelper.ConvertDate(Month);


            var data = db.FeeSlips.Where(w => w.Admission.ClassSection.Class.ID == classse &&
                        w.Admission.CampuseID == campus &&
                        w.Admission.ClassSection.SectionID == section &&
                        w.ForMonth.Month == dtpforMonth.Month &&
                        w.ForMonth.Year == dtpforMonth.Year)
                        .Select(s => new
                        {
                            s.AdmissionId,
                            s.Admission.Student.RegistrationNo,
                            s.Admission.Student.StudentName,
                            s.Admission.Student.FName,
                            s.Admission.ClassSection.Class.ClassName,
                            s.Admission.ClassSection.Section.SectionName,
                            s.Admission.Campus.CampusName,
                            s.ForMonth,
                            Total = s.FeeSlipDetails.Sum(sum => sum.Amount),
                            s.DueDate
                        });

            return Json(new { result = data, count = data.Count() });

        }

        #endregion

        #region SendAttendence

        
        public IActionResult SendAttendence()
        {
            return View();
        }

        public void SendAttendenceTask(List<AttendanceMessageViewModel> data, int campusid)
        {
            try
            {
                SendNotification(hubContext, _st.QeuedReturn("Send Issued Fee Message Task", StaticResources.ToastType.info, "Task Started.", 0, false));
                setting.CampusId = (campusid);

                
                string PresentMessageTemplate = (string)setting.ReadWithType("PresentMessage", typeof(string));
                if (string.IsNullOrEmpty(PresentMessageTemplate))
                {
                    PresentMessageTemplate = "Dear {2}!\n Your child {1}-{0} has reached school at {4} {3}.";
                    setting.WriteorUpdate("PresentMessage", PresentMessageTemplate);
                }

                string AbsentMessageTemplate = (string)setting.ReadWithType("AbsentMessage", typeof(string));
                if (string.IsNullOrEmpty(AbsentMessageTemplate))
                {
                    AbsentMessageTemplate = "Dear {2}!\n Your child {1}-{0} is absent from school  {4}.";
                    setting.WriteorUpdate("AbsentMessage", AbsentMessageTemplate);
                }

                string LeaveMessageTemplate = (string)setting.ReadWithType("LeaveMessage", typeof(string));
                if (string.IsNullOrEmpty(LeaveMessageTemplate))
                {
                    LeaveMessageTemplate = "Dear {2}!\n Your child {1}-{0} is on leave from school  {4}.";
                    setting.WriteorUpdate("LeaveMessage", LeaveMessageTemplate);
                }
                for (var i = 0; i < data.Count(); i++)
                {
                    var message = "";

                    StudentAttendence atnd = db.StudentAttendences.Find(data[i].AttendanceId);
                    if (atnd==null)
                        continue;

                    data[i].AttendanceDate=atnd.AttendanceDate;
                    
                    var regno = data[i].RegistrationNo;

                    var stdid = data[i].StudentId;
                    var substd =
                        dba.CreateTable(
                            "SELECT Result, StudentID FROM dbo.ComSubs WHERE (Attendance = 1) AND (StudentID = " + stdid +
                            ")");
                    if (substd.Rows.Count < 1)
                        //dr.Cells["Status"].Value = "Not Subscribed.";
                        continue;


                    if (data[i].AttendanceType.Trim().ToLower() == "absent")
                        message = string.Format(AbsentMessageTemplate,
                            data[i].RegistrationNo,
                            data[i].StudentName,
                            data[i].FName,
                             data[i].AttendanceDate.ToString("hh:mm tt"),
                             data[i].AttendanceDate.ToString("dd/MM/yyyy"),
                             data[i].AttendanceType,
                             data[i].ClassName);
                    else if (data[i].AttendanceType.Trim().ToLower() == "leave")
                        message = string.Format(LeaveMessageTemplate,
                            data[i].RegistrationNo,
                            data[i].StudentName,
                            data[i].FName,
                             data[i].AttendanceDate.ToString("hh:mm tt"),
                             data[i].AttendanceDate.ToString("dd/MM/yyyy"),
                             data[i].AttendanceType,
                             data[i].ClassName);
                    else// (data[i].AttendanceType.Trim().ToLower() == "present")
                        message = string.Format(PresentMessageTemplate,
                            data[i].RegistrationNo,
                            data[i].StudentName,
                            data[i].FName,
                             data[i].AttendanceDate.ToString("hh:mm tt"),
                             data[i].AttendanceDate.ToString("dd/MM/yyyy"),
                             data[i].AttendanceType,
                             data[i].ClassName);


                    if (ms.sendMessage(regno, message, true).Result)
                    {
                        atnd.sendmessage = true;
                        SendNotification(hubContext, _st.QeuedReturn("Send Issued Fee Message Task", StaticResources.ToastType.info, "Task Running.", (i/data.Count)*100, false));
                    }
                }
                 db.SaveChanges();
                SendNotification(hubContext, _st.QeuedReturn("Send Issued Fee Message Task", StaticResources.ToastType.info, "Task Completed.", 100, true));

            }
            catch
            {
                SendNotification(hubContext, _st.QeuedReturn("Send Public Message Task", StaticResources.ToastType.error, "Task Completed with error.", 100, true));
            }
        }

        [HttpPost]
        public JsonResult SendAttendence(List<AttendanceMessageViewModel> data, int campusid)
        {
            var errormessage = string.Empty;
            
            if (!string.IsNullOrEmpty(ms.error))
                return Json(new { status = false, message = ms.error });

            if (data.Count == 0)
                return Json(new { status = false, message = "No data found" });
            
            try
            {
                BackgroundJob.Enqueue(() => SendAttendenceTask(data, campusid));

                return Json(new { status = true, errormessage = _st.QeuedMessage });
            }
            catch (Exception e)
            {
                return Json(new { status = false, errormessage = e.Message });
            }
        }



        public IActionResult AttendenceList(DataManagerRequest request, int classid, int sectionid, int campusid)
        {
            
            DateTime today = DateTime.Today.Date;
            var data = db.StudentAttendences.Include(i=>i.Admission.ClassSection.Class).Include(i=>i.Admission).ThenInclude(i=>i.Student).Where(w =>w.AttendanceDate== today & w.Admission.CampuseID == campusid & w.Admission.ClassSection.ClassID == classid & w.Admission.ClassSection.SectionID == sectionid).Select(s => new
            {
                AttendanceId = s.ID,
                AdmissionId = s.AdmissionID,
                StudentId=s.Admission.StudentID,
                RegistrationNo = s.Admission.Student.RegistrationNo,
                StudentName = s.Admission.Student.StudentName,
                FName = s.Admission.Student.FName,
                AttendanceDate = s.AttendanceDate,
                ClassName = s.Admission.ClassSection.Class.ClassName,
                AttendanceType = s.StudentAttendanceType.AttendanceName,
                Mobile = s.Admission.Student.StudentMobiles.Where(w => w.IsDefault).Select(t => t.MobileNo).FirstOrDefault()
            });

            return Json(new { result = data, count = data.Count() });
        }

        #endregion

        #region Send ReceiveFee

        
        public IActionResult SendReceiveFee()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> SendReceiveFee(List<IssueMessageViewModel> data, int campusId, string month)
        {
            var messagess = "";
            var Messages = new ArrayList();
            if (!string.IsNullOrEmpty(ms.error)) return Json(new { status = false, message = ms.error });

            if (data.Count == 0)
                return Json(new { status = false, message = "No data found" });

            var forMonth = DateTimeHelper.ConvertDate(month, false, "MMMM,yyyy");
            if (forMonth == DateTime.MinValue)
            {
                return Json(new { status = false, message = "Select a month to continue." });
            }

            setting.CampusId = (campusId);

            string messageTemplate = (string)setting.ReadWithType("FeeReceivedMessage", typeof(string));
            if (string.IsNullOrEmpty(messageTemplate))
            {
                messageTemplate = "Dear {0}({1}),Your Fees Payment of Rs.{2} has been received on {3} at {4}.";
                setting.WriteorUpdate("FeeReceivedMessage", messageTemplate);
            }

            try
            {
                for (var i = 0; i < data.Count; i++)
                {
                    var regno = data[i].RegistrationNo;
                    var feereceipt = db.FeeSlipReceipts.FirstOrDefault(w => w.FeeSlip.Admission.Student.RegistrationNo == regno);

                    var substd =
                        dba.CreateTable(
                            "SELECT IssuedFee, StudentID FROM dbo.ComSubs WHERE (ReceiveFee = 1) AND (StudentID = " +
                            feereceipt.FeeSlip.Admission.StudentID + ")");
                    if (substd.Rows.Count > 0)
                    {
                        var message = "";
                        
                        message = string.Format(messageTemplate,
                       feereceipt.FeeSlip.Admission.Student.StudentName, regno,
                       feereceipt.Amount.ToString(), feereceipt.EntryDate.Date.ToString("dd/MM/yyyy"),
                       feereceipt.EntryDate.TimeOfDay.ToString());


                        // message = HttpUtility.UrlEncode(message.Trim());
                        await ms.sendMessage(regno, message, true);
                    }
                }

                messagess = "success";
            }
            catch (Exception ex)
            {
                messagess = ex.Message;
            }
            return Json(new { status = messagess == "success", message = messagess });


        }

        public IActionResult ReceivedFeeList(DataManagerRequest request, string date,int campusId)
        {
            DateTime dtm = DateTimeHelper.ConvertDate(date.Trim(), false, "MMMM yyyy");

            var data = db.FeeSlipReceipts.Include(i=>i.FeeSlip.Admission.Student.StudentMobiles)
                .Where(w => w.FeeSlip.Admission.CampuseID==campusId & w.FeeSlip.ForMonth.Month == dtm.Month & w.FeeSlip.ForMonth.Year == dtm.Year
                            && w.FeeSlip.Admission.Student.StudentMobiles.Any(ww =>
                                ww.IsDefault) && w.FeeSlip.FeeSlipReceipts.Sum(c => c.Amount)>0
                            && w.FeeSlip.Admission.Student.ComSubs.Any() &&
                            w.FeeSlip.Admission.Student.ComSubs.Any(d => d.ReceiveFee == 1))
                .Select(s => new 
                {
                    s.FeeSlip.Admission.Student.RegistrationNo,
                    s.FeeSlip.AdmissionId,
                    s.FeeSlip.Admission.Student.StudentName,
                    s.FeeSlip.Admission.ClassSection.Class.ClassName,
                    s.FeeSlip.Admission.Student.StudentMobiles.DefaultIfEmpty().Where(ww => ww.IsDefault)
                        .FirstOrDefault().MobileNo,
                    s.FeeSlip.ForMonth,
                    Received = s.Amount,
                     s.EntryDate
                });


            return Json(new { result = data, count = data.Count() });
        }

        #endregion

        #region Send Message To Staff

        
        public IActionResult SendMessagetoStaff()
        {
            return View();
        }

        public IActionResult StaffListForMessage(DataManagerRequest request,int campusId)
        {
            var datas = db.tbl_Employee.Where(w =>w.CampusID==campusId & w.LeavedStaffs.Count() < 1).Select(s => new
            {
                s.Id,
                s.Title,
                s.employeeName,
                s.FatherName,
                s.mobileNumber,
                s.employeeCode
            });

            return Json(new {result=datas, count=datas.Count() });

        }

        [HttpPost]
        public async Task<JsonResult> SendMessagetoStaff(List<StaffMessageViewModel> data)
        {
            var errormessage = string.Empty;
            
            if (!string.IsNullOrEmpty(ms.error)) return Json(new { status = false, message = ms.error });

            if (data.Count == 0)
                return Json(new { status = false, message = "No data found" });

            string message = data[0].Message;

            try
            {

                if (await ms.sendMessage(message, data.Select(s => s.Mobile).ToArray()))
                    errormessage = "success";
                else errormessage = ms.error;
            }
            catch (Exception e)
            {
                errormessage = e.Message;
            }

            return Json(new { status = errormessage == "success", errormessage = errormessage });

        }

        #endregion

        #region SMS Package Details

        string loginUrl = "https://telenorcsms.com.pk:27677/corporate_sms2/login.jsp";
        string history = "https://telenorcsms.com.pk:27677/corporate_sms2/quick_message_history.jsp?p=0";
        string username = "username";//id
        string password = "password";//id
        string loginbutton = "smallbutton";//class


        public IActionResult MyAccount()
        {
            return View();
        }

        //ScrapingBrowser _scrapingbrowser = new ScrapingBrowser();
        //private bool GetHtml(string url)
        //{
        //    WebPage webPage = _scrapingbrowser.NavigateToPage(new Uri(loginUrl));
        //    PageWebForm form = webPage.FindForm("inquiryForm");
        //    return false;
        //}

        #endregion
    }

    public class FeeReceipt
    {
        public int RegistrationNo { get; set; }
        public int AdmissionID { get; set; }
        public string StudentName { get; set; }
        public string ClassName { get; set; }
        public string MobileNo { get; set; }
        public string ForMonth { get; set; }
        public decimal Received { get; set; }
        public string ReceivedDate { get; set; }
        public string ReceivedTime { get; set; }
        public int ReceiveableFeeID { get; set; }
    }
}