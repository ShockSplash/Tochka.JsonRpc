namespace Tochka.JsonRpc.Client.Models;

public class Request<TParams> : ICall<TParams>
{
    public IRpcId Id { get; set; }
    public string Jsonrpc { get; set; }
    public string Method { get; set; }
    public TParams Params { get; set; }

    public IUntypedCall WithSerializedParams(IJsonRpcSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
