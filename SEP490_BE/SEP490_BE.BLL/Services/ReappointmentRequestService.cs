using SEP490_BE.BLL.IServices;
using SEP490_BE.BLL.IServices.IManagerService;
using SEP490_BE.BLL.IServices.ManageReceptionist.ManageAppointment;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.DTOs.ManagerDTO.Notification;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.IRepositories.IManagerRepository;
using SEP490_BE.DAL.IRepositories.ManageReceptionist.ManageAppointment;
using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.Services
{
    public class ReappointmentRequestService : IReappointmentRequestService
    {
        private readonly INotificationService _notificationService;
        private readonly INotificationRepository _notificationRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IAppointmentService _appointmentService;
        private readonly IUserRepository _userRepository;

        public ReappointmentRequestService(
            INotificationService notificationService,
            INotificationRepository notificationRepository,
            IAppointmentRepository appointmentRepository,
            IAppointmentService appointmentService,
            IUserRepository userRepository)
        {
            _notificationService = notificationService;
            _notificationRepository = notificationRepository;
            _appointmentRepository = appointmentRepository;
            _appointmentService = appointmentService;
            _userRepository = userRepository;
        }

        public async Task<int> CreateReappointmentRequestAsync(CreateReappointmentRequestDto request, int doctorUserId, CancellationToken cancellationToken = default)
        {
            // Lấy thông tin appointment hiện tại
            var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken);
            if (appointment == null)
            {
                throw new ArgumentException("Không tìm thấy lịch hẹn.");
            }

            // Kiểm tra bác sĩ có phải là bác sĩ của appointment này không
            var doctor = await _appointmentRepository.GetDoctorByIdAsync(appointment.DoctorId, cancellationToken);
            if (doctor == null)
            {
                throw new ArgumentException("Không tìm thấy thông tin bác sĩ.");
            }

            // Kiểm tra doctorUserId có khớp với doctor của appointment không
            if (doctor.UserId != doctorUserId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền tạo yêu cầu tái khám cho lịch hẹn này.");
            }

            // Tạo JSON data
            var requestData = new ReappointmentRequestData
            {
                AppointmentId = request.AppointmentId,
                PatientId = appointment.PatientId,
                DoctorId = appointment.DoctorId,
                PreferredDate = request.PreferredDate,
                Notes = request.Notes,
                IsCompleted = false
            };

            var jsonContent = JsonSerializer.Serialize(requestData);

            // Lấy thông tin bệnh nhân để tạo title
            var patient = await _appointmentRepository.GetPatientByIdAsync(appointment.PatientId, cancellationToken);
            var patientName = patient?.User?.FullName ?? "Bệnh nhân";

            // Tạo notification
            var notificationDto = new CreateNotificationDTO
            {
                Title = $"Yêu cầu tái khám cho {patientName}",
                Content = jsonContent,
                Type = "ReappointmentRequest",
                CreatedBy = doctorUserId,
                IsGlobal = false,
                RoleNames = new List<string> { "Receptionist" }
            };

            // Tạo notification và lấy ID
            var notificationId = await _notificationRepository.CreateNotificationAsync(notificationDto);
            
            // Lấy danh sách Receptionist để gửi notification
            var receiverIds = await _notificationRepository.GetUserIdsByRolesAsync(new List<string> { "Receptionist" });
            
            // Thêm receivers
            if (receiverIds.Any())
            {
                await _notificationRepository.AddReceiversAsync(notificationId, receiverIds.Distinct().ToList());
            }
            
            return notificationId;
        }

        public async Task<PagedResponse<ReappointmentRequestDto>> GetPendingReappointmentRequestsAsync(
            int receptionistUserId,
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null,
            string sortBy = "createdDate",
            string sortDirection = "desc",
            CancellationToken cancellationToken = default)
        {
            // Lấy tất cả notifications của receptionist có Type = "ReappointmentRequest"
            var user = await _userRepository.GetByIdAsync(receptionistUserId, cancellationToken);
            if (user == null)
            {
                throw new ArgumentException("Không tìm thấy người dùng.");
            }

            // Lấy notifications từ notification service
            var notifications = await _notificationService.GetNotificationsByUserAsync(receptionistUserId, 1, 1000);
            
            // Filter và parse
            var requests = new List<ReappointmentRequestDto>();
            
            foreach (var notif in notifications.Items.Where(n => n.Type == "ReappointmentRequest"))
            {
                try
                {
                    var requestData = JsonSerializer.Deserialize<ReappointmentRequestData>(notif.Content);
                    if (requestData == null) continue;
                    if (requestData.IsCompleted) continue;

                    // Lấy thông tin bệnh nhân và bác sĩ
                    var patient = await _appointmentRepository.GetPatientByIdAsync(requestData.PatientId, cancellationToken);
                    var doctor = await _appointmentRepository.GetDoctorByIdAsync(requestData.DoctorId, cancellationToken);

                    requests.Add(new ReappointmentRequestDto
                    {
                        NotificationId = notif.NotificationId,
                        Title = notif.Title,
                        Content = notif.Content,
                        Type = notif.Type,
                        CreatedDate = notif.CreatedDate,
                        IsRead = notif.IsRead,
                        AppointmentId = requestData.AppointmentId,
                        PatientId = requestData.PatientId,
                        PatientName = patient?.User?.FullName ?? "N/A",
                        PatientPhone = patient?.User?.Phone ?? "N/A",
                        PatientEmail = patient?.User?.Email,
                        DoctorId = requestData.DoctorId,
                        DoctorName = doctor?.User?.FullName ?? "N/A",
                        DoctorSpecialty = doctor?.Specialty ?? "N/A",
                        PreferredDate = requestData.PreferredDate,
                        Notes = requestData.Notes,
                        IsCompleted = requestData.IsCompleted
                    });
                }
                catch (JsonException)
                {
                    // Skip invalid JSON
                    continue;
                }
            }

            // Lọc theo searchTerm
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var keyword = searchTerm.Trim().ToLower();
                requests = requests.Where(r =>
                    (!string.IsNullOrEmpty(r.PatientName) && r.PatientName.ToLower().Contains(keyword)) ||
                    (!string.IsNullOrEmpty(r.DoctorName) && r.DoctorName.ToLower().Contains(keyword)) ||
                    (!string.IsNullOrEmpty(r.PatientPhone) && r.PatientPhone.ToLower().Contains(keyword)) ||
                    (r.Notes != null && r.Notes.ToLower().Contains(keyword))
                ).ToList();
            }

            // Sắp xếp
            IEnumerable<ReappointmentRequestDto> ordered = sortBy.ToLower() switch
            {
                "patientname" => sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
                    ? requests.OrderBy(r => r.PatientName)
                    : requests.OrderByDescending(r => r.PatientName),
                "preferreddate" => sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
                    ? requests.OrderBy(r => r.PreferredDate)
                    : requests.OrderByDescending(r => r.PreferredDate),
                "doctorname" => sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
                    ? requests.OrderBy(r => r.DoctorName)
                    : requests.OrderByDescending(r => r.DoctorName),
                _ => sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
                    ? requests.OrderBy(r => r.CreatedDate)
                    : requests.OrderByDescending(r => r.CreatedDate)
            };

            // Phân trang
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            var totalCount = ordered.Count();
            var items = ordered.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResponse<ReappointmentRequestDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<ReappointmentRequestDto?> GetReappointmentRequestByIdAsync(int notificationId, int receptionistUserId, CancellationToken cancellationToken = default)
        {
            var notifications = await _notificationService.GetNotificationsByUserAsync(receptionistUserId, 1, 1000);
            var notif = notifications.Items.FirstOrDefault(n => n.NotificationId == notificationId && n.Type == "ReappointmentRequest");
            
            if (notif == null)
            {
                return null;
            }

            try
            {
                var requestData = JsonSerializer.Deserialize<ReappointmentRequestData>(notif.Content);
                if (requestData == null) return null;

                // Lấy thông tin bệnh nhân và bác sĩ
                var patient = await _appointmentRepository.GetPatientByIdAsync(requestData.PatientId, cancellationToken);
                var doctor = await _appointmentRepository.GetDoctorByIdAsync(requestData.DoctorId, cancellationToken);

                return new ReappointmentRequestDto
                {
                    NotificationId = notif.NotificationId,
                    Title = notif.Title,
                    Content = notif.Content,
                    Type = notif.Type,
                    CreatedDate = notif.CreatedDate,
                    IsRead = notif.IsRead,
                    AppointmentId = requestData.AppointmentId,
                    PatientId = requestData.PatientId,
                    PatientName = patient?.User?.FullName ?? "N/A",
                    PatientPhone = patient?.User?.Phone ?? "N/A",
                    PatientEmail = patient?.User?.Email,
                    DoctorId = requestData.DoctorId,
                    DoctorName = doctor?.User?.FullName ?? "N/A",
                    DoctorSpecialty = doctor?.Specialty ?? "N/A",
                    PreferredDate = requestData.PreferredDate,
                    Notes = requestData.Notes,
                    IsCompleted = requestData.IsCompleted
                };
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public async Task<int> CompleteReappointmentRequestAsync(CompleteReappointmentRequestDto request, int receptionistUserId, CancellationToken cancellationToken = default)
        {
            // Lấy thông tin notification
            var notifications = await _notificationService.GetNotificationsByUserAsync(receptionistUserId, 1, 1000);
            var notif = notifications.Items.FirstOrDefault(n => n.NotificationId == request.NotificationId && n.Type == "ReappointmentRequest");
            
            if (notif == null)
            {
                throw new ArgumentException("Không tìm thấy yêu cầu tái khám.");
            }

            // Parse JSON từ Content
            ReappointmentRequestData requestData;
            try
            {
                requestData = JsonSerializer.Deserialize<ReappointmentRequestData>(notif.Content);
                if (requestData == null)
                {
                    throw new ArgumentException("Dữ liệu yêu cầu không hợp lệ.");
                }
            }
            catch (JsonException ex)
            {
                throw new ArgumentException($"Không thể parse dữ liệu yêu cầu: {ex.Message}");
            }

            // Lấy ReceptionistId từ UserId
            var receptionist = await _appointmentRepository.GetReceptionistByUserIdAsync(receptionistUserId, cancellationToken);
            if (receptionist == null)
            {
                throw new UnauthorizedAccessException("Không tìm thấy thông tin lễ tân.");
            }

            // Tạo appointment mới
            var createRequest = new DAL.DTOs.ManageReceptionist.ManageAppointment.CreateAppointmentByReceptionistRequest
            {
                PatientId = requestData.PatientId,
                DoctorId = requestData.DoctorId,
                AppointmentDate = request.AppointmentDate,
                ReasonForVisit = request.ReasonForVisit
            };

            var appointmentId = await _appointmentService.CreateAppointmentByReceptionistAsync(createRequest, receptionist.ReceptionistId, cancellationToken);

            // Đánh dấu notification là đã đọc
            await _notificationService.MarkAsReadAsync(receptionistUserId, request.NotificationId);

            // Cập nhật trạng thái hoàn thành trong nội dung thông báo
            requestData.IsCompleted = true;
            var updatedContent = JsonSerializer.Serialize(requestData);
            await _notificationRepository.UpdateNotificationContentAsync(request.NotificationId, updatedContent);

            return appointmentId;
        }

        public async Task<List<ReappointmentRequestDto>> GetMyReappointmentRequestsAsync(int doctorUserId, CancellationToken cancellationToken = default)
        {
            // Lấy doctor từ userId - cần tìm doctor có UserId = doctorUserId
            var user = await _userRepository.GetByIdAsync(doctorUserId, cancellationToken);
            if (user == null || user.Doctor == null)
            {
                throw new ArgumentException("Không tìm thấy thông tin bác sĩ.");
            }

            var doctor = user.Doctor;
            var doctorId = doctor.DoctorId;

            // Lấy tất cả notifications có Type = "ReappointmentRequest" và CreatedBy = doctorUserId
            // Note: Cần lấy từ tất cả notifications, không chỉ của user hiện tại
            // Tạm thời lấy từ notification service với userId = doctorUserId
            var notifications = await _notificationService.GetNotificationsByUserAsync(doctorUserId, 1, 1000);
            
            var requests = new List<ReappointmentRequestDto>();
            
            foreach (var notif in notifications.Items.Where(n => n.Type == "ReappointmentRequest"))
            {
                try
                {
                    var requestData = JsonSerializer.Deserialize<ReappointmentRequestData>(notif.Content);
                    if (requestData == null || requestData.DoctorId != doctor.DoctorId) continue;

                    // Lấy thông tin bệnh nhân
                    var patient = await _appointmentRepository.GetPatientByIdAsync(requestData.PatientId, cancellationToken);

                    requests.Add(new ReappointmentRequestDto
                    {
                        NotificationId = notif.NotificationId,
                        Title = notif.Title,
                        Content = notif.Content,
                        Type = notif.Type,
                        CreatedDate = notif.CreatedDate,
                        IsRead = notif.IsRead,
                        AppointmentId = requestData.AppointmentId,
                        PatientId = requestData.PatientId,
                        PatientName = patient?.User?.FullName ?? "N/A",
                        PatientPhone = patient?.User?.Phone ?? "N/A",
                        PatientEmail = patient?.User?.Email,
                        DoctorId = requestData.DoctorId,
                        DoctorName = doctor.User?.FullName ?? "N/A",
                        DoctorSpecialty = doctor.Specialty ?? "N/A",
                        PreferredDate = requestData.PreferredDate,
                        Notes = requestData.Notes,
                        IsCompleted = requestData.IsCompleted
                    });
                }
                catch (JsonException)
                {
                    continue;
                }
            }

            return requests.OrderByDescending(r => r.CreatedDate).ToList();
        }
    }
}

