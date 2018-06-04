using System.ServiceModel;
using System.Threading.Tasks;

namespace CalculatorServiceWCF
{
    [ServiceContract]
    public interface ICalculatorServiceWCF
    {
        [OperationContract]
        Task<string> Add(int a, int b);

        [OperationContract]
        Task<string> Subtract(int a, int b);
    }
}
