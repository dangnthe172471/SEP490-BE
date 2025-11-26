using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.IRepositories.IPaymentRepositories;
using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.Repositories.PaymentRepositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly DiamondHealthContext _db;

        public PaymentRepository(DiamondHealthContext db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(Payment payment)
        {
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();
            return payment.PaymentId;
        }

        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await _db.Payments.FirstOrDefaultAsync(x => x.PaymentId == id);
        }

        public async Task UpdateAsync(Payment payment)
        {
            _db.Payments.Update(payment);
            await _db.SaveChangesAsync();
        }
        public async Task<List<MedicalService>> GetByRecordIdAsync(int recordId)
        {
            return await _db.MedicalServices
                .Include(x => x.Service)
                .Where(x => x.RecordId == recordId)
                .ToListAsync();
        }
        public async Task<Payment?> GetPendingPaymentByRecordIdAsync(int recordId)
        {
            return await _db.Payments
                .FirstOrDefaultAsync(x => x.RecordId == recordId && x.Status == "Pending");
        }
        public async Task<bool> ExistsMedicalRecord(int recordId)
        {
            return await _db.MedicalRecords.AnyAsync(x => x.RecordId == recordId);
        }
        public async Task<Payment?> GetLastPaymentByRecordIdAsync(int recordId)
        {
            return await _db.Payments
                .Where(p => p.RecordId == recordId)
                .OrderByDescending(p => p.PaymentId)
                .FirstOrDefaultAsync();
        }
        public async Task<Payment?> GetPaidPaymentAsync(int recordId)
        {
            return await _db.Payments
                .Where(p => p.RecordId == recordId && p.Status == "Paid")
                .OrderByDescending(p => p.PaymentId)
                .FirstOrDefaultAsync();
        }

        public async Task<Payment?> GetByOrderCodeAsync(long orderCode)
        {
            return await _db.Payments
                .FirstOrDefaultAsync(x => x.OrderCode == orderCode);
        }

        public async Task<List<Payment>> GetPaymentsByRangeAsync(DateTime start, DateTime end)
        {
            return await _db.Payments
                .Where(p => p.PaymentDate >= start && p.PaymentDate <= end && p.Status.Equals("PAID"))
                .OrderBy(p => p.PaymentDate)
                .ToListAsync();
        }

    }
}
