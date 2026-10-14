using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Domain.Entities
{
    [Table("ApiLogs")]
    public class ApiLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string? APIMethod { get; set; }
        public string? APIURL { get; set; }
        public string? RequestBody { get; set; }
        public string? ResponseBody { get; set; }
        public int? StatusCode { get; set; }
        public long? DurationMs { get; set; }
        public string? ClientIP { get; set; }
        public string? UserAgent { get; set; }
        // Audit
        public long? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}