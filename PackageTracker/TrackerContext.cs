﻿using System;
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

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //This allowd EF to ignore the "DeleteMe" and "StatusOfPackage" property regardless of
            //where it is found, and to never include it in its data model
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