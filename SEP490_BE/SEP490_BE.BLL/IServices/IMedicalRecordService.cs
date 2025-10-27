using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.IServices
{
    public interface IMedicalRecordService
    {
        Task<List<MedicalRecord>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<MedicalRecord?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}
