using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services;
using FedExWebService.FedExWebReference;
using FedExWebService;
using System.Web.Services.Protocols;

namespace PackageTracker
{
    class TrackingControl
    {
        List<TrackerData> TrackingData;


        public void UpdateTrackingInformation(List<TrackerData> CurrentData)
        {
            //A list of objects is passed by reference
            TrackingData = CurrentData;

            //Take list of package data, parse out tracking numbers
            foreach(TrackerData Entry in TrackingData)
            {
                SendRequestToWebService(Entry);
            }
        }

        //Send request to webservices, receive raw data
        private void SendRequestToWebService(TrackerData Entry)
        {
            //open webservice
            //pass in tracking number
            TrackRequest request = FedEx.CreateTrackRequest(Entry.TrackingNumber);
            //
            TrackService service = new TrackService();
            if (FedEx.usePropertyFile())
            {
                service.Url = FedEx.getProperty("endpoint");
            }

            try
            {
                // Call the Track web service passing in a TrackRequest and returning a TrackReply
                TrackReply reply = service.track(request);
                if(reply.HighestSeverity != NotificationSeverityType.ERROR || reply.HighestSeverity != NotificationSeverityType.FAILURE)
                {
                    Console.WriteLine(reply.HighestSeverity);
                    //For debugging purposes
                    FedEx.ShowTrackReply(reply);
                    //Parse raw data here
                    ParseRawDataIntoList(Entry, reply);
                }
                else
                {
                    //ERROR HANDLING STATE HERE
                }
                
            }
            catch (SoapException e)
            {
                //Console.WriteLine(e.Detail.InnerText);
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.Message);
            }
            
        }
        
        //Process raw data and update list
        private void ParseRawDataIntoList(TrackerData Entry, TrackReply NewData)
        {
            foreach(CompletedTrackDetail completedTrackDetail in NewData.CompletedTrackDetails)
            {
                foreach(TrackDetail trackDetail in completedTrackDetail.TrackDetails)
                {
                    if(trackDetail.Notification.Severity == NotificationSeverityType.ERROR)
                    {
                        Entry.Location = "INVALID TRACKING NUMBER";
                        Entry.Status = PackageStatus.NotFound;
                    }
                    else
                    {
                        Entry.Location = trackDetail.StatusDetail.Location.City + ", " +
                            trackDetail.StatusDetail.Location.StateOrProvinceCode;

                        Entry.Service = ParcelService.FedEx;
                        switch (trackDetail.StatusDetail.Code)
                        {
                            case "DL": Entry.Status = PackageStatus.Delivered; break;
                            case "OD": Entry.Status = PackageStatus.OutForDelivery; break;
                            case "ED": Entry.Status = PackageStatus.OutForDelivery; break;
                            case "RS": Entry.Status = PackageStatus.Returned; break;
                            case "IT": Entry.Status = PackageStatus.Shipped; break;
                            case "PU": Entry.Status = PackageStatus.Shipped; break;
                            case "DP": Entry.Status = PackageStatus.Shipped; break;
                            case "AP": Entry.Status = PackageStatus.NotShipped; break;
                            case "OF": Entry.Status = PackageStatus.NotShipped; break;
                            default: Entry.Status = PackageStatus.Other; break;
                        }
                    }  
                }
            }           
        }
    }
}
