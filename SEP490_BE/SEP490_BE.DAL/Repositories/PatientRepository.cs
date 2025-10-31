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
    public class PatientRepository : IPatientRepository
    {
        private readonly DiamondHealthContext _dbContext;

        public PatientRepository(DiamondHealthContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Patient?> GetByIdAsync(int patientId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Patients
                .FirstOrDefaultAsync(u => u.PatientId == patientId, cancellationToken);
        }
    }
}
