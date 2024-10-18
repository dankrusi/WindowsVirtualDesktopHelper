using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using WindowsInput.Native;

namespace WindowsVirtualDesktopHelper {

	/// <summary>
	/// Settings class to manage settings and configuration.
	/// Settings can be loaded from launch arguments, config file, or defaults, and are internally stored as object (which can be string, int, bool or float).
	/// Each key for a setting can be given a namespace, e.g. "icons.automaticallyHidePrevNextOnBounds" or "general.language".
	/// The config file is a text file in the following format:
	/// ```txt
	/// general.language: "en"
	/// icons.automaticallyHidePrevNextOnBounds: true
	/// feature.showSplashScreen.text: "Virtual \nDesktop Helper"
	/// ```
	/// </summary>
	class Settings {

		#region Defaults

		public static void LoadDefaults() {
			// Register any known default settings here

			// General
			RegisterDefault("general.startupWithWindows", false, "If true, the app will register itself with Windows to startup when Windows starts (via the registry).");
			RegisterDefault("general.theme", "auto", "Can be either auto, dark or light. If set to auto, the theme is derived from the current windows theme (dark or light).");
			
			// Theme
			RegisterDefault("theme.icons.disabledOpacity", 0.5, "Defines the opacity to use for icons which are disabled.");
			RegisterDefault("theme.icons.font", "Segoe UI", "Defines the font name to use for the icons (for regular numbers, characters).");
			RegisterDefault("theme.icons.emojiFont", "Segoe UI Symbol", "Defines the font name to use for emoji icons.");
			RegisterDefault("theme.icons.symbolsFont", "Segoe UI Symbol", "Defines the font name to use for symbol icons.");
			RegisterDefault("theme.icons.iconBG.dark", "black");
			RegisterDefault("theme.icons.iconFG.dark", "white");
			RegisterDefault("theme.icons.iconBG.light", "white");
			RegisterDefault("theme.icons.iconFG.light", "black");
			RegisterDefault("theme.overlay.width", 900, "With width in pixels of the overlay.");
			RegisterDefault("theme.overlay.height", 430, "With height in pixels of the overlay.");
			RegisterDefault("theme.overlay.font", "Segoe UI Light", "Defines the font name to use for the overlay.");
			RegisterDefault("theme.overlay.fontSize", 30, "Defines the font size to use for the overlay.");
			RegisterDefault("theme.overlay.overlayBG.dark", "black");
			RegisterDefault("theme.overlay.overlayFG.dark", "white");
			RegisterDefault("theme.overlay.overlayBG.light", "black");
			RegisterDefault("theme.overlay.overlayFG.light", "white");

			// Feature: splash
			RegisterDefault("feature.showSplashScreen", true, "If enabled, a splash screen is shown on startup of the app. Overlays must be enabled.");
			RegisterDefault("feature.showSplashScreen.duration", 2000, "Splash duration in milliseconds.");
			RegisterDefault("feature.showSplashScreen.text", "Virtual Desktop Helper", "The splash text to show.");

			// Feature: showPrevNextIcons
			RegisterDefault("feature.showPrevNextIcons", true, "If enabled, a previous and next arrow will appear in the icons tray of Windows to allow easy switching between desktops.");
			RegisterDefault("feature.showPrevNextIcons.automaticallyHidePrevNextOnBounds", false, "If enabled, the prev/next icon will automatically hide if there is no prev/next desktop.");
			RegisterDefault("feature.showPrevNextIcons.nextChar", "\u203A", "Defines the character to use for next desktop icon (typically a unicode character like the chevron, for example \\xE101 = skip forward (player style), \xE111 = next (arrow style), \\xe26b = next (chevron style), \\u02C3 = next (chevron style), \\u203A = next (chevron style))");
			RegisterDefault("feature.showPrevNextIcons.prevChar", "\u2039", "Defines the character to use for prev desktop icon (typically a unicode character like the chevron, for example \\xE100 = skip back (player style), \\xE112 = previous (arrow style), \\xe26c = previous (chevron style), \\u02C2 = previous (chevron style), \\u2039 = previous (chevron style))");

			// Feature: showDesktopSwitchOverlay
			RegisterDefault("feature.showDesktopSwitchOverlay", true);
			RegisterDefault("feature.showDesktopSwitchOverlay.duration", 2000, "Defines the duration in milliseconds for a switch overlay to show. If set to zero, then the overlay is shown indefinately.");
			RegisterDefault("feature.showDesktopSwitchOverlay.animate", true);
			RegisterDefault("feature.showDesktopSwitchOverlay.translucent", true);
			RegisterDefault("feature.showDesktopSwitchOverlay.showOnAllMonitors", true);
			RegisterDefault("feature.showDesktopSwitchOverlay.position", "middlecenter");

			// Feature: useHotKeyToJumpToDesktopNumber
			RegisterDefault("feature.useHotKeyToJumpToDesktopNumber", false);
			RegisterDefault("feature.useHotKeyToJumpToDesktopNumber.hotkey", "Alt");

			// Feature: showDesktopNumberInIconTray
			RegisterDefault("feature.showDesktopNumberInIconTray", true);
			RegisterDefault("feature.showDesktopNumberInIconTray.clickToOpenTaskView", true);

			// Feature: showDesktopNameInIconTray
			RegisterDefault("feature.showDesktopNameInIconTray", false);

		}

		#endregion

		#region Settings API

		public static List<string> GetUsedConfigFiles() {
			return _settingsConfigFilesUsed;
		}

		public static string GetSettingsAsString() {
			return _compileSettingsDocumentation(_createMergedSettingsDictionary(true, false), false, false);
		}

		public static string GetDocumentationAsString() {
			return _compileSettingsDocumentation(_settingsDefaults, true, false);
		}

		public static string GetDocumentationAsMarkdown() {
			return _compileSettingsDocumentation(_settingsDefaults, true, true);
		}

		public static void RegisterDefault(string key, object value, string documentation = null) {
			_settingsDefaults[key] = value;
			if(documentation != null) _settingsDocumentations[key] = documentation;
		}

		public static void LoadConfig() {
			// Get the config file path
			var path = _getConfigPath();
			// Get the direcotry of the config file
			var dir = System.IO.Path.GetDirectoryName(path);
			// Load
			_loadConfigPath(dir);
		}

		public static void SaveConfig() {
			// Get the config file path
			var path = _getConfigPath();

			// Create a merged dictionary of all config and defaults settings
			var allSettings = _createMergedSettingsDictionary(false, true);

			// Serialize _settingsConfig to text, each setting on a line split by colon, sorting each line by trimming # comments out (ie sorting with comments inline)
			var lines = new List<string>();
			foreach(var kvp in allSettings) {
				var key = kvp.Key;
				var val = _serializeValAsType(kvp.Value);
				lines.Add($"{key}: {val}");
			}
            // Sort the lines by key but so that parent keys appear first (ie feature.showSplashScreen appears before feature.showSplashScreen.duration)
            lines.Sort((a, b) => {
                var aKey = a.Trim().StartsWith("#") ? a.Trim().Substring(1) : a.Trim();
                var bKey = b.Trim().StartsWith("#") ? b.Trim().Substring(1) : b.Trim();

                // Check if aKey and bKey have the same parent key
                var aParentKey = aKey.Substring(0, aKey.LastIndexOf('.'));
                var bParentKey = bKey.Substring(0, bKey.LastIndexOf('.'));
                if(aParentKey == bParentKey) {
                    return string.Compare(aKey, bKey, StringComparison.Ordinal);
                } else {
                    return string.Compare(aParentKey, bParentKey, StringComparison.Ordinal);
                }
            });
			// Write the lines to the config file
			System.IO.File.WriteAllLines(path, lines);
		}

		public static void RegisterLaunchArgs(string[] args) {
			// Parse the launch arguments, e.g. "--key value"
			for(int i = 0; i < args.Length; i++) {
				var arg = args[i];
				if(arg.StartsWith("--")) {
					var key = arg.Substring(2);
					if(i + 1 < args.Length) {
						var val = args[i + 1];
						// Support args with no val as assumed to be true, e.g. "--key" is the same as "--key true"
						if(val.StartsWith("--")) {
							_settingsLaunchArgs[key] = true;
						} else {
							_settingsLaunchArgs[key] = _parseValAsType(val);
						}
					} else if(i + 1 == args.Length) {
						// Support args with no val as assumed to be true, e.g. "--key" is the same as "--key true"
						// In this case there is no proceeding value, so we assume it is true
						_settingsLaunchArgs[key] = true;
					}
				}
			}
		}

		public static List<string> GetKeys() {
			return _createMergedSettingsDictionary(true, true).Keys.ToList();
		}

		public static string GetString(string key, string defaultValue = null) {
			var ret = _get(key, defaultValue);
			if(ret == null) return null;
			return ret as string;
		}

		public static void SetString(string key, string value) {
			_set(key, value);
		}

		public static bool GetBool(string key, bool? defaultValue = null) {
			string defaultValueStr = null;
			if(defaultValue != null) defaultValueStr = defaultValue.ToString().ToLower();
			var ret = _get(key, defaultValueStr);
			if(ret is bool) return (bool)ret;
			if(ret is string) return bool.Parse((string)ret);
			throw new Exception($"Setting {key} is not a bool (value is ${ret})");
		}

		public static void SetBool(string key, bool value) {
			_set(key, value);
		}

		public static int GetInt(string key, int? defaultValue = null) {
			string defaultValueStr = null;
			if(defaultValue != null) defaultValueStr = defaultValue.ToString();
			var ret = _get(key, defaultValueStr);
			if(ret is int) return (int)ret;
			if(ret is string) return int.Parse((string)ret);
			throw new Exception($"Setting {key} is not a int (value is ${ret})");
		}

		public static void SetInt(string key, int value) {
			_set(key, value);
		}

		public static double GetDouble(string key, double? defaultValue = null) {
			string defaultValueStr = null;
			if(defaultValue != null) defaultValueStr = defaultValue.ToString();
			var ret = _get(key, defaultValueStr);
			if(ret is double) return (double)ret;
			if(ret is float) return (double)ret;
			if(ret is int) return double.Parse(ret.ToString());
			if(ret is string) return double.Parse((string)ret);
			throw new Exception($"Setting {key} is not a double (value is ${ret})");
		}

		public static void SetDouble(string key, int value) {
			_set(key, value);
		}

		public static float GetFloat(string key, float? defaultValue = null) {
			string defaultValueStr = null;
			if(defaultValue != null) defaultValueStr = defaultValue.ToString();
			var ret = _get(key, defaultValueStr);
			if(ret is double) return (float)ret;
			if(ret is float) return (float)ret;
			if(ret is int) return float.Parse(ret.ToString());
			if(ret is string) return float.Parse((string)ret);
			throw new Exception($"Setting {key} is not a float (value is ${ret})");
		}

		#endregion

		#region Internal Methods and Properties

		// The launch arguments 
		static private Dictionary<string, string> _settingsDocumentations = new Dictionary<string, string>();
		static private Dictionary<string, object> _settingsLaunchArgs = new Dictionary<string, object>();
		static private Dictionary<string, object> _settingsConfig = new Dictionary<string, object>();
		static private Dictionary<string, object> _settingsDefaults = new Dictionary<string, object>();
		static private List<string> _settingsConfigFilesUsed = new List<string>();

		private static Dictionary<string, object> _createMergedSettingsDictionary(bool includeRuntimeSettings = false, bool includeDefaults = false) {
			var allSettings = new Dictionary<string, object>();
			if(includeRuntimeSettings) {
				foreach(var kvp in _settingsLaunchArgs) {
					if(!allSettings.ContainsKey(kvp.Key)) {
						allSettings[kvp.Key] = kvp.Value; // Add this default as a comment
					}
				}
			}
			foreach(var kvp in _settingsConfig) {
				if(!allSettings.ContainsKey(kvp.Key)) {
					allSettings[kvp.Key] = kvp.Value;
				}
			}
			if(includeDefaults) {
				foreach(var kvp in _settingsDefaults) {
					if(!allSettings.ContainsKey(kvp.Key)) {
						allSettings["#" + kvp.Key] = kvp.Value; // Add this default as a comment
					}
				}
			}
			return allSettings;
		}

		private static string _compileSettingsDocumentation(Dictionary<string, object> settingsToUse, bool includeDocumentation, bool asMarkdown) {
			// Compile the defaults and documentation into a string
			var lines = new List<string>();
			if(asMarkdown) lines.Add(includeDocumentation ? "|Config|Default|Description|" : "|Config|Default|");
			if(asMarkdown) lines.Add(includeDocumentation ? "| --- | --- | --- |" : "| --- | --- |");
			foreach(var kvp in settingsToUse) {
				var key = kvp.Key;
				var val = _serializeValAsType(kvp.Value);
				var line = asMarkdown ? $"| {key} | ``{val}`` |" : $"{key}: {val}";
				if(includeDocumentation) {
					var doc = _settingsDocumentations.ContainsKey(key) ? _settingsDocumentations[key] : null;
					if(doc != null || asMarkdown) line += asMarkdown ? $" {doc} |" : $", {doc}";
				}
				lines.Add(line);
			}
			return string.Join("\n",lines);
		}

		private static object _get(string key, string defaultValue = null) {
			// Check all our sources in the following priority:
			// 1. Launch arguments
			// 2. Config file
			// 3. Default value
			// 4. Defaults
			if(_settingsLaunchArgs.ContainsKey(key)) {
				return _settingsLaunchArgs[key];
			}
			if(_settingsConfig.ContainsKey(key)) {
				return _settingsConfig[key];
			}
			if(defaultValue != null) {
				return defaultValue;
			}
			if(_settingsDefaults.ContainsKey(key)) {
				return _settingsDefaults[key];
			}
			return null;
		}

		private static string _getMainConfigFile() {
			// Get the config file for this exe name, without the directory
			//var exeName = (System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
			var exeName = System.AppDomain.CurrentDomain.FriendlyName;
			return exeName + ".config";
		}

		private static void _set(string key, object value) {
			// Store in the config
			_settingsConfig[key] = value;
		}

		private static string _getConfigPath() {
			// Get the path to the config file
			var path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "WindowsVirtualDesktopHelper");
			if(!System.IO.Directory.Exists(path)) {
				System.IO.Directory.CreateDirectory(path);
			}
			path = System.IO.Path.Combine(path, _getMainConfigFile());
			return path;
		}

		private static string _unescapeString(string str) {
			// Strings are stored using the " character, and can contain the following:
			//  - \n for new line
			//  - \" for quote
			//  - \u2039 for unicode characters (for example)
			if(str == null) return null;
			str = str.Trim('"');
			//str = str.Replace("\\n", "\n");
			//str = str.Replace("\\\"", "\"");
			return System.Text.RegularExpressions.Regex.Unescape(str);
		}

		private static string _escapeString(string str) {
			if(str == null) return null;
			str = str.Replace("\\", "\\\\");
			str = str.Replace("\n", "\\n");
			str = str.Replace("\"", "\\\"");
			var escaped = new System.Text.StringBuilder();
			foreach(char c in str) {
				if(c > 127) escaped.AppendFormat("\\u{0:X4}", (int)c);
				else escaped.Append(c);
			}
			return "\"" + escaped + "\"";
		}

		private static object _parseValAsType(string value) {
			// Try to parse val as int, float, bool, or string, returning the appropriate type
			if(value.StartsWith("\"") && value.EndsWith("\"")) {
				return _unescapeString(value);
			}
			if(int.TryParse(value, out int intValue)) {
				return intValue;
			}
			if(bool.TryParse(value, out bool boolValue)) {
				return boolValue;
			}
			if(float.TryParse(value, out float floatValue)) {
				return floatValue;
			}
			return value; // Return as string if no other type matches
		}

		private static string _serializeValAsType(object val) {
			// Serialize val as string
			if(val is bool) return (bool)val ? "true" : "false";
			if(val is int) return ((int)val).ToString();
			if(val is float) return ((float)val).ToString();
			return _escapeString(val?.ToString()); // string default
		}

		private static void _loadConfigPath(string path) {
			if(System.IO.File.Exists(path)) {
				// Register the config file as used
				_settingsConfigFilesUsed.Add(path);
				// Load the config file
				var lines = System.IO.File.ReadAllLines(path);
				// Parse the lines, split by colon, adding each to the _settingsConfig
				foreach(var line in lines) {
					if(line.Trim().StartsWith("#")) continue; // Skip comments (lines starting with #)
					var parts = line.Split(':');
					if(parts.Length >= 2) {
						var key = parts[0].Trim();
						var val = parts[1].Trim();
						_settingsConfig[key] = _parseValAsType(val);
					}
				}
			} else if(System.IO.Directory.Exists(path)) {
				// Get all .config files in the directory
				var files = System.IO.Directory.GetFiles(path, "*.config");
				foreach(var file in files) {
					_loadConfigPath(file);
				}
			}
		}

		#endregion
	}
}
