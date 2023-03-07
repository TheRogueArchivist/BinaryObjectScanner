﻿using System;
using System.Collections.Generic;
using System.IO;
using BurnOutSharp.Interfaces;
using BurnOutSharp.Matching;
using BinaryObjectScanner.Utilities;
using BurnOutSharp.Wrappers;

namespace BurnOutSharp.Tools
{
    public static class Utilities
    {
        #region File Types

        /// <summary>
        /// Get the supported file type for a magic string
        /// </summary>
        /// <remarks>Recommend sending in 16 bytes to check</remarks>
        public static SupportedFileType GetFileType(byte[] magic)
        {
            // If we have an invalid magic byte array
            if (magic == null || magic.Length == 0)
                return SupportedFileType.UNKNOWN;

            // TODO: For all modelled types, use the constants instead of hardcoded values here
            #region AACSMediaKeyBlock

            // Block starting with verify media key record
            if (magic.StartsWith(new byte?[] { 0x81, 0x00, 0x00, 0x14 }))
                return SupportedFileType.AACSMediaKeyBlock;

            // Block starting with type and version record
            if (magic.StartsWith(new byte?[] { 0x10, 0x00, 0x00, 0x0C }))
                return SupportedFileType.AACSMediaKeyBlock;

            #endregion

            #region BDPlusSVM

            if (magic.StartsWith(new byte?[] { 0x42, 0x44, 0x53, 0x56, 0x4D, 0x5F, 0x43, 0x43 }))
                return SupportedFileType.BDPlusSVM;

            #endregion

            #region BFPK

            if (magic.StartsWith(new byte?[] { 0x42, 0x46, 0x50, 0x4b }))
                return SupportedFileType.BFPK;

            #endregion

            #region BSP

            if (magic.StartsWith(new byte?[] { 0x1e, 0x00, 0x00, 0x00 }))
                return SupportedFileType.BSP;

            #endregion

            #region BZip2

            if (magic.StartsWith(new byte?[] { 0x42, 0x52, 0x68 }))
                return SupportedFileType.BZip2;

            #endregion

            #region CFB

            if (magic.StartsWith(new byte?[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }))
                return SupportedFileType.CFB;

            #endregion

            #region CIA

            // No magic checks for CIA

            #endregion

            #region Executable

            // DOS MZ executable file format (and descendants)
            if (magic.StartsWith(new byte?[] { 0x4d, 0x5a }))
                return SupportedFileType.Executable;

            /*
            // None of the following are supported in scans yet

            // Executable and Linkable Format
            if (magic.StartsWith(new byte?[] { 0x7f, 0x45, 0x4c, 0x46 }))
                return FileTypes.Executable;

            // Mach-O binary (32-bit)
            if (magic.StartsWith(new byte?[] { 0xfe, 0xed, 0xfa, 0xce }))
                return FileTypes.Executable;

            // Mach-O binary (32-bit, reverse byte ordering scheme)
            if (magic.StartsWith(new byte?[] { 0xce, 0xfa, 0xed, 0xfe }))
                return FileTypes.Executable;

            // Mach-O binary (64-bit)
            if (magic.StartsWith(new byte?[] { 0xfe, 0xed, 0xfa, 0xcf }))
                return FileTypes.Executable;

            // Mach-O binary (64-bit, reverse byte ordering scheme)
            if (magic.StartsWith(new byte?[] { 0xcf, 0xfa, 0xed, 0xfe }))
                return FileTypes.Executable;

            // Prefrred Executable File Format
            if (magic.StartsWith(new byte?[] { 0x4a, 0x6f, 0x79, 0x21, 0x70, 0x65, 0x66, 0x66 }))
                return FileTypes.Executable;
            */

            #endregion

            #region GCF

            if (magic.StartsWith(new byte?[] { 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00 }))
                return SupportedFileType.GCF;

            #endregion

            #region GZIP

            if (magic.StartsWith(new byte?[] { 0x1f, 0x8b }))
                return SupportedFileType.GZIP;

            #endregion

            #region IniFile

            // No magic checks for IniFile

            #endregion

            #region InstallShieldArchiveV3

            if (magic.StartsWith(new byte?[] { 0x13, 0x5D, 0x65, 0x8C }))
                return SupportedFileType.InstallShieldArchiveV3;

            #endregion

            #region InstallShieldCAB

            if (magic.StartsWith(new byte?[] { 0x49, 0x53, 0x63 }))
                return SupportedFileType.InstallShieldCAB;

            #endregion

            #region LDSCRYPT

            if (magic.StartsWith(new byte?[] { 0x4C, 0x44, 0x53, 0x43, 0x52, 0x59, 0x50, 0x54 }))
                return SupportedFileType.LDSCRYPT;

            #endregion

            #region MicrosoftCAB

            if (magic.StartsWith(new byte?[] { 0x4d, 0x53, 0x43, 0x46 }))
                return SupportedFileType.MicrosoftCAB;

            #endregion

            #region MicrosoftLZ

            if (magic.StartsWith(new byte?[] { 0x53, 0x5a, 0x44, 0x44, 0x88, 0xf0, 0x27, 0x33 }))
                return SupportedFileType.MicrosoftLZ;

            #endregion

            #region MPQ

            if (magic.StartsWith(new byte?[] { 0x4d, 0x50, 0x51, 0x1a }))
                return SupportedFileType.MPQ;

            if (magic.StartsWith(new byte?[] { 0x4d, 0x50, 0x51, 0x1b }))
                return SupportedFileType.MPQ;

            #endregion

            #region N3DS

            // No magic checks for N3DS

            #endregion

            #region NCF

            if (magic.StartsWith(new byte?[] { 0x01, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00 }))
                return SupportedFileType.NCF;

            #endregion

            #region Nitro

            // No magic checks for Nitro

            #endregion

            #region PAK

            if (magic.StartsWith(new byte?[] { 0x50, 0x41, 0x43, 0x4B }))
                return SupportedFileType.PAK;

            #endregion

            #region PFF

            // Version 2
            if (magic.StartsWith(new byte?[] { 0x14, 0x00, 0x00, 0x00, 0x50, 0x46, 0x46, 0x32 }))
                return SupportedFileType.PFF;

            // Version 3
            if (magic.StartsWith(new byte?[] { 0x14, 0x00, 0x00, 0x00, 0x50, 0x46, 0x46, 0x33 }))
                return SupportedFileType.PFF;

            // Version 4
            if (magic.StartsWith(new byte?[] { 0x14, 0x00, 0x00, 0x00, 0x50, 0x46, 0x46, 0x34 }))
                return SupportedFileType.PFF;

            #endregion

            #region PKZIP

            // PKZIP (Unknown)
            if (magic.StartsWith(new byte?[] { 0x50, 0x4b, 0x00, 0x00 }))
                return SupportedFileType.PKZIP;

            // PKZIP
            if (magic.StartsWith(new byte?[] { 0x50, 0x4b, 0x03, 0x04 }))
                return SupportedFileType.PKZIP;

            // PKZIP (Empty Archive)
            if (magic.StartsWith(new byte?[] { 0x50, 0x4b, 0x05, 0x06 }))
                return SupportedFileType.PKZIP;

            // PKZIP (Spanned Archive)
            if (magic.StartsWith(new byte?[] { 0x50, 0x4b, 0x07, 0x08 }))
                return SupportedFileType.PKZIP;

            #endregion

            #region PLJ

            // https://www.iana.org/assignments/media-types/audio/vnd.everad.plj
            if (magic.StartsWith(new byte?[] { 0xFF, 0x9D, 0x53, 0x4B }))
                return SupportedFileType.PLJ;

            #endregion

            #region Quantum

            if (magic.StartsWith(new byte?[] { 0x44, 0x53 }))
                return SupportedFileType.Quantum;

            #endregion

            #region RAR

            // RAR archive version 1.50 onwards
            if (magic.StartsWith(new byte?[] { 0x52, 0x61, 0x72, 0x21, 0x1a, 0x07, 0x00 }))
                return SupportedFileType.RAR;

            // RAR archive version 5.0 onwards
            if (magic.StartsWith(new byte?[] { 0x52, 0x61, 0x72, 0x21, 0x1a, 0x07, 0x01, 0x00 }))
                return SupportedFileType.RAR;

            #endregion

            #region SevenZip

            if (magic.StartsWith(new byte?[] { 0x37, 0x7a, 0xbc, 0xaf, 0x27, 0x1c }))
                return SupportedFileType.SevenZip;

            #endregion

            #region SFFS

            // Found in Redump entry 81756, confirmed to be "StarForce Filesystem" by PiD.
            if (magic.StartsWith(new byte?[] { 0x53, 0x46, 0x46, 0x53 }))
                return SupportedFileType.SFFS;

            #endregion 

            #region SGA

            if (magic.StartsWith(new byte?[] { 0x5F, 0x41, 0x52, 0x43, 0x48, 0x49, 0x56, 0x45 }))
                return SupportedFileType.SGA;

            #endregion

            #region TapeArchive

            if (magic.StartsWith(new byte?[] { 0x75, 0x73, 0x74, 0x61, 0x72, 0x00, 0x30, 0x30 }))
                return SupportedFileType.TapeArchive;

            if (magic.StartsWith(new byte?[] { 0x75, 0x73, 0x74, 0x61, 0x72, 0x20, 0x20, 0x00 }))
                return SupportedFileType.TapeArchive;

            #endregion

            #region Textfile

            // Not all textfiles can be determined through magic number

            // HTML
            if (magic.StartsWith(new byte?[] { 0x3c, 0x68, 0x74, 0x6d, 0x6c }))
                return SupportedFileType.Textfile;

            // HTML and XML
            if (magic.StartsWith(new byte?[] { 0x3c, 0x21, 0x44, 0x4f, 0x43, 0x54, 0x59, 0x50, 0x45 }))
                return SupportedFileType.Textfile;

            // InstallShield Compiled Rules
            if (magic.StartsWith(new byte?[] { 0x61, 0x4C, 0x75, 0x5A }))
                return SupportedFileType.Textfile;

            // Microsoft Office File (old)
            if (magic.StartsWith(new byte?[] { 0xd0, 0xcf, 0x11, 0xe0, 0xa1, 0xb1, 0x1a, 0xe1 }))
                return SupportedFileType.Textfile;

            // Rich Text File
            if (magic.StartsWith(new byte?[] { 0x7b, 0x5c, 0x72, 0x74, 0x66, 0x31 }))
                return SupportedFileType.Textfile;

            // Windows Help File
            if (magic.StartsWith(new byte?[] { 0x3F, 0x5F, 0x03, 0x00 }))
                return SupportedFileType.Textfile;

            #endregion

            #region VBSP

            if (magic.StartsWith(new byte?[] { 0x56, 0x42, 0x53, 0x50 }))
                return SupportedFileType.VBSP;

            #endregion

            #region VPK

            if (magic.StartsWith(new byte?[] { 0x34, 0x12, 0xaa, 0x55 }))
                return SupportedFileType.VPK;

            #endregion

            #region WAD

            if (magic.StartsWith(new byte?[] { 0x57, 0x41, 0x44, 0x33 }))
                return SupportedFileType.WAD;

            #endregion

            #region XZ

            if (magic.StartsWith(new byte?[] { 0xfd, 0x37, 0x7a, 0x58, 0x5a, 0x00 }))
                return SupportedFileType.XZ;

            #endregion

            #region XZP

            if (magic.StartsWith(new byte?[] { 0x70, 0x69, 0x5A, 0x78 }))
                return SupportedFileType.XZP;

            #endregion

            // We couldn't find a supported match
            return SupportedFileType.UNKNOWN;
        }

        /// <summary>
        /// Get the supported file type for an extension
        /// </summary>
        /// <remarks>This is less accurate than a magic string match</remarks>
        public static SupportedFileType GetFileType(string extension)
        {
            // If we have an invalid extension
            if (string.IsNullOrWhiteSpace(extension))
                return SupportedFileType.UNKNOWN;

            // Normalize the extension
            extension = extension.TrimStart('.').Trim();

            #region AACSMediaKeyBlock

            // Shares an extension with INF setup information so it can't be used accurately
            // Blu-ray
            // if (extension.Equals("inf", StringComparison.OrdinalIgnoreCase))
            //     return SupportedFileType.AACSMediaKeyBlock;

            // HD-DVD
            if (extension.Equals("aacs", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.AACSMediaKeyBlock;

            #endregion

            #region BDPlusSVM

            if (extension.Equals(value: "svm", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.BDPlusSVM;

            #endregion

            #region BFPK

            // No extensions registered for BFPK

            #endregion

            #region BSP

            // Shares an extension with VBSP so it can't be used accurately
            // if (extension.Equals("bsp", StringComparison.OrdinalIgnoreCase))
            //     return SupportedFileType.BSP;

            #endregion

            #region BZip2

            if (extension.Equals("bz2", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.BZip2;

            #endregion

            #region CFB

            // Installer package
            if (extension.Equals("msi", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.CFB;

            // Merge module
            else if (extension.Equals("msm", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.CFB;

            // Patch Package
            else if (extension.Equals("msp", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.CFB;

            // Transform
            else if (extension.Equals("mst", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.CFB;

            // Patch Creation Properties
            else if (extension.Equals("pcp", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.CFB;

            #endregion

            #region CIA

            if (extension.Equals("cia", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.CIA;

            #endregion

            #region Executable

            // DOS MZ executable file format (and descendants)
            if (extension.Equals("exe", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.Executable;

            // DOS MZ library file format (and descendants)
            if (extension.Equals("dll", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.Executable;

            #endregion

            #region GCF

            if (extension.Equals("gcf", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.GCF;

            #endregion

            #region GZIP

            if (extension.Equals("gz", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.GZIP;

            #endregion

            #region IniFile

            if (extension.Equals("ini", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.IniFile;

            #endregion

            #region InstallShieldArchiveV3

            if (extension.Equals("z", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.InstallShieldArchiveV3;

            #endregion

            #region InstallShieldCAB

            // No extensions registered for InstallShieldCAB
            // Both InstallShieldCAB and MicrosoftCAB share the same extension

            #endregion

            #region MicrosoftCAB

            // No extensions registered for InstallShieldCAB
            // Both InstallShieldCAB and MicrosoftCAB share the same extension

            #endregion

            #region MPQ

            if (extension.Equals("mpq", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.MPQ;

            #endregion

            #region N3DS

            // 3DS cart image
            if (extension.Equals("3ds", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.N3DS;

            // CIA package -- Not currently supported
            // else if (extension.Equals(value: "cia", StringComparison.OrdinalIgnoreCase))
            //     return SupportedFileType.N3DS;

            #endregion

            #region NCF

            if (extension.Equals("ncf", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.NCF;

            #endregion

            #region Nitro

            // DS cart image
            if (extension.Equals("nds", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.Nitro;

            // DS development cart image
            else if (extension.Equals("srl", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.Nitro;

            // DSi cart image
            else if (extension.Equals("dsi", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.Nitro;

            // iQue DS cart image
            else if (extension.Equals("ids", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.Nitro;

            #endregion

            #region PAK

            // No extensions registered for PAK
            // Both PAK and Quantum share one extension
            // if (extension.Equals("pak", StringComparison.OrdinalIgnoreCase))
            //     return SupportedFileType.PAK;

            #endregion

            #region PFF

            if (extension.Equals("pff", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.PFF;

            #endregion

            #region PKZIP

            // PKZIP
            if (extension.Equals("zip", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.PKZIP;

            // Android package
            if (extension.Equals("apk", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.PKZIP;

            // Java archive
            if (extension.Equals("jar", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.PKZIP;

            // Google Earth saved working session file
            if (extension.Equals("kmz", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.PKZIP;

            // KWord document
            if (extension.Equals("kwd", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.PKZIP;

            // Microsoft Office Open XML Format (OOXML) Document
            if (extension.Equals("docx", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.PKZIP;

            // Microsoft Office Open XML Format (OOXML) Presentation
            if (extension.Equals("pptx", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.PKZIP;

            // Microsoft Office Open XML Format (OOXML) Spreadsheet
            if (extension.Equals("xlsx", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.PKZIP;

            // OpenDocument text document
            if (extension.Equals("odt", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.PKZIP;

            // OpenDocument presentation
            if (extension.Equals("odp", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.PKZIP;

            // OpenDocument text document template
            if (extension.Equals("ott", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.PKZIP;

            // Microsoft Open XML paper specification file
            if (extension.Equals("oxps", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.PKZIP;

            // OpenOffice spreadsheet
            if (extension.Equals("sxc", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.PKZIP;

            // OpenOffice drawing
            if (extension.Equals("sxd", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.PKZIP;

            // OpenOffice presentation
            if (extension.Equals("sxi", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.PKZIP;

            // OpenOffice word processing
            if (extension.Equals("sxw", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.PKZIP;

            // StarOffice spreadsheet
            if (extension.Equals("sxc", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.PKZIP;

            // Windows Media compressed skin file
            if (extension.Equals("wmz", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.PKZIP;

            // Mozilla Browser Archive
            if (extension.Equals("xpi", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.PKZIP;

            // XML paper specification file
            if (extension.Equals("xps", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.PKZIP;

            // eXact Packager Models
            if (extension.Equals("xpt", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.PKZIP;

            #endregion

            #region PLJ

            // https://www.iana.org/assignments/media-types/audio/vnd.everad.plj
            if (extension.Equals("plj", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.PLJ;

            #endregion

            #region Quantum

            if (extension.Equals("q", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.Quantum;

            // Both PAK and Quantum share one extension
            // if (extension.Equals("pak", StringComparison.OrdinalIgnoreCase))
            //     return SupportedFileType.Quantum;

            #endregion

            #region RAR

            if (extension.Equals("rar", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.RAR;

            #endregion

            #region SevenZip

            if (extension.Equals("7z", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.SevenZip;

            #endregion

            #region SGA

            if (extension.Equals("sga", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.SGA;

            #endregion

            #region TapeArchive

            if (extension.Equals("tar", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.SevenZip;

            #endregion

            #region Textfile

            // "Description in Zip"
            if (extension.Equals("diz", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.Textfile;

            // Generic textfile (no header)
            if (extension.Equals("txt", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.Textfile;

            // HTML
            if (extension.Equals("htm", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.Textfile;
            if (extension.Equals("html", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.Textfile;

            // InstallShield Script
            if (extension.Equals("ins", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.Textfile;

            // Microsoft Office File (old)
            if (extension.Equals("doc", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.Textfile;

            // Rich Text File
            if (extension.Equals("rtf", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.Textfile;

            // Setup information
            if (extension.Equals("inf", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.Textfile;

            // Windows Help File
            if (extension.Equals("hlp", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.Textfile;

            // XML
            if (extension.Equals("xml", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.Textfile;

            #endregion

            #region VBSP

            // Shares an extension with BSP so it can't be used accurately
            // if (extension.Equals("bsp", StringComparison.OrdinalIgnoreCase))
            //     return SupportedFileType.VBSP;

            #endregion

            #region VPK

            // Common extension so this cannot be used accurately
            // if (extension.Equals("vpk", StringComparison.OrdinalIgnoreCase))
            //     return SupportedFileType.VPK;

            #endregion

            #region WAD

            // Common extension so this cannot be used accurately
            // if (extension.Equals("wad", StringComparison.OrdinalIgnoreCase))
            //     return SupportedFileType.WAD;

            #endregion

            #region XZ

            if (extension.Equals("xz", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.XZ;

            #endregion

            #region XZP

            if (extension.Equals("xzp", StringComparison.OrdinalIgnoreCase))
                return SupportedFileType.XZP;

            #endregion

            // We couldn't find a supported match
            return SupportedFileType.UNKNOWN;
        }

        /// <summary>
        /// Create an instance of a scannable based on file type
        /// </summary>
        public static IScannable CreateScannable(SupportedFileType fileType)
        {
            switch (fileType)
            {
                case SupportedFileType.AACSMediaKeyBlock: return new FileType.AACSMediaKeyBlock();
                case SupportedFileType.BDPlusSVM: return new FileType.BDPlusSVM();
                case SupportedFileType.BFPK: return new FileType.BFPK();
                case SupportedFileType.BSP: return new FileType.BSP();
                case SupportedFileType.BZip2: return new FileType.BZip2();
                case SupportedFileType.CFB: return new FileType.CFB();
                //case SupportedFileType.CIA: return new FileType.CIA();
                case SupportedFileType.Executable: return new FileType.Executable();
                case SupportedFileType.GCF: return new FileType.GCF();
                case SupportedFileType.GZIP: return new FileType.GZIP();
                //case FileTypes.IniFile: return new FileType.IniFile();
                case SupportedFileType.InstallShieldArchiveV3: return new FileType.InstallShieldArchiveV3();
                case SupportedFileType.InstallShieldCAB: return new FileType.InstallShieldCAB();
                case SupportedFileType.LDSCRYPT: return new FileType.LDSCRYPT();
                case SupportedFileType.MicrosoftCAB: return new FileType.MicrosoftCAB();
                case SupportedFileType.MicrosoftLZ: return new FileType.MicrosoftLZ();
                case SupportedFileType.MPQ: return new FileType.MPQ();
                //case SupportedFileType.N3DS: return new FileType.N3DS();
                //case SupportedFileType.NCF: return new FileType.NCF();
                //case SupportedFileType.Nitro: return new FileType.Nitro();
                case SupportedFileType.PAK: return new FileType.PAK();
                case SupportedFileType.PFF: return new FileType.PFF();
                case SupportedFileType.PKZIP: return new FileType.PKZIP();
                case SupportedFileType.PLJ: return new FileType.PLJ();
                //case SupportedFileType.Quantum: return new FileType.Quantum();
                case SupportedFileType.RAR: return new FileType.RAR();
                case SupportedFileType.SevenZip: return new FileType.SevenZip();
                case SupportedFileType.SFFS: return new FileType.SFFS();
                case SupportedFileType.SGA: return new FileType.SGA();
                case SupportedFileType.TapeArchive: return new FileType.TapeArchive();
                case SupportedFileType.Textfile: return new FileType.Textfile();
                case SupportedFileType.VBSP: return new FileType.VBSP();
                case SupportedFileType.VPK: return new FileType.VPK();
                case SupportedFileType.WAD: return new FileType.WAD();
                case SupportedFileType.XZ: return new FileType.XZ();
                case SupportedFileType.XZP: return new FileType.XZP();
                default: return null;
            }
        }

        /// <summary>
        /// Create an instance of a wrapper based on file type
        /// </summary>
        public static WrapperBase CreateWrapper(SupportedFileType fileType, Stream data)
        {
            switch (fileType)
            {
                case SupportedFileType.AACSMediaKeyBlock: return AACSMediaKeyBlock.Create(data);
                case SupportedFileType.BDPlusSVM: return BDPlusSVM.Create(data);
                case SupportedFileType.BFPK: return BFPK.Create(data);
                case SupportedFileType.BSP: return BSP.Create(data);
                //case SupportedFileType.BZip2: return BZip2.Create(data);
                case SupportedFileType.CFB: return CFB.Create(data);
                case SupportedFileType.CIA: return CIA.Create(data);
                case SupportedFileType.Executable: return DetermineExecutableType(data);
                case SupportedFileType.GCF: return GCF.Create(data);
                //case SupportedFileType.GZIP: return GZIP.Create(data);
                //case SupportedFileType.IniFile: return IniFile.Create(data);
                //case SupportedFileType.InstallShieldArchiveV3: return InstallShieldArchiveV3.Create(data);
                case SupportedFileType.InstallShieldCAB: return InstallShieldCabinet.Create(data);
                //case SupportedFileType.LDSCRYPT: return LDSCRYPT.Create(data);
                case SupportedFileType.MicrosoftCAB: return MicrosoftCabinet.Create(data);
                //case SupportedFileType.MicrosoftLZ: return MicrosoftLZ.Create(data);
                //case SupportedFileType.MPQ: return MoPaQ.Create(data);
                case SupportedFileType.N3DS: return N3DS.Create(data);
                case SupportedFileType.NCF: return NCF.Create(data);
                case SupportedFileType.Nitro: return Nitro.Create(data);
                case SupportedFileType.PAK: return PAK.Create(data);
                case SupportedFileType.PFF: return PFF.Create(data);
                //case SupportedFileType.PKZIP: return PKZIP.Create(data);
                case SupportedFileType.PLJ: return PlayJAudioFile.Create(data);
                case SupportedFileType.Quantum: return Quantum.Create(data);
                //case SupportedFileType.RAR: return RAR.Create(data);
                //case SupportedFileType.SevenZip: return SevenZip.Create(data);
                //case SupportedFileType.SFFS: return SFFS.Create(data);
                case SupportedFileType.SGA: return SGA.Create(data);
                //case SupportedFileType.TapeArchive: return TapeArchive.Create(data);
                //case SupportedFileType.Textfile: return Textfile.Create(data);
                case SupportedFileType.VBSP: return VBSP.Create(data);
                case SupportedFileType.VPK: return VPK.Create(data);
                case SupportedFileType.WAD: return WAD.Create(data);
                //case SupportedFileType.XZ: return XZ.Create(data);
                case SupportedFileType.XZP: return XZP.Create(data);
                default: return null;
            }
        }

        /// <summary>
        /// Determine the executable type from the stream
        /// </summary>
        /// <param name="stream">Stream data to parse</param>
        /// <returns>WrapperBase representing the executable, null on error</returns>
        public static WrapperBase DetermineExecutableType(Stream stream)
        {
            // Try to get an MS-DOS wrapper first
            WrapperBase wrapper = MSDOS.Create(stream);
            if (wrapper == null)
                return null;

            // Check for a valid new executable address
            if ((wrapper as MSDOS).NewExeHeaderAddr >= stream.Length)
                return wrapper;

            // Try to read the executable info
            stream.Seek((wrapper as MSDOS).NewExeHeaderAddr, SeekOrigin.Begin);
            byte[] magic = stream.ReadBytes(4);

            // New Executable
            if (magic.StartsWith(Models.NewExecutable.Constants.SignatureBytes))
            {
                stream.Seek(0, SeekOrigin.Begin);
                return NewExecutable.Create(stream);
            }

            // Linear Executable
            else if (magic.StartsWith(Models.LinearExecutable.Constants.LESignatureBytes)
                || magic.StartsWith(Models.LinearExecutable.Constants.LXSignatureBytes))
            {
                stream.Seek(0, SeekOrigin.Begin);
                return LinearExecutable.Create(stream);
            }

            // Portable Executable
            else if (magic.StartsWith(Models.PortableExecutable.Constants.SignatureBytes))
            {
                stream.Seek(0, SeekOrigin.Begin);
                return PortableExecutable.Create(stream);
            }

            // Everything else fails
            return null;
        }

        #endregion

        #region Processed Executable Information

        /// <summary>
        /// Get the internal version as reported by the filesystem
        /// </summary>
        /// <param name="file">File to check for version</param>
        /// <returns>Version string, null on error</returns>
        public static string GetInternalVersion(string file)
        {
            try
            {
                using (Stream fileStream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var pex = PortableExecutable.Create(fileStream);
                    return GetInternalVersion(pex);
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Get the internal version as reported by the resources
        /// </summary>
        /// <param name="pex">PortableExecutable representing the file contents</param>
        /// <returns>Version string, null on error</returns>
        public static string GetInternalVersion(PortableExecutable pex)
        {
            string version = pex.FileVersion;
            if (!string.IsNullOrWhiteSpace(version))
                return version.Replace(", ", ".");

            version = pex.ProductVersion;
            if (!string.IsNullOrWhiteSpace(version))
                return version.Replace(", ", ".");

            version = pex.AssemblyVersion;
            if (!string.IsNullOrWhiteSpace(version))
                return version;

            return null;
        }

        #endregion

        #region Wrappers for Matchers

        /// <summary>
        /// Wrapper for GetInternalVersion for use in path matching
        /// </summary>
        /// <param name="firstMatchedString">File to check for version</param>
        /// <param name="files">Full list of input paths</param>
        /// <returns>Version string, null on error</returns>
        public static string GetInternalVersion(string firstMatchedString, IEnumerable<string> files) => GetInternalVersion(firstMatchedString);

        #endregion
    }
}
