using Tochka.JsonRpc.Common.Old.Models.Id;

namespace Tochka.JsonRpc.Client.Old.Services
{
    public interface IJsonRpcIdGenerator
    {
        IRpcId GenerateId();
    }
}
