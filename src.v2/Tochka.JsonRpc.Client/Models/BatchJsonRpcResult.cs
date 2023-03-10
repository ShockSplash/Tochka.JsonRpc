using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;
using Tochka.JsonRpc.Common.Serializers;

namespace Tochka.JsonRpc.Client.Models
{
    internal class BatchJsonRpcResult : IBatchJsonRpcResult
    {
        private readonly IJsonRpcCallContext context;
        private readonly HeaderJsonRpcSerializer headerJsonRpcSerializer;
        private readonly IJsonRpcSerializer serializer;
        private readonly Dictionary<IRpcId, IResponse> responses;

        public BatchJsonRpcResult(IJsonRpcCallContext context, HeaderJsonRpcSerializer headerJsonRpcSerializer, IJsonRpcSerializer serializer)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            if (context.SingleResponse != null)
            {
                throw new ArgumentOutOfRangeException(nameof(context), "Expected batch response");
            }

            responses = CreateDictionary(context.BatchResponse);
            this.headerJsonRpcSerializer = headerJsonRpcSerializer;
            this.serializer = serializer;
        }

        private static Dictionary<IRpcId, IResponse> CreateDictionary(IEnumerable<IResponse>? items) =>
            items?.ToDictionary(static x => x.Id ?? NullId, static x => x) ?? new Dictionary<IRpcId, IResponse>();

        public T? GetResponseOrThrow<T>(IRpcId? id)
        {
            if (!TryGetValue(id, out var response))
            {
                throw new JsonRpcException($"Expected successful response id [{id}] with [{typeof(T).Name}] params, got nothing", context);
            }

            switch (response)
            {
                case UntypedResponse untypedResponse:
                    return untypedResponse.Result.Deserialize<T>(serializer.Settings);
                case UntypedErrorResponse untypedErrorResponse:
                    context.WithError(untypedErrorResponse);
                    throw new JsonRpcException($"Expected successful response id [{id}] with [{typeof(T).Name}] params, got error", context);
                default:
                    throw new ArgumentOutOfRangeException(nameof(response), response.GetType().Name);
            }
        }

        public T? AsResponse<T>(IRpcId? id)
        {
            TryGetValue(id, out var response);
            return response switch
            {
                UntypedResponse untypedResponse => untypedResponse.Result.Deserialize<T>(serializer.Settings),
                _ => default
            };
        }

        public bool HasError(IRpcId? id)
        {
            if (!TryGetValue(id, out var response))
            {
                throw new JsonRpcException($"Expected response id [{id}], got nothing", context);
            }

            return response is UntypedErrorResponse;
        }

        public Error<JsonDocument>? AsAnyError(IRpcId? id)
        {
            TryGetValue(id, out var response);
            return response switch
            {
                UntypedErrorResponse untypedErrorResponse => untypedErrorResponse.Error,
                _ => null
            };
        }

        public Error<T>? AsTypedError<T>(IRpcId? id)
        {
            TryGetValue(id, out var response);
            return response switch
            {
                UntypedErrorResponse untypedErrorResponse => new Error<T>
                {
                    Code = untypedErrorResponse.Error.Code,
                    Message = untypedErrorResponse.Error.Message,
                    Data = GetData<T>(untypedErrorResponse.Error)
                },
                _ => null
            };
        }

        public Error<ExceptionInfo>? AsErrorWithExceptionInfo(IRpcId? id) => AsTypedError<ExceptionInfo>(id);

        private bool TryGetValue(IRpcId? id, [NotNullWhen(true)] out IResponse? response) =>
            responses.TryGetValue(id ?? NullId, out response);

        private T? GetData<T>(Error<JsonDocument> error)
        {
            if (error.Data == null)
            {
                // if data was not present at all, do not throw
                return default;
            }

            var data = error.Data.Deserialize<T>(serializer.Settings);
            if (data.Equals(default(T))) // ask Rast - что за серверная ошибка?
            {
                // if user serializer failed: maybe this is server error, try header serializer
                data = error.Data.Deserialize<T>(headerJsonRpcSerializer.Settings);
            }

            return data;
        }

        /// <summary>
        /// Dummy value for storing responses in dictionary
        /// </summary>
        private static readonly IRpcId NullId = new NullRpcId();

        /// <inheritdoc cref="Tochka.JsonRpc.Common.Models.Id.IRpcId" />
        /// <summary>
        /// Dummy id type for storing responses in dictionary
        /// </summary>
        private class NullRpcId : IRpcId, IEquatable<NullRpcId>
        {
            public bool Equals(NullRpcId? other) => !ReferenceEquals(null, other); // равен всему кроме null? ask Rast

            public bool Equals(IRpcId? other) => Equals(other as NullRpcId);

            public override bool Equals(object? obj) => Equals(obj as NullRpcId);
        }
    }
}
