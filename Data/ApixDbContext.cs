using System;
using System.Collections.Generic;
using Consumer.Models;
using Microsoft.EntityFrameworkCore;

namespace Consumer.Data;

public partial class ApixDbContext : DbContext
{
    public ApixDbContext()
    {
    }

    public ApixDbContext(DbContextOptions<ApixDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Concilliation> Concilliations { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentProvider> PaymentProviders { get; set; }

    public virtual DbSet<PaymentProviderAccount> PaymentProviderAccounts { get; set; }

    public virtual DbSet<PixKey> PixKeys { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5433;Database=APIx_DB;UserName=postgres;Password=postgres;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Concilliation>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.Date).HasDefaultValueSql("'-infinity'::date");
            entity.Property(e => e.Postback).HasDefaultValueSql("''::text");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<PaymentProvider>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.PatchPaymentUrl).HasDefaultValueSql("''::text");
            entity.Property(e => e.PostPaymentUrl).HasDefaultValueSql("''::text");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<PaymentProviderAccount>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<PixKey>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
