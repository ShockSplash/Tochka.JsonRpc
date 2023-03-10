using System.Net.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Tochka.JsonRpc.Client.Services;
using Tochka.JsonRpc.Common.Serializers;

namespace Tochka.JsonRpc.Client.Benchmarks;

public class NewJsonRpcClient : JsonRpcClientBase
{
    public NewJsonRpcClient(HttpClient client) : base(client, new CamelCaseJsonRpcSerializer(), new HeaderJsonRpcSerializer(), new NewJsonRpcClientOptions(), new JsonRpcIdGenerator(Mock.Of<ILogger<JsonRpcIdGenerator>>()), Mock.Of<ILogger<OldJsonRpcClient>>())
    {
    }
}
