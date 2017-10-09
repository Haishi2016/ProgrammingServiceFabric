using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleStoreService
{
    public class ShoppingCart
    {
        public List<ShoppingCartItem> Items { get; private set; }
        public ShoppingCart()
        {
            Items = new List<ShoppingCartItem>();
        }
        public double Total
        {
            get
            {
                return Items.Sum(i => i.Amount);
            }
        }
        public void AddItem(ShoppingCartItem newItem)
        {
            var existingItem = Items.FirstOrDefault(i => i.ProductName == newItem.ProductName);
            if (existingItem != null)
                Items.Remove(existingItem);
            Items.Add(newItem);
        }
    }
}
