//////////////////////////////////////////////////////////////////////////
///This software is provided to you as-is and with not warranties!!!
///Use this software at your own risk.
///This software is Copyright by Scott Smith 2006
///You are free to use this software as you see fit.
//////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace MAX.USPS
{
    public class USPSManager
    {
        #region Private Members
        private const string ProductionUrl = "http://production.shippingapis.com/ShippingAPI.dll";
        private const string TestingUrl = "http://testing.shippingapis.com/ShippingAPITest.dll";
        private WebClient web;
        private string _userid;
        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new USPS Manager instance
        /// </summary>
        /// <param name="USPSWebtoolUserID">The UserID required by the USPS Web Tools</param>
        public USPSManager(string USPSWebtoolUserID)
        {
            web = new WebClient();
            _userid = USPSWebtoolUserID;
            _TestMode = false;
            
        }
        /// <summary>
        /// Creates a new USPS Manager instance
        /// </summary>
        /// <param name="USPSWebtoolUserID">The UserID required by the USPS Web Tools</param>
        /// <param name="testmode">If True, then the USPS Test URL will be used.</param>
        public USPSManager(string USPSWebtoolUserID, bool testmode)
        {
            _TestMode = testmode;
            web = new WebClient();
            _userid = USPSWebtoolUserID;
        }

        #endregion

        #region Properties
        private bool _TestMode;
        /// <summary>
        /// Determines if the Calls to the USPS server is made to the Test or Production server.
        /// </summary>
        public bool TestMode
        {
            get { return _TestMode; }
            set { _TestMode = value; }
        }

        #endregion

        #region Address Methods
        /// <summary>
        /// Validate a single address
        /// </summary>
        /// <param name="address">Address object to be validated</param>
        /// <returns>Validated Address</returns>
        public Address ValidateAddress(Address address)
        {
            try
            {
                string validateUrl = "?API=Verify&XML=<AddressValidateRequest USERID=\"{0}\"><Address ID=\"{1}\"><Address1>{2}</Address1><Address2>{3}</Address2><City>{4}</City><State>{5}</State><Zip5>{6}</Zip5><Zip4>{7}</Zip4></Address></AddressValidateRequest>";
                string url = GetURL() + validateUrl;
                url = String.Format(url, _userid, address.ID.ToString(), address.Address1, address.Address2, address.City, address.State, address.Zip, address.ZipPlus4);
                string addressxml = web.DownloadString(url);
                if (addressxml.Contains("<Error>"))
                {
                    int idx1 = addressxml.IndexOf("<Description>") + 13;
                    int idx2 = addressxml.IndexOf("</Description>");
                    int l = addressxml.Length;
                    string errDesc = addressxml.Substring(idx1, idx2 - idx1);
                    throw new USPSManagerException(errDesc);
                }
                
                return Address.FromXml(addressxml);
            }
            catch(WebException ex)
            {
                throw new USPSManagerException(ex);
            }
        }
        /// <summary>
        /// Get the zip code by providing an Address object with a city and state
        /// </summary>
        /// <param name="city">City</param>
        /// <param name="state">State</param>
        public Address GetZipcode(string city, string state)
        {
            Address a = new Address();
            a.City = city;
            a.State = state;
            return GetZipcode(a);
        }

        /// <summary>
        /// Get the zip code by providing an Address object with a city and state
        /// </summary>
        /// <param name="address">Address Object</param>
        /// <returns>Address Object</returns>
        public Address GetZipcode(Address address)
        {
            try
            {
                //The address must contain a city and state
                if (address.City == null || address.City.Length < 1 || address.State == null || address.State.Length < 1)
                    throw new USPSManagerException("You must supply a city and state for a zipcode lookup request.");

                string zipcodeurl = "?API=ZipCodeLookup&XML=<ZipCodeLookupRequest USERID=\"{0}\"><Address ID=\"{1}\"><Address1>{2}</Address1><Address2>{3}</Address2><City>{4}</City><State>{5}</State></Address></ZipCodeLookupRequest>";
                string url = GetURL() + zipcodeurl;
                url = String.Format(url, _userid, address.ID.ToString(), address.Address1, address.Address2, address.City, address.State, address.Zip, address.ZipPlus4);
                string addressxml = web.DownloadString(url);
                if (addressxml.Contains("<Error>"))
                {
                    int idx1 = addressxml.IndexOf("<Description>") + 13;
                    int idx2 = addressxml.IndexOf("</Description>");
                    int l = addressxml.Length;
                    string errDesc = addressxml.Substring(idx1, idx2 - idx1);
                    throw new USPSManagerException(errDesc);
                }

                return Address.FromXml(addressxml);
            }
            catch (WebException ex)
            {
                throw new USPSManagerException(ex);
            }
        }

        /// <summary>
        /// Get the city and state by proving the zip code.
        /// </summary>
        /// <param name="zipcode">Zipcode</param>
        public Address GetCityState(string zipcode)
        {
            Address a = new Address();
            a.Zip = zipcode;
            return GetCityState(a);
        }

        /// <summary>
        /// Get the city and state by proving the zip code.
        /// </summary>
        /// <param name="address">Address object</param>
        /// <returns>Address Object</returns>
        public Address GetCityState(Address address)
        {
            try
            {
                //The address must contain a city and state
                if (address.Zip == null || address.Zip.Length < 1)
                    throw new USPSManagerException("You must supply a zipcode for a city/state lookup request.");
                
                string citystateurl = "?API=CityStateLookup&XML=<CityStateLookupRequest USERID=\"{0}\"><ZipCode ID= \"{1}\"><Zip5>{2}</Zip5></ZipCode></CityStateLookupRequest>";
                string url = GetURL() + citystateurl;
                url = String.Format(url, _userid, address.ID.ToString(), address.Zip);
                string addressxml = web.DownloadString(url);
                if (addressxml.Contains("<Error>"))
                {
                    int idx1 = addressxml.IndexOf("<Description>") + 13;
                    int idx2 = addressxml.IndexOf("</Description>");
                    int l = addressxml.Length;
                    string errDesc = addressxml.Substring(idx1, idx2 - idx1);
                    throw new USPSManagerException(errDesc);
                }

                return Address.FromXml(addressxml);
            }
            catch (WebException ex)
            {
                throw new USPSManagerException(ex);
            }
        }

        #endregion

        #region Tracking Methods
        public TrackingInfo GetTrackingInfo(string TrackingNumber)
        {
            try
            {
                string trackurl = "?API=TrackV2&XML=<TrackRequest USERID=\"{0}\"><TrackID ID=\"{1}\"></TrackID></TrackRequest>";
                string url = GetURL() + trackurl;
                url = String.Format(url, _userid, TrackingNumber);
                string xml = web.DownloadString(url);
                if (xml.Contains("<Error>"))
                {
                    int idx1 = xml.IndexOf("<Description>") + 13;
                    int idx2 = xml.IndexOf("</Description>");
                    int l = xml.Length;
                    string errDesc = xml.Substring(idx1, idx2 - idx1);
                    throw new USPSManagerException(errDesc);
                }

                return TrackingInfo.FromXml(xml);
            }
            catch (WebException ex)
            {
                throw new USPSManagerException(ex);
            }
        }
        #endregion

        #region Label Methods
        /// <summary>
        /// Fills a package's ShippingLabel with a Byte{} containing the Image for the label
        /// </summary>
        /// <param name="package">Package with From and To addresses provided</param>
        /// <returns>The same package with the ShippingLabel</returns>
        public Package GetDeliveryConfirmationLabel(Package package)
        {
            string labeldate = package.ShipDate.ToShortDateString();
            if (package.ShipDate.ToShortDateString() == DateTime.Now.ToShortDateString())
                labeldate = "";
            string url = "?API=DeliveryConfirmationV3&XML=<DeliveryConfirmationV3.0Request USERID=\"{0}\"><Option>{1}</Option><ImageParameters></ImageParameters><FromName>{2}</FromName><FromFirm>{3}</FromFirm><FromAddress1>{4}</FromAddress1><FromAddress2>{5}</FromAddress2><FromCity>{6}</FromCity><FromState>{7}</FromState><FromZip5>{8}</FromZip5><FromZip4>{9}</FromZip4><ToName>{10}</ToName><ToFirm>{11}</ToFirm><ToAddress1>{12}</ToAddress1><ToAddress2>{13}</ToAddress2><ToCity>{14}</ToCity><ToState>{15}</ToState><ToZip5>{16}</ToZip5><ToZip4>{17}</ToZip4><WeightInOunces>{18}</WeightInOunces><ServiceType>{19}</ServiceType><POZipCode>{20}</POZipCode><ImageType>{21}</ImageType><LabelDate>{22}</LabelDate><CustomerRefNo>{23}</CustomerRefNo><AddressServiceRequested>{24}</AddressServiceRequested><SenderName>{25}</SenderName><SenderEMail>{26}</SenderEMail><RecipientName>{27}</RecipientName><RecipientEMail>{28}</RecipientEMail></DeliveryConfirmationV3.0Request>";
            url = GetURL() + url;
            //url = String.Format(url,this._userid, (int)package.LabelType, package.FromAddress.Contact, package.FromAddress.FirmName, package.FromAddress.Address1, package.FromAddress.Address2, package.FromAddress.City, package.FromAddress.State, package.FromAddress.Zip, package.FromAddress.ZipPlus4, package.ToAddress.Contact, package.ToAddress.FirmName, package.ToAddress.Address1, package.ToAddress.Address2, package.ToAddress.City, package.ToAddress.State, package.ToAddress.Zip, package.ToAddress.ZipPlus4, package.WeightInOunces.ToString(), package.ServiceType.ToString().Replace("_", " "), package.OriginZipcode, package.LabelImageType.ToString(), labeldate, package.ReferenceNumber, package.AddressServiceRequested.ToString(),  package.FromAddress.Contact, package.FromAddress.ContactEmail, package.ToAddress.Contact, package.ToAddress.ContactEmail);
            url = String.Format(url, this._userid, (int)package.LabelType, package.FromAddress.Contact, package.FromAddress.FirmName, package.FromAddress.Address1, package.FromAddress.Address2, package.FromAddress.City, package.FromAddress.State, package.FromAddress.Zip, package.FromAddress.ZipPlus4, package.ToAddress.Contact, package.ToAddress.FirmName, package.ToAddress.Address1, package.ToAddress.Address2, package.ToAddress.City, package.ToAddress.State, package.ToAddress.Zip, package.ToAddress.ZipPlus4, package.WeightInOunces.ToString(), package.ServiceType.ToString().Replace("_", " "), package.OriginZipcode, package.LabelImageType.ToString(), labeldate, package.ReferenceNumber, package.AddressServiceRequested.ToString(), "", "", "", "");
            string xml = web.DownloadString(url);
            if (xml.Contains("<Error>"))
            {
                int idx1 = xml.IndexOf("<Description>") + 13;
                int idx2 = xml.IndexOf("</Description>");
                int l = xml.Length;
                string errDesc = xml.Substring(idx1, idx2 - idx1);
                throw new USPSManagerException(errDesc);
            }
            int i1 = xml.IndexOf("<DeliveryConfirmationLabel>") + 27;
            int i2 = xml.IndexOf("</DeliveryConfirmationLabel>");
            package.ShippingLabel = StringToUTF8ByteArray(xml.Substring(i1, i2 - i1));
            return package;
        }

        /// <summary>
        /// Fills a package's ShippingLabel with a Byte{} containing the Image for the label
        /// </summary>
        /// <param name="package">Package with From and To addresses provided</param>
        /// <returns>The same package with the ShippingLabel</returns>
        public Package GetSignatureConfirmationLabel(Package package)
        {
            string url = "?API=SignatureConfirmationV3&XML=<SignatureConfirmationV3.0Request USERID=\"{0}\"><Option>{1}</Option><ImageParameters></ImageParameters><FromName>{2}</FromName><FromFirm>{3}</FromFirm><FromAddress1>{4}</FromAddress1><FromAddress2>{5}</FromAddress2><FromCity>{6}</FromCity><FromState>{7}</FromState><FromZip5>{8}</FromZip5><FromZip4>{9}</FromZip4><ToName>{10}</ToName><ToFirm>{11}</ToFirm><ToAddress1>{12}</ToAddress1><ToAddress2>{13}</ToAddress2><ToCity>{14}</ToCity><ToState>{15}</ToState><ToZip5>{16}</ToZip5><ToZip4>{17}</ToZip4><WeightInOunces>{18}</WeightInOunces><ServiceType>{19}</ServiceType><POZipCode>{20}</POZipCode><ImageType>{21}</ImageType><LabelDate>{22}</LabelDate><CustomerRefNo>{23}</CustomerRefNo><AddressServiceRequested>{24}</AddressServiceRequested></SignatureConfirmationV3.0Request>";
            url = GetURL() + url;
            url = String.Format(url, this._userid, (int)package.LabelType, package.FromAddress.Contact, package.FromAddress.FirmName, package.FromAddress.Address1, package.FromAddress.Address2, package.FromAddress.City, package.FromAddress.State, package.FromAddress.Zip, package.FromAddress.ZipPlus4, package.ToAddress.Contact, package.ToAddress.FirmName, package.ToAddress.Address1, package.ToAddress.Address2, package.ToAddress.City, package.ToAddress.State, package.ToAddress.Zip, package.ToAddress.ZipPlus4, package.WeightInOunces.ToString(), package.ServiceType.ToString().Replace("_", " "), package.OriginZipcode, package.LabelImageType.ToString(), package.ShipDate.ToShortDateString(), package.ReferenceNumber, package.AddressServiceRequested.ToString(), package.FromAddress.Contact, package.FromAddress.ContactEmail, package.ToAddress.Contact, package.ToAddress.ContactEmail);
            string xml = web.DownloadString(url);
            if (xml.Contains("<Error>"))
            {
                int idx1 = xml.IndexOf("<Description>") + 13;
                int idx2 = xml.IndexOf("</Description>");
                int l = xml.Length;
                string errDesc = xml.Substring(idx1, idx2 - idx1);
                throw new USPSManagerException(errDesc);
            }
            int i1 = xml.IndexOf("<SignatureConfirmationLabel>") + 28;
            int i2 = xml.IndexOf("</DeliveryConfirmationLabel>");
            package.ShippingLabel = StringToUTF8ByteArray(xml.Substring(i1, i2 - i1));
            return package;
        }

     
        #endregion

        #region Rates

        public RateResponse GetRate(Package package)
        {
            try
            {
                string url = "?API=RateV2&XML=<RateV2Request USERID=\"{0}\"><Package ID=\"0\"><Service>{1}</Service><ZipOrigination>{2}</ZipOrigination><ZipDestination>{3}</ZipDestination><Pounds>{4}</Pounds><Ounces>{5}</Ounces><Container>{6}</Container><Size>{7}</Size></Package></RateV2Request>";

                int lb = package.WeightInOunces / 16;
                int oz = package.WeightInOunces % 16;
                string container = package.PackageType.ToString().Replace("_", " ");
                if (container == "None")
                    url = url.Replace("<Container>{6}</Container>", "");
                string fromZip = package.FromAddress.Zip;
                if (package.OriginZipcode != null && package.OriginZipcode.Length > 0)
                    fromZip = package.OriginZipcode;

                
                url = GetURL() + url;
                url = String.Format(url, _userid, package.ServiceType.ToString(), fromZip, package.ToAddress.Zip, lb.ToString(), oz.ToString(), container, package.PackageSize.ToString().Replace("_", " "));
                string xml = web.DownloadString(url);
                if (xml.Contains("<Error>"))
                {
                    int idx1 = xml.IndexOf("<Description>") + 13;
                    int idx2 = xml.IndexOf("</Description>");
                    int l = xml.Length;
                    string errDesc = xml.Substring(idx1, idx2 - idx1);
                    throw new USPSManagerException(errDesc);
                }

                return RateResponse.FromXml(xml);
            }
            catch (WebException ex)
            {
                throw new USPSManagerException(ex);
            }
        }
        #endregion

        #region TextConversions
        /// <summary>
        /// To convert a Byte Array of Unicode values (UTF-8 encoded) to a complete String.
        /// </summary>
        /// <param name="characters">Unicode Byte Array to be converted to String</param>
        /// <returns>String converted from Unicode Byte Array</returns>
        private String UTF8ByteArrayToString(Byte[] characters)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            String constructedString = encoding.GetString(characters);
            return (constructedString);
        }

        /// <summary>
        /// Converts the String to UTF8 Byte array and is used in De serialization
        /// </summary>
        /// <param name="pXmlString"></param>
        /// <returns></returns>
        private Byte[] StringToUTF8ByteArray(String pXmlString)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            Byte[] byteArray = encoding.GetBytes(pXmlString);
            return byteArray;
        }
        #endregion

        #region Private methods
        private string GetURL()
        {
            string url = ProductionUrl;
            if (TestMode)
                url = TestingUrl;
            return url;
        }
        #endregion
    }
}
