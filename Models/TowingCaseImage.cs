using System.ComponentModel.DataAnnotations;

namespace Proffessional.Models
{
    public class TowingCaseImage
    {
        [Key]
        public int ImageId { get; set; }

        [Required]
        public string CaseId { get; set; } = null!;

        [Required]
        public string SectionType { get; set; } = null!;
        // "Before" or "After"

        [Required]
        public string FileName { get; set; } = null!;

        [Required]
        public string ContentType { get; set; } = null!;

        [Required]
        public byte[] ImageData { get; set; } = null!;

        public DateTime UploadedAt { get; set; } = DateTime.Now;

        public TowingCase Case { get; set; }
    }
}
