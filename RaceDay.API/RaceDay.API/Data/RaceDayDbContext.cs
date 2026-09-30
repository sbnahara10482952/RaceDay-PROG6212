using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<EventCategory> EventCategories { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // USER
            modelBuilder.Entity<User>()
                .ToTable("USER");

            modelBuilder.Entity<User>()
                .HasKey(u => u.UserID);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.DateCreated)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<User>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_USER_Role",
                    "[Role] IN ('Organiser', 'Participant')"));

            // EVENT
            modelBuilder.Entity<Event>()
                .ToTable("EVENT");

            modelBuilder.Entity<Event>()
                .HasKey(e => e.EventID);

            modelBuilder.Entity<Event>()
                .Property(e => e.EventName)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Event>()
                .Property(e => e.Location)
                .HasMaxLength(150)
                .IsRequired();

            modelBuilder.Entity<Event>()
                .Property(e => e.DistanceKm)
                .HasColumnType("decimal(6,2)");

            modelBuilder.Entity<Event>()
                .Property(e => e.EventType)
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder.Entity<Event>()
                .Property(e => e.Status)
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder.Entity<Event>()
                .Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Event>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_EVENT_Type",
                    "[EventType] IN ('Run', 'Walk', 'Cycle')"));

            modelBuilder.Entity<Event>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_EVENT_Status",
                    "[Status] IN ('Upcoming', 'Open', 'Closed', 'Completed', 'Cancelled')"));

            // EVENT -> USER
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organiser)
                .WithMany()
                .HasForeignKey(e => e.OrganiserID)
                .OnDelete(DeleteBehavior.Restrict);

            // CATEGORY
            modelBuilder.Entity<Category>()
                .ToTable("CATEGORY");

            modelBuilder.Entity<Category>()
                .HasKey(c => c.CategoryID);

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.CategoryName)
                .IsUnique();

            modelBuilder.Entity<Category>()
                .Property(c => c.CategoryName)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Category>()
                .Property(c => c.DistanceKm)
                .HasColumnType("decimal(6,2)");

            // EVENT_CATEGORY
            modelBuilder.Entity<EventCategory>()
                .ToTable("EVENT_CATEGORY");

            modelBuilder.Entity<EventCategory>()
                .HasKey(ec => ec.EventCategoryID);

            modelBuilder.Entity<EventCategory>()
                .Property(ec => ec.EntryFee)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<EventCategory>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_EVENT_CATEGORY_EntryFee",
                    "[EntryFee] >= 0"));

            modelBuilder.Entity<EventCategory>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_EVENT_CATEGORY_MaxParticipants",
                    "[MaxParticipants] > 0"));

            modelBuilder.Entity<EventCategory>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_EVENT_CATEGORY_AvailableSlots",
                    "[AvailableSlots] >= 0 AND [AvailableSlots] <= [MaxParticipants]"));

            modelBuilder.Entity<EventCategory>()
                .HasOne(ec => ec.Event)
                .WithMany(e => e.EventCategories)
                .HasForeignKey(ec => ec.EventID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventCategory>()
                .HasOne(ec => ec.Category)
                .WithMany(c => c.EventCategories)
                .HasForeignKey(ec => ec.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            // ENROLMENT
            modelBuilder.Entity<Enrolment>()
                .ToTable("ENROLMENT");

            modelBuilder.Entity<Enrolment>()
                .HasKey(e => e.EnrolmentID);

            modelBuilder.Entity<Enrolment>()
                .Property(e => e.EnrolmentDate)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Enrolment>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_ENROLMENT_Status",
                    "[EnrolmentStatus] IN ('Pending', 'Confirmed', 'Cancelled', 'Completed')"));

            modelBuilder.Entity<Enrolment>()
                .HasIndex(e => new
                {
                    e.EventCategoryID,
                    e.RaceNumber
                })
                .IsUnique();

            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Participant)
                .WithMany()
                .HasForeignKey(e => e.ParticipantID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.EventCategory)
                .WithMany()
                .HasForeignKey(e => e.EventCategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            // RESULT
            modelBuilder.Entity<Result>()
                .ToTable("RESULT");

            modelBuilder.Entity<Result>()
                .HasKey(r => r.ResultID);

            modelBuilder.Entity<Result>()
                .HasIndex(r => r.EnrolmentID)
                .IsUnique();

            modelBuilder.Entity<Result>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_RESULT_Position",
                    "[Position] IS NULL OR [Position] > 0"));

            modelBuilder.Entity<Result>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_RESULT_Status",
                    "[ResultStatus] IN ('Pending', 'Finished', 'DNS', 'DNF')"));

            modelBuilder.Entity<Result>()
                .HasOne(r => r.Enrolment)
                .WithOne(e => e.Result)
                .HasForeignKey<Result>(r => r.EnrolmentID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}