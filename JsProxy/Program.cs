using MohawkCollege.Util.Console.Parameters;
using Newtonsoft.Json;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Attributes;
using SanteDB.Core.Model.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JsProxy
{
    class Program
    {
        private static Dictionary<Type, JsonObjectAttribute> primitives = new Dictionary<Type, JsonObjectAttribute>()
        {
            { typeof(DateTimeOffset), new JsonObjectAttribute("Date") },
            { typeof(DateTimeOffset?), new JsonObjectAttribute("Date") },
            { typeof(DateTime), new JsonObjectAttribute("Date") },
            { typeof(DateTime?), new JsonObjectAttribute("Date") },
            { typeof(String), new JsonObjectAttribute("string") },
            { typeof(Int32), new JsonObjectAttribute("number") },
            { typeof(Int32?), new JsonObjectAttribute("number") },
            { typeof(Decimal), new JsonObjectAttribute("number") },
            { typeof(Decimal?), new JsonObjectAttribute("number") },
            { typeof(byte), new JsonObjectAttribute("byte") },
            { typeof(byte[]), new JsonObjectAttribute("Array<byte>") },
            { typeof(Guid), new JsonObjectAttribute("string") },
            { typeof(Guid?), new JsonObjectAttribute("string") },
            { typeof(bool), new JsonObjectAttribute("boolean") },
            { typeof(bool?), new JsonObjectAttribute("boolean") },

        };


        static void Main(string[] args)
        {
            var parms = new ParameterParser<ConsoleParameters>().Parse(args);

            // First we want to open the output file
            using (TextWriter output = File.CreateText(parms.Output ?? "out.js"))
            {


                // Output namespace
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(parms.DocumentationFile ?? Path.ChangeExtension(parms.AssemblyFile, "xml"));

                List<Type> enumerationTypes = new List<Type>();

                List<Type> alreadyGenerated = new List<Type>();
                foreach (var type in Assembly.LoadFile(parms.AssemblyFile).GetTypes().Where(o => o.GetCustomAttribute<JsonObjectAttribute>() != null))
                    GenerateTypeDocumentation(output, type, xmlDoc, parms, enumerationTypes, alreadyGenerated);
                // Generate type documentation for each of the binding enumerations
                foreach (var typ in enumerationTypes.Distinct())
                    GenerateEnumerationDocumentation(output, typ, xmlDoc, parms);

                GenerateEnumerationDocumentation(output, typeof(NullReasonKeys), xmlDoc, parms);

                output.Write(
                    @"
// Empty guid
if(!EmptyGuid)
    EmptyGuid = ""00000000-0000-0000-0000-000000000000"";

if(!Exception)
    /**
    * @class
    * @summary Represents a simple exception class
    * @constructor
    * @memberof SanteDBModel
    * @property {string} message Informational message about the exception
    * @property {any} details Any detail / diagnostic information
    * @property {Exception} cause The cause of the exception
    * @param {string} type The type of exception
    * @param {string} message Informational message about the exception
    * @param {any} detail Any detail / diagnostic information
    * @param {Exception} cause The cause of the exception
    */
    function Exception (type, message, detail, cause) {
        _self = this;
        /** @type {string} */
        this.type = type;
        /** @type {string} */
        this.message = message;
        /** @type {string} */
        this.details = detail;
        /** @type {Exception} */
        this.caused_by = cause;
    }
"
                );
            }
        }


        /// <summary>
        /// Generate enumeration documentation
        /// </summary>
        private static void GenerateEnumerationDocumentation(TextWriter writer, Type type, XmlDocument xmlDoc, ConsoleParameters parms)
        {
            var jobject = type.GetCustomAttribute<JsonObjectAttribute>();
            if (jobject == null)
                jobject = new JsonObjectAttribute(type.Name);

            writer.WriteLine("// {0}", type.AssemblyQualifiedName);
            writer.WriteLine("if(!{0})", jobject.Id);

            writer.WriteLine("/**");
            writer.WriteLine(" * @enum {string}");
            writer.WriteLine(" * @memberof SanteDBModel");
            writer.WriteLine(" * @public");
            writer.WriteLine(" * @readonly");

            // Lookup the summary information
            var typeDoc = xmlDoc.SelectSingleNode(String.Format("//*[local-name() = 'member'][@name = 'T:{0}']", type.FullName));
            if (typeDoc != null)
            {
                if (typeDoc.SelectSingleNode(".//*[local-name() = 'summary']") != null)
                    writer.WriteLine(" * @summary {0}", typeDoc.SelectSingleNode(".//*[local-name() = 'summary']").InnerText.Replace("\r\n", ""));
                if (typeDoc.SelectSingleNode(".//*[local-name() = 'remarks']") != null)
                    writer.WriteLine(" * @description {0}", typeDoc.SelectSingleNode(".//*[local-name() = 'remarks']").InnerText.Replace("\r\n", ""));
                if (typeDoc.SelectSingleNode(".//*[local-name() = 'example']") != null)
                    writer.WriteLine(" * @example {0}", typeDoc.SelectSingleNode(".//*[local-name() = 'example']").InnerText.Replace("\r\n", ""));
            }
            writer.WriteLine(" */");
            writer.WriteLine("const {0} = {{ ", jobject.Id);

            // Enumerate fields
            foreach (var fi in type.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                writer.WriteLine("\t/** ");
                writer.Write("\t * ");
                typeDoc = xmlDoc.SelectSingleNode(String.Format("//*[local-name() = 'member'][@name = 'F:{0}.{1}']", fi.DeclaringType.FullName, fi.Name));
                if (typeDoc != null)
                {
                    if (typeDoc.SelectSingleNode(".//*[local-name() = 'summary']") != null)
                        writer.Write(typeDoc.SelectSingleNode(".//*[local-name() = 'summary']").InnerText.Replace("\r\n", ""));
                }
                writer.WriteLine();
                writer.WriteLine("\t */");


                writer.WriteLine("\t{0} : '{1}',", fi.Name, fi.GetValue(null));
            }

            writer.WriteLine("}}  // {0} ", jobject.Id);

        }

        /// <summary>
        /// Generate a javascript "class"
        /// </summary>
        private static void GenerateTypeDocumentation(TextWriter writer, Type type, XmlDocument xmlDoc, ConsoleParameters parms, List<Type> enumerationTypes, List<Type> alreadyGenerated)
        {
            if (parms.NoAbstract && type.IsAbstract) return;

            if (alreadyGenerated.Contains(type))
                return;
            else
                alreadyGenerated.Add(type);
            writer.WriteLine("// {0}", type.AssemblyQualifiedName);
            writer.WriteLine("if(!{0})", type.GetCustomAttribute<JsonObjectAttribute>().Id);
            writer.WriteLine("/**");
            writer.WriteLine(" * @class");
            writer.WriteLine(" * @constructor");
            writer.WriteLine(" * @memberof SanteDBModel");
            writer.WriteLine(" * @public");
            if (type.IsAbstract)
                writer.WriteLine(" * @abstract");
            var jobject = type.GetCustomAttribute<JsonObjectAttribute>();
            if (type.BaseType != typeof(Object) &&
                (!type.BaseType.IsAbstract ^ !parms.NoAbstract))
                writer.WriteLine(" * @extends {0}", type.BaseType.GetCustomAttribute<JsonObjectAttribute>().Id);

            // Lookup the summary information
            var typeDoc = xmlDoc.SelectSingleNode(String.Format("//*[local-name() = 'member'][@name = 'T:{0}']", type.FullName));
            if (typeDoc != null)
            {
                if (typeDoc.SelectSingleNode(".//*[local-name() = 'summary']") != null)
                    writer.WriteLine(" * @summary {0}", typeDoc.SelectSingleNode(".//*[local-name() = 'summary']").InnerText.Replace("\r\n", ""));
                if (typeDoc.SelectSingleNode(".//*[local-name() = 'remarks']") != null)
                    writer.WriteLine(" * @description {0}", typeDoc.SelectSingleNode(".//*[local-name() = 'remarks']").InnerText.Replace("\r\n", "\r\n * ").Replace("()", ""));
                if (typeDoc.SelectSingleNode(".//*[local-name() = 'example']") != null)
                    writer.WriteLine(" * @example {0}", typeDoc.SelectSingleNode(".//*[local-name() = 'example']").InnerText.Replace("\r\n", ""));
            }

            List<KeyValuePair<String, String>> copyCommands = new List<KeyValuePair<string, string>>();
            // Get all properties and document them
            foreach (var itm in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (itm.GetCustomAttribute<JsonPropertyAttribute>() == null && itm.GetCustomAttribute<SerializationReferenceAttribute>() == null)
                    continue;

                Type itmType = itm.PropertyType;
                if (itmType.IsGenericType) itmType = itmType.GetGenericArguments()[0];

                var itmJobject = itmType.GetCustomAttribute<JsonObjectAttribute>();
                if (itmJobject == null)
                {
                    if (itmType.StripNullable().IsEnum)
                        itmJobject = new JsonObjectAttribute(String.Format("{0}", itmType.Name));
                    else if (!primitives.TryGetValue(itmType, out itmJobject))
                        itmJobject = new JsonObjectAttribute(itmType.Name);
                }
                else
                    itmJobject = new JsonObjectAttribute(String.Format("{0}", itmJobject.Id));

                var simpleAtt = itmType.GetCustomAttribute<SimpleValueAttribute>();
                if (simpleAtt != null)
                {
                    var simpleProperty = itmType.GetProperty(simpleAtt.ValueProperty);
                    if (!primitives.TryGetValue(simpleProperty.PropertyType, out itmJobject))
                        itmJobject = new JsonObjectAttribute(simpleProperty.PropertyType.Name);
                }

                var originalType = itmJobject.Id;

                // Is this a classified object? if so then the classifier values act as properties themselves
                var classAttr = itmType.GetCustomAttribute<ClassifierAttribute>();
                if (classAttr != null && itm.PropertyType.IsGenericType)
                {
                    itmJobject = new JsonObjectAttribute("object");
                }
                else if (itm.Name.Contains("TimeXml") || itm.Name.Contains("DateXml")) // XML Representations of offsets
                    itmJobject = new JsonObjectAttribute("Date");


                writer.Write(" * @property {{{0}}} ", itmJobject.Id);
                var jprop = itm.GetCustomAttribute<JsonPropertyAttribute>();
                var redir = itm.GetCustomAttribute<SerializationReferenceAttribute>();
                if (jprop != null)
                {
                    writer.Write(jprop.PropertyName);
                    copyCommands.Add(new KeyValuePair<String, String>(jprop.PropertyName, itmJobject.Id));
                }
                else if (redir != null)
                {
                    var backingProperty = type.GetProperty(redir.RedirectProperty);
                    jprop = backingProperty.GetCustomAttribute<JsonPropertyAttribute>();
                    writer.Write("{0}Model [Delay loaded from {0}], ", jprop.PropertyName);
                    copyCommands.Add(new KeyValuePair<String, String>(jprop.PropertyName + "Model", itmJobject.Id));

                }
                else
                {
                    writer.Write(itm.Name + "Model");
                    copyCommands.Add(new KeyValuePair<string, string>(itm.Name + "Model", itmJobject.Id));

                }

                // Output documentation
                typeDoc = xmlDoc.SelectSingleNode(String.Format("//*[local-name() = 'member'][@name = 'P:{0}.{1}']", itm.DeclaringType.FullName, itm.Name));
                if (typeDoc != null)
                {
                    if (typeDoc.SelectSingleNode(".//*[local-name() = 'summary']") != null)
                        writer.Write(typeDoc.SelectSingleNode(".//*[local-name() = 'summary']").InnerText.Replace("\r\n", ""));
                }

                var bindAttr = itm.GetCustomAttribute<BindingAttribute>();
                if (itmType.StripNullable().IsEnum)
                    bindAttr = new BindingAttribute(itmType.StripNullable());

                if (bindAttr != null)
                {
                    enumerationTypes.Add(bindAttr.Binding);
                    writer.Write("(see: {{@link {0}}} for values)", bindAttr.Binding.Name);
                }
                writer.WriteLine();

                // Classified object? If so we need to clarify how the object is propogated
                if (classAttr != null && itm.PropertyType.IsGenericType)
                {
                    // Does the classifier have a binding
                    var classProperty = itmType.GetProperty(classAttr.ClassifierProperty);
                    if (classProperty.GetCustomAttribute<SerializationReferenceAttribute>() != null)
                        classProperty = itmType.GetProperty(classProperty.GetCustomAttribute<SerializationReferenceAttribute>().RedirectProperty);
                    bindAttr = classProperty.GetCustomAttribute<BindingAttribute>();
                    if (bindAttr != null)
                    {
                        enumerationTypes.Add(bindAttr.Binding);

                        // Binding attribute found so lets enumerate it
                        foreach (var fi in bindAttr.Binding.GetFields(BindingFlags.Public | BindingFlags.Static))
                        {
                            writer.Write(" * @property {{{0}}} {1}.{2} ", originalType, jprop.PropertyName, fi.Name, classProperty.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName);
                            typeDoc = xmlDoc.SelectSingleNode(String.Format("//*[local-name() = 'member'][@name = 'F:{0}.{1}']", fi.DeclaringType.FullName, fi.Name));
                            if (typeDoc != null)
                            {
                                if (typeDoc.SelectSingleNode(".//*[local-name() = 'summary']") != null)
                                    writer.Write(typeDoc.SelectSingleNode(".//*[local-name() = 'summary']").InnerText.Replace("\r\n", ""));
                            }
                            writer.WriteLine();
                        }
                        writer.WriteLine(" * @property {{{0}}} {1}.$other Unclassified", originalType, jprop.PropertyName);

                    }
                    else
                    {
                        writer.Write(" * @property {{{0}}} {1}.{2} ", originalType, jprop.PropertyName, "classifier");
                        writer.Write(" where classifier is from {{@link {0}}} {1}", classProperty.DeclaringType.GetCustomAttribute<JsonObjectAttribute>().Id, classProperty.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName);
                        writer.WriteLine();
                    }
                }
            }
            writer.WriteLine(" * @param {{{0}}} copyData Copy constructor (if present)", jobject.Id);

            writer.WriteLine(" */");
            writer.WriteLine("function {0} (copyData) {{ ", jobject.Id);

            writer.WriteLine("\tthis.$type = '{0}';", jobject.Id);
            writer.WriteLine("\tif(copyData) {");
            copyCommands.Reverse();
            // Get all properties and document them
            foreach (var itm in copyCommands.Where(o => o.Key != "$type"))
            {
                writer.WriteLine("\t/** @type {{{0}}} */", itm.Value);
                writer.WriteLine("\tthis.{0} = copyData.{0};", itm.Key);
            }
            writer.WriteLine("\t}");

            writer.WriteLine("}}  // {0} ", jobject.Id);
        }
    }
}
