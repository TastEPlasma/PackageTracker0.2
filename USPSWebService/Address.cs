//////////////////////////////////////////////////////////////////////////
///This software is provided to you as-is and with not warranties!!!
///Use this software at your own risk.
///This software is Copyright by Scott Smith 2006
///You are free to use this software as you see fit.
//////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace MAX.USPS
{
    [Serializable()]
    public class Address
    {
        public Address()
        {
            this._ID = 1;
        }

        private int _ID;
        /// <summary>
        /// ID of this address
        /// </summary>
        public int ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        private string _FirmName = "";
        /// <summary>
        /// Name of the Firm or Company
        /// </summary>
        public string FirmName
        {
            get { return _FirmName; }
            set { _FirmName = value; }
        }

        private string _Contact = "";
        /// <summary>
        /// The contact is used to send confirmation when a package is shipped
        /// </summary>
        public string Contact
        {
            get { return _Contact; }
            set { _Contact = value; }
        }

        private string _ContactEmail = "";
        /// <summary>
        /// The contacts email address is used to send delivery confirmation
        /// </summary>
        public string ContactEmail
        {
            get { return _ContactEmail; }
            set { _ContactEmail = value; }
        }


        private string _Address1 = "";
        /// <summary>
        /// Address Line 1 is used to provide an apartment or suite
        /// number, if applicable. Maximum characters allowed: 38
        /// </summary>
        public string Address1
        {
            get { return _Address1; }
            set 
            {
                if (value.Length > 38)
                    throw new USPSManagerException("Address1 is is limited to a maximum of 38 characters.");
                _Address1 = value; 
            }
        }

        private string _Address2 = "";
        /// <summary>
        /// Street address
        /// Maximum characters allowed: 38
        /// </summary>
        public string Address2
        {
            get { return _Address2; }
            set 
            {
                if (value.Length > 38)
                    throw new USPSManagerException("Address2 is is limited to a maximum of 38 characters.");
                _Address2 = value; 
            }
        }

        private string _City = "";
        /// <summary>
        /// City
        /// Either the City and State or Zip are required.
        /// Maximum characters allowed: 15
        /// </summary>
        public string City
        {
            get { return _City; }
            set 
            {
                if (value.Length > 15)
                    throw new USPSManagerException("City is is limited to a maximum of 15 characters.");
                _City = value; }
        }

        private string _State = "";
        /// <summary>
        /// State
        /// Either the City and State or Zip are required.
        /// Maximum characters allowed = 2
        /// </summary>
        public string State
        {
            get { return _State; }
            set 
            {
                if (value.Length > 2)
                    throw new USPSManagerException("State is is limited to a maximum of 2 characters.");
                _State = value; 
            }
        }

        private string _Zip = "";
        /// <summary>
        /// Zipcode
        /// Maximum characters allowed = 5
        /// </summary>
        public string Zip
        {
            get { return _Zip; }
            set 
            {
                if (value.Length > 5)
                    throw new USPSManagerException("Zip is is limited to a maximum of 5 characters.");
                _Zip = value; 
            }
        }

        private string _ZipPlus4 = "";
        /// <summary>
        /// Zip code extension
        /// Maximum characters allowed = 4
        /// </summary>
        public string ZipPlus4
        {
            get { return _ZipPlus4; }
            set 
            {
                if (value.Length > 5)
                    throw new USPSManagerException("Zip is is limited to a maximum of 5 characters.");
                _ZipPlus4 = value; 
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // FromXML medthod provided by viperguynaz via codeproject
        //////////////////////////////////////////////////////////////////////////
        

        /// <summary>
        /// Get an Address object from an xml string.
        /// </summary>
        /// <param name="xml">XML representation of an Address Object</param>
        /// <returns>Address object</returns>
        public static Address FromXml(string xml)
        {
            Address a = new Address();

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(xml);

            System.Xml.XmlNode element = doc.SelectSingleNode("/AddressValidateResponse/Address/FirmName");
            if (element != null)
                a._FirmName = element.InnerText;
            element = doc.SelectSingleNode("/AddressValidateResponse/Address/Address1");
            if (element != null)
                a._Address1 = element.InnerText;
            element = doc.SelectSingleNode("/AddressValidateResponse/Address/Address2");
            if (element != null)
                a._Address2 = element.InnerText;
            element = doc.SelectSingleNode("/AddressValidateResponse/Address/City");
            if (element != null)
                a._City = element.InnerText;
            element = doc.SelectSingleNode("/AddressValidateResponse/Address/State");
            if (element != null)
                a._State = element.InnerText;
            element = doc.SelectSingleNode("/AddressValidateResponse/Address/Zip5");
            if (element != null)
                a._Zip = element.InnerText;
            element = doc.SelectSingleNode("/AddressValidateResponse/Address/Zip4");
            if (element != null)
                a._ZipPlus4 = element.InnerText;

            return a;
        }

        ///// <summary>
        ///// Get an Address object from an xml string.
        ///// </summary>
        ///// <param name="xml">XML representation of an Address Object</param>
        ///// <returns></returns>
        //public static Address FromXml(string xml)
        //{
        //    int idx1 = 0;
        //    int idx2 = 0;
        //    Address a = new Address();
        //    if (xml.Contains("<FirmName>"))
        //    {
        //        idx1 = xml.IndexOf("<FirmName>") + 10;
        //        idx2 = xml.IndexOf("</FirmName>");
        //        a._FirmName = xml.Substring(idx1, idx2-idx1);
        //    }
        //    if (xml.Contains("<Address1>"))
        //    {
        //        idx1 = xml.IndexOf("<Address1>") + 10;
        //        idx2 = xml.IndexOf("</Address1>");
        //        a._Address1 = xml.Substring(idx1, idx2-idx1);
        //    }
        //    if (xml.Contains("<Address2>"))
        //    {
        //        idx1 = xml.IndexOf("<Address2>") + 10;
        //        idx2 = xml.IndexOf("</Address2>");
        //        a._Address2 = xml.Substring(idx1, idx2 - idx1);
        //    }
        //    if (xml.Contains("<City>"))
        //    {
        //        idx1 = xml.IndexOf("<City>") + 6;
        //        idx2 = xml.IndexOf("</City>");
        //        a._City = xml.Substring(idx1, idx2 - idx1);
        //    }
        //    if (xml.Contains("<State>"))
        //    {
        //        idx1 = xml.IndexOf("<State>") + 7;
        //        idx2 = xml.IndexOf("</State>");
        //        a._State = xml.Substring(idx1, idx2 - idx1);
        //    }

        //    if (xml.Contains("<Zip5>"))
        //    {
        //        idx1 = xml.IndexOf("<Zip5>") + 6;
        //        idx2 = xml.IndexOf("</Zip5>");
        //        a._Zip = xml.Substring( idx1, idx2- idx1);
        //    }

        //    if (xml.Contains("<Zip4>"))
        //    {
        //        idx1 = xml.IndexOf("<Zip4>") + 6;
        //        idx2 = xml.IndexOf("</Zip4>");
        //        a._ZipPlus4 = xml.Substring(idx1, idx2-idx1);
        //    }
        //    return a;
        //}

        /// <summary>
        /// Get the Xml representation of this address object
        /// </summary>
        /// <returns>String</returns>
        public string ToXml()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<Address ID=\"" + this.ID.ToString() + "\">");
            sb.Append("<Address1>"+this._Address1+"</Address1>");
            sb.Append("<Address2>" + this._Address2 + "</Address2>");
            sb.Append("<City>"+this.City+"</City>");
            sb.Append("<State>"+this.State+"</State>");
            sb.Append("<Zip5>"+this.Zip+"</Zip5>");
            sb.Append("<Zip4>"+this.ZipPlus4+"</Zip4>");
            sb.Append("</Address>");
            return sb.ToString();
        }
    }
}
