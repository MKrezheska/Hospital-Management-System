using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.Models.ViewModel
{
    public class DoctorsViewModel
    {
        public Doctors Doctors { get; set; }
        public IEnumerable<Specialties> Specialties { get; set; }
        public IEnumerable<SubSpecialties> SubSpecialties { get; set; }

    }
}
