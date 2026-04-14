using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System;
using System.IO;
using System.Net;
using context = System.Web.HttpContext;

namespace EkycAPI
{
    public static class ExceptionLogging
    {

        private static String Request;

        public static void SendErrorToText(string reqst)
        {
            var line = Environment.NewLine + Environment.NewLine;

            Request = reqst;
            //ErrorlineNo = ex.StackTrace; //.Substring(ex.StackTrace.Length - 7, 7);
            //Errormsg = ex.GetType().Name.ToString();
            //extype = ex.GetType().ToString();
            //exurl = context.Current.Request.Url.ToString();
            //ErrorLocation = ex.Message.ToString();

            try
            {
                string filepath = @"D:\Error_Log\"; //Text File Path

                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);

                }
                filepath = filepath + DateTime.Today.ToString("dd-MM-yy") + ".txt";   //Text File Name
                if (!File.Exists(filepath))
                {


                    File.Create(filepath).Dispose();

                }
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    string error = "Log Written Date:" + " " + DateTime.Now.ToString() + line + "Error Line No :" + " " + Request;
                    sw.WriteLine("-----------Exception Details On " + " " + DateTime.Now.ToString() + "-----------------");
                    sw.WriteLine(error);
                    sw.WriteLine("--------------------------------End-------------------------------------------------");
                    sw.Flush();
                    sw.Close();

                }

            }
            catch (Exception e)
            {
                e.ToString();

            }
        }

    }
}