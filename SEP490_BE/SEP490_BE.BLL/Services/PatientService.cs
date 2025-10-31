using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.DTOs.ManageReceptionist.ManageAppointment;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.Services
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _PatientRepository;
        private readonly IUserRepository _UserRository;

        public PatientService(IPatientRepository PatientRepository, IUserRepository userRository)
        {
            _PatientRepository = PatientRepository;
            _UserRository = userRository;
        }
        public async Task<PatientInfoDto?> GetByIdAsync(int patientId, CancellationToken cancellationToken = default)
        {
            var patient = await _PatientRepository.GetByIdAsync(patientId, cancellationToken);
            if (patient == null)
            {
                return null;
            }

            //int userId = patient.UserId;
            //User user = _UserRository.GetByIdAsync(userId, cancellationToken).Result;

            var PatientDto = new PatientInfoDto
            {
                PatientId = patient.PatientId,
                UserId = patient.UserId,
            };
        //{
        //    public int PatientId { get; set; }
        //    public int UserId { get; set; }
        //    public string FullName { get; set; } = string.Empty;
        //    public string Email { get; set; } = string.Empty;
        //    public string Phone { get; set; } = string.Empty;
        //    public string? Allergies { get; set; }
        //    public string? MedicalHistory { get; set; }
        //}
            if (patient != null)
            {
                PatientDto.Allergies = patient.Allergies;
                PatientDto.MedicalHistory = patient.MedicalHistory;
            }

            return PatientDto;
        }
    }
}
