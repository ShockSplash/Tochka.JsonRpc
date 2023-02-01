namespace Tochka.JsonRpc.Client.Models;

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
