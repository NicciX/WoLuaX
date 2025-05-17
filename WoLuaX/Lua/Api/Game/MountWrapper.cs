using System;
using System.Collections.Generic;

using Lumina.Excel;
using Lumina.Excel.Sheets;

using MoonSharp.Interpreter;
using WoLuaX.Constants;

namespace WoLuaX.Lua.Api.Game;

[MoonSharpUserData]
[MoonSharpHideMember("<Clone>$")]
[MoonSharpHideMember(nameof(Equals))]
public sealed record class MountWrapper: IEquatable<MountWrapper> { // TODO luadoc all of this
	internal static readonly Dictionary<ushort, string> mountNames = [];
	internal static readonly Dictionary<ushort, string> mountArticles = [];
	internal static void LoadGameData() {
		using MethodTimer logtimer = new();

		ExcelSheet<Mount> mounts = Service.DataManager.GetExcelSheet<Mount>()!;
		foreach (Mount mount in mounts) {
			mountNames[(ushort)mount.RowId] = mount.Singular.ToString();
			mountArticles[(ushort)mount.RowId] = "A" + (mount.StartsWithVowel > 0 ? "n" : string.Empty);
		}
		mountNames.Remove(0);
		mountArticles.Remove(0);
	}

	public bool Active { get; }
	public ushort Id { get; }
	public string? Name { get; }
	public string? LowercaseArticle { get; }
	public string? UppercaseArticle { get; }

	public MountWrapper(ushort id) {
        Active = mountNames.TryGetValue(id, out string? name);
        Id = Active ? id : (ushort)0;
        Name = name;
        UppercaseArticle = Active ? mountArticles[id] : null;
        LowercaseArticle = UppercaseArticle?.ToLower();
	}

	[MoonSharpUserDataMetamethod(Metamethod.Stringify)]
	public override string ToString() => Name ?? string.Empty;
}
