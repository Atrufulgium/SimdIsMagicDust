using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SimdIsMagicDust.TestHelpers {
    /// <summary>
    /// Test for a method whether the scalar and SIMD version return the same
    /// result <i><b>on the current hardware configuration</b></i>.
    /// <br/>
    /// This may only be applied to instance methods without parameters.
    /// Naturally, they must have a return value.
    /// </summary>
    /// <example>
    /// Suppose you want to test the dot product:
    /// <code>
    /// [SimdTest]
    /// public static int4 TestDotProduct() {
    ///     return Simd.Dot(
    ///         new int4(1,2,3,4),
    ///         new int4(2,3,4,5)
    ///     );
    /// }
    /// </code>
    /// If the SIMD path <i>of your current hardware</i> is written correctly,
    /// this test should pass, as it will give you the same result as the
    /// sequential version the test has run implicitly for you.
    /// </example>
    /// <remarks>
    /// Note that this tests only <i>one</i> hardware target, and not all.
    /// Naturally, you need to run this suite across different hardware to
    /// achieve that.
    /// <br/>
    /// These tests also require the definitions of SIMD branches in the main
    /// project to be surrounded by the #if !DISABLE_MAGIC_DUST compiler
    /// directive.
    /// <br/>
    /// RyuJIT SIMD'ing <see cref="System.Runtime.Intrinsics.Vector128{T}"/>
    /// and the like is not prevented by this, but I feel like "the runtime is
    /// correct" is a fair assumption.
    /// <br/>
    /// If things don't work as expected, check the comment in the source
    /// above this declaration.
    /// </remarks>
    // Ideally, you'd have runtime parameterization, but that behaves _so_
    // poorly with this assembly shit. I don't want to have to do this dance
    // every single time.
    // I do need to make the api a bit more lenient and support more than
    // just `instance nonvoid Method()`.
    //
    // On the more serious side, I owe you an explanation.
    // The general approach here is the _incredibly funny_ (hahaha `:(`) idea
    // of compiling this test project not just _with_ SIMD, but without SIMD
    // as well. We then load _those_ methods (from a different assembly!) and
    // call them here.
    // To get this to work, the csproj of _both_ the base library and the
    // testing library needs some significant messing around to get these two
    // different test dll's.
    //
    // For prosperity (I don't want to redo that, good god), here is the fairly
    // literal copypaste of what I've done.
    // The main project gets:
    // <PropertyGroup>
    //    <IsScalarBuild>false</IsScalarBuild>
    // </PropertyGroup>
    // 
    // <Target Name="BuildScalar" AfterTargets="Build" Condition="'$(IsScalarBuild)' != 'true'">
    //   <MSBuild Projects="$(MSBuildProjectFullPath)"
    //            Properties="DefineConstants=DISABLE_MAGIC_DUST;Configuration=$(Configuration);OutputPath=$(OutputPath)Scalar\;IsScalarBuild=true"
    //            Targets="Build" />
    //   <Move SourceFiles="$(OutputPath)Scalar\$(AssemblyName).dll"
    //         DestinationFiles="$(OutputPath)$(AssemblyName).Scalar.dll" />
    // </Target>
    // This tells MSBuild to also build a scalar version with the constant
    // defined, and copy the resulting dll next to the main dll.
    //
    // Every project referencing the main project must manually specify the dll:
    // <ItemGroup>
    //   <Reference Include="SimdIsMagicDust">
    //     <HintPath>..\SimdIsMagicDust\bin\Release\net9.0\SimdIsMagicDust.dll</HintPath>
    //   </Reference>
    // </ItemGroup>
    // Having two dll's with the same signature in the same build location just
    // seems to confuse the build system (which is I can't really blame).
    // I'm forcing everything to Release because VS has this weird BS that the
    // build succeeds but the syntax highlighter gets a stroke.
    // Should perhaps look into that, but for now am forcing the base project
    // to always build in Release, not like it matters much anyways.
    //
    // The test project gets a little different treatment:
    // <ItemGroup>
    //   <Reference Include="SimdIsMagicDust" Condition="'$(Configuration)' != 'Scalar'">
    //     <HintPath>..\SimdIsMagicDust\bin\$(Configuration)\net9.0\SimdIsMagicDust.dll</HintPath>
    //   </Reference>
    //   <Reference Include="SimdIsMagicDust" Condition="'$(Configuration)' == 'Scalar'">
    //     <HintPath>..\SimdIsMagicDust\bin\Release\net9.0\SimdIsMagicDust.scalar.dll</HintPath>
    //   </Reference>
    // </ItemGroup>
    // 
    // <Target Name="BuildScalar" AfterTargets="Build" Condition="'$(Configuration)' != 'Scalar' And '$(IsScalarBuild)' != 'true'">
    //   <MSBuild Projects="$(MSBuildProjectFullPath)"
    //            Properties="Configuration=Scalar;IsScalarBuild=true"
    //            Targets="Build" />
    // </Target>
    // 
    // <Target Name="CopyScalarDll" AfterTargets="BuildScalar" Condition="'$(Configuration)' != 'Scalar'">
    //   <PropertyGroup>
    //     <ScalarOutputDir>$(MSBuildProjectDirectory)\bin\Scalar\net9.0</ScalarOutputDir>
    //     <OutputDir>$(OutputPath.TrimEnd('\'))</OutputDir>
    //   </PropertyGroup>
    // 
    //   <Move SourceFiles="$(ScalarOutputDir)\$(AssemblyName).dll"
    //         DestinationFiles="$(OutputDir)\$(AssemblyName).scalar.dll" />
    //   <Move SourceFiles="$(ScalarOutputDir)\SimdIsMagicDust.scalar.dll"
    //         DestinationFiles="$(OutputDir)\SimdIsMagicDust.scalar.dll" />
    //   <RemoveDir Directories="$(MSBuildProjectDirectory)\bin\Scalar" />
    // </Target>
    // This tells MSBuild to (1) whenever we build _any_ configuration, to also
    // manually build the "Scalar" configuration, using the correct dlls (this
    // was _annoying_), and (2) afterwards copy the output scalar test _and_
    // base dlls next to the main testing dlls.
    //
    // If you do this correctly, this attribute works correctly. You shouldn't
    // need to think about this as everything's commited with the project.
    // In case you actually read this because stuff broke...
    // God help you? It took me a full day to figure this stuff out. Bleh.
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    internal class SimdTestAttribute : NUnitAttribute, ISimpleTestBuilder, IApplyToTest, IImplyFixture {

        private readonly NUnitTestCaseBuilder builder = new();
        
        public void ApplyToTest(Test test) {
            if (test.Method == null) {
                test.MakeInvalid("This attribute must apply to methods.");
                return;
            }

            if (test.Method.ReturnType.Name == "void" || !test.Method.IsStatic || test.Method.GetParameters().Length > 0)
                test.MakeInvalid("This attribute must apply to non-void static non-parameterized methods.");
        }

        public TestMethod BuildFrom(IMethodInfo method, Test? suite) {
            TestCaseParameters parms = new();
            try {
                parms.ExpectedResult = RunScalarVersion(method);
            } catch (Exception e) {
                // There literally does not seem to be any way to log stuff into the test pane.
                // uhhh then I _guess_ I'll write into the name?
                // (Note to self when you need to debug stuff again: You don't
                //  have breakpoints, you don't have logging, just write to a
                //  file in Documents or something.)
                parms.TestName = $"{method.Name} [Internal Error] Could not run test: {e}";
                parms.RunState = RunState.NotRunnable;
            }

            return builder.BuildTestMethod(method, suite, parms);
        }

        private static object? RunScalarVersion(IMethodInfo method) {
            // Very messy, but it works.
            var scalarType = GetType(method);
            var scalarMethod = scalarType?.GetMethod(method.Name);
            if (scalarType == null || scalarMethod == null) {
                throw new InvalidOperationException($"Could not resolve method \"{method.Name}\" (of \"{method.TypeInfo.FullName}\") properly:\n  scalarType: {scalarType}\n  scalarMethod: {scalarMethod}");
            }
            if (!scalarMethod.IsStatic) {
                throw new InvalidOperationException("Method must be static.");
            }

            var res = scalarMethod.Invoke("this param is ignored because static yay", null);

            // Bit ugly, but: flip the boolean result if we're TestTests.TestTest().
            if (method.Name.EndsWith("TestTest") && scalarType.Name.EndsWith("TestTests"))
                res = !(bool)res!;
            return res;
        }

        // Cache type info so we do less reflective lookups.
        // (What's the lifetime of `static` here anyways?
        //  Can't imagine it being "beyond rebuilds".)
        static readonly Dictionary<string, Type?> typeCache = new();
        private static Type? GetType(IMethodInfo method) {
            string key = method.TypeInfo.FullName;
            if (typeCache.TryGetValue(key, out var type))
                return type;

            type = Assembly.GetType(method.TypeInfo.FullName);
            typeCache.Add(key, type);
            return type;
        }

        private static Assembly? assembly = null;
        private static Assembly Assembly {
            get {
                if (assembly != null)
                    return assembly;

                var testAssemblyLocation = typeof(SimdTestAttribute).Assembly.Location;
                if (testAssemblyLocation == null || testAssemblyLocation == "") {
                    throw new InvalidOperationException("The assembly could not be loaded properly.");
                }
                var dir = Path.GetDirectoryName(testAssemblyLocation);
                if (dir == null || dir == "") {
                    throw new InvalidOperationException("The assembly could not be loaded properly.");
                }
                var testAssemblyFile = Path.GetFileName(testAssemblyLocation);
                var path = Path.Combine(dir, testAssemblyFile.Replace(".dll", ".scalar.dll"));

                // Note, Assembly.LoadFile has the footgun (called "RTFM") that
                // it's a noop if "the same" assembly is already loaded.
                // The solution is to use a different AssemblyLoadContext,
                // which is exactly what Assembly.Load does for you.
                assembly = Assembly.Load(File.ReadAllBytes(path));
                return assembly;
            }
        }
    }
}
