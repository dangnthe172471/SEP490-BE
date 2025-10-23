using Microsoft.Extensions.Configuration;
using SEP490_BE.BLL.IServices;
using SEP490_BE.BLL.IServices.ManageReceptionist.ManageAppointment;
using SEP490_BE.DAL.DTOs.ManageReceptionist.ManageAppointment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.Services.ManageReceptionist.ManageAppointment
{
    public class EmailServiceApp : IEmailServiceApp
    {
        private readonly IConfiguration _configuration;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly bool _enableSsl;
        private readonly string _senderName;
        private readonly string _senderEmail;
        private readonly string _senderPassword;

        public EmailServiceApp(IConfiguration configuration)
        {
            _configuration = configuration;
            _smtpHost = _configuration["Smtp:Host"] ?? throw new ArgumentNullException("Smtp:Host");
            _smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
            _enableSsl = bool.Parse(_configuration["Smtp:EnableSsl"] ?? "true");
            _senderName = _configuration["Smtp:SenderName"] ?? "SEP490 Support";
            _senderEmail = _configuration["Smtp:User"] ?? throw new ArgumentNullException("Smtp:User");
            _senderPassword = _configuration["Smtp:Pass"] ?? throw new ArgumentNullException("Smtp:Pass");
        }

        public async Task SendAppointmentConfirmationEmailAsync(AppointmentConfirmationDto confirmation, CancellationToken cancellationToken = default)
        {
            var subject = "Xác nhận đặt lịch khám bệnh thành công";
            var body = GenerateConfirmationEmailBody(confirmation);

            await SendEmailAsync(confirmation.PatientEmail, subject, body, cancellationToken);
        }

        public async Task SendAppointmentRescheduleEmailAsync(AppointmentConfirmationDto confirmation, CancellationToken cancellationToken = default)
        {
            var subject = "Thông báo thay đổi lịch khám bệnh";
            var body = GenerateRescheduleEmailBody(confirmation);

            await SendEmailAsync(confirmation.PatientEmail, subject, body, cancellationToken);
        }

        public async Task SendAppointmentCancellationEmailAsync(AppointmentConfirmationDto confirmation, CancellationToken cancellationToken = default)
        {
            var subject = "Thông báo hủy lịch khám bệnh";
            var body = GenerateCancellationEmailBody(confirmation);

            await SendEmailAsync(confirmation.PatientEmail, subject, body, cancellationToken);
        }

        public async Task SendAppointmentStatusUpdateEmailAsync(AppointmentConfirmationDto confirmation, CancellationToken cancellationToken = default)
        {
            var subject = "Cập nhật trạng thái lịch khám bệnh";
            var body = GenerateStatusUpdateEmailBody(confirmation);

            await SendEmailAsync(confirmation.PatientEmail, subject, body, cancellationToken);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
        {
            try
            {
                using var message = new MailMessage();
                message.From = new MailAddress(_senderEmail, _senderName);
                message.To.Add(toEmail);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                using var smtpClient = new SmtpClient(_smtpHost, _smtpPort);
                smtpClient.EnableSsl = _enableSsl;
                smtpClient.Credentials = new NetworkCredential(_senderEmail, _senderPassword);

                await smtpClient.SendMailAsync(message, cancellationToken);
            }
            catch (Exception ex)
            {
                // Log error but don't throw to avoid blocking the main operation
                Console.WriteLine($"Failed to send email: {ex.Message}");
                // Consider using proper logging framework like Serilog or NLog
            }
        }

        private string GenerateConfirmationEmailBody(AppointmentConfirmationDto confirmation)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: white; padding: 30px; border-radius: 0 0 5px 5px; }}
        .info-row {{ margin: 15px 0; padding: 10px; background-color: #f5f5f5; border-left: 4px solid #4CAF50; }}
        .label {{ font-weight: bold; color: #555; }}
        .value {{ color: #333; margin-left: 10px; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; text-align: center; color: #777; font-size: 14px; }}
        .note {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✓ Đặt lịch khám thành công</h1>
        </div>
        <div class='content'>
            <p>Kính gửi <strong>{confirmation.PatientName}</strong>,</p>
            <p>Chúng tôi xác nhận đã nhận được yêu cầu đặt lịch khám của bạn. Dưới đây là thông tin chi tiết:</p>
            
            <div class='info-row'>
                <span class='label'>Mã lịch hẹn:</span>
                <span class='value'>#{confirmation.AppointmentId}</span>
            </div>
            
            <div class='info-row'>
                <span class='label'>Bác sĩ:</span>
                <span class='value'>{confirmation.DoctorName}</span>
            </div>
            
            <div class='info-row'>
                <span class='label'>Chuyên khoa:</span>
                <span class='value'>{confirmation.DoctorSpecialty}</span>
            </div>
            
            <div class='info-row'>
                <span class='label'>Thời gian khám:</span>
                <span class='value'>{confirmation.AppointmentDate:dd/MM/yyyy HH:mm}</span>
            </div>
            
            {(!string.IsNullOrWhiteSpace(confirmation.ReasonForVisit) ?
                $@"<div class='info-row'>
                    <span class='label'>Lý do khám:</span>
                    <span class='value'>{confirmation.ReasonForVisit}</span>
                </div>" : "")}
            
            <div class='info-row'>
                <span class='label'>Trạng thái:</span>
                <span class='value'>{GetStatusText(confirmation.Status)}</span>
            </div>
            
            <div class='note'>
                <strong>Lưu ý:</strong>
                <ul>
                    <li>Vui lòng đến trước giờ hẹn 15 phút để làm thủ tục</li>
                    <li>Mang theo giấy tờ tùy thân và các xét nghiệm liên quan (nếu có)</li>
                    <li>Nếu cần thay đổi lịch hẹn, vui lòng liên hệ trước 24 giờ</li>
                </ul>
            </div>
            
            <div class='footer'>
                <p>Cảm ơn bạn đã tin tưởng sử dụng dịch vụ của chúng tôi!</p>
                <p>Nếu có thắc mắc, vui lòng liên hệ: {_senderEmail}</p>
                <p style='font-size: 12px; color: #999; margin-top: 15px;'>
                    Email này được gửi tự động, vui lòng không trả lời trực tiếp.
                </p>
            </div>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateRescheduleEmailBody(AppointmentConfirmationDto confirmation)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9; }}
        .header {{ background-color: #2196F3; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: white; padding: 30px; border-radius: 0 0 5px 5px; }}
        .info-row {{ margin: 15px 0; padding: 10px; background-color: #f5f5f5; border-left: 4px solid #2196F3; }}
        .label {{ font-weight: bold; color: #555; }}
        .value {{ color: #333; margin-left: 10px; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; text-align: center; color: #777; font-size: 14px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔄 Thông báo thay đổi lịch khám</h1>
        </div>
        <div class='content'>
            <p>Kính gửi <strong>{confirmation.PatientName}</strong>,</p>
            <p>Lịch khám của bạn đã được thay đổi thành công. Thông tin mới như sau:</p>
            
            <div class='info-row'>
                <span class='label'>Mã lịch hẹn:</span>
                <span class='value'>#{confirmation.AppointmentId}</span>
            </div>
            
            <div class='info-row'>
                <span class='label'>Thời gian khám mới:</span>
                <span class='value'>{confirmation.AppointmentDate:dd/MM/yyyy HH:mm}</span>
            </div>
            
            <div class='info-row'>
                <span class='label'>Bác sĩ:</span>
                <span class='value'>{confirmation.DoctorName} - {confirmation.DoctorSpecialty}</span>
            </div>
            
            {(!string.IsNullOrWhiteSpace(confirmation.ReasonForVisit) ?
                $@"<div class='info-row'>
                    <span class='label'>Lý do khám:</span>
                    <span class='value'>{confirmation.ReasonForVisit}</span>
                </div>" : "")}
            
            <div class='footer'>
                <p>Cảm ơn bạn đã thông báo!</p>
                <p>Nếu có thắc mắc, vui lòng liên hệ: {_senderEmail}</p>
            </div>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateCancellationEmailBody(AppointmentConfirmationDto confirmation)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9; }}
        .header {{ background-color: #f44336; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: white; padding: 30px; border-radius: 0 0 5px 5px; }}
        .info-row {{ margin: 15px 0; padding: 10px; background-color: #f5f5f5; border-left: 4px solid #f44336; }}
        .label {{ font-weight: bold; color: #555; }}
        .value {{ color: #333; margin-left: 10px; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; text-align: center; color: #777; font-size: 14px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✖ Thông báo hủy lịch khám</h1>
        </div>
        <div class='content'>
            <p>Kính gửi <strong>{confirmation.PatientName}</strong>,</p>
            <p>Lịch khám của bạn đã được hủy. Thông tin lịch hẹn:</p>
            
            <div class='info-row'>
                <span class='label'>Mã lịch hẹn:</span>
                <span class='value'>#{confirmation.AppointmentId}</span>
            </div>
            
            <div class='info-row'>
                <span class='label'>Thời gian khám:</span>
                <span class='value'>{confirmation.AppointmentDate:dd/MM/yyyy HH:mm}</span>
            </div>
            
            <div class='info-row'>
                <span class='label'>Bác sĩ:</span>
                <span class='value'>{confirmation.DoctorName}</span>
            </div>
            
            <div class='footer'>
                <p>Nếu bạn muốn đặt lịch khám mới, vui lòng truy cập hệ thống.</p>
                <p>Nếu có thắc mắc, vui lòng liên hệ: {_senderEmail}</p>
            </div>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateStatusUpdateEmailBody(AppointmentConfirmationDto confirmation)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9; }}
        .header {{ background-color: #FF9800; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: white; padding: 30px; border-radius: 0 0 5px 5px; }}
        .info-row {{ margin: 15px 0; padding: 10px; background-color: #f5f5f5; border-left: 4px solid #FF9800; }}
        .label {{ font-weight: bold; color: #555; }}
        .value {{ color: #333; margin-left: 10px; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; text-align: center; color: #777; font-size: 14px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔔 Cập nhật trạng thái lịch khám</h1>
        </div>
        <div class='content'>
            <p>Kính gửi <strong>{confirmation.PatientName}</strong>,</p>
            <p>Trạng thái lịch khám của bạn đã được cập nhật:</p>
            
            <div class='info-row'>
                <span class='label'>Mã lịch hẹn:</span>
                <span class='value'>#{confirmation.AppointmentId}</span>
            </div>
            
            <div class='info-row'>
                <span class='label'>Trạng thái mới:</span>
                <span class='value'>{GetStatusText(confirmation.Status)}</span>
            </div>
            
            <div class='info-row'>
                <span class='label'>Thời gian khám:</span>
                <span class='value'>{confirmation.AppointmentDate:dd/MM/yyyy HH:mm}</span>
            </div>
            
            <div class='info-row'>
                <span class='label'>Bác sĩ:</span>
                <span class='value'>{confirmation.DoctorName}</span>
            </div>
            
            <div class='footer'>
                <p>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!</p>
                <p>Nếu có thắc mắc, vui lòng liên hệ: {_senderEmail}</p>
            </div>
        </div>
    </div>
</body>
</html>";
        }

        private string GetStatusText(string? status)
        {
            return status switch
            {
                "Pending" => "⏳ Đang chờ xác nhận",
                "Confirmed" => "✓ Đã xác nhận",
                "Completed" => "✓ Đã hoàn thành",
                "Cancelled" => "✖ Đã hủy",
                "No-Show" => "⚠ Không đến khám",
                _ => status ?? "Không xác định"
            };
        }
    }
}