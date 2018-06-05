namespace Common
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    public class HttpUtil
    {
        public static string HttpGet(string Url, string postDataStr, string encoding = "utf-8")
        {


            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + ((postDataStr == "") ? "" : "?") + postDataStr);
            request.Method = "GET";
            request.ContentType = "text/html;charset=" + encoding;
            var responseStream = ((HttpWebResponse)request.GetResponse()).GetResponseStream();
            var reader = new StreamReader(responseStream, Encoding.GetEncoding(encoding));
            string content = reader.ReadToEnd();
            reader.Close();
            responseStream.Close();
            byte[] bytes = new byte[] { 0xc2, 160 };
            string oldValue = Encoding.GetEncoding(encoding).GetString(bytes);
            return content.Replace(oldValue, "&nbsp;");
        }

        public static string GetClientIP()
        {
            if (HttpContext.Current != null)
            {
                try
                {
                    HttpRequest request = HttpContext.Current.Request;
                    string ipstr = request.Headers["Cdn-Src-Ip"];


                    if (string.IsNullOrEmpty(ipstr))
                    {
                        ipstr = request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                        if (string.IsNullOrEmpty(ipstr))
                        {
                            if (request.ServerVariables["HTTP_VIA"] != null)
                            {
                                ipstr = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                                if (ipstr == null)
                                {
                                    ipstr = request.ServerVariables["REMOTE_ADDR"];
                                }
                            }
                            else
                            {
                                ipstr = request.ServerVariables["REMOTE_ADDR"];
                            }

                            if (string.Compare(ipstr, "unknown", true) == 0)
                            {
                                ipstr = request.UserHostAddress;
                            }
                        }
                    }


                    if (!string.IsNullOrEmpty(ipstr))
                    {
                        if (ipstr.IndexOf(":") >= 0)
                        {
                            ipstr = ipstr.Substring(0, ipstr.IndexOf(":"));
                        }
                    }


                    return ipstr;
                }
                catch
                {
                }
            }
            return "127.0.0.1";
        }
    }
}

