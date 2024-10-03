﻿#if NET40_OR_GREATER || NETCOREAPP
using System.Collections.Concurrent;
#endif
using System.Collections.Generic;
using BinaryObjectScanner.Interfaces;
using SabreTools.Matching;
using SabreTools.Matching.Paths;

namespace BinaryObjectScanner.Protection
{
    /// <summary>
    /// CD-Protector is a form of DRM that allows users to create their own copy protected discs.
    /// It prevents copying via "Phantom Trax", intended to confuse dumping software, and by obfuscating a specified EXE.
    /// The official website seems to be https://web.archive.org/web/20000302173822/http://surf.to/nrgcrew.
    /// The author's site should be https://members.xoom.it/_XOOM/Dudez/index.htm, but no captures of this site appear to be functional.
    /// Instructions on how this software can be used: https://3dnews.ru/166065
    /// Download: https://www.cdmediaworld.com/hardware/cdrom/cd_utils_3.shtml#CD-Protector
    /// TODO: See if any of the older versions of CD-Protector are archived, and check if they need to be detected differently.
    /// </summary>
    public class CDProtector : IPathCheck
    {
        /// <inheritdoc/>
#if NET20 || NET35
        public Queue<string> CheckDirectoryPath(string path, IEnumerable<string>? files)
#else
        public ConcurrentQueue<string> CheckDirectoryPath(string path, IEnumerable<string>? files)
#endif
        {
            var matchers = new List<PathMatchSet>
            {
                // These are the main files used by CD-Protector, which should all be present in every protected disc.
                // "_cdp16.dll" and "_cdp32.dll" are actually renamed WAV files.
                // "_cdp32.dat" is actually an archive that contains the original executable.
                // Another EXE is created, with the name of the original executable. I'm not sure what this executable does, but it appears to be compressed with NeoLite.
                // TODO: Invesitage if this EXE itself can be detected in any way.
                new(new FilePathMatch("_cdp16.dat"), "CD-Protector"),
                new(new FilePathMatch("_cdp16.dll"), "CD-Protector"),
                new(new FilePathMatch("_cdp32.dat"), "CD-Protector"),
                new(new FilePathMatch("_cdp32.dll"), "CD-Protector"),

                // This is the "Phantom Trax" file generated by CD-Protector, intended to be burned to a protected CD as an audio track.
                new(new FilePathMatch("Track#1 - Track#2 Cd-Protector.wav"), "CD-Protector"),
            };

            return MatchUtil.GetAllMatches(files, matchers, any: true);
        }

        /// <inheritdoc/>
        public string? CheckFilePath(string path)
        {
            var matchers = new List<PathMatchSet>
            {
                // These are the main files used by CD-Protector, which should all be present in every protected disc.
                // "_cdp16.dll" and "_cdp32.dll" are actually renamed WAV files.
                // "_cdp32.dat" is actually an archive that contains the original executable.
                // Another EXE is created, with the name of the original executable. I'm not sure what this executable does, but it appears to be compressed with NeoLite.
                // TODO: Invesitage if this EXE itself can be detected in any way.
                new(new FilePathMatch("_cdp16.dat"), "CD-Protector"),
                new(new FilePathMatch("_cdp16.dll"), "CD-Protector"),
                new(new FilePathMatch("_cdp32.dat"), "CD-Protector"),
                new(new FilePathMatch("_cdp32.dll"), "CD-Protector"),

                // This is the "Phantom Trax" file generated by CD-Protector, intended to be burned to a protected CD as an audio track.
                new(new FilePathMatch("Track#1 - Track#2 Cd-Protector.wav"), "CD-Protector"),
            };

            return MatchUtil.GetFirstMatch(path, matchers, any: true);
        }
    }
}
