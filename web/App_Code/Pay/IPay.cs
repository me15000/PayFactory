using System;
using System.Collections.Generic;
using System.Collections.Specialized;


namespace Pay
{

    public enum ResultStatus { Succ,Fail }
    public enum PayResultType { Url,Html }
    public class PayResult
    {
        public ResultStatus Status { get; set; }
        public Dictionary<string, object> Data { get; set; }
        public PayResultType ResultType { get; set; }
        public object ResultData { get; set; }
    }


    /// <summary>
    /// PayInfo 的摘要说明
    /// </summary>
    public class PayInfo
    {
        public string OrderNo { get; set; }
        public string IP { get; set; }
        public string Name { get; set; }
        public int Amount { get; set; }
        public string Attach { get; set; }
        public string ReturnUrl { get; set; }
    }

    
    public class CallbackInfo
    {
        public ResultStatus Status { get; set; }
        public string OrderNo { get; set; }
        public string AttachData { get; set; }
        public Dictionary<string, object> Data { get; set; }

    }

    /// <summary>
    /// IPay 的摘要说明
    /// </summary>
    public interface IPay
    {

        PayResult Pay(PayInfo info);

        bool DoCallback(NameValueCollection query,NameValueCollection form,string bodydata,Action<CallbackInfo> action);

        void EchoStatus(ResultStatus status);
    }

}