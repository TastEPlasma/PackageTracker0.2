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


    }

    enum PackageStatus
    {
        NotFound,
        NotShipped,
        Shipped,
        OutForDelivery,
        Delivered
    }

    enum ParcelService
    {
        FedEx,
        UPS,
        USPS
    }
}
