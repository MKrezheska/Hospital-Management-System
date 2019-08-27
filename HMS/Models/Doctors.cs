using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.Models
{
    public class Doctors
    {
        public int Id { get; set; }
        [Display(Name = "Име")]
        public string Name { get; set; }
        [Display(Name = "Тел. бр")]
        public double PhoneNumber { get; set; }

        public bool Available { get; set; }
        [Display(Name = "Слика")]
        public string Image { get; set; }

        public string Focus{ get; set; }

        [Display(Name= "Специјалност")]
        public int SpecialityId { get; set; }
        [Display(Name = "Специјалност")]

        [ForeignKey("SpecialityId")]
        public virtual Specialties Specialties { get; set; }

        [Display(Name = "Субспецијалност")]
        public int SubSpecialtiesId { get; set; }
        [Display(Name = "Субспецијалност")]

        [ForeignKey("SubSpecialtiesId")]
        public virtual SubSpecialties SubSpecialties { get; set; }
    }
}
