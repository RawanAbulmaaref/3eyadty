using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Mvc.Models.Clinic
{
  

    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Specialization { get; set; } 

        [Required]
        [MaxLength(20)]
        public string Status { get; set; }

        [Required]
        [MaxLength(500)]
        public string CertificatePath { get; set; }

        [MaxLength(500)]
        public string? RejectionReason { get; set; }


        public User User { get; set; } = null!;

        public List<Appointment> Appointments { get; set; }=new List<Appointment>();
           
    }
}
