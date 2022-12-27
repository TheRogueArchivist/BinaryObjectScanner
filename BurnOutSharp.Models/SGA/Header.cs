namespace BurnOutSharp.Models.SGA
{
    /// <see href="https://github.com/RavuAlHemio/hllib/blob/master/HLLib/SGAFile.h"/>
    public class Header
    {
        public string Signature;

        public ushort MajorVersion;

        public ushort MinorVersion;
    }
}