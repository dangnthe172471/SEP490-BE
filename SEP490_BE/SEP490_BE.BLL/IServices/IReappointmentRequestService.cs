using SEP490_BE.DAL.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.IServices
{
    public interface IReappointmentRequestService
    {
        // Doctor: Tạo yêu cầu tái khám
        Task<int> CreateReappointmentRequestAsync(CreateReappointmentRequestDto request, int doctorUserId, CancellationToken cancellationToken = default);

        // Receptionist: Lấy danh sách yêu cầu tái khám
        Task<List<ReappointmentRequestDto>> GetPendingReappointmentRequestsAsync(int receptionistUserId, CancellationToken cancellationToken = default);

        // Receptionist: Lấy chi tiết yêu cầu tái khám
        Task<ReappointmentRequestDto?> GetReappointmentRequestByIdAsync(int notificationId, int receptionistUserId, CancellationToken cancellationToken = default);

        // Receptionist: Xử lý yêu cầu - tạo appointment mới
        Task<int> CompleteReappointmentRequestAsync(CompleteReappointmentRequestDto request, int receptionistUserId, CancellationToken cancellationToken = default);

        // Doctor: Lấy danh sách yêu cầu đã tạo
        Task<List<ReappointmentRequestDto>> GetMyReappointmentRequestsAsync(int doctorUserId, CancellationToken cancellationToken = default);
    }
}

