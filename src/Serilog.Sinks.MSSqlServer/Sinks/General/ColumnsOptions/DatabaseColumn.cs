namespace Serilog.Sinks.MSSqlServer.Sinks.General.ColumnsOptions
{
    using System;
    using System.Data;

    using Serilog.Sinks.MSSqlServer.Sinks.General;

    /// <summary>
    /// Shared column customization options.
    /// </summary>
    public abstract class DatabaseColumn<TDataType, TDataTypeMapper> where TDataTypeMapper : IDataTypeMapper<TDataType>
    {
        private TDataType dataType;

        private TDataTypeMapper mapper;

        private string columnName = string.Empty;

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected DatabaseColumn(TDataTypeMapper dataTypeMapper)
        {
            mapper = dataTypeMapper;
        }

        /// <summary>
        /// Constructor with property initialization.
        /// </summary>
        protected DatabaseColumn(TDataTypeMapper dataTypeMapper, string columnName, TDataType dataType, bool allowNull = true, int dataLength = -1) : this(dataTypeMapper)
        {
            this.ColumnName = columnName;
            this.DataType = dataType;
            this.AllowNull = allowNull;
            this.DataLength = dataLength;
        }

        /// <summary>
        /// A constructor that initializes the object from a DataColumn object.
        /// </summary>
        protected DatabaseColumn(TDataTypeMapper dataTypeMapper, DataColumn dataColumn) : this(dataTypeMapper)
        {
            this.ColumnName = dataColumn.ColumnName;
            this.AllowNull = dataColumn.AllowDBNull;

            if (!mapper.TryReverseMap(dataColumn.DataType, out var dataType))
                throw new ArgumentException($".NET type {dataColumn.DataType.ToString()} does not map to a supported Database column data type.");

            this.DataType = dataType;
            this.DataLength = dataColumn.MaxLength;

            if(this.DataLength == 0 && mapper.IsDataLengthRequired(dataType))
                throw new ArgumentException($".NET type {dataColumn.DataType.ToString()} maps to a SQL column data type requiring a non-zero DataLength property.");
        }

        /// <summary>
        /// The name of the column in the database. Always required.
        /// </summary>
        public string ColumnName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(columnName) && this.StandardColumnIdentifier != null)
                    return this.StandardColumnIdentifier.ToString();
                return columnName;
            }
            set
            {
                columnName = value;
            }
        }

        /// <summary>
        /// The SQL data type to be stored in this column. Always required.
        /// </summary>
        // Some Standard Columns hide this (via "new") to impose a more restricted list.
        public TDataType DataType
        {
            get => dataType;
            set
            {
                if (!mapper.TryMap(value, out var type))
                    throw new ArgumentException($"Database column data type {value.ToString()} is not supported by this sink.");
                dataType = value;
            }
        }

        /// <summary>
        /// Indicates whether NULLs can be stored in this column. Default is true. Always required.
        /// </summary>
        // The Id Standard Column hides this (via "new") to force this to false.
        public bool AllowNull { get; set; } = true; 

        /// <summary>
        /// For character-storage DataTypes such as CHAR or VARCHAR, this defines the maximum size. The default -1 represents MAX.
        /// </summary>
        public int DataLength { get; set; } = -1;

        /// <summary>
        /// Determines whether a non-clustered index is created for this column. Compound indexes are not
        /// supported for auto-created log tables. This property is only used when auto-creating a log table.
        /// </summary>
        public bool NonClusteredIndex { get; set; } = false;

        // Set by the constructors of the Standard Column classes that inherit from this;
        // allows Standard Columns and user-defined columns to coexist but remain identifiable
        // and allows casting back to the Standard Column without a lot of switch gymnastics.
        internal StandardColumn? StandardColumnIdentifier { get; set; } = null;
        internal Type StandardColumnType { get; set; } = null;

        /// <summary>
        /// Converts a SQL sink SqlColumn object to a System.Data.DataColumn object. The original
        /// SqlColumn object is stored in the DataColumn's ExtendedProperties collection.
        /// Virtual so that the Id Standard Column can perform additional configuration.
        /// </summary>
        internal virtual DataColumn AsDataColumn() 
        {
            if (!mapper.TryMap(this.DataType, out var type))
            {
                throw new ArgumentException($"Data type {this.DataType.ToString()} does not map to a supported Database column data type.");
            }

            var dataColumn = new DataColumn
            {
                ColumnName = this.ColumnName,
                DataType = type,
                AllowDBNull = this.AllowNull
            };

            if (mapper.IsDataLengthRequired(this.DataType))
            {
                if(this.DataLength == 0)
                    throw new ArgumentException($"Column \"{this.ColumnName}\" is of type {this.DataType.ToString().ToLowerInvariant()} which requires a non-zero DataLength.");

                dataColumn.MaxLength = this.DataLength;
            }

            dataColumn.ExtendedProperties.Add("SqlColumn", this);
            return dataColumn;
        }

        /// <summary>
        /// Configuration accepts DataType as a simple string ("nvarchar" for example) for ease-of-use. 
        /// This converts to SqlDbType and stores it into the DataType property.
        /// </summary>
        internal void SetDataTypeFromConfigString(string requestedSqlType)
        {
            if (!mapper.TryParseIfSupported(requestedSqlType, out TDataType sqlType))
                throw new ArgumentException($"Database column data type {requestedSqlType} is not recognized or not supported by this sink.");

            this.DataType = sqlType;
        }
    }
}
