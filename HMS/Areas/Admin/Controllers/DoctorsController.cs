using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HMS.Data;
using HMS.Models;
using HMS.Models.ViewModel;
using HMS.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HMS.Controllers
{
    [Authorize(Roles = SD.SuperAdminEndUser)]

    [Area("Admin")]
    public class DoctorsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly HostingEnvironment _hostingEnvironment;

        [BindProperty]
        public DoctorsViewModel DoctorsVM { get; set; }


        public DoctorsController(ApplicationDbContext db, HostingEnvironment hostingEnvironment)
        {
            _db = db;
            _hostingEnvironment = hostingEnvironment;
            DoctorsVM = new DoctorsViewModel()
            {
                Specialties = _db.Specialties.ToList(),
                SubSpecialties = _db.SubSpecialties.ToList(),
                Doctors = new Models.Doctors()
            };

        }


        public async Task<IActionResult> Index()
        {
            var products = _db.Doctors.Include(m => m.Specialties).Include(m => m.SubSpecialties);
            return View(await products.ToListAsync());
        }

        //Get : Products Create
        public IActionResult Create()
        {
            return View(DoctorsVM);
        }

        //Post : Products Create
        [HttpPost,ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePOST()
        {
            if (!ModelState.IsValid)
            {
                return View(DoctorsVM);
            }

            _db.Doctors.Add(DoctorsVM.Doctors);
            await _db.SaveChangesAsync();

            //Image being saved

            string webRootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var productsFromDb = _db.Doctors.Find(DoctorsVM.Doctors.Id);

            if(files.Count!=0)
            {
                //Image has been uploaded
                var uploads = Path.Combine(webRootPath, SD.ImageFolder);
                var extension = Path.GetExtension(files[0].FileName);

                using (var filestream = new FileStream(Path.Combine(uploads, DoctorsVM.Doctors.Id + extension), FileMode.Create))
                {
                    files[0].CopyTo(filestream);
                }
                productsFromDb.Image = @"\" + SD.ImageFolder + @"\" + DoctorsVM.Doctors.Id + extension;
            }
            else
            {
                //when user does not upload image
                var uploads = Path.Combine(webRootPath, SD.ImageFolder + @"\" + SD.DefaultProductImage);
                System.IO.File.Copy(uploads, webRootPath + @"\" + SD.ImageFolder + @"\" + DoctorsVM.Doctors.Id + ".png");
                productsFromDb.Image = @"\" + SD.ImageFolder + @"\" + DoctorsVM.Doctors.Id + ".png";
            }
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }



        //GET : Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if(id==null)
            {
                return NotFound();
            }

            DoctorsVM.Doctors = await _db.Doctors.Include(m => m.SubSpecialties).Include(m => m.Specialties).SingleOrDefaultAsync(m => m.Id == id);

            if(DoctorsVM.Doctors==null)
            {
                return NotFound();
            }

            return View(DoctorsVM);
        }


        //Post : Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            if(ModelState.IsValid)
            {
                string webRootPath = _hostingEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;

                var productFromDb = _db.Doctors.Where(m => m.Id == DoctorsVM.Doctors.Id).FirstOrDefault();

                if(files.Count>0 && files[0]!=null)
                {
                    //if user uploads a new image
                    var uploads = Path.Combine(webRootPath, SD.ImageFolder);
                    var extension_new = Path.GetExtension(files[0].FileName);
                    var extension_old = Path.GetExtension(productFromDb.Image);

                    if(System.IO.File.Exists(Path.Combine(uploads, DoctorsVM.Doctors.Id+extension_old)))
                    {
                        System.IO.File.Delete(Path.Combine(uploads, DoctorsVM.Doctors.Id + extension_old));
                    }
                    using (var filestream = new FileStream(Path.Combine(uploads, DoctorsVM.Doctors.Id + extension_new), FileMode.Create))
                    {
                        files[0].CopyTo(filestream);
                    }
                    DoctorsVM.Doctors.Image = @"\" + SD.ImageFolder + @"\" + DoctorsVM.Doctors.Id + extension_new;
                }

                if(DoctorsVM.Doctors.Image !=null)
                {
                    productFromDb.Image = DoctorsVM.Doctors.Image;
                }

                productFromDb.Name = DoctorsVM.Doctors.Name;
                productFromDb.PhoneNumber = DoctorsVM.Doctors.PhoneNumber;
                productFromDb.Available = DoctorsVM.Doctors.Available;
                productFromDb.SpecialityId = DoctorsVM.Doctors.SpecialityId;
                productFromDb.SubSpecialtiesId = DoctorsVM.Doctors.SubSpecialtiesId;
                productFromDb.Focus = DoctorsVM.Doctors.Focus;
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(DoctorsVM);
        }


        //GET : Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DoctorsVM.Doctors = await _db.Doctors.Include(m => m.SubSpecialties).Include(m => m.Specialties).SingleOrDefaultAsync(m => m.Id == id);

            if (DoctorsVM.Doctors == null)
            {
                return NotFound();
            }

            return View(DoctorsVM);
        }

        //GET : Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DoctorsVM.Doctors = await _db.Doctors.Include(m => m.SubSpecialties).Include(m => m.Specialties).SingleOrDefaultAsync(m => m.Id == id);

            if (DoctorsVM.Doctors == null)
            {
                return NotFound();
            }

            return View(DoctorsVM);
        }

        //POST : Delete
        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            string webRootPath = _hostingEnvironment.WebRootPath;
            Doctors products = await _db.Doctors.FindAsync(id);

            if(products==null)
            {
                return NotFound();
            }
            else
            {
                var uploads = Path.Combine(webRootPath, SD.ImageFolder);
                var extension = Path.GetExtension(products.Image);

                if(System.IO.File.Exists(Path.Combine(uploads,products.Id+extension)))
                {
                    System.IO.File.Delete(Path.Combine(uploads, products.Id + extension));
                }
                _db.Doctors.Remove(products);
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
        }

    }
}