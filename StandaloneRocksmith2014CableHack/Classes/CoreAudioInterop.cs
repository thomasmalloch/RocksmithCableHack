using System;
using System.Runtime.InteropServices;


namespace RocksmithCableHack;

public enum EDataFlow : uint
{
    Render = 0,
    Capture = 1,
    All = 2
}

public enum ERole : uint
{
    Console = 0,
    Multimedia = 1,
    Communications = 2
}

[StructLayout(LayoutKind.Sequential)]
public struct PROPERTYKEY(Guid formatId, uint propertyId)
{
    public Guid FormatId = formatId;
    public uint PropertyId = propertyId;
}

[StructLayout(LayoutKind.Explicit, Size = 24)]
public struct PROPVARIANT : IDisposable
{
    [FieldOffset(0)] public ushort VarType;
    [FieldOffset(8)] public IntPtr Value;

    private const ushort VT_LPWSTR = 31;

    public readonly string? AsString()
    {
        if((this.VarType == VT_LPWSTR) && (this.Value != IntPtr.Zero))
            return Marshal.PtrToStringUni(this.Value);

        return null;
    }

    public void Dispose()
    {
        PropVariantClear(ref this);
    }

    [DllImport("ole32.dll")]
    private static extern int PropVariantClear(ref PROPVARIANT pvar);
}

[ComImport]
[Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IMMDeviceEnumerator
{
    [PreserveSig]
    int EnumAudioEndpoints(EDataFlow dataFlow, uint stateMask, out IMMDeviceCollection devices);

    [PreserveSig]
    int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice device);

    [PreserveSig]
    int GetDevice([MarshalAs(UnmanagedType.LPWStr)] string id, out IMMDevice device);

    [PreserveSig]
    int RegisterEndpointNotificationCallback(IntPtr client);

    [PreserveSig]
    int UnregisterEndpointNotificationCallback(IntPtr client);
}

[ComImport]
[Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IMMDeviceCollection
{
    [PreserveSig]
    int GetCount(out uint count);

    [PreserveSig]
    int Item(uint index, out IMMDevice device);
}

[ComImport]
[Guid("D666063F-1587-4E43-81F1-B948E807363F")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IMMDevice
{
    [PreserveSig]
    int Activate(
        ref Guid iid,
        uint clsCtx,
        IntPtr activationParams,
        [MarshalAs(UnmanagedType.IUnknown)] out object interfacePointer);

    [PreserveSig]
    int OpenPropertyStore(uint stgmAccess, out IPropertyStore properties);

    [PreserveSig]
    int GetId([MarshalAs(UnmanagedType.LPWStr)] out string id);

    [PreserveSig]
    int GetState(out uint state);
}

[ComImport]
[Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IPropertyStore
{
    [PreserveSig]
    int GetCount(out uint count);

    [PreserveSig]
    int GetAt(uint index, out PROPERTYKEY key);

    [PreserveSig]
    int GetValue(ref PROPERTYKEY key, out PROPVARIANT value);

    [PreserveSig]
    int SetValue(ref PROPERTYKEY key, ref PROPVARIANT value);

    [PreserveSig]
    int Commit();
}

public static class PropertyKeys
{
    /// <summary>e.g. "Microphone (HyperX Cloud III)"</summary>
    public static PROPERTYKEY DeviceFriendlyName = new(new Guid("A45C254E-DF1C-4EFD-8020-67D146A850E0"), 14);

    /// <summary>e.g. "HyperX Cloud III"</summary>
    public static PROPERTYKEY DeviceInterfaceFriendlyName = new(new Guid("026E516E-B814-414B-8384-C2E8FD8AD5D3"), 2);

    /// <summary>e.g. "Microphone"</summary>
    public static PROPERTYKEY DeviceDescription = new(new Guid("A45C254E-DF1C-4EFD-8020-67D146A850E0"), 2);
}

public static class CoreAudioConstants
{
    public static readonly Guid MMDeviceEnumeratorClsid = new("BCDE0395-E52F-467C-8E3D-C4579291692E");

    public const uint DEVICE_STATE_ACTIVE = 0x00000001;
    public const uint STGM_READ = 0x00000000;
}