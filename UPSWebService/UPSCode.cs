namespace MAX.UPS
{
    using System;
    using UPSWebService.UPSWebReference;

    public class UPSManager
    {
        public UPSManager()
        {
            this.Username = "TastEPlasma";
            this.Password = "Firebolt5";
            this.AccessLicenseNumber = "4CFB51344FD8E476";
        }

        public string Username { get; set; }
        public string Password { get; set; }
        public string AccessLicenseNumber { get; set; }

        public TrackResponse GetTrackingInfo(string TrackingNumber)
        {
            //The following code is from the WebAPI example
            //Construct objects
            var TrackService = new TrackService();
            var TrackRequest = new TrackRequest();
            var UPSSecurityObject = new UPSSecurity();
            var upssSvcAccessToken = new UPSSecurityServiceAccessToken();

            //Assign parameters
            upssSvcAccessToken.AccessLicenseNumber = AccessLicenseNumber;
            UPSSecurityObject.ServiceAccessToken = upssSvcAccessToken;
            var upssUsrNameToken = new UPSSecurityUsernameToken();
            upssUsrNameToken.Username = Username;
            upssUsrNameToken.Password = Password;
            UPSSecurityObject.UsernameToken = upssUsrNameToken;
            TrackService.UPSSecurityValue = UPSSecurityObject;
            RequestType request = new RequestType();
            String[] requestOption = { "15" };
            request.RequestOption = requestOption;
            TrackRequest.Request = request;
            TrackRequest.InquiryNumber = TrackingNumber;
            System.Net.ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();

            //Send request and receive results
            try
            {
                TrackResponse Results = TrackService.ProcessTrack(TrackRequest);
                return Results;
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void ResetCredentialsToDefaults()
        {
            Username = "TastEPlasma";
            Password = "Firebolt5";
            AccessLicenseNumber = "4CFB51344FD8E476";
        }
    }
}