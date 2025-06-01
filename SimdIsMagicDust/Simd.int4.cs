using SimdIsMagicDust.LowLevel;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
#if !DISABLE_MAGIC_DUST
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.Wasm;
using System.Runtime.Intrinsics.X86;
#endif

namespace SimdIsMagicDust {
    public static partial class Simd {

        /// <summary>
        /// Computes the absolute value for each component.
        /// </summary>
        /// <remarks>
        /// Any <see cref="int.MinValue"/> will get mapped to itself.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 Abs(int4 value) {
#if !DISABLE_MAGIC_DUST
            if (AdvSimd.Arm64.IsSupported) {
                return (int4)AdvSimd.Abs(value);
            } else if (Ssse3.IsSupported) {
                return (int4)Ssse3.Abs(value);
            } else if (PackedSimd.IsSupported) {
                return (int4)PackedSimd.Abs(value);
            }
#endif
            return new int4(
                value.x == int.MinValue ? int.MinValue : Math.Abs(value.x),
                value.y == int.MinValue ? int.MinValue : Math.Abs(value.y),
                value.z == int.MinValue ? int.MinValue : Math.Abs(value.z),
                value.w == int.MinValue ? int.MinValue : Math.Abs(value.w)
            );
        }

        /// <summary>
        /// Returns true when any component of <paramref name="value"/> is
        /// nonzero.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Any(int4 value) {
#if !DISABLE_MAGIC_DUST
            if (Sse41.IsSupported) {
                return !Sse41.TestZ(
                    (Vector128<uint>)value,
                    Vector128.Create(0xFFFF_FFFF)
                );
            }
#endif
            return Any((bool4)value);
        }

        /// <summary>
        /// Returns true when all components of <paramref name="value"/> are
        /// nonzero.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool All(int4 value) => All((bool4)value);

        /// <summary>
        /// Clamp <paramref name="value"/> component-wise to the range
        /// [<paramref name="min"/>, <paramref name="max"/>].
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 Clamp(int4 value, int4 min, int4 max) {
            return Max(min, Min(max, value));
        }

        /// <summary>
        /// Returns the dot product of <paramref name="left"/> and
        /// <paramref name="right"/>, i.e.
        /// <code>
        /// left[0] * right[0] + left[1] * right[1] + ...
        /// </code>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Dot(int4 left, int4 right) {
            return HorizontalAdd(left * right);
        }

        /// <summary>
        /// Returns the sum of all components of <paramref name="value"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int HorizontalAdd(int4 value) {
#if !DISABLE_MAGIC_DUST
            if (AdvSimd.Arm64.IsSupported) {
                return AdvSimd.Arm64.AddAcross(value)[0];
            } else if (Sse2.IsSupported) {
                // Note: The compiler is very defensive if you update structs.
                // Much better code is generated if you just define new ones.
                // (Is this even worth it compared to serial?)
                int4 zwxy = Sse2.Shuffle(value, MmShuffle.zwxy);
                var v1 = value + zwxy;
                int4 xyxy = Sse2.Shuffle(v1, MmShuffle.xyxy);
                var v2 = v1 + xyxy;
                return v2.x;
            } else if (Sse.IsSupported) {
                int4 zwxy = (int4)Sse.Shuffle(
                    (Vector128<float>)value,
                    (Vector128<float>)value,
                    MmShuffle.zwxy
                );
                var v1 = value + zwxy;
                int4 xyxy = (int4)Sse.Shuffle(
                    (Vector128<float>)v1,
                    (Vector128<float>)v1,
                    MmShuffle.xyxy
                );
                var v2 = v1 + xyxy;
                return v2.x;
            } else if (PackedSimd.IsSupported) {
                // TODO -- just fallback for now
            }
#endif
            return value.x + value.y + value.z + value.w;
        }

        /// <summary>
        /// Computes the component-wise maximum of <paramref name="left"/> and
        /// <paramref name="right"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 Max(int4 left, int4 right) {
#if !DISABLE_MAGIC_DUST
            if (AdvSimd.IsSupported) {
                return AdvSimd.Max(left, right);
            } else if (Sse41.IsSupported) {
                return Sse41.Max(left, right);
            } else if (PackedSimd.IsSupported) {
                return PackedSimd.Max(left, right);
            }
#endif
            return new int4(
                Math.Max(left.x, right.x),
                Math.Max(left.y, right.y),
                Math.Max(left.z, right.z),
                Math.Max(left.w, right.w)
            );
        }

        /// <summary>
        /// Computes the component-wise minimum of <paramref name="left"/> and
        /// <paramref name="right"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 Min(int4 left, int4 right) {
#if !DISABLE_MAGIC_DUST
            if (AdvSimd.IsSupported) {
                return AdvSimd.Min(left, right);
            } else if (Sse41.IsSupported) {
                return Sse41.Min(left, right);
            } else if (PackedSimd.IsSupported) {
                return PackedSimd.Min(left, right);
            }
#endif
            return new int4(
                Math.Min(left.x, right.x),
                Math.Min(left.y, right.y),
                Math.Min(left.z, right.z),
                Math.Min(left.w, right.w)
            );
        }

        /// <summary>
        /// Checks <paramref name="condition"/> component-wise. A true
        /// component will be result in its respective <paramref name="trues"/>
        /// value, and otherwise in its respective <paramref name="falses"/>
        /// value.
        /// </summary>
        /// <remarks>
        /// Both <paramref name="trues"/> and <paramref name="falses"/> will be
        /// evaluated fully. You cannot use this method to circumvent a large
        /// computation like regular if/else statements.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 Select(bool4 condition, int4 trues, int4 falses) {
#if !DISABLE_MAGIC_DUST
            if (AdvSimd.Arm64.IsSupported
                || Sse2.IsSupported
                || PackedSimd.IsSupported) {
                var val = (int4)(Vector128<int>)condition;
                return (val & trues) + (~val & falses);
            }
#endif
            return new int4(
                condition.x ? trues.x : falses.x,
                condition.y ? trues.y : falses.y,
                condition.z ? trues.z : falses.z,
                condition.w ? trues.w : falses.w
            );
        }

        /// <summary>
        /// Computes, per component `c`, the sign:
        /// <list type="bullet">
        /// <item>If `c &gt; 0`, set `c` to 1;</item>
        /// <item>If `c == 0`, set `c` to 0;</item>
        /// <item>If `c &lt; 0`, set `c` to -1.</item>
        /// </list>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 Sign(int4 value) {
            // TODO:  This impl is pretty bad but it's the sign function -- I
            // don't particularly care too much.
            var gt = (int4)(value > default(int4));
            var lt = (int4)(value < default(int4));
            return gt - lt;
        }
    }
}