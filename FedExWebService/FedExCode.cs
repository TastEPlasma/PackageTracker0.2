namespace FedExWebService
{
    using FedExWebService.FedExWebReference;
    using System;

    public class FedExManager
    {
        public FedExManager()
        {
            this.UserKey = "1CK3fnM8LhfQWteN";
            this.UserPassword = "vh6rTNPVog2PAXKRh44SiJznk";
            this.AccountNumber = "510087186";
            this.MeterNumber = "118691686";
            this.TransactionID = "TRACK";
        }

        public string UserKey { get; set; }
        public string UserPassword { get; set; }
        public string AccountNumber { get; set; }
        public string MeterNumber { get; set; }
        public string TransactionID { get; set; }

        public TrackRequest CreateTrackRequest(string TrackNumber)
        {
            //The following code is almost entirely based on WebAPI example
            // Build the TrackRequest
            TrackRequest request = new TrackRequest();
            //
            request.WebAuthenticationDetail = new WebAuthenticationDetail();
            request.WebAuthenticationDetail.UserCredential = new WebAuthenticationCredential();
            request.WebAuthenticationDetail.UserCredential.Key = UserKey; // Replace "XXX" with the Key
            request.WebAuthenticationDetail.UserCredential.Password = UserPassword; // Replace "XXX" with the Password
            request.WebAuthenticationDetail.ParentCredential = new WebAuthenticationCredential();
            request.WebAuthenticationDetail.ParentCredential.Key = "XXX"; // Replace "XXX" with the Key
            request.WebAuthenticationDetail.ParentCredential.Password = "XXX"; // Replace "XXX"
            request.ClientDetail = new ClientDetail();
            request.ClientDetail.AccountNumber = AccountNumber; // Replace "XXX" with the client's account number
            request.ClientDetail.MeterNumber = MeterNumber; // Replace "XXX" with the client's meter number
            request.TransactionDetail = new TransactionDetail();
            request.TransactionDetail.CustomerTransactionId = TransactionID;  //This is a reference field for the customer.  Any value can be used and will be provided in the response.
            //
            request.Version = new VersionId();
            //
            // Tracking information
            request.SelectionDetails = new TrackSelectionDetail[1] { new TrackSelectionDetail() };
            request.SelectionDetails[0].PackageIdentifier = new TrackPackageIdentifier();
            request.SelectionDetails[0].PackageIdentifier.Value = TrackNumber; // Replace "XXX" with tracking number or door tag
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

        public void ResetCredentialsToDefaults()
        {
            UserKey = "1CK3fnM8LhfQWteN";
            UserPassword = "vh6rTNPVog2PAXKRh44SiJznk";
            AccountNumber = "510087186";
            MeterNumber = "118691686";
            TransactionID = "TRACK";
        }
    }
}