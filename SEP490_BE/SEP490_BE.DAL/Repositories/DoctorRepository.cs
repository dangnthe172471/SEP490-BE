using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.Repositories
{
    public class DoctorRepository: IDoctorRepository
    {
        private readonly DiamondHealthContext _context;

        public DoctorRepository(DiamondHealthContext context)
        {
            _context = context;
        }

        public async Task<List<Doctor>> GetAllDoctorsAsync()
        {
            return await _context.Doctors
                .Include(d => d.User)
                .ToListAsync();
        }
        public async Task<List<Doctor>> SearchDoctorsAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return await GetAllDoctorsAsync();
            }

            keyword = keyword.Trim().ToLower();

            return await _context.Doctors
                .Include(d => d.User)
                .Where(d =>
                    d.User.FullName.ToLower().Contains(keyword) ||
                    d.Specialty.ToLower().Contains(keyword))
                .ToListAsync();
        }
    }
}
