using NUnit.Framework;
using SimdIsMagicDust.TestHelpers;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.Wasm;
using System.Runtime.Intrinsics.X86;

#if !EXCLUDE_FROM_CI
namespace DontWorryThisTest_Must_Fail {
    public static class TestTests {
        // This test is exceptional in that we check for the _difference_ in
        // compilation symbols. If this test passes, both `.scalar.dll` and
        // `.dll` are the same, which is _not_ what we want.
        // So this test _must_ fail.
        // On the other hand, that's kinda awkward for CI, so define the
        // EXCLUDE_FROM_CI symbol in the remote runner.
        // TestTests.TestTest() exists to check the negation of this, but that
        // runs on the additional assumption that my SimdTestAttribute is
        // written correctly and doesn't auto-succeed.
        [SimdTest]
        public static bool TestTestAlt() {
            return SimdIsMagicDust.LowLevel.TestHelperHelper.SimdIsDisabled;
        }
    }
}
#endif

namespace SimdIsMagicDust.TestHelpers {
    /// <summary>
    /// <see cref="SimdTestAttribute"/> checks whether running the tagged
    /// method both with and without SIMD acceleration returns the same.
    /// <br/>
    /// This is an extremely whacky definition, so we need some tests that this
    /// test attribute works correctly.
    /// </summary>
    public static class TestTests {
        // This test is exceptional in that the test runner automatically flips
        // the boolean result of any test "TestTests.TestTest()".
        // As such, this is the only test that checks for a _different_ result
        // instead of the same result.
        [SimdTest]
        public static bool TestTest() {
            return LowLevel.TestHelperHelper.SimdIsDisabled;
        }

        // Of course, we need a reference point beyond "they're different".
        // Combined with the above test, this tells us that:
        // - SimdIsMagicDust.dll is used by SimdIsMagicDustTest.dll
        // - SimdIsMagicDust.scalar.dll is used by SimdIsMagicDustTest.scalar.dll
        // - Only the latter has DISABLE_MAGIC_DUST set.
        [Test(ExpectedResult = false)]
        public static bool TestTestTest() {
            return LowLevel.TestHelperHelper.SimdIsDisabled;
        }

        // So things can apparently go horribly wrong if you try to act as if
        // "Type in Assembly Context A" and "Same Type in Assembly Context B".
        // Check if that applies here.
        [SimdTest]
        public static int4 TestTestTypes() {
            return new int4(1, 2, 3, 4);
        }

        // There's no point to these tests if there is not simd being run
        // actually because the testrunner just doesn't support any.
        // Check for the bare minimum.
        [Test(ExpectedResult = true)]
        public static bool TestSimdCapabilities() {
            if (AdvSimd.IsSupported) {
                return true;
            } if (Sse.IsSupported) {
                return true;
            } else if (PackedSimd.IsSupported) {
                return true;
            }
            return false;
        }

        // Finally basically the end-to-end-test of the testing. Something else
        // may have been messed up that might be caught here.
        [SimdTest]
        public static int4 TestDotProduct() {
            return Simd.Dot(
                new int4(1, 2, 3, 4),
                new int4(2, 3, 4, 5)
            );
        }
    }
}
