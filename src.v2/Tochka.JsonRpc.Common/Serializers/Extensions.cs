using System.Text.Json;

namespace Tochka.JsonRpc.Common.Serializers
{
    public static class Extensions
    {
        public static JsonDocument SerializeParams<T>(this IJsonRpcSerializer serializer, T data)
        {
            if (data == null)
            {
                return null;
            }

            var serialized = JsonSerializer.SerializeToDocument(data, serializer.Settings);
            var jsonValueKind = serialized.RootElement.ValueKind;
            if (jsonValueKind is JsonValueKind.Object or JsonValueKind.Array)
            {
                return serialized;
            }

            throw new InvalidOperationException($"Expected params [{typeof(T).Name}] to be serializable into object or array, got [{jsonValueKind}]");
        }
    }
}
