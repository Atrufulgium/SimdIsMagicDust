using System;
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
    public struct bool4 : IEquatable<bool4> {

        // The internal state is "0" on false and "0xFFFF_FFFFu" on true.
        // All comparisons on both x86 and ARM result in either all-0 or
        // all-1 in the results, making this easy.
        // Note that we _actually use_ this internal representation.
        // Nevertheless, if _you're_ reading this because you want use
        // whatever internal state this has, note that it is subject to change.
        uint _x;
        uint _y;
        uint _z;
        uint _w;

        public bool x { readonly get => _x != 0; set => _x = value ? 0xFFFF_FFFFu : 0; }
        public bool y { readonly get => _y != 0; set => _y = value ? 0xFFFF_FFFFu : 0; }
        public bool z { readonly get => _z != 0; set => _z = value ? 0xFFFF_FFFFu : 0; }
        public bool w { readonly get => _w != 0; set => _w = value ? 0xFFFF_FFFFu : 0; }

        public bool4(bool x, bool y, bool z, bool w) {
            _x = x ? 0xFFFF_FFFFu : 0;
            _y = y ? 0xFFFF_FFFFu : 0;
            _z = z ? 0xFFFF_FFFFu : 0;
            _w = w ? 0xFFFF_FFFFu : 0;
        }

        public static bool4 true4 => Vector128.Create(0xFFFF_FFFFu);
        public static bool4 false4 => default;

        public bool this[int index] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Unsafe.Add(ref _x, index) != 0;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Unsafe.Add(ref _x, index) = value ? 0xFFFF_FFFFu : 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool4 operator !(bool4 operand) {
#if !DISABLE_MAGIC_DUST
            if (AdvSimd.IsSupported) {
                return AdvSimd.CompareEqual(operand, Vector128<uint>.Zero);
            } else if (Sse2.IsSupported) {
                return Sse2.CompareEqual(operand, Vector128<uint>.Zero);
            } else if (PackedSimd.IsSupported) {
                return PackedSimd.CompareEqual(operand, Vector128<uint>.Zero);
            }
#endif
            return new bool4(!operand.x, !operand.y, !operand.z, !operand.w);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool4 operator |(bool4 left, bool4 right) {
#if !DISABLE_MAGIC_DUST
            if (AdvSimd.IsSupported) {
                return AdvSimd.Or(left, right);
            } else if (Sse2.IsSupported) {
                return Sse2.Or(left, right);
            } else if (PackedSimd.IsSupported) {
                return PackedSimd.Or(left, right);
            }
#endif
            return new bool4(left.x | right.x, left.y | right.y, left.z | right.z, left.w | right.w);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool4 operator &(bool4 left, bool4 right) {
#if !DISABLE_MAGIC_DUST
            if (AdvSimd.IsSupported) {
                return AdvSimd.And(left, right);
            } else if (Sse2.IsSupported) {
                return Sse2.And(left, right);
            } else if (PackedSimd.IsSupported) {
                return PackedSimd.And(left, right);
            }
#endif
            return new bool4(left.x & right.x, left.y & right.y, left.z & right.z, left.w & right.w);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool4 operator ^(bool4 left, bool4 right) {
#if !DISABLE_MAGIC_DUST
            if (AdvSimd.IsSupported) {
                return AdvSimd.Xor(left, right);
            } else if (Sse2.IsSupported) {
                return Sse2.Xor(left, right);
            } else if (PackedSimd.IsSupported) {
                return PackedSimd.Xor(left, right);
            }
#endif
            return new bool4(left.x ^ right.x, left.y ^ right.y, left.z ^ right.z, left.w ^ right.w);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool4 operator ==(bool4 left, bool4 right) {
#if !DISABLE_MAGIC_DUST
            if (AdvSimd.IsSupported) {
                return AdvSimd.CompareEqual(left, right);
            } else if (Sse2.IsSupported) {
                return Sse2.CompareEqual(left, right);
            } else if (PackedSimd.IsSupported) {
                return PackedSimd.CompareEqual(left, right);
            }
#endif
            return new bool4(left.x == right.x, left.y == right.y, left.z == right.z, left.w == right.w);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool4 operator !=(bool4 left, bool4 right) {
#if !DISABLE_MAGIC_DUST
            if (AdvSimd.IsSupported) {
                return AdvSimd.Not(AdvSimd.CompareEqual(left, right));
            } else if (Sse2.IsSupported) {
                var res = Sse2.CompareEqual(left, right);
                return Sse2.AndNot(res, Vector128.Create(0xFFFF_FFFFu));
            } else if (PackedSimd.IsSupported) {
                var res = PackedSimd.CompareEqual(left, right);
                return PackedSimd.AndNot(res, Vector128.Create(0xFFFF_FFFFu));
            }
#endif
            return new bool4(left.x != right.x, left.y != right.y, left.z != right.z, left.w != right.w);
        }

        public static implicit operator Vector128<uint>(bool4 val)
            => Unsafe.As<bool4, Vector128<uint>>(ref val);
        public static implicit operator bool4(Vector128<uint> val)
            => Unsafe.As<Vector128<uint>, bool4>(ref val);

        public static explicit operator Vector128<int>(bool4 val)
            => Unsafe.As<bool4, Vector128<int>>(ref val);
        public static explicit operator bool4(Vector128<int> val)
            => Unsafe.As<Vector128<int>, bool4>(ref val);

        public static explicit operator (bool, bool, bool, bool)(bool4 val)
            => new(val.x, val.y, val.z, val.w);
        public static explicit operator bool4((bool, bool, bool, bool) val)
            => new(val.Item1, val.Item2, val.Item3, val.Item4);

        // We internally store all bits instead of just the lsb, so no lazy reinterpret.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator int4(bool4 val) {
            var ival = Unsafe.As<bool4, int4>(ref val);
#if !DISABLE_MAGIC_DUST
            if (AdvSimd.IsSupported) {
                return AdvSimd.And(ival, (int4)1);
            } else if (Sse2.IsSupported) {
                return Sse2.And(ival, (int4)1);
            } else if (PackedSimd.IsSupported) {
                return PackedSimd.And(ival, (int4)1);
            }
#endif
            return ival & 1;
        }

        // We need to make "any nonzero" a "0xFFFF_FFFFu", so no lazy reinterpret.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator bool4(int4 val)
            => val != default;

        public override readonly bool Equals(object? obj) {
            return obj is bool4 @bool && Equals(@bool);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(bool4 other)
            => (Vector128<uint>)this == (Vector128<uint>)other;

        public override readonly int GetHashCode() {
            return HashCode.Combine(_x, _y, _z, _w);
        }

        public override readonly string ToString()
            => $"bool4({x}, {y}, {z}, {w})";
    }
}