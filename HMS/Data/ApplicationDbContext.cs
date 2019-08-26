using System;
using System.Collections.Generic;
using System.Text;
using HMS.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HMS.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Specialties> Specialties { get; set; }
        public DbSet<SubSpecialties> SubSpecialties { get; set; }
        public DbSet<Doctors> Doctors { get; set; }

        public DbSet<Appointments> Appointments { get; set; }
        public DbSet<DoctorsSelectedForAppointment> DoctorsSelectedForAppointment { get; set; }

        public DbSet<ApplicationUser> ApplicationUser { get; set; }
    }
}
