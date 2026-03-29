namespace RocksmithCableHack;

public sealed record HackResult(bool Success, long VidOffset, long PidOffset, string? Error = null)
{
    public static HackResult Fail(string error) => new(false, 0, 0, error);
    public static HackResult Ok(long vidOffset, long pidOffset) => new(true, vidOffset, pidOffset);
}
