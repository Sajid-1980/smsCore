using System.Collections.Generic;

namespace smsCore.Data.Models.ViewModels
{
    public class PayStructureViewModel
    {
        public int PayGroupId { get; set; }
        public int CampusId { get; set; }
        public int EmployeeId { get; set; }

        public string PayStructureName { get; set; }

        public List<PayStructureType> PayStructureTypes { get; set; }
    }

    public class PayStructureType
    {
        public int Id { get; set; }
        public int PayHeadId { get; set; }
        public decimal Amount { get; set; }
    }
}