using System.ComponentModel.DataAnnotations;

namespace SakuraDB_Mini.Models
{
    public class ProcessedFileInfo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public long FileSize { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [MaxLength(32)]
        public string MD5 { get; set; } = string.Empty;

        [Required]
        [MaxLength(8)]
        public string CRC32 { get; set; } = string.Empty;

        [Required]
        [MaxLength(40)]
        public string SHA1 { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Source { get; set; } = string.Empty;

        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}