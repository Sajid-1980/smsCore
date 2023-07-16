using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Models;
using System.Linq;

namespace smsCore.Data.Helpers
{
   public class ClsBussinessSetting
    {
        DatabaseAccessSql dbs;
        public ClsBussinessSetting(DatabaseAccessSql _dbs) { dbs = _dbs; }
        
        private int _campusId;
        public int CampusId { get { return _campusId; } set { _campusId = value; } }

        public long ID { get; set; }
        public string PropertyName { get; set; } = "";
        public string PropertyValue { get; set; } = "";
        public ClsBussinessSetting(int campusId, DatabaseAccessSql _dbs)
        {
            _campusId = campusId;
            dbs = _dbs;
        }
        public ClsBussinessSetting Read(string _PropertyName)
        {
            string 
                q = "Select * from SMSApplicationProperty Where PropertyName='" + _PropertyName + "' AND CampusID = " + CampusId + "";
            

            DataTable tab = dbs.CreateTable(q);
            if (tab.Rows.Count > 0)
            {
                this.ID = int.Parse(tab.Rows[0][0].ToString());
                this.PropertyName = tab.Rows[0]["PropertyName"].ToString().Trim();
                this.PropertyValue = tab.Rows[0]["PropertyValue"].ToString().Trim();
            }
            else return null;
            return this;
        }
        public ClsBussinessSetting Read(string _PropertyName, bool skipcampus)
        {
            string 
                q = "Select * from SMSApplicationProperty Where PropertyName='" + _PropertyName + "'";
            
            DataTable tab = dbs.CreateTable(q);
            if (tab.Rows.Count > 0)
            {
                this.ID = int.Parse(tab.Rows[0][0].ToString());
                this.PropertyName = tab.Rows[0]["PropertyName"].ToString().Trim();
                this.PropertyValue = tab.Rows[0]["PropertyValue"].ToString().Trim();
            }
            else return null;
            return this;
        }
        public object ReadWithType(string _PropertyName,Type t)
        {

            string     q = "Select * from SMSApplicationProperty Where PropertyName='" + _PropertyName + "' AND CampusID = " + CampusId + "";
            
            DataTable tab = dbs.CreateTable(q);

            if (tab.Rows.Count > 0)
            {
                try
                {
                    return Convert.ChangeType(tab.Rows[0]["PropertyValue"].ToString().Trim(), t);
                }
                catch
                {
                    if (t.Name == "DateTime")
                        return DateTime.Now;
                    else if (t.Name == "Boolean")
                        return false;
                    else
                        return Convert.ChangeType("0", t);
                }
            }
            else return t.Name == "Boolean" ? false : (t.Name=="DateTime" ? DateTime.Now : Convert.ChangeType("0", t));
        }

        private bool Write()
        {
            string q = "Insert into SMSApplicationProperty(PropertyValue,PropertyName,CampusID) values('" + PropertyValue + "','" + PropertyName + "'," + CampusId + ")";
            return dbs.Insert(q);
        }

        public bool WriteorUpdate(string _propertyName, string _propertyValue)
        {
            var setting = Read(_propertyName);
            if (setting == null)
            {
                setting = new ClsBussinessSetting(CampusId,dbs);
                setting.PropertyName = _propertyName;
                setting.PropertyValue = _propertyValue;
                setting._campusId = CampusId;
                return setting.Write();
            }
            else
            {
                setting.PropertyValue = _propertyValue;
                return setting.Update(setting);
            }
        }
        public bool Update(ClsBussinessSetting c)
        {
            string q = "Update SMSApplicationProperty SET PropertyValue='" + PropertyValue + "' WHERE ID=" + c.ID + "";
            //System.Windows.Forms.MessageBox.Show(q);
            return dbs.Insert(q);
        }
        public List<ClsBussinessSetting> ReadAll()
        {
            List<ClsBussinessSetting> all = new List<ClsBussinessSetting>();
            string q = "Select * from SMSApplicationProperty Where CampusID = " + CampusId + "";

            DataTable tab = dbs.CreateTable(q);
            if (tab.Rows.Count > 0)
            {
                all = tab.AsEnumerable().Select(dr => new ClsBussinessSetting(CampusId,dbs) { PropertyName = dr["PropertyName"].ToString(), PropertyValue = dr["PropertyValue"].ToString() }).ToList();
                //foreach (DataRow dr in tab.Rows)
                //{
                //    ClsBussinessSetting cb = new ClsBussinessSetting();
                //    cb.ID = int.Parse(dr[0].ToString());
                //    cb.PropertyName = dr["PropertyName"].ToString();
                //    cb.PropertyValue = dr["PropertyValue"].ToString();
                //    bool done = int.TryParse(tab.Rows[0]["CampusID"].ToString(), out int cmpid);
                //    cb._campusId = !done ? -1 : cmpid;
                //    all.Add(cb);
                //}
            }
            else return null;
            return all;
        }

        public override string ToString()
        {
            return string.Format("ID: {0} \nPropertyName: {1} \nPropertyValue: {2} \n CampusID: {3}", ID, PropertyName, PropertyValue, CampusId);
        }

        
    }
}
