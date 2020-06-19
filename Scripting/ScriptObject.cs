using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace SqlUtils.Scripting
{
	public class ScriptObject
    {
        private readonly SqlConnection connection;
        private readonly string _folder;
		private readonly string _alias;
	    private const string indent = "\t";
	    private int numberIndent = 0;
		public ScriptObject(SqlConnection dbConnection, string folder, string alias)
        {
            connection = dbConnection;
            _folder = folder;
			_alias = alias;
        }
        #region Shema
        public bool CreateTableScript()
        {
            if (connection.State == ConnectionState.Closed)
                return false;
            
            
            //--Открываем файл для записи
			StreamWriter file = new StreamWriter(_folder + @"\" + _alias + ".Schema.sql");


            //--Получаем пользовательские таблицы
            string commandtext = @"select * from sys.tables systable 
                                   left join INFORMATION_SCHEMA.TABLES infomtable 
                                   on systable.name=infomtable.TABLE_NAME
                                   where systable.object_id NOT in(select major_id from sys.extended_properties 
									where minor_id = 0 and class = 1 and name = N'microsoft_database_tools_support')
                                   order by systable.name;";
            SqlCommand command = new SqlCommand(commandtext, connection);
            SqlDataReader reader = command.ExecuteReader();
            DataTable tableSchem = new DataTable();
            try
            {
                tableSchem.Load(reader);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {

                reader.Close();
            }
            file.WriteLine(GetIndent(numberIndent, indent) + "/********************************************************************************");
			file.WriteLine(GetIndent(numberIndent, indent) + "Script name	:" + _alias + ".Schema");
            file.WriteLine(GetIndent(numberIndent, indent) + "Description	: Create actual DB schema for database");
            file.WriteLine(GetIndent(numberIndent, indent) + "********************************************************************************/");
            file.WriteLine(GetIndent(numberIndent, indent) + "");
            file.WriteLine(GetIndent(numberIndent, indent) + "");
			file.WriteLine(GetIndent(numberIndent, indent) + "USE " + "$(" + _alias + ")");
            file.WriteLine(GetIndent(numberIndent, indent) + "GO");
            file.WriteLine(GetIndent(numberIndent, indent) + "PRINT 'Creating schema'");
            file.WriteLine(GetIndent(numberIndent, indent) + "GO");


            foreach (DataRow row in tableSchem.Rows)
            {
                string tableName = row["name"].ToString();
                string schemaName = row["TABLE_SCHEMA"].ToString();
                int table_id = Convert.ToInt32(row["object_id"]);
                file.WriteLine(GetIndent(numberIndent, indent) + @"PRINT '-> " + schemaName + "." + tableName + @"'");
                file.WriteLine(GetIndent(numberIndent, indent) + "GO");
				file.WriteLine(GetIndent(numberIndent, indent) + @"IF OBJECT_ID(N'[" + schemaName + "].[" + tableName + "]') IS NULL");
                file.WriteLine(GetIndent(numberIndent, indent) + "BEGIN");
                numberIndent++;
                file.WriteLine(GetIndent(numberIndent,indent)+"CREATE TABLE " + "[" + schemaName + "]" + "." + "[" + tableName + "]");
                file.WriteLine(GetIndent(numberIndent, indent) + "(");
                numberIndent++;

                string comandTextColumn = @"select tab1.name,tab1.defname,tab2.DATA_TYPE,
                                                   tab2.IS_NULLABLE, tab2.COLUMN_DEFAULT,
                                                   tab1.max_length,tab1.scale,tab1.precision,
                                                   tab2.COLLATION_NAME,tab1.is_computed,tab1.definition from (
                                             select coldef.name name, coldef.defname defname,
                                                    coldef.max_length,coldef.scale,coldef.precision,
                                                    coldef.is_computed,coldef.column_id,coldef.object_id,
                                                     syscom.definition from(
                       select syscolum.name name, defcon.name defname,
                              syscolum.max_length,syscolum.scale,syscolum.precision,
                              syscolum.is_computed,syscolum.column_id,syscolum.object_id from (
                         select * from sys.columns as syscol where syscol.object_id='"+table_id.ToString()+@"') as syscolum 
                            left join sys.default_constraints as defcon on syscolum.object_id=defcon.parent_object_id 
                                                                       and syscolum.column_id=defcon.parent_column_id) as coldef
                            left join sys.computed_columns syscom on coldef.object_id = syscom.object_id
                                                                 and coldef.column_id = syscom.column_id ) as tab1 
                            left join (
                               select DATA_TYPE,IS_NULLABLE,COLUMN_DEFAULT,
                                      COLUMN_NAME,COLLATION_NAME from INFORMATION_SCHEMA.COLUMNS infcol 
                                       where infcol.table_name='"+tableName+@"') as tab2 
                               on tab1.name=tab2.COLUMN_NAME";
                DataTable tableColumn = new DataTable();
                SqlCommand commandColum = new SqlCommand(comandTextColumn, connection);
                SqlDataReader readerColumn = commandColum.ExecuteReader();
                try
                {
                    tableColumn.Load(readerColumn);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(@"Error: " + ex.Message);
                }
                finally
                {
                    readerColumn.Close();
                }
                string stringColumnData = "";
                if (tableColumn.Rows.Count!=0)
                {
					stringColumnData = CreateStringColumn(tableName, tableColumn.Rows[0]);
                    file.Write(GetIndent(numberIndent,indent)+stringColumnData);
                }
                for (int i = 1; i < tableColumn.Rows.Count;i++ )
                {
                    DataRow rowCol = tableColumn.Rows[i];
                    file.WriteLine(",");
					stringColumnData = CreateStringColumn(tableName,rowCol);

                    file.Write(GetIndent(numberIndent,indent)+stringColumnData);
                }
                //--- первичные ключи и уникальные значения
                CreateIndex(file, table_id, tableName,false);

                numberIndent--;
                file.WriteLine(GetIndent(numberIndent,indent)+")");
                //--- индексы
                CreateIndex(file, table_id, tableName, true);
                numberIndent--;
                file.WriteLine(GetIndent(numberIndent,indent)+"END");

                file.WriteLine(GetIndent(numberIndent, indent) + "GO");
                file.WriteLine(GetIndent(numberIndent, indent) + "");
                file.WriteLine(GetIndent(numberIndent, indent) + "");
            }
            file.Close();

            return true;
        }

        
        private string GetTypeColumn(DataRow row)
        {
            string returnString = "";
            string typeCol = row["DATA_TYPE"].ToString();
            returnString = typeCol;
            int maxLength = Convert.ToInt32(row["max_length"]);
            switch (typeCol)
            {
                case "decimal":
                case "numeric": { returnString += "(" + row["precision"] + "," + row["scale"] + ")"; break; }
                case "datetime2":
                case "datetimeoffset":
                case "time": { returnString += "(" + row["scale"] + ")"; break; }
                case "varbinary":
                case "varchar":
                case "binary":
                case "char": { returnString += "(" + (maxLength == -1? "MAX": maxLength.ToString()) + ")"; break; }
                case "nchar":
                case "nvarchar": { returnString += "(" + (maxLength == -1? "MAX": ((int)(maxLength/2)).ToString()) + ")"; break; }
                    
            }
            return returnString;
        }
        private string CreateStringColumn(string tablename, DataRow rowCol)
        {
            string stringColumnData = "";
            stringColumnData = "[" + rowCol["name"] + "]";
            bool isCompare = (bool)rowCol["is_computed"];
            if (isCompare)
            {
                stringColumnData += " AS " + rowCol["definition"];
            }
            else
            {
                //----- Тип столбца (!надо добавить вычисляемый столбец и domain тип)
                string TypeColumn = GetTypeColumn(rowCol);
                stringColumnData += " " + TypeColumn;
                //---  имя для параметров сортировки
                string collationName = rowCol["COLLATION_NAME"].ToString();
                stringColumnData += (collationName == ""? "": " COLLATE " + collationName);
                //--- может принимать нулевое зеначение или нет
                string isNullable = rowCol["IS_NULLABLE"].ToString();
                if (isNullable == "YES")
                {
                    stringColumnData += " NULL";
                }
                else
                    stringColumnData += " NOT NULL";
                //---значение по умолчанию
                string isDefault = rowCol["COLUMN_DEFAULT"].ToString();
                if (isDefault != "")
                {
					stringColumnData += " CONSTRAINT " + string.Format("DF_{0}_{1}", tablename, rowCol["name"]) + " DEFAULT(" + rowCol["COLUMN_DEFAULT"] + ")";
                }
            }
            return stringColumnData;
        }

        private void CreateIndex(StreamWriter file, int table_id, string name, bool isIndex)
        {
            //---Индексы

            string commandTextIndex = "";
            if (!isIndex)
            {
                commandTextIndex = @"select * from sys.indexes where object_id='" + table_id.ToString() +
                                   @"' and(is_primary_key='True' or is_unique_constraint='True');";
            }
            else
            {
                commandTextIndex = @"select * from sys.indexes where object_id='" + table_id.ToString() +
                                   @"' and(is_primary_key='False' and is_unique_constraint='False') and name!='';";
            }
            DataTable tableIndex = new DataTable();
            SqlCommand commandIndex = new SqlCommand(commandTextIndex, connection);
            SqlDataReader readerIndex = commandIndex.ExecuteReader();
            try
            {
                tableIndex.Load(readerIndex);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                readerIndex.Close();
            }
            foreach (DataRow rowIndex in tableIndex.Rows)
            {
                
                string nameIndex = rowIndex["name"].ToString();
                string index_id = rowIndex["index_id"].ToString();
                string typeDesc = rowIndex["type_desc"].ToString();
                string isUniqe = rowIndex["is_unique"].ToString();
                string isPrimaryKey = rowIndex["is_primary_key"].ToString();
                string isUniqueConstraint = rowIndex["is_unique_constraint"].ToString();

                string commandTextIndex_column = @"select syscol.name name from 
                              (select object_id,column_id from sys.index_columns 
                               where object_id = '" + table_id.ToString() + @"' and index_id='" + index_id + @"') as indexcol 
                                left join 
                               sys.columns as syscol on indexcol.object_id=syscol.object_id and indexcol.column_id=syscol.column_id;";


                DataTable tableIndex_column = new DataTable();
                SqlCommand commandIndex_column = new SqlCommand(commandTextIndex_column, connection);
                SqlDataReader readerIndex_column = commandIndex_column.ExecuteReader();
                try
                {
                    tableIndex_column.Load(readerIndex_column);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
                finally
                {
                    readerIndex_column.Close();
                }
                if (isPrimaryKey == "True")
                {
                    file.Write(",");
                    file.WriteLine("");
                    file.WriteLine(GetIndent(numberIndent,indent)+"CONSTRAINT " + "[" + nameIndex + "]" + " PRIMARY KEY " + typeDesc);

                }
                else
                    if (isUniqueConstraint == "True")
                    {
                        file.Write(",");
                        file.WriteLine("");
                        file.WriteLine(GetIndent(numberIndent, indent) + "CONSTRAINT " + "[" + nameIndex + "]" + " UNIQUE " + typeDesc);
                    }
                    else
                    {
                        
                       // file.WriteLine(GetIndent(numberIndent,indent)+"CREATE" + (isUniqe == "UNIQUE"? "UNIQUE": "") + " " + (typeDesc == "CLUSTERED"? " " + typeDesc: " ") + "INDEX "+"[" + nameIndex + "]" + " on " +"["+ name+"]");
						file.WriteLine(GetIndent(numberIndent,indent)+"CREATE" + (isUniqe == "UNIQUE"? "UNIQUE": "") + " " + (typeDesc == "CLUSTERED"? " " + typeDesc: " ") + "INDEX "+"[" + nameIndex + "]" + " on " +"["+ name+"]");
                    }

                file.WriteLine(GetIndent(numberIndent, indent) + "(");
                numberIndent++;
                string stringColumn = "";
                if (tableIndex_column.Rows.Count == 0)
                    throw new Exception("Error index " + nameIndex + ": the index does not contain columns of the table");
                DataRow rowColum_index = tableIndex_column.Rows[0];
                stringColumn += "[" + rowColum_index["name"].ToString() + "]";
                for (int i = 1; i < tableIndex_column.Rows.Count; i++)
                {
                    rowColum_index = tableIndex_column.Rows[i];
                    stringColumn += ",[" + rowColum_index["name"].ToString() + "]";
                }
                file.WriteLine(GetIndent(numberIndent,indent)+stringColumn);
                numberIndent--;
                file.WriteLine(GetIndent(numberIndent, indent) + ")");

            }
          //  file.WriteLine("");
            
        }
        #endregion Shema
        #region DataBase
        public void CreateDataBase()
        {

        }
        #endregion DataBase

        #region Constraints
        public bool CreateConstreints()
        {
            if (connection.State == ConnectionState.Closed)
                return false;
            //--Открываем файл для записи
			StreamWriter file = new StreamWriter(_folder + @"\" + _alias + ".Constraints.sql");
            file.WriteLine(GetIndent(numberIndent, indent) + "/********************************************************************************");
			file.WriteLine(GetIndent(numberIndent, indent) + "Script name	:" + _alias + ".Constraint");
            file.WriteLine(GetIndent(numberIndent, indent) + "Description	: Create constraint for database");
            file.WriteLine(GetIndent(numberIndent, indent) + "*******************************************************************************/");
            string commandTextTableIndexes = @"select tabfkobj.parent_object_id object_id,tabfkobj.name name,sysschema.name schemaname from 
    (select table_fk.parent_object_id,sysobj.name,sysobj.schema_id from 
       (select parent_object_id from sys.foreign_keys as syscol group by parent_object_id) as table_fk
			left join
				sys.objects sysobj 
				on sysobj.object_id = table_fk.parent_object_id ) as tabfkobj
       left join
       sys.schemas sysschema on tabfkobj.schema_id = sysschema.schema_id order by name";
            SqlCommand commandTableIndexes = new SqlCommand(commandTextTableIndexes,connection);
            SqlDataReader readerTableIndexes = commandTableIndexes.ExecuteReader();
            DataTable tableIndexes = new DataTable();
            try
            {
                tableIndexes.Load(readerTableIndexes);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: "+ex.Message);
            }
            foreach(DataRow row in tableIndexes.Rows)
            {
                string SchemaName = row["schemaname"].ToString();
                string NameObject = row["name"].ToString();
                string obj_id = row["object_id"].ToString();
                file.WriteLine(GetIndent(numberIndent, indent) + "");
                file.WriteLine(GetIndent(numberIndent, indent) + "");
                file.WriteLine(GetIndent(numberIndent, indent) + "PRINT 'Creating constraints for " + SchemaName + "." + NameObject + "'");
                file.WriteLine(GetIndent(numberIndent, indent) + "GO");
                file.WriteLine(GetIndent(numberIndent, indent) + "");
                file.WriteLine(GetIndent(numberIndent, indent) + "");

                string commandTextForeignKey = @"select ParentConstr.parent_object_id,ParentConstr.constrainName,ParentConstr.id_constraint,ParentConstr.ParentObjName,ParentConstr.ParentSchemName,obj_shem.nameObj ReferenceObjName,obj_shem.nameSchema ReferenceSchemaName 
   from(
     select forkey.parent_object_id,forkey.name constrainName,forkey.object_id id_constraint,
        obj_shem.nameObj ParentObjName,obj_shem.nameSchema ParentSchemName,
        forkey.referenced_object_id referenced_object_id 
        from sys.foreign_keys forkey
          left join
            (select sysobj.object_id object_id,
                    sysobj.name nameObj,
                    syssch.name nameSchema 
                    from sys.objects sysobj 
                      left join sys.schemas syssch 
                             on sysobj.schema_id = syssch.schema_id) as obj_shem 
                               on forkey.parent_object_id = obj_shem.object_id) as ParentConstr
                      left join 
                        (select sysobj.object_id object_id,
                                sysobj.name nameObj,
                                syssch.name nameSchema 
                                from sys.objects sysobj 
                                  left join sys.schemas syssch 
                                  on sysobj.schema_id = syssch.schema_id) as obj_shem 
                                  
                                  on ParentConstr.referenced_object_id = obj_shem.object_id
                                  where ParentConstr.parent_object_id = '" + obj_id + @"'";
                SqlCommand commandForeignKey = new SqlCommand(commandTextForeignKey, connection);
                SqlDataReader readerForeignKey = commandForeignKey.ExecuteReader();
                DataTable TableForeignKey = new DataTable();
                try
                {
                    TableForeignKey.Load(readerForeignKey);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
                finally
                {
                    readerForeignKey.Close();
                }
                foreach (DataRow rowConstr in TableForeignKey.Rows)
                {
                    string parentNameObject = rowConstr["ParentObjName"].ToString();
                    string referenceNameObject = rowConstr["ReferenceObjName"].ToString();
                    string ParentSchemaName = rowConstr["ParentSchemName"].ToString();
                    string ReferenceSchemaName = rowConstr["ReferenceSchemaName"].ToString();

                    string constreintName = rowConstr["constrainName"].ToString();
                    string idConstraint = rowConstr["id_constraint"].ToString();

                    string commandTextForeignKeyColumn = @"select forkeycol_col.name parent_name,syscolRef.name reference_name from
(select syscolPar.name,forkeycol.referenced_object_id,forkeycol.referenced_column_id  from(select * from sys.foreign_key_columns where constraint_object_id='" + idConstraint + @"') as forkeycol
         left join sys.columns as syscolPar on forkeycol.parent_object_id=syscolPar.object_id and forkeycol.parent_column_id=syscolPar.column_id) as forkeycol_col
           left join sys.columns as syscolRef on forkeycol_col.referenced_object_id=syscolRef.object_id and forkeycol_col.referenced_column_id = syscolRef.column_id";
                    SqlCommand commandForeignKeyColumn = new SqlCommand(commandTextForeignKeyColumn, connection);
                    SqlDataReader readerForeignKeyColumn = commandForeignKeyColumn.ExecuteReader();
                    DataTable TableForeignKeyColumn = new DataTable();
                    try
                    {
                        TableForeignKeyColumn.Load(readerForeignKeyColumn);
                    }
                    finally
                    {
                        readerForeignKeyColumn.Close();
                    }

                    file.WriteLine(GetIndent(numberIndent, indent) + "");
                    file.WriteLine(GetIndent(numberIndent, indent) + "");
                    file.WriteLine(GetIndent(numberIndent, indent) + "ALTER TABLE " + "[" + ParentSchemaName + "]" + "." + "[" + parentNameObject + "]" + " ADD CONSTRAINT " + "[" + constreintName + "]" + " FOREIGN KEY");
                    numberIndent++;
                    string stringParentColumn = "";
                    string stringReferencedColum = "";
                    DataRow rowForeignKeyColumn;
                    if (TableForeignKeyColumn.Rows.Count != 0)
                    {
                        rowForeignKeyColumn = TableForeignKeyColumn.Rows[0];
                        stringParentColumn = "[" + rowForeignKeyColumn["parent_name"] + "]";
                        stringReferencedColum = "[" + rowForeignKeyColumn["reference_name"] + "]";
                    }
                    for (int i = 1; i < TableForeignKeyColumn.Rows.Count; i++)
                    {
                        rowForeignKeyColumn = TableForeignKeyColumn.Rows[i];
                        stringParentColumn += ",[" + rowForeignKeyColumn["parent_name"] + "]";
                        stringReferencedColum += ",[" + rowForeignKeyColumn["reference_name"] + "]";
                    }

                    file.WriteLine(GetIndent(numberIndent, indent) + "(");
                    numberIndent++;
                    file.WriteLine(GetIndent(numberIndent, indent) + stringParentColumn);
                    numberIndent--;
                    file.WriteLine(GetIndent(numberIndent, indent) + ")");
                    file.WriteLine(GetIndent(numberIndent, indent) + "REFERENCES " + "[" + ReferenceSchemaName + "]" + "." + "[" + referenceNameObject + "]");
                    file.WriteLine(GetIndent(numberIndent, indent) + "(");
                    numberIndent++;
                    file.WriteLine(GetIndent(numberIndent, indent) + stringReferencedColum);
                    numberIndent--;
                    file.WriteLine(GetIndent(numberIndent, indent) + ")");
                    numberIndent--;
                    file.WriteLine(GetIndent(numberIndent, indent) + "GO");

                }
            }//------


            
            
            
            file.Close();
            return true;
        }
        #endregion Contreints

        #region function and procedures
        private void CreateFolder(string road)
        {
            try{
            if(!Directory.Exists(road))
            {
                Directory.CreateDirectory(road);
            }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error: "+ex.Message+" Directory: "+road);
            }
        }
        public bool CreateFuncAndProc()
        {
            
            try
            {
                string _folderProc = _folder + @"\Procedures";
                CreateFolder(_folderProc);


                //----Обрабатываем процедуры
                string commandTextProc = @"select modobj.object_id,modobj.definition,modobj.name,modobj.type, schem.name nameschema from (select sysmodules.object_id,sysmodules.definition,sysobj.name,sysobj.type,sysobj.schema_id from sys.sql_modules sysmodules
              left join 
              sys.objects sysobj on sysmodules.object_id = sysobj.object_id  where type in (N'P', N'PC') and sysobj.object_id not in(select major_id from sys.extended_properties 
where minor_id = 0 and class = 1 and name = N'microsoft_database_tools_support')) as modobj 
                        left join sys.schemas as schem on schem.schema_id = modobj.schema_id ";
                SqlCommand commandProc = new SqlCommand(commandTextProc, connection);
                SqlDataReader readerProc = commandProc.ExecuteReader();
                DataTable tableProc = new DataTable();
                try
                {
                    tableProc.Load(readerProc);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(@"Error: " + ex.Message);
                }
                finally
                {
                    readerProc.Close();
                }
                foreach (DataRow row in tableProc.Rows)
                {
                    string definition = row["definition"].ToString();
                    string name = row["name"].ToString();
                    string schemaname = row["nameschema"].ToString();
                    /* Удаление процедуры  */
                   
                    /*                     */
					StreamWriter fileProc = new StreamWriter(_folderProc + @"\" + _alias + "." + name + ".sql");
                    try
                    {
                        /* Удаление процедуры  */
                        fileProc.WriteLine(@"IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[" + schemaname + "].[" + name + "]') AND type in (N'P', N'PC'))");
						fileProc.WriteLine("BEGIN");
						fileProc.WriteLine(indent + "PRINT 'Droping procedure [" + schemaname + "].[" + name + "]'");
						fileProc.WriteLine(indent + "DROP PROCEDURE [" + schemaname + "].[" + name + "]");
						fileProc.WriteLine("END");
						fileProc.WriteLine("GO");
						fileProc.WriteLine("");
						fileProc.WriteLine("PRINT 'Creating procedure [" + schemaname + "].[" + name + "]'");
						fileProc.WriteLine("GO");
						fileProc.WriteLine(definition);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(@"Error printf procedure: " + name + @". -" + ex.Message);
                    }
                    finally
                    {
                        fileProc.Close();
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in add procedure: "+ex.Message);
            }
            //----Обрабатываем функции
            try
            {
                string _folderFunc = _folder + @"\Functions";
                CreateFolder(_folderFunc);
                string commandTextFunc = @"select modobj.object_id,modobj.definition,modobj.name,modobj.type, schem.name nameschema from (select sysmodules.object_id,sysmodules.definition,sysobj.name,sysobj.type,sysobj.schema_id from sys.sql_modules sysmodules
													  left join 
													  sys.objects sysobj on sysmodules.object_id = sysobj.object_id  where type in (N'FN', N'IF', N'TF', N'FS', N'FT') and sysobj.object_id not in(select major_id from sys.extended_properties 
										where minor_id = 0 and class = 1 and name = N'microsoft_database_tools_support')) as modobj 
                        left join sys.schemas as schem on schem.schema_id = modobj.schema_id ";
                SqlCommand commandFunc = new SqlCommand(commandTextFunc, connection);
                SqlDataReader readerFunc = commandFunc.ExecuteReader();
                DataTable tableFunc = new DataTable();
                try
                {
                    tableFunc.Load(readerFunc);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
                finally
                {
                    readerFunc.Close();
                }
                foreach (DataRow row in tableFunc.Rows)
                {
                    string definition = row["definition"].ToString();
                    string name = row["name"].ToString();
                    string schemaname = row["nameschema"].ToString();
					var fileFunc = new StreamWriter(_folderFunc + @"\" + _alias + "." + name + ".sql");
                    try
                    {
                        /* Удаление функции  */
                        fileFunc.WriteLine(@"IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[" + schemaname + "].[" + name + "]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))");
						fileFunc.WriteLine("BEGIN");
						fileFunc.WriteLine(indent + "PRINT 'Droping function [" + schemaname + "].[" + name + "]'");
						fileFunc.WriteLine(indent +"DROP FUNCTION [" + schemaname + "].[" + name + "]");
						fileFunc.WriteLine("END");
                        fileFunc.WriteLine("GO");
                        fileFunc.WriteLine("");
						fileFunc.WriteLine("PRINT 'Creating function [" + schemaname + "].[" + name + "]'");
						fileFunc.WriteLine("GO");
						fileFunc.WriteLine(definition);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(@"Error printf function: {0}. -{1}", name, ex.Message);
                    }
                    finally
                    {
                        fileFunc.Close();
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"Error in add function: "+ex.Message);
            }
            return true;
        }
        #endregion function and procedures

        #region 

        private string GetIndent(int num, string str)
        {
            string returnString = "";
            for (int i = 0; i < num; i++)
            {
                returnString += str;
            }
            return returnString;
        }
        #endregion 

    }
}
