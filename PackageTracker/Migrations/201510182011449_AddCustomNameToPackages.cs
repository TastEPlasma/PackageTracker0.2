namespace PackageTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCustomNameToPackages : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TrackerDatas", "CustomName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TrackerDatas", "CustomName");
        }
    }
}
