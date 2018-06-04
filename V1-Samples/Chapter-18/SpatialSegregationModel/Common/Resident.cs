using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public class Resident : IVirtualActor
    {
        ResidentState state;
        Shared2DArray<byte> sharedState;

        int mRange;
        int mId;
        private static Random mRand = new Random();
        private List<Tuple<int, int>> mOffsets = new List<Tuple<int, int>>
        {
            new Tuple<int, int>(-1,-1),
            new Tuple<int, int>(-1,0),
            new Tuple<int, int>(-1,1),
            new Tuple<int, int>(0,1),
            new Tuple<int, int>(0,-1),
            new Tuple<int, int>(1,-1),
            new Tuple<int, int>(1,0),
            new Tuple<int, int>(1,1)
        };

        public Resident(int range, int id, ResidentState state, Shared2DArray<byte>
        sharedState)
        {
            mRange = range;
            mId = id;
            this.state = state;
            this.sharedState = sharedState;
        }

        public Task ApproveProposalAsync(IProposal proposal)
        {
            if (proposal is Proposal2D<byte> p)
            {
                this.state.X = p.NewX;
                this.state.Y = p.NewY;
            }
            return Task.FromResult(1);
        }

        public Task<IProposal> ProposeAsync()
        {
            int count = 0;
            count += CountNeighbour(this.state.X - 1, this.state.Y);
            count += CountNeighbour(this.state.X - 1, this.state.Y - 1);
            count += CountNeighbour(this.state.X - 1, this.state.Y + 1);
            count += CountNeighbour(this.state.X, this.state.Y - 1);
            count += CountNeighbour(this.state.X, this.state.Y + 1);
            count += CountNeighbour(this.state.X + 1, this.state.Y - 1);
            count += CountNeighbour(this.state.X + 1, this.state.Y);
            count += CountNeighbour(this.state.X + 1, this.state.Y + 1);
            if (count <= 3)
            {
                var randList = mOffsets.OrderBy(p => mRand.Next());
                foreach (var item in randList)
                {
                    if (FindEmptyNeighbour(this.state.X + item.Item1, this.state.Y + item.
                    Item2))
                    {
                        return Task.FromResult<IProposal>(new Proposal2D<byte>(mId,
                        this.state.X,
                        this.state.Y,
                        this.state.X + item.Item1,
                        this.state.Y + item.Item2,
                        this.state.Tag));
                    }
                }
            }
            return Task.FromResult<IProposal>(null);
        }

        private int CountNeighbour(int x, int y)
        {
            if (x >= 0 && x < this.mRange && y >= 0 && y < this.mRange)
                return sharedState[x, y] == this.state.Tag ? 1 : 0;
            else
                return 0;
        }

        private bool FindEmptyNeighbour(int x, int y)
        {
            if (x >= 0 && x < this.mRange && y >= 0 && y < this.mRange)
                return sharedState[x, y] == 0;
            else
                return false;
        }
    }
}