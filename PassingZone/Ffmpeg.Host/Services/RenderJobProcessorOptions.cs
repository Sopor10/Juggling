namespace Ffmpeg.Host.Services;

public class RenderJobProcessorOptions
{
    public string InputDirectory { get; set; } = string.Empty;
    public int ScanIntervalSeconds { get; set; } = 5;
}
