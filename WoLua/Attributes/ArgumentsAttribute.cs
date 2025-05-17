using System;
using System.Linq;

namespace WoLuaX.Attributes;

[AttributeUsage(AttributeTargets.Class)]
internal class ArgumentsAttribute(params string[] args): Attribute {
	public string ArgumentDescription => string.Join(" ", Arguments.Select(a => a.EndsWith('?') ? $"[{a.TrimEnd('?')}]" : $"<{a}>"));
	public string[] Arguments { get; } = args;
	public int RequiredArguments => Arguments.Count(a => !a.EndsWith('?'));
	public int MaxArguments => Arguments.Length;
}
