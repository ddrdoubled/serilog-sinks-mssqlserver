using System;
using System.Collections.Generic;
using System.Text;

namespace Serilog.Sinks.MSSqlServer.Sinks.General
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TDataType"></typeparam>
    public interface IDataTypeMapper<TDataType>
    {
        /// <summary>
        /// 
        /// </summary>
        TDataType NotSupportedDataType { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        bool IsDataLengthRequired(TDataType dataType);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        bool IsColumnStoreCompatible(TDataType dataType);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestedType"></param>
        /// <param name="supportedSqlDbType"></param>
        /// <returns></returns>
        bool TryParseIfSupported(string requestedType, out TDataType supportedSqlDbType);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="mappedType"></param>
        /// <returns></returns>
        bool TryReverseMap(Type type, out TDataType mappedType);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        bool TryMap(TDataType dataType, out Type type);
    }
}
