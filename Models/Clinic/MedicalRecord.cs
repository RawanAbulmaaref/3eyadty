using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Mvc.Models.Clinic
{
    

    public class MedicalRecord
    {
        [Key]
        public int RecordId { get; set; }

        [Required]
        [ForeignKey("Appointment")]
        public int AppointmentId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [MaxLength(500)]
        public string Diagnosis { get; set; }

        [MaxLength(500)]
        public string? Prescription { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        public Appointment Appointment { get; set; } 
    }
}
