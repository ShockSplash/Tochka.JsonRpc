using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using RichardSzalay.MockHttp;
using Tochka.JsonRpc.TestUtils;

namespace Tochka.JsonRpc.Client.Benchmarks;

[MemoryDiagnoser]
public class SendNotificationBenchmark
{
    [ParamsSource(nameof(DataValues))]
    public TestData Data { get; set; }

    private MockHttpMessageHandler handlerMock;
    private OldJsonRpcClient oldClient;
    private NewJsonRpcClient newClient;

    [GlobalSetup]
    public void Setup()
    {
        handlerMock = new MockHttpMessageHandler();
        handlerMock.When("*").Respond(HttpStatusCode.OK);
        oldClient = new OldJsonRpcClient(handlerMock.ToHttpClient());
        newClient = new NewJsonRpcClient(handlerMock.ToHttpClient());
    }

    [Benchmark(Baseline = true)]
    public async Task Old() => await oldClient.SendNotification(Method, Data, CancellationToken.None);

    [Benchmark]
    public async Task New() => await newClient.SendNotification(Method, Data, CancellationToken.None);

    public static IEnumerable<TestData> DataValues => new[]
    {
        TestData.Big,
        TestData.Nested,
        TestData.Plain
    };

    private const string Method = "method";
}
