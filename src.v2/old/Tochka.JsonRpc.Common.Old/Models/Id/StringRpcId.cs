using System;
using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common.Old.Models.Id
{
    [ExcludeFromCodeCoverage]
    public class StringRpcId : IRpcId, IEquatable<StringRpcId>
    {
        public readonly string String;

        public StringRpcId(string value) => String = value ?? throw new ArgumentNullException(nameof(value));

        public bool Equals(StringRpcId other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(String, other.String);
        }

        public bool Equals(IRpcId other) => Equals(other as StringRpcId);

        public override string ToString() => String ?? "(null)";

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((StringRpcId) obj);
        }

        public override int GetHashCode() => String != null
            ? String.GetHashCode()
            : 0;

        public static bool operator ==(StringRpcId left, StringRpcId right) => Equals(left, right);

        public static bool operator !=(StringRpcId left, StringRpcId right) => !Equals(left, right);
    }
}
