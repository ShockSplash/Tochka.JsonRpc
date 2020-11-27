using System;
using System.Collections.Generic;
using System.Reflection;

namespace Tochka.JsonRpc.ApiExplorer
{
    public interface ITypeEmitter
    {
        Type CreateRequestType(string actionName, Type baseBodyType, IReadOnlyDictionary<string, Type> properties, Type jsonRpcRequestSerializer);
        Type CreateResponseType(string actionName, Type bodyType, Type jsonRpcRequestSerializer);
    }
}