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

using Serilog.Debugging;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;


namespace Serilog.Sinks.MSSqlServer
{
    using Serilog.Sinks.MSSqlServer.Sinks.General;

    /// <summary>Contains common functionality and properties used by both MSSqlServerSinks.</summary>
    internal sealed class MSSqlServerSinkTraits : DatabaseSinkTraits<ColumnOptions, SqlColumn, SqlDbType, SqlDataTypes>
    {
        public MSSqlServerSinkTraits(
            string connectionString,
            string tableName,
            string schemaName,
            ColumnOptions columnOptions,
            IFormatProvider formatProvider,
            bool autoCreateSqlTable)
            : base(connectionString, tableName, schemaName, columnOptions, formatProvider, SqlTableCreator.Creation, autoCreateSqlTable)
        {
        }

        protected override KeyValuePair<string, object> GetStandardColumnNameAndValue(StandardColumn column, LogEvent logEvent)
        {
            switch (column)
            {
                case StandardColumn.Message:
                    return new KeyValuePair<string, object>(ColumnOptions.Message.ColumnName, logEvent.RenderMessage(FormatProvider));
                case StandardColumn.MessageTemplate:
                    return new KeyValuePair<string, object>(ColumnOptions.MessageTemplate.ColumnName, logEvent.MessageTemplate.Text);
                case StandardColumn.Level:
                    return new KeyValuePair<string, object>(ColumnOptions.Level.ColumnName, ColumnOptions.Level.StoreAsEnum ? (object)logEvent.Level : logEvent.Level.ToString());
                case StandardColumn.TimeStamp:
                    return new KeyValuePair<string, object>(ColumnOptions.TimeStamp.ColumnName, ColumnOptions.TimeStamp.ConvertToUtc ? logEvent.Timestamp.ToUniversalTime().DateTime : logEvent.Timestamp.DateTime);
                case StandardColumn.Exception:
                    return new KeyValuePair<string, object>(ColumnOptions.Exception.ColumnName, logEvent.Exception != null ? logEvent.Exception.ToString() : null);
                case StandardColumn.Properties:
                    return new KeyValuePair<string, object>(ColumnOptions.Properties.ColumnName, ConvertPropertiesToXmlStructure(ColumnOptions.PropertiesOptions, logEvent.Properties));
                case StandardColumn.LogEvent:
                    return new KeyValuePair<string, object>(ColumnOptions.LogEvent.ColumnName, LogEventToJson(logEvent, ColumnOptions.LogEventOptions.ExcludeAdditionalProperties));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
