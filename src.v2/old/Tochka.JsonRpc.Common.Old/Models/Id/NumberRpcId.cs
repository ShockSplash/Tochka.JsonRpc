using System;
using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common.Old.Models.Id
{
    [ExcludeFromCodeCoverage]
    public class NumberRpcId : IRpcId, IEquatable<NumberRpcId>
    {
        public readonly long Number;

        public NumberRpcId(long value) => Number = value;

        public bool Equals(NumberRpcId other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Number == other.Number;
        }

        public bool Equals(IRpcId other) => Equals(other as NumberRpcId);

        public override string ToString() => $"{Number}";

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

            return Equals((NumberRpcId) obj);
        }

        public override int GetHashCode() => Number.GetHashCode();

        public static bool operator ==(NumberRpcId left, NumberRpcId right) => Equals(left, right);

        public static bool operator !=(NumberRpcId left, NumberRpcId right) => !Equals(left, right);
    }
}
