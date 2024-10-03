using System;
using System.IO;
#if NET452_OR_GREATER || NETCOREAPP
using System.Text;
#endif
#if NET40_OR_GREATER || NETCOREAPP
using OpenMcdf;
#endif
using SabreTools.IO.Extensions;
using SabreTools.Serialization.Wrappers;
#if NET462_OR_GREATER || NETCOREAPP
using SharpCompress.Archives;
using SharpCompress.Archives.GZip;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives.Tar;
using SharpCompress.Archives.Zip;
using SharpCompress.Compressors;
using SharpCompress.Compressors.BZip2;
using SharpCompress.Compressors.Xz;
#endif
using UnshieldSharp.Archive;

namespace Test
{
    internal class Extractor
    {
        #region Options

        /// <inheritdoc cref="BinaryObjectScanner.Options.IncludeDebug"/>
        public bool IncludeDebug => _options?.IncludeDebug ?? false;

        /// <summary>
        /// Options object for configuration
        /// </summary>
        private readonly BinaryObjectScanner.Options _options;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="includeDebug">Enable including debug information</param>
        public Extractor(bool includeDebug)
        {
            this._options = new BinaryObjectScanner.Options
            {
                IncludeDebug = includeDebug,
            };

#if NET462_OR_GREATER || NETCOREAPP
            // Register the codepages
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
        }

        /// <summary>
        /// Wrapper to extract data for a single path
        /// </summary>
        /// <param name="path">File or directory path</param>
        /// <param name="outputDirectory">Output directory path</param>
        public void ExtractPath(string path, string outputDirectory)
        {
            Console.WriteLine($"Checking possible path: {path}");

            // Check if the file or directory exists
            if (File.Exists(path))
            {
                ExtractFile(path, outputDirectory);
            }
            else if (Directory.Exists(path))
            {
                foreach (string file in IOExtensions.SafeEnumerateFiles(path, "*", SearchOption.AllDirectories))
                {
                    ExtractFile(file, outputDirectory);
                }
            }
            else
            {
                Console.WriteLine($"{path} does not exist, skipping...");
            }
        }

        /// <summary>
        /// Print information for a single file, if possible
        /// </summary>
        private void ExtractFile(string file, string outputDirectory)
        {
            Console.WriteLine($"Attempting to extract all files from {file}");
            using Stream stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            // Get the extension for certain checks
            string extension = Path.GetExtension(file).ToLower().TrimStart('.');

            // Get the first 16 bytes for matching
            byte[] magic = new byte[16];
            try
            {
                stream.Read(magic, 0, 16);
                stream.Seek(0, SeekOrigin.Begin);
            }
            catch (Exception ex)
            {
                if (IncludeDebug) Console.WriteLine(ex);

                return;
            }

            // Get the file type
            WrapperType ft = WrapperFactory.GetFileType(magic, extension);

            // Executables technically can be "extracted", but let's ignore that
            // TODO: Support executables that include other stuff

            // 7-zip
            if (ft == WrapperType.SevenZip)
            {
                // Build the archive information
                Console.WriteLine("Extracting 7-zip contents");
                Console.WriteLine();

#if NET20 || NET35 || NET40 || NET452
                Console.WriteLine("Extraction is not supported for this framework!");
                Console.WriteLine();
#else
                // If the 7-zip file itself fails
                try
                {
                    using SevenZipArchive sevenZipFile = SevenZipArchive.Open(stream);
                    foreach (var entry in sevenZipFile.Entries)
                    {
                        // If an individual entry fails
                        try
                        {
                            // If the entry is a directory
                            if (entry.IsDirectory)
                                continue;

                            // If the entry has an invalid key
                            if (entry.Key == null)
                                continue;

                            string tempFile = Path.Combine(outputDirectory, entry.Key);
                            entry.WriteToFile(tempFile);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Something went wrong extracting 7-zip entry {entry.Key}: {ex}");
                            Console.WriteLine();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something went wrong extracting 7-zip: {ex}");
                    Console.WriteLine();
                }
#endif
            }

            // BFPK archive
            else if (ft == WrapperType.BFPK)
            {
                // Build the BFPK information
                Console.WriteLine("Extracting BFPK contents");
                Console.WriteLine();

                var bfpk = BFPK.Create(stream);
                if (bfpk == null)
                {
                    Console.WriteLine("Something went wrong parsing BFPK archive");
                    Console.WriteLine();
                    return;
                }

                try
                {
                    // Extract the BFPK contents to the directory
                    BinaryObjectScanner.FileType.BFPK.ExtractAll(bfpk, outputDirectory);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something went wrong extracting BFPK archive: {ex}");
                    Console.WriteLine();
                }
            }

            // BSP
            else if (ft == WrapperType.BSP)
            {
                // Build the BSP information
                Console.WriteLine("Extracting BSP contents");
                Console.WriteLine();

                var bsp = BSP.Create(stream);
                if (bsp == null)
                {
                    Console.WriteLine("Something went wrong parsing BSP");
                    Console.WriteLine();
                    return;
                }

                try
                {
                    // Extract the BSP contents to the directory
                    BinaryObjectScanner.FileType.BSP.ExtractAllLumps(bsp, outputDirectory);
                    BinaryObjectScanner.FileType.BSP.ExtractAllTextures(bsp, outputDirectory);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something went wrong extracting BSP: {ex}");
                    Console.WriteLine();
                }
            }

            // bzip2
            else if (ft == WrapperType.BZip2)
            {
                // Build the bzip2 information
                Console.WriteLine("Extracting bzip2 contents");
                Console.WriteLine();

#if NET20 || NET35 || NET40 || NET452
                Console.WriteLine("Extraction is not supported for this framework!");
                Console.WriteLine();
#else
                using var bz2File = new BZip2Stream(stream, CompressionMode.Decompress, true);

                // If an individual entry fails
                try
                {
                    string tempFile = Path.Combine(outputDirectory, Guid.NewGuid().ToString());
                    using FileStream fs = File.OpenWrite(tempFile);
                    bz2File.CopyTo(fs);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something went wrong extracting bzip2: {ex}");
                    Console.WriteLine();
                }
#endif
            }

            // CFB
            else if (ft == WrapperType.CFB)
            {
                // Build the installer information
                Console.WriteLine("Extracting CFB contents");
                Console.WriteLine();

#if NET45_OR_GREATER || NETCOREAPP
                // If the CFB file itself fails
                try
                {
                    using var cf = new CompoundFile(stream, CFSUpdateMode.ReadOnly, CFSConfiguration.Default);
                    cf.RootStorage.VisitEntries((e) =>
                    {
                        if (!e.IsStream)
                            return;

                        var str = cf.RootStorage.GetStream(e.Name);
                        if (str == null)
                            return;

                        byte[] strData = str.GetData();
                        if (strData == null)
                            return;

                        string decoded = BinaryObjectScanner.FileType.CFB.DecodeStreamName(e.Name)?.TrimEnd('\0') ?? string.Empty;
                        byte[] nameBytes = Encoding.UTF8.GetBytes(e.Name);

                        // UTF-8 encoding of 0x4840.
                        if (nameBytes[0] == 0xe4 && nameBytes[1] == 0xa1 && nameBytes[2] == 0x80)
                            decoded = decoded.Substring(3);

                        foreach (char c in Path.GetInvalidFileNameChars())
                        {
                            decoded = decoded.Replace(c, '_');
                        }

                        string filename = Path.Combine(outputDirectory, decoded);
                        using Stream fs = File.OpenWrite(filename);
                        fs.Write(strData, 0, strData.Length);
                    }, recursive: true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something went wrong extracting CFB: {ex}");
                    Console.WriteLine();
                }
#endif
            }

            // GCF
            else if (ft == WrapperType.GCF)
            {
                // Build the GCF information
                Console.WriteLine("Extracting GCF contents");
                Console.WriteLine();

                var gcf = GCF.Create(stream);
                if (gcf == null)
                {
                    Console.WriteLine("Something went wrong parsing GCF");
                    Console.WriteLine();
                    return;
                }

                try
                {
                    // Extract the GCF contents to the directory
                    BinaryObjectScanner.FileType.GCF.ExtractAll(gcf, outputDirectory);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something went wrong extracting GCF: {ex}");
                    Console.WriteLine();
                }
            }

            // gzip
            else if (ft == WrapperType.GZIP)
            {
                // Build the gzip information
                Console.WriteLine("Extracting gzip contents");
                Console.WriteLine();

#if NET20 || NET35 || NET40 || NET452
                Console.WriteLine("Extraction is not supported for this framework!");
                Console.WriteLine();
#else
                using var zipFile = GZipArchive.Open(stream);
                foreach (var entry in zipFile.Entries)
                {
                    // If an individual entry fails
                    try
                    {
                        // If the entry is a directory
                        if (entry.IsDirectory)
                            continue;

                        // If the entry has an invalid key
                        if (entry.Key == null)
                            continue;

                        string tempFile = Path.Combine(outputDirectory, entry.Key);
                        entry.WriteToFile(tempFile);
                    }

                    catch (Exception ex)
                    {
                        Console.WriteLine($"Something went wrong extracting gzip entry {entry.Key}: {ex}");
                        Console.WriteLine();
                    }
                }
#endif
            }

            // InstallShield Archive V3 (Z)
            else if (ft == WrapperType.InstallShieldArchiveV3)
            {
                // Build the InstallShield Archive V3 information
                Console.WriteLine("Extracting InstallShield Archive V3 contents");
                Console.WriteLine();

                // If the cab file itself fails
                try
                {
                    var archive = new InstallShieldArchiveV3(file);
                    foreach (var cfile in archive.Files)
                    {
                        // If an individual entry fails
                        try
                        {
                            string tempFile = Path.Combine(outputDirectory, cfile.Key);
                            string? directoryName = Path.GetDirectoryName(tempFile);
                            if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
                                Directory.CreateDirectory(directoryName);

                            byte[]? fileContents = archive.Extract(cfile.Key, out string? error);
                            if (!string.IsNullOrEmpty(error))
                                continue;

                            if (fileContents != null && fileContents.Length > 0)
                            {
                                using FileStream fs = File.OpenWrite(tempFile);
                                fs.Write(fileContents, 0, fileContents.Length);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Something went wrong extracting InstallShield Archive V3 entry {cfile.Value.Name}: {ex}");
                            Console.WriteLine();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something went wrong extracting InstallShield Archive V3: {ex}");
                    Console.WriteLine();
                }
            }

            // IS-CAB archive
            else if (ft == WrapperType.InstallShieldCAB)
            {
                // Build the archive information
                Console.WriteLine("Extracting IS-CAB contents");
                Console.WriteLine();

                // If the cab file itself fails
                try
                {
                    var cabfile = UnshieldSharp.Cabinet.InstallShieldCabinet.Open(file);
                    if (cabfile?.HeaderList == null)
                    {
                        Console.WriteLine("Something went wrong parsing IS-CAB archive");
                        Console.WriteLine();
                        return;
                    }

                    for (int i = 0; i < cabfile!.HeaderList.FileCount; i++)
                    {
                        // If an individual entry fails
                        try
                        {
                            string? filename = cabfile.HeaderList.GetFileName(i);
                            string tempFile;
                            try
                            {
                                tempFile = Path.Combine(outputDirectory, filename ?? $"BAD_FILENAME{i}");
                            }
                            catch
                            {
                                tempFile = Path.Combine(outputDirectory, $"BAD_FILENAME{i}");
                            }

                            cabfile?.FileSave(i, tempFile);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Something went wrong extracting IS-CAB entry {i}: {ex}");
                            Console.WriteLine();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something went wrong extracting IS-CAB: {ex}");
                    Console.WriteLine();
                }
            }

#if ((NETFRAMEWORK && !NET20 && !NET35 && !NET40) || NETCOREAPP) && WIN
            // Microsoft Cabinet archive
            else if (ft == WrapperType.MicrosoftCAB)
            {
                // Build the cabinet information
                Console.WriteLine("Extracting MS-CAB contents");
                Console.WriteLine();

                var cabinet = new LibMSPackN.MSCabinet(file);
                if (cabinet == null)
                {
                    Console.WriteLine("Something went wrong parsing MS-CAB archive");
                    Console.WriteLine();
                    return;
                }

                try
                {
                    // Extract the MS-CAB contents to the directory
                    foreach (var compressedFile in cabinet.GetFiles())
                    {
                        try
                        {
                            string tempFile = Path.Combine(outputDirectory, compressedFile.Filename);
                            string? directoryName = Path.GetDirectoryName(tempFile);
                            if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
                                Directory.CreateDirectory(directoryName);

                            compressedFile.ExtractTo(tempFile);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Something went wrong extracting Microsoft Cabinet entry {compressedFile.Filename}: {ex}");
                            Console.WriteLine();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something went wrong extracting MS-CAB: {ex}");
                    Console.WriteLine();
                }
            }
#endif

            // Microsoft LZ / LZ32
            else if (ft == WrapperType.MicrosoftLZ)
            {
                // Build the Microsoft LZ / LZ32 information
                Console.WriteLine("Extracting Microsoft LZ / LZ32 contents");
                Console.WriteLine();

                // If the LZ file itself fails
                try
                {
                    byte[]? data = SabreTools.Compression.LZ.Decompressor.Decompress(stream);

                    // Create the temp filename
                    string tempFile = "temp.bin";
                    if (!string.IsNullOrEmpty(file))
                    {
                        string? expandedFilePath = SabreTools.Compression.LZ.Decompressor.GetExpandedName(file, out _);
                        tempFile = Path.GetFileName(expandedFilePath)?.TrimEnd('\0') ?? string.Empty;
                        if (tempFile.EndsWith(".ex"))
                            tempFile += "e";
                        else if (tempFile.EndsWith(".dl"))
                            tempFile += "l";
                    }

                    tempFile = Path.Combine(outputDirectory, tempFile);

                    // Write the file data to a temp file
                    if (data != null && data.Length > 0)
                    {
                        using Stream tempStream = File.Open(tempFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                        tempStream.Write(data, 0, data.Length);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something went wrong extracting Microsoft LZ / LZ32: {ex}");
                    Console.WriteLine();
                }
            }

#if ((NETFRAMEWORK && !NET20 && !NET35 && !NET40) || NETCOREAPP) && WIN
            // MoPaQ (MPQ) archive
            else if (ft == WrapperType.MoPaQ)
            {
                // Build the archive information
                Console.WriteLine("Extracting MoPaQ contents");
                Console.WriteLine();

                // If the MPQ file itself fails
                try
                {
                    using var mpqArchive = new StormLibSharp.MpqArchive(file, FileAccess.Read);

                    // Try to open the listfile
                    string? listfile = null;
                    StormLibSharp.MpqFileStream listStream = mpqArchive.OpenFile("(listfile)");

                    // If we can't read the listfile, we just return
                    if (!listStream.CanRead)
                    {
                        Console.WriteLine("Could not read the listfile, extraction halted!");
                        Console.WriteLine();
                    }

                    // Read the listfile in for processing
                    using (var sr = new StreamReader(listStream))
                    {
                        listfile = sr.ReadToEnd();
                    }

                    // Split the listfile by newlines
                    string[] listfileLines = listfile.Replace("\r\n", "\n").Split('\n');

                    // Loop over each entry
                    foreach (string sub in listfileLines)
                    {
                        // If an individual entry fails
                        try
                        {
                            string tempFile = Path.Combine(outputDirectory, sub);
                            string? directoryName = Path.GetDirectoryName(tempFile);
                            if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
                                Directory.CreateDirectory(directoryName);

                            mpqArchive.ExtractFile(sub, tempFile);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Something went wrong extracting MoPaQ entry {sub}: {ex}");
                            Console.WriteLine();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something went wrong extracting MoPaQ: {ex}");
                    Console.WriteLine();
                }
            }
#endif

            // PAK
            else if (ft == WrapperType.PAK)
            {
                // Build the archive information
                Console.WriteLine("Extracting PAK contents");
                Console.WriteLine();

                var pak = PAK.Create(stream);
                if (pak == null)
                {
                    Console.WriteLine("Something went wrong parsing PAK");
                    Console.WriteLine();
                    return;
                }

                try
                {
                    // Extract the PAK contents to the directory
                    BinaryObjectScanner.FileType.PAK.ExtractAll(pak, outputDirectory);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something went wrong extracting PAK: {ex}");
                    Console.WriteLine();
                }
            }

            // PFF
            else if (ft == WrapperType.PFF)
            {
                // Build the archive information
                Console.WriteLine("Extracting PFF contents");
                Console.WriteLine();

                var pff = PFF.Create(stream);
                if (pff == null)
                {
                    Console.WriteLine("Something went wrong parsing PFF");
                    Console.WriteLine();
                    return;
                }

                try
                {
                    // Extract the PFF contents to the directory
                    BinaryObjectScanner.FileType.PFF.ExtractAll(pff, outputDirectory);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something went wrong extracting PFF: {ex}");
                    Console.WriteLine();
                }
            }

            // PKZIP
            else if (ft == WrapperType.PKZIP)
            {
                // Build the archive information
                Console.WriteLine("Extracting PKZIP contents");
                Console.WriteLine();

#if NET20 || NET35 || NET40 || NET452
                Console.WriteLine("Extraction is not supported for this framework!");
                Console.WriteLine();
#else
                // If the zip file itself fails
                try
                {
                    using ZipArchive zipFile = ZipArchive.Open(stream);
                    foreach (var entry in zipFile.Entries)
                    {
                        // If an individual entry fails
                        try
                        {
                            // If the entry is a directory
                            if (entry.IsDirectory)
                                continue;

                            // If the entry has an invalid key
                            if (entry.Key == null)
                                continue;

                            string tempFile = Path.Combine(outputDirectory, entry.Key);
                            string? directoryName = Path.GetDirectoryName(tempFile);
                            if (directoryName != null)
                                Directory.CreateDirectory(directoryName);
                            entry.WriteToFile(tempFile);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Something went wrong extracting PKZIP entry {entry.Key}: {ex}");
                            Console.WriteLine();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something went wrong extracting PKZIP: {ex}");
                    Console.WriteLine();
                }
#endif
            }

            // Quantum
            else if (ft == WrapperType.Quantum)
            {
                // Build the archive information
                Console.WriteLine("Extracting Quantum contents");
                Console.WriteLine();

                var quantum = Quantum.Create(stream);
                if (quantum == null)
                {
                    Console.WriteLine("Something went wrong parsing Quantum");
                    Console.WriteLine();
                    return;
                }

                try
                {
                    // Extract the Quantum contents to the directory
                    BinaryObjectScanner.FileType.Quantum.ExtractAll(quantum, outputDirectory);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something went wrong extracting Quantum: {ex}");
                    Console.WriteLine();
                }
            }

            // RAR
            else if (ft == WrapperType.RAR)
            {
                // Build the archive information
                Console.WriteLine("Extracting RAR contents");
                Console.WriteLine();

#if NET20 || NET35 || NET40 || NET452
                Console.WriteLine("Extraction is not supported for this framework!");
                Console.WriteLine();
#else
                // If the rar file itself fails
                try
                {
                    using RarArchive rarFile = RarArchive.Open(stream);
                    foreach (var entry in rarFile.Entries)
                    {
                        // If an individual entry fails
                        try
                        {
                            // If the entry is a directory
                            if (entry.IsDirectory)
                                continue;

                            // If the entry has an invalid key
                            if (entry.Key == null)
                                continue;

                            string tempFile = Path.Combine(outputDirectory, entry.Key);
                            entry.WriteToFile(tempFile);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Something went wrong extracting RAR entry {entry.Key}: {ex}");
                            Console.WriteLine();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something went wrong extracting RAR: {ex}");
                    Console.WriteLine();
                }
#endif
            }

            // SGA
            else if (ft == WrapperType.SGA)
            {
                // Build the archive information
                Console.WriteLine("Extracting SGA contents");
                Console.WriteLine();

                var sga = SGA.Create(stream);
                if (sga == null)
                {
                    Console.WriteLine("Something went wrong parsing SGA");
                    Console.WriteLine();
                    return;
                }

                try
                {
                    // Extract the SGA contents to the directory
                    BinaryObjectScanner.FileType.SGA.ExtractAll(sga, outputDirectory);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something went wrong extracting SGA: {ex}");
                    Console.WriteLine();
                }
            }

            // Tape Archive
            else if (ft == WrapperType.RAR)
            {
                // Build the archive information
                Console.WriteLine("Extracting Tape Archive contents");
                Console.WriteLine();

#if NET20 || NET35 || NET40 || NET452
                Console.WriteLine("Extraction is not supported for this framework!");
                Console.WriteLine();
#else
                // If the tar file itself fails
                try
                {
                    using TarArchive tarFile = TarArchive.Open(stream);
                    foreach (var entry in tarFile.Entries)
                    {
                        // If an individual entry fails
                        try
                        {
                            // If the entry is a directory
                            if (entry.IsDirectory)
                                continue;

                            // If the entry has an invalid key
                            if (entry.Key == null)
                                continue;

                            string tempFile = Path.Combine(outputDirectory, entry.Key);
                            entry.WriteToFile(tempFile);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Something went wrong extracting Tape Archive entry {entry.Key}: {ex}");
                            Console.WriteLine();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something went wrong extracting Tape Archive: {ex}");
                    Console.WriteLine();
                }
#endif
            }

            // VBSP
            else if (ft == WrapperType.VBSP)
            {
                // Build the archive information
                Console.WriteLine("Extracting VBSP contents");
                Console.WriteLine();

                var vbsp = VBSP.Create(stream);
                if (vbsp == null)
                {
                    Console.WriteLine("Something went wrong parsing VBSP");
                    Console.WriteLine();
                    return;
                }

                try
                {
                    // Extract the VBSP contents to the directory
                    BinaryObjectScanner.FileType.VBSP.ExtractAllLumps(vbsp, outputDirectory);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something went wrong extracting VBSP: {ex}");
                    Console.WriteLine();
                }
            }

            // VPK
            else if (ft == WrapperType.VPK)
            {
                // Build the archive information
                Console.WriteLine("Extracting VPK contents");
                Console.WriteLine();

                var vpk = VPK.Create(stream);
                if (vpk == null)
                {
                    Console.WriteLine("Something went wrong parsing VPK");
                    Console.WriteLine();
                    return;
                }

                try
                {
                    // Extract the VPK contents to the directory
                    BinaryObjectScanner.FileType.VPK.ExtractAll(vpk, outputDirectory);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something went wrong extracting VPK: {ex}");
                    Console.WriteLine();
                }
            }

            // WAD
            else if (ft == WrapperType.WAD)
            {
                // Build the archive information
                Console.WriteLine("Extracting WAD contents");
                Console.WriteLine();

                var wad = WAD.Create(stream);
                if (wad == null)
                {
                    Console.WriteLine("Something went wrong parsing WAD");
                    Console.WriteLine();
                    return;
                }

                try
                {
                    // Extract the WAD contents to the directory
                    BinaryObjectScanner.FileType.WAD.ExtractAllLumps(wad, outputDirectory);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something went wrong extracting WAD: {ex}");
                    Console.WriteLine();
                }
            }

            // xz
            else if (ft == WrapperType.RAR)
            {
                // Build the xz information
                Console.WriteLine("Extracting xz contents");
                Console.WriteLine();

#if NET20 || NET35 || NET40 || NET452
                Console.WriteLine("Extraction is not supported for this framework!");
                Console.WriteLine();
#else
                using var xzFile = new XZStream(stream);
                // If an individual entry fails
                try
                {
                    string tempFile = Path.Combine(outputDirectory, Guid.NewGuid().ToString());
                    using FileStream fs = File.OpenWrite(tempFile);
                    xzFile.CopyTo(fs);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something went wrong extracting xz: {ex}");
                    Console.WriteLine();
                }
#endif
            }

            // XZP
            else if (ft == WrapperType.XZP)
            {
                // Build the archive information
                Console.WriteLine("Extracting XZP contents");
                Console.WriteLine();

                var xzp = XZP.Create(stream);
                if (xzp == null)
                {
                    Console.WriteLine("Something went wrong parsing XZP");
                    Console.WriteLine();
                    return;
                }

                try
                {
                    // Extract the XZP contents to the directory
                    BinaryObjectScanner.FileType.XZP.ExtractAll(xzp, outputDirectory);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something went wrong extracting XZP: {ex}");
                    Console.WriteLine();
                }
            }

            // Everything else
            else
            {
                Console.WriteLine("Not a supported extractable file format, skipping...");
                Console.WriteLine();
                return;
            }
        }
    }
}