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
    public class Package
    {
        public Package()
        {
            _LabelType = LabelType.FullLabel;
            _LabelImageType = LabelImageType.TIF;
            _ServiceType = ServiceType.First_Class;
        }

        private LabelType _LabelType;

        public LabelType LabelType
        {
            get { return _LabelType; }
            set { _LabelType = value; }
        }

        private Address _FromAddress = new Address();

        public Address FromAddress
        {
            get { return _FromAddress; }
            set { _FromAddress = value; }
        }

        private Address _ToAddress = new Address();

        public Address ToAddress
        {
            get { return _ToAddress; }
            set { _ToAddress = value; }
        }

        private int _WeightInOunces = 0;
    
        public int WeightInOunces
        {
            get { return _WeightInOunces; }
            set { _WeightInOunces = value; }
        }

        private ServiceType _ServiceType;

        public ServiceType ServiceType
        {
            get { return _ServiceType; }
            set { _ServiceType = value; }
        }

        private bool _SeparateReceiptPage = false;

        public bool SeparateReceiptPage
        {
            get { return _SeparateReceiptPage; }
            set { _SeparateReceiptPage = value; }
        }

        private string _OriginZipcode = "";

        public string OriginZipcode
        {
            get { return _OriginZipcode; }
            set { _OriginZipcode = value; }
        }

        private LabelImageType _LabelImageType = LabelImageType.TIF;

        public LabelImageType LabelImageType
        {
            get { return _LabelImageType; }
            set { _LabelImageType = value; }
        }

        private DateTime _ShipDate = DateTime.Now;

        public DateTime ShipDate
        {
            get { return _ShipDate; }
            set { _ShipDate = value; }
        }

        private string _ReferenceNumber = "";

        public string ReferenceNumber
        {
            get { return _ReferenceNumber; }
            set { _ReferenceNumber = value; }
        }

        private bool _AddressServiceRequested = false;

        public bool AddressServiceRequested
        {
            get { return _AddressServiceRequested; }
            set { _AddressServiceRequested = value; }
        }

        private byte[] _ShippingLabel;

        public byte[] ShippingLabel
        {
            get { return _ShippingLabel; }
            set { _ShippingLabel = value; }
        }

        private PackageType _PackageType;

        public PackageType PackageType
        {
            get { return _PackageType; }
            set { _PackageType = value; }
        }

        private PackageSize _PackageSize;

        public PackageSize PackageSize
        {
            get { return _PackageSize; }
            set { _PackageSize = value; }
        }


    }

    public enum PackageType {None, Flat_Rate_Envelope, Flat_Rate_Box};
    public enum PackageSize { None, Regular, Large, Oversize};
    public enum LabelImageType{TIF, PDF, None};
    public enum ServiceType{Priority, First_Class, Parcel_Post, Bound_Printed_Matter, Media_Mail, Library_Mail};
    public enum LabelType{FullLabel = 1, DeliveryConfirmationBarcode = 2};
}
