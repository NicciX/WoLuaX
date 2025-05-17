using System;

namespace WoLuaX.Lua.Docs;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
internal class AsLuaTypeAttribute: Attribute {
	public string LuaName { get; }

	public AsLuaTypeAttribute(string luaType) => LuaName = luaType;
	public AsLuaTypeAttribute(LuaType luaType) : this(luaType.LuaName()) { }
}
