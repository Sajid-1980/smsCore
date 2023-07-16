//This is a source code or part of Models project
//Copyright (C) 2018  Models
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
namespace Models
{
    public class TransactionsGeneralFillGeneral : DBConnection
    {
        /// <summary>
        /// Function to fill all cash or bank ledgers for combobox financialYearId,display
        /// </summary>
        /// <param name="cmbCashOrBank"></param>
        /// <param name="isAll"></param>
        public IEnumerable<DataRow> FinancialYearComboFill()
        {
            DataTable dtbl = new DataTable();
            SqlDataAdapter sdaadapter = new SqlDataAdapter("FinancialYearComboFill", sqlcon);
            try
            {
                if (sqlcon.State == ConnectionState.Closed)
                {
                    sqlcon.Open();
                }
                sdaadapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                sdaadapter.Fill(dtbl);
            }
            catch 
            {

            }
            finally
            {
                sqlcon.Close();
            }
            return dtbl.AsEnumerable();
        }


        
    }
}
