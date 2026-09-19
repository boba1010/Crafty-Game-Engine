using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using System.Runtime.InteropServices;
using System.Text;

namespace CraftyNative.ThreeD;

public unsafe static class Shaders
{
    public static byte[] CompileShader(string source, string entryPoint, string target)
    {
        ID3D10Blob* code = null;
        ID3D10Blob* errors = null;

        var compiler = D3DCompiler.GetApi();

        var sourcePtr = Marshal.StringToCoTaskMemUTF8(source);
        var entryPointPtr = Marshal.StringToCoTaskMemUTF8(entryPoint);
        var targetPtr = Marshal.StringToCoTaskMemUTF8(target);

        try
        {
            var result = compiler.Compile(
                (void*)sourcePtr,
                (nuint)Encoding.UTF8.GetByteCount(source),
                (string?)null,
                null,
                null,
                (byte*)entryPointPtr,
                (byte*)targetPtr,
                0,
                0,
                &code,
                &errors);

            if (result < 0)
                throw new InvalidOperationException($"Shader compilation failed. HRESULT: 0x{result:X8}");

            var bytecode = new byte[code->GetBufferSize()];

            Marshal.Copy(
                (nint)code->GetBufferPointer(),
                bytecode,
                0,
                bytecode.Length);

            return bytecode;
        }
        finally
        {
            if (errors is not null)
                errors->Release();

            if (code is not null)
                code->Release();

            Marshal.FreeCoTaskMem(sourcePtr);
            Marshal.FreeCoTaskMem(entryPointPtr);
            Marshal.FreeCoTaskMem(targetPtr);
        }
    }
}
