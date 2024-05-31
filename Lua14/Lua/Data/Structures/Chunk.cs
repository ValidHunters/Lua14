namespace Lua14.Lua.Data.Structures;

public readonly struct Chunk
{
    public Chunk(string path, string content)
    {
        Path = path;
        Content = content;
    }

    public string Path { get; init; }
    public string Content { get; init; }
}
