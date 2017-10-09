using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Calculator;
using Grpc.Core;

namespace gRPCServer
{
    public class CalculatorImpl: Calculator.Calculator.CalculatorBase
    {
        private string mReplicaId;
        public CalculatorImpl(string replicaId)
        {
            mReplicaId = replicaId;
        }
        public override Task<CalculateReply> Add(CalculateRequest request, ServerCallContext context)
        {
            return Task.FromResult(new CalculateReply { ReplicaId = mReplicaId, Result = request.A + request.B });
        }
        public override Task<CalculateReply> Subtract(CalculateRequest request, ServerCallContext context)
        {
            return Task.FromResult(new CalculateReply { ReplicaId = mReplicaId, Result = request.A - request.B });
        }
    }
}
