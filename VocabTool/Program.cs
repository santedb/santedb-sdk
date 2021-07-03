using ClosedXML.Excel;
using MohawkCollege.Util.Console.Parameters;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Export;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VocabTool
{
    class Program
    {

        private const int COL_TERM = 1;
        private const int COL_LANG = 2;
        private const int COL_DISPLAY = 3;
        private const int COL_CONCEPT = 4;
        private const int COL_MNEMONIC = 5;
        private const int COL_CS_URI = 6;
        private const int COL_CS_OID = 7;
        private const int COL_CS_AUTH = 8;
        private const int COL_CS_NAME = 9;
        private const int COL_CS_UUID = 10;

        // Code system mapping
        private static Dictionary<String, CodeSystem> m_codeSystemMap = new Dictionary<string, CodeSystem>();

        /// <summary>
        /// Process the specified excel file into a dataset file
        /// </summary>
        static void Main(string[] args)
        {
            try
            {
                var parms = new ParameterParser<ConsoleParameters>().Parse(args);
                if (!File.Exists(parms.SourceFile))
                {
                    throw new FileNotFoundException($"{parms.SourceFile} not found");
                }

                Dataset retVal = new Dataset()
                {
                    Id = parms.Name ?? "Imported Dataset",
                    Action = new List<DataInstallAction>()
                };

                // Open excel file stream
                using (var excelFileStream = File.OpenRead(parms.SourceFile))
                {
                    using (var importWkb = new XLWorkbook(excelFileStream, new LoadOptions()
                    {
                        RecalculateAllFormulas = false
                    }))
                    {
                        retVal.Action = importWkb.Worksheets.SelectMany(o => o.Rows()).SelectMany(o => CreateReferenceTermInstruction(o, parms)).ToList();
                    }
                }

                if (parms.OutputFile == "-")
                    new XmlSerializer(typeof(Dataset)).Serialize(Console.Out, retVal);
                else 
                    using(var fs = File.Create(parms.OutputFile))
                    {
                        new XmlSerializer(typeof(Dataset)).Serialize(fs, retVal);
                    }
            }
            catch(Exception e )
            {
                Console.Error.WriteLine("Error processing file: {0}", e);
            }
        }

        /// <summary>
        /// Create reference term instructions
        /// </summary>
        private static IEnumerable<DataInstallAction> CreateReferenceTermInstruction(IXLRow row, ConsoleParameters parms)
        {
            if (row.Cell(COL_TERM).GetString() == "Reference Term")
                yield break;

            // Create an instruction for the concept
            if(parms.CreateConcept)
            {
                yield return new DataUpdate()
                {
                    InsertIfNotExists = true,
                    IgnoreErrors = true,
                    Element = new Concept()
                    {
                        Key = Guid.Parse( row.Cell(COL_CONCEPT).GetValue<String>()),
                        Mnemonic = row.Cell(COL_MNEMONIC).GetValue<String>(),
                        ConceptNames = new List<ConceptName>()
                           {
                               new ConceptName(row.Cell(COL_LANG).GetValue<String>(), row.Cell(COL_DISPLAY).GetValue<String>())
                           }
                    }
                };
            }

            if(!m_codeSystemMap.TryGetValue(row.Cell(COL_CS_AUTH).GetValue<String>(), out CodeSystem cs) ){
                cs = new CodeSystem()
                {
                    Authority = row.Cell(COL_CS_AUTH).GetValue<String>(),
                    Name = row.Cell(COL_CS_NAME).GetValue<String>(),
                    Oid = row.Cell(COL_CS_OID).GetValue<String>(),
                    Url = row.Cell(COL_CS_URI).GetValue<String>(),
                    Key = Guid.Parse(row.Cell(COL_CS_UUID).GetValue<String>())
                };
                m_codeSystemMap.Add(cs.Authority, cs);
                yield return new DataUpdate()
                {
                    InsertIfNotExists = true,
                    IgnoreErrors = false,
                    Element = cs
                };
            }

            var uuid = Guid.NewGuid();

            yield return new DataUpdate()
            {
                InsertIfNotExists = true,
                IgnoreErrors = true,
                Element = new ReferenceTerm()
                {
                    Mnemonic = row.Cell(COL_TERM).GetValue<String>(),
                    DisplayNames = new List<ReferenceTermName>()
                      {
                          new ReferenceTermName(row.Cell(COL_LANG).GetValue<String>(), row.Cell(COL_DISPLAY).GetValue<String>())
                      },
                    Key = uuid,
                    CodeSystem = cs
                }
            };

            // Link to term
            yield return new DataUpdate()
            {
                InsertIfNotExists = true,
                Element = new ConceptReferenceTerm()
                {
                    Key = Guid.NewGuid(),
                    SourceEntityKey = Guid.Parse(row.Cell(COL_CONCEPT).GetValue<String>()),
                    ReferenceTermKey = uuid,
                    RelationshipTypeKey = ConceptRelationshipTypeKeys.SameAs
                }
            };
        }
    }
}
