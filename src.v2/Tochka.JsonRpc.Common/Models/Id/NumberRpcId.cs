using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Common.Models.Id
{
    [ExcludeFromCodeCoverage]
    public class NumberRpcId : IRpcId, IEquatable<NumberRpcId>
    {
        public long NumberValue { get; }

        public NumberRpcId(long value) => NumberValue = value;

        public override string ToString() => $"{NumberValue}";

        public bool Equals(NumberRpcId? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return NumberValue == other.NumberValue;
        }

        public bool Equals(IRpcId? other) => Equals(other as NumberRpcId);

        public override bool Equals(object? obj)
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

        public override int GetHashCode() => NumberValue.GetHashCode();

        public static bool operator ==(NumberRpcId left, NumberRpcId right) => Equals(left, right);

        public static bool operator !=(NumberRpcId left, NumberRpcId right) => !Equals(left, right);
    }
}
