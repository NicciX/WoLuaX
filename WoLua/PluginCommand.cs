using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Dalamud.Game.Command;
using WoLuaX.Attributes;
using WoLuaX.Chat;
using WoLuaX.Utils;

namespace WoLuaX;

public abstract class PluginCommand: IDisposable {
	protected bool Disposed { get; set; } = false;

	public CommandInfo MainCommandInfo => new(Dispatch) {
		HelpMessage = Summary,
		ShowInHelp = ShowInDalamud,
	};
	public CommandInfo AliasCommandInfo => new(Dispatch) {
		HelpMessage = Summary,
		ShowInHelp = false,
	};
	public string CommandComparable => Command.TrimStart('/').ToLower();
	public IEnumerable<string> AliasesComparable => Aliases.Select(s => s.TrimStart('/').ToLower());
	public IEnumerable<string> HelpLines => Help.Split('\r', '\n').Where(s => !string.IsNullOrWhiteSpace(s));
	public IEnumerable<string> InvocationNames => (new string[] { Command }).Concat(Aliases);
	public IEnumerable<string> InvocationNamesComparable => (new string[] { CommandComparable }).Concat(AliasesComparable);
	public string Command { get; }
	public string Summary { get; }
	public string Help { get; }
	public string Usage { get; }
	public string[] Aliases { get; }
	public bool ShowInDalamud { get; }
	public bool ShowInListing { get; }

	protected Plugin Plugin { get; private set; }

	public string InternalName { get; init; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	protected PluginCommand() {
		Type t = GetType();
		CommandAttribute attrCommand = t.GetCustomAttribute<CommandAttribute>() ?? throw new InvalidOperationException("Cannot construct PluginCommand from type without CommandAttribute");
		ArgumentsAttribute? args = t.GetCustomAttribute<ArgumentsAttribute>();
        Command = $"/{attrCommand.Command.TrimStart('/')}";
        Summary = t.GetCustomAttribute<SummaryAttribute>()?.Summary ?? "";
        Help = ModifyHelpMessage(t.GetCustomAttribute<HelpTextAttribute>()?.HelpMessage ?? "");
        Usage = $"{Command} {args?.ArgumentDescription}".Trim();
        Aliases = (new string[] { "9999p" + Command.TrimStart('/') })
			.Concat(
				attrCommand.Aliases
					.Select(s => s.TrimStart('/'))
					.SelectMany(a => new string[] { "0000" + a, "9999p" + a })
			)
			.OrderBy(s => s)
			.Select(s => "/" + s[4..])
			.ToArray();
        ShowInListing = t.GetCustomAttribute<HideInCommandListingAttribute>() is null;
        ShowInDalamud = ShowInListing && (t.GetCustomAttribute<DoNotShowInHelpAttribute>() is null || string.IsNullOrEmpty(Summary));

        InternalName = t.Name;

		if (Plugin is null)
			Plugin.Log.Warning($"{InternalName}.Plugin is null in constructor - this should not happen!");
	}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

	protected abstract void Execute(string? command, string rawArguments, FlagMap flags, bool verbose, bool dryRun, ref bool showHelp);
	protected virtual string ModifyHelpMessage(string original) => original;
	protected virtual void Initialise() { }
	internal void Setup() {
		Plugin.Log.Information($"Initialising {InternalName}");
        Initialise();
	}

	protected static void Assert(bool succeeds, string message) {
		if (!succeeds)
			throw new CommandAssertionFailureException(message);
	}

	public void Dispatch(string command, string argline) {
		if (Disposed)
			throw new ObjectDisposedException(InternalName, "Plugin command has already been disposed");

		Plugin.Log.Information($"Command invocation: [{command}] [{argline}]");
		try {
			(FlagMap flags, string rawArguments) = ArgumentParser.ExtractFlags(argline);
			Plugin.Log.Information($"Parsed flags: {flags}");
			Plugin.Log.Information($"Remaining argument line: [{rawArguments}]");
			bool showHelp = false;
			bool verbose = flags['?'];
			bool dryRun = flags['!'];
			if (flags["h"]) {
				Plugin.CommandManager.HelpHandler?.Execute(null, command, flags, verbose, dryRun, ref showHelp);
				return;
			}
            Execute(command, rawArguments, flags, verbose, dryRun, ref showHelp);
			if (showHelp)
				Plugin.CommandManager.HelpHandler?.Execute(null, command, flags, verbose, dryRun, ref showHelp);
		}
		catch (CommandAssertionFailureException e) {
			Plugin.Log.Error(e, $"Command assert failed: {Command}: {e.Message}");
			Plugin.CommandManager.ErrorHandler?.Invoke($"Internal assertion check failed:\n{e.Message}");
		}
		catch (Exception e) {
			Plugin.Log.Error(e, "Command invocation failed");
			if (Plugin.CommandManager.ErrorHandler is not null) {
				while (e is not null) {
					Plugin.CommandManager.ErrorHandler.Invoke(
						$"{e.GetType().Name}: {e.Message}\n",
						ChatColour.QUIET,
						e.TargetSite?.DeclaringType is not null ? $"at {e.TargetSite.DeclaringType.FullName} in {e.TargetSite.DeclaringType.Assembly.GetName().Name}" : "at unknown location",
						ChatColour.RESET
					);
					e = e.InnerException!;
				}
			}
		}
	}

	#region IDisposable
	protected virtual void Dispose(bool disposing) {
		if (Disposed)
			return;
        Disposed = true;

        Plugin = null!;
	}

	public void Dispose() {
        Dispose(true);
		GC.SuppressFinalize(this);
	}
	#endregion
}
