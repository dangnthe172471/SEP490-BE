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
    public class ShiftRepository : IShiftRepository
    {
        private readonly DiamondHealthContext _context;
        public ShiftRepository(DiamondHealthContext context)
        {
            _context = context;
        }
        public async Task<List<Shift>> GetAllShiftsAsync()
        {
            return await _context.Shifts.ToListAsync();
        }
    }
}
