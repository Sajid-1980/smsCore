using BoldReports.Web;
using BoldReports.Web.ReportViewer;
using log4net.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace sms.Controllers.BoldReports
{
    [RoutePrefix("")]
    public class ReportViewerController : ApiController,IReportController
    {
        private string resourceRootLoc = "~/Content/Reports/";
        [Route("api/ReportViewer/GetResource")]

        public object GetResource(string key, string resourcetype, bool isPrint)
        {
            return ReportHelper.GetResource(key, resourcetype, isPrint);
        }

        //public void LogError(string message, Exception exception, MethodBase methodType, ErrorType errorType)
        //{
        //    ILogExtensions.LogError(message, exception, methodType, errorType == ErrorType.Error ? "Error" : "Info");
        //}

        //public void LogError(string errorCode, string message, Exception exception, string errorDetail, string methodName, string className)
        //{
        //    LogExtension.LogError(message, exception, System.Reflection.MethodBase.GetCurrentMethod(), errorCode + "-" + errorDetail);
        //}

        [Route("api/ReportViewer/OnInitReportOptions")]

        public void OnInitReportOptions(ReportViewerOptions reportOption)
        {
            reportOption.ReportModel.EmbedImageData = true;
            string reportName = reportOption.ReportModel.ReportPath;
            string directoryName = Path.GetDirectoryName(reportName);
            if (directoryName.Length <= 0)
            {
                reportOption.ReportModel.ReportPath = HttpContext.Current.Server.MapPath(resourceRootLoc + reportName + ".rdlc");
            }
            if (reportName == "load-large-data.rdlc")
            {
                //SqlQuery.getJson();
                reportOption.ReportModel.DataSources.Add(new ReportDataSource("SalesOrderDetail", HttpContext.Current.Cache.Get("SalesOrderDetail") as DataTable));
            }

           //var param = reportOption.ReportModel.Parameters;
           // if (param.Where(w => w.Name == "").Any())
           // {

           // }

        }

        [Route("api/ReportViewer/OnReportLoaded")]
        public void OnReportLoaded(ReportViewerOptions reportOption)
        {

        }

        [CustomCompression]
        [HttpPost]
        [Route("api/ReportViewer/PostReportAction")]
        public object PostReportAction(Dictionary<string, object> jsonResult)
        {
            bool isclearcache = false;
            if (jsonResult != null && jsonResult.ContainsKey("reportAction") && jsonResult["reportAction"].ToString() == "ClearCache")
            {
                isclearcache = true;
            }
            //var reportresult = ReportHelper.ProcessReport(jsonArray, this, this._cache);
            if (isclearcache)
            {
                GC.Collect(); 
                //isclearcache = false;
            }

            //return reportresult;
            return ReportHelper.ProcessReport(jsonResult, this);
        }
    }
}