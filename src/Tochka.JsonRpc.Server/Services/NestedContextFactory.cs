using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Request.Untyped;

namespace Tochka.JsonRpc.Server.Services
{
    /// <summary>
    /// Clone HTTP Context, overriding required data
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class NestedContextFactory : INestedContextFactory
    {
        private readonly IHttpContextFactory httpContextFactory;
        private readonly ILogger log;

        public NestedContextFactory(IHttpContextFactory httpContextFactory, ILogger<NestedContextFactory> log)
        {
            this.httpContextFactory = httpContextFactory;
            this.log = log;
        }

        public HttpContext Create(HttpContext context, IUntypedCall call, Encoding requestEncoding)
        {
            var newFeatures = new FeatureCollection(context.Features);
            OverrideRequestDataFeatures(newFeatures, context.Features, call, requestEncoding);
            // NOTE: factory breaks HttpContextAccessor
            var result = httpContextFactory.Create(newFeatures);
            log.LogTrace($"Created HttpContext [{result}]");
            return result;
        }

        protected virtual void OverrideRequestDataFeatures(IFeatureCollection newFeatures, IFeatureCollection originalFeatures, IUntypedCall call, Encoding requestEncoding)
        {
            var itemsFeature = CreateItems(originalFeatures.Get<IItemsFeature>());
            itemsFeature.Items[JsonRpcConstants.RequestItemKey] = call;
            itemsFeature.Items[JsonRpcConstants.NestedPipelineItemKey] = true;
            newFeatures.Set(itemsFeature);
            var requestFeature = CreateRequest(originalFeatures.Get<IHttpRequestFeature>(), call, requestEncoding);
            newFeatures.Set(requestFeature);
            var responseFeature = new HttpResponseFeature()
            {
                Body = new MemoryStream()
            };
            newFeatures.Set<IHttpResponseFeature>(responseFeature);
        }

        protected virtual IItemsFeature CreateItems(IItemsFeature originalItems)
        {
            var result = new ItemsFeature();
            foreach (var originalItem in originalItems.Items)
            {
                result.Items.Add(originalItem);
            }

            log.LogTrace($"Copied items: [{result.Items.Count}]");
            return result;
        }

        protected virtual IHttpRequestFeature CreateRequest(IHttpRequestFeature originalRequest, IUntypedCall call, Encoding requestEncoding)
        {
            var headers = new Dictionary<string, StringValues>(originalRequest.Headers, StringComparer.Ordinal);
            var result = new HttpRequestFeature
            {
                Protocol = originalRequest.Protocol,
                Method = originalRequest.Method,
                Scheme = originalRequest.Scheme,
                Path = originalRequest.Path,
                PathBase = originalRequest.PathBase,
                QueryString = originalRequest.QueryString,
                RawTarget = originalRequest.RawTarget,
                Headers = new HeaderDictionary(headers),
                Body = new MemoryStream(requestEncoding.GetBytes(call.RawJson))
            };
            log.LogTrace($"Copied request: [{result.Protocol}] [{result.Method}] [{result.Scheme}] [{result.Path}] [{result.PathBase}] [{result.QueryString}] [{result.RawTarget}], [{headers.Count}] headers");
            return result;
        }
    }
}