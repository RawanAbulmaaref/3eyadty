using System.Numerics;
using System.ComponentModel.DataAnnotations;

namespace Mvc.Models.Clinic
{
   
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } 

        [Required]
        [MaxLength(50)]
        public string Password { get; set; } 

        [Required]
        [MaxLength(50)]
        
        public string Email { get; set; } 

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = null!;

            
            public Doctor? Doctor { get; set; }

           
            public Patient? Patient { get; set; }
        }
    }

