using System.Data.Entity;

namespace PackageTracker
{
    class TrackerContext : DbContext
    {
        #region Data Sets
        public DbSet<TrackerData> Packages { get; set; }
        public DbSet<CredentialData> Credentials { get; set; }
        #endregion

        #region Database Configuration
        //exclude unnecessary data bits from being entered into database
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //This allows EF to ignore the "DeleteMe", "StatusOfPackage" and "CarrierCode" 
            //property regardless of where it is found, and to never include it in its data model
            //or push it in updates

            //This syntax enables context-wide ability to ignore specific properties or values
            /*modelBuilder.Types().
                Configure(c => c.Ignore("DeleteMe"));*/  

            //This syntax enables ignoring values from a specific DBSet
            modelBuilder.Entity<TrackerData>().Ignore(p => p.DeleteMe);
            modelBuilder.Entity<TrackerData>().Ignore(p => p.StatusOfPackage);
            modelBuilder.Entity<TrackerData>().Ignore(p => p.CarrierCode);

            base.OnModelCreating(modelBuilder);
        }
        #endregion
    }
}
