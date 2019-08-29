using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.Models.ViewModel
{
    public class DoctorsViewModel
    {
        public Doctors Doctors { get; set; }
        [Display(Name = "Специјалност")]
        public IEnumerable<Specialties> Specialties { get; set; }
        [Display(Name = "Субспецијалност")]
        public IEnumerable<SubSpecialties> SubSpecialties { get; set; }

    }
}
