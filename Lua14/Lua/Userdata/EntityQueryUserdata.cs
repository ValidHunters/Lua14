using HarmonyLib;
using NLua;
using Robust.Shared.GameObjects;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Lua14.Lua.Userdata;

/// <summary>
/// EntityQueryEnumerator but for lua
/// </summary>
public sealed class EntityQueryUserdata : LuaUserdata
{
    private static readonly FieldInfo F_EntTraitArray = AccessTools.Field(typeof(EntityManager), "_entTraitArray");

    private static readonly MethodInfo M_ArrayIndex = AccessTools.Method(typeof(CompIdx), "ArrayIndex", [typeof(Type)]);
    private static int ArrayIndex(Type type) => (int)M_ArrayIndex.Invoke(null, [type])!;

    private LuaEntityQueryEnumerator _query;

    public EntityQueryUserdata(NLua.Lua lua, EntityManager ent, ComponentIndexUserdata[] indexes) : base(lua)
    {
        var entTraitArray = (Dictionary<EntityUid, IComponent>[]?) F_EntTraitArray.GetValue(ent)
            ?? throw new Exception("_entTraitArray in EntityManager was null.");
        var traitDicts = indexes
            .Select(index => entTraitArray[ArrayIndex(index.Type)])
            .ToArray();

        _query = new(traitDicts, ent.MetaQuery);
    }

    [LuaMember(Name = "next")]
    public object[]? Next()
    {
        if (!_query.MoveNext(out var entityUid, out var comps))
            return null;

        return [entityUid, .. comps];
    }

    override protected object[]? Call(LuaUserdata self, params object[] args)
    {
        return Next();
    }

    override protected LuaFunction Iter(LuaUserdata self)
    {
        return (LuaFunction)Userdata["next"];
    }

    private struct LuaEntityQueryEnumerator: IDisposable
    {
        private Dictionary<EntityUid, IComponent>.Enumerator _enumerator;
        private readonly Dictionary<EntityUid, IComponent>[] _traitDicts;
        private readonly EntityQuery<MetaDataComponent> _metaQuery;

        public LuaEntityQueryEnumerator(Dictionary<EntityUid, IComponent>[] traitDicts, EntityQuery<MetaDataComponent> metaQuery)
        {
            _enumerator = traitDicts[0].GetEnumerator();
            _traitDicts = traitDicts;
            _metaQuery = metaQuery;
        }

        public bool MoveNext(out EntityUid uid, [NotNullWhen(true)] out IComponent[]? comps)
        {
            List<IComponent> compsList = [];
            while (true)
            {
                if (!_enumerator.MoveNext())
                {
                    uid = default;
                    comps = default;
                    return false;
                }

                var current = _enumerator.Current;

                if (current.Value.Deleted)
                {
                    continue;
                }

                if (!_metaQuery.TryGetComponent(current.Key, out var metaComp) || metaComp.EntityPaused)
                {
                    continue;
                }

                var skip = false;
                foreach (var traitDict in _traitDicts)
                {
                    if (!traitDict.TryGetValue(current.Key, out var compObj) || compObj.Deleted)
                    {
                        skip = true;
                        continue;
                    }
                    compsList.Add(compObj);
                }

                if (skip)
                    continue;

                uid = current.Key;
                comps = [.. compsList];
                return true;
            }
        }

        public void Dispose()
        {
            _enumerator.Dispose();
        }
    }
}
