using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.Models.ViewModel
{
    public class AppointmentDetailsViewModel
    {

        public Appointments Appointment { get; set; }
        public List<ApplicationUser> DoctorUser { get; set; }
        public List<Doctors> Doctors { get; set; }

    }
}
