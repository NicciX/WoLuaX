using System.Diagnostics;
using System.IO;

using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using WoLuaX.Constants;

namespace WoLuaX.Lua;

public class ScriptLoader: IScriptLoader {
	public string BaseDir { get; }
	public string ScriptName { get; }

	public ScriptLoader(string folder, string script) {
        BaseDir = Path.TrimEndingDirectorySeparator(folder);
        ScriptName = script;
	}

	[Conditional("DEBUG")]
	private void debug(string message) => Service.Log.Information($"[{LogTag.ScriptLoader}:{ScriptName}] {message}");

	public string Clean(string dirty) => Path.ChangeExtension(Path.IsPathFullyQualified(dirty) ? dirty : Path.Join(BaseDir, dirty), "lua");

	public bool IsPathUnderScriptRoot(string name) {
		string absolute = Path.GetFullPath(name);
		string folder = Path.TrimEndingDirectorySeparator(Path.GetDirectoryName(absolute) ?? Path.GetPathRoot(absolute) ?? string.Empty) + Path.DirectorySeparatorChar;
		string root = BaseDir + Path.DirectorySeparatorChar;
		return folder.Length >= root.Length && folder.StartsWith(root);
	}

	// I think this is for when you try to load a file by path, like with `loadfile`
	public string ResolveFileName(string filename, Table globalContext) {
		string absolute = Clean(filename);
        debug($"Resolving file '{filename}' to {absolute}");
		return absolute;
	}

	// And I think this is for when you try to load a module like with `require`
	public string ResolveModuleName(string modname, Table globalContext) {
		string absolute = Clean(modname);
        debug($"Resolving module '{modname}' to {absolute}");
		return absolute;
	}

	// It looks like this needs to return a string consisting of the lua source to load
	public object LoadFile(string file, Table globalContext) {
		string absolute = Path.GetFullPath(file);
        debug($"Attempting to load {absolute}");

		if (!IsPathUnderScriptRoot(absolute))
			throw new ScriptRuntimeException($"Cannot load \"{absolute}\" (outside of module root \"{BaseDir}\")");

		if (!ScriptFileExists(absolute))
			return "return nil";

        debug($"Loading content from {absolute}");
		return File.ReadAllText(absolute);
	}

	// This one's pretty self-explanatory
	public bool ScriptFileExists(string name) {
		string absolute = Path.GetFullPath(name);
        debug($"Checking for existence/validity of {absolute}");
		return IsPathUnderScriptRoot(absolute) && File.Exists(absolute);
	}

}
