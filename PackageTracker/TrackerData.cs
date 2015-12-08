namespace PackageTracker
{
    using System;
    using System.IO;
    using System.Windows.Media.Imaging;

    internal enum PackageStatus
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

    internal enum ParcelService
    {
        FedEx,
        UPS,
        USPS,
        None
    }

    internal class TrackerData
    {
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
                switch (Status)
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
                    case ParcelService.FedEx: return ImageLoadingAndHolding.FedExbitmap;
                    case ParcelService.UPS: return ImageLoadingAndHolding.UPSbitmap;
                    case ParcelService.USPS: return ImageLoadingAndHolding.USPSbitmap;
                    case ParcelService.None: return ImageLoadingAndHolding.Unknownbitmap;
                    default: return ImageLoadingAndHolding.Unknownbitmap;
                }
            }
            set
            {
            }
        }

        public TrackerData()
        {
            DeleteMe = false;
            Service = ParcelService.None;
            Status = PackageStatus.NotFound;
        }
    }

    internal static class ImageLoadingAndHolding
    {
        public static BitmapImage FedExbitmap { get; set; }

        public static BitmapImage UPSbitmap { get; set; }

        public static BitmapImage USPSbitmap { get; set; }

        public static BitmapImage Unknownbitmap { get; set; }

        static ImageLoadingAndHolding()
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
    }
}