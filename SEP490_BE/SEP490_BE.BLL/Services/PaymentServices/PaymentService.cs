using Microsoft.Extensions.Configuration;
using Net.payOS;
using Net.payOS.Types;
using SEP490_BE.BLL.IServices.IPaymentServices;
using SEP490_BE.DAL.DTOs.PaymentDTO;
using SEP490_BE.DAL.IRepositories.IPaymentRepositories;
using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
namespace SEP490_BE.BLL.Services.PaymentServices
{
    public class PaymentService : IPaymentService   
    {
        private readonly IPaymentRepository _repo;
        private readonly IPayOSService _payOsService;
        private readonly IConfiguration _config;
        private readonly BankInfoDto _bankInfo;
        public PaymentService(IPaymentRepository repo, IPayOSService payOsService, IConfiguration configuration)
        {
            _repo = repo;
            _payOsService = payOsService;
            _config = configuration;
            
        }
        public async Task<CreatePaymentResponseDTO> CreatePaymentAsync(CreatePaymentRequestDTO dto, bool role)
        {
            
            var exists = await _repo.ExistsMedicalRecord(dto.MedicalRecordId);
            if (!exists)
                throw new Exception("MedicalRecordId không tồn tại");

           
            var lastPayment = await _repo.GetLastPaymentByRecordIdAsync(dto.MedicalRecordId);

        
            if (lastPayment != null && lastPayment.Status == "Paid")
            {
                throw new Exception("Hồ sơ này đã được thanh toán.");
            }

            if (lastPayment != null && lastPayment.Status == "Pending")
            {
                if (lastPayment.OrderCode != null)
                {
                    bool isActive = await _payOsService.IsPaymentLinkActive(lastPayment.OrderCode.Value);

                    if (isActive)
                    {
                        return new CreatePaymentResponseDTO
                        {
                            PaymentId = lastPayment.PaymentId,
                            CheckoutUrl = lastPayment.CheckoutUrl
                        };
                    }
                }
               
            }


            var newPayment = new Payment
            {
                RecordId = dto.MedicalRecordId,
                Amount = dto.Amount,
                Status = "Pending",
                PaymentDate = DateTime.Now
            };

            int paymentId = await _repo.CreateAsync(newPayment);

            var rnd = new Random(); 
            string datePart = DateTime.UtcNow.ToString("ddMMyy");
            string randomPart = rnd.Next(100000, 999999).ToString();
            long orderCode = long.Parse(datePart + randomPart);
            newPayment.OrderCode = orderCode;
            await _repo.UpdateAsync(newPayment);


            var items = dto.Items.Select(i => new ItemData(i.Name, i.Quantity, i.Price)).ToList();

            // Tạo link PayOS
            string checkoutUrl = role
     ? await _payOsService.CreatePaymentLinkAsync(orderCode, dto, items)
     : await _payOsService.CreatePaymentReceptionistAsync(orderCode, dto, items);


            newPayment.OrderCode = orderCode;

            newPayment.CheckoutUrl = checkoutUrl;
            await _repo.UpdateAsync(newPayment);

            return new CreatePaymentResponseDTO
            {
                PaymentId = paymentId,
                CheckoutUrl = checkoutUrl
            };
        }



        public async Task UpdatePaymentStatusAsync(long orderCode, string status)
        {
            var payment = await _repo.GetByOrderCodeAsync(orderCode);
            if (payment == null) return;

            payment.Status = status;
            payment.PaymentDate = DateTime.Now;

            await _repo.UpdateAsync(payment);
        }

        public async Task<List<MedicalRecordServiceItemDTO>> GetServicesForRecordAsync(int recordId)
        {
            var services = await _repo.GetByRecordIdAsync(recordId);

            return services.Select(x => new MedicalRecordServiceItemDTO
            {
                Name = x.Service.ServiceName,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice
            }).ToList();
        }

        public async Task<PaymentStatusDTO> GetPaymentStatusAsync(int recordId)
        {
            // 1 Kiểm tra đã thanh toán?
            var paidPayment = await _repo.GetPaidPaymentAsync(recordId);

            if (paidPayment != null)
            {
                return new PaymentStatusDTO
                {
                    RecordId = recordId,
                    Status = "Paid",
                    CheckoutUrl = paidPayment.CheckoutUrl
                };
            }

            // Chua co thanh toan nao thanh cong thi lay payment moi nhat
            var lastPayment = await _repo.GetLastPaymentByRecordIdAsync(recordId);

            if (lastPayment == null)
            {
                return new PaymentStatusDTO
                {
                    RecordId = recordId,
                    Status = "None",
                    CheckoutUrl = null
                };
            }

            return new PaymentStatusDTO
            {
                RecordId = recordId,
                Status = lastPayment.Status, 
                CheckoutUrl = lastPayment.CheckoutUrl
            };
        }

        public async Task<List<PaymentChartDto>> GetPaymentsForChartAsync(DateTime start, DateTime end)
        {
            var payments = await _repo.GetPaymentsByRangeAsync(start, end);

            return payments.Select(p => new PaymentChartDto
            {
                PaymentDate = (DateTime)p.PaymentDate,
                Amount = p.Amount
            }).ToList();
        }
        public QrResultDto GenerateQrLink(GenerateQrDto dto, IConfiguration config)
        {
            if (dto.Amount <= 0)
                throw new ArgumentException("Số tiền không hợp lệ");

            // Lấy thông tin ngân hàng trực tiếp từ config
            var bankInfo = new BankInfoDto
            {
                BankId = config["BankInfo:BankId"],
                AccountNo = config["BankInfo:AccountNo"],
                AccountName = config["BankInfo:AccountName"],
                Template = config["BankInfo:Template"]
            };

            string addInfoEncoded = HttpUtility.UrlEncode((dto.AddInfo ?? "").ToUpper());
            string accountNameEncoded = HttpUtility.UrlEncode(bankInfo.AccountName.ToUpper());

            string qrUrl = $"https://img.vietqr.io/image/{bankInfo.BankId}-{bankInfo.AccountNo}-{bankInfo.Template}.png?amount={dto.Amount}&addInfo={addInfoEncoded}&accountName={accountNameEncoded}";

            return new QrResultDto { QrUrl = qrUrl };
        }

    }
}
