using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.Models.ViewModel
{
    public class ShoppingCartViewModel
    {
        public List<Doctors> Doctors { get; set; }
        public Appointments Appointments { get; set; }
    }
}
