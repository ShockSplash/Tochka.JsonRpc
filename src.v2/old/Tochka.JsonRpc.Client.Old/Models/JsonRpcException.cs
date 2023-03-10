using System;
using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Client.Old.Models
{
    [ExcludeFromCodeCoverage]
    public class JsonRpcException : Exception
    {
        public IJsonRpcCallContext Context { get; }

        public override string Message => $"{base.Message}{Environment.NewLine}{Context}";

        // for easy mocking
        public JsonRpcException()
        {
        }

        public JsonRpcException(string message, IJsonRpcCallContext context) : base(message) => Context = context;
    }
}
