using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.Models
{
    public class Appointments
    {
        public int Id { get; set; }

        public string DoctorId { get; set; }

       
        [ForeignKey("DoctorId")]
        public virtual ApplicationUser DoctorUser { get; set; }

        [Display(Name = "Термин датум")]
        public DateTime AppointmentDate { get; set; }

        [NotMapped]
        public DateTime AppointmentTime { get; set; }

        [Display(Name ="Пациент")]
        public string PatientName { get; set; }

        [Display(Name = "Пациент тел.")]
        public string PatientPhoneNumber { get; set; }

        [Display(Name = "Пациент емаил")]
        public string PatientEmail { get; set; }

        [Display(Name = "Потврден")]
        public bool isConfirmed { get; set; }

    }
}
