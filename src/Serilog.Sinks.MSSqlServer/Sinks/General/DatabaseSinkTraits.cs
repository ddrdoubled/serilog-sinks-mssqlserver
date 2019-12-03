// Copyright 2018 Serilog Contributors 
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Serilog.Sinks.MSSqlServer.Sinks.General
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;

    using Serilog.Debugging;
    using Serilog.Events;
    using Serilog.Sinks.MSSqlServer.Sinks.General.ColumnsOptions;

    /// <summary>Contains common functionality and properties used by both MSSqlServerSinks.</summary>
    public abstract class DatabaseSinkTraits<TColumnOptions, TColumnType, TDataType, TDataTypeMapper> : IDisposable
        where TColumnOptions : DatabaseColumnOptions<TColumnType>, new()
        where TColumnType : DatabaseColumn<TDataType, TDataTypeMapper>
        where TDataTypeMapper : IDataTypeMapper<TDataType>
    {
        /// <summary>
        /// 
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// 
        /// </summary>
        public string TableName { get; }

        /// <summary>
        /// 
        /// </summary>
        public string SchemaName { get; }
        /// <summary>
        /// 
        /// </summary>
        public TColumnOptions ColumnOptions { get; }

        /// <summary>
        /// 
        /// </summary>
        public IFormatProvider FormatProvider { get; }

        /// <summary>
        /// 
        /// </summary>
        public JsonLogEventFormatter JsonLogEventFormatter { get; }

        /// <summary>
        /// 
        /// </summary>
        public ISet<string> AdditionalColumnNames { get; }

        /// <summary>
        /// 
        /// </summary>
        public DataTable EventTable { get; }

        /// <summary>
        /// 
        /// </summary>
        public ISet<string> StandardColumnNames { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableName"></param>
        /// <param name="schemaName"></param>
        /// <param name="columnOptions"></param>
        /// <param name="formatProvider"></param>
        /// <param name="tableCreator"></param>
        /// <param name="autoCreateSqlTable"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected DatabaseSinkTraits(
            string connectionString,
            string tableName,
            string schemaName,
            TColumnOptions columnOptions,
            IFormatProvider formatProvider,
            Action<string, string, string, DataTable, TColumnOptions> tableCreator,
            bool autoCreateSqlTable)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException(nameof(tableName));

            this.ConnectionString = connectionString;
            this.TableName = tableName;
            this.SchemaName = schemaName;
            this.ColumnOptions = columnOptions ?? new TColumnOptions();
            this.FormatProvider = formatProvider;

            this.StandardColumnNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var stdCol in this.ColumnOptions.Store)
            {
                var col = this.ColumnOptions.GetStandardColumnOptions(stdCol);
                this.StandardColumnNames.Add(col.ColumnName);
            }

            this.AdditionalColumnNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (this.ColumnOptions.AdditionalColumns != null)
                foreach (var col in this.ColumnOptions.AdditionalColumns)
                    this.AdditionalColumnNames.Add(col.ColumnName);

            if (this.ColumnOptions.Store.Contains(StandardColumn.LogEvent))
                this.JsonLogEventFormatter = new JsonLogEventFormatter(
                    columnOptions.Store,
                    this.GetStandardColumnNameAndValue,
                    this.ColumnOptions.LogEventOptions);

            this.EventTable = this.CreateDataTable();

            if (autoCreateSqlTable)
            {
                try
                {
                    tableCreator(
                        this.ConnectionString,
                        this.SchemaName,
                        this.TableName,
                        this.EventTable,
                        this.ColumnOptions);
                    /*SqlTableCreator tableCreator = new SqlTableCreator(this.connectionString, this.schemaName, this.tableName, this.eventTable, this.columnOptions);
                    tableCreator.CreateTable(); // return code ignored, 0 = failure? */
                }
                catch (Exception ex)
                {
                    SelfLog.WriteLine($"Exception creating table {tableName}:\n{ex}");
                }

            }
        }

        /// <summary>Gets a list of the column names paired with their values to emit for the specified <paramref name="logEvent"/>.</summary>
        /// <param name="logEvent">The log event to emit.</param>
        /// <returns>
        /// A list of mappings between column names and values to emit to the database for the specified <paramref name="logEvent"/>.
        /// </returns>
        public IEnumerable<KeyValuePair<string, object>> GetColumnsAndValues(LogEvent logEvent)
        {
            foreach (var column in this.ColumnOptions.Store)
            {
                // skip Id (auto-incrementing identity)
                if(column != StandardColumn.Id)
                    yield return this.GetStandardColumnNameAndValue(column, logEvent);
            }

            if (this.ColumnOptions.AdditionalColumns != null)
            {
                foreach (var columnValuePair in this.ConvertPropertiesToColumn(logEvent.Properties))
                    yield return columnValuePair;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            this.EventTable.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="column"></param>
        /// <param name="logEvent"></param>
        /// <returns></returns>
        protected abstract KeyValuePair<string, object> GetStandardColumnNameAndValue(
            StandardColumn column,
            LogEvent logEvent);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logEvent"></param>
        /// <param name="excludeAdditionalProperties"></param>
        /// <returns></returns>
        protected string LogEventToJson(LogEvent logEvent, bool excludeAdditionalProperties)
        {
            if (excludeAdditionalProperties)
            {
                var filteredProperties = logEvent.Properties.Where(p => !this.AdditionalColumnNames.Contains(p.Key));
                logEvent = new LogEvent(logEvent.Timestamp, logEvent.Level, logEvent.Exception, logEvent.MessageTemplate, filteredProperties.Select(x => new LogEventProperty(x.Key, x.Value)));
            }

            var sb = new StringBuilder();
            using (var writer = new System.IO.StringWriter(sb))
                this.JsonLogEventFormatter.Format(logEvent, writer);
            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        protected string ConvertPropertiesToXmlStructure(
            IPropertiesColumnOptions options,
            IEnumerable<KeyValuePair<string, LogEventPropertyValue>> properties)
        {
            if (options.ExcludeAdditionalProperties)
                properties = properties.Where(p => !this.AdditionalColumnNames.Contains(p.Key));

            if (options.PropertiesFilter != null)
            {
                try
                {
                    properties = properties.Where(p => options.PropertiesFilter(p.Key));
                }
                catch (Exception ex)
                {
                    SelfLog.WriteLine("Unable to filter properties to store in {0} due to following error: {1}", this, ex);
                }
            }

            var sb = new StringBuilder();

            sb.AppendFormat("<{0}>", options.RootElementName);

            foreach (var property in properties)
            {
                var value = XmlPropertyFormatter.Simplify(property.Value, options);
                if (options.OmitElementIfEmpty && string.IsNullOrEmpty(value))
                {
                    continue;
                }

                if (options.UsePropertyKeyAsElementName)
                {
                    sb.AppendFormat("<{0}>{1}</{0}>", XmlPropertyFormatter.GetValidElementName(property.Key), value);
                }
                else
                {
                    sb.AppendFormat("<{0} key='{1}'>{2}</{0}>", options.PropertyElementName, property.Key, value);
                }
            }

            sb.AppendFormat("</{0}>", options.RootElementName);

            return sb.ToString();
        }

        /// <summary>
        ///     Mapping values from properties which have a corresponding data row.
        ///     Matching is done based on Column name and property key
        ///     Standard columns are not mapped
        /// </summary>        
        /// <param name="properties"></param>
        private IEnumerable<KeyValuePair<string, object>> ConvertPropertiesToColumn(IReadOnlyDictionary<string, LogEventPropertyValue> properties)
        {
            foreach (var property in properties)
            {
                if (!this.EventTable.Columns.Contains(property.Key) || this.StandardColumnNames.Contains(property.Key))
                    continue;

                var columnName = property.Key;
                var columnType = this.EventTable.Columns[columnName].DataType;

                if (!(property.Value is ScalarValue scalarValue))
                {
                    yield return new KeyValuePair<string, object>(columnName, property.Value.ToString());
                    continue;
                }

                if (scalarValue.Value == null && this.EventTable.Columns[columnName].AllowDBNull)
                {
                    yield return new KeyValuePair<string, object>(columnName, DBNull.Value);
                    continue;
                }

                if (TryChangeType(scalarValue.Value, columnType, out var conversion))
                {
                    yield return new KeyValuePair<string, object>(columnName, conversion);
                }
                else
                {
                    yield return new KeyValuePair<string, object>(columnName, property.Value.ToString());
                }
            }
        }

        /// <summary>
        ///     Try to convert the object to the given type
        /// </summary>
        /// <param name="obj">object</param>
        /// <param name="type">type to convert to</param>
        /// <param name="conversion">result of the converted value</param>        
        private static bool TryChangeType(object obj, Type type, out object conversion)
        {
            conversion = null;
            try
            {
                conversion = Convert.ChangeType(obj, type);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private DataTable CreateDataTable()
        {
            var eventsTable = new DataTable(this.TableName);

            foreach (var standardColumn in this.ColumnOptions.Store)
            {
                var standardOpts = this.ColumnOptions.GetStandardColumnOptions(standardColumn);
                var dataColumn = standardOpts.AsDataColumn();
                eventsTable.Columns.Add(dataColumn);
                if(standardOpts == this.ColumnOptions.PrimaryKey)
                    eventsTable.PrimaryKey = new DataColumn[] { dataColumn };
            }

            if (this.ColumnOptions.AdditionalColumns != null)
            {
                foreach(var addCol in this.ColumnOptions.AdditionalColumns)
                {
                    var dataColumn = addCol.AsDataColumn();
                    eventsTable.Columns.Add(dataColumn);
                    if (addCol == this.ColumnOptions.PrimaryKey)
                        eventsTable.PrimaryKey = new DataColumn[] { dataColumn };
                }
            }

            return eventsTable;
        }

    }
}
