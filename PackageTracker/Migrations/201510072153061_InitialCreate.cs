namespace PackageTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TrackerDatas",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        TrackingNumber = c.String(),
                        Location = c.String(),
                        Status = c.Int(nullable: false),
                        Service = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TrackerDatas");
        }
    }
}
