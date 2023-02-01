using Tochka.JsonRpc.Client.Models;

namespace Tochka.JsonRpc.Client.Services;

public interface IJsonRpcIdGenerator
{
    IRpcId GenerateId();
}
