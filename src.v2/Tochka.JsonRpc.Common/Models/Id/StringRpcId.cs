using System;
using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common.Models.Id
{
    [ExcludeFromCodeCoverage]
    public class StringRpcId : IRpcId, IEquatable<StringRpcId>
    {
        [SuppressMessage("Naming", "CA1720:Идентификатор содержит имя типа")]
        public string String { get; }

        public StringRpcId(string value)
        {
            String = value ?? throw new ArgumentNullException(nameof(value));
        }

        public override string ToString() => String ?? "(null)";

        public bool Equals(StringRpcId other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(String, other.String, StringComparison.Ordinal);
        }

        public bool Equals(IRpcId other)
        {
            return Equals(other as StringRpcId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((StringRpcId) obj);
        }

        public override int GetHashCode()
        {
            return (String != null ? String.GetHashCode() : 0);
        }

        public static bool operator ==(StringRpcId left, StringRpcId right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(StringRpcId left, StringRpcId right)
        {
            return !Equals(left, right);
        }
    }
}
