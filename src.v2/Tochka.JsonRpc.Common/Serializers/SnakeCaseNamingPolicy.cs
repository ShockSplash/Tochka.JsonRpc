using System.Text.Json;

namespace Tochka.JsonRpc.Common.Serializers;

internal class SnakeCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
    {
        var nameLetters = name.Select(static (x, i) => i > 0 && char.IsUpper(x)
            ? "_" + x
            : x.ToString());
        return string.Concat(nameLetters).ToLowerInvariant();
    }
}
