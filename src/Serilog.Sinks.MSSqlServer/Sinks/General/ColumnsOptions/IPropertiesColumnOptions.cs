using System;
using System.Collections.Generic;
using System.Text;

namespace Serilog.Sinks.MSSqlServer.Sinks.General.ColumnsOptions
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPropertiesColumnOptions
    {
        /// <summary>
        /// Exclude properties from the Properties column if they are being saved to additional columns.
        /// </summary>
        bool ExcludeAdditionalProperties { get; set; }

        /// <summary>
        /// The name to use for a dictionary element.
        /// </summary>
        string DictionaryElementName { get; set; }

        /// <summary>
        /// The name to use for an item element.
        /// </summary>
        string ItemElementName { get; set; }

        /// <summary>
        /// If true will omit the "dictionary" container element, and will only include child elements.
        /// </summary>
        bool OmitDictionaryContainerElement { get; set; }

        /// <summary>
        /// If true will omit the "sequence" container element, and will only include child elements.
        /// </summary>
        bool OmitSequenceContainerElement { get; set; }

        /// <summary>
        /// If true will omit the "structure" container element, and will only include child elements.
        /// </summary>
        bool OmitStructureContainerElement { get; set; }

        /// <summary>
        /// If true and the property value is empty, then don't include the element.
        /// </summary>
        bool OmitElementIfEmpty { get; set; }

        /// <summary>
        /// The name to use for a property element.
        /// </summary>
        string PropertyElementName { get; set; }

        /// <summary>
        /// The name to use for the root element.
        /// </summary>
        string RootElementName { get; set; }

        /// <summary>
        /// The name to use for a sequence element.
        /// </summary>
        string SequenceElementName { get; set; }

        /// <summary>
        /// The name to use for a structure element.
        /// </summary>
        string StructureElementName { get; set; }

        /// <summary>
        /// If true, will use the property key as the element name.
        /// </summary>
        bool UsePropertyKeyAsElementName { get; set; }

        /// <summary>
        /// If set, will only store properties allowed by the filter.
        /// </summary>
        Predicate<string> PropertiesFilter { get; set; }
    }
}
