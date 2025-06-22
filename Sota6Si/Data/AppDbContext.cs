using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Sota6Si.Models;

namespace Sota6Si.Data;

public partial class AppDbContext : DbContext
{
    private readonly string _connectionString;

    public AppDbContext()
    {
        var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Achievement> Achievements { get; set; }

    public virtual DbSet<DpCategory> DpCategories { get; set; }

    public virtual DbSet<DpImage> DpImages { get; set; }

    public virtual DbSet<DpOrder> DpOrders { get; set; }

    public virtual DbSet<DpOrderComposition> DpOrderCompositions { get; set; }

    public virtual DbSet<DpProduct> DpProducts { get; set; }

    public virtual DbSet<DpProductAttribute> DpProductAttributes { get; set; }

    public virtual DbSet<DpSize> DpSizes { get; set; }

    public virtual DbSet<DpUser> DpUsers { get; set; }

    public virtual DbSet<DpUserProj> DpUserProjs { get; set; }

    public virtual DbSet<UserHasAchievement> UserHasAchievements { get; set; }



    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Achievement>(entity =>
        {
            entity.HasKey(e => e.AchievementId).HasName("PK_Achievement");

            entity.ToTable("achievement");

            entity.Property(e => e.AchievementId).HasColumnName("achievement_id");
            entity.Property(e => e.TextAchievement).HasColumnName("text_achievement");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .HasColumnName("title");

            // Установка многих-ко-многим отношений
            entity.HasMany(a => a.UserHasAchievements)
                .WithOne(uh => uh.Achievement)
                .HasForeignKey(uh => uh.AchievementId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_dp_user_proj_has_achievement_achievement1");
        });

        modelBuilder.Entity<UserHasAchievement>(entity =>
        {
            entity.HasKey(e => new { e.DpUserProjId, e.AchievementId });

            entity.ToTable("user_has_achievement");

            entity.Property(e => e.DpUserProjId).HasColumnName("dp_user_proj_id");
            entity.Property(e => e.AchievementId).HasColumnName("achievement_id");
            entity.Property(e => e.IsObtained).HasColumnName("is_obtained");

            entity.HasOne(uh => uh.Achievement)
                .WithMany(a => a.UserHasAchievements)
                .HasForeignKey(uh => uh.AchievementId)
                .HasConstraintName("fk_user_has_achievement_achievement");

            entity.HasOne(uh => uh.DpUserProj)
                .WithMany(d => d.UserHasAchievements)
                .HasForeignKey(uh => uh.DpUserProjId)
                .HasConstraintName("fk_user_has_achievement_dp_user_proj");
        });

        modelBuilder.Entity<DpCategory>(entity =>
        {
            entity.HasKey(e => e.DpCategoryId).HasName("PK_DpCategory");

            entity.ToTable("dp_category");

            entity.Property(e => e.DpCategoryId).HasColumnName("dp_category_id");
            entity.Property(e => e.DpCategoryTitle)
                .HasMaxLength(45)
                .HasColumnName("dp_category_title");

            
            entity.Property(e => e.SizeId).HasColumnName("size_id");

            
            entity.HasOne(d => d.Size)
                  .WithMany(p => p.DpCategories)
                  .HasForeignKey(d => d.SizeId)
                  .HasConstraintName("FK_DpCategory_DpSize");
        });


        modelBuilder.Entity<DpImage>(entity =>
        {
            entity.HasKey(e => e.DpImagesId).HasName("PK_DpImage");

            entity.ToTable("dp_images");

            entity.HasIndex(e => e.DpProductId, "dp_fk_images_idx");

            entity.Property(e => e.DpImagesId).HasColumnName("dp_images_id");
            entity.Property(e => e.DpImageTitle)
                .HasMaxLength(45)
                .HasColumnName("dp_image_title");
            entity.Property(e => e.DpProductId).HasColumnName("dp_product_id");
            entity.Property(e => e.ImagesData).HasColumnName("images_data");

            entity.HasOne(d => d.DpProduct).WithMany(p => p.DpImages)
                .HasForeignKey(d => d.DpProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("dp_fk_images");
        });

        modelBuilder.Entity<DpOrder>(entity =>
        {
            entity.HasKey(e => e.DpOrderId).HasName("PK_DpOrder");

            entity.ToTable("dp_order");

            entity.HasIndex(e => e.DpUserId, "fk_users_fk_idx");

            entity.Property(e => e.DpOrderId).HasColumnName("dp_order_id");
            entity.Property(e => e.DpDateTimeOrder)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("dp_date_time_order");
            entity.Property(e => e.DpTypeOrder).HasColumnName("dp_type_order");
            entity.Property(e => e.DpUserId).HasColumnName("dp_user_id");

            entity.HasOne(d => d.DpUser).WithMany(p => p.DpOrders)
                .HasForeignKey(d => d.DpUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("dp_fk_users");
        });

        modelBuilder.Entity<DpOrderComposition>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("dp_order_composition");

            entity.HasIndex(e => e.DpAttributesId, "dp_order_composition_ibfk_1_idx");

            entity.HasIndex(e => e.DpOrderId, "dp_order_composition_ibfk_2");

            entity.HasIndex(e => new { e.DpOrderId, e.DpAttributesId }, "unique").IsUnique();

            entity.Property(e => e.DpAttributesId).HasColumnName("dp_attributes_id");
            entity.Property(e => e.DpCost)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("dp_cost");
            entity.Property(e => e.DpOrderId).HasColumnName("dp_order_id");
            entity.Property(e => e.DpQuantity)
                .HasDefaultValueSql("('1')")
                .HasColumnName("dp_quantity");

            entity.HasOne(d => d.DpAttributes).WithMany()
                .HasForeignKey(d => d.DpAttributesId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_dp_order_composition_1");

            entity.HasOne(d => d.DpOrder).WithMany()
                .HasForeignKey(d => d.DpOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("dp_order_composition_ibfk_2");
        });

        modelBuilder.Entity<DpProduct>(entity =>
        {
            entity.HasKey(e => e.DpProductId).HasName("PK_DpProduct");

            entity.ToTable("dp_product");

            entity.HasIndex(e => e.DpCategoryId, "dp_fk_category_idx");

            entity.Property(e => e.DpProductId).HasColumnName("dp_product_id");
            entity.Property(e => e.DpCategoryId).HasColumnName("dp_category_id");
            entity.Property(e => e.DpDescription)
                .HasMaxLength(500)
                .HasColumnName("dp_description");
            entity.Property(e => e.DpDiscountPercent)
                .HasDefaultValueSql("('0')")
                .HasColumnName("dp_discount_percent");
            entity.Property(e => e.DpPrice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("dp_price");
            entity.Property(e => e.DpPurchasePrice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("dp_purchase_price");
            entity.Property(e => e.DpTitle)
                .HasMaxLength(100)
                .HasColumnName("dp_title");

            entity.HasOne(d => d.DpCategory).WithMany(p => p.DpProducts)
                .HasForeignKey(d => d.DpCategoryId)
                .HasConstraintName("dp_fk_category");
        });

        modelBuilder.Entity<DpProductAttribute>(entity =>
        {
            entity.HasKey(e => e.DpAttributesId).HasName("PK_DpProductAttribute");

            entity.ToTable("dp_product_attributes");

            entity.HasIndex(e => e.DpProductId, "dp_product_attributes_ibfk_1");

            entity.HasIndex(e => e.DpSize, "dp_size_attributes_1_idx");

            entity.Property(e => e.DpAttributesId).HasColumnName("dp_attributes_id");
            entity.Property(e => e.DpCount)
                .HasDefaultValueSql("('1')")
                .HasColumnName("dp_count");
            entity.Property(e => e.DpProductId).HasColumnName("dp_product_id");
            entity.Property(e => e.DpSize).HasColumnName("dp_size");

            entity.HasOne(d => d.DpProduct)
                .WithMany(p => p.DpProductAttributes)
                .HasForeignKey(d => d.DpProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("dp_product_attributes_ibfk_1");

            entity.HasOne(d => d.DpSizeNavigation)
                .WithMany(p => p.DpProductAttributes)
                .HasForeignKey(d => d.DpSize)
                .HasConstraintName("dp_size_attributes_1");
        });


        modelBuilder.Entity<DpSize>(entity =>
        {
            entity.HasKey(e => e.SizeId).HasName("PK_DpSize");

            entity.ToTable("dp_size");

            entity.Property(e => e.SizeId).HasColumnName("size_id");
            entity.Property(e => e.Size)
                .HasMaxLength(10)
                .HasColumnName("size");
        });

        modelBuilder.Entity<DpUser>(entity =>
        {
            entity.HasKey(e => e.DpUserId).HasName("PK_DpUser");

            entity.ToTable("dp_user");

            entity.HasIndex(e => e.DpUsername, "dp_username_unique").IsUnique();

            entity.Property(e => e.DpUserId).HasColumnName("dp_user_id");
            entity.Property(e => e.DpEmail)
                .HasMaxLength(100)
                .HasColumnName("dp_email");
            entity.Property(e => e.DpFullName)
                .HasMaxLength(100)
                .HasColumnName("dp_full_name");
            entity.Property(e => e.DpPassword)
                .HasMaxLength(255)
                .HasColumnName("dp_password");
            entity.Property(e => e.DpPhoneNumber)
                .HasMaxLength(11)
                .HasColumnName("dp_phone_number");
            entity.Property(e => e.DpRegistrationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("dp_registration_date");
            entity.Property(e => e.DpUsername)
                .HasMaxLength(50)
                .HasColumnName("dp_username");
        });

        modelBuilder.Entity<DpUserProj>(entity =>
        {
            entity.HasKey(e => e.DpUserProjId).HasName("PK_DpUserProj");
            entity.ToTable("dp_user_proj");

            entity.Property(e => e.DpUserProjId).HasColumnName("dp_user_proj_id");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Login)
                .HasMaxLength(100)
                .HasColumnName("login");
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .HasColumnName("password");

            // Установка многих-ко-многим отношений
            entity.HasMany(d => d.UserHasAchievements)
                .WithOne(uh => uh.DpUserProj)
                .HasForeignKey(uh => uh.DpUserProjId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_dp_user_proj_has_achievement_dp_user_proj1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
