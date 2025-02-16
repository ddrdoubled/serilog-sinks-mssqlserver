﻿namespace Serilog.Sinks.MSSqlServer.Sinks.General.ColumnsOptions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;

    /// <summary>
    /// Options that pertain to columns
    /// </summary>
    public abstract class DatabaseColumnOptions<TColumnType>
    {
        ICollection<StandardColumn> _store;

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected DatabaseColumnOptions()
        {
            // Apply any defaults in the individual Standard Column constructors.
            this.Store = new Collection<StandardColumn>
            {
                StandardColumn.Id,
                StandardColumn.Message,
                StandardColumn.MessageTemplate,
                StandardColumn.Level,
                StandardColumn.TimeStamp,
                StandardColumn.Exception,
                StandardColumn.Properties
            };
        }

        /// <summary>
        /// A list of columns that will be stored in the logs table in the database.
        /// </summary>
        public ICollection<StandardColumn> Store
        {
            get { return _store; }
            set
            {
                if (value == null)
                {
                    _store = new Collection<StandardColumn>();
                    foreach (StandardColumn column in Enum.GetValues(typeof(StandardColumn)))
                    {
                        _store.Add(column);
                    }
                }
                else
                {
                    _store = value;
                }
            }
        }

        /// <summary>
        /// The column which acts as the table's primary key. Primary keys must be non-null, the
        /// AllowNull property will always be forced to false. This can be a Standard Column or a
        /// custom column in the AdditionalColumns collection. This defaults to the Id column for
        /// backwards-compatibility reasons, but a primary key is optional. If the Id column is
        /// removed while the primary key is set to the Id column, no primary key is created.
        /// It is recommended to declare the primary key as a non-clustered index, although this
        /// is not done by default for backwards-compatibility reasons. If the primary key is
        /// not delcared as non-clustered and a Clustered Columnstore Index is not used, the
        /// primary key will be created as a clustered index.
        /// </summary>
        public TColumnType PrimaryKey { get; set; }

        /// <summary>
        /// 
        /// </summary>
        internal abstract ILogEventColumnOptions LogEventOptions { get;  }


        internal abstract IPropertiesColumnOptions PropertiesOptions { get; }

        /// <summary>
        /// When true for auto-created tables, the table will be created with a Clustered
        /// Columnstore Index. In this case, if the Id column is set as the primary key but
        /// is not configured as a non-clustered index, it will not default to a clustered
        /// index. Prior to SQL Server 2017 you must NOT use any NVARCHAR(MAX) columns, and
        /// this restriction includes the Standard Columns (you must change their size).
        /// </summary>
        public bool ClusteredColumnstoreIndex { get; set; } = false;

        /// <summary>
        /// Indicates if triggers should be disabled when inserting log entries.
        /// </summary>
        public bool DisableTriggers { get; set; }

        /// <summary>
        /// Additional log event property columns.
        /// </summary>
        public ICollection<TColumnType> AdditionalColumns { get; set; }

        /// <summary>
        /// Deprecated. Use the <see cref="AdditionalColumns"/> collection instead. The sink constructor
        /// will migrate this collection to the new collection automatically until this feature is removed.
        /// </summary>
        [Obsolete("Use the AdditionalColumns collection instead. This will be removed in a future release.", error: false)]
        public ICollection<DataColumn> AdditionalDataColumns { get; set; }

        /// <summary>
        /// Returns a reference to the Standard Column-specific subclass (ie. properties like ColumnOptions.Id) 
        /// </summary>
        internal abstract TColumnType GetStandardColumnOptions(StandardColumn standardColumn);
    }
}
