namespace SimdIsMagicDust {
    /// <summary>
    /// Contains SIMD-accelerated methods, mostly mathematical in nature.
    /// </summary>
    // This is implemented as a partial class where each file
    //     Simd.type[.gen].cs
    // is responsible for the methods particular to that type.
    // In particular, Simd.type4.cs will contain the blueprints that motivate
    // the Simd.typeN.gen.cs files for N < 3.
    public static partial class Simd { }
}