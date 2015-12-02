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
        private string _userid = "857STUDE5322";

        private bool _TestMode;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor that requires no arguments, uses default ID
        /// </summary>
        public USPSManager()
        {
            web = new WebClient();
            _TestMode = false;
        }

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
        public string setUserID 
        {
            get { return _userid; }
            set { _userid = value; }
        }

        public bool TestMode
        {
            get { return _TestMode; }
            set { _TestMode = value; }
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
        public void ResetResetCredentialsToDefaults()
        {
            _userid = "857STUDE5322";
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
