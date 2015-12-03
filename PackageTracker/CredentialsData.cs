
namespace PackageTracker
{
    class CredentialData
    {

        public int ID { get; set; }

        public FedExCredentialsData FedExCredentials { get; set; }
        public UPSCredentialsData UPSCredentials { get; set; }
        public USPSCredentialsData POSTALCredentials { get; set; }

    }

    class FedExCredentialsData
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

    class USPSCredentialsData
    {

        public string _userid { get; set; }

        public USPSCredentialsData()
        {
            _userid = "857STUDE5322";
        }

    }

    class UPSCredentialsData
    {

        public string username { get; set; }
        public string password { get; set; }
        public string accessLicenseNumber { get; set; }

        public UPSCredentialsData()
        {
            username = "TastEPlasma";
            password = "Firebolt5";
            accessLicenseNumber = "4CFB51344FD8E476";
        }

    }
}
