using System;
using System.Runtime.InteropServices;

namespace SimdIsMagicDust.TestHelpers {
    /// <summary>
    /// A class for casting unmanaged boxed value type "MyType" in a
    /// separate AssemblyLoadContext to boxed MyType in the main
    /// AssembyLoadContext.
    /// </summary>
    internal static class MultiverseCast {
        public static object? Cast(object? fromObject, Type targetType) {
            if (fromObject == null)
                return null;

            var fromType = fromObject.GetType();

            if (fromType.AssemblyQualifiedName != targetType.AssemblyQualifiedName)
                throw new ArgumentException($"The types aren't called the same:\n  From: {fromType.AssemblyQualifiedName}\n  To: {targetType.AssemblyQualifiedName}");

            // Assumption: if the AFQNs are the same, they will "look" the same.

            // These are always safe
            if (fromType.IsPrimitive)
                return fromObject;

            if (!fromType.IsValueType)
                throw new ArgumentException($"Can only cast value types, but \"{fromType.FullName}\" is not a value type.");

            int sizeFrom = Marshal.SizeOf(fromType);
            int sizeTo = Marshal.SizeOf(targetType);

            if (sizeFrom != sizeTo)
                throw new InvalidOperationException($"Size mismatch; trying to cast from {sizeFrom} to {sizeTo}");

            var handle = GCHandle.Alloc(fromObject, GCHandleType.Pinned);
            try {
                IntPtr ptr = handle.AddrOfPinnedObject();
                return Marshal.PtrToStructure(ptr, targetType);
            } finally {
                handle.Free();
            }
        }
    }
}
