using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common
{
    [ServiceContract]
    public interface IShoppingCartService
    {
        [OperationContract]
        Task AddItem(ShoppingCartItem item);
        [OperationContract]
        Task DeleteItem(string productName);
        [OperationContract]
        Task<List<ShoppingCartItem>> GetItems();
    }
}