using Microsoft.EntityFrameworkCore;
using SEP490_BE.BLL.Helpers;
using SEP490_BE.BLL.IServices;
using SEP490_BE.BLL.IServices.IManagerService;
using SEP490_BE.DAL.DTOs.ManagerDTO.Notification;
using SEP490_BE.DAL.IRepositories.IManagerRepositories;
using SEP490_BE.DAL.IRepositories.IManagerRepository;
using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.Services.ManagerServices
{
    public class NotificationService : IServices.IManagerService.INotificationService
    {
        private readonly DiamondHealthContext _context;
        private readonly IEmailService _emailService;
        private readonly INotificationRepository _notificationRepo;
        public NotificationService(DiamondHealthContext context, IEmailService emailService, INotificationRepository notificationRep)
        {
            _context = context;
            _emailService = emailService;
            _notificationRepo = notificationRep;
        }

        public async Task SendAppointmentReminderAsync(CancellationToken cancellationToken = default)
        {
            var tomorrow = DateTime.Today.AddDays(1);
            var nextDay = tomorrow.AddDays(1);

            var appointments = await _context.Appointments
                .Include(a => a.Patient)!.ThenInclude(p => p.User)
                .Include(a => a.Doctor)!.ThenInclude(d => d.User)
                .Where(a =>
                    a.Status == "Confirmed" &&
                    a.AppointmentDate >= tomorrow &&
                    a.AppointmentDate < nextDay)
                .ToListAsync(cancellationToken);

            foreach (var appt in appointments)
            {
                var patientUser = appt.Patient?.User;
                var doctorUser = appt.Doctor?.User;

                if (patientUser == null || string.IsNullOrWhiteSpace(patientUser.Email))
                    continue;

                var template = EmailTemplateHelper.LoadTemplate("AppointmentReminder.html");

                var body = EmailTemplateHelper.RenderTemplate(template, new Dictionary<string, string>
                {
                    ["PatientName"] = patientUser.FullName,
                    ["DoctorName"] = doctorUser?.FullName ?? "Bác sĩ",
                    ["Date"] = appt.AppointmentDate.ToString("dd/MM/yyyy"),
                    ["Time"] = appt.AppointmentDate.ToString("HH:mm")
                });

                await _emailService.SendEmailAsync(
                    patientUser.Email,
                    "Nhắc lịch khám tại Diamond Health Clinic",
                    body,
                    cancellationToken
                );
            }
        }


        public async Task SendNotificationAsync(CreateNotificationDTO dto)
        {
            try
            {
                // 1. Tạo thông báo trong DB
                var notificationId = await _notificationRepo.CreateNotificationAsync(dto);

                // 2. Xác định danh sách người nhận
                List<int> receivers = new();

                if (dto.IsGlobal)
                {
                    receivers = await _notificationRepo.GetUserIdsByRolesAsync(new List<string>
            {
                "Doctor", "Receptionist", "Manager", "Patient"
            });
                }
                else if (dto.RoleNames != null && dto.RoleNames.Any())
                {
                    receivers = await _notificationRepo.GetUserIdsByRolesAsync(dto.RoleNames);
                }
                else if (dto.ReceiverIds != null && dto.ReceiverIds.Any())
                {
                    receivers = dto.ReceiverIds;
                }

                // 3. Lưu danh sách người nhận
                if (receivers.Any())
                {
                    await _notificationRepo.AddReceiversAsync(notificationId, receivers.Distinct().ToList());
                }

                // 4. Lấy danh sách user có email (lọc theo receivers)
                var users = await _context.Users
                    .Where(u => receivers.Contains(u.UserId) && !string.IsNullOrEmpty(u.Email))
                    .ToListAsync();

                Console.WriteLine($"Đang gửi thông báo đến {users.Count} người dùng.");

                if (!users.Any())
                {
                    Console.WriteLine("Không có user nào có email để gửi.");
                    return;
                }

                // 5. Load template duy nhất GenericNotification.html
                string template;
                try
                {
                    template = EmailTemplateHelper.LoadTemplate("GenericNotification.html");
                }
                catch (Exception)
                {
                    template = "<html><body><h3>Xin chào {{UserName}},</h3><p>{{Content}}</p></body></html>";
                    Console.WriteLine("Không tìm thấy file GenericNotification.html, dùng fallback mặc định.");
                }

                // 6. Gửi email song song để tăng tốc
                var sendTasks = users.Select(async user =>
                {
                    try
                    {
                        var body = EmailTemplateHelper.RenderTemplate(template, new Dictionary<string, string>
                        {
                            ["UserName"] = user.FullName,
                            ["Title"] = dto.Title ?? "Thông báo",
                            ["Content"] = dto.Content ?? ""
                        });

                        await _emailService.SendEmailAsync(
                            user.Email,
                            dto.Title ?? "Thông báo từ Diamond Health Clinic",
                            body
                        );

                        Console.WriteLine($"Đã gửi mail đến {user.Email}.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Gửi mail đến {user.Email} thất bại: {ex.Message}");
                    }
                });

                await Task.WhenAll(sendTasks);
                Console.WriteLine("Tất cả mail đã được gửi xong.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong SendNotificationAsync: {ex}");
            }
        }


    }
}
