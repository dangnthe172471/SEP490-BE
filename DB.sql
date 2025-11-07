CREATE DATABASE DiamondHealth;
GO

USE DiamondHealth;
GO

-- 1. Role
CREATE TABLE [Role] (
    RoleID INT PRIMARY KEY IDENTITY,
    RoleName NVARCHAR(30) NOT NULL UNIQUE -- Patient, Doctor, Receptionist, PharmacyProvider, Manager, etc.
);

-- 2. User (All actors)
CREATE TABLE [User] (
    UserID INT PRIMARY KEY IDENTITY,
    Phone NVARCHAR(20) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100),
    DOB DATE,
    Gender NVARCHAR(10),
    RoleID INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (RoleID) REFERENCES [Role](RoleID)
);

-- 3. Doctor (extends User)
CREATE TABLE Doctor (
    DoctorID INT PRIMARY KEY,
    UserID INT UNIQUE NOT NULL,
    Specialty NVARCHAR(100) NOT NULL,
    ExperienceYears INT NOT NULL,
    RoomID INT NOT NULL,
    FOREIGN KEY (UserID) REFERENCES [User](UserID)
);

-- 4. Patient (extends User)
CREATE TABLE Patient (
    PatientID INT PRIMARY KEY,
    UserID INT UNIQUE NOT NULL,
    Allergies NVARCHAR(500) NULL,
    MedicalHistory NVARCHAR(500) NULL,
    FOREIGN KEY (UserID) REFERENCES [User](UserID)
);

-- 5. Receptionist (extends User)
CREATE TABLE Receptionist (
    ReceptionistID INT PRIMARY KEY,
    UserID INT UNIQUE NOT NULL,
    FOREIGN KEY (UserID) REFERENCES [User](UserID)
);

-- 6. PharmacyProvider (extends User)
CREATE TABLE PharmacyProvider (
    ProviderID INT PRIMARY KEY,
    UserID INT UNIQUE NOT NULL,
    Contact NVARCHAR(100),
    FOREIGN KEY (UserID) REFERENCES [User](UserID)
);

-- 7. Room
CREATE TABLE Room (
    RoomID INT PRIMARY KEY IDENTITY,
    RoomName NVARCHAR(50) NOT NULL
);

-- Add FK from Doctor to Room after Room is created
ALTER TABLE Doctor
ADD CONSTRAINT FK_Doctor_Room FOREIGN KEY (RoomID) REFERENCES Room(RoomID);

-- 8. Shift
CREATE TABLE Shift (
    ShiftID INT PRIMARY KEY IDENTITY,
    ShiftType NVARCHAR(20) NOT NULL, -- Morning, Afternoon, Evening
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL
);

-- 9. DoctorShift (Lịch làm việc cố định của bác sĩ)
CREATE TABLE DoctorShift (
    DoctorShiftID INT PRIMARY KEY IDENTITY,
    DoctorID INT NOT NULL,
    ShiftID INT NOT NULL,
    EffectiveFrom DATE NOT NULL,    -- Ngày bắt đầu có hiệu lực
    EffectiveTo DATE NULL,          -- Ngày kết thúc (NULL = vĩnh viễn)
    Status NVARCHAR(255) NULL DEFAULT 'Active',       -- Ghi chú
    FOREIGN KEY (DoctorID) REFERENCES Doctor(DoctorID),
    FOREIGN KEY (ShiftID) REFERENCES Shift(ShiftID)
);

CREATE TABLE DoctorShiftExchange (
    ExchangeID INT PRIMARY KEY IDENTITY,
    Doctor1ID INT NOT NULL,                   -- Bác sĩ 1
    Doctor1ShiftRefID INT NOT NULL, 
	DoctorOld1ShiftID INT NOT NULL,
    Doctor2ID INT NULL,                       -- Bác sĩ 2 (NULL = nghỉ phép)
    Doctor2ShiftRefID INT NULL,  
	DoctorOld2ShiftID INT NULL,
    ExchangeDate DATE NULL,					  -- Ngày đổi ca/nghỉ
    Status NVARCHAR(20) DEFAULT 'Pending',   -- Pending, Approved, Rejected
    FOREIGN KEY (Doctor1ID) REFERENCES Doctor(DoctorID),
    FOREIGN KEY (Doctor1ShiftRefID) REFERENCES DoctorShift(DoctorShiftID),
    FOREIGN KEY (Doctor2ID) REFERENCES Doctor(DoctorID),
    FOREIGN KEY (Doctor2ShiftRefID) REFERENCES DoctorShift(DoctorShiftID)
);

-- 10. Appointment (booking by Patient with Doctor)
CREATE TABLE Appointment (
    AppointmentID INT PRIMARY KEY IDENTITY,
    PatientID INT NOT NULL,
    DoctorID INT NOT NULL,
    AppointmentDate DATETIME NOT NULL,
    ReceptionistID INT NULL,
    UpdatedBy INT NULL,
    Status NVARCHAR(20) DEFAULT 'Pending', -- Pending, Confirmed, Cancelled, Completed
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (PatientID) REFERENCES Patient(PatientID),
    FOREIGN KEY (DoctorID) REFERENCES Doctor(DoctorID),
    FOREIGN KEY (ReceptionistID) REFERENCES Receptionist(ReceptionistID),
    FOREIGN KEY (UpdatedBy) REFERENCES [User](UserID)
);

-- 11. MedicalRecord
CREATE TABLE MedicalRecord (
    RecordID INT PRIMARY KEY IDENTITY,
    AppointmentID INT NOT NULL,
    DoctorNotes NVARCHAR(MAX),
    Diagnosis NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (AppointmentID) REFERENCES Appointment(AppointmentID)
);

-- 12 ObstetricRecord (sản phụ)
CREATE TABLE ObstetricRecord (
    RecordID INT PRIMARY KEY,
    Gravida INT NULL,
    Para INT NULL,
    LMPDate DATE NULL,
    GestationalAgeWeeks INT NULL,
    FetalHeartRateBPM INT NULL,
    ExpectedDueDate DATE NULL,
    ComplicationsNotes NVARCHAR(255) NULL,
    FOREIGN KEY (RecordID) REFERENCES MedicalRecord(RecordID)
);

-- 13 PediatricRecord (nhi khoa)
CREATE TABLE PediatricRecord (
    RecordID INT PRIMARY KEY,
    WeightKg DECIMAL(5,2) NULL,
    HeightCm DECIMAL(5,2) NULL,
    HeartRate INT NULL,
    TemperatureC DECIMAL(4,1) NULL,
    FOREIGN KEY (RecordID) REFERENCES MedicalRecord(RecordID)
);

-- 14 InternalMedRecord (nội khoa)
CREATE TABLE InternalMedRecord (
    RecordID INT PRIMARY KEY,
    BloodPressure INT NULL,
    HeartRate INT NULL,
    BloodSugar DECIMAL(6,2) NULL,
    Notes NVARCHAR(255) NULL,
    FOREIGN KEY (RecordID) REFERENCES MedicalRecord(RecordID)
);

-- 15 TestType (catalog of lab tests)
CREATE TABLE TestType (
    TestTypeID INT PRIMARY KEY IDENTITY,
    TestName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255) NULL
);

-- 16 TestResult (results linked to MedicalRecord and TestType)
CREATE TABLE TestResult (
    TestResultID INT PRIMARY KEY IDENTITY,
    RecordID INT NOT NULL,
    TestTypeID INT NOT NULL,
    ResultValue NVARCHAR(100) NOT NULL,
    Unit NVARCHAR(50) NULL,
    Attachment NVARCHAR(255) NULL,
    ResultDate DATETIME DEFAULT GETDATE(),
    Notes NVARCHAR(255) NULL,
    FOREIGN KEY (RecordID) REFERENCES MedicalRecord(RecordID),
    FOREIGN KEY (TestTypeID) REFERENCES TestType(TestTypeID)
);

-- 17. Medicine
CREATE TABLE Medicine (
    MedicineID INT PRIMARY KEY IDENTITY,
    ProviderID INT NOT NULL,
    MedicineName NVARCHAR(100) NOT NULL,
    SideEffects NVARCHAR(255),
    Status NVARCHAR(20) DEFAULT 'Available', -- Available, Discontinued
    FOREIGN KEY (ProviderID) REFERENCES PharmacyProvider(ProviderID)
);

-- 18. Prescription
CREATE TABLE Prescription (
    PrescriptionID INT PRIMARY KEY IDENTITY,
    RecordID INT NOT NULL,
    DoctorID INT NOT NULL,
    IssuedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (RecordID) REFERENCES MedicalRecord(RecordID),
    FOREIGN KEY (DoctorID) REFERENCES Doctor(DoctorID)
);

-- 19. PrescriptionDetail
CREATE TABLE PrescriptionDetail (
    PrescriptionDetailID INT PRIMARY KEY IDENTITY,
    PrescriptionID INT NOT NULL,
    MedicineID INT NOT NULL,
    Dosage NVARCHAR(100) NOT NULL,
    Duration NVARCHAR(50) NOT NULL,
    FOREIGN KEY (PrescriptionID) REFERENCES Prescription(PrescriptionID),
    FOREIGN KEY (MedicineID) REFERENCES Medicine(MedicineID)
);

-- 20. Payment (for MedicalRecord)
CREATE TABLE Payment (
    PaymentID INT PRIMARY KEY IDENTITY,
    RecordID INT NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    PaymentDate DATETIME DEFAULT GETDATE(),
    Method NVARCHAR(50), -- Cash, Card, Online
    Status NVARCHAR(20) DEFAULT 'Pending',
    FOREIGN KEY (RecordID) REFERENCES MedicalRecord(RecordID)
);

-- 21. ChatLog (Patient <-> Receptionist)
CREATE TABLE ChatLog (
    ChatID INT PRIMARY KEY IDENTITY,
    PatientID INT NOT NULL,
    ReceptionistID INT NOT NULL,
    RoomChat NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (PatientID) REFERENCES Patient(PatientID),
    FOREIGN KEY (ReceptionistID) REFERENCES Receptionist(ReceptionistID)
);
Go
-- Thêm cột SwapType vào bảng DoctorShiftExchange
ALTER TABLE DoctorShiftExchange 
ADD SwapType NVARCHAR(20) DEFAULT 'Temporary';
Go
ALTER TABLE [dbo].[Appointment]
ADD [ReasonForVisit] NVARCHAR(500) NULL;
Go
ALTER TABLE [dbo].[User]
ADD [Avatar] NVARCHAR(500) NULL;
GO
CREATE TABLE [dbo].[Notification] (
    [NotificationId] INT IDENTITY(1,1) PRIMARY KEY,
    [Title] NVARCHAR(200) NOT NULL,             
    [Content] NVARCHAR(MAX) NOT NULL,             
    [Type] NVARCHAR(50) NOT NULL,                 
    [CreatedBy] INT NULL,                         
    [CreatedDate] DATETIME NOT NULL DEFAULT(GETDATE()),
    [IsGlobal] BIT NOT NULL DEFAULT(0),          
    [IsEmailSent] BIT NOT NULL DEFAULT(0),           
    CONSTRAINT FK_Notification_User FOREIGN KEY ([CreatedBy]) 
        REFERENCES [dbo].[User]([UserId]) ON DELETE SET NULL
);
GO


CREATE TABLE [dbo].[NotificationReceiver] (
    [NotificationId] INT NOT NULL,                    -- FK → Notification
    [ReceiverId] INT NOT NULL,                        -- FK → User
    [IsRead] BIT NOT NULL DEFAULT(0),                 -- 0 = chưa đọc, 1 = đã đọc
    [ReadDate] DATETIME NULL,                         -- Thời điểm đọc
    CONSTRAINT PK_NotificationReceiver PRIMARY KEY ([NotificationId], [ReceiverId]),
    CONSTRAINT FK_NotificationReceiver_Notification FOREIGN KEY ([NotificationId]) 
        REFERENCES [dbo].[Notification]([NotificationId]) ON DELETE CASCADE,
    CONSTRAINT FK_NotificationReceiver_User FOREIGN KEY ([ReceiverId]) 
        REFERENCES [dbo].[User]([UserId]) ON DELETE CASCADE
);
GO
--------------------------------------------------------------------------------------------------------------------------------------------------

-- Thêm các Role theo đúng thứ tự yêu cầu
INSERT INTO Role (RoleName)
VALUES
(N'Guest'),
(N'Patient'),
(N'Receptionist'),
(N'Doctor'),
(N'Nurse'),
(N'Pharmacy Provider'),
(N'Clinic Manager'),
(N'Administrator');
GO

-- Insert User với RoleID đúng theo bảng Role
-- Role thứ tự:
-- 1 Guest | 2 Patient | 3 Receptionist | 4 Doctor | 5 Nurse | 6 Pharmacy Provider | 7 Clinic Manager | 8 Administrator

INSERT INTO [User] (Phone, PasswordHash, FullName, Email, DOB, Gender, RoleID)
VALUES
-- Doctor
(N'0905123456', N'$2a$11$uP69F9o4TwZP9ftmztyzB.oH/HCDKLCWNmAveQv.2rlKx.nfhcrIW', N'Nguyễn Văn A', N'a.nguyen@diamondhealth.vn', '1990-05-15', N'Nam', 4),
-- Patient
(N'0906123456', N'$2a$11$uP69F9o4TwZP9ftmztyzB.oH/HCDKLCWNmAveQv.2rlKx.nfhcrIW', N'Lê Thị B', N'b.le@diamondhealth.vn', '1995-03-22', N'Nữ', 2),
-- Receptionist
(N'0907123456', N'$2a$11$uP69F9o4TwZP9ftmztyzB.oH/HCDKLCWNmAveQv.2rlKx.nfhcrIW', N'Phạm Minh C', N'c.pham@diamondhealth.vn', '1992-07-10', N'Nam', 3),
-- Pharmacy Provider
(N'0908123456', N'$2a$11$uP69F9o4TwZP9ftmztyzB.oH/HCDKLCWNmAveQv.2rlKx.nfhcrIW', N'Võ Tấn D', N'd.vo@diamondhealth.vn', '1988-09-30', N'Nam', 6),
-- Clinic Manager
(N'0909123456', N'$2a$11$uP69F9o4TwZP9ftmztyzB.oH/HCDKLCWNmAveQv.2rlKx.nfhcrIW', N'Huỳnh Thị E', N'e.huynh@diamondhealth.vn', '1985-12-05', N'Nữ', 7),
-- Nurse
(N'0910123456', N'$2a$11$uP69F9o4TwZP9ftmztyzB.oH/HCDKLCWNmAveQv.2rlKx.nfhcrIW', N'Trần Thị F', N'f.tran@diamondhealth.vn', '1993-08-18', N'Nữ', 5),
-- Administrator
(N'0911123456', N'$2a$11$uP69F9o4TwZP9ftmztyzB.oH/HCDKLCWNmAveQv.2rlKx.nfhcrIW', N'Đặng Quốc G', N'g.dang@diamondhealth.vn', '1980-01-20', N'Nam', 8);
GO

--Receptionist (UserID 3)
INSERT INTO Receptionist (ReceptionistID, UserID)
VALUES
(1, 3);
GO

--Room (moved earlier to satisfy Doctor.RoomID FK)
INSERT INTO Room (RoomName)
VALUES
(N'Phòng khám tổng quát 101'),
(N'Phòng tim mạch 202');
GO

--Doctor (liên kết UserID 1)
INSERT INTO Doctor (DoctorID, UserID, Specialty, ExperienceYears, RoomID)
VALUES 
(1, 1, N'Sản phụ khoa', 10, 2);
GO

--Patient (UserID 2)
INSERT INTO Patient (PatientID, UserID, Allergies, MedicalHistory)
VALUES
(1, 2, N'Dị ứng với penicillin', N'Tiền sử tăng huyết áp nhẹ');
GO

--Pharmacy Provider (UserID 4)
INSERT INTO PharmacyProvider (ProviderID, UserID, Contact)
VALUES
(1, 4, N'Nhà thuốc Minh Châu - 25 Lý Thường Kiệt, Hà Nội');
GO

--Shift
INSERT INTO Shift (ShiftType, StartTime, EndTime)
VALUES
(N'Sáng', '08:00', '12:00'),
(N'Chiều', '13:30', '17:30'),
(N'Tối', '18:00', '22:00');
GO

-- Thêm thêm bác sĩ để demo
INSERT INTO [User] (Phone, PasswordHash, FullName, Email, DOB, Gender, RoleID)
VALUES
(N'0905123457', N'$2a$11$uP69F9o4TwZP9ftmztyzB.oH/HCDKLCWNmAveQv.2rlKx.nfhcrIW', N'Nguyễn Thị H', N'h.nguyen@diamondhealth.vn', '1988-08-20', N'Nữ', 4),
(N'0905123458', N'$2a$11$uP69F9o4TwZP9ftmztyzB.oH/HCDKLCWNmAveQv.2rlKx.nfhcrIW', N'Trần Văn I', N'i.tran@diamondhealth.vn', '1992-03-15', N'Nam', 4),
(N'0905123459', N'$2a$11$uP69F9o4TwZP9ftmztyzB.oH/HCDKLCWNmAveQv.2rlKx.nfhcrIW', N'Lê Thị J', N'j.le@diamondhealth.vn', '1985-11-10', N'Nữ', 4);

-- Thêm thêm bệnh nhân để demo
INSERT INTO [User] (Phone, PasswordHash, FullName, Email, DOB, Gender, RoleID)
VALUES
(N'0906123458', N'$2a$11$uP69F9o4TwZP9ftmztyzB.oH/HCDKLCWNmAveQv.2rlKx.nfhcrIW', N'Phạm Văn K', N'k.pham@email.com', '1990-06-15', N'Nam', 2),
(N'0906123459', N'$2a$11$uP69F9o4TwZP9ftmztyzB.oH/HCDKLCWNmAveQv.2rlKx.nfhcrIW', N'Hoàng Thị L', N'l.hoang@email.com', '1987-12-03', N'Nữ', 2);
GO

INSERT INTO Doctor (DoctorID, UserID, Specialty, ExperienceYears, RoomID)
VALUES 
(2, 8, N'Nội khoa', 8, 1),
(3, 9, N'Da liễu', 6, 1),
(4, 10, N'Sản phụ khoa', 12, 2);

-- Thêm data cho các bệnh nhân mới
INSERT INTO Patient (PatientID, UserID, Allergies, MedicalHistory)
VALUES
(2, 11, N'Không có', N'Không có bệnh lý nền'),
(3, 12, N'Dị ứng với hải sản', N'Tiền sử đau dạ dày');
GO

-- Thêm phòng khám
INSERT INTO Room (RoomName)
VALUES
(N'Phòng khám tổng quát 102'),
(N'Phòng tim mạch 203');
GO

--DoctorShift - Lịch làm việc cố định (đơn giản)
INSERT INTO DoctorShift (DoctorID, ShiftID, EffectiveFrom, EffectiveTo)
VALUES
-- Bác sĩ 1: Ca sáng, ca chiều
(1, 1, '2025-11-01', '2025-12-01'),
(1, 2, '2025-11-01', '2025-12-01'), 

(1, 1, '2025-12-01', '2026-01-01'),
(1, 2, '2025-12-01', '2026-01-01'), 

-- Bác sĩ 2: Ca sáng, ca tối
(2, 1, '2025-11-01', '2025-12-01'),
(2, 3, '2025-11-01', '2025-12-01'),

-- Bác sĩ 3: Ca chiều, ca tối
(3, 2, '2025-01-01', '2026-01-01'),
(3, 3, '2025-01-01', '2026-01-01'),

(4, 3, '2025-11-01','2025-12-01'),
(4, 3, '2025-12-01','2026-01-01');

GO

--Appointment
INSERT INTO Appointment (PatientID, DoctorID, AppointmentDate, ReceptionistID, UpdatedBy, Status)
VALUES
(1, 1, '2025-10-10 09:00', 1, 3, N'Confirmed');
GO

--MedicalRecord
INSERT INTO MedicalRecord (AppointmentID, DoctorNotes, Diagnosis)
VALUES
(1, N'Bệnh nhân có triệu chứng đau ngực nhẹ, huyết áp hơi cao.', N'Tăng huyết áp độ 1');
GO

--Specialty extensions sample
-- Link RecordID=1 as internal medicine example
INSERT INTO InternalMedRecord (RecordID, BloodPressure, HeartRate, BloodSugar, Notes)
VALUES (1, 135, 78, NULL, N'Khám tổng quát - theo dõi huyết áp');
GO

--Medicine
INSERT INTO Medicine (ProviderID, MedicineName, SideEffects, Status)
VALUES
(1, N'Paracetamol 500mg', N'Buồn ngủ, mệt nhẹ', N'Available'),
(1, N'Amlodipine 5mg', N'Phù chân, nhức đầu', N'Available');
GO

--Prescription
INSERT INTO Prescription (RecordID, DoctorID)
VALUES
(1, 1);
GO

--PrescriptionDetail
INSERT INTO PrescriptionDetail (PrescriptionID, MedicineID, Dosage, Duration)
VALUES
(1, 1, N'1 viên - 3 lần/ngày', N'5 ngày'),
(1, 2, N'1 viên - 1 lần/ngày', N'7 ngày');
GO

--TestResult
-- Seed TestType
INSERT INTO TestType (TestName, Description)
VALUES
(N'Huyết áp tâm thu', N'Huyết áp tâm thu tiêu chuẩn'),
(N'Huyết áp tâm trương', N'Huyết áp tâm trương tiêu chuẩn');
GO

-- Seed TestResult
INSERT INTO TestResult (RecordID, TestTypeID, ResultValue, Notes)
VALUES
(1, 1, N'135', N'Nhẹ cao'),
(1, 2, N'88', N'Cao nhẹ');
GO

--Payment
INSERT INTO Payment (RecordID, Amount, Method, Status)
VALUES
(1, 350000.00, N'Tiền mặt', N'Paid');
GO