CREATE DATABASE DiamondHealth;
GO

USE DiamondHealth;
GO

-- 1. User (All actors)
CREATE TABLE [User] (
    UserID INT PRIMARY KEY IDENTITY,
    Username NVARCHAR(50) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NULL,
    Phone NVARCHAR(20) NULL,
    DOB DATE NULL,
    Gender NVARCHAR(10) NULL,
    Role NVARCHAR(30) NOT NULL, -- Patient, Doctor, Receptionist, etc.
);

-- 2. Doctor (extends User)
CREATE TABLE Doctor (
    DoctorID INT PRIMARY KEY,
    UserID INT UNIQUE NOT NULL,
    Specialty NVARCHAR(100) NOT NULL,
    ExperienceYears INT NOT NULL,
    FOREIGN KEY (UserID) REFERENCES [User](UserID)
);

-- 3. Patient (extends User)
CREATE TABLE Patient (
    PatientID INT PRIMARY KEY,
    UserID INT UNIQUE NOT NULL,
    FOREIGN KEY (UserID) REFERENCES [User](UserID)
);

-- 4. Room
CREATE TABLE Room (
    RoomID INT PRIMARY KEY IDENTITY,
    RoomName NVARCHAR(50) NOT NULL,
);

-- 5. Shift (created by Manager)
CREATE TABLE Shift (
    ShiftID INT PRIMARY KEY IDENTITY,
    RoomID INT NOT NULL,
    ShiftType NVARCHAR(20) NOT NULL, -- Morning, Afternoon, Evening
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    FOREIGN KEY (RoomID) REFERENCES Room(RoomID)
);

-- 6. DoctorShift (Doctor register into Shift)
CREATE TABLE DoctorShift (
    DoctorShiftID INT PRIMARY KEY IDENTITY,
    DoctorID INT NOT NULL,
    ShiftID INT NOT NULL,
    Status NVARCHAR(20) DEFAULT 'Confirmed', -- Pending, Confirmed
    FOREIGN KEY (DoctorID) REFERENCES Doctor(DoctorID),
    FOREIGN KEY (ShiftID) REFERENCES Shift(ShiftID)
);

-- 7. Appointment (booking by Patient)
CREATE TABLE Appointment (
    AppointmentID INT PRIMARY KEY IDENTITY,
    PatientID INT NOT NULL,
    DoctorShiftID INT NOT NULL,
    AppointmentDate DATETIME NOT NULL,
    Status NVARCHAR(20) DEFAULT 'Pending', -- Pending, Confirmed, Cancelled, Completed
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (PatientID) REFERENCES Patient(PatientID),
    FOREIGN KEY (DoctorShiftID) REFERENCES DoctorShift(DoctorShiftID)
);

-- 8. MedicalRecord
CREATE TABLE MedicalRecord (
    RecordID INT PRIMARY KEY IDENTITY,
    AppointmentID INT NOT NULL,
    DoctorNotes NVARCHAR(MAX),
    Diagnosis NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (AppointmentID) REFERENCES Appointment(AppointmentID)
);

-- 9. PharmacyProvider
CREATE TABLE PharmacyProvider (
    ProviderID INT PRIMARY KEY IDENTITY,
    ProviderName NVARCHAR(100) NOT NULL,
    Contact NVARCHAR(100) NULL
);

-- 10. Medicine
CREATE TABLE Medicine (
    MedicineID INT PRIMARY KEY IDENTITY,
    ProviderID INT NOT NULL,
    MedicineName NVARCHAR(100) NOT NULL,
	SideEffects NVARCHAR(255),
    Status NVARCHAR(20) DEFAULT 'Available', -- Available, Discontinued
    FOREIGN KEY (ProviderID) REFERENCES PharmacyProvider(ProviderID)
);

-- 11. Prescription
CREATE TABLE Prescription (
    PrescriptionID INT PRIMARY KEY IDENTITY,
    RecordID INT NOT NULL,
    DoctorID INT NOT NULL,
    IssuedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (RecordID) REFERENCES MedicalRecord(RecordID),
    FOREIGN KEY (DoctorID) REFERENCES Doctor(DoctorID)
);

-- 12. PrescriptionDetail
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

-- 13. Payment
CREATE TABLE Payment (
    PaymentID INT PRIMARY KEY IDENTITY,
    AppointmentID INT NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    PaymentDate DATETIME DEFAULT GETDATE(),
    Method NVARCHAR(50), -- Cash, Card, Online
    Status NVARCHAR(20) DEFAULT 'Pending',
    FOREIGN KEY (AppointmentID) REFERENCES Appointment(AppointmentID)
);

-- 14. ChatLog (final chat storage from Firebase)
CREATE TABLE ChatLog (
    ChatID INT PRIMARY KEY IDENTITY,
    PatientID INT NOT NULL,
    ReceptionistID INT NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (PatientID) REFERENCES Patient(PatientID),
    FOREIGN KEY (ReceptionistID) REFERENCES [User](UserID)
);

