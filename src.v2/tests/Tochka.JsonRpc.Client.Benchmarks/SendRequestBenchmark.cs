using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using RichardSzalay.MockHttp;
using Tochka.JsonRpc.TestUtils;
using OldId = Tochka.JsonRpc.Common.Old.Models.Id.StringRpcId;
using NewId = Tochka.JsonRpc.Common.Models.Id.StringRpcId;

namespace Tochka.JsonRpc.Client.Benchmarks;

[MemoryDiagnoser]
public class SendRequestBenchmark
{
    [ParamsSource(nameof(DataValues))]
    public TestData Data { get; set; }

    [ParamsSource(nameof(ResponseValues))]
    public string Response { get; set; }

    public static IEnumerable<TestData> DataValues => new[]
    {
        TestData.Big,
        TestData.Nested,
        TestData.Plain
    };

    public static IEnumerable<string> ResponseValues => new[]
    {
        "big",
        "nested",
        "plain"
    };

    private readonly OldId oldId = new(Id.ToString());
    private readonly NewId newId = new(Id.ToString());
    private MockHttpMessageHandler handlerMock;
    private OldJsonRpcClient oldClient;
    private NewJsonRpcClient newClient;

    [GlobalSetup]
    public void Setup()
    {
        handlerMock = new MockHttpMessageHandler();
        handlerMock.When($"{Constants.BaseUrl}big")
            .Respond(_ => new StringContent(Responses.GetBigResponse(Id), Encoding.UTF8, "application/json"));
        handlerMock.When($"{Constants.BaseUrl}nested")
            .Respond(_ => new StringContent(Responses.GetNestedResponse(Id), Encoding.UTF8, "application/json"));
        handlerMock.When($"{Constants.BaseUrl}plain")
            .Respond(_ => new StringContent(Responses.GetPlainResponse(Id), Encoding.UTF8, "application/json"));
        oldClient = new OldJsonRpcClient(handlerMock.ToHttpClient());
        newClient = new NewJsonRpcClient(handlerMock.ToHttpClient());
    }

    [Benchmark(Baseline = true)]
    public async Task<TestData> Old()
    {
        var response = await oldClient.SendRequest(Response, oldId, Method, Data, CancellationToken.None);
        return response.GetResponseOrThrow<TestData>();
    }

    [Benchmark]
    public async Task<TestData?> New()
    {
        var response = await newClient.SendRequest(Response, newId, Method, Data, CancellationToken.None);
        return response.GetResponseOrThrow<TestData>();
    }

    private const string Method = "method";

    private static readonly Guid Id = Guid.NewGuid();
}
