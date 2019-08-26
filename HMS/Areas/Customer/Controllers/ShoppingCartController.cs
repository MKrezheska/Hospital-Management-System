using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMS.Data;
using HMS.Extensions;
using HMS.Models;
using HMS.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HMS.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext _db;

        [BindProperty]
        public ShoppingCartViewModel ShoppingCartVM { get; set; }

        public ShoppingCartController( ApplicationDbContext db)
        {
            _db = db;
            ShoppingCartVM = new ShoppingCartViewModel()
            {
                Doctors = new List<Models.Doctors>()
            };
        }

        //Get Index Shopping Cart
        public async  Task<IActionResult> Index()
        {
            List<int> lstShoppingCart = HttpContext.Session.Get<List<int>>("ssShoppingCart");
            if(lstShoppingCart.Count>0)
            {
                foreach(int cartItem in lstShoppingCart)
                {
                    Doctors doctors = _db.Doctors.Include(p => p.SubSpecialties).Include(p => p.Specialties).Where(p => p.Id == cartItem).FirstOrDefault();
                    ShoppingCartVM.Doctors.Add(doctors);
                }
            }
            return View(ShoppingCartVM);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public IActionResult IndexPost()
        {
            List<int> lstCartItems = HttpContext.Session.Get<List<int>>("ssShoppingCart");

            ShoppingCartVM.Appointments.AppointmentDate = ShoppingCartVM.Appointments.AppointmentDate
                                                            .AddHours(ShoppingCartVM.Appointments.AppointmentTime.Hour)
                                                            .AddMinutes(ShoppingCartVM.Appointments.AppointmentTime.Minute);

            Appointments appointments = ShoppingCartVM.Appointments;
            _db.Appointments.Add(appointments);
            _db.SaveChanges();

            int appointmentId = appointments.Id;

            foreach(int productId in lstCartItems)
            {
                DoctorsSelectedForAppointment productsSelectedForAppointment = new DoctorsSelectedForAppointment()
                {
                    AppointmentId = appointmentId,
                    DoctorId = productId
                };
                _db.DoctorsSelectedForAppointment.Add(productsSelectedForAppointment);
                
            }
            _db.SaveChanges();
            lstCartItems = new List<int>();
            HttpContext.Session.Set("ssShoppingCart", lstCartItems);

            return RedirectToAction("AppointmentConfirmation","ShoppingCart", new { Id = appointmentId});

        }

        public IActionResult Remove(int id)
        {
            List<int> lstCartItems = HttpContext.Session.Get<List<int>>("ssShoppingCart");

            if(lstCartItems.Count>0)
            {
                if(lstCartItems.Contains(id))
                {
                    lstCartItems.Remove(id);
                }
            }

            HttpContext.Session.Set("ssShoppingCart", lstCartItems);

            return RedirectToAction(nameof(Index));
        }


        //Get
        public IActionResult AppointmentConfirmation(int id)
        {
            ShoppingCartVM.Appointments = _db.Appointments.Where(a => a.Id == id).FirstOrDefault();
            List<DoctorsSelectedForAppointment> objProdList = _db.DoctorsSelectedForAppointment.Where(p => p.AppointmentId == id).ToList();

            foreach(DoctorsSelectedForAppointment prodAptObj in objProdList)
            {
                ShoppingCartVM.Doctors.Add(_db.Doctors.Include(p => p.Specialties).Include(p => p.SubSpecialties).Where(p => p.Id == prodAptObj.DoctorId).FirstOrDefault());
            }

            return View(ShoppingCartVM);
        }

    }
}