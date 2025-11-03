using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.DTOs.ManageReceptionist.ManageAppointment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.IServices
{
    public interface IPatientService
    {
        Task<PatientInfoDto?> GetByIdAsync(int patientId, CancellationToken cancellationToken = default);
    }
}
