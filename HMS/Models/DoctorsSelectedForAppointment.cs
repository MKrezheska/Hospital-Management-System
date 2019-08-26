using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.Models
{
    public class DoctorsSelectedForAppointment
    {
        public int Id { get; set; }

        public int AppointmentId { get; set; }

        [ForeignKey("AppointmentId")]
        public virtual Appointments Appointments { get; set; }

        public int DoctorId { get; set; }

        [ForeignKey("DoctorId")]
        public virtual Doctors Doctors { get; set; }
    }
}
