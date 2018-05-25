using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace AdoDbContext
{
    public abstract class DbContext : IDisposable
    {
        private SqlConnection connection;

        protected DbContext(string conenctionString)
        {
            connection = new SqlConnection(conenctionString);
            connection.Open();
            InitializeDb();
        }

        private void InitializeDb()
        {
            var properties = GetDbListProperties();
            var dataSet = GetDataSet(properties);

            foreach (var prop in properties)
            {
                prop.SetValue(this, Activator.CreateInstance(prop.PropertyType, dataSet, connection));
            }
        }

        private DataSet GetDataSet(List<PropertyInfo> dbListProperties)
        {
            var tableNames = GetTableNames(dbListProperties);
            var sql = "";
            foreach (var tableName in tableNames)
            {
                sql += $"SELECT * FROM {tableName}; ";
            }
            SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);
            DataSet set = new DataSet();
            adapter.FillSchema(set, SchemaType.Mapped);
            adapter.Fill(set);
            for (int i = 0; i < tableNames.Count; i++)
            {
                set.Tables[i].TableName = tableNames[i];
            }

            InitializeRelations(set, dbListProperties);

            return set;
        }

        private List<PropertyInfo> GetDbListProperties()
        {
            var dbListProperties = new List<PropertyInfo>();
            var properties = this.GetType().GetProperties();
            foreach (var prop in properties)
            {
                if (prop.PropertyType.IsGenericType &&
                   prop.PropertyType.Name == typeof(DbList<>).Name)
                {
                    dbListProperties.Add(prop);
                }
            }
            return dbListProperties;
        }

        private List<string> GetTableNames(List<PropertyInfo> dbListProperties)
        {
            var tableNames = new List<string>();
            foreach (var prop in dbListProperties)
            {
                if (prop.PropertyType.GenericTypeArguments.Count() > 0)
                {
                    var argument = prop.PropertyType.GenericTypeArguments[0];
                    var attribute = argument.GetCustomAttribute(typeof(TableAttribute)) as TableAttribute;
                    if (attribute != null)
                    {
                        tableNames.Add(attribute.Name);
                    }
                }
            }
            return tableNames;
        }

        private void InitializeRelations(DataSet set, List<PropertyInfo> dbListProperties)
        {
            foreach (var prop in dbListProperties)
            {
                if (prop.PropertyType.GenericTypeArguments.Count() > 0)
                {
                    var entityType = prop.PropertyType.GenericTypeArguments[0];
                    var properties = entityType.GetProperties();
                    foreach (var propertie in properties)
                    {
                        var fkAttr = propertie.GetCustomAttribute(typeof(ForeignKeyAttribute)) as ForeignKeyAttribute;
                        if (fkAttr != null)
                        {
                            var fkName = fkAttr.Name;
                            var propName = propertie.Name;
                            var tableAttribute = entityType.GetCustomAttribute(typeof(TableAttribute)) as TableAttribute;
                            if (tableAttribute != null)
                            {
                                var tableName = tableAttribute.Name;
                                var parrentEntityType = propertie.PropertyType;
                                var parrentAtrribute = parrentEntityType.GetCustomAttribute(typeof(TableAttribute)) as TableAttribute;
                                if (parrentAtrribute != null)
                                {
                                    var parrentTableName = parrentAtrribute.Name;
                                    var parrentProperties = parrentEntityType.GetProperties();
                                    foreach (var parrentProp in parrentProperties)
                                    {
                                        var keyAttr = parrentProp.GetCustomAttribute(typeof(KeyAttribute)) as KeyAttribute;
                                        if (keyAttr != null)
                                        {
                                            var parrentPropName = parrentProp.Name;
                                            set.Relations.Add(fkName,
                                                              set.Tables[parrentTableName].Columns[parrentPropName],
                                                              set.Tables[tableName].Columns[propName]);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            connection.Close();
        }
    }
}
