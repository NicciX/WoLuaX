using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using WoLuaX.Constants;

namespace WoLuaX;

public sealed class MethodTimer: IDisposable {
	private readonly Stopwatch timer;
	private readonly string label;
	public long Elapsed => timer.ElapsedMilliseconds;
	public MethodTimer() {
		StackFrame caller = new(1);
		string owner = "????";
		string method = "????";
		string args = "????";
		if (caller.HasMethod()) {
			MethodBase func = caller.GetMethod()!;
			owner = func.DeclaringType?.FullName ?? owner;
			method = func.Name;
			args = string.Join(", ", func.GetParameters().Select(p => p.ParameterType.Name));
		}
        label = $"{owner}.{method}({args})";
        timer = Stopwatch.StartNew();
	}
	public void Dispose() {
        timer.Stop();
#if DEBUG
		Service.Log.Information($"[{LogTag.MethodTiming}] {this.label}: {this.Elapsed}ms");
#endif
	}
}
