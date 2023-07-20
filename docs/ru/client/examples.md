# Клиент/Примеры

Здесь представлены примеры для различных сценариев. Типичные подробности, вроде HTTP заголовков, конструкторов, реализаций настроек, регистрации сервисов в DI опущены для простоты.

> Для деталей о более продвинутом использовании смотрите страницу [Конфигурация](configuration)

## Запрос, Уведомление, Батч с настройками по умолчанию

Примеры базовых JSON-RPC вызовов с настройками по умолчанию

<details>
<summary>Развернуть</summary>

<table>
<tr>
    <td>
        Метод клиента
    </td>
    <td>
        Отправленный JSON-RPC вызов 
    </td>
</tr>

<tr>
<td valign="top">

Запрос
```cs
public async Task<Guid> CreateUser(string login, CancellationToken token)
{
    var response = await SendRequest("users.create", new CreateRequest(login), token);
    return response.GetResponseOrThrow<Guid>();
}

var response = await myClient.CreateUser("user_login", token);
```

</td>
<td>

```json
{
    "id": "56249f26-9748-461c-aeaf-b74b6a244ac6",
    "method": "users.create",
    "params": {
        "login": "user_login"
    },
    "jsonrpc": "2.0"
}
```

</td>
</tr>

<tr>
<td valign="top">

Уведомление
```cs
public async Task CreateUser(string login, CancellationToken token) =>
    await SendNotification("users.create", new CreateRequest(login), token);

await myClient.CreateUser("user_login", token);
```

</td>
<td>

```json
{
    "method": "users.create",
    "params": {
        "login": "user_login"
    },
    "jsonrpc": "2.0"
}
```

</td>
</tr>

<tr>
<td valign="top">

Батч
```cs
public async Task<Dictionary<string, Guid>> CreateUsers(IEnumerable<string> logins, CancellationToken token)
{
    var calls = logins.Select(l =>
            new Request<CreateRequest>(RpcIdGenerator.GenerateId(), "user.create", new(l)))
        .ToArray();
    var response = await SendBatch(calls, token);
    return calls.ToDictionary(static c => c.Params.Login, c => response.GetResponseOrThrow<Guid>(c.Id));
}

var response = await myClient.CreateUsers(new[] { "user_login1", "user_login2" }, token);
```

</td>
<td>

```json
[
    {
        "id": "8fc6020d-c9a7-4d9b-913a-6868580a5f72",
        "method": "users.create",
        "params": {
            "login": "user_login1"
        },
        "jsonrpc": "2.0"
    },
    {
        "id": "5c24149a-c6b3-47ba-babf-1e5ad774973d",
        "method": "users.create",
        "params": {
            "login": "user_login2"
        },
        "jsonrpc": "2.0"
    }
]
```

</td>
</tr>

</table>

</details>

## Получение сырых данных в ответе

Возврат сырых данных без десериализации и валидации

<details>
<summary>Развернуть</summary>

```cs
public async Task<byte[]> GetFile(string name, CancellationToken token)
{
    var call = new Request<GetFileRequest>(RpcIdGenerator.GenerateId(), "file.get", new(name));
    var response = await Send(call, token); // response имеет тип HttpResponseMessage
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsByteArrayAsync(token);
}
```

</details>

</details>

## Настройка HttpClient (например, заголовки авторизации)

Настройка внутреннего `HttpClient`, который используется для отправки запросов

<details>
<summary>Развернуть</summary>

```cs
public class MyJsonRpcClient
{
    public override string UserAgent => "User-Agent header value";
    protected override Encoding Encoding => Encoding.UTF32;

    public MyJsonRpcClient(HttpClient client, IOptions<MyJsonRpcClientOptions> options, IJsonRpcIdGenerator jsonRpcIdGenerator, ILogger<MyJsonRpcClient> logger)
        : base(client, options.Value, jsonRpcIdGenerator, logger)
    {
        var basicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes("login:password"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuth);
    }
}
```

</details>

## Настройка сериализации параметров и десериализации результата

Изменение настроек сериализации JSON для добавления другой политики именования или дополнительных конвертеров

<details>
<summary>Развернуть</summary>

Вы можете использовать одно из предоставленных в классе `JsonRpcSerializerOptions` значений или создать собственный объект `JsonSerializerOptions`.

> Эти настройки не повлияют на "заголовки" JSON-RPC (id, method, jsonrpc) - логика их сериализации настраивается через `HeadersJsonSerializerOptions` и изменять ее не рекомендуется!

```cs
public class MyJsonRpcClient
{
    public override JsonSerializerOptions DataJsonSerializerOptions => JsonRpcSerializerOptions.CamelCase;

    public MyJsonRpcClient(HttpClient client, IOptions<MyJsonRpcClientOptions> options, IJsonRpcIdGenerator jsonRpcIdGenerator, ILogger<MyJsonRpcClient> logger)
        : base(client, options.Value, jsonRpcIdGenerator, logger)
    {
    }
}
```

</details>

## Обработка ошибок

Обработка ошибок в ответах без выбрасывания исключений

<details>
<summary>Развернуть</summary>

```cs
public async Task<BusinessError?> GetError(CancellationToken token)
{
    var response = await SendRequest("error.get", new { }, token);
    if (!response.HasError())
    {
        return null;
    }

    var errorCode = response.AsAnyError().Code;
    if (errorCode != 123)
    {
        throw new ArgumentException($"Unexpected error code {errorCode}");
    }

    return response.AsTypedError<BusinessError>().Data;
}
```

</details>