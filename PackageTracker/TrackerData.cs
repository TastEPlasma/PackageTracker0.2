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
    #region Namespace Enumerations
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
    #endregion

    class TrackerData
    {
        #region Properties
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
                    case ParcelService.FedEx: return ImageLoadingAndHolding.FedEx;
                    case ParcelService.UPS: return ImageLoadingAndHolding.UPS;
                    case ParcelService.USPS: return ImageLoadingAndHolding.USPS;
                    case ParcelService.None: return ImageLoadingAndHolding.Unknown;
                    default: return ImageLoadingAndHolding.Unknown;
                }
            }
            set
            {

            }
        }
        #endregion

        #region Constructors
        public TrackerData()
        {
            DeleteMe = false;
            Service = ParcelService.None;
            Status = PackageStatus.NotFound;
        }
        #endregion
    }

    static class ImageLoadingAndHolding
    {
        #region Private Members
        static BitmapImage FedExbitmap;
        static BitmapImage UPSbitmap;
        static BitmapImage USPSbitmap;
        static BitmapImage Unknownbitmap;
        #endregion

        #region Properties
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
        #endregion

        #region Constructors
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
        #endregion


    }
}
