using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Services;

using WoLua.Lua.Api;

using WoLua.Api;

namespace WoLua.Ipc;

public class WoLuaIpc : IDisposable {
	private bool isAvailable = false;
	private readonly ICallGateProvider<bool> cgIsAvailable;
	private readonly ICallGateProvider<string, string, string, string, bool> updateChat;

	public WoLuaIpc() {
		this.cgIsAvailable = Service.Interface.GetIpcProvider<bool>("WoLua.IsAvailable");
		this.cgIsAvailable.RegisterFunc(this.IsAvailable);
		//_getItemInfoProvider = Service.Interface.GetIpcProvider<uint, HashSet<(uint npcId, uint territory, (float x, float y))>?>("ItemVendorLocation.GetItemVendors");
		//_pluginLog = pluginLog;

		this.updateChat = Service.Interface.GetIpcProvider<string, string, string, string, bool>("WoLua.UpdateChat");
		this.updateChat.RegisterFunc(this.UpdateChat);

		this.RegisterFunctions();
		Service.Log.Information($"New Chat: Api Call");
		this.isAvailable = true;
		this.cgIsAvailable.SendMessage();
	}
	public bool IsAvailable() {
		Service.Log.Information($"New Chat: Api Call2");
		return this.isAvailable;
	}
	private bool UpdateChat(
		string msg,
		string sender,
		string chn,
		string match
	) {
		

		NewChat(msg, sender, chn, match);
		return true;
	}
	//private bool UpdateChat(string msg, string sender, string chn) {
		//NewChat(msg, sender, chn);
		//return msg;
	//}

	public void Dispose() {
		//_getItemInfoProvider.UnregisterFunc();
		//this.newChat.UnregisterFunc();
	}

	public void RegisterFunctions() {
		//this.updateChat = Service.Interface.GetIpcProvider<string, string, string, bool>("WoLua.UpdateChat");
		this.updateChat.RegisterFunc(this.UpdateChat);
		//_getItemInfoProvider.RegisterFunc(GetItemVendors);
		Service.Log.Information($"New Chat: Api Call2");
	}

	private static bool NewChat(string msg, string sender, string chn, string match) {
		WoLuaApi.Msg = msg;
		WoLuaApi.Sender = sender;
		WoLuaApi.Chn = chn;
		WoLuaApi.Match = match;
		WoLuaApi.Stamp = (uint)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
		Service.Log.Information($"ChatMatch Received: [{chn}] : [{sender}] : {msg}");
		Service.Log.Information($"ChatMatch Received :: Match: {match}");
		return true;
	}

	
}
