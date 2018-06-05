using System;
using System.Reflection;

namespace Pay
{






    /// <summary>
    /// PayFactory 的摘要说明
    /// </summary>
    public class PayFactory
    {
        public static IPay Create(string configKey)
        {
            var config = Config.GetPayConfig(configKey);

            return Create(config);
        }


        public static IPay Create(PayConfig config)
        {

            Assembly assembly = Assembly.GetExecutingAssembly();

            var type = assembly.GetType(config.ClientType);

            var payclient = Activator.CreateInstance(type, config) as IPay;

            if (payclient != null)
            {
                return payclient;
            }
            else
            {

                return null;
            }

            /*
            string key = config.ClientType.ToLower();

            switch (key)
            {
                case "pay.wxpay.payclient":
                    return new WXPay.PayClient(config);
                case "pay.alipay.payclient":
                    return new Alipay.PayClient(config);

                case "pay.sftpay.payclient":
                    return new SFTPay.PayClient(config);

                case "pay.haibeipay.payclient":
                    return new HaiBeiPay.PayClient(config);

            }

            return null;
            */
        }
    }

}