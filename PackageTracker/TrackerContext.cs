using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace PackageTracker
{
    class TrackerContext : DbContext
    {
        public DbSet<TrackerData> Packages { get; set; }

        //exclude unnecessary data bits from being entered into database
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //This allows EF to ignore the "DeleteMe", "StatusOfPackage" and "CarrierCode" 
            //property regardless of where it is found, and to never include it in its data model
            //or push it in updates
            modelBuilder.Types().
                Configure(c => c.Ignore("DeleteMe"));
            modelBuilder.Types().
                Configure(c => c.Ignore("StatusOfPackage"));
            modelBuilder.Types().
                Configure(c => c.Ignore("CarrierCode"));
            base.OnModelCreating(modelBuilder);
        }
    }
}
