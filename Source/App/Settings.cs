using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace WindowsVirtualDesktopHelper {

	/// <summary>
	/// Settings class to manage settings and configuration.
	/// Settings can be loaded from launch arguments, config file, or defaults, and are internally stored as object (which can be string, int, bool or float).
	/// Each key for a setting can be given a namespace, e.g. "icons.automaticallyHidePrevNextOnBounds" or "general.language".
	/// The config file is a text file in the following format:
	/// ```txt
	/// general.language: "en"
	/// icons.automaticallyHidePrevNextOnBounds: true
	/// ```
	/// 
	/// NOTE: CURRENTLY UNUSED, BUT KEPT FOR FUTURE USE - SETTINGS WILL BE MIGRATED TO THIS SOON
	/// </summary>
	class Settings {

		#region Defaults

		public static void LoadDefaults() {
			// Register any known default settings here

			// General
			RegisterDefault("general.startupWithWindows", false, "If true, the app will register itself with Windows to startup when Windows starts (via the registry).");
			
			// Theme
			RegisterDefault("theme.icons.font", "Segoe UI", "Defines the font name to use for the icons (for regular numbers, characters).");
			RegisterDefault("theme.icons.emojiFont", "Segoe UI Symbol", "Defines the font name to use for symbol and emoji icons.");
			RegisterDefault("theme.icons.iconBG.dark", "black");
			RegisterDefault("theme.icons.iconFG.dark", "white");
			RegisterDefault("theme.icons.iconBG.light", "white");
			RegisterDefault("theme.icons.iconFG.light", "black");
			
			// Feature: splash
			RegisterDefault("feature.showSplashScreen", true, "If enabled, a splash screen is shown on startup of the app. Overlays must be enabled.");
			RegisterDefault("feature.showSplashScreen.duration", 2000, "Splash duration in milliseconds.");
			RegisterDefault("feature.showSplashScreen.text", "Virtual Desktop Helper", "The splash text to show.");

			// Feature: showPrevNextIcons
			RegisterDefault("feature.showPrevNextIcons", false, "If enabled, a previous and next arrow will appear in the icons tray of Windows to allow easy switching between desktops.");
			RegisterDefault("feature.showPrevNextIcons.automaticallyHidePrevNextOnBounds", false, "If enabled, the prev/next icon will automatically hide if there is no prev/next desktop.");

		}

		#endregion

		#region Settings API

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
			var allSettings = new Dictionary<string, object>();
			foreach(var kvp in _settingsConfig) {
				allSettings[kvp.Key] = kvp.Value;
			}
			foreach(var kvp in _settingsDefaults) {
				if(!allSettings.ContainsKey(kvp.Key)) {
					allSettings["#"+kvp.Key] = kvp.Value; // Add this default as a comment
				}
			}

			// Serialize _settingsConfig to text, each setting on a line split by colon, sorting each line by trimming # comments out (ie sorting with comments inline)
			var lines = new List<string>();
			foreach(var kvp in allSettings) {
				var key = kvp.Key;
				var val = _serializeValAsType(kvp.Value);
				lines.Add($"{key}: {val}");
			}
			// Sort the lines
			lines.Sort((a, b) => {
				var aKey = a.Trim().StartsWith("#") ? a.Trim().Substring(1) : a.Trim();
				var bKey = b.Trim().StartsWith("#") ? b.Trim().Substring(1) : b.Trim();
				return string.Compare(aKey, bKey, StringComparison.Ordinal);
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
						_settingsLaunchArgs[key] = _parseValAsType(val);
					}
				}
			}
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
			if(ret is string) return double.Parse((string)ret);
			throw new Exception($"Setting {key} is not a double (value is ${ret})");
		}

		public static void SetDouble(string key, int value) {
			_set(key, value);
		}

		#endregion

		#region Internal Methods and Properties

		// The launch arguments 
		static private Dictionary<string, string> _settingsDocumentations = new Dictionary<string, string>();
		static private Dictionary<string, object> _settingsLaunchArgs = new Dictionary<string, object>();
		static private Dictionary<string, object> _settingsConfig = new Dictionary<string, object>();
		static private Dictionary<string, object> _settingsDefaults = new Dictionary<string, object>();

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

		private static object _parseValAsType(string value) {
			// Try to parse val as int, float, bool, or string, returning the appropriate type
			if(value.StartsWith("\"") && value.EndsWith("\"")) {
				// Remove leading and trailing quotes and replace \n with actual new line characters
				return value.Trim('"').Replace("\\n", "\n").Replace("\\\"", "\"");
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
			return "\"" + val.ToString().Replace("\"", "\\\"").Replace("\n", "\\\n") + "\""; // string default
		}

		private static void _loadConfigPath(string path) {
			if(System.IO.File.Exists(path)) {
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
