using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using SqlUtils.ConfigSettings;

namespace SqlUtils.Commands
{
    internal class ConcatCommand : ISimpleCommand
    {
	    public const string DefaultVersionTag = "$Version";
	    private Dictionary<string, ConcatSetting> _settings;
	    internal ConcatCommand(string configName)
		{
			_settings = _loadConfig(configName);
		}

		private Dictionary<string, ConcatSetting> _loadConfig(string configName)
	    {
			var result = new Dictionary<string, ConcatSetting>();
			var path = File.Exists(configName) ? Path.GetDirectoryName(configName) : Environment.CurrentDirectory;
			var config = ConfigHelper.GetSection<ConcatConfigSection>(path, "concat", configName);
		    if (config == null)
		    {
				throw new ApplicationException(string.Format("Не удается прочитать файл конфигурации \"{0}\"", configName));
		    }

			if (config.Databases.Count < 1)
			{
				throw new ApplicationException(string.Format("Файл конфигурации \"{0}\" не заполнен", configName));
			}
			foreach (DatabaseElement qConfig in config.Databases)
			{
				var uploadDataTypeSetting = new ConcatSetting
				{
					DatabaseName = qConfig.DatabaseName,
					BuildDBScript = qConfig.BuildDBScript,
					Prefix = qConfig.Prefix,
					MigrationFileName = qConfig.MigrationFileName,
					MigrationFolder = qConfig.MigrationFolder,
					ProgrammabilityFolder = qConfig.ProgrammabilityFolder,
					ProgrammabilityFile = qConfig.ProgrammabilityFile,
					Version = qConfig.Version,
					Processings = new Dictionary<int, string>()
				};
				foreach (ProsessingItem processingItem in qConfig.Processings)
				{
					uploadDataTypeSetting.Processings.Add(processingItem.Id, processingItem.File);
				}
				result.Add(qConfig.DatabaseName, uploadDataTypeSetting);
			}
			return result;
	    }

		void ISimpleCommand.Execute()
        {
			foreach (var setting in _settings)
			{
				var _buildDBScriptName = setting.Value.BuildDBScript;
				using (var resultWriter = new StreamWriter(_buildDBScriptName))
				{
					resultWriter.AutoFlush = true;
					ProcessSection(resultWriter, setting.Value);
					resultWriter.Close();
				}
			}
		}

		private static bool ProcessSection(StreamWriter writer, ConcatSetting setting)
		{
			bool ret = true;
			string _resultFolder = Environment.CurrentDirectory;
			string _dbName = setting.DatabaseName;
			if (string.IsNullOrEmpty(_dbName))
			{
				throw new Exception("Configuration parameter DatabaseName not found");
			}
			string _prefix = setting.Prefix;
			if (string.IsNullOrEmpty(_prefix))
			{
				throw new Exception("Configuration parameter Prefix not found");
			}

			//<add key="ResultFolder" value="BuildDB Scripts"/>
			string[] _bildList = setting.Processings.Select(_=>_.Value).ToArray();
			if (_bildList.Length == 0)
			{
				throw new Exception("Configuration parameter process list not found");
			}

			var _programmabilityFolder = setting.ProgrammabilityFolder;
			var _programmabilityFile =  Path.Combine(Environment.CurrentDirectory, setting.ProgrammabilityFile);
			using(StreamWriter releaseWriter = new StreamWriter(_programmabilityFile))
			{
				releaseWriter.AutoFlush = true;
				//process header
				releaseWriter.WriteLine("/********************************************************************************");
				releaseWriter.WriteLine("Script name		: "+_prefix+".Programmability");
				releaseWriter.WriteLine("Description		: Create function and procedures for database");
				releaseWriter.WriteLine("********************************************************************************/");

				//programmability
				if(!string.IsNullOrEmpty(_programmabilityFolder))
				{
					ret &= ProcessDirectory(releaseWriter,_programmabilityFolder);
				}

				releaseWriter.Close();
				releaseWriter.Dispose();
			}

			//<add key="ConvertationFolder" value=""/>
			var _convertationFolder = setting.MigrationFolder;

			if(!string.IsNullOrEmpty(_convertationFolder))
			{
				var _convertationFileName = Path.Combine(Environment.CurrentDirectory, setting.MigrationFileName);
				using(StreamWriter convertationWriter = new StreamWriter(_convertationFileName))
				{
					convertationWriter.AutoFlush = true;
					//process header
					convertationWriter.WriteLine("/********************************************************************************");
					convertationWriter.WriteLine("Script name		: "+_prefix+".Migration");
					convertationWriter.WriteLine("Description		: Migration for database");
					convertationWriter.WriteLine("********************************************************************************/");
					convertationWriter.WriteLine("");
					convertationWriter.WriteLine("USE "+_dbName);
					convertationWriter.WriteLine("PRINT 'Executing convertation for DB'");
					convertationWriter.WriteLine("GO");
					convertationWriter.WriteLine("");
					//process folder
					ret &= ProcessDirectory(convertationWriter,_convertationFolder);
					convertationWriter.Close();
					convertationWriter.Dispose();
				}
			}
			// process buildpath (create db scripts)	
			writer.AutoFlush = true;
			//<add key="Version" value=""/>
			var _version = setting.Version;
			foreach(var fileName in _bildList)
			{
				string _path = Path.Combine(_resultFolder,fileName);
				if (File.Exists(_path))
				{
					using(StreamReader reader = new StreamReader(_path,true))
					{
						string _sqlSTMT = reader.ReadToEnd();
						_sqlSTMT = _sqlSTMT.Replace(DefaultVersionTag, _version);
						writer.WriteLine(_sqlSTMT);
						writer.WriteLine("GO");
						reader.Close();
					}
					Console.WriteLine(_path);
				}
			}
			return ret;
		}

		private static bool ProcessDirectory(StreamWriter streamWriter, string path)
		{
			var fileNames = Directory.GetFiles(path,"*.sql");
			var ret = true;
			var msg = "";
			foreach (var fname in fileNames)
			{
				var sr2 = new StreamReader(fname);
				var str = sr2.ReadToEnd();
				ret = IsCharsValid(str, ref msg);
				if (ret==false)
				{
					Console.Out.WriteLine(fname);
					Console.Out.WriteLine(msg);
					Console.Out.WriteLine("");
				}
				streamWriter.WriteLine(str);
				streamWriter.WriteLine("GO");
				sr2.Close();
			}
			var dirNames = Directory.GetDirectories(path);
			return dirNames.Aggregate(ret, (current, dname) => current & ProcessDirectory(streamWriter, dname));
		}

		private static bool IsCharsValid(string str, ref string msg)
		{
			const bool ret = true;
			Byte prev=0;
			// Create an ASCII encoding.
			Encoding ascii = Encoding.ASCII;
			// Encode the string.
			Byte[] encodedBytes = ascii.GetBytes(str);
			for (int i=0;i<encodedBytes.Length;i++)
			{
				var curr=encodedBytes[i];
				if (curr > 127)
				{
					msg = "Character code more than 127.";
					return false;
				}

				if (i > 0)
				{
					if (prev==13 && curr != 10)
					{
						msg = "There is no line feed after carriage return.";
						return false;
					}

					if (curr == 10 && prev != 13)
					{
						msg = "There is no carriage return before line feed.";
						return false;
					}
				}
				prev = curr;
			}
			return ret;
		}
	}
}

