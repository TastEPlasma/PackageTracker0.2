using System;
using UPSWebService.UPSWebReference;

namespace MAX.UPS
{
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
            TrackService track = new TrackService();
            TrackRequest tr = new TrackRequest();
            UPSSecurity upss = new UPSSecurity();
            UPSSecurityServiceAccessToken upssSvcAccessToken = new UPSSecurityServiceAccessToken();

            //Assign parameters
            upssSvcAccessToken.AccessLicenseNumber = AccessLicenseNumber;
            upss.ServiceAccessToken = upssSvcAccessToken;
            UPSSecurityUsernameToken upssUsrNameToken = new UPSSecurityUsernameToken();
            upssUsrNameToken.Username = Username;
            upssUsrNameToken.Password = Password;
            upss.UsernameToken = upssUsrNameToken;
            track.UPSSecurityValue = upss;
            RequestType request = new RequestType();
            String[] requestOption = { "15" };
            request.RequestOption = requestOption;
            tr.Request = request;
            tr.InquiryNumber = TrackingNumber;
            System.Net.ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();

            //Send request and receive results
            try
            {
                TrackResponse Results = track.ProcessTrack(tr);
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