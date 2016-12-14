using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kip.Utils.Payment
{
    public class PaymentOrder
    {
        public string OrderNo { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quota { get; set; }
        public string TradeNo { get; set; }

        public decimal TotalAmount
        {
            get { return this.Price * this.Quota; }
        }
    }
}
