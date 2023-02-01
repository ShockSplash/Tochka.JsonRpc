namespace Tochka.JsonRpc.Client.Models;

public class SingleResponseWrapper : IResponseWrapper
{
    public IResponse Single { get; set; }
}
