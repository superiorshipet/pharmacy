using Microsoft.EntityFrameworkCore;
using DawaeeBackend.Models;

namespace DawaeeBackend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Medication> Medications { get; set; }
        public DbSet<Interaction> Interactions { get; set; }
        public DbSet<MedicationSchedule> MedicationSchedules { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Medication>()
                .HasIndex(m => m.NameEn)
                .IsUnique();

            // Fix the Interaction relationship
            modelBuilder.Entity<Interaction>()
                .HasOne(i => i.Medication)
                .WithMany(m => m.Interactions)
                .HasForeignKey(i => i.MedicationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Interaction>()
                .HasOne(i => i.ConflictingMedication)
                .WithMany()
                .HasForeignKey(i => i.ConflictingMedicationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
