using System;
using System.Collections.Generic;
using System.Text;
using UPSWebService.UPSWebReference;
using System.ServiceModel;

namespace MAX.UPS
{
    public class UPSManager
    {
        #region Private Members
        private string username = "TastEPlasma";
        private string password = "Firebolt5";
        private string accessLicenseNumber = "4CFB51344FD8E476";
        #endregion

        #region Properties
        public string SetUsername 
        {
            get { return "no access"; }
            set { username = value; }
        }
        public string SetPassword 
        {
            get { return "no access"; }
            set { password = value; }
        }
        public string SetLicenseNumber 
        {
            get { return "no access"; }
            set { accessLicenseNumber = value; }
        }
        #endregion

        #region Constructors
        //Using Default
        #endregion

        #region Public Interface
        public TrackResponse GetTrackingInfo(string TrackingNumber)
        {
            //Construct objects
            TrackService track = new TrackService();
            TrackRequest tr = new TrackRequest();
            UPSSecurity upss = new UPSSecurity();
            UPSSecurityServiceAccessToken upssSvcAccessToken = new UPSSecurityServiceAccessToken();

            //Assign parameters
            upssSvcAccessToken.AccessLicenseNumber = accessLicenseNumber;
            upss.ServiceAccessToken = upssSvcAccessToken;
            UPSSecurityUsernameToken upssUsrNameToken = new UPSSecurityUsernameToken();
            upssUsrNameToken.Username = username;
            upssUsrNameToken.Password = password;
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
            catch(System.Web.Services.Protocols.SoapException ex)
            {
                throw new Exception(ex.Message);
            }    
        }

        public void ResetCredentialsToDefaultValues()
        {
            username = "TastEPlasma";
            password = "Firebolt5";
            accessLicenseNumber = "4CFB51344FD8E476";
        }
        #endregion

        #region UPS Example Code
        public static void TestCode()
        {
            try
            {
                TrackService track = new TrackService();
                TrackRequest tr = new TrackRequest();
                UPSSecurity upss = new UPSSecurity();
                UPSSecurityServiceAccessToken upssSvcAccessToken = new UPSSecurityServiceAccessToken();
                upssSvcAccessToken.AccessLicenseNumber = "";
                upss.ServiceAccessToken = upssSvcAccessToken;
                UPSSecurityUsernameToken upssUsrNameToken = new UPSSecurityUsernameToken();
                upssUsrNameToken.Username = "";
                upssUsrNameToken.Password = "";
                upss.UsernameToken = upssUsrNameToken;
                track.UPSSecurityValue = upss;
                RequestType request = new RequestType();
                String[] requestOption = { "15" };
                request.RequestOption = requestOption;
                tr.Request = request;
                tr.InquiryNumber = "";
                System.Net.ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();
                TrackResponse trackResponse = track.ProcessTrack(tr);
                Console.WriteLine("The transaction was a " + trackResponse.Response.ResponseStatus.Description);
                Console.WriteLine("Shipment Service " + trackResponse.Shipment[0].Service.Description);
                Console.WriteLine("Location is " + trackResponse.Shipment[0].Package[0].Activity[0].ActivityLocation.Address.City);
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {
                Console.WriteLine("");
                Console.WriteLine("---------Track Web Service returns error----------------");
                Console.WriteLine("---------\"Hard\" is user error \"Transient\" is system error----------------");
                Console.WriteLine("SoapException Message= " + ex.Message);
                Console.WriteLine("");
                Console.WriteLine("SoapException Category:Code:Message= " + ex.Detail.LastChild.InnerText);
                Console.WriteLine("");
                Console.WriteLine("SoapException XML String for all= " + ex.Detail.LastChild.OuterXml);
                Console.WriteLine("");
                Console.WriteLine("SoapException StackTrace= " + ex.StackTrace);
                Console.WriteLine("-------------------------");
                Console.WriteLine("");
            }
            catch (System.ServiceModel.CommunicationException ex)
            {
                Console.WriteLine("");
                Console.WriteLine("--------------------");
                Console.WriteLine("CommunicationException= " + ex.Message);
                Console.WriteLine("CommunicationException-StackTrace= " + ex.StackTrace);
                Console.WriteLine("-------------------------");
                Console.WriteLine("");

            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("-------------------------");
                Console.WriteLine(" General Exception= " + ex.Message);
                Console.WriteLine(" General Exception-StackTrace= " + ex.StackTrace);
                Console.WriteLine("-------------------------");

            }
        }
        #endregion
    }
}
