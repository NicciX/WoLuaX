using System;
using System.Diagnostics.CodeAnalysis;

using MoonSharp.Interpreter;

namespace WoLuaX.Lua.Api.Game;

[MoonSharpUserData]
[MoonSharpHideMember(nameof(Equals))]
[MoonSharpHideMember("<Clone>$")]
public record class EorzeanTime: IComparable<EorzeanTime>, IEquatable<EorzeanTime> {
	public const double ConversionRate = 144d / 7d;

	public byte Hour { get; init; }
	public byte Minute { get; init; }

	[MoonSharpHidden]
	public EorzeanTime(byte hour, byte minute) {
        Hour = hour;
        Minute = minute;
	}
	[MoonSharpHidden]
	public EorzeanTime(long realMs) {
		long totalMinutes = (long)(realMs * ConversionRate / 60000);
        Hour = (byte)(totalMinutes / 60 % 24);
        Minute = (byte)(totalMinutes % 60);
	}
	[MoonSharpHidden]
	public EorzeanTime() : this(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) { }

	[MoonSharpHidden]
	public int CompareTo(EorzeanTime? other) {
		return other is null
			? 1
			: Hour > other.Hour
			? 1
			: Hour < other.Hour
			? -1
			: Minute > other.Minute
			? 1
			: Minute < other.Minute
			? -1
			: 0;
	}

	[MoonSharpHidden]
	public static bool operator <(EorzeanTime left, EorzeanTime right) => left.CompareTo(right) < 0;

	[MoonSharpHidden]
	public static bool operator <=(EorzeanTime left, EorzeanTime right) => left.CompareTo(right) <= 0;

	[MoonSharpHidden]
	public static bool operator >(EorzeanTime left, EorzeanTime right) => left.CompareTo(right) > 0;

	[MoonSharpHidden]
	public static bool operator >=(EorzeanTime left, EorzeanTime right) => left.CompareTo(right) >= 0;
}
