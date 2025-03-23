using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Job> Jobs { get; set; }
        public DbSet<Department> Department { get; set; }
        public DbSet<Employee> Employee { get; set; }
        public DbSet<JobApplication> JobApplications { get; set; }
        public DbSet<JobRecommend> JobRecommend { get; set; }
        public DbSet<ApplicantExperience> ApplicantExperiences { get; set; }
        public DbSet<ApplicantSkill> ApplicantSkills { get; set; }
        public DbSet<ApplicantProject> ApplicantProjects { get; set; }
        public DbSet<BulkCvsJobsHistory> BulkCvsJobsHistory { get; set; }
        public DbSet<HiringTemplate> hiringTemplates { get; set; }
        public DbSet<HiringStage> hiringStages { get; set; }
        public DbSet<HiringTemplateStage> HiringTemplateStages { get; set; }
        public DbSet<HiringStageOutcome> hiringStageOutcomes { get; set; }
        public DbSet<HiringParameter> hiringParameters { get; set; }
        public DbSet<HiringStageParameter> hiringStageParameters { get; set; }
        public DbSet<HiringTask> HiringTasks { get; set; }
        public DbSet<EmployeeTask> EmployeeTasks { get; set; }
        public DbSet<StageDepartmentNeed> stageDepartmentNeeds { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<HiringStageParameter>()
                .HasKey(hsp => new { hsp.StageId, hsp.ParameterId });

            modelBuilder.Entity<StageDepartmentNeed>()
                .HasKey(sdn => new { sdn.DepartmentId, sdn.StageId });

            modelBuilder.Entity<HiringStage>()
               .HasOne(h => h.CreatedBy)
               .WithMany()
               .HasForeignKey(h => h.AppUserId)
               .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<HiringTemplate>()
                .HasOne(t => t.CreatedBy)
                .WithMany()
                .HasForeignKey(t => t.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Batch>()
                .HasOne(b => b.HiringTemplate) // Assuming Batch has a navigation property to HiringTemplate
                .WithMany() // Assuming HiringTemplate does not have a navigation property back to Batch
                .HasForeignKey(b => b.TemplateId) // Foreign key property in Batch
                .OnDelete(DeleteBehavior.Restrict); // Or DeleteBehavior.Cascade, depending on your requirements

            modelBuilder.Entity<EmployeeTask>()
                .HasKey(et => new { et.TaskId, et.EmployeeId });
        }
    }
}
