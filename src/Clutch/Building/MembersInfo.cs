using System;
using System.Collections.Generic;
using System.Reflection;
using Clutch.Helpers;

namespace Clutch.Building
{
    public class MembersInfo
    {
        private readonly Dictionary<string, MemberInfo> _members = new Dictionary<string, MemberInfo>();

        private readonly Dictionary<string, MemberDeclarationSource> _newMembers = new Dictionary<string, MemberDeclarationSource>();

        public MembersInfo(TypeGraphNode node)
        {
            var type = node.Type;
            AddType(type);
            if (type.IsInterface)
            {
                type.GetInterfaces().ForEach(AddType);
            }
        }

        public FieldInfo GetField(string name)
        {
            if (_members.TryGetValue(name, out var memberInfo))
            {
                return memberInfo as FieldInfo;
            }
            return null;
        }

        public void AddNewMember(string name, MemberDeclarationSource source)
        {
            if (Contains(name, out _))
                throw new ClutchInternalErrorException($"member '{name}' is already declared");

            _newMembers.Add(name, source);
        }

        public bool Contains(string name, out string memberDescription)
        {
            if (_members.TryGetValue(name, out var member))
            {
                memberDescription = $"{member.GetType().Name} {name}";
                return true;
            }

            if (_newMembers.TryGetValue(name, out var source))
            {
                memberDescription = $"{source} {name}";
                return true;
            }

            memberDescription = String.Empty;
            return false;
        }

        private void AddType(Type type)
        {
            var baseType = type.BaseType;
            if (baseType != null)
                AddType(baseType);

            BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            AddGroup(type.GetMembers(flags));
        }

        private void AddGroup(IEnumerable<MemberInfo> items)
        {
            items.ForEach(s => _members[s.Name] = s);
        }

        public string GenerateUniqueFieldName(string propertyName)
        {
            var baseName = GetUndescoredFieldName(propertyName);
            var result = baseName;
            int counter = 1;
            while (Contains(result, out _))
                result = baseName + (counter++);
            return result;
        }

        private static string GetUndescoredFieldName(string propertyName)
        {
            return "_" + propertyName.Substring(0, 1).ToLower() + propertyName.Substring(1);
        }

        public FieldInfo FindBackingField(string propertyName)
        {
            return FindBackingFieldByConvention(propertyName) ??
                   GetField($"<{propertyName}>k__BackingField");
        }

        private FieldInfo FindBackingFieldByConvention(string propertyName)
        {
            return GetField(GetUndescoredFieldName(propertyName)) ?? 
                   GetField(propertyName.Substring(0, 1).ToLower() + propertyName.Substring(1)) ??
                   GetField("m_" + propertyName.Substring(0, 1).ToLower() + propertyName.Substring(1));
        }
    }

    public enum MemberDeclarationSource
    {
        BackingField,

    }
}
