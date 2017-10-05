using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculatorService.Interfaces
{
    public interface ICalculatorService: IService
    {
        Task<int> Add(int a, int b);
        Task<int> Subtract(int a, int b);
    }
}
