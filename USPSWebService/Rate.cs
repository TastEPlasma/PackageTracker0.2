//////////////////////////////////////////////////////////////////////////
///This software is provided to you as-is and with not warranties!!!
///Use this software at your own risk.
///This software is Copyright by Scott Smith 2006
///You are free to use this software as you see fit.
//////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

namespace MAX.USPS
{
    public class RateResponse
    {
        public RateResponse()
        {
            _Postage = new List<Postage>();
        }
        private string _OriginZip = "";

        public string OriginZip
        {
            get { return _OriginZip; }
            set { _OriginZip = value; }
        }

        private string _DestZip = "";

        public string DestZip
        {
            get { return _DestZip; }
            set { _DestZip = value; }
        }

        private int _Pounds = 0;

        public int Pounds
        {
            get { return _Pounds; }
            set { _Pounds = value; }
        }

        private int _Ounces = 0;

        public int Ounces
        {
            get { return _Ounces; }
            set { _Ounces = value; }
        }

        private string _Container = "";

        public string Container
        {
            get { return _Container; }
            set { _Container = value; }
        }

        private string _Size = "";

        public string Size
        {
            get { return _Size; }
            set { _Size = value; }
        }

        private string _Zone = "";

        public string Zone
        {
            get { return _Zone; }
            set { _Zone = value; }
        }


        private List<Postage> _Postage;

        public List<Postage> Postage
        {
            get { return _Postage; }
            set { _Postage = value; }
        }

        public static RateResponse FromXml(string xml)
        {
            RateResponse r = new RateResponse();
            int idx1 = 0;
            int idx2 = 0;

            if(xml.Contains("<ZipOrigination>"))
            {
                idx1 = xml.IndexOf("<ZipOrigination>") + 17;
                idx2 = xml.IndexOf("</ZipOrigination>");
                r.OriginZip = xml.Substring(idx1, idx2 - idx1);
            }

            if (xml.Contains("<ZipDestination>"))
            {
                idx1 = xml.IndexOf("<ZipDestination>") + 16;
                idx2 = xml.IndexOf("</ZipDestination>");
                r.OriginZip = xml.Substring(idx1, idx2 - idx1);
            }

            if (xml.Contains("<Pounds>"))
            {
                idx1 = xml.IndexOf("<Pounds>") + 8;
                idx2 = xml.IndexOf("</Pounds>");
                r.OriginZip = xml.Substring(idx1, idx2 - idx1);
            }

            if (xml.Contains("<Ounces>"))
            {
                idx1 = xml.IndexOf("<Ounces>") + 8;
                idx2 = xml.IndexOf("</Ounces>");
                r.OriginZip = xml.Substring(idx1, idx2 - idx1);
            }

            if (xml.Contains("<Container>"))
            {
                idx1 = xml.IndexOf("<Container>") + 11;
                idx2 = xml.IndexOf("</Container>");
                r.OriginZip = xml.Substring(idx1, idx2 - idx1);
            }

            if (xml.Contains("<Size>"))
            {
                idx1 = xml.IndexOf("<Size>") + 6;
                idx2 = xml.IndexOf("</Size>");
                r.OriginZip = xml.Substring(idx1, idx2 - idx1);
            }

            if (xml.Contains("<Zone>"))
            {
                idx1 = xml.IndexOf("<Zone>") + 6;
                idx2 = xml.IndexOf("</Zone>");
                r.OriginZip = xml.Substring(idx1, idx2 - idx1);
            }

            int lastidx = 0;
            while (xml.IndexOf("<MailService>", lastidx) > -1)
            {
                Postage p = new Postage();
                idx1 = xml.IndexOf("<MailService>", lastidx) + 13;
                idx2 = xml.IndexOf("</MailService>", lastidx + 13);
                p.MailService = xml.Substring(idx1, idx2 - idx1);

                idx1 = xml.IndexOf("<Rate>", lastidx) + 6;
                idx2 = xml.IndexOf("</Rate>", lastidx + 13);
                p.Amount = Decimal.Parse(xml.Substring(idx1, idx2 - idx1));
                r.Postage.Add(p);
                lastidx = idx2;
            }
            return r;
        }
    }

    public class Postage
    {
        private string _MailService;

        public string MailService
        {
            get { return _MailService; }
            set { _MailService = value; }
        }

        private decimal _Amount;

        public decimal Amount
        {
            get { return _Amount; }
            set { _Amount = value; }
        }
    };
}
