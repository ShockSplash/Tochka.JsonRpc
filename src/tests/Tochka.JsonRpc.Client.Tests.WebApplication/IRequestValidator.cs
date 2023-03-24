﻿using System.Text.Json;
using Tochka.JsonRpc.TestUtils.Integration;

namespace Tochka.JsonRpc.Client.Tests.WebApplication;

public interface IRequestValidator
{
    void Validate(HttpRequest request, JsonDocument body);
    void Validate(TestData data);
    void Validate(object? data);
}
