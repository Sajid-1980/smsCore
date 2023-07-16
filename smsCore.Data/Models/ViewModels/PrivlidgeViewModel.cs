using System;

namespace smsCore.Data.Models.ViewModels
{
    public class PrivlidgeViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayText { get; set; }
        public string _Type { get; set; }
        public bool Allow { get; set; }
        public int ParentId { get; set; }
    }
}
