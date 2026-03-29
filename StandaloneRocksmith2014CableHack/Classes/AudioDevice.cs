using System;
using System.Globalization;

namespace RocksmithCableHack;

public record AudioDevice
{
    public string Id { get; }
    public string Name { get; }
    public string Vid { get; }
    public string Pid { get; }
    public byte[] VidBytes { get; }
    public byte[] PidBytes { get; }
    public bool IsConnected { get; }
    public bool IsValid => ((!string.IsNullOrEmpty(this.Vid)) && (!string.IsNullOrEmpty(this.Pid)));
    public string DeviceKey => $"{this.Vid}:{this.Pid}";

    public AudioDevice(string id, string name, string vid, string pid, bool isConnected = true)
    {
        this.Id = id;
        this.Name = name;
        this.Vid = vid;
        this.Pid = pid;
        this.IsConnected = isConnected;
        this.VidBytes = ParseHexBytesLE(vid);
        this.PidBytes = ParseHexBytesLE(pid);
    }

    public static AudioDevice CreateDisconnected(string name, string vid, string pid)
    {
        return new(string.Empty, name, vid, pid, isConnected: false);
    }

    private static byte[] ParseHexBytesLE(string hex)
    {
        if((string.IsNullOrEmpty(hex)) || (hex.Length < 4))
            return [];

        return
        [
            byte.Parse(hex.AsSpan(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture),
            byte.Parse(hex.AsSpan(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture)
        ];
    }

    public override string ToString() => (this.IsConnected) ?
        $"{this.Name,-30} VID: {this.Vid}  PID: {this.Pid}" :
        $"{this.Name,-30} VID: {this.Vid}  PID: {this.Pid}  (disconnected)";
}
