﻿using System;
using System.IO;
using System.Linq;
using BinaryObjectScanner.Interfaces;
using UnshieldSharp.Archive;

namespace BurnOutSharp.FileType
{
    /// <summary>
    /// InstallShield archive v3
    /// </summary>
    public class InstallShieldArchiveV3 : IExtractable
    {
        /// <inheritdoc/>
        public string Extract(string file)
        {
            if (!File.Exists(file))
                return null;

            using (var fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Extract(fs, file);
            }
        }

        /// <inheritdoc/>
        public string Extract(Stream stream, string file)
        {
            // Create a temp output directory
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);

            UnshieldSharp.Archive.InstallShieldArchiveV3 archive = new UnshieldSharp.Archive.InstallShieldArchiveV3(file);
            foreach (CompressedFile cfile in archive.Files.Select(kvp => kvp.Value))
            {
                string tempFile = Path.Combine(tempPath, cfile.FullPath);
                if (!Directory.Exists(Path.GetDirectoryName(tempFile)))
                    Directory.CreateDirectory(Path.GetDirectoryName(tempFile));

                (byte[] fileContents, string error) = archive.Extract(cfile.FullPath);
                if (!string.IsNullOrWhiteSpace(error))
                    continue;

                using (FileStream fs = File.OpenWrite(tempFile))
                {
                    fs.Write(fileContents, 0, fileContents.Length);
                }
            }

            return tempPath;
        }
    }
}
