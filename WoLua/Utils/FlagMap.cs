using System.Collections.Generic;
using System.Linq;

namespace WoLuaX.Utils;

public class FlagMap: Dictionary<string, bool> {
	public new bool this[string key] {
		get {
			if (string.IsNullOrEmpty(key))
				return false;
			bool found = TryGetValue(key, out bool val);
			return found && val;
		}
		set {
			if (!string.IsNullOrEmpty(key)) {
                Remove(key);
                Add(key, value);
			}
		}
	}
	public bool this[char key] {
		get => this[key.ToString()];
		set => this[key.ToString()] = value;
	}

	public void SetAll(IEnumerable<string> keys) {
		foreach (string key in keys) {
			this[key] = true;
		}
	}
	public void SetAll(IEnumerable<char> keys) => SetAll(keys.Select(c => c.ToString()));

	public void Set(params string[] keys) => SetAll(keys);
	public void Set(params char[] keys) => SetAll(keys);

	public void ToggleAll(IEnumerable<string> keys) {
		foreach (string key in keys) {
			this[key] = !this[key];
		}
	}
	public void ToggleAll(IEnumerable<char> keys) => ToggleAll(keys.Select(c => c.ToString()));

	public void Toggle(params string[] keys) => ToggleAll(keys);
	public void Toggle(params char[] keys) => ToggleAll(keys);

	public bool Get(string key) => this[key];
	public bool Get(char key) => Get(key.ToString());

	public override string ToString()
		=> string.Join(", ", Keys.Where(k => this[k]));
}
