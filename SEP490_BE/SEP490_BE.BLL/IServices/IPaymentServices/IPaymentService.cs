using SEP490_BE.DAL.DTOs.PaymentDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.IServices.IPaymentServices
{
    public interface IPaymentService
    {
        Task<CreatePaymentResponseDTO> CreatePaymentAsync(CreatePaymentRequestDTO dto);
        Task UpdatePaymentStatusAsync(long orderCode, string status);
        Task<List<MedicalRecordServiceItemDTO>> GetServicesForRecordAsync(int recordId);
        Task<PaymentStatusDTO> GetPaymentStatusAsync(int recordId);
        Task<List<PaymentChartDto>> GetPaymentsForChartAsync(DateTime start, DateTime end);
    }
}
