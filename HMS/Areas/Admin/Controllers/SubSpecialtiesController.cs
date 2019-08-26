using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMS.Data;
using HMS.Models;
using HMS.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HMS.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.SuperAdminEndUser)]
    [Area("Admin")]
    public class SubSpecialtiesController : Controller
    {

        private readonly ApplicationDbContext _db;

        public SubSpecialtiesController( ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View(_db.SubSpecialties.ToList());
        }

        //GET Create Action Method
        public IActionResult Create()
        {
            return View();
        }

        //POST Create action Method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubSpecialties subSpecialties)
        {
            if(ModelState.IsValid)
            {
                _db.Add(subSpecialties);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(subSpecialties);
        }


        //GET Edit Action Method
        public async Task<IActionResult> Edit(int? id)
        {
            if(id==null)
            {
                return NotFound();
            }

            var specialTags = await _db.SubSpecialties.FindAsync(id);
            if (specialTags == null)
            {
                return NotFound();
            }

            return View(specialTags);
        }

        //POST Edit action Method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SubSpecialties subSpecialties)
        {
            if(id!= subSpecialties.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _db.Update(subSpecialties);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(subSpecialties);
        }

        //GET Details Action Method
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var specialTags = await _db.SubSpecialties.FindAsync(id);
            if (specialTags == null)
            {
                return NotFound();
            }

            return View(specialTags);
        }


        //GET Delete Action Method
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var specialTags = await _db.SubSpecialties.FindAsync(id);
            if (specialTags == null)
            {
                return NotFound();
            }

            return View(specialTags);
        }

        //POST Delete action Method
        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var specialTags = await _db.SubSpecialties.FindAsync(id);
            _db.SubSpecialties.Remove(specialTags);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}