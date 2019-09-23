using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HMS.Models;
using HMS.Data;
using Microsoft.EntityFrameworkCore;
using HMS.Extensions;
using HMS.Models.ViewModel;

namespace HMS.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {

        private readonly ApplicationDbContext _db;
        [BindProperty]
        public HomeViewModel Hvm { get; set; }
        public HomeController(ApplicationDbContext db)
        {
            _db = db;
            Hvm = new HomeViewModel()
            {
                Doctors = _db.Doctors.ToList(),
                Specialties = _db.Specialties.ToList(),
            };

        }

        public async Task<IActionResult> Index()
        {
            Hvm = new HomeViewModel()
            {
                Doctors = _db.Doctors.ToList(),
                Specialties = _db.Specialties.ToList(),
            };


            return View(Hvm);


        }

       

        public async Task<IActionResult> Details(int id)
        {
            var product = await _db.Doctors.Include(m => m.Specialties).Include(m => m.SubSpecialties).Where(m=>m.Id==id).FirstOrDefaultAsync();


            return View(product);
        }

        [HttpPost,ActionName("Details")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DetailsPost(int id)
        {
            List<int> lstShoppingCart = HttpContext.Session.Get<List<int>>("ssShoppingCart");
            if(lstShoppingCart==null)
            {
                lstShoppingCart = new List<int>();
            }
            lstShoppingCart.Add(id);
            HttpContext.Session.Set("ssShoppingCart", lstShoppingCart);

            return RedirectToAction("Index", "Home", new { area = "Customer" });

        }

        public IActionResult Remove(int id)
        {
            List<int> lstShoppingCart = HttpContext.Session.Get<List<int>>("ssShoppingCart");
            if(lstShoppingCart.Count>0)
            {
                if(lstShoppingCart.Contains(id))
                {
                    lstShoppingCart.Remove(id);
                }
            }

            HttpContext.Session.Set("ssShoppingCart", lstShoppingCart);

            return RedirectToAction(nameof(Index));
        }


        public IActionResult Support() {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
