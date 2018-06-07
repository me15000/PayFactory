using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
namespace Services
{

    public enum OrderStatus
    {
        [DefaultValue("begin")]
        Begin,

        [DefaultValue("succ")]
        Succ,

        [DefaultValue("fail")]
        Fail,

        [DefaultValue("unknown")]
        UnKnown
    }

    public static class ExtClass
    {
        public static string GetStringValue(this Enum status)
        {
            return GetValue<string>(status);
        }

        public static T GetValue<T>(this Enum status)
        {
            Type type = status.GetType();
            string name = Enum.GetName(type, status);
            var field = type.GetField(name);
            var attribute = Attribute.GetCustomAttribute(field, typeof(DefaultValueAttribute)) as DefaultValueAttribute;
            var val = attribute == null ? null : attribute.Value;

            return (T)(Convert.ChangeType(val, typeof(T)));
        }

        public static OrderStatus GetOrderStatus(string val)
        {
            foreach (OrderStatus item in Enum.GetValues(typeof(OrderStatus)))
            {
                if (item.GetStringValue().Equals(val))
                {
                    return item;
                }
            }

            return OrderStatus.UnKnown;
        }
    }

    /// <summary>
    /// OrderService 的摘要说明
    /// </summary>
    public class OrderService
    {
        public OrderService()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }

        string genOrderNo(string ch)
        {
            string abc = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            int abclen = abc.Length;
            var rnd = new Random();

            string chars = string.Empty;
            for (int i = 0; i < 3; i++)
            {
                chars += abc[rnd.Next(0, abclen)];
            }

            string ticks = ((DateTime.Now.Ticks - 636619107902566515L) / 100000L).ToString();

            return ch + ticks + chars + rnd.Next(100, 999);
        }

        public bool ExistsOrderNo(string orderno)
        {
            var db = Common.DB.Factory.CreateDBHelper();

            return ExistsOrderNo(orderno, db);
        }

        public bool ExistsOrderNo(string orderno, Common.DB.IDBHelper db)
        {

            return db.Exists("select top 1 1 from [orderinfo] where orderno=@0", orderno);
        }

        public string genNotExistOrerNo(string project)
        {
            var db = Common.DB.Factory.CreateDBHelper();

            string orderno = null;
            do
            {
                orderno = genOrderNo(project);

            } while (ExistsOrderNo(orderno, db));

            return orderno;
        }

        public OrderStatus QueryOrderStatus(string orderno)
        {
            var statusInfo = OrderStatus.UnKnown;
            var db = Common.DB.Factory.CreateDBHelper();
            string status = db.ExecuteScalar<string>("select status from [orderinfo] where orderno=@0", orderno);

            if (!string.IsNullOrEmpty(status))
            {
                statusInfo = ExtClass.GetOrderStatus(status);
            }

            return statusInfo;
        }

        public void UpdateCallbackData(string orderno, string data)
        {
            var db = Common.DB.Factory.CreateDBHelper();

            db.ExecuteNoneQuery("update [orderinfo] set callbackdata=@1,callbackdate=@2 where orderno=@0", orderno, data, DateTime.Now);
        }


        public OrderInfo GetOrderInfo(string orderno)
        {
            var db = Common.DB.Factory.CreateDBHelper();

            var data = db.GetData("select amount,orderno,paych,paytype,project from [orderinfo] where orderno=@0", orderno);
            if (data == null)
            {
                return null;
            }

            OrderInfo info = new OrderInfo();

            info.Amount = Convert.ToInt32(data["amount"]);
            info.OrderNo = orderno;
            info.PayCH = data["paych"] as string ?? string.Empty;
            info.PayType = data["paytype"] as string ?? string.Empty;
            info.Project = data["project"] as string ?? string.Empty;

            return info;

        }

        public bool UpdateOrderStatus(string orderno, OrderStatus status)
        {
            var db = Common.DB.Factory.CreateDBHelper();
            int n = db.ExecuteNoneQuery("update [orderinfo] set status=@1 where orderno=@0", orderno, status.GetStringValue());

            if (n > 0)
            {
                return true;
            }

            return false;
        }

        public bool InitOrder(OrderInfo info)
        {
            var db = Common.DB.Factory.CreateDBHelper();

            if (ExistsOrderNo(info.OrderNo, db))
            {
                return false;
            }

            DateTime now = DateTime.Now;

            var nvc = new Common.DB.NVCollection();
            nvc["orderno"] = info.OrderNo;
            nvc["project"] = info.Project;
            nvc["status"] = OrderStatus.Begin.GetStringValue();
            nvc["date"] = now;
            nvc["created"] = now;
            nvc["amount"] = info.Amount;
            nvc["paytype"] = info.PayType;
            nvc["paych"] = info.PayCH;


            int num = db.ExecuteNoneQuery("insert into [orderinfo](orderno,project,status,date,created,amount,paytype,paych) values(@orderno,@project,@status,@date,@created,@amount,@paytype,@paych)", nvc);

            if (num > 0)
            {
                return true;
            }

            return false;

        }
    }

}