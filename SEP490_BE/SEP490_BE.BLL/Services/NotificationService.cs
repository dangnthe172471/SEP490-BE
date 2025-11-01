using Microsoft.EntityFrameworkCore;
using SEP490_BE.BLL.Helpers;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.ManageReceptionist.ManageAppointment;
using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.Services
{
    public class NotificationService : INotificationService
    {
        private readonly DiamondHealthContext _context;
        private readonly IEmailService _emailService;

        public NotificationService(DiamondHealthContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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

    }
}
