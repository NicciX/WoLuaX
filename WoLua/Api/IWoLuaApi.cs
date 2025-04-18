using System.Collections.Generic;

using NicciX.WoLua;


namespace NicciX.WoLua.Api;

public interface IWoLuaApi
{
	public int ApiVersion { get; }

	/// <summary>
	/// Adds entry to VoidList
	/// </summary>
	/// <param name="name">Full player name</param>
	/// <param name="worldId">World ID</param>
	/// <param name="reason">Reason for adding</param>
	public void NewChat(string msg, string sender, string chn, string match);

	
}
