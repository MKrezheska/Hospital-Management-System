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

        [Display(Name ="Име на доктор")]
        public string DoctorId { get; set; }

        [ForeignKey("DoctorId")]
        public virtual ApplicationUser DoctorUser { get; set; }

        public DateTime AppointmentDate { get; set; }

        [NotMapped]
        public DateTime AppointmentTime { get; set; }

        public string PatientName { get; set; }
        public string PatientPhoneNumber { get; set; }
        public string PatientEmail { get; set; }
        public bool isConfirmed { get; set; }

    }
}
