using BoldReports.Web;
using BoldReports.Web.ReportDesigner;
using BoldReports.Web.ReportViewer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Http;

namespace sms.Controllers.BoldReports
{
    [Route("api/[controller]/[action]")]
    public class ReportDesignerController : ApiController, IReportDesignerController,  IReportHelperSettings
    {
        const string CachePath = "Cache\\";

        internal ReportHelperSettings _helperSettings = null;

        internal ExternalServer Server
        {
            get;
            set;
        }

        internal string ServerURL
        {
            get;
            set;
        }

        internal string AuthorizationHeaderValue
        {
            get;
            set;
        }

        internal ReportHelperSettings HelperSettings
        {
            get { return this._helperSettings; }
            set { this._helperSettings = value; }
        }
        public ReportDesignerController()
        {
            ExternalServer externalServer = new ExternalServer();
            this.Server = externalServer;
            this.ServerURL = "Sample";
            externalServer.ReportServerUrl = this.ServerURL;
        }
        [Route("api/ReportDesigner/InitializeSettings")]
        [HttpGet]
        public void InitializeSettings(ReportHelperSettings helperSettings)
        {
            helperSettings.ReportingServer = Server;
            HelperSettings = helperSettings;
        }

        [Route("api/ReportDesigner/UploadReportAction")]

        [HttpPost]
        public void UploadReportAction()
        {
            ReportDesignerHelper.ProcessDesigner(null, this, HttpContext.Current.Request.Files[0]);
        }

        [HttpGet]
        [Route("api/ReportDesigner/GetImage")]

        public object GetImage(string key, string image)
        {
            return ReportDesignerHelper.GetImage(key, image, this);
        }

        [Route("api/ReportDesigner/DisposeObjects")]

        [HttpPost]
        public bool DisposeObjects()
        {
            try
            {
                string targetFolder = HttpContext.Current.Server.MapPath("~/");
                targetFolder += "Cache";

                if (Directory.Exists(targetFolder))
                {
                    string[] dirs = Directory.GetDirectories(targetFolder);

                    for (var index = 0; index < dirs.Length; index++)
                    {
                        string[] files = Directory.GetFiles(dirs[index]);

                        var fileCount = 0;
                        for (var fileIndex = 0; fileIndex < files.Length; fileIndex++)
                        {
                            FileInfo fi = new FileInfo(files[fileIndex]);
                            if (fi.LastAccessTimeUtc < DateTime.UtcNow.AddDays(-2))
                            {
                                fileCount++;
                            }
                        }

                        if (files.Length == 0 || (files.Length == fileCount))
                        {
                            Directory.Delete(dirs[index], true);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                //LogExtension.LogError(ex.Message, ex, MethodBase.GetCurrentMethod());
            }
            return false;
        }


        [HttpPost]
        [CustomCompression]
        [Route("api/ReportDesigner/PostDesignerAction")]

        public object PostDesignerAction(Dictionary<string, object> jsonResult)
        {
            this.UpdateReportType(jsonResult);
            return ReportDesignerHelper.ProcessDesigner(jsonResult, this, null);
        }
        [HttpPost]
        [Route("api/ReportDesigner/PostReportAction")]
        [CustomCompression]
        public object PostReportAction(Dictionary<string, object> jsonResult)
        {
            this.UpdateReportType(jsonResult);
            return ReportHelper.ProcessReport(jsonResult, this as IReportController);
        }

        [Route("api/ReportDesigner/OnInitReportOptions")]
        [HttpGet]
        public void OnInitReportOptions(ReportViewerOptions reportOption)
        {
            string reportName = reportOption.ReportModel.ReportPath;
            reportOption.ReportModel.ReportingServer = this.Server;
            reportOption.ReportModel.ReportServerUrl = this.ServerURL;
            reportOption.ReportModel.ReportServerCredential = new NetworkCredential("Sample", "Passwprd");
            if (reportName == "load-large-data.rdlc")
            {
                //SqlQuery.getJson();
                reportOption.ReportModel.ProcessingMode = ProcessingMode.Remote;
                reportOption.ReportModel.DataSources.Add(new ReportDataSource("SalesOrderDetail", HttpContext.Current.Cache.Get("SalesOrderDetail") as DataTable));
            }

        }

        [Route("api/ReportDesigner/OnReportLoaded")]
        [HttpGet]
        public void OnReportLoaded(ReportViewerOptions reportOption)
        {
        }

        [Route("api/ReportDesigner/GetResource")]
        [HttpGet]
        public object GetResource(string key, string resourcetype, bool isPrint)
        {
            return ReportHelper.GetResource(key, resourcetype, isPrint);
        }
        [NonAction]
        private string GetFilePath(string itemName, string key)
        {
            string targetFolder = HttpContext.Current.Server.MapPath("~/");
            targetFolder += "Cache";

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            if (!Directory.Exists(targetFolder + "\\" + key))
            {
                Directory.CreateDirectory(targetFolder + "\\" + key);
            }

            return targetFolder + "\\" + key + "\\" + itemName;
        }
        [Route("api/ReportDesigner/SetData")]
        [HttpGet]
        public bool SetData(string key, string itemId, ItemInfo itemData, out string errMsg)
        {
            errMsg = string.Empty;
            try
            {
                if (itemData.Data != null)
                {
                    File.WriteAllBytes(this.GetFilePath(itemId, key), itemData.Data);
                }
                else if (itemData.PostedFile != null)
                {
                    var fileName = itemId;
                    if (string.IsNullOrEmpty(itemId))
                    {
                        fileName = Path.GetFileName(itemData.PostedFile.FileName);
                    }
                    itemData.PostedFile.SaveAs(this.GetFilePath(fileName, key));
                }
            }
            catch (Exception ex)
            {
                //LogExtension.LogError(ex.Message, ex, MethodBase.GetCurrentMethod());
                errMsg = ex.Message;
                return false;
            }
            return true;
        }


        [Route("api/ReportDesigner/GetData")]
        [HttpGet]
        public ResourceInfo GetData(string key, string itemId)
        {
            var resource = new ResourceInfo();
            try
            {
                resource.Data = File.ReadAllBytes(this.GetFilePath(itemId, key));
            }
            catch (Exception ex)
            {
                //LogExtension.LogError(ex.Message, ex, MethodBase.GetCurrentMethod());
                resource.ErrorMessage = ex.Message;
            }
            return resource;
        }


        //[Route("api/ReportDesigner/LogError")]
        //[HttpGet]
        //public void LogError(string message, Exception exception, MethodBase methodType, ErrorType errorType)
        //{
        //    //LogExtension.LogError(message, exception, methodType, errorType == ErrorType.Error ? "Error" : "Info");
        //}

        [Route("api/ReportDesigner/LogError")]
        [HttpGet]
        public void LogError(string errorCode, string message, Exception exception, string errorDetail, string methodName, string className)
        {
            //LogExtension.LogError(message, exception, System.Reflection.MethodBase.GetCurrentMethod(), errorCode + "-" + errorDetail);
        }


        [Route("api/ReportDesigner/UpdateReportType")]
        [HttpGet]
        public void UpdateReportType(Dictionary<string, object> jsonResult)
        {
            string reportType = "";

            if (jsonResult.ContainsKey("customData"))
            {
                string customData = jsonResult["customData"].ToString();
                reportType = (string)(JsonConvert.DeserializeObject(customData) as dynamic).reportType;
            }
            else if (!string.IsNullOrEmpty(HttpContext.Current.Request.Form["customData"]))
            {
                string customData = JsonConvert.DeserializeObject(HttpContext.Current.Request.Form["customData"]).ToString();
                reportType = (JsonConvert.DeserializeObject(customData) as dynamic).reportType;
            }
            this.Server.reportType = String.IsNullOrEmpty(reportType) ? "RDL" : reportType;
        }
    }
}