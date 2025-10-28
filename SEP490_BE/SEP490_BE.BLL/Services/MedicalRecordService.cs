using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;
using SEP490_BE.DAL.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.Services
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly IMedicalRecordRepository _medicalRecordRepository;

        public MedicalRecordService(IMedicalRecordRepository medicalRecordRepository)
        {
            _medicalRecordRepository = medicalRecordRepository;
        }

        public async Task<List<MedicalRecord>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _medicalRecordRepository.GetAllAsync(cancellationToken);
        }

        public async Task<MedicalRecord?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _medicalRecordRepository.GetByIdAsync(id, cancellationToken);
        }
    }
}
