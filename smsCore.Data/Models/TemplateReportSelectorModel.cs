using System;

namespace smsCore.Data.Models
{
    /// <summary>
/// View model to draw date picker
/// </summary>
    public class TemplateReportSelector
    {
        public bool AllowDesign { get; set; } = false;
        public string FolderName { get; set; }
        public string ReportName { get; set; }
        public string ParentContainerID { get; set; } = string.Empty;

        public string ColCssClass { get; set; } = "col-12 col-sm-6 col-md-4 col-lg-3";

        public string Label { get; set; } = "Template";


        public string FieldName { get; set; } = "report-template";
        public string FieldId { get; set; } = "report-template";
        public bool IsRequired { get; set; } = false;

    }
}