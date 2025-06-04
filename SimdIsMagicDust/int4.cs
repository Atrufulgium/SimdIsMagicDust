using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
#if !DISABLE_MAGIC_DUST
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.Wasm;
using System.Runtime.Intrinsics.X86;
#endif

namespace SimdIsMagicDust {
    [StructLayout(LayoutKind.Sequential)]
    public struct int4
      : IAdditionOperators<int4, int4, int4>,
        IAdditiveIdentity<int4, int4>,
        IBitwiseOperators<int4, int4, int4>,
        IComparisonOperators<int4, int4, bool4>,
        IDivisionOperators<int4, int4, int4>,
        IEqualityOperators<int4, int4, bool4>,
        IEquatable<int4>,
        IModulusOperators<int4, int4, int4>,
        IMultiplicativeIdentity<int4, int4>,
        IMultiplyOperators<int4, int4, int4>,
        IShiftOperators<int4, int, int4>,
        ISubtractionOperators<int4, int4, int4>,
        IUnaryNegationOperators<int4, int4>, 
        IUnaryPlusOperators<int4, int4> {

        public int x;
        public int y;
        public int z;
        public int w;

        public int4(int x = 0, int y = 0, int z = 0, int w = 0) {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public int this[int index] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Unsafe.Add(ref x, index);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Unsafe.Add(ref x, index) = value;
        }

        public static int4 AdditiveIdentity => default;
        public static int4 MultiplicativeIdentity => Vector128.Create(1);

        // The jitter already creates the optimal assembly for most operations
        // just by using Vector128<>.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 operator +(int4 left, int4 right)
            => (Vector128<int>)left + (Vector128<int>)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 operator +(int4 operand) => operand;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 operator -(int4 left, int4 right)
            => (Vector128<int>)left - (Vector128<int>)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 operator -(int4 operand)
            => AdditiveIdentity - operand;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 operator *(int4 left, int4 right)
            => (Vector128<int>)left * (Vector128<int>)right;

        // At the time of writing, the jitter does this fully scalar.
        // vmovs might be an options, but does it really matter?
        // There's also the trick of regular arithmetic with magic numbers if
        // you divide by a constant.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 operator /(int4 left, int4 right)
            => (Vector128<int>)left / (Vector128<int>)right;

        // Vector<> doesn't even *define* %.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 operator %(int4 left, int4 right)
            => new(left.x % right.x, left.y % right.y, left.z % right.z, left.w % right.w);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 operator &(int4 left, int4 right)
            => (Vector128<int>)left & (Vector128<int>)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 operator |(int4 left, int4 right)
            => (Vector128<int>)left | (Vector128<int>)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 operator ^(int4 left, int4 right)
            => (Vector128<int>)left ^ (Vector128<int>)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 operator ~(int4 value)
            => ~(Vector128<int>)value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 operator <<(int4 value, int shiftAmount)
            => (Vector128<int>)value << shiftAmount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 operator >>(int4 value, int shiftAmount)
            => (Vector128<int>)value >> shiftAmount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 operator >>>(int4 value, int shiftAmount)
            => (Vector128<int>)value >>> shiftAmount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool4 operator ==(int4 left, int4 right) {
#if !DISABLE_MAGIC_DUST
            if (AdvSimd.IsSupported) {
                return AdvSimd.CompareEqual(
                    (Vector128<uint>)left,
                    (Vector128<uint>)right
                );
            } else if (Sse2.IsSupported) {
                return Sse2.CompareEqual(
                    (Vector128<uint>)left,
                    (Vector128<uint>)right
                );
            } else if (PackedSimd.IsSupported) {
                return PackedSimd.CompareEqual(
                    (Vector128<uint>)left,
                    (Vector128<uint>)right
                );
            }
#endif
            return new bool4(left.x == right.x, left.y == right.y, left.z == right.z, left.w == right.w);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool4 operator !=(int4 left, int4 right) {
#if !DISABLE_MAGIC_DUST
            if (AdvSimd.IsSupported) {
                return (bool4)AdvSimd.Not(AdvSimd.CompareEqual(left, right));
            } else if (Sse2.IsSupported) {
                // Somewhat awkward...
                var res = Sse2.CompareEqual(
                    (Vector128<uint>)left,
                    (Vector128<uint>)right
                );
                return Sse2.AndNot(res, Vector128.Create(0xFFFF_FFFF));
            } else if (PackedSimd.IsSupported) {
                return PackedSimd.Not(PackedSimd.CompareEqual(
                    (Vector128<uint>)left,
                    (Vector128<uint>)right
                ));
            }
#endif
            return new bool4(left.x != right.x, left.y != right.y, left.z != right.z, left.w != right.w);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool4 operator >(int4 left, int4 right) {
#if !DISABLE_MAGIC_DUST
            if (AdvSimd.IsSupported) {
                return (bool4)AdvSimd.CompareGreaterThan(left, right);
            } else if (Sse2.IsSupported) {
                return (bool4)Sse2.CompareGreaterThan(left, right);
            } else if (PackedSimd.IsSupported) {
                return (bool4)PackedSimd.CompareGreaterThan(left, right);
            }
#endif
            return new bool4(left.x > right.x, left.y > right.y, left.z > right.z, left.w > right.w);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool4 operator >=(int4 left, int4 right) {
#if !DISABLE_MAGIC_DUST
            if (AdvSimd.IsSupported) {
                return (bool4)AdvSimd.CompareGreaterThanOrEqual(left, right);
            } else if (Sse2.IsSupported) {
                // bleh
                return !(bool4)Sse2.CompareLessThan(left, right);
            } else if (PackedSimd.IsSupported) {
                return (bool4)PackedSimd.CompareGreaterThanOrEqual(left, right);
            }
#endif
            return new bool4(left.x >= right.x, left.y >= right.y, left.z >= right.z, left.w >= right.w);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool4 operator <(int4 left, int4 right)
            => right > left;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool4 operator <=(int4 left, int4 right)
            => right >= left;

        public static implicit operator Vector128<int>(int4 val)
            => Unsafe.As<int4, Vector128<int>>(ref val);
        public static implicit operator int4(Vector128<int> val)
            => Unsafe.As<Vector128<int>, int4>(ref val);

        public static explicit operator Vector128<uint>(int4 val)
            => Unsafe.As<int4, Vector128<uint>>(ref val);
        public static explicit operator int4(Vector128<uint> val)
            => Unsafe.As<Vector128<uint>, int4>(ref val);

        // (Needed for Sse shuffles that somewhy only apply to floats,
        //  with the int shuffle only in Sse2.)
        public static explicit operator Vector128<float>(int4 val)
            => Unsafe.As<int4, Vector128<float>>(ref val);
        public static explicit operator int4(Vector128<float> val)
            => Unsafe.As<Vector128<float>, int4>(ref val);

        public static implicit operator int4(int val) => Vector128.Create(val);
        // _Technically_ incorrect as ValueTuple<> is LayoutKind.Auto making
        // you unable to assume it won't shuffle the four args around, but...
        // Really. It just won't do that.
        public static implicit operator (int, int, int, int)(int4 val)
            => Unsafe.As<int4, ValueTuple<int, int, int, int>>(ref val);
        public static implicit operator int4((int, int, int, int) val)
            => Unsafe.As<ValueTuple<int,int,int,int>, int4>(ref val);

        public override readonly bool Equals(object? obj)
            => obj is int4 @int && Equals(@int);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(int4 other)
            => (Vector128<int>)this == (Vector128<int>)other;

        // Vector<> is doing basically this anyways.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly int GetHashCode() {
            return HashCode.Combine(x, y, z, w);
        }

        public override readonly string ToString()
            => $"int4({x}, {y}, {z}, {w})";
    }
}