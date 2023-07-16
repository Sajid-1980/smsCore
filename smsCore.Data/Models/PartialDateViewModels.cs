using System;

namespace smsCore.Data.Models
{
    /// <summary>
/// View model to draw date picker
/// </summary>
    public class PartialDateViewModels
    {
        public string ParentContainerID { get; set; } = string.Empty;

        public string ColCssClass { get; set; } = "col-12 col-sm-6 col-md-4 col-lg-3";

        public string Label { get; set; } = "Date";

        public string Format { get; set; } = "dd/mm/yy";

        public string DateFieldName { get; set; } = "date";
        public string DateFieldId { get; set; } = "date";
        public bool KeepDataFieldReadOnly { get; set; }
        public bool IsRequired { get; set; } = true;
        public string AltFieldName { get; set; } = "date";
        public string AltFieldId { get; set; } = "date";
        public string AltFieldFormat { get; set; } = "dd/mm/yy";

        /// <summary>
        /// date||month||time
        /// </summary>
        public string PickerType { get; set; } = "date";
        public bool ShowAltField { get; set; } = true;
        public DateTime? DefaultDate { get; set; } = null;
        public string DefaultDateString { get; set; }
        public bool SetMask { get; set; } = false;
        public string Mask { get; set; } = "99/99/9999";
    }
}