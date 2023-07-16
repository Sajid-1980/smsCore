using System.Collections.Generic;

namespace smsCore.Data.Models.ViewModels
{
    public class FeeStructureViewModel
    {
        public int ClassFeeGroupId { get; set; }
        public int CampusId { get; set; }
        public int ClassId { get; set; }

        public int FeeGroupId { get; set; }
        public string FeeGroupName { get; set; }

        public List<FeeStructureType> FeeStructureTypes { get; set; }
    }

    public class FeeStructureType
    {
        public int Id { get; set; }
        public int FeeTypeId { get; set; }
        public decimal Amount { get; set; }
    }
}