using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mvc.Models.Clinic
{

    
    public class Patient
    {
        [Key]
        public int PatientId { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string Gender { get; set; }

       
        public User User { get; set; } 

        public List<Appointment> Appointments { get; set; } = new List<Appointment>();
            
    }
}
