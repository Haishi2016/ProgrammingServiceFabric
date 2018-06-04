using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Common
{
    [DataContract]
    public class Shared2DArray<T>
        where T : IComparable
    {
        private int mSize;
        private T[,] mArray;
        private List<Proposal2D<T>> mProposals;
        private object mSyncRoot = new object();

        public Shared2DArray()
        {
        }

        public void Initialize(int size)
        {
            mSize = size;
            mArray = new T[size, size];
            mProposals = new List<Proposal2D<T>>();
        }

        public void Initialize(int size, string[] values)
        {
            Initialize(size);
            int index = 1;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    mArray[x, y] = (T)Convert.ChangeType(byte.Parse(values[index]), typeof(T));
                    index++;
                }
            }
        }

        public T this[int x, int y]
        {
            get
            {
                return mArray[x, y];
            }
        }

        public void Propose(IProposal proposal)
        {
            if (proposal == null)
                return;
            if (proposal is Proposal2D<T>)
            {
                lock (mSyncRoot)
                {
                    mProposals.Add((Proposal2D<T>)proposal);
                }
            }
            else
                throw new ArgumentException("Only Proposal2D < T > is supported.");
        }

        public void ResolveConflictsAndCommit(Action<IProposal> callback)
        {
            foreach (var proposal in mProposals)
            {
                var otherProposals = from p in mProposals
                                     where p.NewX == proposal.NewX
                                        && p.NewY == proposal.NewY
                                        && p.OldX != proposal.OldX
                                        && p.OldY != proposal.OldY
                                     select p;

                if (otherProposals.Count() == 0)
                {
                    mArray[proposal.OldX, proposal.OldY] = (T)Convert.ChangeType(0, typeof(T));
                    mArray[proposal.NewX, proposal.NewY] = proposal.ProposedValue;
                    callback?.Invoke(proposal);
                }
            }
            mProposals.Clear();
        }

        override public string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(mSize).Append(",");
            for (int y = 0; y < mSize; y++)
            {
                for (int x = 0; x < mSize; x++)
                    sb.Append(mArray[x, y]).Append(",");
            }
            return sb.ToString().Substring(0, sb.Length - 1);
        }

        public static Shared2DArray<T> FromString(string json)
        {
            Shared2DArray<T> ret = new Shared2DArray<T>();
            string[] parts = json.Split(',');
            int size = int.Parse(parts[0]);
            ret.Initialize(size, parts);
            return ret;
        }
    }
}
