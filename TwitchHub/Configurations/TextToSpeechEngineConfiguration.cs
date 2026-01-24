namespace TwitchHub.Configurations;

public class TextToSpeechEngineConfiguration
{
    public const string SectionName = "TTSEngine";
    public bool Enabled { get; set; } = true;
    public int Volume { get; set; } = 100;
    public int Rate { get; set; } = 0;
    public string? Voice { get; set; }
    public string? BannedWordsFilePath { get; set; }
}