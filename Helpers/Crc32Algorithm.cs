namespace Force.Crc32
{
    /// <summary>
    /// Implementation of CRC-32.
    /// This class supports several convenient static methods returning the CRC as UInt32.
    /// </summary>
    public class Crc32Algorithm
    {
        private const uint DefaultPolynomial = 0xedb88320u;
        private const uint DefaultSeed = 0xffffffffu;

        private static readonly uint[] DefaultTable;

        static Crc32Algorithm()
        {
            DefaultTable = InitializeTable(DefaultPolynomial);
        }

        /// <summary>
        /// Computes CRC-32 from stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The CRC-32 value.</returns>
        public static uint Compute(Stream stream)
        {
            return Compute(DefaultSeed, stream);
        }

        /// <summary>
        /// Computes CRC-32 from stream.
        /// </summary>
        /// <param name="seed">The initial seed value.</param>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The CRC-32 value.</returns>
        public static uint Compute(uint seed, Stream stream)
        {
            uint crc = seed;
            byte[] buffer = new byte[4096];
            int count;
            while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                crc = Append(crc, buffer, 0, count);
            }
            return ~crc;
        }

        /// <summary>
        /// Appends CRC-32 data.
        /// </summary>
        /// <param name="initial">The initial CRC value.</param>
        /// <param name="data">The data to process.</param>
        /// <param name="offset">The offset of first byte in data.</param>
        /// <param name="length">The number of bytes to process.</param>
        /// <returns>The updated CRC value.</returns>
        public static uint Append(uint initial, byte[] data, int offset, int length)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (offset < 0 || length < 0 || offset + length > data.Length)
                throw new ArgumentOutOfRangeException();

            uint crc = initial;
            for (int i = offset; i < offset + length; i++)
            {
                crc = (crc >> 8) ^ DefaultTable[(crc & 0xFF) ^ data[i]];
            }
            return crc;
        }

        private static uint[] InitializeTable(uint polynomial)
        {
            uint[] table = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                uint entry = (uint)i;
                for (int j = 0; j < 8; j++)
                {
                    if ((entry & 1) == 1)
                        entry = (entry >> 1) ^ polynomial;
                    else
                        entry = entry >> 1;
                }
                table[i] = entry;
            }
            return table;
        }
    }
}