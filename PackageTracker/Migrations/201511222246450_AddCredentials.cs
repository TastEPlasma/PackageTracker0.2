namespace PackageTracker.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddCredentials : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CredentialDatas",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        FedExCredentials_UserKey = c.String(),
                        FedExCredentials_UserPassword = c.String(),
                        FedExCredentials_AccountNumber = c.String(),
                        FedExCredentials_MeterNumber = c.String(),
                        UPSCredentials_username = c.String(),
                        UPSCredentials_password = c.String(),
                        UPSCredentials_accessLicenseNumber = c.String(),
                        POSTALCredentials__userid = c.String(),
                    })
                .PrimaryKey(t => t.ID);
        }

        public override void Down()
        {
            DropTable("dbo.CredentialDatas");
        }
    }
}