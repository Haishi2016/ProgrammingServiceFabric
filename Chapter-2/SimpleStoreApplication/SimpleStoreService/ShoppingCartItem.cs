using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace SimpleStoreService
{
    public class ShoppingCartItem
    {
        public string ProductName { get; set; }
        public double UnitPrice { get; set; }
        public int Quantity { get; set; }
        public double LineTotal
        {
            get
            {
                return Quantity * UnitPrice;
            }
        }
    }
}
