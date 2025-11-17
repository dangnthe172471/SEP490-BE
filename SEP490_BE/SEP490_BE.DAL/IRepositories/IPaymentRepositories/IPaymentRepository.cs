using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.IRepositories.IPaymentRepositories
{
    public interface IPaymentRepository
    {
        Task<int> CreateAsync(Payment payment);
        Task<Payment?> GetByIdAsync(int id);
        Task UpdateAsync(Payment payment);
        Task<List<MedicalService>> GetByRecordIdAsync(int recordId);
        Task<Payment?> GetPendingPaymentByRecordIdAsync(int recordId);
        Task<bool> ExistsMedicalRecord(int recordId);
        Task<Payment?> GetLastPaymentByRecordIdAsync(int recordId);
        Task<Payment?> GetPaidPaymentAsync(int recordId);
        Task<Payment?> GetByOrderCodeAsync(long orderCode);
    }
}
