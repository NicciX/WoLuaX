using System;

namespace NicciX.WoLua.Lua;

[AttributeUsage(AttributeTargets.Property)]
internal class LuaGlobalAttribute: Attribute {
	public readonly string Name;
	public LuaGlobalAttribute(string name) => this.Name = name;
}
