using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlUtils.ConfigSettings
{
	public class ProsessingItem : ConfigurationElement
	{
		[ConfigurationProperty("id", DefaultValue = "0", IsKey = true, IsRequired = true)]
		public int Id
		{
			get { return (int)base["id"]; }
			set { base["Id"] = value; }
		}
		[ConfigurationProperty("file", DefaultValue = "", IsRequired = true)]
		public string File
		{
			get { return (string)base["file"]; }
			set { base["file"] = value; }
		}
	}

	[ConfigurationCollection(typeof(ProsessingItem), AddItemName = "processing")]
	public class ProsessingItemCollection : ConfigurationElementCollection
	{
		protected override ConfigurationElement CreateNewElement()
		{
			return new ProsessingItem();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((ProsessingItem)element).Id;
		}

		public ProsessingItem this[int index]
		{
			get { return (ProsessingItem)BaseGet(index); }
		}
	}

	public class DatabaseElement : ConfigurationElement
	{
		[ConfigurationProperty("databaseName", DefaultValue = "", IsKey = true, IsRequired = true)]
		public string DatabaseName
		{
			get { return (string)base["databaseName"]; }
			set { base["databaseName"] = value; }
		}
		[ConfigurationProperty("prefix", DefaultValue = "", IsKey = true, IsRequired = true)]
		public string Prefix
		{
			get { return (string)base["prefix"]; }
			set { base["prefix"] = value; }
		}
		[ConfigurationProperty("buildDBScript", DefaultValue = "", IsKey = false, IsRequired = true)]
		public string BuildDBScript
		{
			get { return (string)base["buildDBScript"]; }
			set { base["buildDBScript"] = value; }
		}

		[ConfigurationProperty("migrationFile", DefaultValue = "", IsKey = false, IsRequired = true)]
		public string MigrationFileName
		{
			get { return (string)base["migrationFile"]; }
			set { base["migrationFile"] = value; }
		}

		[ConfigurationProperty("migrationFolder", DefaultValue = "Migration", IsKey = false, IsRequired = true)]
		public string MigrationFolder
		{
			get { return (string)base["migrationFolder"]; }
			set { base["migrationFolder"] = value; }
		}

		[ConfigurationProperty("programmabilityFile", DefaultValue = "", IsKey = false, IsRequired = true)]
		public string ProgrammabilityFile
		{
			get { return (string)base["programmabilityFile"]; }
			set { base["programmabilityFile"] = value; }
		}
		
		[ConfigurationProperty("programmabilityFolder", DefaultValue = "Programmability", IsKey = false, IsRequired = true)]
		public string ProgrammabilityFolder
		{
			get { return (string)base["programmabilityFolder"]; }
			set { base["programmabilityFolder"] = value; }
		}

		[ConfigurationProperty("version", DefaultValue = "version", IsKey = false, IsRequired = true)]
		public string Version
		{
			get { return (string)base["version"]; }
			set { base["version"] = value; }
		}

		[ConfigurationProperty("processings")]
		public ProsessingItemCollection Processings
		{
			get { return (ProsessingItemCollection)base["processings"]; }
			set { base["processings"] = value; }
		}
	}

	[ConfigurationCollection(typeof(DatabaseElement), AddItemName = "database")]
	public class DatabaseCollection : ConfigurationElementCollection
	{
		protected override ConfigurationElement CreateNewElement()
		{
			return new DatabaseElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((DatabaseElement)element).DatabaseName;
		}

		public DatabaseElement this[int index]
		{
			get { return (DatabaseElement)BaseGet(index); }
		}
	}

	public class ConcatConfigSection : ConfigurationSection
	{
		[ConfigurationProperty("databases")]
		public DatabaseCollection Databases
		{
			get { return (DatabaseCollection)base["databases"]; }
			set { base["databases"] = value; }
		}
	}
}
