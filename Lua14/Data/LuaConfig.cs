using System.Text.Json.Serialization;

namespace Lua14.Data;

public class LuaConfig
{
    [JsonPropertyName("mainFile")]
    public required string MainFile { get; set; }
    [JsonPropertyName("name")]
    public required string Name { get; set; }
}
