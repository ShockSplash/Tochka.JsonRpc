namespace Tochka.JsonRpc.Client.Models;

public class Notification<TParams> : ICall<TParams>
{
    public string Jsonrpc { get; set; }
    public string Method { get; set; }
    public TParams Params { get; set; }

    public IUntypedCall WithSerializedParams(IJsonRpcSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
