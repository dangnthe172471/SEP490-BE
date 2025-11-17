using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.DTOs.PaymentDTO
{
    public class PayOSItemDTO
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
    }

    public class CreatePaymentRequestDTO
    {
        public int MedicalRecordId { get; set; }
        public int Amount { get; set; }
        public string Description { get; set; }
        public List<ItemDTO>? Items { get; set; } = new List<ItemDTO>();
    }

    public class ItemDTO
    {
        public string? Name { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
    }
    public class MedicalRecordServiceItemDTO
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public decimal Total => Quantity * UnitPrice;
    }

    public class PayOSCallbackDTO
    {
        public int OrderCode { get; set; }   
        public string Status { get; set; }      // PAID | CANCELLED
        public string Signature { get; set; }   
    }

    public class CreatePaymentResponseDTO
    {
        public int PaymentId { get; set; }
        public string CheckoutUrl { get; set; } = string.Empty;
    }
    public class PaymentStatusDTO
    {
        public int RecordId { get; set; }
        public string Status { get; set; } = "";
        public string? CheckoutUrl { get; set; }
    }


}
