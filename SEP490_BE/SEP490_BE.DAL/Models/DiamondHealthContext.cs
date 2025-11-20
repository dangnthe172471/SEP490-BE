using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SEP490_BE.DAL.Models;

public partial class DiamondHealthContext : DbContext
{
    public DiamondHealthContext()
    {
    }

    public DiamondHealthContext(DbContextOptions<DiamondHealthContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<ChatLog> ChatLogs { get; set; }

    public virtual DbSet<DermatologyRecord> DermatologyRecords { get; set; }

    public virtual DbSet<Doctor> Doctors { get; set; }

    public virtual DbSet<DoctorShift> DoctorShifts { get; set; }

    public virtual DbSet<DoctorShiftExchange> DoctorShiftExchanges { get; set; }

    public virtual DbSet<InternalMedRecord> InternalMedRecords { get; set; }

    public virtual DbSet<MedicalRecord> MedicalRecords { get; set; }

    public virtual DbSet<MedicalService> MedicalServices { get; set; }

    public virtual DbSet<Medicine> Medicines { get; set; }

    public virtual DbSet<MedicineVersion> MedicineVersions { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NotificationReceiver> NotificationReceivers { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PediatricRecord> PediatricRecords { get; set; }

    public virtual DbSet<PharmacyProvider> PharmacyProviders { get; set; }

    public virtual DbSet<Prescription> Prescriptions { get; set; }

    public virtual DbSet<PrescriptionDetail> PrescriptionDetails { get; set; }

    public virtual DbSet<Receptionist> Receptionists { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<Shift> Shifts { get; set; }

    public virtual DbSet<TestResult> TestResults { get; set; }

    public virtual DbSet<TestType> TestTypes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PK__Appointm__8ECDFCA2991E5F49");

            entity.ToTable("Appointment");

            entity.Property(e => e.AppointmentId).HasColumnName("AppointmentID");
            entity.Property(e => e.AppointmentDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DoctorId).HasColumnName("DoctorID");
            entity.Property(e => e.PatientId).HasColumnName("PatientID");
            entity.Property(e => e.ReasonForVisit).HasMaxLength(500);
            entity.Property(e => e.ReceptionistId).HasColumnName("ReceptionistID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Appointme__Docto__619B8048");

            entity.HasOne(d => d.Patient).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Appointme__Patie__60A75C0F");

            entity.HasOne(d => d.Receptionist).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.ReceptionistId)
                .HasConstraintName("FK__Appointme__Recep__628FA481");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK__Appointme__Updat__6383C8BA");
        });

        modelBuilder.Entity<ChatLog>(entity =>
        {
            entity.HasKey(e => e.ChatId).HasName("PK__ChatLog__A9FBE626922A258A");

            entity.ToTable("ChatLog");

            entity.Property(e => e.ChatId).HasColumnName("ChatID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PatientId).HasColumnName("PatientID");
            entity.Property(e => e.ReceptionistId).HasColumnName("ReceptionistID");

            entity.HasOne(d => d.Patient).WithMany(p => p.ChatLogs)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChatLog__Patient__0B91BA14");

            entity.HasOne(d => d.Receptionist).WithMany(p => p.ChatLogs)
                .HasForeignKey(d => d.ReceptionistId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChatLog__Recepti__0C85DE4D");
        });

        modelBuilder.Entity<DermatologyRecord>(entity =>
        {
            entity.HasKey(e => e.DermRecordId).HasName("PK__Dermatol__39ED6A60E79CBBE3");

            entity.ToTable("DermatologyRecord");

            entity.Property(e => e.DermRecordId).HasColumnName("DermRecordID");
            entity.Property(e => e.Attachment).HasMaxLength(255);
            entity.Property(e => e.BodyArea).HasMaxLength(200);
            entity.Property(e => e.PerformedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PerformedByUserId).HasColumnName("PerformedByUserID");
            entity.Property(e => e.RecordId).HasColumnName("RecordID");
            entity.Property(e => e.RequestedProcedure).HasMaxLength(200);

            entity.HasOne(d => d.PerformedByUser).WithMany(p => p.DermatologyRecords)
                .HasForeignKey(d => d.PerformedByUserId)
                .HasConstraintName("FK__Dermatolo__Perfo__2B0A656D");

            entity.HasOne(d => d.Record).WithMany(p => p.DermatologyRecords)
                .HasForeignKey(d => d.RecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Dermatolo__Recor__2A164134");
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(e => e.DoctorId).HasName("PK__Doctor__2DC00EDF9936CCAA");

            entity.ToTable("Doctor");

            entity.HasIndex(e => e.UserId, "UQ__Doctor__1788CCAD324C6F8B").IsUnique();

            entity.Property(e => e.DoctorId)
                .ValueGeneratedNever()
                .HasColumnName("DoctorID");
            entity.Property(e => e.RoomId).HasColumnName("RoomID");
            entity.Property(e => e.Specialty).HasMaxLength(100);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Room).WithMany(p => p.Doctors)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Doctor_Room");

            entity.HasOne(d => d.User).WithOne(p => p.Doctor)
                .HasForeignKey<Doctor>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Doctor__UserID__403A8C7D");
        });

        modelBuilder.Entity<DoctorShift>(entity =>
        {
            entity.HasKey(e => e.DoctorShiftId).HasName("PK__DoctorSh__9BD0D8BBAB628D3D");

            entity.ToTable("DoctorShift");

            entity.Property(e => e.DoctorShiftId).HasColumnName("DoctorShiftID");
            entity.Property(e => e.DoctorId).HasColumnName("DoctorID");
            entity.Property(e => e.ShiftId).HasColumnName("ShiftID");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasDefaultValue("Active");

            entity.HasOne(d => d.Doctor).WithMany(p => p.DoctorShifts)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DoctorShi__Docto__5441852A");

            entity.HasOne(d => d.Shift).WithMany(p => p.DoctorShifts)
                .HasForeignKey(d => d.ShiftId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DoctorShi__Shift__5535A963");
        });

        modelBuilder.Entity<DoctorShiftExchange>(entity =>
        {
            entity.HasKey(e => e.ExchangeId).HasName("PK__DoctorSh__72E600AB88307F87");

            entity.ToTable("DoctorShiftExchange");

            entity.Property(e => e.ExchangeId).HasColumnName("ExchangeID");
            entity.Property(e => e.Doctor1Id).HasColumnName("Doctor1ID");
            entity.Property(e => e.Doctor1ShiftRefId).HasColumnName("Doctor1ShiftRefID");
            entity.Property(e => e.Doctor2Id).HasColumnName("Doctor2ID");
            entity.Property(e => e.Doctor2ShiftRefId).HasColumnName("Doctor2ShiftRefID");
            entity.Property(e => e.DoctorOld1ShiftId).HasColumnName("DoctorOld1ShiftID");
            entity.Property(e => e.DoctorOld2ShiftId).HasColumnName("DoctorOld2ShiftID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.SwapType)
                .HasMaxLength(20)
                .HasDefaultValue("Temporary");

            entity.HasOne(d => d.Doctor1).WithMany(p => p.DoctorShiftExchangeDoctor1s)
                .HasForeignKey(d => d.Doctor1Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DoctorShi__Docto__59063A47");

            entity.HasOne(d => d.Doctor1ShiftRef).WithMany(p => p.DoctorShiftExchangeDoctor1ShiftRefs)
                .HasForeignKey(d => d.Doctor1ShiftRefId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DoctorShi__Docto__59FA5E80");

            entity.HasOne(d => d.Doctor2).WithMany(p => p.DoctorShiftExchangeDoctor2s)
                .HasForeignKey(d => d.Doctor2Id)
                .HasConstraintName("FK__DoctorShi__Docto__5AEE82B9");

            entity.HasOne(d => d.Doctor2ShiftRef).WithMany(p => p.DoctorShiftExchangeDoctor2ShiftRefs)
                .HasForeignKey(d => d.Doctor2ShiftRefId)
                .HasConstraintName("FK__DoctorShi__Docto__5BE2A6F2");
        });

        modelBuilder.Entity<InternalMedRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("PK__Internal__FBDF78C92C4DE5CD");

            entity.ToTable("InternalMedRecord");

            entity.Property(e => e.RecordId)
                .ValueGeneratedNever()
                .HasColumnName("RecordID");
            entity.Property(e => e.BloodSugar).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.Notes).HasMaxLength(255);

            entity.HasOne(d => d.Record).WithOne(p => p.InternalMedRecord)
                .HasForeignKey<InternalMedRecord>(d => d.RecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__InternalM__Recor__6FE99F9F");
        });

        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("PK__MedicalR__FBDF78C9EB718FFE");

            entity.ToTable("MedicalRecord");

            entity.Property(e => e.RecordId).HasColumnName("RecordID");
            entity.Property(e => e.AppointmentId).HasColumnName("AppointmentID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Appointment).WithMany(p => p.MedicalRecords)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MedicalRe__Appoi__6754599E");
        });

        modelBuilder.Entity<MedicalService>(entity =>
        {
            entity.HasKey(e => e.MedicalServiceId).HasName("PK__MedicalS__BA70D88B65FF3C66");

            entity.ToTable("MedicalService");

            entity.Property(e => e.MedicalServiceId).HasColumnName("MedicalServiceID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.Quantity).HasDefaultValue(1);
            entity.Property(e => e.RecordId).HasColumnName("RecordID");
            entity.Property(e => e.ServiceId).HasColumnName("ServiceID");
            entity.Property(e => e.TotalPrice)
                .HasComputedColumnSql("([Quantity]*[UnitPrice])", true)
                .HasColumnType("decimal(29, 2)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Record).WithMany(p => p.MedicalServices)
                .HasForeignKey(d => d.RecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MedicalService_Record");

            entity.HasOne(d => d.Service).WithMany(p => p.MedicalServices)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MedicalService_Service");
        });

        modelBuilder.Entity<Medicine>(entity =>
        {
            entity.HasKey(e => e.MedicineId).HasName("PK__Medicine__4F2128F03C19868F");

            entity.ToTable("Medicine");

            entity.Property(e => e.MedicineId).HasColumnName("MedicineID");
            entity.Property(e => e.ActiveIngredient).HasMaxLength(200);
            entity.Property(e => e.DosageForm).HasMaxLength(100);
            entity.Property(e => e.MedicineName).HasMaxLength(100);
            entity.Property(e => e.NoteForDoctor).HasMaxLength(500);
            entity.Property(e => e.PackSize).HasMaxLength(100);
            entity.Property(e => e.PrescriptionUnit).HasMaxLength(50);
            entity.Property(e => e.ProviderId).HasColumnName("ProviderID");
            entity.Property(e => e.Route).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Available");
            entity.Property(e => e.Strength).HasMaxLength(50);
            entity.Property(e => e.TherapeuticClass).HasMaxLength(100);

            entity.HasOne(d => d.Provider).WithMany(p => p.Medicines)
                .HasForeignKey(d => d.ProviderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Medicine__Provid__7A672E12");
        });

        modelBuilder.Entity<MedicineVersion>(entity =>
        {
            entity.HasKey(e => e.MedicineVersionId).HasName("PK__Medicine__624E46FD00BA40A1");

            entity.ToTable("MedicineVersion");

            entity.HasIndex(e => e.MedicineId, "IX_MedicineVersion_MedicineId");

            entity.Property(e => e.ActiveIngredient).HasMaxLength(200);
            entity.Property(e => e.CommonSideEffects).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DosageForm).HasMaxLength(100);
            entity.Property(e => e.MedicineName).HasMaxLength(200);
            entity.Property(e => e.NoteForDoctor).HasMaxLength(500);
            entity.Property(e => e.PackSize).HasMaxLength(100);
            entity.Property(e => e.PrescriptionUnit).HasMaxLength(50);
            entity.Property(e => e.ProviderContact).HasMaxLength(100);
            entity.Property(e => e.ProviderName).HasMaxLength(100);
            entity.Property(e => e.Route).HasMaxLength(100);
            entity.Property(e => e.Strength).HasMaxLength(100);
            entity.Property(e => e.TherapeuticClass).HasMaxLength(200);

            entity.HasOne(d => d.Medicine).WithMany(p => p.MedicineVersions)
                .HasForeignKey(d => d.MedicineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MedicineVersion_Medicine");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E1299C56965");

            entity.ToTable("Notification");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Type).HasMaxLength(50);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Notification_User");
        });

        modelBuilder.Entity<NotificationReceiver>(entity =>
        {
            entity.HasKey(e => new { e.NotificationId, e.ReceiverId });

            entity.ToTable("NotificationReceiver");

            entity.Property(e => e.ReadDate).HasColumnType("datetime");

            entity.HasOne(d => d.Notification).WithMany(p => p.NotificationReceivers)
                .HasForeignKey(d => d.NotificationId)
                .HasConstraintName("FK_NotificationReceiver_Notification");

            entity.HasOne(d => d.Receiver).WithMany(p => p.NotificationReceivers)
                .HasForeignKey(d => d.ReceiverId)
                .HasConstraintName("FK_NotificationReceiver_User");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.PatientId).HasName("PK__Patient__970EC3468E76D6BA");

            entity.ToTable("Patient");

            entity.HasIndex(e => e.UserId, "UQ__Patient__1788CCAD2F3F4D32").IsUnique();

            entity.Property(e => e.PatientId)
                .ValueGeneratedNever()
                .HasColumnName("PatientID");
            entity.Property(e => e.Allergies).HasMaxLength(500);
            entity.Property(e => e.MedicalHistory).HasMaxLength(500);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithOne(p => p.Patient)
                .HasForeignKey<Patient>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Patient__UserID__440B1D61");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payment__9B556A5808F5F740");

            entity.ToTable("Payment");

            entity.Property(e => e.PaymentId).HasColumnName("PaymentID");
            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CheckoutUrl).HasMaxLength(500);
            entity.Property(e => e.Method).HasMaxLength(50);
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RecordId).HasColumnName("RecordID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.Record).WithMany(p => p.Payments)
                .HasForeignKey(d => d.RecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payment__RecordI__07C12930");
        });

        modelBuilder.Entity<PediatricRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("PK__Pediatri__FBDF78C9C71E37AE");

            entity.ToTable("PediatricRecord");

            entity.Property(e => e.RecordId)
                .ValueGeneratedNever()
                .HasColumnName("RecordID");
            entity.Property(e => e.HeightCm).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TemperatureC).HasColumnType("decimal(4, 1)");
            entity.Property(e => e.WeightKg).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Record).WithOne(p => p.PediatricRecord)
                .HasForeignKey<PediatricRecord>(d => d.RecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Pediatric__Recor__6D0D32F4");
        });

        modelBuilder.Entity<PharmacyProvider>(entity =>
        {
            entity.HasKey(e => e.ProviderId).HasName("PK__Pharmacy__B54C689DBF66C37D");

            entity.ToTable("PharmacyProvider");

            entity.HasIndex(e => e.UserId, "UQ__Pharmacy__1788CCADE6620A38").IsUnique();

            entity.Property(e => e.ProviderId)
                .ValueGeneratedNever()
                .HasColumnName("ProviderID");
            entity.Property(e => e.Contact).HasMaxLength(100);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithOne(p => p.PharmacyProvider)
                .HasForeignKey<PharmacyProvider>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PharmacyP__UserI__4BAC3F29");
        });

        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasKey(e => e.PrescriptionId).HasName("PK__Prescrip__401308125A905D15");

            entity.ToTable("Prescription");

            entity.Property(e => e.PrescriptionId).HasColumnName("PrescriptionID");
            entity.Property(e => e.DoctorId).HasColumnName("DoctorID");
            entity.Property(e => e.IssuedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RecordId).HasColumnName("RecordID");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Prescript__Docto__7F2BE32F");

            entity.HasOne(d => d.Record).WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.RecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Prescript__Recor__7E37BEF6");
        });

        modelBuilder.Entity<PrescriptionDetail>(entity =>
        {
            entity.HasKey(e => e.PrescriptionDetailId).HasName("PK__Prescrip__6DB7668ABC208CC9");

            entity.ToTable("PrescriptionDetail");

            entity.Property(e => e.PrescriptionDetailId).HasColumnName("PrescriptionDetailID");
            entity.Property(e => e.Dosage).HasMaxLength(100);
            entity.Property(e => e.Duration).HasMaxLength(50);
            entity.Property(e => e.Instruction).HasMaxLength(200);
            entity.Property(e => e.PrescriptionId).HasColumnName("PrescriptionID");

            entity.HasOne(d => d.MedicineVersion).WithMany(p => p.PrescriptionDetails)
                .HasForeignKey(d => d.MedicineVersionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PrescriptionDetail_MedicineVersion");

            entity.HasOne(d => d.Prescription).WithMany(p => p.PrescriptionDetails)
                .HasForeignKey(d => d.PrescriptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Prescript__Presc__02084FDA");
        });

        modelBuilder.Entity<Receptionist>(entity =>
        {
            entity.HasKey(e => e.ReceptionistId).HasName("PK__Receptio__0F8C2048CA768671");

            entity.ToTable("Receptionist");

            entity.HasIndex(e => e.UserId, "UQ__Receptio__1788CCADFC6E9B96").IsUnique();

            entity.Property(e => e.ReceptionistId)
                .ValueGeneratedNever()
                .HasColumnName("ReceptionistID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithOne(p => p.Receptionist)
                .HasForeignKey<Receptionist>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reception__UserI__47DBAE45");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__8AFACE3A6F58044E");

            entity.ToTable("Role");

            entity.HasIndex(e => e.RoleName, "UQ__Role__8A2B61601E782E88").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.RoleName).HasMaxLength(30);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("PK__Room__328639192E3448E9");

            entity.ToTable("Room");

            entity.Property(e => e.RoomId).HasColumnName("RoomID");
            entity.Property(e => e.RoomName).HasMaxLength(50);
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK__Service__C51BB0EA985F0947");

            entity.ToTable("Service");

            entity.Property(e => e.ServiceId).HasColumnName("ServiceID");
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ServiceName).HasMaxLength(150);

            entity.HasOne(d => d.TestType).WithMany(p => p.Services)
                .HasForeignKey(d => d.TestTypeId)
                .HasConstraintName("FK_Service_TestType");
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.HasKey(e => e.ShiftId).HasName("PK__Shift__C0A838E11D8E5257");

            entity.ToTable("Shift");

            entity.Property(e => e.ShiftId).HasColumnName("ShiftID");
            entity.Property(e => e.ShiftType).HasMaxLength(20);
        });

        modelBuilder.Entity<TestResult>(entity =>
        {
            entity.HasKey(e => e.TestResultId).HasName("PK__TestResu__E2463A6792E63995");

            entity.ToTable("TestResult");

            entity.Property(e => e.TestResultId).HasColumnName("TestResultID");
            entity.Property(e => e.Attachment).HasMaxLength(255);
            entity.Property(e => e.Notes).HasMaxLength(255);
            entity.Property(e => e.RecordId).HasColumnName("RecordID");
            entity.Property(e => e.ResultDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ResultValue).HasMaxLength(100);
            entity.Property(e => e.TestTypeId).HasColumnName("TestTypeID");
            entity.Property(e => e.Unit).HasMaxLength(50);

            entity.HasOne(d => d.Record).WithMany(p => p.TestResults)
                .HasForeignKey(d => d.RecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TestResul__Recor__75A278F5");

            entity.HasOne(d => d.TestType).WithMany(p => p.TestResults)
                .HasForeignKey(d => d.TestTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TestResul__TestT__76969D2E");
        });

        modelBuilder.Entity<TestType>(entity =>
        {
            entity.HasKey(e => e.TestTypeId).HasName("PK__TestType__9BB876469E18DD8C");

            entity.ToTable("TestType");

            entity.Property(e => e.TestTypeId).HasColumnName("TestTypeID");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.TestName).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CCACEDA4FEE9");

            entity.ToTable("User");

            entity.HasIndex(e => e.Phone, "UQ__User__5C7E359EC50A987C").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Avatar).HasMaxLength(500);
            entity.Property(e => e.Dob).HasColumnName("DOB");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__User__RoleID__3C69FB99");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
