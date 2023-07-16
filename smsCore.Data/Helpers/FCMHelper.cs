using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace smsCore.Data.Helpers
{
   
    public class FCMHelper
    {
        private readonly IConfiguration _config;
        
        public FCMHelper(IConfiguration config)
        {
            _config=config;
        }

        public async Task<string> SendNotification(string title, string body, string[] toids, string data)
        {
            string applicationID = _config.GetSection("FireBase:FCMServerKey").Value;
            string SENDER_ID = _config.GetSection("FireBase:FCMSenderId").Value;
            if (toids.Length == 0)
            {
                return "No user device found";
            }


            var payload = new
            {
                registration_ids = toids,
                priority = "high",
                content_available = true,
                notification = new
                {
                    body = body,
                    title = title,
                },
                data = new
                {
                    data = data
                }
            };

            var jsonBody = JsonConvert.SerializeObject(payload);
            using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://fcm.googleapis.com/fcm/send"))
            {
                httpRequest.Headers.TryAddWithoutValidation("Authorization", "key=" + applicationID);
                httpRequest.Headers.TryAddWithoutValidation("Sender", "id=" + SENDER_ID);
                httpRequest.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                httpRequest.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    var result = await httpClient.SendAsync(httpRequest);
                    string content = await result.Content.ReadAsStringAsync();
                    return content;
                    //400 - bad Request
                }
            }
        }
        public string SendNotification(string to,string title, string body, string[] toids)
        {
            string applicationID = _config.GetSection("FireBase:FCMServerKey").Value;
            string SENDER_ID = _config.GetSection("FireBase:FCMSenderId").Value;


            WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
            tRequest.Method = "post";
            //serverKey - Key from Firebase cloud messaging server  
            tRequest.Headers.Add(string.Format("Authorization: key={0}", applicationID));
            //Sender Id - From firebase project setting  
            tRequest.Headers.Add(string.Format("Sender: id={0}", SENDER_ID));
            tRequest.ContentType = "application/json";
            var payload = new
            {
                priority = "high",
                content_available = true,
                notification = new
                {
                    body = body,
                    title = title,
                    badge = 1
                },
                //,
                data = new
                {
                    Message = body
                }

            };

            string postbody = JsonConvert.SerializeObject(payload).ToString();
            Byte[] byteArray = Encoding.UTF8.GetBytes(postbody);
            tRequest.ContentLength = byteArray.Length;
            using (Stream dataStream = tRequest.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
                using (WebResponse tResponse = tRequest.GetResponse())
                {
                    using (Stream dataStreamResponse = tResponse.GetResponseStream())
                    {
                        if (dataStreamResponse != null)
                        {
                            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                            {
                                String sResponseFromServer = tReader.ReadToEnd();
                                //result.Response = sResponseFromServer;
                                return sResponseFromServer;
                            }
                        }
                        else return "Data stream response is null";
                    }
                }
            }
        }
    }
}