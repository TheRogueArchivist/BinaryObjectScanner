﻿using System;
using System.IO;
using BinaryObjectScanner.Interfaces;
#if NET462_OR_GREATER || NETCOREAPP
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
#endif

namespace BinaryObjectScanner.FileType
{
    /// <summary>
    /// PKWARE ZIP archive and derivatives
    /// </summary>
    public class PKZIP : IExtractable
    {
        /// <inheritdoc/>
        public string? Extract(string file, bool includeDebug)
        {
            if (!File.Exists(file))
                return null;

            using var fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return Extract(fs, file, includeDebug);
        }

        /// <inheritdoc/>
        public string? Extract(Stream? stream, string file, bool includeDebug)
        {
            if (stream == null)
                return null;

#if NET462_OR_GREATER || NETCOREAPP
            try
            {
                // Create a temp output directory
                string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempPath);

                using (ZipArchive zipFile = ZipArchive.Open(stream))
                {
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

                            string tempFile = Path.Combine(tempPath, entry.Key);
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
                }

                return tempPath;
            }
            catch (Exception ex)
            {
                if (includeDebug) Console.WriteLine(ex);
                return null;
            }
#else
            return null;
#endif
        }
    }
}
