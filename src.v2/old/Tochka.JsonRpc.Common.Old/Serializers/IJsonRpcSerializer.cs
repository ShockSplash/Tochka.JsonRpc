using Newtonsoft.Json;

namespace Tochka.JsonRpc.Common.Old.Serializers
{
    public interface IJsonRpcSerializer
    {
        JsonSerializerSettings Settings { get; }
        JsonSerializer Serializer { get; }
    }
}
