namespace Lua14.Lua.Data.Structures;

public readonly struct Chunk
{
    public Chunk(string relativePath, string content)
    {
        RelativePath = relativePath;
        Content = content;
    }

    public string RelativePath { get; init; }
    public string Content { get; init; }
}
