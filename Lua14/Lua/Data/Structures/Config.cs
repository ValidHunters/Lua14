using System.Text.Json.Serialization;

namespace Lua14.Lua.Data.Structures;

public readonly struct Config
{
    [JsonPropertyName("MainFile")]
    public required string MainFile { get; init; }
    [JsonPropertyName("Name")]
    public required string Name { get; init; }
}
