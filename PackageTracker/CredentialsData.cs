﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageTracker
{
    class CredentialData
    {
        #region Properties
        public int ID { get; set; }

        public FedExCredentialsData FedExCredentials { get; set; }
        public UPSCredentialsData UPSCredentials { get; set; }
        public USPSCredentialsData POSTALCredentials { get; set; }
        #endregion
    }

    class FedExCredentialsData
    {
        #region Properties
        public string UserKey { get; set; }
        public string UserPassword { get; set; }
        public string AccountNumber { get; set; }
        public string MeterNumber { get; set; }
        #endregion

        #region Constructors
        public FedExCredentialsData()
        {
            UserKey = "1CK3fnM8LhfQWteN";
            UserPassword = "vh6rTNPVog2PAXKRh44SiJznk";
            AccountNumber = "510087186";
            MeterNumber = "118691686";
        }
        #endregion
    }

    class USPSCredentialsData
    {
        #region Properties
        public string _userid { get; set; }
        #endregion

        #region Constructor
        public USPSCredentialsData()
        {
            _userid = "857STUDE5322";
        }
        #endregion

    }

    class UPSCredentialsData
    {
        #region Properties
        public string username { get; set; }
        public string password { get; set; }
        public string accessLicenseNumber { get; set; }
        #endregion

        #region Constructors
        public UPSCredentialsData()
        {
            username = "TastEPlasma";
            password = "Firebolt5";
            accessLicenseNumber = "4CFB51344FD8E476";
        }
        #endregion
    }
}