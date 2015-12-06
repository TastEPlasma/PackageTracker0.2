namespace PackageTracker
{
    internal class CredentialData
    {
        public int ID { get; set; }

        public FedExCredentialsData FedExCredentials { get; set; }
        public UPSCredentialsData UPSCredentials { get; set; }
        public USPSCredentialsData POSTALCredentials { get; set; }
    }

    internal class FedExCredentialsData
    {
        public string UserKey { get; set; }
        public string UserPassword { get; set; }
        public string AccountNumber { get; set; }
        public string MeterNumber { get; set; }

        public FedExCredentialsData()
        {
            UserKey = "1CK3fnM8LhfQWteN";
            UserPassword = "vh6rTNPVog2PAXKRh44SiJznk";
            AccountNumber = "510087186";
            MeterNumber = "118691686";
        }
    }

    internal class USPSCredentialsData
    {
        public string UserID { get; set; }

        public USPSCredentialsData()
        {
            UserID = "857STUDE5322";
        }
    }

    internal class UPSCredentialsData
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string AccessLicenseNumber { get; set; }

        public UPSCredentialsData()
        {
            Username = "TastEPlasma";
            Password = "Firebolt5";
            AccessLicenseNumber = "4CFB51344FD8E476";
        }
    }
}