using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data
{
    public class RaceDayContext : DbContext
    {
        public RaceDayContext(DbContextOptions<RaceDayContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<EventEnrolment> EventEnrolments => Set<EventEnrolment>();
        public DbSet<Result> Results => Set<Result>();
        public DbSet<RouteInfo> RouteInfos => Set<RouteInfo>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---- User ----
            modelBuilder.Entity<User>(e =>
            {
                e.HasIndex(u => u.Email).IsUnique();
                e.ToTable("User", t => t.HasCheckConstraint(
                    "CK_User_Role", "[Role] IN ('Organiser', 'Participant')"));
            });

            // ---- Event ----
            modelBuilder.Entity<Event>(e =>
            {
                e.HasOne(ev => ev.Organiser)
                 .WithMany(u => u.OrganisedEvents)
                 .HasForeignKey(ev => ev.OrganiserId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.ToTable("Event", t => t.HasCheckConstraint(
                    "CK_Event_Type", "[EventType] IN ('Run', 'Walk', 'Cycle')"));
            });

            // ---- Category ----
            modelBuilder.Entity<Category>(e =>
            {
                e.HasOne(c => c.Event)
                 .WithMany(ev => ev.Categories)
                 .HasForeignKey(c => c.EventId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.ToTable("Category");
            });

            // ---- EventEnrolment ----
            modelBuilder.Entity<EventEnrolment>(e =>
            {
                e.HasKey(en => en.EnrolmentId);
                e.HasOne(en => en.Participant)
                 .WithMany(u => u.Enrolments)
                 .HasForeignKey(en => en.ParticipantId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(en => en.Category)
                 .WithMany(c => c.Enrolments)
                 .HasForeignKey(en => en.CategoryId)
                 .OnDelete(DeleteBehavior.Restrict);

                // A participant can only enrol once per category
                e.HasIndex(en => new { en.ParticipantId, en.CategoryId }).IsUnique();

                e.ToTable("EventEnrolment", t => t.HasCheckConstraint(
                    "CK_Enrolment_Status", "[Status] IN ('Confirmed', 'Cancelled')"));
            });

            // ---- Result (one-to-zero-or-one from EventEnrolment) ----
            modelBuilder.Entity<Result>(e =>
            {
                e.HasOne(r => r.Enrolment)
                 .WithOne(en => en.Result)
                 .HasForeignKey<Result>(r => r.EnrolmentId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(r => r.EnrolmentId).IsUnique();

                e.ToTable("Result", t => t.HasCheckConstraint(
                    "CK_Result_Status", "[Status] IN ('Finished', 'DNF', 'DNS')"));
            });

            // ---- RouteInfo ----
            modelBuilder.Entity<RouteInfo>(e =>
            {
                e.HasKey(r => r.RouteId);
                e.HasOne(r => r.Category)
                 .WithMany()
                 .HasForeignKey(r => r.CategoryId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.ToTable("RouteInfo");
            });
        }
    }
}