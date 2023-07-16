using Models;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Utilities
{
    public enum FeeLogicIDTypes { feeslipId = 1, admissionId = 2, none = 0 }
    public class FeeLogics
    {
        SchoolEntities _db;
        DatabaseAccessSql _dba;
        public FeeLogics(SchoolEntities db, DatabaseAccessSql dba)
        {
            _db = db;
            _dba = dba;
        }


        public DataTable Construct(int[] IDs, bool? received, bool? isexpell, FeeLogicIDTypes logicIDType, string otherCondition = "")
        {
        int[] ids = new int[] { };
        string condition = string.Empty;
        string having = string.Empty;
            ids = IDs;
            if (logicIDType == FeeLogicIDTypes.feeslipId)
            {
                condition = @" dbo.FeeSlips.Id In (" + string.Join(",", IDs) + ") ";
            }
            else if (logicIDType == FeeLogicIDTypes.admissionId)
            {
                condition = @" dbo.Admissions.Id In (" + string.Join(",", IDs) + ") ";
            }

            if (received.HasValue && received.Value)
            {
                condition += " AND dbo.GetTotalReceivedFeeAmount(dbo.Admissions.ID, MONTH(dbo.FeeSlips.ForMonth), YEAR(dbo.FeeSlips.ForMonth)) > 0 ";
            }
            else if (!received.HasValue || !received.Value)
            {
               // condition += " AND (dbo.GetTotalFeeAmount(dbo.Admissions.ID, MONTH(dbo.FeeSlips.ForMonth), YEAR(dbo.FeeSlips.ForMonth)) - dbo.GetTotalReceivedFeeAmount(dbo.Admissions.ID, MONTH(dbo.FeeSlips.ForMonth), YEAR(dbo.FeeSlips.ForMonth)) <> 0) ";
                // having = ",dbo.GetTotalFeeAmount(dbo.Admissions.ID, MONTH(dbo.FeeSlips.ForMonth), YEAR(dbo.FeeSlips.ForMonth)),SUM(dbo.FeeSlipReceipts.Amount)";
                having = @" HAVING (dbo.GetTotalFeeAmount(dbo.Admissions.ID, MONTH(dbo.FeeSlips.ForMonth), YEAR(dbo.FeeSlips.ForMonth)) - SUM(dbo.FeeSlipReceipts.Amount) IS NULL) OR
                (dbo.GetTotalFeeAmount(dbo.Admissions.ID, MONTH(dbo.FeeSlips.ForMonth), YEAR(dbo.FeeSlips.ForMonth)) - SUM(dbo.FeeSlipReceipts.Amount) <> 0) ";
            }

            if (isexpell.HasValue)
            {
                if (isexpell.Value)
                {
                    condition += " AND (dbo.Admissions.IsExpell = 1) ";
                }
                else
                {
                    condition += " AND (dbo.Admissions.IsExpell = 0) ";
                }
            }
            condition += otherCondition;
            DataTable dataTable = new DataTable();

            var feetypeNames = this.pivotColumns();

            var pivot = "[" + string.Join(",", feetypeNames).Replace(",", "],[") + "]";
            SqlParameter pivotColumns = new SqlParameter("pivotCols", pivot);
            SqlParameter receivedCond = new SqlParameter("receivedCondition", condition);
            SqlParameter havingCond = new SqlParameter("having", having);
            List<SqlParameter> collection = new List<SqlParameter>();
            collection.Add(pivotColumns);
            collection.Add(receivedCond);
            collection.Add(havingCond);
            dataTable = _dba.ExecuteSP("[dbo].[StudentFeeDueStatement]", collection);
            return dataTable;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="admIds"></param>
        /// <returns>FeeSlip iIDs of outstanding fee</returns>
        public int[] GetOutStanding(int[] admIds)
        {
            var data = _db.FeeSlips.Where(w => admIds.Contains(w.AdmissionId) && (w.FeeSlipReceipts.Count==0 || w.FeeSlipReceipts.DefaultIfEmpty().Select(s => s.Amount).Sum() < w.FeeSlipDetails.Select(s => s.Amount).Sum())).Select(s => s.Id);
            return data.ToArray();
        }

        public int[] GetOutStandingOfStudents(int[] stdIds)
        {
            var data = _db.FeeSlips.Where(w => stdIds.Contains(w.Admission.StudentID) && (w.FeeSlipReceipts.Count==0 || w.FeeSlipReceipts.DefaultIfEmpty().Select(s => s.Amount).Sum() < w.FeeSlipDetails.Select(s => s.Amount).Sum())).Select(s => s.Id);
            return data.ToArray();
        }


        public object GetOutStanding(int admIds)
        {
            var data = _db.FeeSlips.Where(w => w.AdmissionId == admIds && (w.FeeSlipReceipts.Count == 0 || w.FeeSlipReceipts.DefaultIfEmpty().Select(s => s.Amount).Sum() < w.FeeSlipDetails.Select(s => s.Amount).Sum())).Select(s => new { s.Id,s.ForMonth, Amount = s.FeeSlipDetails.Sum(aa => aa.Amount) - s.FeeSlipReceipts.DefaultIfEmpty().Sum(aa => aa.Amount) }).ToList();

            return data.ToArray();
        }

        

        public string[] pivotColumns()
        {
            var feetypeNames = _db.FeeTypes.OrderBy(o => o.SortOrder).ToList().Select(w => w.TypeName.Trim()).ToArray();

            return feetypeNames;
        }
    }
}
