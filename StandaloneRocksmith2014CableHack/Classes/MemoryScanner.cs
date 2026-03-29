using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace RocksmithCableHack;

[SupportedOSPlatform("windows")]
public static class MemoryScanner
{
    private const int ChunkSize = 65536;

    [DllImport("kernel32.dll", SetLastError = false, PreserveSig = true)]
    public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, out uint bytesRead);

    [DllImport("kernel32.dll", SetLastError = false, PreserveSig = true)]
    public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, out uint bytesWritten);

    public static long TryCachedOffset(IntPtr processHandle, long baseAddress, long offset, byte[] expectedPattern)
    {
        if(offset <= 0)
            return -1;

        long address = baseAddress + offset;
        var buffer = new byte[expectedPattern.Length];
        if(!ReadProcessMemory(processHandle, new IntPtr(address), buffer, (uint)buffer.Length, out uint read))
            return -1;

        if(read != (uint)buffer.Length)
            return -1;

        return buffer.AsSpan().SequenceEqual(expectedPattern) ? address : -1;
    }

    public static long FindPattern(IntPtr processHandle, long startAddress, long endAddress, byte[] pattern)
    {
        int overlap = pattern.Length - 1;
        var buffer = new byte[ChunkSize + overlap];
        long current = startAddress;
        byte firstByte = pattern[0];
        while(current < endAddress)
        {
            uint toRead = (uint)Math.Min(buffer.Length, endAddress - current);
            if(!ReadProcessMemory(processHandle, new IntPtr(current), buffer, toRead, out uint bytesRead))
            {
                current += ChunkSize;
                continue;
            }

            if(bytesRead < (uint)pattern.Length)
            {
                current += ChunkSize;
                continue;
            }

            var span = buffer.AsSpan(0, (int)bytesRead);
            int searchStart = 0;
            while(searchStart <= span.Length - pattern.Length)
            {
                int offset = span[searchStart..].IndexOf(firstByte);
                if(offset < 0)
                    break;

                int position = searchStart + offset;
                if(position + pattern.Length > span.Length)
                    break;

                if(span.Slice(position, pattern.Length).SequenceEqual(pattern))
                    return current + position;

                searchStart = position + 1;
            }

            current += ChunkSize;
        }

        return -1;
    }

    public static bool WriteAndVerify(IntPtr processHandle, long address, byte[] data)
    {
        var dataArray = data.ToArray();
        if(!WriteProcessMemory(processHandle, new IntPtr(address), dataArray, (uint)dataArray.Length, out uint written))
            return false;

        if(written != (uint)dataArray.Length)
            return false;

        var verify = new byte[dataArray.Length];
        if(!ReadProcessMemory(processHandle, new IntPtr(address), verify, (uint)verify.Length, out _))
            return false;

        return verify.AsSpan().SequenceEqual(data);
    }
}