
using Newtonsoft.Json;
using System;
using System.IO;
using System.Web;

namespace API
{
    /// <summary>
    /// 主入口
    /// </summary>
    public partial class OpenApi : IHttpHandler
    {
        HttpRequest Request;
        HttpResponse Response;




        public void ProcessRequest(HttpContext context)
        {
            Request = context.Request;
            Response = context.Response;



            if (pay_callback())
            {
                return;
            }


            string pathInfo = Request.PathInfo ?? string.Empty;

            switch (pathInfo)
            {
                case "/pay.do":
                    pay_do();
                    break;
                case "/pay/status.json":
                    pay_status();
                    break;
                case "/test.do":
                    Pay.PayFactory.Create("weixin.jinku");
                    break;
                default:
                    break;
            }
        }

        void pay_status()
        {
            string orderno = Request.QueryString["orderno"] ?? string.Empty;
            if (string.IsNullOrEmpty(orderno))
            {
                return;
            }

            var service = new Services.OrderService();
            var status = service.QueryOrderStatus(orderno);

            CallbackData(new { code = 0, status = status, orderno = orderno });
        }

        bool pay_callback()
        {
            string pathInfo = Request.PathInfo ?? string.Empty;
            string callbackSign = "/pay/callback.do/";
            if (pathInfo.IndexOf(callbackSign) == 0)
            {
                string payConfigKey = pathInfo.Substring(callbackSign.Length);


                pay_callback_do(payConfigKey);

                return true;
            }

            return false;
        }

        void pay_callback_do(string payConfigKey)
        {
            if (string.IsNullOrEmpty(payConfigKey))
            {
                return;
            }

            var query = Request.QueryString;
            var form = Request.Form;
            string bodydata = ReadBodyData();


            var payclient = Pay.PayFactory.Create(payConfigKey);

            Action<Pay.CallbackInfo> action = new Action<Pay.CallbackInfo>((info) =>
            {
                string orderno = info.OrderNo;
                if (!string.IsNullOrEmpty(orderno))
                {
                    var service = new Services.OrderService();

                    var status = Services.OrderStatus.Fail;

                    if (info.Status == Pay.ResultStatus.Succ)
                    {
                        status = Services.OrderStatus.Succ;
                    }

                    service.UpdateOrderStatus(orderno, status);
                }


            });

            if (payclient.DoCallback(query, form, bodydata, action))
            {
                payclient.EchoStatus(Pay.ResultStatus.Succ);
            }
            else
            {
                payclient.EchoStatus(Pay.ResultStatus.Fail);
            }
        }

        void pay_do()
        {
            string datatype = Request.QueryString["dt"] ?? string.Empty;
            string project = Request.QueryString["project"] ?? string.Empty;
            string paytype = Request.QueryString["paytype"] ?? string.Empty;
            string name = Request.QueryString["name"] ?? string.Empty;

            string rurl = Request.QueryString["rurl"];
            string rurlOK = null;
            if (!string.IsNullOrEmpty(rurl))
            {
                Uri rurlUri = null;

                if (Uri.TryCreate(rurl, UriKind.Absolute, out rurlUri))
                {
                    rurlOK = rurl;
                }
            }


            int amount = int.Parse(Request.QueryString["amt"] ?? "0");

            if (string.IsNullOrEmpty(project))
            {
                EchoFailJson("project is null or empty");
                return;
            }

            if (amount <= 0)
            {
                EchoFailJson("amt<=0");
                return;
            }

            var payconfig = Config.GetPayConfig(project, paytype);

            if (payconfig == null)
            {
                EchoFailJson("payconfig is null");
                return;
            }


            var orderService = new Services.OrderService();


            string orderno = Request.QueryString["orderno"] ?? string.Empty;

            if (string.IsNullOrEmpty(orderno))
            {
                orderno = orderService.genNotExistOrerNo(project);
            }

            string cip = Common.HttpUtil.GetClientIP();

            bool succ = orderService.InitOrder(new Services.OrderInfo
            {
                Amount = amount,
                OrderNo = orderno,
                PayCH = payconfig.Key,
                PayType = paytype,
                Project = project
            });


            if (!succ)
            {

                EchoFailJson("InitOrder fail");
                return;
            }


            var payclient = Pay.PayFactory.Create(payconfig);

            var result = payclient.Pay(new Pay.PayInfo
            {

                Amount = amount,
                Attach = "project=" + project + "&paytype=" + paytype,
                IP = cip,
                Name = name,
                OrderNo = orderno,
                ReturnUrl = rurlOK
            });

            if (result.Status != Pay.ResultStatus.Succ)
            {
                EchoFailJson("pay fail");
                return;
            }



            if (result.ResultType == Pay.PayResultType.Url)
            {
                var data = new
                {
                    url = result.ResultData,
                    data = result.Data,
                    type = "url",
                    orderno = orderno
                };

                if (datatype.Equals("html"))
                {
                    string url = data.url as string;
                    if (!string.IsNullOrEmpty(url))
                    {
                        Response.Redirect(url);
                    }
                    else
                    {
                        EchoFailJson("pay fail");
                        return;
                    }
                }
                else
                {
                    EchoSuccJson(data);

                }

            }
            else if (result.ResultType == Pay.PayResultType.Html)
            {
                Response.ContentType = "text/html;charset=utf-8";
                Response.Write(result.ResultData.ToString());
            }
        }


        void CallbackData(object data)
        {
            string callback = Request.QueryString["callback"] ?? string.Empty;

            if (!string.IsNullOrEmpty(callback))
            {
                Response.Write(callback + "(" + JsonConvert.SerializeObject(data) + ")");
            }
            else
            {
                Response.Write(JsonConvert.SerializeObject(data));
            }
        }



        void EchoFailJson(string msg = null)
        {
            var rsp = new Common.DB.NVCollection();

            rsp["code"] = -1;
            rsp["status"] = "fail";
            if (!string.IsNullOrEmpty(msg))
            {
                rsp["msg"] = msg;
            }

            CallbackData(rsp);
        }

        void EchoSuccJson(object data)
        {
            var rsp = new Common.DB.NVCollection();

            rsp["code"] = 0;
            rsp["status"] = "succ";
            if (data != null)
            {
                rsp["data"] = data;
            }

            CallbackData(rsp);
        }

        string ReadBodyData()
        {
            using (var sr = new StreamReader(Request.InputStream))
            {
                string content = sr.ReadToEnd();

                if (!string.IsNullOrEmpty(content))
                {
                    return content;
                }

            }

            return null;

        }



        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

    }


}