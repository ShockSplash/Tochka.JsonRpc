using System.Net.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Tochka.JsonRpc.Client.Old.Services;
using Tochka.JsonRpc.Common.Old.Serializers;

namespace Tochka.JsonRpc.Client.Benchmarks;

public class OldJsonRpcClient : Old.JsonRpcClientBase
{
    public OldJsonRpcClient(HttpClient client) : base(client, new CamelCaseJsonRpcSerializer(), new HeaderJsonRpcSerializer(), new OldJsonRpcClientOptions(), new JsonRpcIdGenerator(Mock.Of<ILogger<JsonRpcIdGenerator>>()), Mock.Of<ILogger<OldJsonRpcClient>>())
    {
    }
}
