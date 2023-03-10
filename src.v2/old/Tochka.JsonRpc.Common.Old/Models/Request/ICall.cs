using Tochka.JsonRpc.Common.Old.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Old.Serializers;

namespace Tochka.JsonRpc.Common.Old.Models.Request
{
    public interface ICall
    {
        string Jsonrpc { get; set; }

        string Method { get; set; }

        IUntypedCall WithSerializedParams(IJsonRpcSerializer serializer);
    }

    public interface ICall<TParams> : ICall
    {
        TParams Params { get; set; }
    }
}
