using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SimdIsMagicDust.Collections {
    public static class CollectionExtensions {

        /// <summary>
        /// Please see
        /// <a href="https://tooslowexception.com/getting-rid-of-array-bound-checks-ref-returns-and-net-5/">
        ///     this link
        /// </a>
        /// or the documentation of
        ///     <see cref="MemoryMarshal.GetArrayDataReference{T}(T[])"/>
        /// for an explanation of what this does, and all the dangers that come
        /// with it.
        /// <br/>
        /// Short version: Only call this on arrays that you know the GC won't
        /// move around (eg `fixed`, unmanaged, or just disabling GC entirely),
        /// and only call this when not doing whacky type stuff.
        /// <br/>
        /// Note that this may disable optimisations the JITter could've done,
        /// as it reason more about the built-in `array[index]` than user code.
        /// Please profile whether using this actulaly improves your code.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static unsafe ref T UnsafeGet<T>(this T[] array, int index) where T : unmanaged {
            ref var data = ref MemoryMarshal.GetArrayDataReference(array);
            return ref Unsafe.Add(ref data, index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static unsafe void UnsafeSet<T>(this T[] array, int index, T value) where T : unmanaged {
            ref var data = ref MemoryMarshal.GetArrayDataReference(array);
            Unsafe.Add(ref data, index) = value;
        }
    }
}
