namespace PackageTracker.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class NameingConventions : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.CredentialDatas", "POSTALCredentials__userid", "POSTALCredentials_UserID");
            //AUTO GENERATED CODE
            //AddColumn("dbo.CredentialDatas", "POSTALCredentials_UserID", c => c.String());
            //DropColumn("dbo.CredentialDatas", "POSTALCredentials__userid");
        }

        public override void Down()
        {
            RenameColumn("dbo.CredentialDatas", "POSTALCredentials_UserID", "POSTALCredentials__userid");
            //AUTO GENERATED CODE
            //AddColumn("dbo.CredentialDatas", "POSTALCredentials__userid", c => c.String());
            //DropColumn("dbo.CredentialDatas", "POSTALCredentials_UserID");
        }
    }
}