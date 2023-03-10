using System;
using FluentAssertions;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Untyped;

namespace Tochka.JsonRpc.Common.Tests.Models
{
    public class ModelTests
    {
        [Test]
        public void Test_ErrorResponse_HasDefaultVersion()
        {
            var value = new ErrorResponse<object>();
            value.Jsonrpc.Should().Be(JsonRpcConstants.Version);
        }

        [Test]
        public void Test_FormattedErrorResponse_HasDefaultVersion()
        {
            var value = new UntypedErrorResponse();
            value.Jsonrpc.Should().Be(JsonRpcConstants.Version);
        }

        [Test]
        public void Test_FormattedResponse_HasDefaultVersion()
        {
            var value = new UntypedResponse();
            value.Jsonrpc.Should().Be(JsonRpcConstants.Version);
        }

        [Test]
        public void Test_Response_HasDefaultVersion()
        {
            var value = new Response<int>();
            value.Jsonrpc.Should().Be(JsonRpcConstants.Version);
        }

        [Test]
        public void Test_StringRpcId_AllowsEmpty()
        {
            var value = new StringRpcId(string.Empty);
            value.StringValue.Should().Be(string.Empty);
        }

        [Test]
        public void Test_StringRpcId_ThrowsOnNull()
        {
            var action = () => new StringRpcId(null);
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Test_Request_HasDefaultVersion()
        {
            var value = new Request<int>();
            value.Jsonrpc.Should().Be(JsonRpcConstants.Version);
        }

        [Test]
        public void Test_Notification_HasDefaultVersion()
        {
            var value = new Notification<int>();
            value.Jsonrpc.Should().Be(JsonRpcConstants.Version);
        }

        [Test]
        public void Test_UntypedRequest_HasDefaultVersion()
        {
            var value = new UntypedRequest();
            value.Jsonrpc.Should().Be(JsonRpcConstants.Version);
        }

        [Test]
        public void Test_UntypedNotification_HasDefaultVersion()
        {
            var value = new UntypedNotification();
            value.Jsonrpc.Should().Be(JsonRpcConstants.Version);
        }
    }
}
