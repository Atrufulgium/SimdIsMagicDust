using System.ComponentModel;

namespace SimdIsMagicDust.LowLevel {
    /// <summary>
    /// I'm doing funky stuff with the build process to do funky stuff with my
    /// tests.
    /// <br/>
    /// This class is to make sure that the conditional compilation is actually
    /// succesful when testing -- the test project will read these values and
    /// compare them between dlls.
    /// </summary>
    // Not really the correct place for this, but I don't have a more hidden
    // spot for this anyways so this is _fiiine_.
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class TestHelperHelper {
        /// <summary>
        /// Whether the custom SIMD behaviour is turned off.
        /// <br/>
        /// This doesn't stop RyuJIT from SIMD'ing all the
        /// <see cref="System.Runtime.Intrinsics"/> stuff.
        /// </summary>
#if DISABLE_MAGIC_DUST
        public static bool SimdIsDisabled => true;
#else
        public static bool SimdIsDisabled => false;
#endif
    }
}
