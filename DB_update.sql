USE [master];
GO

-- (Tùy chọn) Nếu đã tồn tại DB DiamondHealth thì drop đi để tạo lại
IF DB_ID(N'DiamondHealth') IS NOT NULL
BEGIN
    ALTER DATABASE [DiamondHealth] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [DiamondHealth];
END
GO

-- Tạo database, KHÔNG chỉ định FILENAME
CREATE DATABASE [DiamondHealth]
CONTAINMENT = NONE;
GO

ALTER DATABASE [DiamondHealth] SET COMPATIBILITY_LEVEL = 160;
GO

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
BEGIN
    EXEC [DiamondHealth].[dbo].[sp_fulltext_database] @action = 'enable';
END
GO

ALTER DATABASE [DiamondHealth] SET ANSI_NULL_DEFAULT OFF;
ALTER DATABASE [DiamondHealth] SET ANSI_NULLS OFF;
ALTER DATABASE [DiamondHealth] SET ANSI_PADDING OFF;
ALTER DATABASE [DiamondHealth] SET ANSI_WARNINGS OFF;
ALTER DATABASE [DiamondHealth] SET ARITHABORT OFF;
ALTER DATABASE [DiamondHealth] SET AUTO_CLOSE OFF;
ALTER DATABASE [DiamondHealth] SET AUTO_SHRINK OFF;
ALTER DATABASE [DiamondHealth] SET AUTO_UPDATE_STATISTICS ON;
ALTER DATABASE [DiamondHealth] SET CURSOR_CLOSE_ON_COMMIT OFF;
ALTER DATABASE [DiamondHealth] SET CURSOR_DEFAULT GLOBAL;
ALTER DATABASE [DiamondHealth] SET CONCAT_NULL_YIELDS_NULL OFF;
ALTER DATABASE [DiamondHealth] SET NUMERIC_ROUNDABORT OFF;
ALTER DATABASE [DiamondHealth] SET QUOTED_IDENTIFIER OFF;
ALTER DATABASE [DiamondHealth] SET RECURSIVE_TRIGGERS OFF;
ALTER DATABASE [DiamondHealth] SET ENABLE_BROKER;
ALTER DATABASE [DiamondHealth] SET AUTO_UPDATE_STATISTICS_ASYNC OFF;
ALTER DATABASE [DiamondHealth] SET DATE_CORRELATION_OPTIMIZATION OFF;
ALTER DATABASE [DiamondHealth] SET TRUSTWORTHY OFF;
ALTER DATABASE [DiamondHealth] SET ALLOW_SNAPSHOT_ISOLATION OFF;
ALTER DATABASE [DiamondHealth] SET PARAMETERIZATION SIMPLE;
ALTER DATABASE [DiamondHealth] SET READ_COMMITTED_SNAPSHOT OFF;
ALTER DATABASE [DiamondHealth] SET HONOR_BROKER_PRIORITY OFF;
ALTER DATABASE [DiamondHealth] SET RECOVERY FULL;
ALTER DATABASE [DiamondHealth] SET MULTI_USER;
ALTER DATABASE [DiamondHealth] SET PAGE_VERIFY CHECKSUM;
ALTER DATABASE [DiamondHealth] SET DB_CHAINING OFF;
ALTER DATABASE [DiamondHealth] SET FILESTREAM (NON_TRANSACTED_ACCESS = OFF);
ALTER DATABASE [DiamondHealth] SET TARGET_RECOVERY_TIME = 60 SECONDS;
ALTER DATABASE [DiamondHealth] SET DELAYED_DURABILITY = DISABLED;
ALTER DATABASE [DiamondHealth] SET ACCELERATED_DATABASE_RECOVERY = OFF;
GO

EXEC sys.sp_db_vardecimal_storage_format N'DiamondHealth', N'ON';
GO

ALTER DATABASE [DiamondHealth] SET QUERY_STORE = ON;
GO

ALTER DATABASE [DiamondHealth] SET QUERY_STORE
(
    OPERATION_MODE = READ_WRITE,
    CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30),
    DATA_FLUSH_INTERVAL_SECONDS = 900,
    INTERVAL_LENGTH_MINUTES = 60,
    MAX_STORAGE_SIZE_MB = 1000,
    QUERY_CAPTURE_MODE = AUTO,
    SIZE_BASED_CLEANUP_MODE = AUTO,
    MAX_PLANS_PER_QUERY = 200,
    WAIT_STATS_CAPTURE_MODE = ON
);
GO

USE [DiamondHealth];
GO

-- Từ đây trở xuống là script CREATE TABLE (Appointment, User, Doctor, ...)
-- ... (dán tiếp phần CREATE TABLE của bạn)

/****** Object:  Table [dbo].[Appointment]    Script Date: 22/11/2025 19:51:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Appointment](
	[AppointmentID] [int] IDENTITY(1,1) NOT NULL,
	[PatientID] [int] NOT NULL,
	[DoctorID] [int] NOT NULL,
	[AppointmentDate] [datetime] NOT NULL,
	[ReceptionistID] [int] NULL,
	[UpdatedBy] [int] NULL,
	[Status] [nvarchar](20) NULL,
	[CreatedAt] [datetime] NULL,
	[ReasonForVisit] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[AppointmentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DermatologyRecord]    Script Date: 22/11/2025 19:51:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DermatologyRecord](
	[DermRecordID] [int] IDENTITY(1,1) NOT NULL,
	[RecordID] [int] NOT NULL,
	[PerformedByUserID] [int] NULL,
	[RequestedProcedure] [nvarchar](200) NOT NULL,
	[BodyArea] [nvarchar](200) NULL,
	[ProcedureNotes] [nvarchar](max) NULL,
	[ResultSummary] [nvarchar](max) NULL,
	[Attachment] [nvarchar](255) NULL,
	[PerformedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[DermRecordID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Doctor]    Script Date: 22/11/2025 19:51:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Doctor](
	[DoctorID] [int] NOT NULL,
	[UserID] [int] NOT NULL,
	[Specialty] [nvarchar](100) NOT NULL,
	[ExperienceYears] [int] NOT NULL,
	[RoomID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[DoctorID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DoctorShift]    Script Date: 22/11/2025 19:51:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DoctorShift](
	[DoctorShiftID] [int] IDENTITY(1,1) NOT NULL,
	[DoctorID] [int] NOT NULL,
	[ShiftID] [int] NOT NULL,
	[EffectiveFrom] [date] NOT NULL,
	[EffectiveTo] [date] NULL,
	[Status] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[DoctorShiftID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DoctorShiftExchange]    Script Date: 22/11/2025 19:51:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DoctorShiftExchange](
	[ExchangeID] [int] IDENTITY(1,1) NOT NULL,
	[Doctor1ID] [int] NOT NULL,
	[Doctor1ShiftRefID] [int] NOT NULL,
	[DoctorOld1ShiftID] [int] NOT NULL,
	[Doctor2ID] [int] NULL,
	[Doctor2ShiftRefID] [int] NULL,
	[DoctorOld2ShiftID] [int] NULL,
	[ExchangeDate] [date] NULL,
	[Status] [nvarchar](20) NULL,
	[SwapType] [nvarchar](20) NULL,
PRIMARY KEY CLUSTERED 
(
	[ExchangeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[InternalMedRecord]    Script Date: 22/11/2025 19:51:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InternalMedRecord](
	[RecordID] [int] NOT NULL,
	[BloodPressure] [int] NULL,
	[HeartRate] [int] NULL,
	[BloodSugar] [decimal](6, 2) NULL,
	[Notes] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[RecordID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MedicalRecord]    Script Date: 22/11/2025 19:51:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MedicalRecord](
	[RecordID] [int] IDENTITY(1,1) NOT NULL,
	[AppointmentID] [int] NOT NULL,
	[DoctorNotes] [nvarchar](max) NULL,
	[Diagnosis] [nvarchar](max) NULL,
	[CreatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[RecordID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MedicalService]    Script Date: 22/11/2025 19:51:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MedicalService](
	[MedicalServiceID] [int] IDENTITY(1,1) NOT NULL,
	[RecordID] [int] NOT NULL,
	[ServiceID] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
	[UnitPrice] [decimal](18, 2) NOT NULL,
	[TotalPrice]  AS ([Quantity]*[UnitPrice]) PERSISTED,
	[Notes] [nvarchar](500) NULL,
	[CreatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[MedicalServiceID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Medicine]    Script Date: 22/11/2025 19:51:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Medicine](
	[MedicineID] [int] IDENTITY(1,1) NOT NULL,
	[ProviderID] [int] NOT NULL,
	[MedicineName] [nvarchar](100) NOT NULL,
	[Status] [nvarchar](20) NULL,
	[ActiveIngredient] [nvarchar](200) NULL,
	[Strength] [nvarchar](50) NULL,
	[DosageForm] [nvarchar](100) NULL,
	[Route] [nvarchar](50) NULL,
	[PrescriptionUnit] [nvarchar](50) NULL,
	[TherapeuticClass] [nvarchar](100) NULL,
	[PackSize] [nvarchar](100) NULL,
	[CommonSideEffects] [nvarchar](max) NULL,
	[NoteForDoctor] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[MedicineID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MedicineVersion]    Script Date: 22/11/2025 19:51:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MedicineVersion](
	[MedicineVersionId] [int] IDENTITY(1,1) NOT NULL,
	[MedicineId] [int] NOT NULL,
	[MedicineName] [nvarchar](200) NOT NULL,
	[Strength] [nvarchar](100) NULL,
	[DosageForm] [nvarchar](100) NULL,
	[Route] [nvarchar](100) NULL,
	[PrescriptionUnit] [nvarchar](50) NULL,
	[TherapeuticClass] [nvarchar](200) NULL,
	[ProviderId] [int] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[ActiveIngredient] [nvarchar](200) NULL,
	[PackSize] [nvarchar](100) NULL,
	[CommonSideEffects] [nvarchar](1000) NULL,
	[NoteForDoctor] [nvarchar](500) NULL,
	[ProviderName] [nvarchar](100) NULL,
	[ProviderContact] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[MedicineVersionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Notification]    Script Date: 22/11/2025 19:51:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notification](
	[NotificationId] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](200) NOT NULL,
	[Content] [nvarchar](max) NOT NULL,
	[Type] [nvarchar](50) NOT NULL,
	[CreatedBy] [int] NULL,
	[CreatedDate] [datetime] NOT NULL,
	[IsGlobal] [bit] NOT NULL,
	[IsEmailSent] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[NotificationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NotificationReceiver]    Script Date: 22/11/2025 19:51:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NotificationReceiver](
	[NotificationId] [int] NOT NULL,
	[ReceiverId] [int] NOT NULL,
	[IsRead] [bit] NOT NULL,
	[ReadDate] [datetime] NULL,
 CONSTRAINT [PK_NotificationReceiver] PRIMARY KEY CLUSTERED 
(
	[NotificationId] ASC,
	[ReceiverId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Patient]    Script Date: 22/11/2025 19:51:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Patient](
	[PatientID] [int] NOT NULL,
	[UserID] [int] NOT NULL,
	[Allergies] [nvarchar](500) NULL,
	[MedicalHistory] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[PatientID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Payment]    Script Date: 22/11/2025 19:51:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Payment](
	[PaymentID] [int] IDENTITY(1,1) NOT NULL,
	[RecordID] [int] NOT NULL,
	[Amount] [decimal](10, 2) NOT NULL,
	[PaymentDate] [datetime] NULL,
	[Method] [nvarchar](50) NULL,
	[Status] [nvarchar](20) NULL,
	[OrderCode] [bigint] NULL,
	[CheckoutUrl] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[PaymentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PediatricRecord]    Script Date: 22/11/2025 19:51:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PediatricRecord](
	[RecordID] [int] NOT NULL,
	[WeightKg] [decimal](5, 2) NULL,
	[HeightCm] [decimal](5, 2) NULL,
	[HeartRate] [int] NULL,
	[TemperatureC] [decimal](4, 1) NULL,
PRIMARY KEY CLUSTERED 
(
	[RecordID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PharmacyProvider]    Script Date: 22/11/2025 19:51:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PharmacyProvider](
	[ProviderID] [int] NOT NULL,
	[UserID] [int] NOT NULL,
	[Contact] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[ProviderID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Prescription]    Script Date: 22/11/2025 19:51:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Prescription](
	[PrescriptionID] [int] IDENTITY(1,1) NOT NULL,
	[RecordID] [int] NOT NULL,
	[DoctorID] [int] NOT NULL,
	[IssuedDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[PrescriptionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PrescriptionDetail]    Script Date: 22/11/2025 19:51:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PrescriptionDetail](
	[PrescriptionDetailID] [int] IDENTITY(1,1) NOT NULL,
	[PrescriptionID] [int] NOT NULL,
	[Dosage] [nvarchar](100) NOT NULL,
	[Duration] [nvarchar](50) NOT NULL,
	[Instruction] [nvarchar](200) NULL,
	[MedicineVersionId] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[PrescriptionDetailID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Receptionist]    Script Date: 22/11/2025 19:51:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Receptionist](
	[ReceptionistID] [int] NOT NULL,
	[UserID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ReceptionistID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Role]    Script Date: 22/11/2025 19:51:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Role](
	[RoleID] [int] IDENTITY(1,1) NOT NULL,
	[RoleName] [nvarchar](30) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[RoleID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Room]    Script Date: 22/11/2025 19:51:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Room](
	[RoomID] [int] IDENTITY(1,1) NOT NULL,
	[RoomName] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[RoomID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Service]    Script Date: 22/11/2025 19:51:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Service](
	[ServiceID] [int] IDENTITY(1,1) NOT NULL,
	[ServiceName] [nvarchar](150) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[Price] [decimal](18, 2) NULL,
	[Category] [nvarchar](100) NULL,
	[IsActive] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ServiceID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Shift]    Script Date: 22/11/2025 19:51:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Shift](
	[ShiftID] [int] IDENTITY(1,1) NOT NULL,
	[ShiftType] [nvarchar](20) NOT NULL,
	[StartTime] [time](7) NOT NULL,
	[EndTime] [time](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ShiftID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TestResult]    Script Date: 22/11/2025 19:51:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TestResult](
	[TestResultID] [int] IDENTITY(1,1) NOT NULL,
	[RecordID] [int] NOT NULL,
	[ServiceID] [int] NOT NULL,
	[ResultValue] [nvarchar](100) NOT NULL,
	[Unit] [nvarchar](50) NULL,
	[Attachment] [nvarchar](255) NULL,
	[ResultDate] [datetime] NULL,
	[Notes] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[TestResultID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[User]    Script Date: 22/11/2025 19:51:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[User](
	[UserID] [int] IDENTITY(1,1) NOT NULL,
	[Phone] [nvarchar](20) NOT NULL,
	[PasswordHash] [nvarchar](255) NOT NULL,
	[FullName] [nvarchar](100) NOT NULL,
	[Email] [nvarchar](100) NULL,
	[DOB] [date] NULL,
	[Gender] [nvarchar](10) NULL,
	[RoleID] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Avatar] [nvarchar](500) NULL,
	[EmailVerified] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[Appointment] ON 

-- ============================================
-- INSERT APPOINTMENTS
-- ============================================
-- Note: Updated DoctorID references to match new doctor structure
INSERT [dbo].[Appointment] ([AppointmentID], [PatientID], [DoctorID], [AppointmentDate], [ReceptionistID], [UpdatedBy], [Status], [CreatedAt], [ReasonForVisit]) VALUES (1, 1, 1, CAST(N'2025-10-10T09:00:00.000' AS DateTime), 1, 3, N'Confirmed', CAST(N'2025-11-15T23:18:53.543' AS DateTime), N'Khám tổng quát')
INSERT [dbo].[Appointment] ([AppointmentID], [PatientID], [DoctorID], [AppointmentDate], [ReceptionistID], [UpdatedBy], [Status], [CreatedAt], [ReasonForVisit]) VALUES (2, 3, 1, CAST(N'2025-11-17T17:00:00.000' AS DateTime), 1, NULL, N'Confirmed', CAST(N'2025-11-16T22:38:44.063' AS DateTime), N'Đau bụng')
INSERT [dbo].[Appointment] ([AppointmentID], [PatientID], [DoctorID], [AppointmentDate], [ReceptionistID], [UpdatedBy], [Status], [CreatedAt], [ReasonForVisit]) VALUES (3, 3, 3, CAST(N'2025-11-18T17:00:00.000' AS DateTime), 1, NULL, N'Confirmed', CAST(N'2025-11-17T02:58:09.480' AS DateTime), N'Khám nhi khoa')
INSERT [dbo].[Appointment] ([AppointmentID], [PatientID], [DoctorID], [AppointmentDate], [ReceptionistID], [UpdatedBy], [Status], [CreatedAt], [ReasonForVisit]) VALUES (4, 1, 1, CAST(N'2025-11-19T06:00:00.000' AS DateTime), 1, NULL, N'Cancelled', CAST(N'2025-11-18T05:59:10.927' AS DateTime), N'Đau đầu')
INSERT [dbo].[Appointment] ([AppointmentID], [PatientID], [DoctorID], [AppointmentDate], [ReceptionistID], [UpdatedBy], [Status], [CreatedAt], [ReasonForVisit]) VALUES (5, 1, 1, CAST(N'2025-11-19T17:00:00.000' AS DateTime), 1, NULL, N'Confirmed', CAST(N'2025-11-18T06:01:48.297' AS DateTime), N'Đau đầu')
INSERT [dbo].[Appointment] ([AppointmentID], [PatientID], [DoctorID], [AppointmentDate], [ReceptionistID], [UpdatedBy], [Status], [CreatedAt], [ReasonForVisit]) VALUES (6, 2, 2, CAST(N'2025-11-21T17:00:00.000' AS DateTime), 1, NULL, N'Confirmed', CAST(N'2025-11-18T06:34:10.970' AS DateTime), N'Đau bụng')
INSERT [dbo].[Appointment] ([AppointmentID], [PatientID], [DoctorID], [AppointmentDate], [ReceptionistID], [UpdatedBy], [Status], [CreatedAt], [ReasonForVisit]) VALUES (7, 3, 5, CAST(N'2025-11-22T17:00:00.000' AS DateTime), 1, NULL, N'Confirmed', CAST(N'2025-11-18T06:55:03.453' AS DateTime), N'Khám da liễu')
INSERT [dbo].[Appointment] ([AppointmentID], [PatientID], [DoctorID], [AppointmentDate], [ReceptionistID], [UpdatedBy], [Status], [CreatedAt], [ReasonForVisit]) VALUES (8, 3, 5, CAST(N'2025-11-22T17:00:00.000' AS DateTime), 1, NULL, N'Confirmed', CAST(N'2025-11-18T06:59:58.500' AS DateTime), N'Khám da liễu')
INSERT [dbo].[Appointment] ([AppointmentID], [PatientID], [DoctorID], [AppointmentDate], [ReceptionistID], [UpdatedBy], [Status], [CreatedAt], [ReasonForVisit]) VALUES (9, 3, 5, CAST(N'2025-11-22T17:00:00.000' AS DateTime), 1, NULL, N'Confirmed', CAST(N'2025-11-18T08:05:24.443' AS DateTime), N'Khám da liễu')
INSERT [dbo].[Appointment] ([AppointmentID], [PatientID], [DoctorID], [AppointmentDate], [ReceptionistID], [UpdatedBy], [Status], [CreatedAt], [ReasonForVisit]) VALUES (10, 1, 7, CAST(N'2025-11-23T17:00:00.000' AS DateTime), 1, NULL, N'Confirmed', CAST(N'2025-11-18T08:11:37.540' AS DateTime), N'Khám chuyên khoa')
SET IDENTITY_INSERT [dbo].[Appointment] OFF
GO
SET IDENTITY_INSERT [dbo].[DermatologyRecord] ON 

INSERT [dbo].[DermatologyRecord] ([DermRecordID], [RecordID], [PerformedByUserID], [RequestedProcedure], [BodyArea], [ProcedureNotes], [ResultSummary], [Attachment], [PerformedAt]) VALUES (1, 1, NULL, N'Soi da bằng đèn Wood', N'Má trái', N'Vệ sinh vùng da, bệnh nhân hợp tác tốt.', N'Tăng sắc tố nhẹ, không dấu hiệu ác tính.', N'/uploads/derm/record1/wood_lamp_left_cheek.jpg', CAST(N'2025-11-16T19:07:07.863' AS DateTime))
INSERT [dbo].[DermatologyRecord] ([DermRecordID], [RecordID], [PerformedByUserID], [RequestedProcedure], [BodyArea], [ProcedureNotes], [ResultSummary], [Attachment], [PerformedAt]) VALUES (2, 9, 6, N'Khám da liễu', NULL, N'Sát khuẩn bằng cồn 70°, lấy nhân mụn bằng dụng cụ vô trùng, bôi thuốc kháng viêm Fucidin. Không ghi nhận phản ứng dị ứng.', N'Da vùng điều trị khô, giảm sưng đỏ sau 15 phút, không có biến chứng. Hẹn tái khám sau 5 ngày.', NULL, CAST(N'2025-11-22T02:25:18.047' AS DateTime))
INSERT [dbo].[DermatologyRecord] ([DermRecordID], [RecordID], [PerformedByUserID], [RequestedProcedure], [BodyArea], [ProcedureNotes], [ResultSummary], [Attachment], [PerformedAt]) VALUES (3, 8, NULL, N'Khám da liễu', NULL, NULL, NULL, NULL, CAST(N'2025-11-18T12:01:43.703' AS DateTime))
INSERT [dbo].[DermatologyRecord] ([DermRecordID], [RecordID], [PerformedByUserID], [RequestedProcedure], [BodyArea], [ProcedureNotes], [ResultSummary], [Attachment], [PerformedAt]) VALUES (4, 4, 6, N'Lazer da', N'tay', N'đưa máy lại gần da', N'da bình thường sau khi thực hiện', NULL, CAST(N'2025-11-18T13:44:12.673' AS DateTime))
INSERT [dbo].[DermatologyRecord] ([DermRecordID], [RecordID], [PerformedByUserID], [RequestedProcedure], [BodyArea], [ProcedureNotes], [ResultSummary], [Attachment], [PerformedAt]) VALUES (5, 6, 6, N'khám da', N'tay', N'soi lazer da', N'da bình thường', N'aa.jpg', CAST(N'2025-11-18T13:53:59.123' AS DateTime))
SET IDENTITY_INSERT [dbo].[DermatologyRecord] OFF
GO
-- ============================================
-- INSERT DOCTORS - 4 SPECIALTIES, 2 DOCTORS EACH
-- MỖI BÁC SĨ CÓ 1 PHÒNG RIÊNG
-- ============================================
-- Nội khoa (Internal Medicine) - 2 doctors
INSERT [dbo].[Doctor] ([DoctorID], [UserID], [Specialty], [ExperienceYears], [RoomID]) VALUES (1, 8, N'Nội khoa', 10, 1)   -- Phòng 101
INSERT [dbo].[Doctor] ([DoctorID], [UserID], [Specialty], [ExperienceYears], [RoomID]) VALUES (2, 13, N'Nội khoa', 8, 5)   -- Phòng 102

-- Nhi khoa (Pediatrics) - 2 doctors
INSERT [dbo].[Doctor] ([DoctorID], [UserID], [Specialty], [ExperienceYears], [RoomID]) VALUES (3, 14, N'Nhi khoa', 12, 3)   -- Phòng 301
INSERT [dbo].[Doctor] ([DoctorID], [UserID], [Specialty], [ExperienceYears], [RoomID]) VALUES (4, 15, N'Nhi khoa', 9, 6)    -- Phòng 302

-- Da liễu (Dermatology) - 2 doctors
INSERT [dbo].[Doctor] ([DoctorID], [UserID], [Specialty], [ExperienceYears], [RoomID]) VALUES (5, 9, N'Da liễu', 7, 4)      -- Phòng 401
INSERT [dbo].[Doctor] ([DoctorID], [UserID], [Specialty], [ExperienceYears], [RoomID]) VALUES (6, 16, N'Da liễu', 6, 7)     -- Phòng 402

-- Chuyên khoa (Specialist) - 2 doctors
INSERT [dbo].[Doctor] ([DoctorID], [UserID], [Specialty], [ExperienceYears], [RoomID]) VALUES (7, 1, N'Chuyên khoa', 15, 2) -- Phòng 201
INSERT [dbo].[Doctor] ([DoctorID], [UserID], [Specialty], [ExperienceYears], [RoomID]) VALUES (8, 10, N'Chuyên khoa', 11, 8) -- Phòng 202
GO
-- ============================================
-- INSERT DOCTOR SHIFTS
-- ============================================
SET IDENTITY_INSERT [dbo].[DoctorShift] ON 

-- Nội khoa Doctors (1, 2)
-- Doctor 1 (Nội khoa)
INSERT [dbo].[DoctorShift] ([DoctorShiftID], [DoctorID], [ShiftID], [EffectiveFrom], [EffectiveTo], [Status]) VALUES (1, 1, 1, CAST(N'2025-11-01' AS Date), CAST(N'2025-12-01' AS Date), N'Active')
INSERT [dbo].[DoctorShift] ([DoctorShiftID], [DoctorID], [ShiftID], [EffectiveFrom], [EffectiveTo], [Status]) VALUES (2, 1, 2, CAST(N'2025-11-01' AS Date), CAST(N'2025-12-01' AS Date), N'Active')
INSERT [dbo].[DoctorShift] ([DoctorShiftID], [DoctorID], [ShiftID], [EffectiveFrom], [EffectiveTo], [Status]) VALUES (3, 1, 1, CAST(N'2025-12-01' AS Date), CAST(N'2026-01-01' AS Date), N'Active')
INSERT [dbo].[DoctorShift] ([DoctorShiftID], [DoctorID], [ShiftID], [EffectiveFrom], [EffectiveTo], [Status]) VALUES (4, 1, 2, CAST(N'2025-12-01' AS Date), CAST(N'2026-01-01' AS Date), N'Active')
-- Doctor 2 (Nội khoa)
INSERT [dbo].[DoctorShift] ([DoctorShiftID], [DoctorID], [ShiftID], [EffectiveFrom], [EffectiveTo], [Status]) VALUES (5, 2, 1, CAST(N'2025-11-01' AS Date), CAST(N'2025-12-01' AS Date), N'Active')
INSERT [dbo].[DoctorShift] ([DoctorShiftID], [DoctorID], [ShiftID], [EffectiveFrom], [EffectiveTo], [Status]) VALUES (6, 2, 3, CAST(N'2025-11-01' AS Date), CAST(N'2025-12-01' AS Date), N'Active')
INSERT [dbo].[DoctorShift] ([DoctorShiftID], [DoctorID], [ShiftID], [EffectiveFrom], [EffectiveTo], [Status]) VALUES (7, 2, 1, CAST(N'2025-12-01' AS Date), CAST(N'2026-01-01' AS Date), N'Active')
INSERT [dbo].[DoctorShift] ([DoctorShiftID], [DoctorID], [ShiftID], [EffectiveFrom], [EffectiveTo], [Status]) VALUES (8, 2, 3, CAST(N'2025-12-01' AS Date), CAST(N'2026-01-01' AS Date), N'Active')

-- Nhi khoa Doctors (3, 4)
-- Doctor 3 (Nhi khoa)
INSERT [dbo].[DoctorShift] ([DoctorShiftID], [DoctorID], [ShiftID], [EffectiveFrom], [EffectiveTo], [Status]) VALUES (9, 3, 1, CAST(N'2025-11-01' AS Date), CAST(N'2025-12-01' AS Date), N'Active')
INSERT [dbo].[DoctorShift] ([DoctorShiftID], [DoctorID], [ShiftID], [EffectiveFrom], [EffectiveTo], [Status]) VALUES (10, 3, 2, CAST(N'2025-11-01' AS Date), CAST(N'2025-12-01' AS Date), N'Active')
INSERT [dbo].[DoctorShift] ([DoctorShiftID], [DoctorID], [ShiftID], [EffectiveFrom], [EffectiveTo], [Status]) VALUES (11, 3, 1, CAST(N'2025-12-01' AS Date), CAST(N'2026-01-01' AS Date), N'Active')
INSERT [dbo].[DoctorShift] ([DoctorShiftID], [DoctorID], [ShiftID], [EffectiveFrom], [EffectiveTo], [Status]) VALUES (12, 3, 2, CAST(N'2025-12-01' AS Date), CAST(N'2026-01-01' AS Date), N'Active')
-- Doctor 4 (Nhi khoa)
INSERT [dbo].[DoctorShift] ([DoctorShiftID], [DoctorID], [ShiftID], [EffectiveFrom], [EffectiveTo], [Status]) VALUES (13, 4, 2, CAST(N'2025-11-01' AS Date), CAST(N'2025-12-01' AS Date), N'Active')
INSERT [dbo].[DoctorShift] ([DoctorShiftID], [DoctorID], [ShiftID], [EffectiveFrom], [EffectiveTo], [Status]) VALUES (14, 4, 3, CAST(N'2025-11-01' AS Date), CAST(N'2025-12-01' AS Date), N'Active')
INSERT [dbo].[DoctorShift] ([DoctorShiftID], [DoctorID], [ShiftID], [EffectiveFrom], [EffectiveTo], [Status]) VALUES (15, 4, 2, CAST(N'2025-12-01' AS Date), CAST(N'2026-01-01' AS Date), N'Active')
INSERT [dbo].[DoctorShift] ([DoctorShiftID], [DoctorID], [ShiftID], [EffectiveFrom], [EffectiveTo], [Status]) VALUES (16, 4, 3, CAST(N'2025-12-01' AS Date), CAST(N'2026-01-01' AS Date), N'Active')

-- Da liễu Doctors (5, 6)
-- Doctor 5 (Da liễu)
INSERT [dbo].[DoctorShift] ([DoctorShiftID], [DoctorID], [ShiftID], [EffectiveFrom], [EffectiveTo], [Status]) VALUES (17, 5, 2, CAST(N'2025-01-01' AS Date), CAST(N'2026-01-01' AS Date), N'Active')
INSERT [dbo].[DoctorShift] ([DoctorShiftID], [DoctorID], [ShiftID], [EffectiveFrom], [EffectiveTo], [Status]) VALUES (18, 5, 3, CAST(N'2025-01-01' AS Date), CAST(N'2026-01-01' AS Date), N'Active')
-- Doctor 6 (Da liễu)
INSERT [dbo].[DoctorShift] ([DoctorShiftID], [DoctorID], [ShiftID], [EffectiveFrom], [EffectiveTo], [Status]) VALUES (19, 6, 1, CAST(N'2025-11-01' AS Date), CAST(N'2025-12-01' AS Date), N'Active')
INSERT [dbo].[DoctorShift] ([DoctorShiftID], [DoctorID], [ShiftID], [EffectiveFrom], [EffectiveTo], [Status]) VALUES (20, 6, 2, CAST(N'2025-11-01' AS Date), CAST(N'2025-12-01' AS Date), N'Active')
INSERT [dbo].[DoctorShift] ([DoctorShiftID], [DoctorID], [ShiftID], [EffectiveFrom], [EffectiveTo], [Status]) VALUES (21, 6, 1, CAST(N'2025-12-01' AS Date), CAST(N'2026-01-01' AS Date), N'Active')
INSERT [dbo].[DoctorShift] ([DoctorShiftID], [DoctorID], [ShiftID], [EffectiveFrom], [EffectiveTo], [Status]) VALUES (22, 6, 2, CAST(N'2025-12-01' AS Date), CAST(N'2026-01-01' AS Date), N'Active')

-- Chuyên khoa Doctors (7, 8)
-- Doctor 7 (Chuyên khoa)
INSERT [dbo].[DoctorShift] ([DoctorShiftID], [DoctorID], [ShiftID], [EffectiveFrom], [EffectiveTo], [Status]) VALUES (23, 7, 1, CAST(N'2025-11-01' AS Date), CAST(N'2025-12-01' AS Date), N'Active')
INSERT [dbo].[DoctorShift] ([DoctorShiftID], [DoctorID], [ShiftID], [EffectiveFrom], [EffectiveTo], [Status]) VALUES (24, 7, 2, CAST(N'2025-11-01' AS Date), CAST(N'2025-12-01' AS Date), N'Active')
INSERT [dbo].[DoctorShift] ([DoctorShiftID], [DoctorID], [ShiftID], [EffectiveFrom], [EffectiveTo], [Status]) VALUES (25, 7, 1, CAST(N'2025-12-01' AS Date), CAST(N'2026-01-01' AS Date), N'Active')
INSERT [dbo].[DoctorShift] ([DoctorShiftID], [DoctorID], [ShiftID], [EffectiveFrom], [EffectiveTo], [Status]) VALUES (26, 7, 2, CAST(N'2025-12-01' AS Date), CAST(N'2026-01-01' AS Date), N'Active')
-- Doctor 8 (Chuyên khoa)
INSERT [dbo].[DoctorShift] ([DoctorShiftID], [DoctorID], [ShiftID], [EffectiveFrom], [EffectiveTo], [Status]) VALUES (27, 8, 3, CAST(N'2025-11-01' AS Date), CAST(N'2025-12-01' AS Date), N'Active')
INSERT [dbo].[DoctorShift] ([DoctorShiftID], [DoctorID], [ShiftID], [EffectiveFrom], [EffectiveTo], [Status]) VALUES (28, 8, 3, CAST(N'2025-12-01' AS Date), CAST(N'2026-01-01' AS Date), N'Active')

SET IDENTITY_INSERT [dbo].[DoctorShift] OFF
GO
INSERT [dbo].[InternalMedRecord] ([RecordID], [BloodPressure], [HeartRate], [BloodSugar], [Notes]) VALUES (1, 135, 78, NULL, N'Khám tổng quát - theo dõi huyết áp')
INSERT [dbo].[InternalMedRecord] ([RecordID], [BloodPressure], [HeartRate], [BloodSugar], [Notes]) VALUES (4, NULL, NULL, NULL, NULL)
INSERT [dbo].[InternalMedRecord] ([RecordID], [BloodPressure], [HeartRate], [BloodSugar], [Notes]) VALUES (6, 11, 11, CAST(11.00 AS Decimal(6, 2)), N'11')
INSERT [dbo].[InternalMedRecord] ([RecordID], [BloodPressure], [HeartRate], [BloodSugar], [Notes]) VALUES (8, NULL, NULL, NULL, NULL)
INSERT [dbo].[InternalMedRecord] ([RecordID], [BloodPressure], [HeartRate], [BloodSugar], [Notes]) VALUES (9, 128, 78, CAST(5.80 AS Decimal(6, 2)), N'Huyết áp và nhịp tim trong giới hạn bình thường. Cần duy trì chế độ ăn ít muối và kiểm tra định kỳ hàng tháng.')
GO
SET IDENTITY_INSERT [dbo].[MedicalRecord] ON 

INSERT [dbo].[MedicalRecord] ([RecordID], [AppointmentID], [DoctorNotes], [Diagnosis], [CreatedAt]) VALUES (1, 1, N'Bệnh nhân có triệu chứng đau ngực nhẹ, huyết áp hơi cao.', N'Tăng huyết áp độ 1', CAST(N'2025-11-15T23:18:53.550' AS DateTime))
INSERT [dbo].[MedicalRecord] ([RecordID], [AppointmentID], [DoctorNotes], [Diagnosis], [CreatedAt]) VALUES (2, 2, NULL, NULL, CAST(N'2025-11-16T22:39:33.730' AS DateTime))
INSERT [dbo].[MedicalRecord] ([RecordID], [AppointmentID], [DoctorNotes], [Diagnosis], [CreatedAt]) VALUES (3, 3, NULL, N'đau bụng dạ dày', CAST(N'2025-11-17T03:00:47.890' AS DateTime))
INSERT [dbo].[MedicalRecord] ([RecordID], [AppointmentID], [DoctorNotes], [Diagnosis], [CreatedAt]) VALUES (4, 5, NULL, NULL, CAST(N'2025-11-18T06:02:21.280' AS DateTime))
INSERT [dbo].[MedicalRecord] ([RecordID], [AppointmentID], [DoctorNotes], [Diagnosis], [CreatedAt]) VALUES (5, 6, NULL, NULL, CAST(N'2025-11-18T06:35:03.893' AS DateTime))
INSERT [dbo].[MedicalRecord] ([RecordID], [AppointmentID], [DoctorNotes], [Diagnosis], [CreatedAt]) VALUES (6, 7, NULL, NULL, CAST(N'2025-11-18T06:55:50.463' AS DateTime))
INSERT [dbo].[MedicalRecord] ([RecordID], [AppointmentID], [DoctorNotes], [Diagnosis], [CreatedAt]) VALUES (7, 8, NULL, NULL, CAST(N'2025-11-18T07:00:22.870' AS DateTime))
INSERT [dbo].[MedicalRecord] ([RecordID], [AppointmentID], [DoctorNotes], [Diagnosis], [CreatedAt]) VALUES (8, 9, NULL, NULL, CAST(N'2025-11-18T08:05:53.250' AS DateTime))
INSERT [dbo].[MedicalRecord] ([RecordID], [AppointmentID], [DoctorNotes], [Diagnosis], [CreatedAt]) VALUES (9, 10, NULL, NULL, CAST(N'2025-11-18T08:11:59.890' AS DateTime))
SET IDENTITY_INSERT [dbo].[MedicalRecord] OFF
GO
SET IDENTITY_INSERT [dbo].[MedicalService] ON 

INSERT [dbo].[MedicalService] ([MedicalServiceID], [RecordID], [ServiceID], [Quantity], [UnitPrice], [Notes], [CreatedAt]) VALUES (10, 8, 13, 1, CAST(200000.00 AS Decimal(18, 2)), N'Khám Dermatology', CAST(N'2025-11-18T12:01:44.460' AS DateTime))
INSERT [dbo].[MedicalService] ([MedicalServiceID], [RecordID], [ServiceID], [Quantity], [UnitPrice], [Notes], [CreatedAt]) VALUES (11, 8, 14, 1, CAST(450000.00 AS Decimal(18, 2)), N'Khám InternalMed', CAST(N'2025-11-18T12:02:44.097' AS DateTime))
INSERT [dbo].[MedicalService] ([MedicalServiceID], [RecordID], [ServiceID], [Quantity], [UnitPrice], [Notes], [CreatedAt]) VALUES (12, 8, 15, 1, CAST(300000.00 AS Decimal(18, 2)), N'Khám Pediatric', CAST(N'2025-11-18T12:02:45.440' AS DateTime))
INSERT [dbo].[MedicalService] ([MedicalServiceID], [RecordID], [ServiceID], [Quantity], [UnitPrice], [Notes], [CreatedAt]) VALUES (13, 4, 13, 1, CAST(200000.00 AS Decimal(18, 2)), N'Khám Dermatology', CAST(N'2025-11-18T12:15:40.443' AS DateTime))
INSERT [dbo].[MedicalService] ([MedicalServiceID], [RecordID], [ServiceID], [Quantity], [UnitPrice], [Notes], [CreatedAt]) VALUES (14, 4, 14, 1, CAST(450000.00 AS Decimal(18, 2)), N'Khám InternalMed', CAST(N'2025-11-18T12:16:34.073' AS DateTime))
INSERT [dbo].[MedicalService] ([MedicalServiceID], [RecordID], [ServiceID], [Quantity], [UnitPrice], [Notes], [CreatedAt]) VALUES (15, 4, 15, 1, CAST(300000.00 AS Decimal(18, 2)), N'Khám Pediatric', CAST(N'2025-11-18T12:16:34.757' AS DateTime))
INSERT [dbo].[MedicalService] ([MedicalServiceID], [RecordID], [ServiceID], [Quantity], [UnitPrice], [Notes], [CreatedAt]) VALUES (16, 6, 14, 1, CAST(450000.00 AS Decimal(18, 2)), N'Khám InternalMed', CAST(N'2025-11-18T13:46:49.450' AS DateTime))
INSERT [dbo].[MedicalService] ([MedicalServiceID], [RecordID], [ServiceID], [Quantity], [UnitPrice], [Notes], [CreatedAt]) VALUES (17, 6, 15, 1, CAST(300000.00 AS Decimal(18, 2)), N'Khám Pediatric', CAST(N'2025-11-18T13:46:51.373' AS DateTime))
INSERT [dbo].[MedicalService] ([MedicalServiceID], [RecordID], [ServiceID], [Quantity], [UnitPrice], [Notes], [CreatedAt]) VALUES (18, 6, 13, 1, CAST(200000.00 AS Decimal(18, 2)), N'Khám Dermatology', CAST(N'2025-11-18T13:46:57.837' AS DateTime))
INSERT [dbo].[MedicalService] ([MedicalServiceID], [RecordID], [ServiceID], [Quantity], [UnitPrice], [Notes], [CreatedAt]) VALUES (19, 9, 20, 1, CAST(180000.00 AS Decimal(18, 2)), N'Dịch vụ xét nghiệm', CAST(N'2025-11-22T01:55:25.777' AS DateTime))
INSERT [dbo].[MedicalService] ([MedicalServiceID], [RecordID], [ServiceID], [Quantity], [UnitPrice], [Notes], [CreatedAt]) VALUES (20, 9, 23, 1, CAST(100000.00 AS Decimal(18, 2)), N'Dịch vụ xét nghiệm', CAST(N'2025-11-22T02:05:15.587' AS DateTime))
SET IDENTITY_INSERT [dbo].[MedicalService] OFF
GO
SET IDENTITY_INSERT [dbo].[Medicine] ON 

INSERT [dbo].[Medicine] ([MedicineID], [ProviderID], [MedicineName], [Status], [ActiveIngredient], [Strength], [DosageForm], [Route], [PrescriptionUnit], [TherapeuticClass], [PackSize], [CommonSideEffects], [NoteForDoctor]) VALUES (1, 1, N'Paracetamol 500mg', N'Providing', N'Paracetamol', N'500mg', N'Viên nén', N'Uống', N'Viên', N'Giảm đau – hạ sốt', N'Hộp 10 vỉ x 10 viên', N'Buồn ngủ, mệt nhẹ', N'Dùng thận trọng với bệnh nhân có bệnh lý gan')
INSERT [dbo].[Medicine] ([MedicineID], [ProviderID], [MedicineName], [Status], [ActiveIngredient], [Strength], [DosageForm], [Route], [PrescriptionUnit], [TherapeuticClass], [PackSize], [CommonSideEffects], [NoteForDoctor]) VALUES (2, 1, N'Amlodipineeee 5mg', N'Providing', N'Amlodipinee', N'5mgg', N'Viên nénn', N'Uốngg', N'Viênn', N'Thuốc hạ huyết ápp (nhóm chẹn kênh canxi)', N'Hộp 3 vỉ x 10 viênn', N'Phù chân, nhức đầuu', N'Theo dõi huyết áp 3–5 ngày đầuu')
INSERT [dbo].[Medicine] ([MedicineID], [ProviderID], [MedicineName], [Status], [ActiveIngredient], [Strength], [DosageForm], [Route], [PrescriptionUnit], [TherapeuticClass], [PackSize], [CommonSideEffects], [NoteForDoctor]) VALUES (3, 1, N'Amocilin', N'Providing', N'ABC', N'500ml', N'Viên', N'uống', N'Viên', N'Hạ sốt', N'10 vỉ x 10 viên', N'mẩn đỏ', N'lưu ý với bệnh nhân men gan cao')
INSERT [dbo].[Medicine] ([MedicineID], [ProviderID], [MedicineName], [Status], [ActiveIngredient], [Strength], [DosageForm], [Route], [PrescriptionUnit], [TherapeuticClass], [PackSize], [CommonSideEffects], [NoteForDoctor]) VALUES (4, 1, N'ABC', N'Stopped', N'Para', N'500mg', N'Viên nén', N'Uống', N'Viên', N'Hạ sốt', N'10 vỉ x 10 viên', N'mẩn đỏ', N'lưu ý bệnh nhân nhỏ tuổi')
INSERT [dbo].[Medicine] ([MedicineID], [ProviderID], [MedicineName], [Status], [ActiveIngredient], [Strength], [DosageForm], [Route], [PrescriptionUnit], [TherapeuticClass], [PackSize], [CommonSideEffects], [NoteForDoctor]) VALUES (5, 1, N'Amoxicillin 500mg', N'Providing', N'Amoxicillin', N'500mg', N'Capsule', N'Oral', N'Viên', N'Kháng sinh beta-lactam', N'Hộp 2 vỉ x 10 viên', N'Tiêu chảy, dị ứng, nổi mề đay', N'Không dùng cho bệnh nhân dị ứng penicillin')
INSERT [dbo].[Medicine] ([MedicineID], [ProviderID], [MedicineName], [Status], [ActiveIngredient], [Strength], [DosageForm], [Route], [PrescriptionUnit], [TherapeuticClass], [PackSize], [CommonSideEffects], [NoteForDoctor]) VALUES (6, 1, N'Loratadine 10mg', N'Providing', N'Loratadine', N'10mg', N'Tablet', N'Oral', N'Viên', N'Kháng histamin', N'Hộp 1 vỉ x 10 viên', N'Đau đầu, buồn ngủ nhẹ', N'Dùng 1 viên/ngày, tránh dùng chung với rượu')
INSERT [dbo].[Medicine] ([MedicineID], [ProviderID], [MedicineName], [Status], [ActiveIngredient], [Strength], [DosageForm], [Route], [PrescriptionUnit], [TherapeuticClass], [PackSize], [CommonSideEffects], [NoteForDoctor]) VALUES (7, 1, N'Diclofenac 50mg', N'Stopped', N'Diclofenac', N'50mg', N'Tablet', N'Oral', N'Viên', N'Chống viêm, giảm đau', N'Hộp 3 vỉ x 10 viên', N'Đau dạ dày, buồn nôn', N'Thận trọng ở bệnh nhân có tiền sử loét dạ dày')
INSERT [dbo].[Medicine] ([MedicineID], [ProviderID], [MedicineName], [Status], [ActiveIngredient], [Strength], [DosageForm], [Route], [PrescriptionUnit], [TherapeuticClass], [PackSize], [CommonSideEffects], [NoteForDoctor]) VALUES (8, 1, N'Paracetamol DH 500', N'Providing', N'Paracetamol', N'500mg', N'Viên nén', N'Uống', N'Viên', N'Thuốc hạ sốt, giảm đau', N'Hộp 10 vỉ x 10 viên', N'Buồn nôn nhẹ, đau đầu', N'Không dùng quá 4g paracetamol/ngày')
INSERT [dbo].[Medicine] ([MedicineID], [ProviderID], [MedicineName], [Status], [ActiveIngredient], [Strength], [DosageForm], [Route], [PrescriptionUnit], [TherapeuticClass], [PackSize], [CommonSideEffects], [NoteForDoctor]) VALUES (9, 1, N'Amoxicillinn DH 500', N'Providing', N'Amoxicillinn', N'500mgg', N'Viên nang', N'Uống', N'Viên', N'Kháng sinh penicillin', N'Hộp 2 vỉ x 10 viên', N'Tiêu chảy, phát ban da', N'Thận trọng với người dị ứng penicillin')
SET IDENTITY_INSERT [dbo].[Medicine] OFF
GO
SET IDENTITY_INSERT [dbo].[MedicineVersion] ON 

INSERT [dbo].[MedicineVersion] ([MedicineVersionId], [MedicineId], [MedicineName], [Strength], [DosageForm], [Route], [PrescriptionUnit], [TherapeuticClass], [ProviderId], [CreatedAt], [ActiveIngredient], [PackSize], [CommonSideEffects], [NoteForDoctor], [ProviderName], [ProviderContact]) VALUES (1, 1, N'Paracetamol 500mg', N'500mg', N'Viên nén', N'Uống', N'Viên', N'Giảm đau – hạ sốt', 1, CAST(N'2025-11-16T17:42:57.9946704' AS DateTime2), N'Paracetamol', N'Hộp 10 vỉ x 10 viên', N'Buồn ngủ, mệt nhẹ', N'Dùng thận trọng với bệnh nhân có bệnh lý gan', N'Võ Tấn D', N'Nhà thuốc Minh Châu - 25 Lý Thường Kiệt, Hà Nội')
INSERT [dbo].[MedicineVersion] ([MedicineVersionId], [MedicineId], [MedicineName], [Strength], [DosageForm], [Route], [PrescriptionUnit], [TherapeuticClass], [ProviderId], [CreatedAt], [ActiveIngredient], [PackSize], [CommonSideEffects], [NoteForDoctor], [ProviderName], [ProviderContact]) VALUES (2, 2, N'Amlodipine 5mg', N'5mg', N'Viên nén', N'Uống', N'Viên', N'Thuốc hạ huyết áp (nhóm chẹn kênh canxi)', 1, CAST(N'2025-11-16T17:42:57.9946704' AS DateTime2), N'Amlodipine', N'Hộp 3 vỉ x 10 viên', N'Phù chân, nhức đầu', N'Theo dõi huyết áp 3–5 ngày đầu', N'Võ Tấn D', N'Nhà thuốc Minh Châu - 25 Lý Thường Kiệt, Hà Nội')
INSERT [dbo].[MedicineVersion] ([MedicineVersionId], [MedicineId], [MedicineName], [Strength], [DosageForm], [Route], [PrescriptionUnit], [TherapeuticClass], [ProviderId], [CreatedAt], [ActiveIngredient], [PackSize], [CommonSideEffects], [NoteForDoctor], [ProviderName], [ProviderContact]) VALUES (3, 3, N'Amocilin', N'500ml', N'Viên', N'uống', N'Viên', N'Hạ sốt', 1, CAST(N'2025-11-16T17:42:57.9946704' AS DateTime2), N'ABC', N'10 vỉ x 10 viên', N'mẩn đỏ', N'lưu ý với bệnh nhân men gan cao', N'Võ Tấn D', N'Nhà thuốc Minh Châu - 25 Lý Thường Kiệt, Hà Nội')
INSERT [dbo].[MedicineVersion] ([MedicineVersionId], [MedicineId], [MedicineName], [Strength], [DosageForm], [Route], [PrescriptionUnit], [TherapeuticClass], [ProviderId], [CreatedAt], [ActiveIngredient], [PackSize], [CommonSideEffects], [NoteForDoctor], [ProviderName], [ProviderContact]) VALUES (4, 4, N'ABC', N'500mg', N'Viên nén', N'Uống', N'Viên', N'Hạ sốt', 1, CAST(N'2025-11-16T17:42:57.9946704' AS DateTime2), N'Para', N'10 vỉ x 10 viên', N'mẩn đỏ', N'lưu ý bệnh nhân nhỏ tuổi', N'Võ Tấn D', N'Nhà thuốc Minh Châu - 25 Lý Thường Kiệt, Hà Nội')
INSERT [dbo].[MedicineVersion] ([MedicineVersionId], [MedicineId], [MedicineName], [Strength], [DosageForm], [Route], [PrescriptionUnit], [TherapeuticClass], [ProviderId], [CreatedAt], [ActiveIngredient], [PackSize], [CommonSideEffects], [NoteForDoctor], [ProviderName], [ProviderContact]) VALUES (5, 5, N'Amoxicillin 500mg', N'500mg', N'Capsule', N'Oral', N'Viên', N'Kháng sinh beta-lactam', 1, CAST(N'2025-11-16T17:42:57.9946704' AS DateTime2), N'Amoxicillin', N'Hộp 2 vỉ x 10 viên', N'Tiêu chảy, dị ứng, nổi mề đay', N'Không dùng cho bệnh nhân dị ứng penicillin', N'Võ Tấn D', N'Nhà thuốc Minh Châu - 25 Lý Thường Kiệt, Hà Nội')
INSERT [dbo].[MedicineVersion] ([MedicineVersionId], [MedicineId], [MedicineName], [Strength], [DosageForm], [Route], [PrescriptionUnit], [TherapeuticClass], [ProviderId], [CreatedAt], [ActiveIngredient], [PackSize], [CommonSideEffects], [NoteForDoctor], [ProviderName], [ProviderContact]) VALUES (6, 6, N'Loratadine 10mg', N'10mg', N'Tablet', N'Oral', N'Viên', N'Kháng histamin', 1, CAST(N'2025-11-16T17:42:57.9946704' AS DateTime2), N'Loratadine', N'Hộp 1 vỉ x 10 viên', N'Đau đầu, buồn ngủ nhẹ', N'Dùng 1 viên/ngày, tránh dùng chung với rượu', N'Võ Tấn D', N'Nhà thuốc Minh Châu - 25 Lý Thường Kiệt, Hà Nội')
INSERT [dbo].[MedicineVersion] ([MedicineVersionId], [MedicineId], [MedicineName], [Strength], [DosageForm], [Route], [PrescriptionUnit], [TherapeuticClass], [ProviderId], [CreatedAt], [ActiveIngredient], [PackSize], [CommonSideEffects], [NoteForDoctor], [ProviderName], [ProviderContact]) VALUES (7, 7, N'Diclofenac 50mg', N'50mg', N'Tablet', N'Oral', N'Viên', N'Chống viêm, giảm đau', 1, CAST(N'2025-11-16T17:42:57.9946704' AS DateTime2), N'Diclofenac', N'Hộp 3 vỉ x 10 viên', N'Đau dạ dày, buồn nôn', N'Thận trọng ở bệnh nhân có tiền sử loét dạ dày', N'Võ Tấn D', N'Nhà thuốc Minh Châu - 25 Lý Thường Kiệt, Hà Nội')
INSERT [dbo].[MedicineVersion] ([MedicineVersionId], [MedicineId], [MedicineName], [Strength], [DosageForm], [Route], [PrescriptionUnit], [TherapeuticClass], [ProviderId], [CreatedAt], [ActiveIngredient], [PackSize], [CommonSideEffects], [NoteForDoctor], [ProviderName], [ProviderContact]) VALUES (8, 8, N'Paracetamol DH 500', N'500mg', N'Viên nén', N'Uống', N'Viên', N'Thuốc hạ sốt, giảm đau', 1, CAST(N'2025-11-16T17:42:57.9946704' AS DateTime2), N'Paracetamol', N'Hộp 10 vỉ x 10 viên', N'Buồn nôn nhẹ, đau đầu', N'Không dùng quá 4g paracetamol/ngày', N'Võ Tấn D', N'Nhà thuốc Minh Châu - 25 Lý Thường Kiệt, Hà Nội')
INSERT [dbo].[MedicineVersion] ([MedicineVersionId], [MedicineId], [MedicineName], [Strength], [DosageForm], [Route], [PrescriptionUnit], [TherapeuticClass], [ProviderId], [CreatedAt], [ActiveIngredient], [PackSize], [CommonSideEffects], [NoteForDoctor], [ProviderName], [ProviderContact]) VALUES (9, 9, N'Amoxicillin DH 500', N'500mg', N'Viên nang', N'Uống', N'Viên', N'Kháng sinh penicillin', 1, CAST(N'2025-11-16T17:42:57.9946704' AS DateTime2), N'Amoxicillin', N'Hộp 2 vỉ x 10 viên', N'Tiêu chảy, phát ban da', N'Thận trọng với người dị ứng penicillin', N'Võ Tấn D', N'Nhà thuốc Minh Châu - 25 Lý Thường Kiệt, Hà Nội')
INSERT [dbo].[MedicineVersion] ([MedicineVersionId], [MedicineId], [MedicineName], [Strength], [DosageForm], [Route], [PrescriptionUnit], [TherapeuticClass], [ProviderId], [CreatedAt], [ActiveIngredient], [PackSize], [CommonSideEffects], [NoteForDoctor], [ProviderName], [ProviderContact]) VALUES (10, 2, N'Amlodipinee 5mg', N'5mgg', N'Viên nénn', N'Uốngg', N'Viênn', N'Thuốc hạ huyết ápp (nhóm chẹn kênh canxi)', 1, CAST(N'2025-11-17T23:57:25.5837175' AS DateTime2), N'Amlodipinee', N'Hộp 3 vỉ x 10 viênn', N'Phù chân, nhức đầuu', N'Theo dõi huyết áp 3–5 ngày đầuu', N'Võ Tấn D', N'0908123456')
INSERT [dbo].[MedicineVersion] ([MedicineVersionId], [MedicineId], [MedicineName], [Strength], [DosageForm], [Route], [PrescriptionUnit], [TherapeuticClass], [ProviderId], [CreatedAt], [ActiveIngredient], [PackSize], [CommonSideEffects], [NoteForDoctor], [ProviderName], [ProviderContact]) VALUES (11, 2, N'Amlodipineee 5mg', N'5mgg', N'Viên nénn', N'Uốngg', N'Viênn', N'Thuốc hạ huyết ápp (nhóm chẹn kênh canxi)', 1, CAST(N'2025-11-18T00:01:24.4656808' AS DateTime2), N'Amlodipinee', N'Hộp 3 vỉ x 10 viênn', N'Phù chân, nhức đầuu', N'Theo dõi huyết áp 3–5 ngày đầuu', N'Võ Tấn D', N'0908123456')
INSERT [dbo].[MedicineVersion] ([MedicineVersionId], [MedicineId], [MedicineName], [Strength], [DosageForm], [Route], [PrescriptionUnit], [TherapeuticClass], [ProviderId], [CreatedAt], [ActiveIngredient], [PackSize], [CommonSideEffects], [NoteForDoctor], [ProviderName], [ProviderContact]) VALUES (12, 2, N'Amlodipineeee 5mg', N'5mgg', N'Viên nénn', N'Uốngg', N'Viênn', N'Thuốc hạ huyết ápp (nhóm chẹn kênh canxi)', 1, CAST(N'2025-11-18T00:07:44.2360896' AS DateTime2), N'Amlodipinee', N'Hộp 3 vỉ x 10 viênn', N'Phù chân, nhức đầuu', N'Theo dõi huyết áp 3–5 ngày đầuu', N'Võ Tấn D', N'Nhà thuốc Minh Châu - 25 Lý Thường Kiệt, Hà Nội - 0908123456')
SET IDENTITY_INSERT [dbo].[MedicineVersion] OFF
GO
INSERT [dbo].[Patient] ([PatientID], [UserID], [Allergies], [MedicalHistory]) VALUES (1, 2, N'Dị ứng với penicillin', N'Tiền sử tăng huyết áp nhẹ')
INSERT [dbo].[Patient] ([PatientID], [UserID], [Allergies], [MedicalHistory]) VALUES (2, 11, N'Không có', N'Không có bệnh lý nền')
INSERT [dbo].[Patient] ([PatientID], [UserID], [Allergies], [MedicalHistory]) VALUES (3, 12, N'Dị ứng với hải sản', N'Tiền sử đau dạ dày')
GO
SET IDENTITY_INSERT [dbo].[Payment] ON 

INSERT [dbo].[Payment] ([PaymentID], [RecordID], [Amount], [PaymentDate], [Method], [Status], [OrderCode], [CheckoutUrl]) VALUES (1, 1, CAST(350000.00 AS Decimal(10, 2)), CAST(N'2025-11-15T23:18:53.600' AS DateTime), N'Tiền mặt', N'Paid', NULL, NULL)
INSERT [dbo].[Payment] ([PaymentID], [RecordID], [Amount], [PaymentDate], [Method], [Status], [OrderCode], [CheckoutUrl]) VALUES (2, 1, CAST(350000.00 AS Decimal(10, 2)), CAST(N'2025-11-17T20:00:29.810' AS DateTime), N'Tiền mặt', N'Paid', NULL, NULL)
SET IDENTITY_INSERT [dbo].[Payment] OFF
GO
INSERT [dbo].[PediatricRecord] ([RecordID], [WeightKg], [HeightCm], [HeartRate], [TemperatureC]) VALUES (4, NULL, NULL, NULL, NULL)
INSERT [dbo].[PediatricRecord] ([RecordID], [WeightKg], [HeightCm], [HeartRate], [TemperatureC]) VALUES (6, CAST(11.00 AS Decimal(5, 2)), CAST(11.00 AS Decimal(5, 2)), 11, CAST(11.0 AS Decimal(4, 1)))
INSERT [dbo].[PediatricRecord] ([RecordID], [WeightKg], [HeightCm], [HeartRate], [TemperatureC]) VALUES (8, NULL, NULL, NULL, NULL)
INSERT [dbo].[PediatricRecord] ([RecordID], [WeightKg], [HeightCm], [HeartRate], [TemperatureC]) VALUES (9, CAST(25.00 AS Decimal(5, 2)), CAST(120.00 AS Decimal(5, 2)), 90, CAST(37.2 AS Decimal(4, 1)))
GO
INSERT [dbo].[PharmacyProvider] ([ProviderID], [UserID], [Contact]) VALUES (1, 4, N'Nhà thuốc Minh Châu - 25 Lý Thường Kiệt, Hà Nội')
GO
SET IDENTITY_INSERT [dbo].[Prescription] ON 

-- ============================================
-- INSERT PRESCRIPTIONS
-- ============================================
-- Note: Updated DoctorID references to match new doctor structure
INSERT [dbo].[Prescription] ([PrescriptionID], [RecordID], [DoctorID], [IssuedDate]) VALUES (1, 1, 1, CAST(N'2025-11-15T23:18:53.567' AS DateTime))
INSERT [dbo].[Prescription] ([PrescriptionID], [RecordID], [DoctorID], [IssuedDate]) VALUES (2, 2, 1, CAST(N'2025-11-16T16:50:06.077' AS DateTime))
INSERT [dbo].[Prescription] ([PrescriptionID], [RecordID], [DoctorID], [IssuedDate]) VALUES (3, 3, 3, CAST(N'2025-11-16T20:02:27.007' AS DateTime))
INSERT [dbo].[Prescription] ([PrescriptionID], [RecordID], [DoctorID], [IssuedDate]) VALUES (4, 4, 1, CAST(N'2025-11-17T23:25:11.340' AS DateTime))
INSERT [dbo].[Prescription] ([PrescriptionID], [RecordID], [DoctorID], [IssuedDate]) VALUES (5, 5, 1, CAST(N'2025-11-17T23:35:35.273' AS DateTime))
INSERT [dbo].[Prescription] ([PrescriptionID], [RecordID], [DoctorID], [IssuedDate]) VALUES (6, 6, 2, CAST(N'2025-11-17T23:56:15.713' AS DateTime))
INSERT [dbo].[Prescription] ([PrescriptionID], [RecordID], [DoctorID], [IssuedDate]) VALUES (7, 7, 5, CAST(N'2025-11-18T00:00:52.500' AS DateTime))
INSERT [dbo].[Prescription] ([PrescriptionID], [RecordID], [DoctorID], [IssuedDate]) VALUES (8, 8, 5, CAST(N'2025-11-18T01:06:56.353' AS DateTime))
INSERT [dbo].[Prescription] ([PrescriptionID], [RecordID], [DoctorID], [IssuedDate]) VALUES (9, 9, 5, CAST(N'2025-11-18T01:14:11.337' AS DateTime))
SET IDENTITY_INSERT [dbo].[Prescription] OFF
GO
SET IDENTITY_INSERT [dbo].[PrescriptionDetail] ON 

INSERT [dbo].[PrescriptionDetail] ([PrescriptionDetailID], [PrescriptionID], [Dosage], [Duration], [Instruction], [MedicineVersionId]) VALUES (1, 1, N'1 viên - 3 lần/ngày', N'5 ngày', NULL, 1)
INSERT [dbo].[PrescriptionDetail] ([PrescriptionDetailID], [PrescriptionID], [Dosage], [Duration], [Instruction], [MedicineVersionId]) VALUES (2, 1, N'1 viên - 1 lần/ngày', N'7 ngày', NULL, 2)
INSERT [dbo].[PrescriptionDetail] ([PrescriptionDetailID], [PrescriptionID], [Dosage], [Duration], [Instruction], [MedicineVersionId]) VALUES (3, 2, N'3 viên x ngày', N'5 ngày', NULL, 2)
INSERT [dbo].[PrescriptionDetail] ([PrescriptionDetailID], [PrescriptionID], [Dosage], [Duration], [Instruction], [MedicineVersionId]) VALUES (4, 2, N'2 viên x 3 ngày', N'5 ngày', NULL, 2)
INSERT [dbo].[PrescriptionDetail] ([PrescriptionDetailID], [PrescriptionID], [Dosage], [Duration], [Instruction], [MedicineVersionId]) VALUES (5, 3, N'2 viên x 3 lần/ngày', N'5 ngày', N'uống sau ăn', 1)
INSERT [dbo].[PrescriptionDetail] ([PrescriptionDetailID], [PrescriptionID], [Dosage], [Duration], [Instruction], [MedicineVersionId]) VALUES (6, 4, N'2 viên x 3 ngày', N'5 ngày', N'uống sau ăn', 8)
INSERT [dbo].[PrescriptionDetail] ([PrescriptionDetailID], [PrescriptionID], [Dosage], [Duration], [Instruction], [MedicineVersionId]) VALUES (7, 4, N'2 viên x 3 lần/ngày', N'5 ngaỳ', N'uống buổi tối', 2)
INSERT [dbo].[PrescriptionDetail] ([PrescriptionDetailID], [PrescriptionID], [Dosage], [Duration], [Instruction], [MedicineVersionId]) VALUES (8, 5, N'2 viên x 3 lần/ngày', N'5 ngày', N'uống sau ăn', 2)
INSERT [dbo].[PrescriptionDetail] ([PrescriptionDetailID], [PrescriptionID], [Dosage], [Duration], [Instruction], [MedicineVersionId]) VALUES (9, 6, N'2 viên x 3 lần', N'5 ngày', N'uống sau ăn', 2)
INSERT [dbo].[PrescriptionDetail] ([PrescriptionDetailID], [PrescriptionID], [Dosage], [Duration], [Instruction], [MedicineVersionId]) VALUES (10, 7, N'2 viên x 3 lần', N'5 ngày', N'uống sau ăn', 10)
INSERT [dbo].[PrescriptionDetail] ([PrescriptionDetailID], [PrescriptionID], [Dosage], [Duration], [Instruction], [MedicineVersionId]) VALUES (11, 8, N'2 viên x 3 lần/ngày', N'5 ngày', N'uống sau ăn', 12)
INSERT [dbo].[PrescriptionDetail] ([PrescriptionDetailID], [PrescriptionID], [Dosage], [Duration], [Instruction], [MedicineVersionId]) VALUES (12, 8, N'2 viên x 2 lần/ngày', N'5 ngày', N'uống sau ăn', 1)
INSERT [dbo].[PrescriptionDetail] ([PrescriptionDetailID], [PrescriptionID], [Dosage], [Duration], [Instruction], [MedicineVersionId]) VALUES (13, 9, N'2 viên x 2 lần/ngày', N'4 ngày', N'uống sau ăn', 12)
INSERT [dbo].[PrescriptionDetail] ([PrescriptionDetailID], [PrescriptionID], [Dosage], [Duration], [Instruction], [MedicineVersionId]) VALUES (14, 9, N'2 viên /ngày', N'5 ngày', N'sau ăn', 8)
SET IDENTITY_INSERT [dbo].[PrescriptionDetail] OFF
GO
INSERT [dbo].[Receptionist] ([ReceptionistID], [UserID]) VALUES (1, 3)
GO
-- ============================================
-- INSERT ROLES
-- ============================================
SET IDENTITY_INSERT [dbo].[Role] ON 

INSERT [dbo].[Role] ([RoleID], [RoleName]) VALUES (1, N'Guest')
INSERT [dbo].[Role] ([RoleID], [RoleName]) VALUES (2, N'Patient')
INSERT [dbo].[Role] ([RoleID], [RoleName]) VALUES (3, N'Receptionist')
INSERT [dbo].[Role] ([RoleID], [RoleName]) VALUES (4, N'Doctor')
INSERT [dbo].[Role] ([RoleID], [RoleName]) VALUES (5, N'Nurse')
INSERT [dbo].[Role] ([RoleID], [RoleName]) VALUES (6, N'Pharmacy Provider')
INSERT [dbo].[Role] ([RoleID], [RoleName]) VALUES (7, N'Clinic Manager')
INSERT [dbo].[Role] ([RoleID], [RoleName]) VALUES (8, N'Administrator')

SET IDENTITY_INSERT [dbo].[Role] OFF
GO
-- ============================================
-- INSERT ROOMS - MỖI BÁC SĨ 1 PHÒNG
-- ============================================
SET IDENTITY_INSERT [dbo].[Room] ON 

-- Nội khoa
INSERT [dbo].[Room] ([RoomID], [RoomName]) VALUES (1, N'Phòng nội khoa 101 - BS. Nguyễn Thị H')
INSERT [dbo].[Room] ([RoomID], [RoomName]) VALUES (5, N'Phòng nội khoa 102 - BS. Phạm Văn M')

-- Chuyên khoa
INSERT [dbo].[Room] ([RoomID], [RoomName]) VALUES (2, N'Phòng chuyên khoa 201 - BS. Nguyễn Văn A')
INSERT [dbo].[Room] ([RoomID], [RoomName]) VALUES (8, N'Phòng chuyên khoa 202 - BS. Lê Thị J')

-- Nhi khoa
INSERT [dbo].[Room] ([RoomID], [RoomName]) VALUES (3, N'Phòng nhi khoa 301 - BS. Nguyễn Thị N')
INSERT [dbo].[Room] ([RoomID], [RoomName]) VALUES (6, N'Phòng nhi khoa 302 - BS. Trần Văn O')

-- Da liễu
INSERT [dbo].[Room] ([RoomID], [RoomName]) VALUES (4, N'Phòng da liễu 401 - BS. Trần Văn I')
INSERT [dbo].[Room] ([RoomID], [RoomName]) VALUES (7, N'Phòng da liễu 402 - BS. Lê Thị P')

SET IDENTITY_INSERT [dbo].[Room] OFF
GO
SET IDENTITY_INSERT [dbo].[Service] ON 

INSERT [dbo].[Service] ([ServiceID], [ServiceName], [Description], [Price], [Category], [IsActive]) VALUES (13, N'Dermatology', N'Khám da li?u t?ng quát', CAST(200000.00 AS Decimal(18, 2)), N'Dermatology', 1)
INSERT [dbo].[Service] ([ServiceID], [ServiceName], [Description], [Price], [Category], [IsActive]) VALUES (14, N'InternalMed', N'Khám n?i t?ng quát', CAST(450000.00 AS Decimal(18, 2)), N'InternalMed', 1)
INSERT [dbo].[Service] ([ServiceID], [ServiceName], [Description], [Price], [Category], [IsActive]) VALUES (15, N'Pediatric', N'Khám nhi t?ng quát', CAST(300000.00 AS Decimal(18, 2)), N'Pediatric', 1)
INSERT [dbo].[Service] ([ServiceID], [ServiceName], [Description], [Price], [Category], [IsActive]) VALUES (16, N'Test máu tổng quát', N'Xét nghiệm máu tổng quát', CAST(150000.00 AS Decimal(18, 2)), N'Test', 1)
INSERT [dbo].[Service] ([ServiceID], [ServiceName], [Description], [Price], [Category], [IsActive]) VALUES (17, N'Test nước tiểu', N'Xét nghiệm nước tiểu', CAST(100000.00 AS Decimal(18, 2)), N'Test', 1)
INSERT [dbo].[Service] ([ServiceID], [ServiceName], [Description], [Price], [Category], [IsActive]) VALUES (18, N'Test đường huyết', N'Xét nghiệm đường huyết', CAST(80000.00 AS Decimal(18, 2)), N'Test', 1)
INSERT [dbo].[Service] ([ServiceID], [ServiceName], [Description], [Price], [Category], [IsActive]) VALUES (19, N'Test mỡ máu', N'Xét nghiệm mỡ máu', CAST(120000.00 AS Decimal(18, 2)), N'Test', 1)
INSERT [dbo].[Service] ([ServiceID], [ServiceName], [Description], [Price], [Category], [IsActive]) VALUES (20, N'Test chức năng gan', N'Xét nghiệm chức năng gan', CAST(180000.00 AS Decimal(18, 2)), N'Test', 1)
INSERT [dbo].[Service] ([ServiceID], [ServiceName], [Description], [Price], [Category], [IsActive]) VALUES (21, N'Test chức năng thận', N'Xét nghiệm chức năng thận', CAST(180000.00 AS Decimal(18, 2)), N'Test', 1)
INSERT [dbo].[Service] ([ServiceID], [ServiceName], [Description], [Price], [Category], [IsActive]) VALUES (22, N'Test viêm gan B', N'Xét nghiệm viêm gan B (HBsAg, Anti-HBs)', CAST(200000.00 AS Decimal(18, 2)), N'Test', 1)
INSERT [dbo].[Service] ([ServiceID], [ServiceName], [Description], [Price], [Category], [IsActive]) VALUES (23, N'Test COVID-19 nhanh', N'Xét nghiệm nhanh SARS-CoV-2', CAST(100000.00 AS Decimal(18, 2)), N'Test', 1)
SET IDENTITY_INSERT [dbo].[Service] OFF
GO
SET IDENTITY_INSERT [dbo].[Shift] ON 

INSERT [dbo].[Shift] ([ShiftID], [ShiftType], [StartTime], [EndTime]) VALUES (1, N'Sáng', CAST(N'08:00:00' AS Time), CAST(N'12:00:00' AS Time))
INSERT [dbo].[Shift] ([ShiftID], [ShiftType], [StartTime], [EndTime]) VALUES (2, N'Chiều', CAST(N'13:30:00' AS Time), CAST(N'17:30:00' AS Time))
INSERT [dbo].[Shift] ([ShiftID], [ShiftType], [StartTime], [EndTime]) VALUES (3, N'Tối', CAST(N'18:00:00' AS Time), CAST(N'22:00:00' AS Time))
SET IDENTITY_INSERT [dbo].[Shift] OFF
GO
SET IDENTITY_INSERT [dbo].[TestResult] ON 

INSERT [dbo].[TestResult] ([TestResultID], [RecordID], [ServiceID], [ResultValue], [Unit], [Attachment], [ResultDate], [Notes]) VALUES (1, 9, 20, N'ALT: 45 U/L, AST: 38 U/L', N'U/L', NULL, CAST(N'2025-11-21T18:55:00.000' AS DateTime), N'Kết quả men gan hơi cao, cần theo dõi chế độ ăn và tái khám sau 2 tuần')
INSERT [dbo].[TestResult] ([TestResultID], [RecordID], [ServiceID], [ResultValue], [Unit], [Attachment], [ResultDate], [Notes]) VALUES (2, 9, 20, N'PENDING', NULL, NULL, CAST(N'2025-11-22T01:59:56.623' AS DateTime), N'Chờ điều dưỡng cập nhật kết quả')
INSERT [dbo].[TestResult] ([TestResultID], [RecordID], [ServiceID], [ResultValue], [Unit], [Attachment], [ResultDate], [Notes]) VALUES (3, 9, 23, N'Âm tính (–)', NULL, NULL, CAST(N'2025-11-21T19:05:00.000' AS DateTime), N'Kết quả âm tính, không phát hiện kháng nguyên SARS-CoV-2')
SET IDENTITY_INSERT [dbo].[TestResult] OFF
GO
-- ============================================
-- INSERT USERS
-- ============================================
SET IDENTITY_INSERT [dbo].[User] ON 

-- Doctors (8 doctors - 2 per specialty)
-- Chuyên khoa Doctors
INSERT [dbo].[User] ([UserID], [Phone], [PasswordHash], [FullName], [Email], [DOB], [Gender], [RoleID], [IsActive], [Avatar], [EmailVerified]) VALUES (1, N'0905123456', N'$2a$11$uP69F9o4TwZP9ftmztyzB.oH/HCDKLCWNmAveQv.2rlKx.nfhcrIW', N'Nguyễn Văn A', N'a.nguyen@diamondhealth.vn', CAST(N'1990-05-15' AS Date), N'Nam', 4, 1, NULL, 1)
INSERT [dbo].[User] ([UserID], [Phone], [PasswordHash], [FullName], [Email], [DOB], [Gender], [RoleID], [IsActive], [Avatar], [EmailVerified]) VALUES (10, N'0905123459', N'$2a$11$uP69F9o4TwZP9ftmztyzB.oH/HCDKLCWNmAveQv.2rlKx.nfhcrIW', N'Lê Thị J', N'j.le@diamondhealth.vn', CAST(N'1985-11-10' AS Date), N'Nữ', 4, 1, NULL, 1)

-- Nội khoa Doctors
INSERT [dbo].[User] ([UserID], [Phone], [PasswordHash], [FullName], [Email], [DOB], [Gender], [RoleID], [IsActive], [Avatar], [EmailVerified]) VALUES (8, N'0905123457', N'$2a$11$uP69F9o4TwZP9ftmztyzB.oH/HCDKLCWNmAveQv.2rlKx.nfhcrIW', N'Nguyễn Thị H', N'h.nguyen@diamondhealth.vn', CAST(N'1988-08-20' AS Date), N'Nữ', 4, 1, NULL, 1)
INSERT [dbo].[User] ([UserID], [Phone], [PasswordHash], [FullName], [Email], [DOB], [Gender], [RoleID], [IsActive], [Avatar], [EmailVerified]) VALUES (13, N'0905123460', N'$2a$11$uP69F9o4TwZP9ftmztyzB.oH/HCDKLCWNmAveQv.2rlKx.nfhcrIW', N'Phạm Văn M', N'm.pham@diamondhealth.vn', CAST(N'1985-04-18' AS Date), N'Nam', 4, 1, NULL, 1)

-- Nhi khoa Doctors
INSERT [dbo].[User] ([UserID], [Phone], [PasswordHash], [FullName], [Email], [DOB], [Gender], [RoleID], [IsActive], [Avatar], [EmailVerified]) VALUES (14, N'0905123461', N'$2a$11$uP69F9o4TwZP9ftmztyzB.oH/HCDKLCWNmAveQv.2rlKx.nfhcrIW', N'Nguyễn Thị N', N'n.nguyen@diamondhealth.vn', CAST(N'1982-09-25' AS Date), N'Nữ', 4, 1, NULL, 1)
INSERT [dbo].[User] ([UserID], [Phone], [PasswordHash], [FullName], [Email], [DOB], [Gender], [RoleID], [IsActive], [Avatar], [EmailVerified]) VALUES (15, N'0905123462', N'$2a$11$uP69F9o4TwZP9ftmztyzB.oH/HCDKLCWNmAveQv.2rlKx.nfhcrIW', N'Trần Văn O', N'o.tran@diamondhealth.vn', CAST(N'1989-07-12' AS Date), N'Nam', 4, 1, NULL, 1)

-- Da liễu Doctors
INSERT [dbo].[User] ([UserID], [Phone], [PasswordHash], [FullName], [Email], [DOB], [Gender], [RoleID], [IsActive], [Avatar], [EmailVerified]) VALUES (9, N'0905123458', N'$2a$11$uP69F9o4TwZP9ftmztyzB.oH/HCDKLCWNmAveQv.2rlKx.nfhcrIW', N'Trần Văn I', N'i.tran@diamondhealth.vn', CAST(N'1992-03-15' AS Date), N'Nam', 4, 1, NULL, 1)
INSERT [dbo].[User] ([UserID], [Phone], [PasswordHash], [FullName], [Email], [DOB], [Gender], [RoleID], [IsActive], [Avatar], [EmailVerified]) VALUES (16, N'0905123463', N'$2a$11$uP69F9o4TwZP9ftmztyzB.oH/HCDKLCWNmAveQv.2rlKx.nfhcrIW', N'Lê Thị P', N'p.le@diamondhealth.vn', CAST(N'1991-11-08' AS Date), N'Nữ', 4, 1, NULL, 1)

-- Patients
INSERT [dbo].[User] ([UserID], [Phone], [PasswordHash], [FullName], [Email], [DOB], [Gender], [RoleID], [IsActive], [Avatar], [EmailVerified]) VALUES (2, N'0906123456', N'$2a$11$uP69F9o4TwZP9ftmztyzB.oH/HCDKLCWNmAveQv.2rlKx.nfhcrIW', N'Lê Thị B', N'b.le@diamondhealth.vn', CAST(N'1995-03-22' AS Date), N'Nữ', 2, 1, NULL, 1)
INSERT [dbo].[User] ([UserID], [Phone], [PasswordHash], [FullName], [Email], [DOB], [Gender], [RoleID], [IsActive], [Avatar], [EmailVerified]) VALUES (11, N'0906123458', N'$2a$11$uP69F9o4TwZP9ftmztyzB.oH/HCDKLCWNmAveQv.2rlKx.nfhcrIW', N'Phạm Văn K', N'k.pham@email.com', CAST(N'1990-06-15' AS Date), N'Nam', 2, 1, NULL, 1)
INSERT [dbo].[User] ([UserID], [Phone], [PasswordHash], [FullName], [Email], [DOB], [Gender], [RoleID], [IsActive], [Avatar], [EmailVerified]) VALUES (12, N'0906123459', N'$2a$11$uP69F9o4TwZP9ftmztyzB.oH/HCDKLCWNmAveQv.2rlKx.nfhcrIW', N'Hoàng Thị L', N'l.hoang@email.com', CAST(N'1987-12-03' AS Date), N'Nữ', 2, 1, NULL, 1)

-- Receptionist
INSERT [dbo].[User] ([UserID], [Phone], [PasswordHash], [FullName], [Email], [DOB], [Gender], [RoleID], [IsActive], [Avatar], [EmailVerified]) VALUES (3, N'0907123456', N'$2a$11$uP69F9o4TwZP9ftmztyzB.oH/HCDKLCWNmAveQv.2rlKx.nfhcrIW', N'Phạm Minh C', N'c.pham@diamondhealth.vn', CAST(N'1992-07-10' AS Date), N'Nam', 3, 1, NULL, 1)

-- Pharmacy Provider
INSERT [dbo].[User] ([UserID], [Phone], [PasswordHash], [FullName], [Email], [DOB], [Gender], [RoleID], [IsActive], [Avatar], [EmailVerified]) VALUES (4, N'0908123456', N'$2a$11$uP69F9o4TwZP9ftmztyzB.oH/HCDKLCWNmAveQv.2rlKx.nfhcrIW', N'Võ Tấn D', N'd.vo@diamondhealth.vn', CAST(N'1988-09-30' AS Date), N'Nam', 6, 1, NULL, 1)

-- Clinic Manager
INSERT [dbo].[User] ([UserID], [Phone], [PasswordHash], [FullName], [Email], [DOB], [Gender], [RoleID], [IsActive], [Avatar], [EmailVerified]) VALUES (5, N'0909123456', N'$2a$11$uP69F9o4TwZP9ftmztyzB.oH/HCDKLCWNmAveQv.2rlKx.nfhcrIW', N'Huỳnh Thị E', N'e.huynh@diamondhealth.vn', CAST(N'1985-12-05' AS Date), N'Nữ', 7, 1, NULL, 1)

-- Nurse
INSERT [dbo].[User] ([UserID], [Phone], [PasswordHash], [FullName], [Email], [DOB], [Gender], [RoleID], [IsActive], [Avatar], [EmailVerified]) VALUES (6, N'0910123456', N'$2a$11$uP69F9o4TwZP9ftmztyzB.oH/HCDKLCWNmAveQv.2rlKx.nfhcrIW', N'Trần Thị F', N'f.tran@diamondhealth.vn', CAST(N'1993-08-18' AS Date), N'Nữ', 5, 1, NULL, 1)

-- Administrator
INSERT [dbo].[User] ([UserID], [Phone], [PasswordHash], [FullName], [Email], [DOB], [Gender], [RoleID], [IsActive], [Avatar], [EmailVerified]) VALUES (7, N'0911123456', N'$2a$11$uP69F9o4TwZP9ftmztyzB.oH/HCDKLCWNmAveQv.2rlKx.nfhcrIW', N'Đặng Quốc G', N'g.dang@diamondhealth.vn', CAST(N'1980-01-20' AS Date), N'Nam', 8, 1, NULL, 1)
SET IDENTITY_INSERT [dbo].[User] OFF
GO
/****** Object:  Index [UQ__Doctor__1788CCAD324C6F8B]    Script Date: 22/11/2025 19:51:58 ******/
ALTER TABLE [dbo].[Doctor] ADD UNIQUE NONCLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_MedicineVersion_MedicineId]    Script Date: 22/11/2025 19:51:58 ******/
CREATE NONCLUSTERED INDEX [IX_MedicineVersion_MedicineId] ON [dbo].[MedicineVersion]
(
	[MedicineId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UQ__Patient__1788CCAD2F3F4D32]    Script Date: 22/11/2025 19:51:58 ******/
ALTER TABLE [dbo].[Patient] ADD UNIQUE NONCLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UQ__Pharmacy__1788CCADE6620A38]    Script Date: 22/11/2025 19:51:58 ******/
ALTER TABLE [dbo].[PharmacyProvider] ADD UNIQUE NONCLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UQ__Receptio__1788CCADFC6E9B96]    Script Date: 22/11/2025 19:51:58 ******/
ALTER TABLE [dbo].[Receptionist] ADD UNIQUE NONCLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__Role__8A2B61601E782E88]    Script Date: 22/11/2025 19:51:58 ******/
ALTER TABLE [dbo].[Role] ADD UNIQUE NONCLUSTERED 
(
	[RoleName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__User__5C7E359EC50A987C]    Script Date: 22/11/2025 19:51:58 ******/
ALTER TABLE [dbo].[User] ADD UNIQUE NONCLUSTERED 
(
	[Phone] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Appointment] ADD  DEFAULT ('Pending') FOR [Status]
GO
ALTER TABLE [dbo].[Appointment] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[DermatologyRecord] ADD  DEFAULT (getdate()) FOR [PerformedAt]
GO
ALTER TABLE [dbo].[DoctorShift] ADD  DEFAULT ('Active') FOR [Status]
GO
ALTER TABLE [dbo].[DoctorShiftExchange] ADD  DEFAULT ('Pending') FOR [Status]
GO
ALTER TABLE [dbo].[DoctorShiftExchange] ADD  DEFAULT ('Temporary') FOR [SwapType]
GO
ALTER TABLE [dbo].[MedicalRecord] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[MedicalService] ADD  DEFAULT ((1)) FOR [Quantity]
GO
ALTER TABLE [dbo].[MedicalService] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Medicine] ADD  DEFAULT ('Available') FOR [Status]
GO
ALTER TABLE [dbo].[MedicineVersion] ADD  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Notification] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Notification] ADD  DEFAULT ((0)) FOR [IsGlobal]
GO
ALTER TABLE [dbo].[Notification] ADD  DEFAULT ((0)) FOR [IsEmailSent]
GO
ALTER TABLE [dbo].[NotificationReceiver] ADD  DEFAULT ((0)) FOR [IsRead]
GO
ALTER TABLE [dbo].[Payment] ADD  DEFAULT (getdate()) FOR [PaymentDate]
GO
ALTER TABLE [dbo].[Payment] ADD  DEFAULT ('Pending') FOR [Status]
GO
ALTER TABLE [dbo].[Prescription] ADD  DEFAULT (getdate()) FOR [IssuedDate]
GO
ALTER TABLE [dbo].[Service] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[TestResult] ADD  DEFAULT (getdate()) FOR [ResultDate]
GO
ALTER TABLE [dbo].[User] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[User] ADD  DEFAULT ((0)) FOR [EmailVerified]
GO
ALTER TABLE [dbo].[Appointment]  WITH CHECK ADD FOREIGN KEY([DoctorID])
REFERENCES [dbo].[Doctor] ([DoctorID])
GO
ALTER TABLE [dbo].[Appointment]  WITH CHECK ADD FOREIGN KEY([PatientID])
REFERENCES [dbo].[Patient] ([PatientID])
GO
ALTER TABLE [dbo].[Appointment]  WITH CHECK ADD FOREIGN KEY([ReceptionistID])
REFERENCES [dbo].[Receptionist] ([ReceptionistID])
GO
ALTER TABLE [dbo].[Appointment]  WITH CHECK ADD FOREIGN KEY([UpdatedBy])
REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[DermatologyRecord]  WITH CHECK ADD FOREIGN KEY([PerformedByUserID])
REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[DermatologyRecord]  WITH CHECK ADD FOREIGN KEY([RecordID])
REFERENCES [dbo].[MedicalRecord] ([RecordID])
GO
ALTER TABLE [dbo].[Doctor]  WITH CHECK ADD FOREIGN KEY([UserID])
REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[Doctor]  WITH CHECK ADD  CONSTRAINT [FK_Doctor_Room] FOREIGN KEY([RoomID])
REFERENCES [dbo].[Room] ([RoomID])
GO
ALTER TABLE [dbo].[Doctor] CHECK CONSTRAINT [FK_Doctor_Room]
GO
ALTER TABLE [dbo].[DoctorShift]  WITH CHECK ADD FOREIGN KEY([DoctorID])
REFERENCES [dbo].[Doctor] ([DoctorID])
GO
ALTER TABLE [dbo].[DoctorShift]  WITH CHECK ADD FOREIGN KEY([ShiftID])
REFERENCES [dbo].[Shift] ([ShiftID])
GO
ALTER TABLE [dbo].[DoctorShiftExchange]  WITH CHECK ADD FOREIGN KEY([Doctor1ID])
REFERENCES [dbo].[Doctor] ([DoctorID])
GO
ALTER TABLE [dbo].[DoctorShiftExchange]  WITH CHECK ADD FOREIGN KEY([Doctor1ShiftRefID])
REFERENCES [dbo].[DoctorShift] ([DoctorShiftID])
GO
ALTER TABLE [dbo].[DoctorShiftExchange]  WITH CHECK ADD FOREIGN KEY([Doctor2ID])
REFERENCES [dbo].[Doctor] ([DoctorID])
GO
ALTER TABLE [dbo].[DoctorShiftExchange]  WITH CHECK ADD FOREIGN KEY([Doctor2ShiftRefID])
REFERENCES [dbo].[DoctorShift] ([DoctorShiftID])
GO
ALTER TABLE [dbo].[InternalMedRecord]  WITH CHECK ADD FOREIGN KEY([RecordID])
REFERENCES [dbo].[MedicalRecord] ([RecordID])
GO
ALTER TABLE [dbo].[MedicalRecord]  WITH CHECK ADD FOREIGN KEY([AppointmentID])
REFERENCES [dbo].[Appointment] ([AppointmentID])
GO
ALTER TABLE [dbo].[MedicalService]  WITH CHECK ADD  CONSTRAINT [FK_MedicalService_Record] FOREIGN KEY([RecordID])
REFERENCES [dbo].[MedicalRecord] ([RecordID])
GO
ALTER TABLE [dbo].[MedicalService] CHECK CONSTRAINT [FK_MedicalService_Record]
GO
ALTER TABLE [dbo].[MedicalService]  WITH CHECK ADD  CONSTRAINT [FK_MedicalService_Service] FOREIGN KEY([ServiceID])
REFERENCES [dbo].[Service] ([ServiceID])
GO
ALTER TABLE [dbo].[MedicalService] CHECK CONSTRAINT [FK_MedicalService_Service]
GO
ALTER TABLE [dbo].[Medicine]  WITH CHECK ADD FOREIGN KEY([ProviderID])
REFERENCES [dbo].[PharmacyProvider] ([ProviderID])
GO
ALTER TABLE [dbo].[MedicineVersion]  WITH CHECK ADD  CONSTRAINT [FK_MedicineVersion_Medicine] FOREIGN KEY([MedicineId])
REFERENCES [dbo].[Medicine] ([MedicineID])
GO
ALTER TABLE [dbo].[MedicineVersion] CHECK CONSTRAINT [FK_MedicineVersion_Medicine]
GO
ALTER TABLE [dbo].[Notification]  WITH CHECK ADD  CONSTRAINT [FK_Notification_User] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[User] ([UserID])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[Notification] CHECK CONSTRAINT [FK_Notification_User]
GO
ALTER TABLE [dbo].[NotificationReceiver]  WITH CHECK ADD  CONSTRAINT [FK_NotificationReceiver_Notification] FOREIGN KEY([NotificationId])
REFERENCES [dbo].[Notification] ([NotificationId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[NotificationReceiver] CHECK CONSTRAINT [FK_NotificationReceiver_Notification]
GO
ALTER TABLE [dbo].[NotificationReceiver]  WITH CHECK ADD  CONSTRAINT [FK_NotificationReceiver_User] FOREIGN KEY([ReceiverId])
REFERENCES [dbo].[User] ([UserID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[NotificationReceiver] CHECK CONSTRAINT [FK_NotificationReceiver_User]
GO
ALTER TABLE [dbo].[Patient]  WITH CHECK ADD FOREIGN KEY([UserID])
REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[Payment]  WITH CHECK ADD FOREIGN KEY([RecordID])
REFERENCES [dbo].[MedicalRecord] ([RecordID])
GO
ALTER TABLE [dbo].[PediatricRecord]  WITH CHECK ADD FOREIGN KEY([RecordID])
REFERENCES [dbo].[MedicalRecord] ([RecordID])
GO
ALTER TABLE [dbo].[PharmacyProvider]  WITH CHECK ADD FOREIGN KEY([UserID])
REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[Prescription]  WITH CHECK ADD FOREIGN KEY([DoctorID])
REFERENCES [dbo].[Doctor] ([DoctorID])
GO
ALTER TABLE [dbo].[Prescription]  WITH CHECK ADD FOREIGN KEY([RecordID])
REFERENCES [dbo].[MedicalRecord] ([RecordID])
GO
ALTER TABLE [dbo].[PrescriptionDetail]  WITH CHECK ADD FOREIGN KEY([PrescriptionID])
REFERENCES [dbo].[Prescription] ([PrescriptionID])
GO
ALTER TABLE [dbo].[PrescriptionDetail]  WITH CHECK ADD  CONSTRAINT [FK_PrescriptionDetail_MedicineVersion] FOREIGN KEY([MedicineVersionId])
REFERENCES [dbo].[MedicineVersion] ([MedicineVersionId])
GO
ALTER TABLE [dbo].[PrescriptionDetail] CHECK CONSTRAINT [FK_PrescriptionDetail_MedicineVersion]
GO
ALTER TABLE [dbo].[Receptionist]  WITH CHECK ADD FOREIGN KEY([UserID])
REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[TestResult]  WITH CHECK ADD FOREIGN KEY([RecordID])
REFERENCES [dbo].[MedicalRecord] ([RecordID])
GO
ALTER TABLE [dbo].[TestResult]  WITH CHECK ADD  CONSTRAINT [FK_TestResult_Service] FOREIGN KEY([ServiceID])
REFERENCES [dbo].[Service] ([ServiceID])
GO
ALTER TABLE [dbo].[TestResult] CHECK CONSTRAINT [FK_TestResult_Service]
GO
ALTER TABLE [dbo].[User]  WITH CHECK ADD FOREIGN KEY([RoleID])
REFERENCES [dbo].[Role] ([RoleID])
GO
USE [master]
GO
ALTER DATABASE [DiamondHealth] SET  READ_WRITE 
GO
