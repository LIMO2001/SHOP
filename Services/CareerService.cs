using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LaptopStore.Data;
using LaptopStore.Models;

namespace LaptopStore.Services
{
    public interface ICareerService
    {
        Task<List<Career>> GetActiveCareersAsync();
        Task<List<Career>> GetAllCareersAsync();
        Task<Career> GetCareerByIdAsync(int id);
        Task<Career> CreateCareerAsync(Career career);
        Task<Career> UpdateCareerAsync(Career career);
        Task DeleteCareerAsync(int id);
        Task<List<string>> GetDepartmentsAsync();
    }

    public class CareerService : ICareerService
    {
        private readonly ApplicationDbContext _context;

        public CareerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Career>> GetActiveCareersAsync()
        {
            return await _context.Careers
                .Where(c => c.IsActive && c.ApplicationDeadline >= DateTime.Today)
                .OrderByDescending(c => c.DatePosted)
                .ToListAsync();
        }

        public async Task<List<Career>> GetAllCareersAsync()
        {
            return await _context.Careers
                .OrderByDescending(c => c.DatePosted)
                .ToListAsync();
        }

        public async Task<Career> GetCareerByIdAsync(int id)
        {
            return await _context.Careers.FindAsync(id);
        }

        public async Task<Career> CreateCareerAsync(Career career)
        {
            career.DatePosted = DateTime.UtcNow;
            _context.Careers.Add(career);
            await _context.SaveChangesAsync();
            return career;
        }

        public async Task<Career> UpdateCareerAsync(Career career)
        {
            _context.Careers.Update(career);
            await _context.SaveChangesAsync();
            return career;
        }

        public async Task DeleteCareerAsync(int id)
        {
            var career = await _context.Careers.FindAsync(id);
            if (career != null)
            {
                _context.Careers.Remove(career);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<string>> GetDepartmentsAsync()
        {
            return await _context.Careers
                .Where(c => c.IsActive)
                .Select(c => c.Department)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();
        }
    }
}