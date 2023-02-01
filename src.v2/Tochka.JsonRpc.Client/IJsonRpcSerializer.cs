using Newtonsoft.Json;

namespace Tochka.JsonRpc.Client;

public interface IJsonRpcSerializer
{
    JsonSerializerSettings Settings { get; }
    JsonSerializer Serializer { get; }
}
