using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlUtils.ConfigSettings
{
	public class ConcatSetting
	{
		public string DatabaseName { get; set; }
		public string BuildDBScript { get; set; }
		public string MigrationFileName { get; set; }
		public string MigrationFolder { get; set; }
		public string ProgrammabilityFile { get; set; }
		public string ProgrammabilityFolder { get; set; }
		public string Version { get; set; }
		public string Prefix { get; set; }
		public Dictionary<int, string> Processings { get; set; }
	}
}
