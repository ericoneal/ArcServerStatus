using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using RestSharp;

namespace ArcServerStatus
{
    public partial class Default : System.Web.UI.Page
    {
        private static int oid = 0;

        private static string urlRootFolder = WebConfigurationManager.AppSettings["urlRootFolder"];
        private static string urlRootFolder2 = WebConfigurationManager.AppSettings["urlRootFolder2"];
        private static string urlFolders = WebConfigurationManager.AppSettings["urlFolders"];
        private static string urlStatusReport = WebConfigurationManager.AppSettings["urlStatusReport"];
        private static string urlServiceStartStop = WebConfigurationManager.AppSettings["urlServiceStartStop"];
        private static string urlLogs = WebConfigurationManager.AppSettings["urlLogs"];
        private static string urlGenerateToken = WebConfigurationManager.AppSettings["urlGenerateToken"];
        private static string username = WebConfigurationManager.AppSettings["username"];
        private static string password = WebConfigurationManager.AppSettings["password"];

        private static string token = GetToken();

        [WebMethod(EnableSession = true)]
        public static object StatReport()
        {



            var client = new RestClient(urlRootFolder + token);

            var request = new RestRequest();
            RestResponse response = client.Execute(request) as RestResponse;
            var content = response.Content; // raw content as string

            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var result = serializer.DeserializeObject(content);

            dynamic dataItem = result;
            ServiceReport serviceReport;

            List<ServiceReport> lstALLServices = new List<ServiceReport>();

            foreach (KeyValuePair<string, object> k in dataItem)
            {
                //root folder services
                if (k.Key.ToUpper() == "SERVICES")
                {
                  
                    dynamic services = k.Value;
                    foreach (Dictionary<string, object> service in services)
                    {
                        serviceReport = GetServiceReport(service);
                        lstALLServices.Add(serviceReport);
                    }


                }

                //Loop thru folders and get services
                if (k.Key.ToUpper() == "FOLDERS")
                {
                    dynamic folders = k.Value;
                    foreach (object folder in folders)
                    {

                        List<ServiceReport> lstServiceReports = FolderReport(folder.ToString());
                        lstALLServices = lstALLServices.Concat(lstServiceReports).ToList();
                    }
                }
            }



    
            ////Return result to jTable
            return new { Result = "OK", Records = lstALLServices };

        }


        [WebMethod(EnableSession = true)]
        public static object GetLogs(string strFoldername, string strServicename, string strType)
        {
            string strServiceStartTime = GetServiceStartTime(strFoldername, strServicename, strType);

            string strStartTime = GetEpochTime(0).ToString();
            string strEndTime = strServiceStartTime;
            //string strEndTime = GetEpochTime(-12).ToString();
            string strUrl = urlLogs + "startTime=" + strStartTime + "&endTime=" + strEndTime + "&level=WARNING&filterType=json&filter={%22server%22:%20%22*%22,%22services%22:%20[%22" + strFoldername + "/" + strServicename + "." + strType + "%22]}&f=pjson&token=" + token;
            var client = new RestClient(strUrl);

            var request = new RestRequest();
            RestResponse response = client.Execute(request) as RestResponse;
            var content = response.Content; // raw content as string

            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var result = serializer.DeserializeObject(content);

            dynamic dataItem = result;

            Log logmsg;
            List<Log> lstLogMsgs = new List<Log>();

            foreach (KeyValuePair<string, object> k in dataItem)
            {
                if (k.Key.ToString().ToUpper() == "LOGMESSAGES")
                {

                    dynamic logmessage = k.Value;

                    foreach (Dictionary<string, object> l in logmessage)
                    {

                        logmsg = new Log();
                        string strCode = l["code"].ToString();
                        logmsg.code = strCode;
                        string strMessage = l["message"].ToString();
                        logmsg.message = strMessage;


                        if (strCode == "10801")
                        {
                            lstLogMsgs.Add(logmsg);
                        }
                      
                    }

                }


            }


            if (lstLogMsgs.Count == 0)
            {
                logmsg = new Log();
                logmsg.code = "0";
                logmsg.message = "No error logs.";
                lstLogMsgs.Add(logmsg);
            }
       

            ////Return result to jTable
            return new { Result = "OK", Records = lstLogMsgs };
        }


        private static string GetServiceStartTime(string strFoldername, string strServicename, string strType)
        {

            string strStartTime = GetEpochTime(0).ToString();
            string strUrl = urlLogs + "startTime=" + strStartTime + "&sinceServerStart=TRUE&level=INFO&filterType=json&filter={%22server%22:%20%22*%22,%22services%22:%20[%22" + strFoldername + "/" + strServicename + "." + strType + "%22]}&f=pjson&token=" + token;
            var client = new RestClient(strUrl);

            var request = new RestRequest();
            RestResponse response = client.Execute(request) as RestResponse;
            var content = response.Content; // raw content as string

            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var result = serializer.DeserializeObject(content);

            dynamic dataItem = result;

            Log logmsg;
            List<Log> lstLogMsgs = new List<Log>();

            foreach (KeyValuePair<string, object> k in dataItem)
            {
                if (k.Key.ToString().ToUpper() == "LOGMESSAGES")
                {

                    dynamic logmessage = k.Value;

                    foreach (Dictionary<string, object> l in logmessage)
                    {

                        logmsg = new Log();
                        string strCode = l["code"].ToString();
                        logmsg.code = strCode;
                        string strMessage = l["message"].ToString();
                        logmsg.message = strMessage;
                        string strTime = l["time"].ToString();
                        logmsg.time = strTime;

                        //Get the service start time from the logs and substract one minute
                        if (strCode == "12003")
                        {
                            double dblTime = Convert.ToDouble(strTime) - 1000;
                            return dblTime.ToString();
                        }

                    }

                }


            }


            return "";
        }


        [WebMethod(EnableSession = true)]
        public static string ServiceOptions(string strAction, string strFoldername, string strServicename, string strType)
        {


            string responsebody = "";
            using (WebClient client = new WebClient())
            {
                System.Collections.Specialized.NameValueCollection reqparm = new System.Collections.Specialized.NameValueCollection();
                reqparm.Add("token", token);
                reqparm.Add("f", "json");
                byte[] responsebytes = client.UploadValues(urlRootFolder2 + strFoldername + "/" + strServicename + "." + strType + "/" + strAction, "POST", reqparm);
                responsebody = Encoding.UTF8.GetString(responsebytes);
            }

            return responsebody;

          

        }







        public static List<ServiceReport> FolderReport(string folder)
        {

            string strURL = string.Format(urlFolders, folder, token);
            var client = new RestClient(strURL);

          
            var request = new RestRequest();
            RestResponse response = client.Execute(request) as RestResponse;
            var content = response.Content; // raw content as string

            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var result = serializer.DeserializeObject(content);


            
            dynamic dataItem = result;
            ServiceReport serviceReport;
            List<ServiceReport> lstServices = new List<ServiceReport>();
            //int oid = 0;
            foreach (KeyValuePair<string, object> k in dataItem)
            {
              if (k.Key.ToUpper() == "SERVICES")
                {
                    dynamic services = k.Value;
                    foreach (Dictionary<string, object> service in services)
                    {
                        serviceReport = GetServiceReport(service);
                        lstServices.Add(serviceReport);
                    }

                  
                }
            }

            return lstServices;

        }

        private static ServiceReport GetServiceReport(Dictionary<string, object> service)
        {
            
                ServiceReport serviceReport;
                string strFolderName = service["folderName"].ToString();
                string strName = service["serviceName"].ToString();
                string strType = service["type"].ToString();

                Dictionary<string, object> status = GetStatusReport(strFolderName, strName, strType);
                bool hasError = SerivceHasError(strFolderName, strName, strType);
              

                serviceReport = new ServiceReport();
                serviceReport.OID = oid++;
                serviceReport.Folder = strFolderName;
                serviceReport.ServiceName = strName;
                serviceReport.configuredState = status["configuredState"].ToString();
                serviceReport.realTimeState = status["realTimeState"].ToString();
                serviceReport.type = strType;
                serviceReport.hasError = hasError.ToString();

                return serviceReport;
      
        }

        private static bool SerivceHasError(string strFolderName, string strName, string strType)
        {
            var o = GetLogs(strFolderName, strName, strType);
            dynamic dataItem = o;
            List<ArcServerStatus.Log> l = dataItem.Records;

            foreach (Log log in l)
            {
                if (log.code == "10801")
                {
                    return true;
                }
            }

            return false;
        }

        private static Dictionary<string, object> GetStatusReport(string foldername, string servicename, string type)
        {

            string strURL = string.Format(urlStatusReport, foldername, servicename, type, token);
            var client = new RestClient(strURL);

            var request = new RestRequest();
            RestResponse response = client.Execute(request) as RestResponse;
            var content = response.Content; // raw content as string

            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var result = serializer.DeserializeObject(content);

            dynamic dataItem = result;


            return dataItem;
        }


        private static long GetEpochTime(int offset)
        {
            DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime starttime = DateTime.Now.AddHours(offset);
            DateTime dateTime = starttime.ToUniversalTime();
            double totalMs = (dateTime - UnixEpoch).TotalMilliseconds;
            long dt = (long)totalMs;
            return dt;

        }


        private static string GetToken()
        {
            string responsebody = "";
            using (WebClient client = new WebClient())
            {
                System.Collections.Specialized.NameValueCollection reqparm = new System.Collections.Specialized.NameValueCollection();
                reqparm.Add("username", username);
                reqparm.Add("password", password);
                reqparm.Add("client", "requestip");
                reqparm.Add("f", "json");
                byte[] responsebytes = client.UploadValues(urlGenerateToken, "POST", reqparm);
                responsebody = Encoding.UTF8.GetString(responsebytes);
            }

            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var result = serializer.DeserializeObject(responsebody);
            dynamic dataItem = result;

            foreach (KeyValuePair<string, object> k in dataItem)
            {
                if (k.Key.ToString().ToUpper() == "TOKEN")
                {
                    return k.Value.ToString();
                }
            }



            return "";


        }




    }





    public class ServiceReport
    {

        public int OID { get; set; }
        public string Folder { get; set; }
        public string ServiceName { get; set; }
        public string configuredState { get; set; }
        public string realTimeState { get; set; }
        public string type { get; set; }
        public string hasError { get; set; }
    }

    public class Log
    {
        public string code { get; set; }
        public string message { get; set; }
        public string time { get; set; }

    }
}