using Newtonsoft.Json.Linq;

namespace Tochka.JsonRpc.Client.Models;

internal class UntypedRequest : Request<JContainer>, IUntypedCall
{
    
}
