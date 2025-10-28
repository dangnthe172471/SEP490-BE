using FluentAssertions;
using Moq;
using SEP490_BE.BLL.Services;
using SEP490_BE.DAL.DTOs.MedicineDTO;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.Tests.Services
{
    public class MedicineServiceTests
    {
        private readonly Mock<IMedicineRepository> _repo = new(MockBehavior.Strict);
        private MedicineService NewService() => new(_repo.Object);

        // Test 1: 1,"Paracetamol","Nausea","Providing"  -> success
        [Fact(DisplayName = "T1 - provider=1, name=Paracetamol, SE=Nausea, Status=Providing -> success")]
        public async Task T1_Create_Provider1_Paracetamol_Success()
        {
            var svc = NewService();
            var dto = new CreateMedicineDto { MedicineName = "Paracetamol", SideEffects = "Nausea", Status = "Providing" };
            int providerId = 1;

            _repo.Setup(r => r.CreateMedicineAsync(
                    It.Is<Medicine>(m =>
                        m.ProviderId == providerId &&
                        m.MedicineName == "Paracetamol" &&
                        m.SideEffects == "Nausea" &&
                        m.Status == "Providing"
                    ),
                    It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

            await svc.CreateMedicineAsync(dto, providerId, CancellationToken.None);

            _repo.Verify(r => r.CreateMedicineAsync(It.IsAny<Medicine>(), It.IsAny<CancellationToken>()), Times.Once);
            _repo.VerifyNoOtherCalls();
        }

        // Test 2: 2,"Paracetamol","Nausea","Providing"  -> success (khác provider)
        [Fact(DisplayName = "T2 - provider=2, name=Paracetamol, SE=Nausea, Status=Providing -> success")]
        public async Task T2_Create_Provider2_SameName_Success()
        {
            var svc = NewService();
            var dto = new CreateMedicineDto { MedicineName = "Paracetamol", SideEffects = "Nausea", Status = "Providing" };
            int providerId = 2;

            _repo.Setup(r => r.CreateMedicineAsync(
                    It.Is<Medicine>(m =>
                        m.ProviderId == providerId &&
                        m.MedicineName == "Paracetamol" &&
                        m.SideEffects == "Nausea" &&
                        m.Status == "Providing"
                    ),
                    It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

            await svc.CreateMedicineAsync(dto, providerId, CancellationToken.None);

            _repo.Verify(r => r.CreateMedicineAsync(It.IsAny<Medicine>(), It.IsAny<CancellationToken>()), Times.Once);
            _repo.VerifyNoOtherCalls();
        }

        // Test 3: 1,"Paracetamol","Nausea","Providing"  -> giả lập duplicate tại Repo (throw InvalidOperationException)
        [Fact(DisplayName = "T3 - provider=1, name=Paracetamol duplicate -> repo throws InvalidOperationException")]
        public async Task T3_Create_Provider1_Paracetamol_Duplicate_Conflict()
        {
            var svc = NewService();
            var dto = new CreateMedicineDto { MedicineName = "Paracetamol", SideEffects = "Nausea", Status = "Providing" };
            int providerId = 1;

            _repo.Setup(r => r.CreateMedicineAsync(
                    It.Is<Medicine>(m =>
                        m.ProviderId == providerId &&
                        m.MedicineName == "Paracetamol" &&
                        m.SideEffects == "Nausea" &&
                        m.Status == "Providing"
                    ),
                    It.IsAny<CancellationToken>()))
                 .ThrowsAsync(new InvalidOperationException("Medicine 'Paracetamol' already exists for this provider."));

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                svc.CreateMedicineAsync(dto, providerId, CancellationToken.None));

            Assert.Contains("already exists", ex.Message, StringComparison.OrdinalIgnoreCase);
            _repo.Verify(r => r.CreateMedicineAsync(It.IsAny<Medicine>(), It.IsAny<CancellationToken>()), Times.Once);
            _repo.VerifyNoOtherCalls();
        }

        // Test 4: 2," Amoxillin ","Fever","Providing" -> success (trim name)
        [Fact(DisplayName = "T4 - provider=2, name=' Amoxillin ' (trim), SE=Fever, Status=Providing -> success")]
        public async Task T4_Create_TrimName_Success()
        {
            var svc = NewService();
            var dto = new CreateMedicineDto { MedicineName = " Amoxillin ", SideEffects = "Fever", Status = "Providing" };
            int providerId = 2;

            _repo.Setup(r => r.CreateMedicineAsync(
                    It.Is<Medicine>(m =>
                        m.ProviderId == providerId &&
                        m.MedicineName == "Amoxillin" &&           // đã Trim
                        m.SideEffects == "Fever" &&
                        m.Status == "Providing"
                    ),
                    It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

            await svc.CreateMedicineAsync(dto, providerId, CancellationToken.None);

            _repo.Verify(r => r.CreateMedicineAsync(It.IsAny<Medicine>(), It.IsAny<CancellationToken>()), Times.Once);
            _repo.VerifyNoOtherCalls();
        }

        // Test 5: 1," Amoxillin ", null, null -> success (trim name, Status default = Providing)
        [Fact(DisplayName = "T5 - provider=1, name=' Amoxillin ' (trim), SE=null, Status=null -> default Providing")]
        public async Task T5_Create_DefaultStatus_And_TrimName_Success()
        {
            var svc = NewService();
            var dto = new CreateMedicineDto { MedicineName = " Amoxillin ", SideEffects = null, Status = null };
            int providerId = 1;

            _repo.Setup(r => r.CreateMedicineAsync(
                    It.Is<Medicine>(m =>
                        m.ProviderId == providerId &&
                        m.MedicineName == "Amoxillin" &&           // đã Trim
                        m.SideEffects == null &&
                        m.Status == "Providing"                     // default do Status null
                    ),
                    It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

            await svc.CreateMedicineAsync(dto, providerId, CancellationToken.None);

            _repo.Verify(r => r.CreateMedicineAsync(It.IsAny<Medicine>(), It.IsAny<CancellationToken>()), Times.Once);
            _repo.VerifyNoOtherCalls();
        }

        [Theory(DisplayName = "T6 - Invalid name (null/empty/whitespace) → throws InvalidOperationException")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task T6_Create_InvalidName_Throws(string badName)
        {
            var svc = NewService();
            var dto = new CreateMedicineDto { MedicineName = badName, SideEffects = "X", Status = "Providing" };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                svc.CreateMedicineAsync(dto, 1, CancellationToken.None));

            Assert.Equal("Medicine name is required.", ex.Message);
            _repo.VerifyNoOtherCalls(); // Repo không được gọi
        }

        // ===================== UPDATE TESTS (Service) =====================

        [Fact(DisplayName = "U1 - id=10, name='Paracetamol 500', SE='Nausea', Status='Stopped' -> success")]
        public async Task Update_U1_Success_TrimName_And_Update_All()
        {
            var svc = NewService();
            var id = 10;

            // existing trong DB
            var existing = new Medicine
            {
                MedicineId = id,
                ProviderId = 1,
                MedicineName = "Paracetamol",
                SideEffects = "OldSE",
                Status = "Providing"
            };

            var dto = new UpdateMedicineDto
            {
                MedicineName = "  Paracetamol 500  ",
                SideEffects = "Nausea",
                Status = "Stopped"
            };

            _repo.Setup(r => r.GetMedicineByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

            _repo.Setup(r => r.UpdateMedicineAsync(
                        It.Is<Medicine>(m =>
                            m.MedicineId == id &&
                            m.ProviderId == 1 &&                         // Provider giữ nguyên
                            m.MedicineName == "Paracetamol 500" &&       // đã Trim
                            m.SideEffects == "Nausea" &&
                            m.Status == "Stopped"
                        ),
                        It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

            await svc.UpdateMedicineAsync(id, dto, CancellationToken.None);

            _repo.VerifyAll();
        }

        [Fact(DisplayName = "U2 - id=20, name='Paracetamol 500', SE=null, Status=null -> success (giữ SE/Status cũ)")]
        public async Task Update_U2_Success_NameOnly()
        {
            var svc = NewService();
            var id = 20;

            var existing = new Medicine
            {
                MedicineId = id,
                ProviderId = 2,
                MedicineName = "Paracetamol",
                SideEffects = "OldSE",
                Status = "Providing"
            };

            var dto = new UpdateMedicineDto
            {
                MedicineName = "Paracetamol 500",
                SideEffects = null,         // giữ nguyên
                Status = null               // giữ nguyên
            };

            _repo.Setup(r => r.GetMedicineByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

            _repo.Setup(r => r.UpdateMedicineAsync(
                        It.Is<Medicine>(m =>
                            m.MedicineId == id &&
                            m.ProviderId == 2 &&
                            m.MedicineName == "Paracetamol 500" &&
                            m.SideEffects == "OldSE" &&                 // giữ nguyên
                            m.Status == "Providing"                     // giữ nguyên
                        ),
                        It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

            await svc.UpdateMedicineAsync(id, dto, CancellationToken.None);

            _repo.VerifyAll();
        }

        [Fact(DisplayName = "U3 - id=30, name='' (rỗng) -> ArgumentException (không gọi repo.Update)")]
        public async Task Update_U3_Fail_EmptyName()
        {
            var svc = NewService();
            var id = 30;

            // vẫn phải tìm thấy existing để qua được kiểm tra null
            _repo.Setup(r => r.GetMedicineByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new Medicine
                 {
                     MedicineId = id,
                     ProviderId = 1,
                     MedicineName = "Any",
                     SideEffects = "Old",
                     Status = "Providing"
                 });

            var dto = new UpdateMedicineDto
            {
                MedicineName = "",      // rỗng -> lỗi
                SideEffects = "Nausea",
                Status = "Providing"
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                svc.UpdateMedicineAsync(id, dto, CancellationToken.None));

            Assert.Contains("cannot be empty or whitespace", ex.Message, StringComparison.OrdinalIgnoreCase);

            // không được gọi Update
            _repo.Verify(r => r.UpdateMedicineAsync(It.IsAny<Medicine>(), It.IsAny<CancellationToken>()), Times.Never);
            _repo.Verify(r => r.GetMedicineByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
            _repo.VerifyNoOtherCalls();
        }

        [Fact(DisplayName = "U4 - id=40, repo báo đổi ProviderId -> InvalidOperationException (bubbles up)")]
        public async Task Update_U4_Fail_ChangeProvider_BubbledFromRepo()
        {
            var svc = NewService();
            var id = 40;

            // existing ProviderId=1
            var existing = new Medicine
            {
                MedicineId = id,
                ProviderId = 1,
                MedicineName = "OldName",
                SideEffects = "OldSE",
                Status = "Providing"
            };

            var dto = new UpdateMedicineDto
            {
                MedicineName = " Amoxicillin ",
                SideEffects = "Fever",
                Status = "Providing"
            };

            _repo.Setup(r => r.GetMedicineByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

            // Service không đổi ProviderId, nhưng ta giả lập repo phát hiện lệch (hoặc chính sách khác)
            _repo.Setup(r => r.UpdateMedicineAsync(It.IsAny<Medicine>(), It.IsAny<CancellationToken>()))
                 .ThrowsAsync(new InvalidOperationException("Changing Provider is not allowed."));

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                svc.UpdateMedicineAsync(id, dto, CancellationToken.None));

            Assert.Contains("Changing Provider", ex.Message);

            _repo.Verify(r => r.GetMedicineByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
            _repo.Verify(r => r.UpdateMedicineAsync(It.Is<Medicine>(m =>
                m.MedicineId == id &&
                m.ProviderId == 1 &&                       // vẫn 1 (service không đổi)
                m.MedicineName == "Amoxicillin" &&         // Trim
                m.SideEffects == "Fever" &&
                m.Status == "Providing"
            ), It.IsAny<CancellationToken>()), Times.Once);
            _repo.VerifyNoOtherCalls();
        }

        [Fact(DisplayName = "U5 - id=50, name=' Amoxicillin ', SE=null, Status='Stopped' -> success (trim & giữ SE cũ)")]
        public async Task Update_U5_Success_Trim_Stopped()
        {
            var svc = NewService();
            var id = 50;

            var existing = new Medicine
            {
                MedicineId = id,
                ProviderId = 1,
                MedicineName = "OldName",
                SideEffects = "OldSE",
                Status = "Providing"
            };

            var dto = new UpdateMedicineDto
            {
                MedicineName = " Amoxicillin ",
                SideEffects = null,      // giữ SE cũ
                Status = "Stopped"
            };

            _repo.Setup(r => r.GetMedicineByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

            _repo.Setup(r => r.UpdateMedicineAsync(
                        It.Is<Medicine>(m =>
                            m.MedicineId == id &&
                            m.ProviderId == 1 &&
                            m.MedicineName == "Amoxicillin" &&           // Trim
                            m.SideEffects == "OldSE" &&                  // giữ cũ
                            m.Status == "Stopped"
                        ),
                        It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

            await svc.UpdateMedicineAsync(id, dto, CancellationToken.None);

            _repo.VerifyAll();
        }

    }
}
