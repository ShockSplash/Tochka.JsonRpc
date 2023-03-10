using Tochka.JsonRpc.Client.Settings;

namespace Tochka.JsonRpc.Client.Benchmarks;

public class NewJsonRpcClientOptions : JsonRpcClientOptionsBase
{
    public override string Url { get; set; } = Constants.BaseUrl;
}
