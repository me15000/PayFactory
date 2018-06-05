using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace Services
{


    /// <summary>
    /// OrderInfo 的摘要说明
    /// </summary>
    public class OrderInfo
    {
        public string OrderNo { get; set; }
        public string Project { get; set; }
        public int Amount { get; set; }
        public string PayType { get; set; }
        public string PayCH { get; set; }
    }

}