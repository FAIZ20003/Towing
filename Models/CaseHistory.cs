using System.ComponentModel.DataAnnotations;

namespace Proffessional.Models
{
    public class CaseHistory
    {
        [Key]
        public int HistoryId { get; set; }

        public string CaseId { get; set; } = string.Empty;

        public string ActionType { get; set; } = string.Empty;

        public string? OldValue { get; set; }      // ✅ MUST be nullable
        public string? NewValue { get; set; }      // ✅ MUST be nullable
        public string? ChangedBy { get; set; }     // ✅ MUST be nullable

        public DateTime ChangedAt { get; set; }    // ✅ NOT NULL in DB
    }

}