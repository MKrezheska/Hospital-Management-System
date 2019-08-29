using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using HMS.Data;
using HMS.Models;
using HMS.Models.ViewModel;
using HMS.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;


namespace HMS.Areas.Admin.Controllers
{
    [Authorize(Roles =SD.AdminEndUser + "," + SD.SuperAdminEndUser)]
    [Area("Admin")]

    public class AppointmentsController : Controller
    {

        private readonly ApplicationDbContext _db;
        private int PageSize = 3;

        public AppointmentsController(ApplicationDbContext db)
        {
            _db = db;
        }


        public async Task<IActionResult> Index(int doctorPage=1, string searchName=null, string searchEmail =null, string searchPhone=null, string searchDate = null)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            AppointmentViewModel appointmentVM = new AppointmentViewModel()
            {
                Appointments = new List<Models.Appointments>()
            };

            StringBuilder param = new StringBuilder();

            param.Append("/Admin/Appointments?doctorPage=:");
            param.Append("&searchName=");
            if(searchName!=null)
            {
                param.Append(searchName);
            }
            param.Append("&searchEmail=");
            if (searchEmail != null)
            {
                param.Append(searchEmail);
            }
            param.Append("&searchPhone=");
            if (searchPhone != null)
            {
                param.Append(searchPhone);
            }
            param.Append("&searchDate=");
            if (searchDate != null)
            {
                param.Append(searchDate);
            }




            appointmentVM.Appointments = _db.Appointments.Include(a => a.DoctorUser).ToList();
            if(User.IsInRole(SD.AdminEndUser))
            {
                appointmentVM.Appointments = appointmentVM.Appointments.Where(a => a.DoctorId == claim.Value).ToList();
            }


            if (searchName != null)
            {
                appointmentVM.Appointments = appointmentVM.Appointments.Where(a => a.PatientName.ToLower().Contains(searchName.ToLower())).ToList();
            }
            if (searchEmail != null)
            {
                appointmentVM.Appointments = appointmentVM.Appointments.Where(a => a.PatientEmail.ToLower().Contains(searchEmail.ToLower())).ToList();
            }
            if (searchPhone != null)
            {
                appointmentVM.Appointments = appointmentVM.Appointments.Where(a => a.PatientPhoneNumber.ToLower().Contains(searchPhone.ToLower())).ToList();
            }
            if (searchDate != null)
            {
                try
                {
                    DateTime appDate = Convert.ToDateTime(searchDate);
                    appointmentVM.Appointments = appointmentVM.Appointments.Where(a => a.AppointmentDate.ToShortDateString().Equals(appDate.ToShortDateString())).ToList();
                }
                catch(Exception ex)
                {

                }
                
            }

            var count = appointmentVM.Appointments.Count;

            appointmentVM.Appointments = appointmentVM.Appointments.OrderBy(p => p.AppointmentDate)
                .Skip((doctorPage - 1) * PageSize)
                .Take(PageSize).ToList();


            appointmentVM.PagingInfo = new PagingInfo
            {
                CurrentPage = doctorPage,
                ItemsPerPage = PageSize,
                TotalItems = count,
                urlParam = param.ToString()
            };


            return View(appointmentVM);
        }

        //GET Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if(id==null)
            {
                return NotFound();
            }

            var doctorsList = (IEnumerable<Doctors>)(from p in _db.Doctors
                                                   join a in _db.DoctorsSelectedForAppointment
                                                   on p.Id equals a.DoctorId
                                                   where a.AppointmentId == id
                                                   select p).Include("Specialties");

            AppointmentDetailsViewModel objAppointmentVM = new AppointmentDetailsViewModel()
            {
                Appointment = _db.Appointments.Include(a => a.DoctorUser).Where(a => a.Id == id).FirstOrDefault(),
                DoctorUser = _db.ApplicationUser.ToList(),
                Doctors = doctorsList.ToList()
            };

            return View(objAppointmentVM);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppointmentDetailsViewModel objAppointmentVM)
        {
            if(ModelState.IsValid)
            {
                objAppointmentVM.Appointment.AppointmentDate = objAppointmentVM.Appointment.AppointmentDate
                                    .AddHours(objAppointmentVM.Appointment.AppointmentTime.Hour)
                                    .AddMinutes(objAppointmentVM.Appointment.AppointmentTime.Minute);

                var appointmentFromDb = _db.Appointments.Where(a => a.Id == objAppointmentVM.Appointment.Id).FirstOrDefault();

                appointmentFromDb.PatientName = objAppointmentVM.Appointment.PatientName;
                appointmentFromDb.PatientEmail = objAppointmentVM.Appointment.PatientEmail;
                appointmentFromDb.PatientPhoneNumber = objAppointmentVM.Appointment.PatientPhoneNumber;
                appointmentFromDb.AppointmentDate = objAppointmentVM.Appointment.AppointmentDate;
                appointmentFromDb.isConfirmed = objAppointmentVM.Appointment.isConfirmed;
                if(User.IsInRole(SD.SuperAdminEndUser))
                {
                    appointmentFromDb.DoctorId = objAppointmentVM.Appointment.DoctorId;
                }
                _db.SaveChanges();

                return RedirectToAction(nameof(Index));


            }

            return View(objAppointmentVM);
        }


        //GET Detials
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctorsList = (IEnumerable<Doctors>)(from p in _db.Doctors
                                                      join a in _db.DoctorsSelectedForAppointment
                                                      on p.Id equals a.DoctorId
                                                      where a.AppointmentId == id
                                                      select p).Include("Specialties");

            AppointmentDetailsViewModel objAppointmentVM = new AppointmentDetailsViewModel()
            {
                Appointment = _db.Appointments.Include(a => a.DoctorUser).Where(a => a.Id == id).FirstOrDefault(),
                DoctorUser = _db.ApplicationUser.ToList(),
                Doctors = doctorsList.ToList()
            };

            return View(objAppointmentVM);

        }


        //GET Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctorsList = (IEnumerable<Doctors>)(from p in _db.Doctors
                                                     join a in _db.DoctorsSelectedForAppointment
                                                      on p.Id equals a.DoctorId
                                                      where a.AppointmentId == id
                                                      select p).Include("Specialties");

            AppointmentDetailsViewModel objAppointmentVM = new AppointmentDetailsViewModel()
            {
                Appointment = _db.Appointments.Include(a => a.DoctorUser).Where(a => a.Id == id).FirstOrDefault(),
                DoctorUser = _db.ApplicationUser.ToList(),
                Doctors = doctorsList.ToList()
            };

            return View(objAppointmentVM);

        }


        //POST Delete
        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed (int id)
        {
            var appointment = await _db.Appointments.FindAsync(id);
            _db.Appointments.Remove(appointment);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public IActionResult AppointmentPDF(int doctorPage = 1, string searchName = null, string searchEmail = null, string searchPhone = null, string searchDate = null)
        {
          
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            AppointmentViewModel appointmentVM = new AppointmentViewModel()
            {
                Appointments = new List<Models.Appointments>()
            };

            StringBuilder param = new StringBuilder();

            param.Append("/Admin/Appointments?doctorPage=:");
            param.Append("&searchName=");
            if (searchName != null)
            {
                param.Append(searchName);
            }
            param.Append("&searchEmail=");
            if (searchEmail != null)
            {
                param.Append(searchEmail);
            }
            param.Append("&searchPhone=");
            if (searchPhone != null)
            {
                param.Append(searchPhone);
            }
            param.Append("&searchDate=");
            if (searchDate != null)
            {
                param.Append(searchDate);
            }




            appointmentVM.Appointments = _db.Appointments.Include(a => a.DoctorUser).ToList();
            if (User.IsInRole(SD.AdminEndUser))
            {
                appointmentVM.Appointments = appointmentVM.Appointments.Where(a => a.DoctorId == claim.Value).ToList();
            }


            if (searchName != null)
            {
                appointmentVM.Appointments = appointmentVM.Appointments.Where(a => a.PatientName.ToLower().Contains(searchName.ToLower())).ToList();
            }
            if (searchEmail != null)
            {
                appointmentVM.Appointments = appointmentVM.Appointments.Where(a => a.PatientEmail.ToLower().Contains(searchEmail.ToLower())).ToList();
            }
            if (searchPhone != null)
            {
                appointmentVM.Appointments = appointmentVM.Appointments.Where(a => a.PatientPhoneNumber.ToLower().Contains(searchPhone.ToLower())).ToList();
            }
            if (searchDate != null)
            {
                try
                {
                    DateTime appDate = Convert.ToDateTime(searchDate);
                    appointmentVM.Appointments = appointmentVM.Appointments.Where(a => a.AppointmentDate.ToShortDateString().Equals(appDate.ToShortDateString())).ToList();
                }
                catch (Exception ex)
                {

                }

            }

            var count = appointmentVM.Appointments.Count;

            appointmentVM.Appointments = appointmentVM.Appointments.OrderBy(p => p.AppointmentDate)
                .Skip((doctorPage - 1) * PageSize)
                .Take(PageSize).ToList();


            appointmentVM.PagingInfo = new PagingInfo
            {
                CurrentPage = doctorPage,
                ItemsPerPage = PageSize,
                TotalItems = count,
                urlParam = param.ToString()
            };


            return new ViewAsPdf(appointmentVM);
        }

    }
}