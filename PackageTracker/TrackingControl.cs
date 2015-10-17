using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services;
using FedExWebService.FedExWebReference;
using FedExWebService;
using System.Web.Services.Protocols;
using TrackWSSample;
using UPSWebService.UPSWebReference;
using System.ServiceModel;

namespace PackageTracker
{
    class TrackingControl
    {
        
        public void UpdateTrackingInformation(List<TrackerData> TrackingData)
        {
            //Take list of package data, parse out tracking numbers, run each through web service
            foreach(TrackerData Entry in TrackingData)
            {
                //Check for reasonable length tracking number
                int TNlength = Entry.TrackingNumber.Length;
                if(TNlength > 6 && TNlength < 23)
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
                        SendRequestToFedExWebService(Entry);
                    }

                        //TODO: USPS
                        //Create and add XML based service

                    else
                    {
                        Entry.Location = "Invalid Tracking Number";
                        Entry.Service = ParcelService.None;
                        Entry.Status = PackageStatus.NotFound;
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

        private bool CheckFedExNumber(string TrackingNumber)
        {
            long number;
            //Determine if the tracking number is all numbers and no letters;
            if(long.TryParse(TrackingNumber, out number))
            {
                //convert tracking number to array of individual digits
                int[] numberArray = (number.ToString().Select(o => Convert.ToInt32(o - 48)).ToArray());

                //Algorythm to generate and check against check digit
                int arrayLength = numberArray.Length;
                int CheckDigit = numberArray[arrayLength - 1];
                int SumOfModifiedDigits = 0;
                int Multiplier = 1;
                for(int i = (arrayLength - 2); i > -1; i--)
                {
                    SumOfModifiedDigits += (Multiplier * numberArray[i]);

                    switch(Multiplier)
                    {
                        case 1: Multiplier = 3; break;
                        case 3: Multiplier = 7; break;
                        case 7: Multiplier = 1; break;
                    }
                }

                //Divide by 11 and get remainder
                int Remainder = SumOfModifiedDigits % 11;
                if(Remainder == 10)
                {
                    Remainder = 0;
                }

                //debug
                Console.WriteLine("Remainder is {0}", Remainder);
                Console.WriteLine("CheckDigit is {0}", CheckDigit);
                
                //if Check digit is valid, return true and try fedex web service
                if(Remainder == CheckDigit)
                {
                    return true;
                }

            }
            else
            {
                Console.WriteLine("Tracking Number was not conveted into an integer number");
            }

            return false;
        }


        //Largely UPS's code
        private void SendRequestToUPSWebService(TrackerData Entry)
        {
            try
            {
                UPSWebService.UPSWebReference.TrackService track = new UPSWebService.UPSWebReference.TrackService();
                UPSWebService.UPSWebReference.TrackRequest tr = new UPSWebService.UPSWebReference.TrackRequest();
                UPSSecurity upss = new UPSSecurity();
                UPSSecurityServiceAccessToken upssSvcAccessToken = new UPSSecurityServiceAccessToken();
                upssSvcAccessToken.AccessLicenseNumber = "4CFB51344FD8E476";
                upss.ServiceAccessToken = upssSvcAccessToken;
                UPSSecurityUsernameToken upssUsrNameToken = new UPSSecurityUsernameToken();
                upssUsrNameToken.Username = "TastEPlasma";
                upssUsrNameToken.Password = "Firebolt5";
                upss.UsernameToken = upssUsrNameToken;
                track.UPSSecurityValue = upss;
                RequestType request = new RequestType();
                String[] requestOption = { "15" };
                request.RequestOption = requestOption;
                tr.Request = request;
                tr.InquiryNumber = Entry.TrackingNumber;
                System.Net.ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();
                TrackResponse trackResponse = track.ProcessTrack(tr);

                //Check for error state, if not begin editing entry
                if (trackResponse.Response.ResponseStatus.Code == "1")
                {
                    ParseRawUPSDataIntoList(Entry, trackResponse);
                }
                else
                {
                    Entry.Location = "UPS ERROR";
                    Entry.Service = ParcelService.UPS;
                    Console.WriteLine("UPS ERROR");
                }
                

                //Debug
                Console.WriteLine("The transaction was a " + trackResponse.Response.ResponseStatus.Description);
                Console.WriteLine("Shipment Service " + trackResponse.Shipment[0].Service.Description);
                Console.WriteLine("Location is " + trackResponse.Shipment[0].Package[0].Activity[0].ActivityLocation.Address.City);
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {
                if (ex.Detail.LastChild.InnerText == "Hard151018Invalid tracking number")
                {
                    Entry.Location = "INVALID TRACKING NUMBER";
                    Entry.Service = ParcelService.UPS;
                }

                //Debug
                Console.WriteLine("");
                Console.WriteLine("---------Track Web Service returns error----------------");
                Console.WriteLine("---------\"Hard\" is user error \"Transient\" is system error----------------");
                Console.WriteLine("SoapException Message= " + ex.Message);
                Console.WriteLine("");
                Console.WriteLine("SoapException Category:Code:Message= " + ex.Detail.LastChild.InnerText);
                Console.WriteLine("");
                Console.WriteLine("SoapException XML String for all= " + ex.Detail.LastChild.OuterXml);
                Console.WriteLine("");
                Console.WriteLine("SoapException StackTrace= " + ex.StackTrace);
                Console.WriteLine("-------------------------");
                Console.WriteLine("");
            }
            catch (System.ServiceModel.CommunicationException ex)
            {
                //Debug
                Console.WriteLine("");
                Console.WriteLine("--------------------");
                Console.WriteLine("CommunicationException= " + ex.Message);
                Console.WriteLine("CommunicationException-StackTrace= " + ex.StackTrace);
                Console.WriteLine("-------------------------");
                Console.WriteLine("");

            }
            catch (Exception ex)
            {
                //Debug
                Console.WriteLine("");
                Console.WriteLine("-------------------------");
                Console.WriteLine(" General Exception= " + ex.Message);
                Console.WriteLine(" General Exception-StackTrace= " + ex.StackTrace);
                Console.WriteLine("-------------------------");

            }
        }

        //Process raw UPS data, update entry via list
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


        //Send request to FEDEX webservices, receive raw data
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
                    //For debugging purposes
                    //Console.WriteLine(reply.HighestSeverity);
                    //FedEx.ShowTrackReply(reply);

                    //Parse raw data here
                    ParseFedExRawDataIntoList(Entry, reply);
                }
                else
                {
                    //error handling for blank or incomplete tracking numbers
                    Entry.Location = "ERROR";
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
        
        //Process raw FEDEX data and update list
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
