using Newtonsoft.Json;

namespace Tochka.JsonRpc.Client;

internal class HeaderJsonRpcSerializer : IJsonRpcSerializer
{
    public JsonSerializerSettings Settings { get; }
    public JsonSerializer Serializer { get; }
}
