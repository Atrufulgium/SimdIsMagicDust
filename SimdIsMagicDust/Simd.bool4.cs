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
        /// Returns true when any component of <paramref name="value"/> is
        /// true.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool Any(bool4 value) {
#if !DISABLE_MAGIC_DUST
            if (AdvSimd.Arm64.IsSupported) {
                // simply adds all the entries together
                // as we're either 0 or 0xFFFF_FFFF, this comes down to "don't
                // sum to zero". In particular, the lower half.
                var res = AdvSimd.Arm64.AddAcross(value);
                return res[0] != 0;
            } else if (Sse41.IsSupported) {
                // TestZ here returns "(value & true) == 0"
                return !Sse41.TestZ(value, Vector128.Create(0xFFFF_FFFF));
            } else if (Sse.IsSupported) {
                // movmskps grabs the _msb_ of each vector entry and puts them
                // in a single int.
                // yes this is a bitwise operation on floats, don't @ me, @ the
                // designers at intel who defined it like this.
                return Sse.MoveMask(*(Vector128<float>*)&value) > 0;
            } else if (PackedSimd.IsSupported) {
                return PackedSimd.AnyTrue(value);
            }
#endif
            return value.x | value.y | value.z | value.w;
        }

        /// <summary>
        /// Returns true when all components of <paramref name="value"/> are
        /// true.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool All(bool4 value) {
#if !DISABLE_MAGIC_DUST
            if (AdvSimd.Arm64.IsSupported) {
                // like above, but now three 0xFFFF_FFFFs sum to 0x3_FFFF_FFFC.
                var res = AdvSimd.Arm64.AddAcross(value);
                return res[1] == 3;
            } else if (Sse41.IsSupported) {
                // TestC here returns "(~value & true) == 0")
                return Sse41.TestC(value, Vector128.Create(0xFFFF_FFFF));
            } else if (Sse.IsSupported) {
                return Sse.MoveMask(*(Vector128<float>*)&value) == 0b1111;
            } else if (PackedSimd.IsSupported) {
                return PackedSimd.AllTrue(value);
            }
#endif
            return value.x & value.y & value.z & value.w;
        }
    }
}