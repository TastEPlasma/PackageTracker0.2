using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Net;

namespace USPSWebService
{
    public static class USPS
    {
        //need to create an XML document request containing tracking number
        public static void CreateXMLRequest(string TrackingNumber)
        {
            // build XML request 
            var requestXml = new XmlDocument();
            requestXml.CreateElement("TrackRequest");
            requestXml.CreateAttribute("USERID", "857STUDE5322");
            requestXml.CreateAttribute("TrackID", TrackingNumber);
            
            
             
            //USERID="xxxxxxx"><Address ID="0"><Address1></Address1><Address2>6406 Ivy Lane</Address2><City>Greenbelt</City><State>MD</State></Address></ZipCodeLookupRequest>

            /*http://production.shippingapis.com/ShippingAPITest.dll?API=TrackV2 
            &XML=<TrackRequest USERID="xxxxxxxx"> <TrackID ID="EJ958083578US">
                </TrackID></TrackRequest>*/ //USERID='857STUDE5322

            //var requestUrl = new UriBuilder("http://stg-production.shippingapis.com/ShippingAPI.dll?API=TrackV2");
            //requestUrl.Query = "&XML=<?xml version='1.0' encoding='UTF-8' ?><TrackRequest USERID='EJ958083578US'> <TrackID ID='EJ123456780US'></TrackID></TrackRequest>";
            //var request = WebRequest.Create(requestUrl.Uri);




            var httpRequest = HttpWebRequest.Create("http://stg-production.shippingapis.com/ShippingAPI.dll?API=TrackV2");
            httpRequest.Method = "POST"; 
            httpRequest.ContentType = "text/xml";


            // set appropriate headers

            using (var requestStream = httpRequest.GetRequestStream())
            {
                requestXml.Save(requestStream);
            }

            using (var response = (HttpWebResponse)httpRequest.GetResponse())
            using (var responseStream = response.GetResponseStream())
            {
                // may want to check response.StatusCode to
                // see if the request was successful

                var responseXml = new XmlDocument();
                responseXml.Load(responseStream);
                responseXml.Save(Console.Out);

            }

        }

    }
}
