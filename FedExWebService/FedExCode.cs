﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Protocols;
using System.Diagnostics;
using FedExWebService.FedExWebReference;

namespace FedExWebService
{
    static class AccountInfo
    {
        public static string UserKey = "1CK3fnM8LhfQWteN";
        public static string UserPassword = "vh6rTNPVog2PAXKRh44SiJznk";
        public static string AccountNumber = "510087186";
        public static string MeterNumber = "118691686";
        public static string TransactionID = "TEST";
        public static string TrackingNumber;
    }

    public static class FedEx
    {
        public static TrackRequest CreateTrackRequest(string TrackingNumber)
        {
            //create credentials object
            //AccountInfo Credentials = new AccountInfo();
            AccountInfo.TrackingNumber = TrackingNumber;

            // Build the TrackRequest
            TrackRequest request = new TrackRequest();
            //
            request.WebAuthenticationDetail = new WebAuthenticationDetail();
            request.WebAuthenticationDetail.UserCredential = new WebAuthenticationCredential();
            request.WebAuthenticationDetail.UserCredential.Key = AccountInfo.UserKey; // Replace "XXX" with the Key
            request.WebAuthenticationDetail.UserCredential.Password = AccountInfo.UserPassword; // Replace "XXX" with the Password
            request.WebAuthenticationDetail.ParentCredential = new WebAuthenticationCredential();
            request.WebAuthenticationDetail.ParentCredential.Key = "XXX"; // Replace "XXX" with the Key
            request.WebAuthenticationDetail.ParentCredential.Password = "XXX"; // Replace "XXX"
            request.ClientDetail = new ClientDetail();
            request.ClientDetail.AccountNumber = AccountInfo.AccountNumber; // Replace "XXX" with the client's account number
            request.ClientDetail.MeterNumber = AccountInfo.MeterNumber; // Replace "XXX" with the client's meter number
            request.TransactionDetail = new TransactionDetail();
            request.TransactionDetail.CustomerTransactionId = AccountInfo.TransactionID;  //This is a reference field for the customer.  Any value can be used and will be provided in the response.
            //
            request.Version = new VersionId();
            //
            // Tracking information
            request.SelectionDetails = new TrackSelectionDetail[1] { new TrackSelectionDetail() };
            request.SelectionDetails[0].PackageIdentifier = new TrackPackageIdentifier();
            request.SelectionDetails[0].PackageIdentifier.Value = AccountInfo.TrackingNumber; // Replace "XXX" with tracking number or door tag
            request.SelectionDetails[0].PackageIdentifier.Type = TrackIdentifierType.TRACKING_NUMBER_OR_DOORTAG;
            //
            // Date range is optional.
            // If omitted, set to false
            request.SelectionDetails[0].ShipDateRangeBegin = DateTime.Parse("06/18/2012"); //MM/DD/YYYY
            request.SelectionDetails[0].ShipDateRangeEnd = request.SelectionDetails[0].ShipDateRangeBegin.AddDays(0);
            request.SelectionDetails[0].ShipDateRangeBeginSpecified = false;
            request.SelectionDetails[0].ShipDateRangeEndSpecified = false;
            //
            // Include detailed scans is optional.
            // If omitted, set to false
            request.ProcessingOptions = new TrackRequestProcessingOptionType[1];
            request.ProcessingOptions[0] = TrackRequestProcessingOptionType.INCLUDE_DETAILED_SCANS;
            return request;
        }

        public static void ShowTrackReply(TrackReply reply)
        {
            // Track details for each package
            foreach (CompletedTrackDetail completedTrackDetail in reply.CompletedTrackDetails)
            {
                foreach (TrackDetail trackDetail in completedTrackDetail.TrackDetails)
                {
                    Console.WriteLine("Tracking details:");
                    Console.WriteLine("**************************************");
                    ShowNotification(trackDetail.Notification);
                    Console.WriteLine("Tracking number: {0}", trackDetail.TrackingNumber);
                    Console.WriteLine("Tracking number unique identifier: {0}", trackDetail.TrackingNumberUniqueIdentifier);
                    if(trackDetail.StatusDetail != null)
                    {
                        Console.WriteLine("Track Status: {0} {1}", trackDetail.StatusDetail.Description, trackDetail.StatusDetail.Code);
                    }
                    Console.WriteLine("Carrier code: {0}", trackDetail.CarrierCode);

                    if (trackDetail.OtherIdentifiers != null)
                    {
                        foreach (TrackOtherIdentifierDetail identifier in trackDetail.OtherIdentifiers)
                        {
                            Console.WriteLine("Other Identifier: {0} {1}", identifier.PackageIdentifier.Type, identifier.PackageIdentifier.Value);
                        }
                    }
                    if (trackDetail.Service != null)
                    {
                        Console.WriteLine("ServiceInfo: {0}", trackDetail.Service.Description);
                    }
                    if (trackDetail.PackageWeight != null)
                    {
                        Console.WriteLine("Package weight: {0} {1}", trackDetail.PackageWeight.Value, trackDetail.PackageWeight.Units);
                    }
                    if (trackDetail.ShipmentWeight != null)
                    {
                        Console.WriteLine("Shipment weight: {0} {1}", trackDetail.ShipmentWeight.Value, trackDetail.ShipmentWeight.Units);
                    }
                    if (trackDetail.Packaging != null)
                    {
                        Console.WriteLine("Packaging: {0}", trackDetail.Packaging);
                    }
                    Console.WriteLine("Package Sequence Number: {0}", trackDetail.PackageSequenceNumber);
                    Console.WriteLine("Package Count: {0} ", trackDetail.PackageCount);
                    if (trackDetail.ShipTimestampSpecified)
                    {
                        Console.WriteLine("Ship timestamp: {0}", trackDetail.ShipTimestamp);
                    }
                    if (trackDetail.DestinationAddress != null)
                    {
                        Console.WriteLine("Destination: {0}, {1}", trackDetail.DestinationAddress.City, trackDetail.DestinationAddress.StateOrProvinceCode);
                    }
                    if (trackDetail.ActualDeliveryTimestampSpecified)
                    {
                        Console.WriteLine("Actual delivery timestamp: {0}", trackDetail.ActualDeliveryTimestamp);
                    }
                    if (trackDetail.AvailableImages != null)
                    {
                        foreach (AvailableImageType ImageType in trackDetail.AvailableImages)
                        {
                            Console.WriteLine("Image availability: {0}", ImageType);
                        }
                    }
                    if (trackDetail.NotificationEventsAvailable != null)
                    {
                        foreach (EMailNotificationEventType notificationEventType in trackDetail.NotificationEventsAvailable)
                        {
                            Console.WriteLine("EmailNotificationEvent type : {0}", notificationEventType);
                        }
                    }

                    //Events
                    Console.WriteLine();
                    if (trackDetail.Events != null)
                    {
                        Console.WriteLine("Track Events:");
                        foreach (TrackEvent trackevent in trackDetail.Events)
                        {
                            if (trackevent.TimestampSpecified)
                            {
                                Console.WriteLine("Timestamp: {0}", trackevent.Timestamp);
                            }
                            Console.WriteLine("Event: {0} ({1})", trackevent.EventDescription, trackevent.EventType);
                            Console.WriteLine("***");
                        }
                        Console.WriteLine();
                    }
                    Console.WriteLine("**************************************");
                }
            }

        }
        private static void ShowNotification(Notification notification)
        {
            Console.WriteLine(" Severity: {0}", notification.Severity);
            Console.WriteLine(" Code: {0}", notification.Code);
            Console.WriteLine(" Message: {0}", notification.Message);
            Console.WriteLine(" Source: {0}", notification.Source);
        }
        private static void ShowNotifications(TrackReply reply)
        {
            Console.WriteLine("Notifications");
            for (int i = 0; i < reply.Notifications.Length; i++)
            {
                Notification notification = reply.Notifications[i];
                Console.WriteLine("Notification no. {0}", i);
                ShowNotification(notification);
            }
        }
        public static bool usePropertyFile() //Set to true for common properties to be set with getProperty function.
        {
            return getProperty("usefile").Equals("true");
        }
        public static String getProperty(String propertyname) //Sets common properties for testing purposes.
        {
            try
            {
                String filename = "C:\\filepath\\filename.txt";
                if (System.IO.File.Exists(filename))
                {
                    System.IO.StreamReader sr = new System.IO.StreamReader(filename);
                    do
                    {
                        String[] parts = sr.ReadLine().Split(',');
                        if (parts[0].Equals(propertyname) && parts.Length == 2)
                        {
                            return parts[1];
                        }
                    }
                    while (!sr.EndOfStream);
                }
                Console.WriteLine("Property {0} set to default 'XXX'", propertyname);
                return "XXX";
            }
            catch (Exception e)
            {
                Console.WriteLine("Property {0} set to default 'XXX': {1}", propertyname, e);
                return "XXX";
            }
        }
    }
}
