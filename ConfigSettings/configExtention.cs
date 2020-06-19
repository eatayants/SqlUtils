using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SqlUtils.ConfigSettings
{
	public class ConfigHelper
	{
		private static ConcurrentDictionary<string, Configuration> _configs =
			new ConcurrentDictionary<string, Configuration>();

		public static T Get<T>(string key)
		{
			try
			{
				return (T) Convert.ChangeType(ConfigurationManager.AppSettings[key], typeof (T));
			}
			catch (Exception)
			{
				// ignored
			}

			return default(T);
		}

		public static T Get<T>(string section, string key)
		{
			try
			{
				NameValueCollection _section = (NameValueCollection) ConfigurationManager.GetSection(section);
				return (T) Convert.ChangeType(_section[key], typeof (T));
			}
			catch (Exception)
			{
				// ignored
			}

			return default(T);
		}

		public static T GetSection<T>(string sectionName)
		{
			try
			{
				return (T) ConfigurationManager.GetSection(sectionName);
			}
			catch (Exception)
			{
				// ignored
			}
			return default(T);
		}

		public static T GetSection<T>(string sectionName, string configFileName) where T : ConfigurationSection
		{
			var directory = Environment.CurrentDirectory;
			return GetSection<T>(directory, sectionName, configFileName);
		}

		public static T GetSection<T>(string fileLocation, string sectionName, string configFileName)
			where T : ConfigurationSection
		{
			try
			{
				string key = string.Concat(sectionName, configFileName);
				if (_configs.ContainsKey(key)) return (T) (_configs[key].GetSection(sectionName));
				var configMap = new ExeConfigurationFileMap
				{
					ExeConfigFilename = Path.Combine(fileLocation, configFileName)
				};
				var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
				_configs.TryAdd(key, config);
				return (T) config.GetSection(sectionName);
			}
			catch (ConfigurationErrorsException exception)
			{
				throw new ApplicationException(
					string.Format("Ошибка при чтении файла конфигурации \"{0}\", секция: \"{1}\"", configFileName, sectionName),
					exception);
			}
		}
	}
}
