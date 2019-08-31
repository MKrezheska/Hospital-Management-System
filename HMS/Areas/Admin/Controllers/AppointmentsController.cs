using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using HMS.Data;
using HMS.Models;
using HMS.Models.ViewModel;
using HMS.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using NPOI.XWPF.UserModel;
using Rotativa.AspNetCore;
using NPOI.XWPF.UserModel;


namespace HMS.Areas.Admin.Controllers
{
    [Authorize(Roles =SD.AdminEndUser + "," + SD.SuperAdminEndUser)]
    [Area("Admin")]

    public class AppointmentsController : Controller
    {

        private readonly ApplicationDbContext _db;
        private int PageSize = 10;

        IConfiguration Configuration;

        private static string homePath = (Environment.OSVersion.Platform == PlatformID.Unix ||
               Environment.OSVersion.Platform == PlatformID.MacOSX)
             ? Environment.GetEnvironmentVariable("HOME")
             : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

        public AppointmentsController(ApplicationDbContext db, IConfiguration configuration)
        {
            _db = db;
            Configuration = configuration;
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


        public async Task<IActionResult> AppointmentCSV(int doctorPage = 1, string searchName = null, string searchEmail = null, string searchPhone = null, string searchDate = null)
        {

            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            AppointmentViewModel appointmentVM = new AppointmentViewModel()
            {
                Appointments = new List<Models.Appointments>()
            };

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

            using (var writer = new StreamWriter(homePath + Configuration["DownloadsRelativePath"]))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(appointmentVM.Appointments);
            }

            return RedirectToAction("Index", "Appointments");
        }


        public IActionResult AppointmentExcel()
        {

            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            AppointmentViewModel appointmentVM = new AppointmentViewModel()
            {
                Appointments = new List<Models.Appointments>()
            };

            appointmentVM.Appointments = _db.Appointments.Include(a => a.DoctorUser).ToList();

            var newFile = homePath + "\\Downloads\\" + "AppointmentExcel.xlsx";

            using (var fs = new FileStream(newFile, FileMode.Create, FileAccess.Write))
            {

                IWorkbook workbook = new XSSFWorkbook();

                ISheet sheet1 = workbook.CreateSheet("Sheet1");

                var style1 = workbook.CreateCellStyle();
                style1.FillForegroundColor = HSSFColor.LightGreen.Index;
                style1.FillPattern = FillPattern.SolidForeground;


                for (int i = 0; i < appointmentVM.Appointments.Count; i++)
                {
                    if (appointmentVM.Appointments.ElementAt(i).DoctorUser is null)
                    {
                        appointmentVM.Appointments.RemoveAt(i);
                    }
                }
                
                for (int i = 0; i < appointmentVM.Appointments.Count; i++)
                {
                  
                    if (i == 0)
                    {
                        sheet1.CreateRow(i);
                        for (int j = 0; j < 6; j++)
                        {
                            sheet1.GetRow(i).CreateCell(j);
                            sheet1.GetRow(i).GetCell(j).CellStyle = style1;
                        }

                        sheet1.GetRow(i).GetCell(0).SetCellValue("Доктор");
                        sheet1.GetRow(i).GetCell(1).SetCellValue("Термин датум");
                        sheet1.GetRow(i).GetCell(2).SetCellValue("Пациент");
                        sheet1.GetRow(i).GetCell(3).SetCellValue("Пациент тел.");
                        sheet1.GetRow(i).GetCell(4).SetCellValue("Пациент емаил");
                        sheet1.GetRow(i).GetCell(5).SetCellValue("Потврден");
                       
                    }

                    sheet1.CreateRow(i + 1);

                    sheet1.GetRow(i + 1).CreateCell(0);
                    sheet1.GetRow(i + 1).CreateCell(1);
                    sheet1.GetRow(i + 1).CreateCell(2);
                    sheet1.GetRow(i + 1).CreateCell(3);
                    sheet1.GetRow(i + 1).CreateCell(4);
                    sheet1.GetRow(i + 1).CreateCell(5);


                    sheet1.GetRow(i + 1).GetCell(0).SetCellValue(appointmentVM.Appointments.ElementAt(i).DoctorUser.Name);
                    sheet1.GetRow(i + 1).GetCell(1).SetCellValue(appointmentVM.Appointments.ElementAt(i).AppointmentDate.ToString());
                    sheet1.GetRow(i + 1).GetCell(2).SetCellValue(appointmentVM.Appointments.ElementAt(i).PatientName);
                    sheet1.GetRow(i + 1).GetCell(3).SetCellValue(appointmentVM.Appointments.ElementAt(i).PatientPhoneNumber);
                    sheet1.GetRow(i + 1).GetCell(4).SetCellValue(appointmentVM.Appointments.ElementAt(i).PatientEmail);
                    sheet1.GetRow(i + 1).GetCell(5).SetCellValue(appointmentVM.Appointments.ElementAt(i).isConfirmed.ToString());

                }

                workbook.Write(fs);
            }


            return RedirectToAction("Index", "Appointments");
        }

        public IActionResult AppointmentWord()
        {

            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            AppointmentViewModel appointmentVM = new AppointmentViewModel()
            {
                Appointments = new List<Models.Appointments>()
            };

            appointmentVM.Appointments = _db.Appointments.Include(a => a.DoctorUser).ToList();
          
            var newFile = homePath + "\\Downloads\\" + "AppointmentWord.docx";
            
            XWPFDocument doc = new XWPFDocument();
            XWPFParagraph para = doc.CreateParagraph();
            XWPFRun r0 = para.CreateRun();

            r0.SetText("Термини датум: " + DateTime.Now.ToString("MM/dd/yyyy HH:mm"));
            r0.FontSize = 20;
            r0.SetColor("5A5A5A");
            para.Alignment = ParagraphAlignment.CENTER;

            para.BorderTop = Borders.Thick;
            para.FillBackgroundColor = "90EE90";
            
            for (int i = 0; i < appointmentVM.Appointments.Count; i++)
            {
                if (appointmentVM.Appointments.ElementAt(i).DoctorUser is null)
                {
                    appointmentVM.Appointments.RemoveAt(i);
                }
            }

            XWPFTable table = doc.CreateTable(appointmentVM.Appointments.Count + 1, 6);

            for (int i = 0; i < appointmentVM.Appointments.Count; i++)
                {
                if (i == 0) {

                    for (int j = 0; j < 6; j++)
                    {
                        table.GetRow(i).GetCell(j).SetColor("6CC3D5");
                    }

                    table.GetRow(i).GetCell(0).SetText("Доктор");
                    table.GetRow(i).GetCell(1).SetText("Термин датум");
                    table.GetRow(i).GetCell(2).SetText("Пациент");
                    table.GetRow(i).GetCell(3).SetText("Пациент тел.");
                    table.GetRow(i).GetCell(4).SetText("Пациент емаил");
                    table.GetRow(i).GetCell(5).SetText("Потврден");
                }
                    table.GetRow(i + 1).GetCell(0).SetText(appointmentVM.Appointments.ElementAt(i).DoctorUser.Name);
                    table.GetRow(i + 1).GetCell(1).SetText(appointmentVM.Appointments.ElementAt(i).AppointmentDate.ToString());
                    table.GetRow(i + 1).GetCell(2).SetText(appointmentVM.Appointments.ElementAt(i).PatientName);
                    table.GetRow(i + 1).GetCell(3).SetText(appointmentVM.Appointments.ElementAt(i).PatientPhoneNumber);
                    table.GetRow(i + 1).GetCell(4).SetText(appointmentVM.Appointments.ElementAt(i).PatientEmail);
                    table.GetRow(i + 1).GetCell(5).SetText(appointmentVM.Appointments.ElementAt(i).isConfirmed.ToString());
            
                }

                FileStream out1 = new FileStream(newFile, FileMode.Create);
                doc.Write(out1);
        
            return RedirectToAction("Index", "Appointments");
        }
    }
}
