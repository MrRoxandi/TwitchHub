namespace TwitchHub.Configurations;

public class LuaStorageContainerConfiguration
{
    public const string SectionName = "LuaStorage";
    public required string StorageDirectory { get; set; } = "data";
}
