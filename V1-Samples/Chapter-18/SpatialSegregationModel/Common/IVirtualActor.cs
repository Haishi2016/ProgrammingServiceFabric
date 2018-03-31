using System.Threading.Tasks;

namespace Common
{
    public interface IVirtualActor
    {
        Task<IProposal> ProposeAsync();
        Task ApproveProposalAsync(IProposal proposal);
    }
}
