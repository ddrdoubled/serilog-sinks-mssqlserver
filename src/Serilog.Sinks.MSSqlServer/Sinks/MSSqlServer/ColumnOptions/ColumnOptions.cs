using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;

namespace Serilog.Sinks.MSSqlServer
{
    using Serilog.Sinks.MSSqlServer.Sinks.General.ColumnsOptions;

    /// <summary>
    /// Options that pertain to columns
    /// </summary>
    public partial class ColumnOptions : DatabaseColumnOptions<SqlColumn>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ColumnOptions()
        {
            // Apply any defaults in the individual Standard Column constructors.
            Id = new IdColumnOptions();
            Level = new LevelColumnOptions();
            Properties = new PropertiesColumnOptions();
            Message = new MessageColumnOptions();
            MessageTemplate = new MessageTemplateColumnOptions();
            TimeStamp = new TimeStampColumnOptions();
            Exception = new ExceptionColumnOptions();
            LogEvent = new LogEventColumnOptions();

            Store = new Collection<StandardColumn>
            {
                StandardColumn.Id,
                StandardColumn.Message,
                StandardColumn.MessageTemplate,
                StandardColumn.Level,
                StandardColumn.TimeStamp,
                StandardColumn.Exception,
                StandardColumn.Properties
            };

            PrimaryKey = Id; // for backwards-compatibility, ignored if Id removed from Store
        }

        /// <summary>
        /// Options for the Id column.
        /// </summary>
        public IdColumnOptions Id { get; private set; }

        /// <summary>
        /// Options for the Level column.
        /// </summary>
        public LevelColumnOptions Level { get; private set; }

        /// <summary>
        /// Options for the Properties column.
        /// </summary>
        public PropertiesColumnOptions Properties { get; private set; }

        /// <summary>
        /// Options for the Exception column.
        /// </summary>
        public ExceptionColumnOptions Exception { get; set; }

        /// <summary>
        /// Options for the MessageTemplate column.
        /// </summary>
        public MessageTemplateColumnOptions MessageTemplate { get; set; }

        /// <summary>
        /// Options for the Message column.
        /// </summary>
        public MessageColumnOptions Message { get; set; }
        
        /// <summary>
        /// Options for the TimeStamp column.
        /// </summary>
        public TimeStampColumnOptions TimeStamp { get; private set; }

        /// <summary>
        /// Options for the LogEvent column.
        /// </summary>
        public LogEventColumnOptions LogEvent { get; private set; }

        internal override ILogEventColumnOptions LogEventOptions => this.LogEvent;

        internal override IPropertiesColumnOptions PropertiesOptions => this.Properties;

        /// <summary>
        /// Returns a reference to the Standard Column-specific subclass (ie. properties like ColumnOptions.Id) 
        /// </summary>
        internal override SqlColumn GetStandardColumnOptions(StandardColumn standardColumn)
        {
            switch(standardColumn)
            {
                case StandardColumn.Id: return Id;
                case StandardColumn.Level: return Level;
                case StandardColumn.TimeStamp: return TimeStamp;
                case StandardColumn.LogEvent: return LogEvent;
                case StandardColumn.Message: return Message;
                case StandardColumn.MessageTemplate: return MessageTemplate;
                case StandardColumn.Exception: return Exception;
                case StandardColumn.Properties: return Properties;
                default: return null;
            }
        }
    }
}
