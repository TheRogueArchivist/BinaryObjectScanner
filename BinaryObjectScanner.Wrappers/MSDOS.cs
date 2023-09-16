﻿using System.IO;
using System.Text;

namespace BinaryObjectScanner.Wrappers
{
    public class MSDOS : WrapperBase<SabreTools.Models.MSDOS.Executable>
    {
        #region Descriptive Properties

        /// <inheritdoc/>
        public override string DescriptionString => "MS-DOS Executable";

        #endregion

        #region Constructors

        /// <inheritdoc/>
#if NET48
        public MSDOS(SabreTools.Models.MSDOS.Executable model, byte[] data, int offset)
#else
        public MSDOS(SabreTools.Models.MSDOS.Executable? model, byte[]? data, int offset)
#endif
            : base(model, data, offset)
        {
            // All logic is handled by the base class
        }

        /// <inheritdoc/>
#if NET48
        public MSDOS(SabreTools.Models.MSDOS.Executable model, Stream data)
#else
        public MSDOS(SabreTools.Models.MSDOS.Executable? model, Stream? data)
#endif
            : base(model, data)
        {
            // All logic is handled by the base class
        }/// <summary>
         /// Create an MS-DOS executable from a byte array and offset
         /// </summary>
         /// <param name="data">Byte array representing the executable</param>
         /// <param name="offset">Offset within the array to parse</param>
         /// <returns>An MS-DOS executable wrapper on success, null on failure</returns>
#if NET48
        public static MSDOS Create(byte[] data, int offset)
#else
        public static MSDOS? Create(byte[]? data, int offset)
#endif
        {
            // If the data is invalid
            if (data == null)
                return null;

            // If the offset is out of bounds
            if (offset < 0 || offset >= data.Length)
                return null;

            // Create a memory stream and use that
            MemoryStream dataStream = new MemoryStream(data, offset, data.Length - offset);
            return Create(dataStream);
        }

        /// <summary>
        /// Create an MS-DOS executable from a Stream
        /// </summary>
        /// <param name="data">Stream representing the executable</param>
        /// <returns>An MS-DOS executable wrapper on success, null on failure</returns>
#if NET48
        public static MSDOS Create(Stream data)
#else
        public static MSDOS? Create(Stream? data)
#endif
        {
            // If the data is invalid
            if (data == null || data.Length == 0 || !data.CanSeek || !data.CanRead)
                return null;

            var executable = new SabreTools.Serialization.Streams.MSDOS().Deserialize(data);
            if (executable == null)
                return null;

            try
            {
                return new MSDOS(executable, data);
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Printing

        /// <inheritdoc/>
        public override StringBuilder PrettyPrint()
        {
            StringBuilder builder = new StringBuilder();
            Printing.MSDOS.Print(builder, this.Model);
            return builder;
        }

        #endregion
    }
}