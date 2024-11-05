﻿using System;
using System.IO;
using BinaryObjectScanner.Interfaces;
#if NET462_OR_GREATER || NETCOREAPP
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Readers;
#endif

namespace BinaryObjectScanner.FileType
{
    /// <summary>
    /// PKWARE ZIP archive and derivatives
    /// </summary>
    public class PKZIP : IExtractable
    {
        /// <inheritdoc/>
        public bool Extract(string file, string outDir, bool includeDebug)
            => Extract(file, outDir, lookForHeader: false, includeDebug);

        /// <inheritdoc cref="IExtractable.Extract(string, string, bool)"/>
        public bool Extract(string file, string outDir, bool lookForHeader, bool includeDebug)
        {
            if (!File.Exists(file))
                return false;

            using var fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return Extract(fs, file, outDir, lookForHeader, includeDebug);
        }

        /// <inheritdoc/>
        public bool Extract(Stream? stream, string file, string outDir, bool includeDebug)
            => Extract(stream, file, outDir, lookForHeader: false, includeDebug);

        /// <inheritdoc cref="IExtractable.Extract(Stream?, string, string, bool)"/>
        public bool Extract(Stream? stream, string file, string outDir, bool lookForHeader, bool includeDebug)
        {
            if (stream == null || !stream.CanRead)
                return false;

#if NET462_OR_GREATER || NETCOREAPP
            try
            {
                var readerOptions = new ReaderOptions() { LookForHeader = lookForHeader };
                using var zipFile = ZipArchive.Open(stream, readerOptions);
                foreach (var entry in zipFile.Entries)
                {
                    try
                    {
                        // If the entry is a directory
                        if (entry.IsDirectory)
                            continue;

                        // If the entry has an invalid key
                        if (entry.Key == null)
                            continue;

                        // If the entry is partial due to an incomplete multi-part archive, skip it
                        if (!entry.IsComplete)
                            continue;

                        string tempFile = Path.Combine(outDir, entry.Key);
                        var directoryName = Path.GetDirectoryName(tempFile);
                        if (directoryName != null && !Directory.Exists(directoryName))
                            Directory.CreateDirectory(directoryName);

                        entry.WriteToFile(tempFile);
                    }
                    catch (Exception ex)
                    {
                        if (includeDebug) Console.WriteLine(ex);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                if (includeDebug) Console.WriteLine(ex);
                return false;
            }
#else
            return false;
#endif
        }
    }
}
