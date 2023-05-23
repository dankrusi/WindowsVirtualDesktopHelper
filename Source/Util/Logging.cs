using System;
using System.Collections.Generic;

namespace WindowsVirtualDesktopHelper.Util {
	public class Logging {

		private static List<string> _log = new List<string>();

		public static void WriteLine(string line) {
			Console.WriteLine(line);
			_log.Add(line);
			//TODO: trim front if longer than x lines?
		}

		public static List<string> GetLogHistory() {
			return _log;
		}

	}
}
