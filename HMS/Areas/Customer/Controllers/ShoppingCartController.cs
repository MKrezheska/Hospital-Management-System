using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMS.Data;
using HMS.Extensions;
using HMS.Models;
using HMS.Models.ViewModel;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Utils;

namespace HMS.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IHostingEnvironment _env;
        private readonly IConfigurationRoot _congif;

        [BindProperty]
        public ShoppingCartViewModel ShoppingCartVM { get; set; }

        public ShoppingCartController( ApplicationDbContext db, IHostingEnvironment env, IConfiguration config)
        {
            _db = db;
            ShoppingCartVM = new ShoppingCartViewModel()
            {
                Doctors = new List<Models.Doctors>()
            };

            _env = env;
            _congif = new ConfigurationBuilder()
                            .SetBasePath(env.ContentRootPath)
                            .AddJsonFile("configurationSecrets.json")
                            .Build(); ;
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
        public IActionResult IndexPost(string PatientName, string PatientEmail, string datepicker, string AppointmentTime)
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


            //Sending Email 
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(_congif["EmailConfig:Name"], _congif["EmailConfig:Email"]));

            message.To.Add(new MailboxAddress(PatientName,PatientEmail));

            message.Subject = "Закажан термин : " + datepicker + " " + AppointmentTime;

            //message.Body = new TextPart("plain")
            //{
            //    Text = "Вашито термин е закажан. Датум : " + DateTime.Now.ToString("MM/dd/yyyy HH:mm") + "Со почит Hospital Management System"
            //};

            var builder = new BodyBuilder();

            // Set the plain-text version of the message text
            

            // In order to reference selfie.jpg from the html text, we'll need to add it
            // to builder.LinkedResources and then use its Content-Id value in the img src.

            var path = _env.WebRootFileProvider.GetFileInfo("images/logo.jpg")?.PhysicalPath;
            var image = builder.LinkedResources.Add(path);
            image.ContentId = MimeUtils.GenerateMessageId();
            
            //Set the html version of the message text
            builder.HtmlBody = string.Format(
                @"<center><img src=""cid:{0}"" style=""width:100px;""></center>
                <p>{1}, вашиот термин е закажан.</p>
                <p>Датум: {2}</p>
                <p>Време: {3}</p>
                <p>Со почит, <br>
                Hospital Management System </p>
                ", image.ContentId, PatientName, datepicker, AppointmentTime);

            // We may also want to attach a calendar event for Monica's party...
            //builder.Attachments.Add(@"C:\Users\Joey\Documents\party.ics");

            // Now we just need to set the message body and we're done
            message.Body = builder.ToMessageBody();




            //Configure client

            using (var client = new SmtpClient()) {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                client.Connect(_congif["EmailConfig:Host"], Convert.ToInt32(_congif["EmailConfig:Port"]), false);

                client.Authenticate(_congif["EmailConfig:UsernameAuth"], _congif["EmailConfig:PasswordAuth"]);

                client.Send(message);

                client.Disconnect(true);
            }


            //
            return RedirectToAction("AppointmentConfirmation", "ShoppingCart", new { Id = appointmentId });

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