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
    Username NVARCHAR(50) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100),
    Phone NVARCHAR(20),
    DOB DATE,
    Gender NVARCHAR(10),
    RoleID INT NOT NULL,
    FOREIGN KEY (RoleID) REFERENCES [Role](RoleID)
);

-- 3. Doctor (extends User)
CREATE TABLE Doctor (
    DoctorID INT PRIMARY KEY,
    UserID INT UNIQUE NOT NULL,
    Specialty NVARCHAR(100) NOT NULL,
    ExperienceYears INT NOT NULL,
    FOREIGN KEY (UserID) REFERENCES [User](UserID)
);

-- 4. Patient (extends User)
CREATE TABLE Patient (
    PatientID INT PRIMARY KEY,
    UserID INT UNIQUE NOT NULL,
    FOREIGN KEY (UserID) REFERENCES [User](UserID)
);

-- 5. PharmacyProvider (extends User)
CREATE TABLE PharmacyProvider (
    ProviderID INT PRIMARY KEY,
    UserID INT UNIQUE NOT NULL,
    Contact NVARCHAR(100),
    FOREIGN KEY (UserID) REFERENCES [User](UserID)
);

-- 6. Room
CREATE TABLE Room (
    RoomID INT PRIMARY KEY IDENTITY,
    RoomName NVARCHAR(50) NOT NULL
);

-- 7. Shift
CREATE TABLE Shift (
    ShiftID INT PRIMARY KEY IDENTITY,
    RoomID INT NOT NULL,
    ShiftType NVARCHAR(20) NOT NULL, -- Morning, Afternoon, Evening
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    FOREIGN KEY (RoomID) REFERENCES Room(RoomID)
);

-- 8. DoctorShift (Doctor register into Shift)
CREATE TABLE DoctorShift (
    DoctorShiftID INT PRIMARY KEY IDENTITY,
    DoctorID INT NOT NULL,
    ShiftID INT NOT NULL,
    Status NVARCHAR(20) DEFAULT 'Confirmed', -- Pending, Confirmed
    FOREIGN KEY (DoctorID) REFERENCES Doctor(DoctorID),
    FOREIGN KEY (ShiftID) REFERENCES Shift(ShiftID)
);

-- 9. Appointment (booking by Patient with Doctor)
CREATE TABLE Appointment (
    AppointmentID INT PRIMARY KEY IDENTITY,
    PatientID INT NOT NULL,
    DoctorID INT NOT NULL,
    AppointmentDate DATETIME NOT NULL,
    RoomID INT NULL,
    Status NVARCHAR(20) DEFAULT 'Pending', -- Pending, Confirmed, Cancelled, Completed
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (PatientID) REFERENCES Patient(PatientID),
    FOREIGN KEY (DoctorID) REFERENCES Doctor(DoctorID),
    FOREIGN KEY (RoomID) REFERENCES Room(RoomID)
);

-- 10. MedicalRecord
CREATE TABLE MedicalRecord (
    RecordID INT PRIMARY KEY IDENTITY,
    AppointmentID INT NOT NULL,
    DoctorNotes NVARCHAR(MAX),
    Diagnosis NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (AppointmentID) REFERENCES Appointment(AppointmentID)
);

-- 11. Medicine
CREATE TABLE Medicine (
    MedicineID INT PRIMARY KEY IDENTITY,
    ProviderID INT NOT NULL,
    MedicineName NVARCHAR(100) NOT NULL,
    SideEffects NVARCHAR(255),
    Status NVARCHAR(20) DEFAULT 'Available', -- Available, Discontinued
    FOREIGN KEY (ProviderID) REFERENCES PharmacyProvider(ProviderID)
);

-- 12. Prescription
CREATE TABLE Prescription (
    PrescriptionID INT PRIMARY KEY IDENTITY,
    RecordID INT NOT NULL,
    DoctorID INT NOT NULL,
    IssuedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (RecordID) REFERENCES MedicalRecord(RecordID),
    FOREIGN KEY (DoctorID) REFERENCES Doctor(DoctorID)
);

-- 13. PrescriptionDetail
CREATE TABLE PrescriptionDetail (
    PrescriptionDetailID INT PRIMARY KEY IDENTITY,
    PrescriptionID INT NOT NULL,
    MedicineID INT NOT NULL,
    Dosage NVARCHAR(50) NOT NULL,
    Frequency NVARCHAR(50) NOT NULL,
    Duration NVARCHAR(50) NOT NULL,
    FOREIGN KEY (PrescriptionID) REFERENCES Prescription(PrescriptionID),
    FOREIGN KEY (MedicineID) REFERENCES Medicine(MedicineID)
);

-- 14. Payment (for MedicalRecord, paid by Patient)
CREATE TABLE Payment (
    PaymentID INT PRIMARY KEY IDENTITY,
    RecordID INT NOT NULL,
    PatientID INT NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    PaymentDate DATETIME DEFAULT GETDATE(),
    Method NVARCHAR(50), -- Cash, Card, Online
    Status NVARCHAR(20) DEFAULT 'Pending',
    FOREIGN KEY (RecordID) REFERENCES MedicalRecord(RecordID),
    FOREIGN KEY (PatientID) REFERENCES Patient(PatientID)
);

-- 15. ChatLog (Patient <-> Receptionist)
CREATE TABLE ChatLog (
    ChatID INT PRIMARY KEY IDENTITY,
    PatientID INT NOT NULL,
    ReceptionistID INT NOT NULL,
    RoomChat NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (PatientID) REFERENCES Patient(PatientID),
    FOREIGN KEY (ReceptionistID) REFERENCES [User](UserID)
);

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

INSERT INTO [User] (Username, PasswordHash, FullName, Email, Phone, DOB, Gender, RoleID)
VALUES
-- Doctor
(N'nguyenvana', N'123456', N'Nguyễn Văn A', N'a.nguyen@diamondhealth.vn', N'0905123456', '1990-05-15', N'Nam', 4),
-- Patient
(N'lethib', N'123456', N'Lê Thị B', N'b.le@diamondhealth.vn', N'0906123456', '1995-03-22', N'Nữ', 2),
-- Receptionist
(N'phamminhc', N'123456', N'Phạm Minh C', N'c.pham@diamondhealth.vn', N'0907123456', '1992-07-10', N'Nam', 3),
-- Pharmacy Provider
(N'votand', N'123456', N'Võ Tấn D', N'd.vo@diamondhealth.vn', N'0908123456', '1988-09-30', N'Nam', 6),
-- Clinic Manager
(N'huynhthie', N'123456', N'Huỳnh Thị E', N'e.huynh@diamondhealth.vn', N'0909123456', '1985-12-05', N'Nữ', 7),
-- Nurse
(N'tranthif', N'123456', N'Trần Thị F', N'f.tran@diamondhealth.vn', N'0910123456', '1993-08-18', N'Nữ', 5),
-- Administrator
(N'dangquocg', N'admin@123', N'Đặng Quốc G', N'g.dang@diamondhealth.vn', N'0911123456', '1980-01-20', N'Nam', 8);
GO

--1.Doctor (liên kết UserID 1)
INSERT INTO Doctor (DoctorID, UserID, Specialty, ExperienceYears)
VALUES 
(1, 1, N'Tim mạch', 10);
GO

--2.Patient (UserID 2)
INSERT INTO Patient (PatientID, UserID)
VALUES
(1, 2);
GO

--3.Pharmacy Provider (UserID 4)
INSERT INTO PharmacyProvider (ProviderID, UserID, Contact)
VALUES
(1, 4, N'Nhà thuốc Minh Châu - 25 Lý Thường Kiệt, Hà Nội');
GO

--4.Room
INSERT INTO Room (RoomName)
VALUES
(N'Phòng khám tổng quát 101'),
(N'Phòng tim mạch 202');
GO


--5.Shift
INSERT INTO Shift (RoomID, ShiftType, StartTime, EndTime)
VALUES
(2, N'Sáng', '08:00', '12:00'),
(2, N'Chiều', '13:30', '17:30');
GO

--6.DoctorShift
INSERT INTO DoctorShift (DoctorID, ShiftID, Status)
VALUES
(1, 1, N'Confirmed'),
(1, 2, N'Confirmed');
GO

--7.Appointment
INSERT INTO Appointment (PatientID, DoctorID, AppointmentDate, RoomID, Status)
VALUES
(1, 1, '2025-10-10 09:00', 2, N'Confirmed');
GO

--8.MedicalRecord
INSERT INTO MedicalRecord (AppointmentID, DoctorNotes, Diagnosis)
VALUES
(1, N'Bệnh nhân có triệu chứng đau ngực nhẹ, huyết áp hơi cao.', N'Tăng huyết áp độ 1');
GO

--9.Medicine
INSERT INTO Medicine (ProviderID, MedicineName, SideEffects, Status)
VALUES
(1, N'Paracetamol 500mg', N'Buồn ngủ, mệt nhẹ', N'Available'),
(1, N'Amlodipine 5mg', N'Phù chân, nhức đầu', N'Available');
GO

--10.Prescription
INSERT INTO Prescription (RecordID, DoctorID)
VALUES
(1, 1);
GO

--11.PrescriptionDetail
INSERT INTO PrescriptionDetail (PrescriptionID, MedicineID, Dosage, Frequency, Duration)
VALUES
(1, 1, N'1 viên', N'3 lần/ngày', N'5 ngày'),
(1, 2, N'1 viên', N'1 lần/ngày', N'7 ngày');
GO

--12.Payment
INSERT INTO Payment (RecordID, PatientID, Amount, Method, Status)
VALUES
(1, 1, 350000.00, N'Tiền mặt', N'Paid');
GO


