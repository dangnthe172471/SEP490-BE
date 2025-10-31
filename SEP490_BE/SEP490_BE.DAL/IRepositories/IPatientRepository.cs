using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.IRepositories
{
    public interface IPatientRepository
    {
        //Task<List<Patient>> GetAllAsync(CancellationToken cancellationToken = default);
        //Task<Patient?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default);
        Task<Patient?> GetByIdAsync(int PatientId, CancellationToken cancellationToken = default);
    }
}
