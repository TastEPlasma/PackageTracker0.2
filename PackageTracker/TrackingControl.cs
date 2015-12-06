using FedExWebService;
using FedExWebService.FedExWebReference;
using MAX.UPS;
using MAX.USPS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services.Protocols;
using UPSWebService.UPSWebReference;

namespace PackageTracker
{
    class TrackingControl
    {
        //Web service interface managers
        USPSManager POSTAL; 
        FedExManager FedEx;
        UPSManager UPS;

        public TrackingControl()
        {
            POSTAL = new USPSManager();
            UPS = new UPSManager();
            FedEx = new FedExManager();
        }

        public void UpdateTrackingInformation(List<TrackerData> TrackingData)
        {
            //Take list of package data, parse out tracking numbers, run each through web service
            foreach(TrackerData Entry in TrackingData)
            {
                if(Entry.TrackingNumber != "")
                {
                    //Check for reasonable length tracking number
                    int TNlength = Entry.TrackingNumber.Length;
                    if (TNlength > 6 && TNlength < 27)
                    {
                        //Check for UPS header characters in Tracking Number
                        char[] TNArray = Entry.TrackingNumber.ToArray();
                        if (TNArray[0] == '1' && TNArray[1] == 'Z')
                        {
                            SendRequestToUPSWebService(Entry);
                        }

                        //Check for sum digit to verify FedEx Tracking Number
                        else if (CheckFedExNumber(Entry.TrackingNumber))
                        {
                            //FedEX support removed due to lack of legitimate access
                            SendRequestToFedExWebService(Entry);
                            //FedExNotSupported(Entry);
                        }
                        //Default to check postal service
                        else
                        {
                            SendRequestToUSPSWebService(Entry);
                        }
                    }
                    else
                    {
                        Entry.Location = "Invalid Number Length";
                        Entry.Service = ParcelService.None;
                        Entry.Status = PackageStatus.NotFound;
                    }
                }
            }
        }

        public void UpdateCredentialInformation(FedExCredentialsData NewFedExData)
        {
            if (!String.IsNullOrWhiteSpace(NewFedExData.UserKey))
            {
                FedEx.UserKey = NewFedExData.UserKey;
            }
            if (!String.IsNullOrWhiteSpace(NewFedExData.UserPassword))
            {
                FedEx.UserPassword = NewFedExData.UserPassword;
            }
            if (!String.IsNullOrWhiteSpace(NewFedExData.AccountNumber))
            {
                FedEx.AccountNumber = NewFedExData.AccountNumber;
            }
            if (!String.IsNullOrWhiteSpace(NewFedExData.MeterNumber))
            {
                FedEx.MeterNumber = NewFedExData.MeterNumber;
            }  
        }

        public void UpdateCredentialInformation(UPSCredentialsData NewUPSData)
        {    
            if (!String.IsNullOrWhiteSpace(NewUPSData.Username))
            {
                UPS.Username = NewUPSData.Username;
            }
            if (!String.IsNullOrWhiteSpace(NewUPSData.Password))
            {
                UPS.Password = NewUPSData.Password;
            }
            if (!String.IsNullOrWhiteSpace(NewUPSData.AccessLicenseNumber))
            {
                UPS.AccessLicenseNumber = NewUPSData.AccessLicenseNumber;
            }
        }

        public void UpdateCredentialInformation(USPSCredentialsData NewUSPSData)
        {
            if(!String.IsNullOrWhiteSpace(NewUSPSData.UserID))
            {
                POSTAL.UserID = NewUSPSData.UserID;
            }
        }

        public void ResetCredentialsToDefaults(ParcelService Service)
        {
            switch (Service)
            {
                case ParcelService.FedEx:
                    {
                        FedEx.ResetCredentialsToDefaults();
                    } break;
                case ParcelService.UPS:
                    {
                        UPS.ResetCredentialsToDefaults();
                    } break;
                case ParcelService.USPS:
                    {
                        POSTAL.ResetCredentialsToDefaults();
                    } break;
                default: ; break;
            }
        }


        private void SendRequestToUSPSWebService(TrackerData Entry)
        {
            try
            {
                TrackingInfo Reply = POSTAL.GetTrackingInfo(Entry.TrackingNumber);
                ParseUSPSRawDataIntoList(Entry, Reply);
            }
            catch(USPSManagerException ex)
            {
                if(ex.Message == "Unable to connect to remote server")
                {
                    Entry.Location = "ERROR: No Connection";
                    Entry.Status = PackageStatus.Other;
                }
                else
                {
                    Entry.Location = "ERROR";
                    Entry.Status = PackageStatus.Other;
                }
            }   
        }

        private void ParseUSPSRawDataIntoList(TrackerData Entry, TrackingInfo Reply)
        {
            //Only need the information in Reply.Summary
            if (Reply.Summary.Contains("delivered"))
            {
                Entry.Status = PackageStatus.Delivered;
                Entry.Location = ExtractAddressFromString(Reply.Summary);
                Entry.Service = ParcelService.USPS;
            }
            else if (Reply.Summary.Contains("departed"))
            {
                Entry.Status = PackageStatus.Shipped;
                Entry.Location = ExtractAddressFromString(Reply.Summary);
                Entry.Service = ParcelService.USPS;
            }
            else if (Reply.Summary.Contains("picked up"))
            {
                Entry.Status = PackageStatus.Shipped;
                Entry.Location = ExtractAddressFromString(Reply.Summary);
                Entry.Service = ParcelService.USPS;
            }
            else if (Reply.Summary.Contains("arrived"))
            {
                Entry.Status = PackageStatus.Shipped;
                Entry.Location = ExtractAddressFromString(Reply.Summary);
                Entry.Service = ParcelService.USPS;
            }
            else if (Reply.Summary.Contains("error"))
            {
                Entry.Status = PackageStatus.NotFound;
                Entry.Location = "Not Found";
            }
            else
            {
                Entry.Status = PackageStatus.Other;
                Entry.Location = "Error";
            }  
        }

        private string ExtractAddressFromString(string source)
        {
            //The return object
            string address = "";

            //the char coordinates inside the stirng of the address
            int[] Range = new int[2];
            Range[0] = 0; Range[1] = 0;

            //reverse loop to traverse array from back to front
            char[] addressArray = source.ToCharArray();
            int sourceSize = addressArray.Length;
            for (int i = sourceSize - 1; i > 0; i--)
            {
                //looking for commas as markers of useful data
                if(addressArray[i] == ',')
                {
                    if(Range[1] == 0)
                    {
                        //The extra 4 characters will contain a space and the state abbreviation
                        Range[1] = i + 4;
                    }
                    else
                    {
                        //the plus ten here is to not capture the comma, year, or spaces
                        Range[0] = i + 10;

                        //escape the for loop
                        i = -1;
                    }
                }
            }

            //calculate length of found address, extract
            int sizeOfActualAddress = Range[1] - Range[0];
            address = source.Substring(Range[0], sizeOfActualAddress);

            return address;
        }


        private void SendRequestToUPSWebService(TrackerData Entry)
        {
            try
            {
                TrackResponse Response = UPS.GetTrackingInfo(Entry.TrackingNumber);
                ParseRawUPSDataIntoList(Entry, Response);
            }
            catch(Exception ex)
            {
                if(ex.Message == "An exception has been raised as a result of client data.")
                {
                    Entry.Location = "ERROR: Bad Credentials";
                    Entry.Status = PackageStatus.Other;
                }
                else
                {
                    Entry.Location = "ERROR";
                    Entry.Status = PackageStatus.Other;
                }
            }
        }

        private void ParseRawUPSDataIntoList(TrackerData Entry, TrackResponse trackResponse)
        {
            Entry.Location = (trackResponse.Shipment[0].Package[0].Activity[0].ActivityLocation.Address.City + ", " + trackResponse.Shipment[0].Package[0].Activity[0].ActivityLocation.Address.StateProvinceCode);
            Entry.Service = ParcelService.UPS;
            switch (trackResponse.Shipment[0].Package[0].Activity[0].Status.Code)
            {
                case "I": Entry.Status = PackageStatus.Shipped; break;
                case "D": Entry.Status = PackageStatus.Delivered; break;
                case "X": Entry.Status = PackageStatus.Other; break;
                case "P": Entry.Status = PackageStatus.PickUp; break;
                default: Entry.Status = PackageStatus.Other; break;
            }
        }


        private bool CheckFedExNumber(string TrackingNumber)
        {
            long number;
            //Determine if the tracking number is all numbers and no letters;
            if (long.TryParse(TrackingNumber, out number))
            {
                //convert tracking number to array of individual digits
                int[] numberArray = (number.ToString().Select(o => Convert.ToInt32(o - 48)).ToArray());

                //Algorythm to generate and check against check digit
                int arrayLength = numberArray.Length;
                int CheckDigit = numberArray[arrayLength - 1];
                int SumOfModifiedDigits = 0;
                int Multiplier = 1;
                for (int i = (arrayLength - 2); i > -1; i--)
                {
                    SumOfModifiedDigits += (Multiplier * numberArray[i]);

                    switch (Multiplier)
                    {
                        case 1: Multiplier = 3; break;
                        case 3: Multiplier = 7; break;
                        case 7: Multiplier = 1; break;
                    }
                }

                //Divide by 11 and get remainder
                int Remainder = SumOfModifiedDigits % 11;
                if (Remainder == 10)
                {
                    Remainder = 0;
                }

                //if Check digit is valid, return true and try fedex web service
                if (Remainder == CheckDigit)
                {
                    return true;
                }

            }
            else
            {
                //DEBUG
                //Console.WriteLine("Tracking Number was not converted into an integer number");
            }

            return false;
        }

        private void FedExNotSupported(TrackerData Entry)
        {
            Entry.Service = ParcelService.FedEx;
            Entry.Status = PackageStatus.Other;
            Entry.Location = "FedEx not supported";
        }

        private void SendRequestToFedExWebService(TrackerData Entry)
        {
            //open webservice, pass in tracking number
            FedExWebService.FedExWebReference.TrackRequest request = FedEx.CreateTrackRequest(Entry.TrackingNumber);
            
            FedExWebService.FedExWebReference.TrackService service = new FedExWebService.FedExWebReference.TrackService();

            try
            {
                // Call the Track web service passing in a TrackRequest and returning a TrackReply
                TrackReply reply = service.track(request);
                if(reply.HighestSeverity != NotificationSeverityType.ERROR && reply.HighestSeverity != NotificationSeverityType.FAILURE)
                {
                    //Parse raw data here
                    ParseFedExRawDataIntoList(Entry, reply);
                }
                else
                {
                    //error handling for blank and incomplete tracking numbers, or invalid requests due to faulty credentials
                    Entry.Location = "ERROR";
                    Entry.Status = PackageStatus.Other;
                }
                
            }
            catch (SoapException e)
            {
                Console.WriteLine(e.Detail.InnerText);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }
        
        private void ParseFedExRawDataIntoList(TrackerData Entry, TrackReply NewData)
        {
            foreach(CompletedTrackDetail completedTrackDetail in NewData.CompletedTrackDetails)
            {
                foreach(TrackDetail trackDetail in completedTrackDetail.TrackDetails)
                {
                    //Check for error, likely from invalid tracking number
                    if(trackDetail.Notification.Severity == NotificationSeverityType.ERROR)
                    {
                        Entry.Location = "INVALID TRACKING NUMBER";
                        Entry.Status = PackageStatus.NotFound;
                    }
                    else
                    {
                        //check for error-less state of no record found by web service
                        if (trackDetail.StatusDetail != null)
                        {
                            //Input city state location as single string
                            Entry.Location = trackDetail.StatusDetail.Location.City + ", " +
                                trackDetail.StatusDetail.Location.StateOrProvinceCode;
                        }
                        else
                        {
                            Entry.Location = "NO CURRENT LOCATION FOUND";
                        }
                        

                        Entry.Service = ParcelService.FedEx;
                        //check for error-less state of no record found by web service
                        if(trackDetail.StatusDetail != null)
                        {
                            //a small sample of the package status codes, mapped to PackageStatus ENUM
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
                        else
                        {
                            Entry.Status = PackageStatus.NotFound;
                        }
                    }  
                }
            }
        }

    }
}
