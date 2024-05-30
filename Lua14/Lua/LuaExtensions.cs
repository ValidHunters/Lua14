using HarmonyLib;
using NLua;
using System.Reflection;

namespace Lua14.Lua;

public static class LuaExtensions
{
    private static readonly FieldInfo F_Translator = AccessTools.Field(typeof(NLua.Lua), "_translator")
        ?? throw new Exception("Field _translator doesnt exist on NLua.Lua");
    private static readonly MethodInfo M_GetUserData = AccessTools.Method(typeof(ObjectTranslator), "GetUserData")
        ?? throw new Exception("Method GetUserData doesnt exist on ObjectTranslator");

    private const string TempPath = "_LUA14_TEMP";

    #region Tables
    public static LuaTable NewTable(this NLua.Lua lua)
    {
        lua.NewTable(TempPath);
        var table = lua.GetTable(TempPath);
        lua[TempPath] = null;
        return table;
    }

    public static void SetFunction(this NLua.Lua lua, LuaTable table, string path, MethodBase method, object target = null)
    {
        lua[TempPath] = table;
        lua.RegisterFunction($"{TempPath}.{path}", target, method);
        lua[TempPath] = null;
    }

    public static LuaTable EnumerableToTable<T>(this NLua.Lua lua, IEnumerable<T> enumerable)
    {
        var table = lua.NewTable();
        int index = 0;

        foreach (var item in enumerable)
        {
            table[index++] = item;
        }

        return table;
    }

    public static Dictionary<T1, T2> TableToDictionary<T1, T2>(this NLua.Lua lua, LuaTable table) where T1 : notnull
    {
        Dictionary<object, object> dictionary = lua.GetTableDict(table);
        Dictionary<T1, T2> result = [];
        foreach (var pair in dictionary)
        {
            if (pair.Key is not T1 keyCasted || pair.Value is not T2 valueCasted)
            {
                return null;
            }
            result[keyCasted] = valueCasted;
        }

        return result;
    }

    public static IEnumerable<T> TableToEnumerable<T>(this NLua.Lua lua, LuaTable table)
    {
        var dictionary = lua.TableToDictionary<int, T>(table);
        if (dictionary == null)
            return null;

        List<T> result = [];
        foreach (var pair in dictionary)
        {
            result[pair.Key] = pair.Value;
        }

        return result;
    }

    public static LuaTable GetMetatable(this NLua.Lua lua, LuaTable table)
    {
        return lua.GetFunction("getmetatable").Call(table).FirstOrDefault() as LuaTable;
    }

    public static LuaTable SetMetatable(this NLua.Lua lua, LuaTable table, LuaTable metatable)
    {
        return (LuaTable)lua.GetFunction("setmetatable").Call(table, metatable).First();
    }

    /// <summary>
    /// gets an object from a LuaTable, ignoring its metatable
    /// </summary>
    public static object RawGet(this NLua.Lua lua, LuaTable table, object key)
    {
        return lua.GetFunction("rawget").Call(table, key).FirstOrDefault();
    }

    /// <summary>
    /// sets an object in a LuaTable, ignoring its metatable
    /// </summary>
    public static void RawSet(this NLua.Lua lua, LuaTable table, object key, object value)
    {
        lua.GetFunction("rawset").Call(table, key, value);
    }
    #endregion

    #region Userdata
    public static LuaUserData ObjectToUserdata<T>(this NLua.Lua lua, T target) where T : class
    {
        var luaState = lua.State;
        var translator = GetTranslator(lua);
        int oldTop = luaState.GetTop();

        lua[TempPath] = target;
        luaState.GetGlobal(TempPath);
        var userdata = GetUserdata(translator, luaState, -1);

        luaState.SetTop(oldTop);
        return userdata;
    }

    public static LuaTable GetMetatable(this NLua.Lua lua, LuaUserData userdata)
    {
        return lua.GetFunction("getmetatable").Call(userdata).FirstOrDefault() as LuaTable;
    }

    public static LuaTable SetMetatable(this NLua.Lua lua, LuaUserData userdata, LuaTable metatable)
    {
        return (LuaTable)lua.GetFunction("setmetatable").Call(userdata, metatable).First();
    }
    #endregion

    #region Translator
    public static ObjectTranslator GetTranslator(this NLua.Lua lua)
    {
        return (ObjectTranslator)F_Translator.GetValue(lua)!;
    }

    public static LuaUserData GetUserdata(this ObjectTranslator translator, NLua.Native.LuaState luaState, int index)
    {
        return (LuaUserData)M_GetUserData.Invoke(translator, [luaState, index])!;
    }
    #endregion
}
