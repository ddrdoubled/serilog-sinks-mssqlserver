// Copyright 2015 Serilog Contributors
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
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;

    using Serilog.Events;
    using Serilog.Sinks.MSSqlServer.Sinks.General.ColumnsOptions;

    /// <summary>
    ///     Converts <see cref="LogEventProperty" /> values into simple scalars,
    ///     dictionaries and lists so that they can be persisted in MSSqlServer.
    /// </summary>
    public static class XmlPropertyFormatter
    {

        /// <summary>
        /// Regex to trasnform any non-xml char into ?, acoiding any exceptions on inserting the xml properties into the database
        /// </summary>
        private static Regex _invalidXMLChars = new Regex(
        @"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]",
        RegexOptions.Compiled);

        /// <summary>
        ///     Simplify the object so as to make handling the serialized
        ///     representation easier.
        /// </summary>
        /// <param name="value">The value to simplify (possibly null).</param>
        /// <param name="options">Options to use during formatting</param>
        /// <returns>A simplified representation.</returns>
        public static string Simplify(LogEventPropertyValue value, IPropertiesColumnOptions options)
        {
            if (value is ScalarValue scalar)
                return SimplifyScalar(scalar.Value);

            if (value is DictionaryValue dict)
            {
                var sb = new StringBuilder();

                bool isEmpty = true;

                foreach (var element in dict.Elements)
                {
                    var itemValue = Simplify(element.Value, options);
                    if (options.OmitElementIfEmpty && string.IsNullOrEmpty(itemValue))
                    {
                        continue;
                    }

                    if (isEmpty)
                    {
                        isEmpty = false;
                        if (!options.OmitDictionaryContainerElement)
                        {
                            sb.AppendFormat("<{0}>", options.DictionaryElementName);
                        }
                    }

                    var key = SimplifyScalar(element.Key.Value);
                    if (options.UsePropertyKeyAsElementName)
                    {
                        sb.AppendFormat("<{0}>{1}</{0}>", GetValidElementName(key), itemValue);
                    }
                    else
                    {
                        sb.AppendFormat("<{0} key='{1}'>{2}</{0}>", options.ItemElementName, key, itemValue);
                    }
                }

                if (!isEmpty && !options.OmitDictionaryContainerElement)
                {
                    sb.AppendFormat("</{0}>", options.DictionaryElementName);
                }

                return sb.ToString();
            }

            if (value is SequenceValue seq)
            {
                var sb = new StringBuilder();

                bool isEmpty = true;

                foreach (var element in seq.Elements)
                {
                    var itemValue = Simplify(element, options);
                    if (options.OmitElementIfEmpty && string.IsNullOrEmpty(itemValue))
                    {
                        continue;
                    }

                    if (isEmpty)
                    {
                        isEmpty = false;
                        if (!options.OmitSequenceContainerElement)
                        {
                            sb.AppendFormat("<{0}>", options.SequenceElementName);
                        }
                    }

                    sb.AppendFormat("<{0}>{1}</{0}>", options.ItemElementName, itemValue);
                }

                if (!isEmpty && !options.OmitSequenceContainerElement)
                {
                    sb.AppendFormat("</{0}>", options.SequenceElementName);
                }

                return sb.ToString();
            }

            if (value is StructureValue str)
            {
                var props = str.Properties.ToDictionary(p => p.Name, p => Simplify(p.Value, options));

                var sb = new StringBuilder();

                bool isEmpty = true;

                foreach (var element in props)
                {
                    var itemValue = element.Value;
                    if (options.OmitElementIfEmpty && string.IsNullOrEmpty(itemValue))
                    {
                        continue;
                    }

                    if (isEmpty)
                    {
                        isEmpty = false;
                        if (!options.OmitStructureContainerElement)
                        {
                            if (options.UsePropertyKeyAsElementName)
                            {
                                sb.AppendFormat("<{0}>", GetValidElementName(str.TypeTag));
                            }
                            else
                            {
                                sb.AppendFormat("<{0} type='{1}'>", options.StructureElementName, str.TypeTag);
                            }
                        }
                    }

                    if (options.UsePropertyKeyAsElementName)
                    {
                        sb.AppendFormat("<{0}>{1}</{0}>", GetValidElementName(element.Key), itemValue);
                    }
                    else
                    {
                        sb.AppendFormat("<{0} key='{1}'>{2}</{0}>", options.PropertyElementName,
                            element.Key, itemValue);
                    }
                }

                if (!isEmpty && !options.OmitStructureContainerElement)
                {
                    if (options.UsePropertyKeyAsElementName)
                    {
                        sb.AppendFormat("</{0}>", GetValidElementName(str.TypeTag));
                    }
                    else
                    {
                        sb.AppendFormat("</{0}>", options.StructureElementName);
                    }
                }

                return sb.ToString();
            }

            return null;
        }

        internal static string GetValidElementName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "x";
            }

            string validName = name.Trim();

            if (!char.IsLetter(validName[0]) || validName.StartsWith("xml", true, CultureInfo.CurrentCulture))
            {
                validName = "x" + name;
            }

            validName = Regex.Replace(validName, @"\s", "_");

            return validName;
        }

        static string SimplifyScalar(object value)
        {
            if (value == null) return null;

            return new XText(_invalidXMLChars.Replace(value.ToString(), m => "\\u" + ((ushort)m.Value[0]).ToString("x4"))).ToString();
        }
    }
}
