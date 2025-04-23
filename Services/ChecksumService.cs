using System.Security.Cryptography;
using System.Text;
using Force.Crc32;

namespace SakuraDB_Mini.Services
{
    public class ChecksumService
    {
        public (string MD5, string CRC32, string SHA1) CalculateChecksums(string filePath)
        {
            string md5Hash1 = string.Empty;
            string md5Hash2 = string.Empty;
            string crc32Hash1 = string.Empty;
            string crc32Hash2 = string.Empty;
            string sha1Hash1 = string.Empty;
            string sha1Hash2 = string.Empty;

            // Calculate hashes twice and ensure they match to ensure file integrity
            do
            {
                md5Hash1 = CalculateMD5(filePath);
                crc32Hash1 = CalculateCRC32(filePath);
                sha1Hash1 = CalculateSHA1(filePath);

                md5Hash2 = CalculateMD5(filePath);
                crc32Hash2 = CalculateCRC32(filePath);
                sha1Hash2 = CalculateSHA1(filePath);
            }
            while (md5Hash1 != md5Hash2 || crc32Hash1 != crc32Hash2 || sha1Hash1 != sha1Hash2);

            return (md5Hash1, crc32Hash1, sha1Hash1);
        }

        private string CalculateMD5(string filePath)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filePath);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private string CalculateCRC32(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            uint crc = Crc32Algorithm.Compute(stream);
            return crc.ToString("X8"); // Return CRC32 as 8-character hex string
        }

        private string CalculateSHA1(string filePath)
        {
            using var sha1 = SHA1.Create();
            using var stream = File.OpenRead(filePath);
            var hash = sha1.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}