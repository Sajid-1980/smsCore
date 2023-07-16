using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using Models;

namespace smsCore.Data.Helpers
{
    public class MessagingSystem
    {
        public MessagingSystem(SchoolEntities _objdb, ClsBussinessSetting _setting, CurrentUser user)
        {
            setting = _setting;
            objdb = _objdb;
            error = string.Empty;
           

            try
            {
                var setting = objdb.SMSApplicationProperties.AsNoTracking().Where(w =>
                    w.PropertyName == "MSISDN" || w.PropertyName == "Password" || w.PropertyName == "Mask");
                Msisdn = setting.Where(w => w.PropertyName == "MSISDN").Select(s => s.PropertyValue).FirstOrDefault()
                    .Trim();
                pwd = setting.Where(w => w.PropertyName == "Password").Select(s => s.PropertyValue).FirstOrDefault()
                    .Trim();
                mask = setting.Where(w => w.PropertyName == "Mask").Select(s => s.PropertyValue).FirstOrDefault()
                    .Trim();
                GetConnected();
            }
            catch (Exception aa)
            {
                error = aa.Message;
            }
            _user = user;
        }

        #region private member variables
        private readonly ClsBussinessSetting setting;
        private SchoolEntities objdb;
        private readonly CurrentUser _user;
        private string SessionId = "";
        private string response = "";
        private readonly string mask = "";
        public string Msisdn = "";
        public string pwd = "";
        private string to = "";
        public string error { get; set; }

        #endregion

        #region Public member functions

        public string AttendanceMessageTempalte(int campusid, PublicVariables.AttendanceType type)
        {
            setting.CampusId=campusid;
            string message = string.Empty;
            if (type == PublicVariables.AttendanceType.Present || type== PublicVariables.AttendanceType.LateComing)
            {
                message = (string)setting.ReadWithType("PresentMessage", typeof(string));
                if (string.IsNullOrEmpty(message))
                {
                    message = "Dear {2}!\n Your child {1}-{0} has reached school at {4} {3}.";
                    setting.WriteorUpdate("PresentMessage", message);
                }
            }
            else if (type == PublicVariables.AttendanceType.Absent)
            {
                message = (string)setting.ReadWithType("AbsentMessage", typeof(string));
                if (string.IsNullOrEmpty(message))
                {
                    message = "Dear {2}!\n Your child {1}-{0} is absent from school  {4}.";
                    setting.WriteorUpdate("AbsentMessage", message);
                }
            }
            else if (type == PublicVariables.AttendanceType.Leave)
            {
                message = (string)setting.ReadWithType("LeaveMessage", typeof(string));
                if (string.IsNullOrEmpty(message))
                {
                    message = "Dear {2}!\n Your child {1}-{0} is on leave from school  {4}.";
                    setting.WriteorUpdate("LeaveMessage", message);
                }
            }
            return message;
        }

        /// <summary>
        ///     Send Message to nos
        /// </summary>
        /// <param name="Message">text to sent to specified nos in parameters</param>
        /// <param name="mobile">mobile no in any format e.g 034,9234</param>
        /// <returns></returns>
        public async Task<bool> sendMessage(string Message, string mobile, bool unicode = false)
        {
            error = string.Empty;
            var send = false;
            var thisMessage = string.Format("to={0}&text={1}&mask={2}&unicode={3}", FormatNos(new object[] { mobile }),
                Message, mask, unicode);
            try
            {
                var message = await sendMessage(thisMessage);
                if (message)
                    send = true;
            }
            catch (Exception aa)
            {
                error = aa.Message;
                send = false;
            }

            return send;
        }

        /// <summary>
        ///     Message type '0' for general message
        ///     Message type '1' for attendance message
        ///     Message type '2' for Fee type message
        ///     send same message to group of nos
        ///     Message type use for message type like attendace,fee received, fee issued
        /// </summary>
        /// <param name="Message">text to sent to specified nos in parameters</param>
        /// <param name="mobile">mobile nos(array/group) in any format e.g 034,9234</param>
        /// <returns></returns>
        public async Task<bool> sendMessage(string Message, object[] mobile, bool unicode = false)
        {
            var thisMessage = string.Format("to={0}&text={1}&mask={2}&unicode={3}", FormatNos(mobile), Message, mask,
                unicode);

            return await sendMessage(thisMessage);
        }

        public async Task<bool> sendMessage(string Message, string mobile, int amdid, bool unicode = false)
        {
            error = string.Empty;
            var send = false;
            var stdid = objdb.Admissions.Where(w => w.ID == amdid).FirstOrDefault().Student.ID;
            var thisMessage = string.Format("to={0}&text={1}&mask={2}&unicode={3}", FormatNos(new object[] { mobile }),
                Message, mask, unicode);
            try
            {
                var messageSent = await sendMessage(thisMessage);
                if (messageSent)
                {
                    send = true;

                    var mr = new MessageRecord
                    {
                        TextMessage = Message,
                        StudentID = stdid,
                        UserID = _user.UserID,
                        EntryDate = DateTime.Now.Date,
                        PerSMSCharge = true
                    };
                    objdb.MessageRecords.Add(mr);
                    try
                    {
                        objdb.SaveChanges();
                    }
                    catch
                    {
                    }
                }

                else
                {
                    send = false;
                    var mr = new MessageRecord();

                    mr.TextMessage = Message;
                    mr.StudentID = stdid;
                    mr.UserID = _user.UserID;
                    mr.EntryDate = DateTime.Now.Date;
                    mr.PerSMSCharge = true;
                    objdb.MessageRecords.Add(mr);
                    try
                    {
                        objdb.SaveChanges();
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception aa)
            {
                error = aa.Message;
                send = false;
            }

            return send;
        }


        /// <summary>
        /// </summary>
        /// <param name="RegNo"></param>
        /// <param name="message"></param>
        /// <param name="save"></param>
        /// <returns></returns>
        public async Task<bool> sendMessage(int RegNo, string message, bool save, bool applypersmscharges = false,
            bool unicode = false)
        {
            var send = false;
            var std = objdb.Students.Where(w => w.RegistrationNo == RegNo).FirstOrDefault();
            var mobile = std.StudentMobiles.Where(w => w.IsDefault).Select(s => s.MobileNo).ToArray();
            //System.Windows.Forms.MessageBox.Show(mobile.ToString());

            message = System.Web.HttpUtility.UrlEncode(message);
            var thisMessage = string.Format("to={0}&text={1}&mask={2}&unicode={3}", FormatNos(mobile), message, mask,
                unicode);
            var messageSent = await sendMessage(thisMessage);
            if (messageSent)
            {
                send = true;
                if (save)
                {
                    var id = 0;
                    var mr = new MessageRecord();
                    try
                    {
                        id = objdb.MessageRecords.Select(s => s.ID).Max();
                    }
                    catch
                    {
                    }

                    id++;
                    mr.ID = id;
                    mr.TextMessage = message;
                    mr.StudentID = std.ID;
                    mr.EntryDate = DateTime.Now.Date;

                    mr.UserID = _user.UserID;
                    mr.EntryDate = DateTime.Now.Date;
                    mr.PerSMSCharge = applypersmscharges;
                    objdb.MessageRecords.Add(mr);
                    try
                    {
                        objdb.SaveChanges();
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                send = false;
                if (save)
                {
                    var id = 0;
                    var mr = new MessageRecord();
                    try
                    {
                        id = objdb.MessageRecords.Select(s => s.ID).Max();
                    }
                    catch
                    {
                    }

                    id++;
                    mr.ID = id;
                    mr.TextMessage = message;
                    mr.StudentID = std.ID;
                    mr.EntryDate = DateTime.Now.Date;

                    mr.UserID = _user.UserID;
                    mr.EntryDate = DateTime.Now.Date;
                    mr.PerSMSCharge = applypersmscharges;
                    objdb.MessageRecords.Add(mr);
                    try
                    {
                        objdb.SaveChanges();
                    }
                    catch
                    {
                    }
                }
            }

            return send;
        }

        #endregion


        #region private member functions

        private bool GetConnected()
        {
            var con = string.Format(
                "https://telenorcsms.com.pk:27677/corporate_sms2/api/auth.jsp?msisdn={0}&password={1}", System.Web.HttpUtility.UrlEncode(Msisdn.Trim()),
              System.Web.HttpUtility.UrlEncode(pwd.Trim()));
            var myReq = (HttpWebRequest)WebRequest.Create(con);
            var myResp = (HttpWebResponse)myReq.GetResponse();
            var rdr = new StreamReader(myResp.GetResponseStream());
            var resp = rdr.ReadToEnd();
            rdr.Close();
            myResp.Close();

            var xdoc = new XmlDocument();
            xdoc.LoadXml(resp);

            foreach (XmlNode x in xdoc)
                foreach (XmlNode xx in x)
                    if (xx.OuterXml.Contains("data"))
                        SessionId = xx.InnerText;
                    else if (xx.OuterXml.Contains("response")) response = xx.InnerText;

            return HandleError(SessionId, "Connection Error");
        }

        private bool HandleError(string ecode, string function)
        {
            error = string.Empty;
            if (ecode == "Error 200")
            {
                error = function + ": Failed login. Username and password do not match.";
                //SchoolSystemCommon.ErrorLogging.Error("Failed login. Username and password do not match.");
                return false;
            }

            if (ecode == "Internet")
            {
                error = function + " : Unable to connect with internet.";
                //.ErrorLogging.Error("Unknown MSISDN, Please Check Format i-e 92345xxxxxxxxx");
                return false;
            }

            if (ecode == "Error 201")
            {
                error = function + ": Unknown MSISDN, Please Check Format i-e 92345xxxxxxxxx";
                //.ErrorLogging.Error("Unknown MSISDN, Please Check Format i-e 92345xxxxxxxxx");
                return false;
            }

            if (ecode == "Error 100")
            {
                error = function + ": Out of credit.";
                //SchoolSystemCommon.ErrorLogging.Error("Out of credit.");
                return false;
            }

            if (ecode == "Error 101")
            {
                error = function + ": Feild or input parameter missing";
                //SchoolSystemCommon.ErrorLogging.Error("Feild or input parameter missing");
                return false;
            }

            if (ecode == "Error 102")
            {
                error = function + ": Invalid session ID ot the session has expired.Login again.";
                //SchoolSystemCommon.ErrorLogging.Error("Invalid session ID ot the session has expired.Login again.");
                return false;
            }

            if (ecode == "Error 103")
            {
                error = function + ": Invalid Mask";
                // SchoolSystemCommon.ErrorLogging.Error("Invalid Mask");
                return false;
            }
            else if (ecode == "Error 211")
            {
                error = function + ": Unknown Message ID.";
                //SchoolSystemCommon.ErrorLogging.Error("Unknown Message ID.");
                return false;
            }
            else if (ecode == "Error 300")
            {
                error = function + "Account has been blocked/Suspended";
                //SchoolSystemCommon.ErrorLogging.Error("Account has been blocked/Suspended");
                return false;
            }
            else if (ecode == "Error 400")
            {
                error = function + "Duplicate list name";
                //SchoolSystemCommon.ErrorLogging.Error("Duplicate list name");
                return false;
            }
            else if (ecode == "Error 401")
            {
                error = function + "List name is missing";
                //SchoolSystemCommon.ErrorLogging.Error("List name is missing.");
                return false;
            }
            else if (ecode == "Error 411")
            {
                error = function + "Invalid MSISDN in the list";
                //SchoolSystemCommon.ErrorLogging.Error("Invalid MSISDN in the list.");
                return false;
            }
            else if (ecode == "Error 412")
            {
                error = function + "List Id is missing";
                //SchoolSystemCommon.ErrorLogging.Error("List Id is missing.");
                return false;
            }
            else if (ecode == "Error 413")
            {
                error = function + "No MSISDN in the list";
                //SchoolSystemCommon.ErrorLogging.Error("No MSISDN in the list.");
                return false;
            }
            else if (ecode == "Error 414")
            {
                error = function + "List could not be updated. Unknown error";
                //SchoolSystemCommon.ErrorLogging.Error("List could not be updated. Unknown error.");
                return false;
            }
            else if (ecode == "Error 415")
            {
                error = function + "Invalid list ID";
                //SchoolSystemCommon.ErrorLogging.Error("Invalid list ID");
                return false;
            }
            else if (ecode == "Error 500")
            {
                error = function + "Duplicate campaign name";
                //SchoolSystemCommon.ErrorLogging.Error("Duplicate campaign name.");
                return false;
            }
            else if (ecode == "Error 501")
            {
                error = function + "Campaiign name is missing";
                //SchoolSystemCommon.ErrorLogging.Error("Campaiign name is missing.");
                return false;
            }
            else if (ecode == "Error 502")
            {
                error = function + "SMS text is missing";
                //SchoolSystemCommon.ErrorLogging.Error("SMS text is missing.");
                return false;
            }
            else if (ecode == "Error 503")
            {
                error = function + "No list selected or one of the list IDs is invalid";
                //SchoolSystemCommon.ErrorLogging.Error("No list selected or one of the list IDs is invalid");
                return false;
            }
            else if (ecode == "Error 504")
            {
                error = function + "Invalid schedule time for campaign";
                //SchoolSystemCommon.ErrorLogging.Error("Invalid schedule time for campaign.");
                return false;
            }
            else if (ecode == "Error 506")
            {
                error = function + "Can not send message at the specified time. Please specify a different time";
                //SchoolSystemCommon.ErrorLogging.Error("Can not send message at the specified time. Please specify a different time.");
                return false;
            }
            else if (ecode == "Error 507")
            {
                error = function + "Campaign could not be saed. Unknown Error";
                //SchoolSystemCommon.ErrorLogging.Error("Campaign could not be saed. Unknown Error.");
                return false;
            }
            else if (ecode == "Error 600")
            {
                error = function + "Campaign ID is missing";
                //SchoolSystemCommon.ErrorLogging.Error("Campaign ID is missing");
                return false;
            }

            return true;
        }

        private async Task<bool> sendMessage(string message)
        {
            error = string.Empty;
            var ecode = "";
            try
            {
                var req = string.Format(
                    "https://telenorcsms.com.pk:27677/corporate_sms2/api/sendsms.jsp?session_id={0}&{1}", SessionId,
                    message);
                //MessageBox.Show(req);
                var myReq = (HttpWebRequest)WebRequest.Create(req);
                var myResp = (HttpWebResponse)await myReq.GetResponseAsync();
                var rdr = new StreamReader(myResp.GetResponseStream());
                var nresp = rdr.ReadToEnd();
                //MessageBox.Show(nresp);
                rdr.Close();
                myResp.Close();

                var xdoc = new XmlDocument();
                xdoc.LoadXml(nresp);

                foreach (XmlNode x in xdoc)
                    foreach (XmlNode xx in x)
                        if (xx.OuterXml.Contains("data"))
                            ecode = xx.InnerText;
                        else if (xx.OuterXml.Contains("response")) response = xx.InnerText;

                #region error handling

                #endregion

                return HandleError(ecode, "Sending Error");
            }
            catch (Exception aa)
            {
                error = aa.Message;
                ecode = "Internet";
            }

            return HandleError(ecode, "Connection error");
        }

        private string FormatNos(object[] nos)
        {
            if (nos.Count() < 1)
                return "";
            for (var i = 0; i < nos.Count(); i++)
                if (!nos[i].ToString().StartsWith("92"))
                    nos[i] = nos[i].ToString().StartsWith("0")
                        ? "92" + nos[i].ToString().Remove(nos[i].ToString().IndexOf('0', 0, 1), 1).Trim()
                        : "92" + nos[i];
            to = string.Join(",", nos);
            return to;
        }

        #endregion

        //public MessageHistoryResponse GetHistory()
        //{
        //    ScrapingBrowser browser = new ScrapingBrowser();
        //    browser.AllowAutoRedirect = true;
        //    browser.AllowMetaRedirect = true;

        //    string baseUrl = "https://telenorcsms.com.pk:27677/corporate_sms2/";
        //    WebPage homePage = browser.NavigateToPage(new Uri(baseUrl+"login.jsp"));
        //    var formnode = homePage.Html.CssSelect("form[action*='LoginActionServlet']").FirstOrDefault();
        //    PageWebForm form = new PageWebForm(formnode, browser);
        //    form.Action = baseUrl+ form.Action;
        //    form["username"] = Msisdn;

        //    form["password"] = pwd;
        //    browser.AllowAutoRedirect = true;

        //    form.Method = HttpVerb.Post;
        //    WebPage resultsPage = form.Submit();

        //    //Second Section Start Here
        //    var profilePageUrl = baseUrl+"userProfile.jsp";

        //    WebPage profilePage = browser.NavigateToPage(new Uri(profilePageUrl));


        //    var CRD = profilePage.Html.SelectSingleNode(".//td[./b[contains(text(),'Credit:')]]").GetNextSibling("td").InnerText;

        //    var ACDText = profilePage.Html.SelectSingleNode(".//td[./b[contains(text(),'Account Creation Date:')]]").GetNextSibling("td").InnerText;

        //    //Console.WriteLine("Credit: "+CRD + " Created On: " + ACDText);
        //    var MessageProfile = baseUrl + "quick_message_history.jsp?p=0";

        //    WebPage homePage2 = browser.NavigateToPage(new Uri(MessageProfile));
        //    var formnode2 = homePage2.Html.CssSelect("form[action*='quick_message_history.jsp']").FirstOrDefault();
        //    PageWebForm form2 = new PageWebForm(formnode2, browser);

        //    //string date = DateTime.Now.ToString("d MMM yy, hh:mmtt");
        //    string format = "d MMM, yy hh:mmtt";
        //    //DateTime dt = DateTime.ParseExact(ACDText, format, System.Globalization.CultureInfo.InvariantCulture);
        //    DateTime dt = DateTime.ParseExact(ACDText, format, System.Globalization.CultureInfo.InvariantCulture);
        //    List<MessageHistory> data = new List<MessageHistory>();

        //    if (dt <= DateTime.Today)
        //    {
        //        for (var t = dt; t <= DateTime.Today; t=t.AddDays(1))
        //        {
        //            form2["date"] = t.ToString("yyyy-MM-dd");// "2022-05-23";

        //            //browser.AllowAutoRedirect = true;

        //            form2.Method = HttpVerb.Post;
        //            WebPage resultPage2 = form2.Submit();

        //            //Next section start here //table[contains(@class,'custom')]

        //            var historyTable = resultPage2.Html.SelectNodes("//table[contains(@class,'custom')]");
        //            foreach (HtmlNode table in historyTable)
        //            {
        //                int k = 0;
        //                foreach (HtmlNode row in table.SelectNodes("tr"))
        //                {
        //                    if (row == null)
        //                        continue;
        //                    if (k == 0)
        //                    {
        //                        k++;
        //                        continue;
        //                    }
        //                    int i = 0;
        //                    MessageHistory history = new MessageHistory();
        //                    var datas = row.SelectNodes("th|td");
        //                    if (datas==null || datas.Count == 0)
        //                        continue;
        //                    foreach (HtmlNode cell in datas)
        //                    {
        //                        if (i == 0)
        //                        {
        //                            history.Source = cell.InnerText;
        //                        }
        //                        if (i == 1)
        //                        {
        //                            history.Destination = cell.InnerText;
        //                        }
        //                        if (i == 2)
        //                        {
        //                            history.Message = cell.InnerText;
        //                        }
        //                        if (i == 3)
        //                        {
        //                            history.Timestamp = cell.InnerText;
        //                        }
        //                        if (i == 4)
        //                        {
        //                            history.Operator = cell.InnerText;
        //                        }
        //                        if (i == 5)
        //                        {
        //                            decimal.TryParse(cell.InnerText, out decimal cost);
        //                            history.Cost = cost;
        //                        }
        //                        i++;
        //                    }
        //                    data.Add(history);

        //                }
        //            }
        //        }
        //    }

        //    MessageHistoryResponse model = new MessageHistoryResponse();
        //    model.CreationDate=CRD;
        //    decimal.TryParse(CRD, out decimal Credit);
        //    model.Credit=Credit;
        //    model.Messages=data;
        //    model.LastCheckedDate=DateTime.Now;
        //    return model;



        //}

    }
    public class MessageHistoryResponse
    {
        public string CreationDate { get; set; }
        public decimal Credit { get; set; }
        public DateTime LastCheckedDate { get; set; }
        public List<MessageHistory> Messages { get; set; }
    }
}