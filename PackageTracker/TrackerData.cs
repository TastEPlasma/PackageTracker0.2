using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace PackageTracker
{
    class TrackerData
    {
        public TrackerData()
        {
            DeleteMe = false;
            Service = ParcelService.None;
            Status = PackageStatus.NotFound;
        }

        public int ID { get; set; }
        public string CustomName { get; set; }
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
                    case PackageStatus.NotFound: return "NotFound";
                    case PackageStatus.NotShipped: return "NotShipped";
                    case PackageStatus.Other: return "Other";
                    case PackageStatus.OutForDelivery: return "OutForDelivery";
                    case PackageStatus.Returned: return "Returned";
                    case PackageStatus.Shipped: return "Shipped";
                    case PackageStatus.PickUp: return "ReadyForPickUp";
                    default: return "Other";
                }
            }
            set
            {

            }
        }

        public BitmapImage CarrierCode
        {
            get
            {
                switch (Service)
                {
                    case ParcelService.FedEx: return ImageHolding.FedEx;
                    case ParcelService.UPS: return ImageHolding.UPS;
                    case ParcelService.USPS: return ImageHolding.USPS;
                    case ParcelService.None: return ImageHolding.Unknown;
                    default: return ImageHolding.Unknown;
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
        Other,
        PickUp
    }

    enum ParcelService
    {
        FedEx,
        UPS,
        USPS,
        None
    }


    static class ImageHolding
    { 
        static ImageHolding()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "FedExCarrier.GIF");
            var uri = new Uri(path);
            FedExbitmap = new BitmapImage(uri);

            path = Path.Combine(Environment.CurrentDirectory, "UPSCarrier.GIF");
            uri = new Uri(path);
            UPSbitmap = new BitmapImage(uri);

            path = Path.Combine(Environment.CurrentDirectory, "USPSCarrier.GIF");
            uri = new Uri(path);
            USPSbitmap = new BitmapImage(uri); 

            path = Path.Combine(Environment.CurrentDirectory, "UnknownCarrier.GIF");
            uri = new Uri(path);
            Unknownbitmap = new BitmapImage(uri);
        }

        static BitmapImage FedExbitmap;
        static BitmapImage UPSbitmap;
        static BitmapImage USPSbitmap;
        static BitmapImage Unknownbitmap;


        public static BitmapImage FedEx
        {
            get { return FedExbitmap; }
            set { /*Do Nothing*/ }
        }

        public static BitmapImage UPS 
        {
            get { return UPSbitmap; }
            set { /*Do Nothing*/ }
        }

        public static BitmapImage USPS
        {
            get { return USPSbitmap; }
            set { /*Do Nothing*/ }
        }

        public static BitmapImage Unknown
        {
            get { return Unknownbitmap; }
            set { /*Do Nothing*/ }
        }
    }
}
