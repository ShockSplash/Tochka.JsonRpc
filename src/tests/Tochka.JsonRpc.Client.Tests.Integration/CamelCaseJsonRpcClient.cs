﻿using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Tochka.JsonRpc.Client.Services;
using Tochka.JsonRpc.Common;

namespace Tochka.JsonRpc.Client.Tests.Integration;

internal class CamelCaseJsonRpcClient : JsonRpcClientBase
{
    public CamelCaseJsonRpcClient(HttpClient client, IJsonRpcIdGenerator jsonRpcIdGenerator) : base(client, new SimpleJsonRpcClientOptions(), jsonRpcIdGenerator, Mock.Of<ILogger>())
    {
    }

    public override JsonSerializerOptions DataJsonSerializerOptions => JsonRpcSerializerOptions.CamelCase;
}
