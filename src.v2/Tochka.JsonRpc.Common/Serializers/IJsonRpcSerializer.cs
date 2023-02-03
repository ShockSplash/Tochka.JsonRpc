using System.Text.Json;

namespace Tochka.JsonRpc.Common.Serializers
{
    public interface IJsonRpcSerializer
    {
        JsonSerializerOptions Settings { get; }
        // JsonSerializer Serializer { get; }
    }
}
