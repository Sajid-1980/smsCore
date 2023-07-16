using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace smsCore
{
    public  class StaticResources
    {
         const string rootPath = "/Resources/demos/Report/";
        private readonly SchoolEntities db;
        private readonly IHttpContextAccessor _context;
        private readonly IWebHostEnvironment _env;
        public StaticResources(SchoolEntities db, IHttpContextAccessor context, IWebHostEnvironment env)
        {
            this.db = db;
            _context = context;
            _env = env; 
        }

        public  JsonResult GetResult(bool status, string message)
        {
            var result = new JsonResult(new { status = status, message = message });
            return result;
        }
        public  List<string> GetReports(string foldername)
        {
            List<string> reportFiles = new List<string>();

            string rootPath2 = _env.ContentRootPath + rootPath + foldername;//  HttpContext.Current.Server.MapPath(rootPath + foldername);
            if (!Directory.Exists(rootPath2))
                return reportFiles;
            var directory = Directory.GetFiles(rootPath2).Select(s => s.Remove(0, s.LastIndexOf("\\") + 1).Replace(".rdlc", "")).ToList();
            return directory;
        }

        public  string GetReportPath(string folder, string file)
        {
            if (!file.EndsWith(".rdlc"))
                file += ".rdlc";
            return rootPath + folder + "/" + file;
        }


        public  string GetReportPath(string folder)
        {
            folder = folder.Replace(".", "/");
            if (!folder.EndsWith(".rdlc"))
                folder += ".rdlc";
            return rootPath + folder;
        }

        public  string GetImageUrl(string id, EnumManager.Imagetype type)
        {
            id = type== EnumManager.Imagetype.Logo ? "logo" : id;
            var exist = checkFileExist(id, type);
            if (exist!="204")
                return exist;

            bool converted = int.TryParse(id, out int cid);
            byte[]? imgBytes = null;
            if (type== EnumManager.Imagetype.Logo)
            {
                imgBytes= db.Company.Select(s => s.logo).FirstOrDefault();
            }
           else if (type== EnumManager.Imagetype.Students)
            {
                if (converted)
                {
                    imgBytes = ( db.StudentPhotos.Where(w => w.StudentID==cid).Select(s => s.StudentImage).ToList()).LastOrDefault();
                }

            }
            else if (type== EnumManager.Imagetype.User)
            {

            }
            else if (type== EnumManager.Imagetype.Employee)
            {
                imgBytes = db.tbl_Employee.Where(w => w.Id==cid).Select(s => s.Photo).FirstOrDefault();
            }
            else if (type== EnumManager.Imagetype.Father)
            {
                imgBytes = db.Students.Where(w => w.ID==cid).Select(s => s.FatherPhoto).FirstOrDefault();
            }
            else if (type== EnumManager.Imagetype.FatherSignature)
            {
                imgBytes = db.SignatureSpecimen.Where(w => w.StudentID==cid).Select(s => s.FatherSign).FirstOrDefault();

            }
            else if (type== EnumManager.Imagetype.GaurdianSignature)
            {
                imgBytes = db.SignatureSpecimen.Where(w => w.StudentID==cid).Select(s => s.GaurdianSign).FirstOrDefault();
            }
            else if (type== EnumManager.Imagetype.Gaurdian)
            {

            }
            return GetFile(id, type, imgBytes);
        }

         string checkFileExist(string cid, EnumManager.Imagetype type)
        {
            string folder = type.ToString()+"";
            string picture = string.Empty;
            //string path = HttpContext.Current.Server.MapPath("~/Uploads/images/"+folder);
            string path = _env.WebRootPath + "/Uploads/images/" + folder;//  HttpContext.Current.Server.MapPath(rootPath + foldername);

            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            string file = path+"/"+cid+".jpg";
            if (System.IO.File.Exists(file))
            {
                picture=GetBaseUrl()+ "Uploads/images/"+folder+"/"+cid+".jpg";
                return picture;
            }
            return "204";
        }
        public  string? GetBaseUrl()
        {
            
            var baseUrl = _context.HttpContext?.Request.BaseUrl();// string.Format("{0}://{1}{2}", request.Url.Scheme, request.Url.Authority, appUrl);

            return baseUrl;
        }
         string GetFile(string cid, EnumManager.Imagetype type, byte[]? imgByes = null)
        {
            string folder = type.ToString();
            string picture = GetDefaultStudentPic();
            
           // string path = HttpContext.Current.Server.MapPath("~/Uploads/images/"+folder);
            string path = _env.WebRootPath + "/Uploads/images/" + folder;//  HttpContext.Current.Server.MapPath(rootPath + foldername);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string file = path+cid+".jpg";
            if (!File.Exists(file) && imgByes !=null && imgByes.Length>0)
            {
                FileStream stream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                stream.Write(imgByes, 0, imgByes.Length);
                return GetBaseUrl()+ "Uploads/images/"+folder+"/"+cid+".jpg";
            }
            else return picture;

        }
         string GetDefaultStudentPic()
        {
            return GetBaseUrl()+"Uploads/images/user.png";
        }
        public enum ToastType { error, info }
        public  string JavaScript = "";// "type=\"88fc5bdb31c41a1b4439888a-text/javascript\"";
        public  string QeuedMessage = "Success: Request has been qeued successfully. you will be notified on completion.";
        public  object QeuedReturn(string title, ToastType type, string message, int percent, bool completed)
        {
            return new { title = title, type = type.ToString(), message = message, percent = percent, completed = completed };
        }
        //public  object QeuedReturn(NotificationVm data)
        //{
        //    return new { title = data.NotificationCategory, type = data.NotificationType.ToString(), message = data.Message, percent = 100, completed = true };
        //}

        public  string GetTaskRunning(string MethodName)
        {
            //var jobs = JobStorage.Current.GetMonitoringApi().ProcessingJobs(0, 1000);
            //var exist = jobs.Find(f => f.Value.Job.Method.Name == MethodName);
            //if (exist.Key != null & exist.Value != null)
            //{
            //    return "Already running this task from somewhere. Please wait to complete the task execution.";
            //}
            return string.Empty;
        }

        public  bool SendEmail(string from, string to, string subject, string body)
        {
            SmtpClient smtpClient = new SmtpClient();
            MailMessage msg = new MailMessage(from, to, subject, body);

            msg.IsBodyHtml = true;
            smtpClient.Send(from, to, subject, body);
            return true;
        }
    }
}