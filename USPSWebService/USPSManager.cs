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
        public USPSManager()
        {
            web = new WebClient();
            this.UserID = "857STUDE5322";
        }

        private const string ProductionUrl = "http://production.shippingapis.com/ShippingAPI.dll";
        private WebClient web;

        public string UserID { get; set; }


        public TrackingInfo GetTrackingInfo(string TrackingNumber)
        {
            try
            {
                string trackurl = "?API=TrackV2&XML=<TrackRequest USERID=\"{0}\"><TrackID ID=\"{1}\"></TrackID></TrackRequest>";
                string url = ProductionUrl + trackurl;
                url = String.Format(url, UserID, TrackingNumber);
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
        public void ResetCredentialsToDefaults()
        {
            UserID = "857STUDE5322";
        }

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
        
    }
}
