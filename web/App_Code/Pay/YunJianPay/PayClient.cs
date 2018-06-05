using System;
using System.Collections.Specialized;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Web.Security;
using System.Web;

namespace Pay.YunJianPay
{


    /// <summary>
    /// PayClient 的摘要说明
    /// </summary>
    public class PayClient : IPay
    {

        string appid;
        string appsecret;
        string notifyUrl;

        public PayClient(PayConfig config)
        {
            this.appid = config.Data["appid"];
            this.appsecret = config.Data["appsecret"];
            this.notifyUrl = config.Data["notifyurl"];
        }

        public bool DoCallback(NameValueCollection query, NameValueCollection form, string bodydata, Action<CallbackInfo> action)
        {


            var dict = new Dictionary<string, string>();
            dict["status"] = form["status"];
            dict["customerid"] = form["customerid"];
            dict["sdpayno"] = form["sdpayno"];
            dict["sdorderno"] = form["sdorderno"];
            dict["total_fee"] = form["total_fee"];
            dict["paytype"] = form["paytype"];
            dict["remark"] = form["remark"];
            dict["sign"] = form["sign"];



            string signString = "customerid=" + dict["customerid"]
                + "&status=" + dict["status"]
                + "&sdpayno=" + dict["sdpayno"]
                + "&sdorderno=" + dict["sdorderno"]
                + "&total_fee=" + dict["total_fee"]
                + "&paytype=" + dict["paytype"]
                + "&" + this.appsecret;



            string sign = UserMd5(signString, 32);


            var callback = new CallbackInfo();
            callback.Status = ResultStatus.Fail;

            if (!string.IsNullOrEmpty(dict["sdorderno"])
                //&& sign.Equals(dict["sign"])
                && dict["status"].Equals("1"))
            {
                callback.Status = ResultStatus.Succ;
                callback.OrderNo = dict["sdorderno"];
                callback.AttachData = null;
            }


            action(callback);

            if (callback.Status == ResultStatus.Succ)
            {
                return true;
            }

            return false;


        }

        public void EchoStatus(ResultStatus status)
        {
            if (status == ResultStatus.Succ)
            {
                HttpContext.Current.Response.Write("success");
            }
            else
            {
                HttpContext.Current.Response.Write("fail");
            }
        }

        public PayResult Pay(PayInfo info)
        {


            var dict = new Dictionary<string, string>();


            dict["version"] = "1.0";
            dict["customerid"] = this.appid;
            dict["sdorderno"] = info.OrderNo;
            dict["total_fee"] = ((decimal)(info.Amount) * 0.01m).ToString("F2");
            dict["paytype"] = "wxh5";
            //dict["bankcode"] = "";
            dict["notifyurl"] = this.notifyUrl;
            dict["returnurl"] = info.ReturnUrl;
            //dict["get_code"] = "";
            dict["sign"] = GetSign(dict);


            var sb = new StringBuilder();
            sb.Append(@"<!doctype html>\r\n");
            sb.Append(@"<html>");
            sb.Append(@"<head>");
            sb.Append(@"    <meta charset=""utf8"">");
            sb.Append(@"    <title>正在转到付款页</title>");
            sb.Append(@"</head>");
            sb.Append(@"<body onLoad=""document.pay.submit()"">");
            sb.Append(@"    <form name=""pay"" action=""http://www.yunjianpay.com/apisubmit"" method=""post"">");
            sb.Append(@"        <input type=""hidden"" name=""version"" value=""" + dict["version"] + @""">");
            sb.Append(@"        <input type=""hidden"" name=""customerid"" value=""" + dict["customerid"] + @""">");
            sb.Append(@"        <input type=""hidden"" name=""sdorderno"" value=""" + dict["sdorderno"] + @""">");
            sb.Append(@"        <input type=""hidden"" name=""total_fee"" value=""" + dict["total_fee"] + @""">");
            sb.Append(@"        <input type=""hidden"" name=""paytype"" value=""" + dict["paytype"] + @""">");
            sb.Append(@"        <input type=""hidden"" name=""notifyurl"" value=""" + dict["notifyurl"] + @""">");
            sb.Append(@"        <input type=""hidden"" name=""returnurl"" value=""" + dict["returnurl"] + @""">");    
            sb.Append(@"        <input type=""hidden"" name=""sign"" value=""" + dict["sign"] + @""">");
            sb.Append(@"    </form>");
            sb.Append(@"</body>");
            sb.Append(@"</html>");

            string payContent = sb.ToString();



            var result = new PayResult();
            if (!string.IsNullOrEmpty(payContent))
            {
                result.ResultType = PayResultType.Html;
                result.ResultData = payContent;
                result.Status = ResultStatus.Succ;
            }
            else
            {
                result.Status = ResultStatus.Fail;
            }

            return result;
        }

        string GetSign(Dictionary<string, string> dict)
        {

            string signString = "version=" + dict["version"]
                + "&customerid=" + dict["customerid"]
                + "&total_fee=" + dict["total_fee"]
                + "&sdorderno=" + dict["sdorderno"]
                + "&notifyurl=" + dict["notifyurl"]
                + "&returnurl=" + dict["returnurl"]
                + "&" + this.appsecret;

            return UserMd5(signString, 32);
        }


        public static string UserMd5(string str, int code)
        {
            if (code == 16) //16位MD5加密（取32位加密的9~25字符）
            {
                return FormsAuthentication.HashPasswordForStoringInConfigFile(str, "MD5").ToLower().Substring(8, 16);
            }
            if (code == 32)
            {
                return FormsAuthentication.HashPasswordForStoringInConfigFile(str, "MD5").ToLower();
            }

            return str;
        }

    }

}
