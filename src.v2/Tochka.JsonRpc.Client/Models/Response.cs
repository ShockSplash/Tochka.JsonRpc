namespace Tochka.JsonRpc.Client.Models;

public class Response<TResult> : IResponse
{
    public IRpcId Id { get; set; }

    /// <summary>
    ///
    /// </summary>
    public string Jsonrpc { get; set; } = JsonRpcConstants.Version;

    public TResult Result { get; set; }

    public override string ToString() => $"{nameof(Request<object>)}<{typeof(TResult).Name}>: {nameof(Id)} [{Id}], {nameof(Result)} [{Result}]";
}
