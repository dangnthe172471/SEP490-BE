using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SEP490_BE.API.Controllers;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.MedicineDTO;
using System.Security.Claims;

namespace SEP490_BE.Tests.Controllers
{
    public class MedicineControllerTests
    {
        private readonly Mock<IMedicineService> _svc = new(MockBehavior.Strict);

        private static ClaimsPrincipal MakeUser(int userId)
        {
            var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Role, "Pharmacy Provider")
        };
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        }

        private MedicineController NewController(ClaimsPrincipal user)
        {
            var ctrl = new MedicineController(_svc.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                }
            };
            return ctrl;
        }

        // ✅ TC1: Provider=1, Paracetamol, Nausea, Providing
        [Fact(DisplayName = "TC1 - provider=1, Paracetamol/Nausea/Providing → 200 OK")]
        public async Task TC1_Create_Provider1_Paracetamol()
        {
            var user = MakeUser(10);
            var ctrl = NewController(user);
            var dto = new CreateMedicineDto { MedicineName = "Paracetamol", SideEffects = "Nausea", Status = "Providing" };

            _svc.Setup(s => s.GetProviderIdByUserIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _svc.Setup(s => s.CreateMedicineAsync(dto, 1, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await ctrl.Create(dto, default);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);
            Assert.Contains("Medicine added successfully", ok.Value!.ToString());
            _svc.VerifyAll();
        }

        // ✅ TC2: Provider=2, Paracetamol, Nausea, Providing
        [Fact(DisplayName = "TC2 - provider=2, Paracetamol/Nausea/Providing → 200 OK")]
        public async Task TC2_Create_Provider2_Paracetamol()
        {
            var user = MakeUser(20);
            var ctrl = NewController(user);
            var dto = new CreateMedicineDto { MedicineName = "Paracetamol", SideEffects = "Nausea", Status = "Providing" };

            _svc.Setup(s => s.GetProviderIdByUserIdAsync(20, It.IsAny<CancellationToken>()))
                .ReturnsAsync(2);
            _svc.Setup(s => s.CreateMedicineAsync(dto, 2, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await ctrl.Create(dto, default);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);
            _svc.VerifyAll();
        }

        // ❌ TC3: Provider=1, Paracetamol duplicate → 409 Conflict
        [Fact(DisplayName = "TC3 - provider=1 duplicate Paracetamol → 409 Conflict")]
        public async Task TC3_Create_Conflict_Duplicate()
        {
            var user = MakeUser(10);
            var ctrl = NewController(user);
            var dto = new CreateMedicineDto { MedicineName = "Paracetamol", SideEffects = "Nausea", Status = "Providing" };

            _svc.Setup(s => s.GetProviderIdByUserIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _svc.Setup(s => s.CreateMedicineAsync(dto, 1, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Medicine 'Aspirin' already exists for this provider."));

            var result = await ctrl.Create(dto, default);
            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(409, conflict.StatusCode);
            Assert.Contains("already exists", conflict.Value!.ToString());
            _svc.VerifyAll();
        }

        // ✅ TC4: Provider=2, " Amoxicillin ", Fever, Providing
        [Fact(DisplayName = "TC4 - provider=2, ' Amoxicillin ', Fever, providing → 200 OK (trim name)")]
        public async Task TC4_Create_TrimName_Amoxicillin()
        {
            var user = MakeUser(30);
            var ctrl = NewController(user);
            var dto = new CreateMedicineDto { MedicineName = " Amoxicillin ", SideEffects = "Fever", Status = "providing" };

            _svc.Setup(s => s.GetProviderIdByUserIdAsync(30, It.IsAny<CancellationToken>()))
                .ReturnsAsync(2);
            _svc.Setup(s => s.CreateMedicineAsync(
                    It.Is<CreateMedicineDto>(d => d.MedicineName.Trim() == "Amoxicillin"),
                    2,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await ctrl.Create(dto, default);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);
            _svc.VerifyAll();
        }

        // ✅ TC5: Provider=1, " Amoxicillin ", SideEffects=null, Status=null → default Providing
        [Fact(DisplayName = "TC5 - provider=1, ' Amoxicillin ', null, null → default Providing → 200 OK")]
        public async Task TC5_Create_DefaultStatus_Providing()
        {
            var user = MakeUser(40);
            var ctrl = NewController(user);
            var dto = new CreateMedicineDto { MedicineName = " Amoxicillin ", SideEffects = null, Status = null };

            _svc.Setup(s => s.GetProviderIdByUserIdAsync(40, It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _svc.Setup(s => s.CreateMedicineAsync(
                    It.Is<CreateMedicineDto>(d => d.MedicineName.Trim() == "Amoxicillin" && d.Status == "Providing"),
                    1,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await ctrl.Create(dto, default);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);
            _svc.VerifyAll();
        }

        [Fact(DisplayName = "U1 - Provider=1, 'Paracetamol 500', 'Nausea', 'Stopped' → 200 OK")]
        public async Task U1_Update_Success_Paracetamol500_Stopped()
        {
            var user = MakeUser(1);
            var ctrl = NewController(user);
            var dto = new UpdateMedicineDto
            {
                MedicineName = "Paracetamol 500",
                SideEffects = "Nausea",
                Status = "Stopped"
            };

            _svc.Setup(s => s.UpdateMedicineAsync(1, dto, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await ctrl.Update(1, dto, default);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);
            Assert.Contains("Medicine updated successfully", ok.Value!.ToString());
            _svc.VerifyAll();
        }

        [Fact(DisplayName = "U2 - Provider=2, 'Amoxicillin', 'Fever', 'Providing' → 200 OK")]
        public async Task U2_Update_Success_Amoxicillin_Fever_Providing()
        {
            var user = MakeUser(2);
            var ctrl = NewController(user);
            var dto = new UpdateMedicineDto
            {
                MedicineName = "Amoxicillin",
                SideEffects = "Fever",
                Status = "Providing"
            };

            _svc.Setup(s => s.UpdateMedicineAsync(2, dto, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await ctrl.Update(2, dto, default);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);
            Assert.Contains("Medicine updated successfully", ok.Value!.ToString());
            _svc.VerifyAll();
        }

        [Fact(DisplayName = "U3 - Provider=100 (không tồn tại) → 404 NotFound")]
        public async Task U3_Update_Fail_NotFound()
        {
            var user = MakeUser(100);
            var ctrl = NewController(user);
            var dto = new UpdateMedicineDto
            {
                MedicineName = "Amoxicillin",
                SideEffects = "Fever",
                Status = "Providing"
            };

            _svc.Setup(s => s.UpdateMedicineAsync(100, dto, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException("Medicine with ID 100 not found."));

            var result = await ctrl.Update(100, dto, default);
            var notfound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notfound.StatusCode);
            Assert.Contains("not found", notfound.Value!.ToString(), StringComparison.OrdinalIgnoreCase);
            _svc.VerifyAll();
        }

        [Fact(DisplayName = "U4 - Provider=1, MedicineName='' → 409 Conflict (empty name)")]
        public async Task U4_Update_Fail_EmptyName()
        {
            var user = MakeUser(1);
            var ctrl = NewController(user);
            var dto = new UpdateMedicineDto
            {
                MedicineName = "",
                SideEffects = "Nausea",
                Status = "Providing"
            };

            _svc.Setup(s => s.UpdateMedicineAsync(1, dto, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Medicine name cannot be empty or whitespace."));

            var result = await ctrl.Update(1, dto, default);
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, bad.StatusCode);
            Assert.Contains("cannot be empty or whitespace", bad.Value!.ToString(), StringComparison.OrdinalIgnoreCase);
            _svc.VerifyAll();
        }

        [Fact(DisplayName = "U5 - Provider=1, 'Paracetamol 500', null, null → 200 OK (giữ nguyên cũ)")]
        public async Task U5_Update_Success_NullFields_KeepOld()
        {
            var user = MakeUser(1);
            var ctrl = NewController(user);
            var dto = new UpdateMedicineDto
            {
                MedicineName = "Paracetamol 500",
                SideEffects = null,
                Status = null
            };

            _svc.Setup(s => s.UpdateMedicineAsync(1, dto, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await ctrl.Update(1, dto, default);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);
            Assert.Contains("Medicine updated successfully", ok.Value!.ToString());
            _svc.VerifyAll();
        }
    }
}
