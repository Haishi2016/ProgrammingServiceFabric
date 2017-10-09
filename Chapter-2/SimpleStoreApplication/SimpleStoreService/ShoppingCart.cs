using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleStoreService
{
    public class ShoppingCart
    {
        private List<ShoppingCartItem> mItems;
        public IEnumerable<ShoppingCartItem> GetItems()
        {
            return mItems.AsEnumerable<ShoppingCartItem>();
        }
        public ShoppingCart()
        {
            mItems = new List<ShoppingCartItem>();
        }
        public ShoppingCart(ShoppingCartItem item):this()
        {
            this.AddItem(item);
        }
        public double Total
        {
            get
            {
                return mItems.Sum(i => i.LineTotal);
            }
        }
        public ShoppingCart AddItem(ShoppingCartItem newItem)
        {
            var existingItem = mItems.FirstOrDefault(i => i.ProductName == newItem.ProductName);
            if (existingItem != null)
                existingItem.Quantity += newItem.Quantity;
            else 
                mItems.Add(newItem);
            return this;
        }
        public ShoppingCart RemoveItem(string productName)
        {
            var existingItem = mItems.FirstOrDefault(i => i.ProductName == productName);
            if (existingItem != null)
                mItems.Remove(existingItem);
            return this;
        }
    }
}
