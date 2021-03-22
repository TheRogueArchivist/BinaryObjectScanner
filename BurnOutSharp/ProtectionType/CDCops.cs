﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BurnOutSharp.Matching;

namespace BurnOutSharp.ProtectionType
{
    public class CDCops : IContentCheck, IPathCheck
    {
        /// <inheritdoc/>
        public string CheckContents(string file, byte[] fileContent, bool includePosition = false)
        {
            var matchers = new List<Matcher>
            {
                // CD-Cops,  ver. 
                new Matcher(new byte?[]
                {
                    0x43, 0x44, 0x2D, 0x43, 0x6F, 0x70, 0x73, 0x2C,
                    0x20, 0x20, 0x76, 0x65, 0x72, 0x2E, 0x20
                }, GetVersion, "CD-Cops"),

                // .grand + (char)0x00
                new Matcher(new byte?[] { 0x2E, 0x67, 0x72, 0x61, 0x6E, 0x64, 0x00 }, "CD-Cops"),
            };

            return MatchUtil.GetFirstContentMatch(file, fileContent, matchers, includePosition);
        }

        /// <inheritdoc/>
        public string CheckDirectoryPath(string path, IEnumerable<string> files)
        {
            if (files.Any(f => Path.GetFileName(f).Equals("CDCOPS.DLL", StringComparison.OrdinalIgnoreCase))
                && (files.Any(f => Path.GetExtension(f).Trim('.').Equals("GZ_", StringComparison.OrdinalIgnoreCase))
                    || files.Any(f => Path.GetExtension(f).Trim('.').Equals("W_X", StringComparison.OrdinalIgnoreCase))
                    || files.Any(f => Path.GetExtension(f).Trim('.').Equals("Qz", StringComparison.OrdinalIgnoreCase))
                    || files.Any(f => Path.GetExtension(f).Trim('.').Equals("QZ_", StringComparison.OrdinalIgnoreCase))))
            {
                return "CD-Cops";
            }

            return null;
        }

        /// <inheritdoc/>
        public string CheckFilePath(string path)
        {
            if (Path.GetFileName(path).Equals("CDCOPS.DLL", StringComparison.OrdinalIgnoreCase)
                || Path.GetExtension(path).Trim('.').Equals("GZ_", StringComparison.OrdinalIgnoreCase)
                || Path.GetExtension(path).Trim('.').Equals("W_X", StringComparison.OrdinalIgnoreCase)
                || Path.GetExtension(path).Trim('.').Equals("Qz", StringComparison.OrdinalIgnoreCase)
                || Path.GetExtension(path).Trim('.').Equals("QZ_", StringComparison.OrdinalIgnoreCase))
            {
                return "CD-Cops";
            }
            
            return null;
        }

        public static string GetVersion(string file, byte[] fileContent, List<int> positions)
        {
            char[] version = new ArraySegment<byte>(fileContent, positions[0] + 15, 4).Select(b => (char)b).ToArray();
            if (version[0] == 0x00)
                return string.Empty;

            return new string(version);
        }
    }
}
