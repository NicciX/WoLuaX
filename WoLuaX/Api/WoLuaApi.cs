using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using Lumina.Excel.Sheets;

using MoonSharp.Interpreter;

using WoLuaX.Lua.Actions;
using WoLuaX.Lua.Api;

using static System.Runtime.InteropServices.JavaScript.JSType;
using static FFXIVClientStructs.FFXIV.Client.Graphics.Render.ModelRenderer;

namespace WoLuaX.Api;

public class WoLuaApi : IDisposable, IWoLuaApi
{
    public int ApiVersion => 1;

    private bool initialised;

    public WoLuaApi() => this.initialised = true;

	public static string Msg { get; set; } = string.Empty;
	public static string Sender { get;  set; } = string.Empty;
	public static string Chn { get;  set; } = string.Empty;
	public static string Match { get; set; } = string.Empty;
	public static uint Stamp { get; set; } = 0;

	private void CheckInitialised()
    {
        if (!this.initialised)
            throw new Exception("PluginShare is not initialised.");
    }

    public void NewChat(string msg, string sender, string chn, string match)
    {
        this.CheckInitialised();

        //Service.Log.Information($"New Chat: {msg}");
		Msg = msg;
		Sender = sender;
		Chn = chn;
		Match = match;
		Stamp = (uint)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
		
	}
	

    public void Dispose() => this.initialised = false;
}
