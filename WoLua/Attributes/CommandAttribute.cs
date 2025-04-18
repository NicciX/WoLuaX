using System;

namespace NicciX.WoLua.Attributes;

[AttributeUsage(AttributeTargets.Class)]
internal class CommandAttribute(string command, params string[] aliases): Attribute {
	public string Command { get; } = command;
	public string[] Aliases { get; } = aliases;
}
