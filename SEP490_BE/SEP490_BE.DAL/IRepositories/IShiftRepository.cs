using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.IRepositories
{
    public interface IShiftRepository
    {
        Task<List<Shift>> GetAllShiftsAsync();
    }
}
