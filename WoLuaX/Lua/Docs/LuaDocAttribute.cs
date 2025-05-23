using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WoLuaX.Lua.Docs;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.ReturnValue, Inherited = true)]
internal partial class LuaDocAttribute: Attribute {
	private static readonly Regex lineBreakTag = lineBreakRegex();

	public string Description { get; protected init; }
	public string[] Lines => this.Description.Split('\n', StringSplitOptions.TrimEntries);

	public LuaDocAttribute(params string[] help) => this.Description = CleanJoin(help);

	[GeneratedRegex("<br\\s*/?>", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
	private static partial Regex lineBreakRegex();
	protected static string CleanLine(string line) => lineBreakTag.Replace(line, "\n");
	protected static IEnumerable<string> CleanLines(IEnumerable<string> lines) => lines.Select(CleanLine);
	protected static string CleanJoin(IEnumerable<string> lines) => string.Join("\n", CleanLines(lines));
}
