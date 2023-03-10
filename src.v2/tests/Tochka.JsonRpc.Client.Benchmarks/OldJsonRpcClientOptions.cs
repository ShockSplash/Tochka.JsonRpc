using Tochka.JsonRpc.Client.Old.Settings;

namespace Tochka.JsonRpc.Client.Benchmarks;

public class OldJsonRpcClientOptions : JsonRpcClientOptionsBase
{
    public override string Url { get; set; } = Constants.BaseUrl;
}
