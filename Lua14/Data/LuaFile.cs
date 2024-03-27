namespace Lua14.Data;

public sealed class LuaFile
{
    public LuaFile(string relativePath, string content)
    {
        RelativePath = relativePath;
        Content = content;
    }

    public string RelativePath;
    public string Content;
}
