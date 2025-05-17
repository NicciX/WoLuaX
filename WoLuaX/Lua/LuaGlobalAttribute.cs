using System;

namespace WoLuaX.Lua;

[AttributeUsage(AttributeTargets.Property)]
internal class LuaGlobalAttribute: Attribute {
	public readonly string Name;
	public LuaGlobalAttribute(string name) => Name = name;
}
