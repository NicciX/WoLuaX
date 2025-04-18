using System;
using System.Runtime.Serialization;

namespace NicciX.WoLua;

[Serializable]
public class CommandAssertionFailureException: Exception {
	public CommandAssertionFailureException() { }
	public CommandAssertionFailureException(string? message) : base(message) { }
	public CommandAssertionFailureException(string? message, Exception? inner) : base(message, inner) { }
}
