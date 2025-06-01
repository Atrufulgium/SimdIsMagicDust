Shoo! You're not supposed to read this, this repo's barely started!  
TODO: Write a proper readme.

SIMD is magic dust
==================
I like it when computers go brrrr  
I don't like the disjoint mess that is
- `System.Numerics` (c'mon, you can do better than Matrix3x2, Matrix4x4, Vector2, Vector3, Vector4);
- `System.Runtime.Intrinsics` (Vector128<> and friends are _oh so_ nice, but _oh so_ verbose); and
- `System.Runtime.Intrinsics.<architecture>` (exposes so much more than the other two, but it's obviously hardware-dependent).  
So I'm doing the worst of both worlds now: probably make computers go slightly less brr and add to the pile.

Overview
--------
The project setup is as follows:
- `SimdIsMagicDust` contains the main SIMD library, this is where the magic happens.
- `SimdIsMagicDustTests` contains tests that check whether the SIMD and non-SIMD paths are consistent.
- `SimdIsMagicDustBenchmarks` contains various misc benchmarks for when I need to decide something.

The build process is an abomination. Please don't try.

(The first time you load the project, some projects' references won't resolve in the editor, as I'm referencing projects' build directory instead of the projects themselves. Build the project -- which should work -- and then reload each offending project.)

Main project
------------
The main star are the vector types `primitiveN` or `primitiveNxM`, like `bool4` or `float3` or `sbyte2x3` or whatever, and the funky math you can do on them. (Though currently only `bool4` and `int4` exist.)

The types and all mathematical operations involving them are currently on one big pile instead of something better architected, which I should fix only after it gets out of hand.

Note to self: enclose all SIMD code in `#if !DISABLE_MAGIC_DUST` directives to allow for easy testing, and sanity-check the generated assembly with either SharpLab or DisAsmo.

Tests
-----
The tests compare the non-SIMD paths (run on test-discovery) and SIMD paths (run on test run).  
This process uses quite some reflection (more than NUnit base) and loads a dll of `SimdIsMagicDust` with the SIMD ripped out, so this may take a while.

Due to jank in this process, both the Tests and Benchmarks rely on a Release build of the main library. This is already set in the build configuration. Autodetection of new methods usually doesn't work and you manually have to rebuild the testing project.

These tests only verify that the current SIMD hardware matches scalar behaviour. It makes no claims about other hardware. This is a problem. I haven't thought about fixing this yet. (In particular, I don't have any ARM hardware.)

Benchmarks
----------
They questionably exist.
