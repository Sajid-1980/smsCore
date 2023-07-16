//This is a source code or part of Models project
//Copyright (C) 2018  Models
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Models
{
    public class TransactionsGeneralFill : DBConnection
    {

        private readonly SchoolEntities db;// = new SchoolEntities(null);

        public TransactionsGeneralFill(SchoolEntities _db) { db = _db; }

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
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                sqlcon.Close();
            }
            return dtbl.AsEnumerable();
        }



        
        /// <summary>
        /// Function to get next VoucherNo for voucher on Automatic generation 
        /// </summary>
        /// <param name="VoucherTypeId"></param>
        /// <param name="txtBox"></param>
        /// <param name="date"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public string VoucherNumberAutomaicGeneration(decimal VoucherTypeId, decimal txtBox, DateTime date, string tableName)
        {
            string strVoucherNo = string.Empty;
            try
            {
                if (sqlcon.State == ConnectionState.Closed)
                {
                    sqlcon.Open();
                }
                SqlCommand sccmd = new SqlCommand("VoucherNumberAutomaicGeneration", sqlcon);
                sccmd.CommandType = CommandType.StoredProcedure;
                SqlParameter sprmparam = new SqlParameter();
                sprmparam = sccmd.Parameters.Add("@voucherTypeId", SqlDbType.Decimal);
                sprmparam.Value = VoucherTypeId;
                sprmparam = sccmd.Parameters.Add("@txtBox", SqlDbType.Decimal);
                sprmparam.Value = txtBox;
                sprmparam = sccmd.Parameters.Add("@date", SqlDbType.DateTime);
                sprmparam.Value = date;
                sprmparam = sccmd.Parameters.Add("@tab_name", SqlDbType.VarChar);
                sprmparam.Value = tableName;
                object obj = sccmd.ExecuteScalar();
                if (obj != null)
                {
                    strVoucherNo = obj.ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                sqlcon.Close();
            }
            return strVoucherNo;
        }
        /// <summary>
        /// Function to fill all Currencies created for combobx
        /// </summary>
        /// <returns></returns>
        public DataTable CurrencyComboFill()
        {
            DataTable dtbl = new DataTable();
            try
            {
                if (sqlcon.State == ConnectionState.Closed)
                {
                    sqlcon.Open();
                }
                SqlDataAdapter sdaadapter = new SqlDataAdapter("CurrencyComboFill", sqlcon);
                sdaadapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                sdaadapter.Fill(dtbl);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                sqlcon.Close();
            }
            return dtbl;
        }

        /// <summary>
        /// Function to fill all AccountLedgers under Expenses forcombox
        /// </summary>
        /// <param name="cmbType"></param>
        /// <param name="isAll"></param>
        /// <returns></returns>
        public DataTable AccountLedgerUnderExpenses(bool isAll)
        {
            DataTable dtbl = new DataTable();
            try
            {
                var expenseId = (int)PublicVariables.EnumAccountGroup.Indirect_Expenses;
                db.AccountLedgers.Where(w => w.AccountGroupId == expenseId).AsNoTracking().Select(s => new {
                s.Id, s.LedgerName
                });

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                sqlcon.Close();
            }
            return dtbl;
        }
        /// <summary>
        /// Function to fill AccountLedgers for combobox
        /// </summary>
        /// <returns></returns>
        public DataTable AccountLedgerComboFill()
        {
            DataTable dtbl = new DataTable();
            try
            {
                //AccountLedgerSP spaccountledger = new AccountLedgerSP();
                //dtbl = spaccountledger.AccountLedgerViewAll();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dtbl;
        }
        /// <summary>
        /// Function to fill All cash/Bank ledgers for Combobox
        /// </summary>
        /// <param name="isAll"></param>
        /// <returns></returns>
        public DataTable BankOrCashComboFill(bool isAll)
        {
            DataTable dtbl = new DataTable();
            SqlDataAdapter sdaadapter = new SqlDataAdapter("CashOrBankComboFill", sqlcon);
            try
            {
                if (sqlcon.State == ConnectionState.Closed)
                {
                    sqlcon.Open();
                }
                sdaadapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                sdaadapter.Fill(dtbl);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                sqlcon.Close();
            }
            return dtbl;
        }
        /// <summary>
        /// Function to check the status of PrintAfterSave and returns the status
        /// </summary>
        /// <returns></returns>
        public bool StatusOfPrintAfterSave()
        {
            string strStatus = "";
            bool isTrue = false;
            try
            {
                if (sqlcon.State == ConnectionState.Closed)
                {
                    sqlcon.Open();
                }
                SqlCommand sccmd = new SqlCommand("PrintAfterSave", sqlcon);
                sccmd.CommandType = CommandType.StoredProcedure;
                strStatus = sccmd.ExecuteScalar().ToString();
                if (strStatus == "Yes")
                {
                    isTrue = true;
                }
                else
                {
                    isTrue = false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                sqlcon.Close();
            }
            return isTrue;
        }
        /// <summary>
        /// Function to check the status of Tax and return the status
        /// </summary>
        /// <returns></returns>
        public bool TaxStatus()
        {
            string strStatus = "";
            bool isTrue = false;
            try
            {
                if (sqlcon.State == ConnectionState.Closed)
                {
                    sqlcon.Open();
                }
                SqlCommand sccmd = new SqlCommand("TaxStatus", sqlcon);
                sccmd.CommandType = CommandType.StoredProcedure;
                strStatus = sccmd.ExecuteScalar().ToString();
                if (strStatus == "Yes")
                {
                    isTrue = true;
                }
                else
                {
                    isTrue = false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                sqlcon.Close();
            }
            return isTrue;
        }
        /// <summary>
        /// Function to check the status of Godown and return the status
        /// </summary>
        /// <returns></returns>
        public bool GodownStatus()
        {
            string strStatus = "";
            bool isTrue = false;
            try
            {
                if (sqlcon.State == ConnectionState.Closed)
                {
                    sqlcon.Open();
                }
                SqlCommand sccmd = new SqlCommand("GodownStatus", sqlcon);
                sccmd.CommandType = CommandType.StoredProcedure;
                strStatus = sccmd.ExecuteScalar().ToString();
                if (strStatus == "Yes")
                {
                    isTrue = true;
                }
                else
                {
                    isTrue = false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                sqlcon.Close();
            }
            return isTrue;
        }
        /// <summary>
        /// Function to fill All Currencies created On Each Date
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public DataTable CurrencyComboByDate(DateTime date)
        {
            DataTable dtbl = new DataTable();
            try
            {
                if (sqlcon.State == ConnectionState.Closed)
                {
                    sqlcon.Open();
                }
                SqlDataAdapter sdaadapter = new SqlDataAdapter("CurrencyComboByDate", sqlcon);
                sdaadapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                SqlParameter sqlparameter = new SqlParameter();
                sqlparameter = sdaadapter.SelectCommand.Parameters.Add("@date", SqlDbType.DateTime);
                sqlparameter.Value = date;
                sdaadapter.Fill(dtbl);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                sqlcon.Close();
            }
            return dtbl;
        }
        /// <summary>
        /// Function to fill all Bank ledgers for combobox
        /// </summary>
        /// <returns></returns>
        public DataTable BankComboFill()
        {
            DataTable dtblAccount = new DataTable();
            try
            {
                if (sqlcon.State == ConnectionState.Closed)
                {
                    sqlcon.Open();
                }
                SqlDataAdapter sqlda = new SqlDataAdapter("BankAccountComboFill", sqlcon);
                sqlda.SelectCommand.CommandType = CommandType.StoredProcedure;
                sqlda.Fill(dtblAccount);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                sqlcon.Close();
            }
            return dtblAccount;
        }
    }
}
