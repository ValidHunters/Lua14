namespace Lua14.Mod;

internal sealed class LuaFile
{
    public LuaFile(string relativePath, string content)
    {
        RelativePath = relativePath;
        Content = content;
    }

    public string RelativePath;
    public string Content;
}
