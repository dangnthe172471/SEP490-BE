﻿using System;
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

    public virtual DbSet<Doctor> Doctors { get; set; }

    public virtual DbSet<DoctorShift> DoctorShifts { get; set; }

    public virtual DbSet<DoctorShiftExchange> DoctorShiftExchanges { get; set; }

    public virtual DbSet<InternalMedRecord> InternalMedRecords { get; set; }

    public virtual DbSet<MedicalRecord> MedicalRecords { get; set; }

    public virtual DbSet<Medicine> Medicines { get; set; }

    public virtual DbSet<ObstetricRecord> ObstetricRecords { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PediatricRecord> PediatricRecords { get; set; }

    public virtual DbSet<PharmacyProvider> PharmacyProviders { get; set; }

    public virtual DbSet<Prescription> Prescriptions { get; set; }

    public virtual DbSet<PrescriptionDetail> PrescriptionDetails { get; set; }

    public virtual DbSet<Receptionist> Receptionists { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<Shift> Shifts { get; set; }

    public virtual DbSet<TestResult> TestResults { get; set; }

    public virtual DbSet<TestType> TestTypes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PK__Appointm__8ECDFCA221D6DCC4");

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
                .HasConstraintName("FK__Appointme__Docto__60A75C0F");

            entity.HasOne(d => d.Patient).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Appointme__Patie__5FB337D6");

            entity.HasOne(d => d.Receptionist).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.ReceptionistId)
                .HasConstraintName("FK__Appointme__Recep__619B8048");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK__Appointme__Updat__628FA481");
        });

        modelBuilder.Entity<ChatLog>(entity =>
        {
            entity.HasKey(e => e.ChatId).HasName("PK__ChatLog__A9FBE6262357BE48");

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
                .HasConstraintName("FK__ChatLog__Patient__0A9D95DB");

            entity.HasOne(d => d.Receptionist).WithMany(p => p.ChatLogs)
                .HasForeignKey(d => d.ReceptionistId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChatLog__Recepti__0B91BA14");
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(e => e.DoctorId).HasName("PK__Doctor__2DC00EDF1E089CE5");

            entity.ToTable("Doctor");

            entity.HasIndex(e => e.UserId, "UQ__Doctor__1788CCADA4D32528").IsUnique();

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
            entity.HasKey(e => e.DoctorShiftId).HasName("PK__DoctorSh__9BD0D8BB1C880FDF");

            entity.ToTable("DoctorShift");

            entity.Property(e => e.DoctorShiftId).HasColumnName("DoctorShiftID");
            entity.Property(e => e.DoctorId).HasColumnName("DoctorID");
            entity.Property(e => e.ShiftId).HasColumnName("ShiftID");
            entity.Property(e => e.Status).HasMaxLength(255);

            entity.HasOne(d => d.Doctor).WithMany(p => p.DoctorShifts)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DoctorShi__Docto__534D60F1");

            entity.HasOne(d => d.Shift).WithMany(p => p.DoctorShifts)
                .HasForeignKey(d => d.ShiftId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DoctorShi__Shift__5441852A");
        });

        modelBuilder.Entity<DoctorShiftExchange>(entity =>
        {
            entity.HasKey(e => e.ExchangeId).HasName("PK__DoctorSh__72E600AB9C94CBEE");

            entity.ToTable("DoctorShiftExchange");

            entity.Property(e => e.ExchangeId).HasColumnName("ExchangeID");
            entity.Property(e => e.Doctor1Id).HasColumnName("Doctor1ID");
            entity.Property(e => e.Doctor1ShiftRefId).HasColumnName("Doctor1ShiftRefID");
            entity.Property(e => e.Doctor2Id).HasColumnName("Doctor2ID");
            entity.Property(e => e.Doctor2ShiftRefId).HasColumnName("Doctor2ShiftRefID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Approved");
            entity.Property(e => e.SwapType)
                .HasMaxLength(20)
                .HasDefaultValue("Temporary");

            entity.HasOne(d => d.Doctor1).WithMany(p => p.DoctorShiftExchangeDoctor1s)
                .HasForeignKey(d => d.Doctor1Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DoctorShi__Docto__5812160E");

            entity.HasOne(d => d.Doctor1ShiftRef).WithMany(p => p.DoctorShiftExchangeDoctor1ShiftRefs)
                .HasForeignKey(d => d.Doctor1ShiftRefId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DoctorShi__Docto__59063A47");

            entity.HasOne(d => d.Doctor2).WithMany(p => p.DoctorShiftExchangeDoctor2s)
                .HasForeignKey(d => d.Doctor2Id)
                .HasConstraintName("FK__DoctorShi__Docto__59FA5E80");

            entity.HasOne(d => d.Doctor2ShiftRef).WithMany(p => p.DoctorShiftExchangeDoctor2ShiftRefs)
                .HasForeignKey(d => d.Doctor2ShiftRefId)
                .HasConstraintName("FK__DoctorShi__Docto__5AEE82B9");
        });

        modelBuilder.Entity<InternalMedRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("PK__Internal__FBDF78C9DC7C5E3A");

            entity.ToTable("InternalMedRecord");

            entity.Property(e => e.RecordId)
                .ValueGeneratedNever()
                .HasColumnName("RecordID");
            entity.Property(e => e.BloodSugar).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.Notes).HasMaxLength(255);

            entity.HasOne(d => d.Record).WithOne(p => p.InternalMedRecord)
                .HasForeignKey<InternalMedRecord>(d => d.RecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__InternalM__Recor__6EF57B66");
        });

        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("PK__MedicalR__FBDF78C9BDA19569");

            entity.ToTable("MedicalRecord");

            entity.Property(e => e.RecordId).HasColumnName("RecordID");
            entity.Property(e => e.AppointmentId).HasColumnName("AppointmentID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            // Ensure one MedicalRecord per Appointment
            entity.HasIndex(e => e.AppointmentId)
                .IsUnique();

            entity.HasOne(d => d.Appointment).WithMany(p => p.MedicalRecords)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MedicalRe__Appoi__66603565");
        });

        modelBuilder.Entity<Medicine>(entity =>
        {
            entity.HasKey(e => e.MedicineId).HasName("PK__Medicine__4F2128F059719266");

            entity.ToTable("Medicine");

            entity.Property(e => e.MedicineId).HasColumnName("MedicineID");
            entity.Property(e => e.MedicineName).HasMaxLength(100);
            entity.Property(e => e.ProviderId).HasColumnName("ProviderID");
            entity.Property(e => e.SideEffects).HasMaxLength(255);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Available");

            entity.HasOne(d => d.Provider).WithMany(p => p.Medicines)
                .HasForeignKey(d => d.ProviderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Medicine__Provid__797309D9");
        });

        modelBuilder.Entity<ObstetricRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("PK__Obstetri__FBDF78C979E71477");

            entity.ToTable("ObstetricRecord");

            entity.Property(e => e.RecordId)
                .ValueGeneratedNever()
                .HasColumnName("RecordID");
            entity.Property(e => e.ComplicationsNotes).HasMaxLength(255);
            entity.Property(e => e.FetalHeartRateBpm).HasColumnName("FetalHeartRateBPM");
            entity.Property(e => e.Lmpdate).HasColumnName("LMPDate");

            entity.HasOne(d => d.Record).WithOne(p => p.ObstetricRecord)
                .HasForeignKey<ObstetricRecord>(d => d.RecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Obstetric__Recor__693CA210");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.PatientId).HasName("PK__Patient__970EC346DF4AA4B0");

            entity.ToTable("Patient");

            entity.HasIndex(e => e.UserId, "UQ__Patient__1788CCAD3BFB04B0").IsUnique();

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
            entity.HasKey(e => e.PaymentId).HasName("PK__Payment__9B556A58EAF9027E");

            entity.ToTable("Payment");

            entity.Property(e => e.PaymentId).HasColumnName("PaymentID");
            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
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
                .HasConstraintName("FK__Payment__RecordI__06CD04F7");
        });

        modelBuilder.Entity<PediatricRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("PK__Pediatri__FBDF78C90F3B32D9");

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
                .HasConstraintName("FK__Pediatric__Recor__6C190EBB");
        });

        modelBuilder.Entity<PharmacyProvider>(entity =>
        {
            entity.HasKey(e => e.ProviderId).HasName("PK__Pharmacy__B54C689D2BB8E0A9");

            entity.ToTable("PharmacyProvider");

            entity.HasIndex(e => e.UserId, "UQ__Pharmacy__1788CCAD0C0F1602").IsUnique();

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
            entity.HasKey(e => e.PrescriptionId).HasName("PK__Prescrip__401308126C948E01");

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
                .HasConstraintName("FK__Prescript__Docto__7E37BEF6");

            entity.HasOne(d => d.Record).WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.RecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Prescript__Recor__7D439ABD");
        });

        modelBuilder.Entity<PrescriptionDetail>(entity =>
        {
            entity.HasKey(e => e.PrescriptionDetailId).HasName("PK__Prescrip__6DB7668AB4F9557B");

            entity.ToTable("PrescriptionDetail");

            entity.Property(e => e.PrescriptionDetailId).HasColumnName("PrescriptionDetailID");
            entity.Property(e => e.Dosage).HasMaxLength(100);
            entity.Property(e => e.Duration).HasMaxLength(50);
            entity.Property(e => e.MedicineId).HasColumnName("MedicineID");
            entity.Property(e => e.PrescriptionId).HasColumnName("PrescriptionID");

            entity.HasOne(d => d.Medicine).WithMany(p => p.PrescriptionDetails)
                .HasForeignKey(d => d.MedicineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Prescript__Medic__02084FDA");

            entity.HasOne(d => d.Prescription).WithMany(p => p.PrescriptionDetails)
                .HasForeignKey(d => d.PrescriptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Prescript__Presc__01142BA1");
        });

        modelBuilder.Entity<Receptionist>(entity =>
        {
            entity.HasKey(e => e.ReceptionistId).HasName("PK__Receptio__0F8C20480A1C1CAF");

            entity.ToTable("Receptionist");

            entity.HasIndex(e => e.UserId, "UQ__Receptio__1788CCADDBE6AAF4").IsUnique();

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
            entity.HasKey(e => e.RoleId).HasName("PK__Role__8AFACE3AA635375A");

            entity.ToTable("Role");

            entity.HasIndex(e => e.RoleName, "UQ__Role__8A2B616011E3ECBE").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.RoleName).HasMaxLength(30);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("PK__Room__32863919F2FE41BC");

            entity.ToTable("Room");

            entity.Property(e => e.RoomId).HasColumnName("RoomID");
            entity.Property(e => e.RoomName).HasMaxLength(50);
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.HasKey(e => e.ShiftId).HasName("PK__Shift__C0A838E147B827D7");

            entity.ToTable("Shift");

            entity.Property(e => e.ShiftId).HasColumnName("ShiftID");
            entity.Property(e => e.ShiftType).HasMaxLength(20);
        });

        modelBuilder.Entity<TestResult>(entity =>
        {
            entity.HasKey(e => e.TestResultId).HasName("PK__TestResu__E2463A67585C072A");

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
                .HasConstraintName("FK__TestResul__Recor__74AE54BC");

            entity.HasOne(d => d.TestType).WithMany(p => p.TestResults)
                .HasForeignKey(d => d.TestTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TestResul__TestT__75A278F5");
        });

        modelBuilder.Entity<TestType>(entity =>
        {
            entity.HasKey(e => e.TestTypeId).HasName("PK__TestType__9BB87646337C0E02");

            entity.ToTable("TestType");

            entity.Property(e => e.TestTypeId).HasColumnName("TestTypeID");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.TestName).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CCAC01E1E9BC");

            entity.ToTable("User");

            entity.HasIndex(e => e.Phone, "UQ__User__5C7E359E137223BB").IsUnique();

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
