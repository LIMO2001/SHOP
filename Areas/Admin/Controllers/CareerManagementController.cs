using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LaptopStore.Services;
using LaptopStore.Models;

namespace LaptopStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CareerManagementController : Controller
    {
        private readonly ICareerService _careerService;

        public CareerManagementController(ICareerService careerService)
        {
            _careerService = careerService;
        }

        public async Task<IActionResult> Index()
        {
            var careers = await _careerService.GetAllCareersAsync();
            return View(careers);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Career career)
        {
            if (ModelState.IsValid)
            {
                await _careerService.CreateCareerAsync(career);
                TempData["Success"] = "Career opportunity created successfully!";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Failed to create career. Please check your input.";
            return View(career);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var career = await _careerService.GetCareerByIdAsync(id);
            if (career == null)
            {
                return NotFound();
            }
            return View(career);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Career career)
        {
            if (id != career.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _careerService.UpdateCareerAsync(career);
                TempData["Success"] = "Career opportunity updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Update failed. Please check your input.";
            return View(career);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var career = await _careerService.GetCareerByIdAsync(id);
            if (career == null)
            {
                return NotFound();
            }
            return View(career);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _careerService.DeleteCareerAsync(id);
            TempData["Success"] = "Career opportunity deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
