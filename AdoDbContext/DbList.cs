using System;
using System.Collections.Generic;
using System.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace AdoDbContext
{
    public class DbList<T> where T : new()
    {
        public List<T> Items { get; private set; }
        Type entityType;
        string tableName;
        DataSet _dataSet;
        DataTable table;
        SqlDataAdapter _adapter;
        SqlConnection _connection;

        public DbList(DataSet dataSet, SqlConnection connection)
        {
            _connection = connection;
            _dataSet = dataSet;
            Items = new List<T>();
            entityType = typeof(T);
            var atr = entityType.GetCustomAttributes(typeof(TableAttribute)).FirstOrDefault() as TableAttribute;
            tableName = atr.Name;
            foreach (DataTable tab in _dataSet.Tables)
            {
                if (tab.TableName == tableName)
                {
                    table = tab;
                }
            }
            _adapter = new SqlDataAdapter("SELECT * FROM " + tableName, connection);
            GetData();
        }

        public void GetData()
        {
            Items.Clear();
            foreach (DataRow row in table.Rows)
            {
                var newEntity = new T();
                for (int i = 0; i < row.ItemArray.Length; i++)
                {
                    var prop = entityType.GetProperty(table.Columns[i].ColumnName);
                    SetProperty(prop, newEntity, row, i);
                }
                Items.Add(newEntity);
            }
        }

        private void SetProperty(PropertyInfo property, object entity, DataRow row, int columnCount)
        {
            var forKeyAttr = property.GetCustomAttribute(typeof(ForeignKeyAttribute)) as ForeignKeyAttribute;
            if (forKeyAttr != null)
            {
                var parrentRow = row.GetParentRow(forKeyAttr.Name);
                var childElement = Activator.CreateInstance(property.PropertyType);
                var childProps = property.PropertyType.GetProperties();
                for (int i = 0; i < parrentRow.ItemArray.Length; i++)
                {
                    var childProp = property.PropertyType.GetProperty(parrentRow.Table.Columns[i].ColumnName);
                    SetProperty(childProp, childElement, parrentRow, i);
                }
                property.SetValue(entity, childElement);
            }
            else
            {
                property.SetValue(entity, row.ItemArray[columnCount]);
            }
        }

        public void Add(T item)
        {
            var row = table.NewRow();
            PropertyInfo keyProp = null;
            var properties = entityType.GetProperties();
            foreach (var prop in properties)
            {
                var propAttr = prop.GetCustomAttribute(typeof(KeyAttribute)) as KeyAttribute;
                if (propAttr == null)
                {
                    var fkAttr = prop.GetCustomAttribute(typeof(ForeignKeyAttribute)) as ForeignKeyAttribute;
                    if (fkAttr != null)
                    {
                        var parrentEntity = prop.GetValue(item);
                        var parrentProps = parrentEntity.GetType().GetProperties();
                        foreach (var parrentProp in parrentProps)
                        {
                            var parrentAttr = parrentProp.GetCustomAttribute(typeof(KeyAttribute)) as KeyAttribute;
                            if (parrentAttr != null)
                            {
                                row[prop.Name] = parrentProp.GetValue(parrentEntity);
                                break;
                            }
                        }
                    }
                    else
                    {
                        row[prop.Name] = prop.GetValue(item);
                    }
                }
                else
                {
                    keyProp = prop;
                }
            }
            table.Rows.Add(row);
            SqlCommandBuilder commandBuilder = new SqlCommandBuilder(_adapter);
            _adapter.Update(table);
            _dataSet.AcceptChanges();

            SqlCommand command = new SqlCommand("SELECT @@IDENTITY", _connection);
            var id = Convert.ToInt32(command.ExecuteScalar());
            keyProp.SetValue(item, id);
            Items.Add(item);
        }

        public void Remove(T item)
        {
            var properties = entityType.GetProperties();
            int id = -1;
            string propName = "";
            foreach (var prop in properties)
            {
                var propAttr = prop.GetCustomAttribute(typeof(KeyAttribute)) as KeyAttribute;
                if (propAttr != null)
                {
                    propName = prop.Name;
                    id = (int)prop.GetValue(item);
                }
            }

            for (int i = 0; i < table.Rows.Count; i++)
            {
                if ((int)table.Rows[i][propName] == id)
                {
                    table.Rows[i].Delete();
                }
            }
            SqlCommandBuilder commandBuilder = new SqlCommandBuilder(_adapter);
            _adapter.Update(table);
            _dataSet.AcceptChanges();
        }

        public void Update(T updatedItem)
        {
            var properties = entityType.GetProperties();
            int id = -1;
            string propName = "";
            foreach (var prop in properties)
            {
                var propAttr = prop.GetCustomAttribute(typeof(KeyAttribute)) as KeyAttribute;
                if (propAttr != null)
                {
                    propName = prop.Name;
                    id = (int)prop.GetValue(updatedItem);
                }
            }

            for (int i = 0; i < table.Rows.Count; i++)
            {
                if ((int)table.Rows[i][propName] == id)
                {
                    var row = table.Rows[i];
                    foreach (var prop in properties)
                    {
                        var propAttr = prop.GetCustomAttribute(typeof(KeyAttribute)) as KeyAttribute;
                        if (propAttr == null)
                        {
                            var propFKAttr = prop.GetCustomAttribute(typeof(ForeignKeyAttribute)) as ForeignKeyAttribute;
                            if (propFKAttr != null)
                            {
                                var parrentEntity = prop.GetValue(updatedItem);
                                var parrentProps = parrentEntity.GetType().GetProperties();
                                foreach (var parrentProp in parrentProps)
                                {
                                    var parrentAttr = parrentProp.GetCustomAttribute(typeof(KeyAttribute)) as KeyAttribute;
                                    if (parrentAttr != null)
                                    {
                                        row[prop.Name] = parrentProp.GetValue(parrentEntity);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                row[prop.Name] = prop.GetValue(updatedItem);
                            }
                        }
                    }
                    break;
                }
            }
            SqlCommandBuilder commandBuilder = new SqlCommandBuilder(_adapter);
            _adapter.Update(table);
            _dataSet.AcceptChanges();
        }
    }
}
