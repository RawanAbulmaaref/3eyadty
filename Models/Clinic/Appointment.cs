using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mvc.Models.Clinic
{
    
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [Required]
        [ForeignKey("Patient")]
        public int PatientId { get; set; }

        [Required]
        [ForeignKey("Doctor")]
        public int DoctorId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public TimeSpan Time { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } 

        public Patient Patient { get; set; } 

        
        public Doctor Doctor { get; set; } 

        public MedicalRecord? MedicalRecord { get; set; }
    }
}
