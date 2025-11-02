using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LaptopStore.Services;

namespace LaptopStore.Controllers
{
    public class CareerController : Controller
    {
        private readonly ICareerService _careerService;

        public CareerController(ICareerService careerService)
        {
            _careerService = careerService;
        }

        public async Task<IActionResult> Index()
        {
            var careers = await _careerService.GetActiveCareersAsync();
            ViewBag.Departments = await _careerService.GetDepartmentsAsync();
            return View(careers);
        }

        public async Task<IActionResult> Details(int id)
        {
            var career = await _careerService.GetCareerByIdAsync(id);
            if (career == null || !career.IsActive)
            {
                return NotFound();
            }
            return View(career);
        }
    }
}