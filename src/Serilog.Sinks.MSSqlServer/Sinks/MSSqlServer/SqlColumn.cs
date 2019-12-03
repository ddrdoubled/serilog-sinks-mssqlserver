using System;
using System.Data;

namespace Serilog.Sinks.MSSqlServer
{
    using Serilog.Sinks.MSSqlServer.Sinks.General.ColumnsOptions;

    /// <summary>
    /// Shared column customization options.
    /// </summary>
    public class SqlColumn : DatabaseColumn<SqlDbType, SqlDataTypes>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public SqlColumn()
            : base(SqlDataTypes.Instance)
        {
            DataType = SqlDbType.VarChar; // backwards-compatibility default
        }

        /// <summary>
        /// Constructor with property initialization.
        /// </summary>
        public SqlColumn(string columnName, SqlDbType dataType, bool allowNull = true, int dataLength = -1)
            : base(SqlDataTypes.Instance, columnName, dataType, allowNull, dataLength)
        {
            ColumnName = columnName;
            DataType = dataType;
            AllowNull = allowNull;
            DataLength = dataLength;
        }

        /// <summary>
        /// A constructor that initializes the object from a DataColumn object.
        /// </summary>
        public SqlColumn(DataColumn dataColumn) : base(SqlDataTypes.Instance, dataColumn)
        {
            ColumnName = dataColumn.ColumnName;
            AllowNull = dataColumn.AllowDBNull;

            if (!SqlDataTypes.ReverseTypeMap.ContainsKey(dataColumn.DataType))
                throw new ArgumentException($".NET type {dataColumn.DataType.ToString()} does not map to a supported SQL column data type.");

            DataType = SqlDataTypes.ReverseTypeMap[dataColumn.DataType];
            DataLength = dataColumn.MaxLength;

            if(DataLength == 0 && SqlDataTypes.DataLengthRequired.Contains(DataType))
                throw new ArgumentException($".NET type {dataColumn.DataType.ToString()} maps to a SQL column data type requiring a non-zero DataLength property.");
        }
    }
}
