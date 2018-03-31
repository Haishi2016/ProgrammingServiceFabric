using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Box.Interfaces
{
    public interface IBox: IService
    {
        Task<bool> TryPickUpWoodChipAsync(int x, int y);
        Task<bool> TryPutDownWoodChipAsync(int x, int y);
        Task<List<int>> ReadBox();
        Task ResetBox();
    }
}
