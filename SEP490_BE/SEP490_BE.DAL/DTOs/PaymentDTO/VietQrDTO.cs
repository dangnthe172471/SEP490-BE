using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.DTOs.PaymentDTO
{
    public class GenerateQrDto
    {
        public int Amount { get; set; }
        public string? AddInfo { get; set; }
    }

    public class QrResultDto
    {
        public string QrUrl { get; set; }
    }

    public class BankInfoDto
    {
        public string BankId { get; set; }
        public string AccountNo { get; set; }
        public string AccountName { get; set; }
        public string Template { get; set; }
    }
}
