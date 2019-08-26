using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.Models
{
    public class SubSpecialties
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
