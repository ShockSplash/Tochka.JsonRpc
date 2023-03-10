using System.Text.Json;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;
using Tochka.JsonRpc.Common.Serializers;

namespace Tochka.JsonRpc.Client.Models
{
    internal class SingleJsonRpcResult : ISingleJsonRpcResult
    {
        private readonly IJsonRpcCallContext context;
        private readonly HeaderJsonRpcSerializer headerJsonRpcSerializer;
        private readonly IJsonRpcSerializer serializer;
        private readonly IResponse? response;

        public SingleJsonRpcResult(IJsonRpcCallContext context, HeaderJsonRpcSerializer headerJsonRpcSerializer, IJsonRpcSerializer serializer)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            if (context.BatchResponse != null)
            {
                throw new ArgumentOutOfRangeException(nameof(context), "Expected single response");
            }

            response = context.SingleResponse;
            this.headerJsonRpcSerializer = headerJsonRpcSerializer;
            this.serializer = serializer;
        }

        public T? GetResponseOrThrow<T>()
        {
            switch (response)
            {
                case null:
                    throw new JsonRpcException($"Expected successful response with [{typeof(T).Name}] params, got nothing", context);
                case UntypedResponse untypedResponse:
                    return untypedResponse.Result.Deserialize<T>(serializer.Settings);
                case UntypedErrorResponse untypedErrorResponse:
                    context.WithError(untypedErrorResponse);
                    throw new JsonRpcException($"Expected successful response with [{typeof(T).Name}] params, got error", context);
                default:
                    throw new ArgumentOutOfRangeException(nameof(response), response.GetType().Name);
            }
        }

        public T? AsResponse<T>() => response switch
        {
            UntypedResponse untypedResponse => untypedResponse.Result.Deserialize<T>(serializer.Settings),
            _ => default
        };

        public bool HasError() => response is UntypedErrorResponse;

        public Error<JsonDocument>? AsAnyError() => response switch
        {
            UntypedErrorResponse untypedErrorResponse => untypedErrorResponse.Error,
            _ => null
        };

        public Error<T>? AsTypedError<T>() => response switch
        {
            UntypedErrorResponse untypedErrorResponse => new Error<T>()
            {
                Code = untypedErrorResponse.Error.Code,
                Message = untypedErrorResponse.Error.Message,
                Data = GetData<T>(untypedErrorResponse.Error)
            },
            _ => null
        };

        private T? GetData<T>(Error<JsonDocument> error)
        {
            if (error.Data == null)
            {
                // if data was not present at all, do not throw
                return default;
            }

            var data = error.Data.Deserialize<T>(serializer.Settings);
            if (data.Equals(default(T))) // ask Rast - что за ошибка?
            {
                // if user serializer failed: maybe this is server error, try header serializer
                data = error.Data.Deserialize<T>(headerJsonRpcSerializer.Settings);
            }

            return data;
        }

        public Error<ExceptionInfo>? AsErrorWithExceptionInfo() => AsTypedError<ExceptionInfo>();
    }
}
