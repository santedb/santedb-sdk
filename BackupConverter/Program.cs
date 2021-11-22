using MohawkCollege.Util.Console.Parameters;
using SharpCompress.Readers.Tar;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace BackupConverter
{
    internal class Program
    {
        /// <summary>
        /// MAGIC HEADER BYTES
        /// </summary>
        private static readonly byte[] MAGIC = { (byte)'S', (byte)'D', (byte)'B', (byte)2 };

        /// <summary>
        /// Prompt for a masked password prompt
        /// </summary>
        internal static string PasswordPrompt(string prompt)
        {
            Console.Write(prompt);

            var c = (ConsoleKey)0;
            StringBuilder passwd = new StringBuilder();
            while (c != ConsoleKey.Enter)
            {
                var ki = Console.ReadKey();
                c = ki.Key;

                if (c == ConsoleKey.Backspace)
                {
                    if (passwd.Length > 0)
                    {
                        passwd = passwd.Remove(passwd.Length - 1, 1);
                        Console.Write(" \b");
                    }
                    else
                        Console.CursorLeft = Console.CursorLeft + 1;
                }
                else if (c == ConsoleKey.Escape)
                    return String.Empty;
                else if (c != ConsoleKey.Enter)
                {
                    passwd.Append(ki.KeyChar);
                    Console.Write("\b*");
                }
            }
            Console.WriteLine();
            return passwd.ToString();
        }

        private static void Main(string[] args)
        {
            var parser = new ParameterParser<ConsoleParameters>();
            var parameters = parser.Parse(args);

            Console.WriteLine("SanteDB Secure Clinical Backup Extractor");
            Console.WriteLine("Version {0}", Assembly.GetEntryAssembly().GetName().Version);
            Console.WriteLine(Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright);

            if (parameters.Help || args.Length == 0)
            {
                parser.WriteHelp(Console.Out);
            }
            else
            {
                using (var fs = File.OpenRead(parameters.Source))
                {
                    // Validate header
                    byte[] header = new byte[MAGIC.Length];
                    fs.Read(header, 0, MAGIC.Length);
                    if (!header.SequenceEqual(MAGIC))
                        throw new InvalidOperationException("Backup file is invalid");

                    Stream inStream = fs;
                    try
                    {
                        // Encrypted?
                        if (fs.ReadByte() == 1)
                        {
                            // Read length of IV
                            byte[] ivLengthByte = new byte[4];
                            fs.Read(ivLengthByte, 0, 4);
                            var ivLength = BitConverter.ToInt32(ivLengthByte, 0);

                            // Read IV
                            byte[] iv = new byte[ivLength];
                            fs.Read(iv, 0, ivLength);

                            // Now Create crypto stream with password
                            var password = PasswordPrompt("Archive Password:");

                            var desCrypto = AesCryptoServiceProvider.Create();
                            var passKey = ASCIIEncoding.ASCII.GetBytes(password);
                            passKey = Enumerable.Range(0, 32).Select(o => passKey.Length > o ? passKey[o] : (byte)0).ToArray();
                            desCrypto.IV = iv;
                            desCrypto.Key = passKey;
                            inStream = new CryptoStream(fs, desCrypto.CreateDecryptor(), CryptoStreamMode.Read);
                        }

                        if (!Directory.Exists(parameters.Destination))
                            Directory.CreateDirectory(parameters.Destination);

                        using (var gzs = new GZipStream(inStream, CompressionMode.Decompress))
                        using (var tr = TarReader.Open(gzs))
                        {
                            // Move to next entry & copy
                            while (tr.MoveToNextEntry())
                            {
                                Console.WriteLine("Extracting : {0}", tr.Entry.Key);
                                var destDir = Path.Combine(parameters.Destination, tr.Entry.Key.Replace('/', Path.DirectorySeparatorChar));
                                if (!Directory.Exists(Path.GetDirectoryName(destDir)))
                                    Directory.CreateDirectory(Path.GetDirectoryName(destDir));
                                if (!tr.Entry.IsDirectory)
                                    using (var s = tr.OpenEntryStream())
                                    using (var ofs = File.Create(Path.Combine(parameters.Destination, tr.Entry.Key)))
                                        s.CopyTo(ofs);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Could not extract backup: {0}", e);
                    }
                }
            }
        }
    }
}