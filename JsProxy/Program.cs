/*
 * Copyright 2015-2018 Mohawk College of Applied Arts and Technology
 *
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you
 * may not use this file except in compliance with the License. You may
 * obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations under
 * the License.
 *
 * User: justin
 * Date: 2018-7-23
 */

using Microsoft.CSharp;
using MohawkCollege.Util.Console.Parameters;
using Newtonsoft.Json;
using SanteDB.Core.Applets.ViewModel.Json;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Attributes;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Services;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace JsProxy
{
    internal class Program
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

        /// <summary>
        /// Document transfor
        /// </summary>
        private static XslCompiledTransform m_docTransform;

        private static void Main(string[] args)
        {
            var parms = new ParameterParser<ConsoleParameters>().Parse(args);

            Console.WriteLine("SanteDB ViewModel Utility v{0} ({1})", Assembly.GetEntryAssembly().GetName().Version, Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);
            Console.WriteLine(Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright);

            if (parms.Help || args.Length == 0)
            {
                new ParameterParser<ConsoleParameters>().WriteHelp(Console.Out);
                return;
            }
            if (parms.JsProxy)
            {
                m_docTransform = new XslCompiledTransform();
                using (var sr = typeof(Program).Assembly.GetManifestResourceStream("JsProxy.xdoc.xslt"))
                {
                    using (var xr = XmlReader.Create(sr))
                    {
                        m_docTransform.Load(xr, new XsltSettings()
                        {
                            EnableScript = true
                        }, null);
                    }
                }

                Dictionary<String, Object> metaData = new Dictionary<string, object>();

                // First we want to open the output file
                using (TextWriter output = File.CreateText(parms.Output ?? "out.js"))
                {
                    foreach (var asm in parms.AssemblyFile)
                    {
                        // Output namespace
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(parms.DocumentationFile ?? Path.ChangeExtension(asm, "xml"));

                        List<Type> enumerationTypes = new List<Type>();

                        List<Type> alreadyGenerated = new List<Type>();
                        foreach (var type in Assembly.LoadFile(asm).GetTypes().Where(o => o.GetCustomAttribute<JsonObjectAttribute>() != null))
                            GenerateTypeDocumentation(output, type, xmlDoc, parms, enumerationTypes, alreadyGenerated, metaData);
                        // Generate type documentation for each of the binding enumerations
                        foreach (var typ in enumerationTypes.Distinct())
                            GenerateEnumerationDocumentation(output, typ, xmlDoc, parms);

                        GenerateEnumerationDocumentation(output, typeof(NullReasonKeys), xmlDoc, parms);
                    }

                    output.Write(
                        @"
EmptyGuid = ""00000000 - 0000 - 0000 - 0000 - 000000000000"";

/**
* @class
* @summary Represents a simple exception class
* @constructor
* @memberof OpenIZModel
* @property {string} message Informational message about the exception
* @property {any} details Any detail / diagnostic information
* @property {Exception} cause The cause of the exception
* @param {string} type The type of exception
* @param {string} message Informational message about the exception
* @param {any} detail Any detail / diagnostic information
* @param {Exception} cause The cause of the exception
*/
function Exception(type, message, detail, cause, stack, policyId, policyOutcome, rules, data) {
    _self = this;
    /** @type {string} */
    this.$type = type;
    /** @type {string} */
    this.message = message;
    /** @type {string} */
    this.detail = detail;
    /** @type {Exception} */
    this.cause = cause;
    /** @type {string} */
    this.stack = stack;
    /** @type {string} */
    this.policy = policyId;
    /** @type {string} */
    this.policyOutcome = policyOutcome;
    /** @type {Array} */
    this.rules = rules;
    /** @type {Array} */
    this.data = data;
}

                    "
                    );
                }

                using (TextWriter outputMeta = File.CreateText(Path.ChangeExtension(parms.Output ?? "out.js", ".mex.json")))
                {
                    outputMeta.Write(JsonConvert.SerializeObject(metaData));
                }
            }
            else if (parms.ViewModelSerializer)
            {
                // First we want to open the output file
                using (TextWriter output = File.CreateText(parms.Output ?? "out.cs"))
                {
                    foreach (var asm in parms.AssemblyFile)
                    {
                        JsonSerializerFactory serFact = new JsonSerializerFactory();
                        CSharpCodeProvider csProvider = new CSharpCodeProvider();
                        CodeCompileUnit compileUnit = new CodeCompileUnit();

                        // Add namespace
                        compileUnit.Namespaces.Add(serFact.CreateCodeNamespace(parms.Namespace ?? Path.GetFileNameWithoutExtension(asm) + ".Json.Formatter", Assembly.LoadFile(asm)));
                        compileUnit.ReferencedAssemblies.Add("System.dll");
                        compileUnit.ReferencedAssemblies.Add("Newtonsoft.Json.dll");
                        compileUnit.ReferencedAssemblies.Add(typeof(IdentifiedData).Assembly.Location);
                        compileUnit.ReferencedAssemblies.Add(typeof(IJsonViewModelTypeFormatter).Assembly.Location);
                        compileUnit.ReferencedAssemblies.Add(typeof(Tracer).Assembly.Location);
                        csProvider.GenerateCodeFromCompileUnit(compileUnit, output, new CodeGeneratorOptions()
                        {
                            BlankLinesBetweenMembers = true
                        });
                    }
                }
            }
            else if (parms.ServiceDocumentation)
            {
                // First we want to open the output file
                foreach (var asm in parms.AssemblyFile)
                {
                    // Output namespace
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(parms.DocumentationFile ?? Path.ChangeExtension(asm, "xml"));

                    List<Type> serviceTypes = new List<Type>();

                    List<Type> alreadyGenerated = new List<Type>();
                    foreach (var type in Assembly.LoadFile(asm).GetTypes().Where(o => typeof(IServiceImplementation).IsAssignableFrom(o) && o.IsInterface))
                    {
                        var fileName = type.Name;
                        if (fileName.Contains("`"))
                            fileName = fileName.Substring(0, fileName.IndexOf("`"));

                        var metaData = new Dictionary<String, Object>();
                        using (TextWriter output = File.CreateText(Path.Combine(parms.Output ?? "out", fileName + ".md")))
                            GenerateServiceDocumentation(output, type, xmlDoc);
                    }
                }
            }
        }

        /// <summary>
        /// Generate service documentation
        /// </summary>
        private static void GenerateServiceDocumentation(TextWriter writer, Type type, XmlDocument xmlDoc)
        {
            // Emit the template
            writer.WriteLine("---");
            writer.WriteLine("description: {0} ({1})", GenerateCSName(type), type.Assembly.GetName().Name);
            writer.WriteLine("---");
            writer.WriteLine("\r\n## Summary");

            // Lookup the summary information
            var typeDoc = xmlDoc.SelectSingleNode(String.Format("//*[local-name() = 'member'][@name = 'T:{0}']", type.FullName));
            if (typeDoc != null)
            {
                if (typeDoc.SelectSingleNode(".//*[local-name() = 'summary']") != null)
                    writer.WriteLine(typeDoc.SelectSingleNode(".//*[local-name() = 'summary']").InnerText?.Trim());
                if (typeDoc.SelectSingleNode(".//*[local-name() = 'remarks']") != null)
                {
                    writer.WriteLine("\r\n### Remarks");
                    writer.WriteLine(typeDoc.SelectSingleNode(".//*[local-name() = 'remarks']").InnerText?.Trim());
                }
            }

            List<MethodInfo> ignores = new List<MethodInfo>();

            if (type.GetRuntimeEvents().Any())
            {
                writer.WriteLine("\r\n## Events\r\n");
                writer.WriteLine(("|Event|Type|Description|"));
                writer.WriteLine("|-|-|-|");
                foreach (var itm in type.GetRuntimeEvents())
                {
                    ignores.Add(itm.GetAddMethod());
                    ignores.Add(itm.GetRemoveMethod());
                    var docText = xmlDoc
                        .SelectSingleNode(String.Format("//*[local-name() = 'member'][@name = 'E:{0}.{1}']", itm.DeclaringType.FullName, itm.Name))?
                        .SelectSingleNode(".//*[local-name() = 'summary']")?
                        .InnerText?
                        .Replace("\r\n", "")
                        .Trim();

                    writer.Write("|{0}|{1}|", itm.Name, GenerateCSName(itm.EventHandlerType).Replace("<", "&lt;"));
                    if (!String.IsNullOrEmpty(docText))
                        writer.WriteLine("{0}|", docText);
                    else
                        writer.WriteLine("TODO|");
                }
            }

            if (type.GetRuntimeProperties().Any())
            {
                writer.WriteLine("\r\n## Properties\r\n");

                writer.WriteLine(("|Property|Type|Access|Description|"));
                writer.WriteLine("|-|-|-|-|");
                foreach (var itm in type.GetRuntimeProperties())
                {
                    ignores.AddRange(itm.GetAccessors());

                    var docText = xmlDoc
                        .SelectSingleNode(String.Format("//*[local-name() = 'member'][@name = 'P:{0}.{1}']", itm.DeclaringType.FullName, itm.Name))?
                        .SelectSingleNode(".//*[local-name() = 'summary']")?
                        .InnerText?
                        .Replace("\r\n", "")
                        .Trim();

                    writer.Write("|{0}|{1}|{2}{3}|", itm.Name, GenerateCSName(itm.PropertyType).Replace("<", "&lt;"), itm.CanRead ? "R" : "", itm.CanWrite ? "W" : "");
                    if (!String.IsNullOrEmpty(docText))
                        writer.WriteLine("{0}|", docText);
                    else
                        writer.WriteLine("TODO|");
                }
            }

            if (type.GetRuntimeMethods().Any(r => !ignores.Contains(r)))
            {
                writer.WriteLine("\r\n## Operations\r\n");
                writer.WriteLine(("|Operation|Response/Return|Input/Parameter|Description|"));
                writer.WriteLine("|-|-|-|-|");
                foreach (var itm in type.GetRuntimeMethods())
                {
                    if (ignores.Contains(itm)) continue;

                    var docText = xmlDoc
                        .SelectSingleNode(String.Format("//*[local-name() = 'member'][contains(@name, '{0}')]", GenerateXName(itm)))?
                        .SelectSingleNode(".//*[local-name() = 'summary']")?
                        .InnerText?
                        .Replace("\r\n", "")
                        .Trim();

                    writer.Write("|{0}|{1}|{2}|", itm.Name, GenerateCSName(itm.ReturnType).Replace("<", "&lt;"), String.Join("<br/>", itm.GetParameters().Select(p => $"*{GenerateCSName(p.ParameterType).Replace("<", "&lt;")}* **{p.Name}**")));
                    if (!String.IsNullOrEmpty(docText))
                        writer.WriteLine("{0}|", docText);
                    else
                        writer.WriteLine("TODO|");
                }
            }

            writer.WriteLine("\r\n## Implementations\r\n");

            // Find all implementations
            bool hasImpl = false;
            foreach (var itm in Directory.GetFiles(Path.GetDirectoryName(type.Assembly.Location), "*.dll"))
                try
                {
                    var asm = Assembly.LoadFile(itm);
                    foreach (var impl in asm.GetTypes().Where(t => type.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface))
                    {
                        var spa = impl.GetCustomAttribute<ServiceProviderAttribute>(false);

                        XmlDocument subDoc = new XmlDocument();
                        typeDoc = null;
                        try
                        {
                            subDoc.Load(Path.ChangeExtension(impl.Assembly.Location, "xml"));
                            typeDoc = subDoc.SelectSingleNode(String.Format("//*[local-name() = 'member'][@name = 'T:{0}']", impl.FullName));
                        }
                        catch { }
                        writer.WriteLine("\r\n### {0} - ({1})", spa?.Name ?? impl.Name, impl.Assembly.GetName().Name);

                        if (typeDoc?.SelectSingleNode(".//*[local-name() = 'summary']") != null)
                            writer.WriteLine(typeDoc?.SelectSingleNode(".//*[local-name() = 'summary']")?.InnerText?.Trim());
                        else
                            writer.WriteLine("TODO: Document this");

                        writer.WriteLine("\r\n#### Service Registration");
                        writer.WriteLine("```markup\r\n...\r\n<section xsi:type=\"ApplicationServiceContextConfigurationSection\" threadPoolSize=\"4\">\r\n\t<serviceProviders>\r\n\t\t...");
                        writer.WriteLine("\t\t<add type=\"{0}\" />", impl.AssemblyQualifiedName);
                        writer.WriteLine("\t\t...\r\n\t</serviceProviders>\r\n```");

                        hasImpl = true;
                    }
                }
                catch { }

            if (!hasImpl)
                writer.WriteLine("None\r\n");

            if (typeDoc?.SelectSingleNode(".//*[local-name() = 'example']") != null)
            {
                writer.WriteLine("## Example Use");
                writer.WriteLine("```csharp");
                writer.WriteLine(typeDoc.SelectSingleNode(".//*[local-name() = 'example']").InnerText?.Trim());
                writer.WriteLine("```");
            }

            writer.WriteLine("## Example Implementation");
            writer.WriteLine("```csharp");
            writer.WriteLine("/// Example Implementation");
            writer.WriteLine("using {0};", type.Namespace);
            writer.WriteLine("/// Other usings here");
            if (!type.IsGenericTypeDefinition)
                writer.WriteLine("public class My{0} : {1} {{ ", type.Name.Substring(1), type.FullName);
            else
                writer.WriteLine("public class My{0}<{2}> : {1}<{2}> {{ ", type.Name.Substring(1, type.Name.Length - 3), type.FullName.Substring(0, type.FullName.Length - 2), String.Join(",", type.GetGenericArguments().Select(o => o.Name)));

            // Get all properties
            writer.WriteLine("\tpublic String ServiceName => \"My own {0} service\";", type.Name);
            foreach (var itm in type.GetRuntimeEvents())
            {
                typeDoc = xmlDoc.SelectSingleNode(String.Format("//*[local-name() = 'member'][@name = 'E:{0}.{1}']", itm.DeclaringType.FullName, itm.Name));
                if (typeDoc != null)
                {
                    writer.WriteLine("\t/// <summary>");
                    if (typeDoc.SelectSingleNode(".//*[local-name() = 'summary']") != null)
                        writer.WriteLine("\t/// {0}", typeDoc.SelectSingleNode(".//*[local-name() = 'summary']").InnerText.Replace("\r\n", "").Trim());
                    writer.WriteLine("\t/// </summary>");
                }

                writer.WriteLine("\tpublic event {0} {1};", GenerateCSName(itm.EventHandlerType), itm.Name);
            }

            foreach (var itm in type.GetRuntimeProperties())
            {
                // Output documentation
                typeDoc = xmlDoc.SelectSingleNode(String.Format("//*[local-name() = 'member'][@name = 'P:{0}.{1}']", itm.DeclaringType.FullName, itm.Name));
                if (typeDoc != null)
                {
                    writer.WriteLine("\t/// <summary>");
                    if (typeDoc.SelectSingleNode(".//*[local-name() = 'summary']") != null)
                        writer.WriteLine("\t/// {0}", typeDoc.SelectSingleNode(".//*[local-name() = 'summary']").InnerText.Replace("\r\n", "").Trim());
                    writer.WriteLine("\t/// </summary>");
                }

                writer.WriteLine("\tpublic {0} {1} {{", GenerateCSName(itm.PropertyType), itm.Name); ;

                if (itm.CanRead)
                    writer.WriteLine("\t\tget;");
                if (itm.CanWrite)
                    writer.WriteLine("\t\tset;");
                writer.WriteLine("\t}");
            }

            foreach (var itm in type.GetRuntimeMethods())
            {
                if (ignores.Contains(itm)) continue;
                // Output documentation
                typeDoc = xmlDoc.SelectSingleNode(String.Format("//*[local-name() = 'member'][contains(@name, '{0}')]", GenerateXName(itm)));
                if (typeDoc != null)
                {
                    writer.WriteLine("\t/// <summary>");
                    if (typeDoc.SelectSingleNode(".//*[local-name() = 'summary']") != null)
                        writer.WriteLine("\t/// {0}", typeDoc.SelectSingleNode(".//*[local-name() = 'summary']").InnerText.Replace("\r\n", "").Trim());
                    writer.WriteLine("\t/// </summary>");
                }
                writer.Write("\tpublic {0} {1}", GenerateCSName(itm.ReturnType), itm.Name);
                if (itm.IsGenericMethodDefinition)
                    writer.Write("<{0}>", String.Join(",", itm.GetGenericArguments().Select(o => GenerateCSName(o))));
                writer.Write("({0})", String.Join(",", itm.GetParameters().Select(p => $"{GenerateCSName(p.ParameterType)} {p.Name}")));
                writer.WriteLine("{");
                writer.WriteLine("\t\tthrow new System.NotImplementedException();");
                writer.WriteLine("\t}");
            }

            writer.WriteLine("}");
            writer.WriteLine("```");
        }

        private static string GenerateCSName(Type type)
        {
            if (type == typeof(void))
                return "void";
            else if (type.IsGenericParameter)
                return type.Name;
            else if (type.IsGenericType || type.IsGenericTypeDefinition)
                return $"{type.Name.Substring(0, type.Name.Length - 2)}<{String.Join(",", type.GetGenericArguments().Select(n => GenerateCSName(n)))}>";
            else
                return type.Name;
        }

        private static string GenerateXName(MethodInfo method)
        {
            StringBuilder sb = new StringBuilder("M:");
            sb.Append(method.DeclaringType.FullName);
            sb.Append(".");
            sb.Append(method.Name);
            if (method.IsGenericMethodDefinition)
                sb.AppendFormat("``{0}", method.GetGenericArguments().Length);
            sb.Append("(");

            return sb.ToString();
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
            writer.WriteLine("// if(!{0})", jobject.Id);

            writer.WriteLine("/**");
            writer.WriteLine(" * @enum {string}");
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
        private static void GenerateTypeDocumentation(TextWriter writer, Type type, XmlDocument xmlDoc, ConsoleParameters parms, List<Type> enumerationTypes, List<Type> alreadyGenerated, Dictionary<String, object> metaData)
        {
            if (parms.NoAbstract && type.IsAbstract) return;

            var propertyData = new Dictionary<String, object>();

            if (!type.IsAbstract && !type.IsInterface && !type.IsGenericTypeDefinition)
            {
                metaData.Add(type.Name, propertyData);
            }

            if (alreadyGenerated.Contains(type))
                return;
            else
                alreadyGenerated.Add(type);
            writer.WriteLine("// {0}", type.AssemblyQualifiedName);
            writer.WriteLine("//if(!{0})", type.GetCustomAttribute<JsonObjectAttribute>().Id);
            writer.WriteLine("/**");
            writer.WriteLine(" * @class");
            writer.WriteLine(" * @constructor");
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
                    writer.WriteLine(" * @summary {0}", TransformXDoc(typeDoc.SelectSingleNode(".//*[local-name() = 'summary']")));
                if (typeDoc.SelectSingleNode(".//*[local-name() = 'remarks']") != null)
                    writer.WriteLine(" * @description {0}", TransformXDoc(typeDoc.SelectSingleNode(".//*[local-name() = 'remarks']")));
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

                // We're going to add this to metadata
                var propertyInfo = new Dictionary<String, Object>();
                if (propertyData.TryGetValue(jprop.PropertyName, out object cValue))
                {
                    propertyInfo = (Dictionary<String, Object>)cValue;
                    if (propertyInfo["type"].Equals("Guid"))
                    {
                        propertyInfo["type"] = itm.PropertyType.StripGeneric().Name;
                    }
                }
                else
                {
                    propertyData.Add(jprop.PropertyName, propertyInfo);
                    // Now - let's add some info
                    propertyInfo.Add("isCollection", typeof(ICollection).IsAssignableFrom(itm.PropertyType));
                    propertyInfo.Add("type", itm.PropertyType.StripGeneric().Name);
                }

                if (propertyInfo["isCollection"].Equals(true)) // Classifier?
                {
                    var classifier = itm.PropertyType.StripGeneric().GetCustomAttribute<ClassifierAttribute>();
                    if (classifier != null)
                    {
                        var classifierProperty = itm.PropertyType.StripGeneric().GetProperty(classifier.ClassifierProperty);
                        propertyInfo.Add("classifierType", classifierProperty.PropertyType.StripGeneric().Name);

                        var sredir = classifierProperty.GetCustomAttribute<SerializationReferenceAttribute>();
                        if (sredir != null)
                        {
                            classifierProperty = itm.PropertyType.StripGeneric().GetProperty(sredir.RedirectProperty);
                        }

                        var binding = classifierProperty.GetCustomAttribute<BindingAttribute>();
                        if (binding != null)
                        {
                            propertyInfo.Add("classifierValues", binding.Binding.GetFields().Where(r => r.FieldType == typeof(Guid)).Select(o => o.Name));
                        }
                    }
                }
                else
                {
                    var binding = itm.GetCustomAttribute<BindingAttribute>();
                    if (binding != null)
                    {
                        propertyInfo.Add("values", binding.Binding.GetFields().Where(r => r.FieldType == typeof(Guid)).Select(o => o.Name));
                    }
                }

                // Output documentation
                typeDoc = xmlDoc.SelectSingleNode(String.Format("//*[local-name() = 'member'][@name = 'P:{0}.{1}']", itm.DeclaringType.FullName, itm.Name));
                if (typeDoc != null)
                {
                    if (typeDoc.SelectSingleNode(".//*[local-name() = 'summary']") != null)
                        writer.Write($" {TransformXDoc(typeDoc.SelectSingleNode(".//*[local-name() = 'summary']"))}");
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
                writer.WriteLine("\t/**");
                writer.WriteLine("\t * @type {{{0}}} ", itm.Value);
                writer.WriteLine("\t */");
                writer.WriteLine("\tthis.{0} = copyData.{0};", itm.Key);
            }
            writer.WriteLine("\t}");

            writer.WriteLine("}}  // {0} ", jobject.Id);
        }

        /// <summary>
        /// Transform from XML doc to HTML
        /// </summary>
        private static object TransformXDoc(XmlNode documentationNode)
        {
            using (StringReader sr = new StringReader(documentationNode.OuterXml))
            {
                using (XmlReader xr = XmlReader.Create(sr))
                {
                    using (StringWriter sw = new StringWriter())
                    {
                        using (XmlWriter xw = XmlWriter.Create(sw, new XmlWriterSettings()
                        {
                            ConformanceLevel = ConformanceLevel.Fragment,
                            Indent = false
                        }))
                        {
                            m_docTransform.Transform(xr, xw);
                        }
                        return sw.ToString().Trim();
                    }
                }
            }
        }
    }
}