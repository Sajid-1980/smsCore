using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Configuration;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Models

{
    public partial class DatabaseAccessSql
    {
        protected SqlConnection sqlcon = null;// new SqlConnection("Data Source=.\\SQLEXPRESS;AttachDbFilename=|DataDirectory|\\DataBase\\SchoolSystemModels.mdf;Integrated Security=True;Connect Timeout=30;User Instance=True;MultipleActiveResultSets=True");
        //IConfiguration _configuration;
        //public DatabaseAccessSql()
        //{
        //    sqlcon = new SqlConnection(GetConnectionString());
        //    //sqlcon = new SqlConnection(@"Data Source="+ConnectionSetting.ServerName+";AttachDbFilename="+ConnectionSetting.DBsPath+";Integrated Security=" + ConnectionSetting.IntegratedSecurity + ";uid=" + ConnectionSetting.Username + ";pwd=" + ConnectionSetting.Pwd + ";Connect Timeout=30;User Instance=" + ConnectionSetting.UserInstance + ";");//MultipleActiveResultSets=True;
        //}
        //public DatabaseAccessSql(bool a = true)
        //{
        //    sqlcon = new SqlConnection(GetConnectionString());
        //}
        public DatabaseAccessSql(IConfiguration configuration)
        {
            string ConnectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
            sqlcon = new SqlConnection(ConnectionString);
        }
        public bool VerifyConnection()
        {
            try
            {
                sqlcon.Open();
            }
            catch
            {
                if (sqlcon.State == ConnectionState.Open)
                    sqlcon.Close();
                return false;
            }
            return true;
        }
        /// <summary>
        /// Main function from where the connection string of whole application is build
        /// </summary>
        /// <returns></returns>
        public static string GetConnectionString()
        {
            //try
            //{
            //    string connection = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            //    if (!string.IsNullOrEmpty(connection))
            //    {
            //        return connection;
            //    }
            //}
            //catch { }
            return "";
        }

        public DataTable GetSchema()
        {
            sqlcon.Open();
            DataTable tab = sqlcon.GetSchema("Tables");
            sqlcon.Close();
            return tab;
        }

        public bool Insert(ArrayList query)
        {

            SqlCommand com = sqlcon.CreateCommand();
            SqlTransaction mytrans = null;
            sqlcon.Open();
            mytrans = sqlcon.BeginTransaction();
            com.Connection = sqlcon;
            com.Transaction = mytrans;
            try
            {
                foreach (string q in query)
                {
                    com.CommandText = q;
                    com.ExecuteNonQuery();
                }
                mytrans.Commit();
                sqlcon.Close();
                //message = "Selected Data was added to the DATABASE Successfully";
                return true;
            }
            catch
            {
                try
                {
                    mytrans.Rollback();
                }
                catch 
                { }
                return false;
            }
            finally
            {
                sqlcon.Close();
            }
        }
        public DataTable CreateTable(string query)
        {
            try
            {
                // if (System.Environment.UserInteractive) MessageBox.Show(mycon.ConnectionString);
                sqlcon.Open();
                SqlCommand com = new SqlCommand(query, sqlcon);
                SqlDataAdapter adap = new SqlDataAdapter(com);
                DataSet set = new DataSet();
                //if (System.Environment.UserInteractive) MessageBox.Show(com.CommandText);
                adap.Fill(set);
                DataTable table = new DataTable();
                table = set.Tables[0];
                sqlcon.Close();
                return table;
            }
            catch (Exception ex)
            {
                if (sqlcon.State == ConnectionState.Open)
                    sqlcon.Close();
                throw ex;
            }
        }
        public SqlDataReader CreateReader(string query)
        {
            //SqlDataReader dr;
            try
            {
                sqlcon.Open();
                SqlCommand com = new SqlCommand(query, sqlcon);
                return com.ExecuteReader();
            }
            catch
            {
                if (sqlcon.State == ConnectionState.Open)
                    sqlcon.Close();
                DataTable tab = new DataTable();
                return null;
            }
        }

        public DataTable ExecuteSP(string StoredProcedureName, List<SqlParameter> sqlParameterCollection = null)
        {
            try
            {
                SqlCommand com = new SqlCommand(StoredProcedureName, sqlcon);
                com.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter adap = new SqlDataAdapter(com);
                sqlcon.Open();
                if (sqlParameterCollection != null)
                {
                    foreach (var parameters in sqlParameterCollection)
                    {
                        com.Parameters.Add(parameters);
                    }
                }
                DataSet set = new DataSet();
                adap.Fill(set);
                DataTable table = new DataTable();
                table = set.Tables[0];
                return table;
            }
            catch//(Exception ex)
            {
                if (sqlcon.State == ConnectionState.Open)
                    sqlcon.Close();
                DataTable tab = new DataTable();
                return tab;
            }
            finally
            {
                sqlcon.Close();
            }
        }
        public ArrayList AddtoCombo(DataTable table, int clm)
        {
            ArrayList schname = new ArrayList();
            schname.Clear();
            for (int i = 0; i < table.Rows.Count; i++)
            {
                schname.Add(table.Rows[i][clm]);
            }
            schname.Sort();
            return schname;

        }
        public ArrayList AddtoCombo(DataTable table, string clm)
        {
            ArrayList schname = new ArrayList();
            schname.Clear();
            for (int i = 0; i < table.Rows.Count; i++)
            {
                schname.Add(table.Rows[i][clm]);
            }
            schname.Sort();
            return schname;

        }
        public ArrayList AddtoCombo(DataTable table)
        {
            ArrayList schname = new ArrayList();
            schname.Clear();
            for (int i = 0; i < table.Rows.Count; i++)
            {
                schname.Add(table.Rows[i][0]);
            }
            return schname;

        }
        public bool Insert(string query)
        {
            SqlCommand com = sqlcon.CreateCommand();
            SqlTransaction mytrans = null;
            sqlcon.Open();
            mytrans = sqlcon.BeginTransaction();
            com.Connection = sqlcon;
            com.Transaction = mytrans;
            try
            {
                //if (System.Environment.UserInteractive) MessageBox.Show(query);
                com.CommandText = query;
                int effected = com.ExecuteNonQuery();
                mytrans.Commit();
                sqlcon.Close();
                //message = "yes";
                if (effected > 0)
                    return true;
                else return false;
            }
            catch (Exception ex)
            {
                try
                {
                    mytrans.Rollback();
                }
                catch
                { }
                return false;
            }
            finally
            {
                if (sqlcon.State == ConnectionState.Open)
                    sqlcon.Close();
            }
        }
        /// <summary>
        /// string q = "select " + columnname + " from " + tablename + " where " + comparecolumn + "='" + value + "'";
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="columnname"></param>
        /// <param name="value"></param>
        /// <param name="comparecolumn"></param>
        /// <returns></returns>
        public int GetId(string tablename, string columnname, string value, string comparecolumn, bool isInt)
        {
            int id = 0;
            try
            {
                string q = "";
                if (isInt)
                    q = "select " + columnname + " from " + tablename + " where " + comparecolumn + "=" + value + "";
                else
                    q = "select " + columnname + " from " + tablename + " where " + comparecolumn + "='" + value + "'";
                DataTable tab = CreateTable(q);
                id = Convert.ToInt16(tab.Rows[0][0]);
            }
            catch
            {
            }
            return id;
        }
        /// <summary>
        ///  string q = "select "+column+" from " + tablenam + " order by "+column+"";
        /// </summary>
        /// <param name="tablenam"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public int GetMaximumId(string column, string tablenam)
        {
            int maxid = 0;
            try
            {
                string q = "select " + column + " from " + tablenam + " order by " + column + "";
                DataTable tab = new DataTable();
                tab = CreateTable(q);
                if (tab.Rows.Count > 0)
                {
                    int rowno = tab.Rows.Count - 1;
                    maxid = Convert.ToInt32(tab.Rows[rowno][0]);
                }
            }
            catch { }
            return maxid + 1;
        }



        /// <summary>
        /// Delte data from 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="id"></param>

        public void delete(string tableName, int id)
        {
            try
            {
                sqlcon.Open();
                SqlCommand com = new SqlCommand();
                //DELETE FROM dbo.StudentComplaigns where StudentComplaigns .ID=3
                string qry = "DELETE FROM dbo." + tableName + " WHERE (((" + tableName + ".ID)=" + id + "))";
                com = new SqlCommand(qry, sqlcon);
                com.ExecuteNonQuery();
                sqlcon.Close();

            }
            catch (Exception a)
            {
                if (sqlcon.State == ConnectionState.Open)
                    sqlcon.Close();
                else throw a;
            }
        }


        /// <summary>
        /// DataTable tab = createtable("Select " + column + " from " + tablename + "");
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public ArrayList GetSingleColumn(string tablename, string column)
        {
            ArrayList columnValues = new ArrayList();
            try
            {
                DataTable tab = CreateTable("Select " + column + " from " + tablename + "");
                columnValues = AddtoCombo(tab, 0);
                return columnValues;

            }
            catch
            {

                return columnValues;

            }
        }



        public string GetSingleValue(string Query)
        {
            //string q = "Select GroupClassId From GroupClass WHERE ClassId=" + ClassId + " and GroupId=" + GroupId + "";
            DataTable tab = CreateTable(Query);
            //if (System.Environment.UserInteractive) MessageBox.Show(Query);
            string Id = "0";
            try
            {
                Id = tab.Rows[0][0].ToString();
            }
            catch { }
            return Id;
        }


        public DataTable SetEntryDate(DataTable dtable, string OldDateColumn, string NewDateColumn, bool RemoveOld)
        {
            DateTime dt = DateTime.Now;
            dtable.Columns.Add(NewDateColumn);
            foreach (DataRow ro in dtable.Rows)
            {
                try
                {
                    dt = DateTime.Parse(ro[OldDateColumn].ToString());
                }
                catch { }
                ro[NewDateColumn] = dt.ToString("dd/MM/yy");
            }
            if (RemoveOld)
                dtable.Columns.Remove(OldDateColumn);
            return dtable;
        }

        public string GetYear()
        {
            string CYear = "";
            CYear = DateTime.Now.Year.ToString();
            return CYear;
        }



        //public ParameterFields GetTitle(bool contact)
        //{
        //    ParameterFields pfds = new ParameterFields();
        //    ParameterField InvAdress = new ParameterField();
        //    ParameterDiscreteValue Add = new ParameterDiscreteValue();
        //    InvAdress.Name = "Address";
        //    Add.Value =PublicVariables.CompanyInformation.Address;
        //    InvAdress.CurrentValues.Add(Add);
        //    pfds.Add(InvAdress);

        //    ParameterField InvTitle = new ParameterField();
        //    ParameterDiscreteValue Tit = new ParameterDiscreteValue();
        //    InvTitle.Name = "FullName";
        //    Tit.Value = PublicVariables.CompanyInformation.CompanyName;
        //    InvTitle.CurrentValues.Add(Tit);
        //    pfds.Add(InvTitle);

        //    ParameterField Invconta = new ParameterField();
        //    ParameterDiscreteValue Cont = new ParameterDiscreteValue();
        //    Invconta.Name = "Contact";
        //    Cont.Value = "Cont: " + PublicVariables.CompanyInformation.Phone + " , " + PublicVariables.CompanyInformation.Mobile;
        //    Invconta.CurrentValues.Add(Cont);
        //    pfds.Add(Invconta);

        //    return pfds;
        //}

        //public ParameterFields GetTitle()
        //{
        //    ParameterFields pfds = new ParameterFields();
        //    ParameterField InvAdress = new ParameterField();
        //    ParameterDiscreteValue Add = new ParameterDiscreteValue();
        //    InvAdress.Name = "Address";
        //    Add.Value = PublicVariables.CompanyInformation.Address;
        //    InvAdress.CurrentValues.Add(Add);
        //    pfds.Add(InvAdress);

        //    ParameterField InvTitle = new ParameterField();
        //    ParameterDiscreteValue Tit = new ParameterDiscreteValue();
        //    InvTitle.Name = "FullName";
        //    Tit.Value = PublicVariables.CompanyInformation.CompanyName;
        //    InvTitle.CurrentValues.Add(Tit);
        //    pfds.Add(InvTitle);

        //    return pfds;
        //}

    }
}
