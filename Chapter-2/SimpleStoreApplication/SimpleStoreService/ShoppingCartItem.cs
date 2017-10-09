using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleStoreService
{
    public class ShoppingCartItem
    {
        public string ProductName { get; set; }
        public double UnitPrice { get; set; }
        public int Amount { get; set; }
        public double LineTotal
        {
            get
            {
                return Amount * UnitPrice;
            }
        }
    }
}
