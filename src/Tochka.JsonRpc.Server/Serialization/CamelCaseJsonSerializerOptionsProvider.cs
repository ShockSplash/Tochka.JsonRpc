﻿using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using JetBrains.Annotations;
using Tochka.JsonRpc.Common;

namespace Tochka.JsonRpc.Server.Serialization;

/// <inheritdoc />
/// <summary>
/// <see cref="IJsonSerializerOptionsProvider" /> with <see cref="JsonRpcSerializerOptions.CamelCase" /> options
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public class CamelCaseJsonSerializerOptionsProvider : IJsonSerializerOptionsProvider
{
    /// <inheritdoc />
    /// <summary>
    ///     <see cref="JsonRpcSerializerOptions.CamelCase" /> <see cref="JsonSerializerOptions" />
    /// </summary>
    public JsonSerializerOptions Options => JsonRpcSerializerOptions.CamelCase;
}
