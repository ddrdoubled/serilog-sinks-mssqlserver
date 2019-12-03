using System;
using System.Collections.Generic;
using System.Text;

namespace Serilog.Sinks.MSSqlServer.Sinks.General.ColumnsOptions
{
    /// <summary>
    /// 
    /// </summary>
    public interface ILogEventColumnOptions
    {
        /// <summary>
        /// Exclude properties from the LogEvent column if they are being saved to additional columns.
        /// Defaults to false for backwards-compatibility, but true is the recommended setting.
        /// </summary>
        bool ExcludeAdditionalProperties { get; set; }

        /// <summary>
        /// Whether to include Standard Columns in the LogEvent column (for backwards compatibility).
        /// Defaults to false for backwards-compatibility, but true is the recommended setting.
        /// </summary>
        bool ExcludeStandardColumns { get; set; }
    }
}
