using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageTracker
{
    class TrackerData
    {
        public TrackerData()
        {
            DeleteMe = false;
        }

        public int ID { get; set; }
        public string TrackingNumber { get; set; }
        public string Location { get; set; }
        public PackageStatus Status { get; set; }
        public ParcelService Service { get; set; }
        public bool DeleteMe { get; set; }

        public string StatusOfPackage
        {
            get
            {
                switch(Status)
                {
                    case PackageStatus.Delivered: return "Delivered";
                    case PackageStatus.NotFound: return "Not Found";
                    case PackageStatus.NotShipped: return "NotShipped";
                    case PackageStatus.Other: return "Other";
                    case PackageStatus.OutForDelivery: return "OutForDelivery";
                    case PackageStatus.Returned: return "Returned";
                    case PackageStatus.Shipped: return "Shipped";
                    default: return "Other";
                }
            }
            set
            {

            }
        }

        public string CarrierCode
        {
            get
            {
                switch (Service)
                {
                    case ParcelService.FedEx: return "FedEx";
                    case ParcelService.UPS: return "UPS";
                    case ParcelService.USPS: return "USPS";
                    default: return "Other";
                }
            }
            set
            {

            }
        }
    }

    enum PackageStatus
    {
        NotFound,
        NotShipped,
        Shipped,
        OutForDelivery,
        Delivered,
        Returned,
        Other
    }

    enum ParcelService
    {
        FedEx,
        UPS,
        USPS
    }
}
