//This is a source code or part of Models project
//Copyright (C) 2018  Models
using System;
using System.Configuration;
using System.Data.SqlClient;

//<summary>    
//Summary description for DBConnection    
//</summary>    
namespace Models
{
    public class DBConnection
    {
        protected SqlConnection sqlcon;
        public DBConnection()
        {
            SqlConnection.ClearAllPools();
            sqlcon = new SqlConnection(SClass.ConnectionString());// @"Data Source=" + ConnectionSetting.ServerName + ";AttachDbFilename=" + ConnectionSetting.DBsPath + ";Integrated Security=" + ConnectionSetting.IntegratedSecurity + ";uid=" + ConnectionSetting.Username + ";pwd=" + ConnectionSetting.Pwd + ";Connect Timeout=300;User Instance=" + ConnectionSetting.UserInstance + ";");
            try
            {
                sqlcon.Open();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}