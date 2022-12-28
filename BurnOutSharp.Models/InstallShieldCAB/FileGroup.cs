namespace BurnOutSharp.Models.InstallShieldCabinet
{
    /// <see href="https://github.com/twogood/unshield/blob/main/lib/libunshield.h"/>
    public sealed class FileGroup
    {
        public uint NameOffset;

        public string Name;

        public uint FirstFile;

        public uint LastFile;
    }
}