using System.Data.Entity;

namespace PackageTracker
{
    internal class TrackerContext : DbContext
    {
        public DbSet<TrackerData> Packages { get; set; }
        public DbSet<CredentialData> Credentials { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //This syntax enables context-wide ability to ignore specific properties or values
            //modelBuilder.Types().Configure(c => c.Ignore("DeleteMe"));  

            //This syntax enables ignoring values from a specific DBSet
            modelBuilder.Entity<TrackerData>().Ignore(p => p.DeleteMe);
            modelBuilder.Entity<TrackerData>().Ignore(p => p.StatusOfPackage);
            modelBuilder.Entity<TrackerData>().Ignore(p => p.CarrierCode);

            base.OnModelCreating(modelBuilder);
        }
    }
}