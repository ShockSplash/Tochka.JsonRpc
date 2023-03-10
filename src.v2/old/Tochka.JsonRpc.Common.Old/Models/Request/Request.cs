using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.Common.Old.Models.Id;
using Tochka.JsonRpc.Common.Old.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Old.Serializers;

namespace Tochka.JsonRpc.Common.Old.Models.Request
{
    [ExcludeFromCodeCoverage]
    public class Request<TParams> : ICall<TParams>
    {
        public IRpcId Id { get; set; }

        public string Jsonrpc { get; set; } = JsonRpcConstants.Version;

        public string Method { get; set; }

        public TParams Params { get; set; }

        public IUntypedCall WithSerializedParams(IJsonRpcSerializer serializer)
        {
            var serializedParams = serializer.SerializeParams(Params);
            return new UntypedRequest
            {
                Id = Id,
                Method = Method,
                Params = serializedParams
            };
        }

        public override string ToString() => $"{nameof(Request<object>)}: {nameof(Method)} [{Method}], {nameof(Id)} [{Id}], {nameof(Params)} [{Params}]";
    }
}
