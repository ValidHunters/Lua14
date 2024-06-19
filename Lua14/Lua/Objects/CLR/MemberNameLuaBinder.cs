using System.Reflection;
using System.Collections.ObjectModel;
using Eluant.ObjectBinding;
using Eluant;

namespace Lua14.Lua.Objects.CLR
{
    using MemberNameMap = Dictionary<string, List<MemberInfo>>;

    public class MemberNameLuaBinder : ILuaBinder
    {
        private static readonly Dictionary<Type, MemberNameMap> memberNameCache = [];

        private static readonly MemberInfo[] noMembers = [];

        private readonly BindingFlags _binding;

        public MemberNameLuaBinder(BindingFlags binding)
        { 
            _binding = binding;
        }

        private MemberNameMap GetMembersByName(Type type)
        {
            var membersByName = new MemberNameMap();

            foreach (var member in type.GetMembers(_binding)) {
                var method = member as MethodInfo;
                if (method != null && method.IsGenericMethodDefinition) {
                    continue;
                }

                var memberName = member.Name;

                if (!membersByName.TryGetValue(memberName, out List<MemberInfo>? members))
                {
                    members = [];
                    membersByName[memberName] = members;
                }

                members.Add(member);
            }

            return membersByName;
        }

        #region ILuaBinder implementation

        public virtual ICollection<MemberInfo> GetMembersByName(object targetObject, string memberName)
        {
            ArgumentNullException.ThrowIfNull(targetObject, nameof(targetObject));
            ArgumentNullException.ThrowIfNull(memberName, nameof(memberName));

            var type = targetObject.GetType();

            MemberNameMap? memberNameMap;

            lock (memberNameCache) {
                if (!memberNameCache.TryGetValue(type, out memberNameMap)) {
                    memberNameMap = GetMembersByName(type);
                    memberNameCache[type] = memberNameMap;
                }
            }

            if (memberNameMap.TryGetValue(memberName, out List<MemberInfo>? members))
            {
                return new ReadOnlyCollection<MemberInfo>(members);
            }

            return noMembers;
        }

        public virtual MethodInfo ResolveOverload(ICollection<MemberInfo> possibleOverloads, LuaVararg arguments)
        {
            throw new NotImplementedException("Overload resolution is not yet supported.");
        }

        public virtual LuaValue ObjectToLuaValue(object obj, IBindingContext bindingContext, LuaRuntime runtime)
        {
            return runtime.AsLuaValue(obj) ??
                new LuaTransparentClrObject(obj, bindingContext.Binder, bindingContext.BindingSecurityPolicy);
        }

        #endregion
    }
}
