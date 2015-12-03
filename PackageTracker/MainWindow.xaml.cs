using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;

namespace PackageTracker
{
    public partial class MainWindow : Window
    {
        //In this case, the context is left open per session since it is accessing a local DB
        private TrackerContext _context = new TrackerContext();

        //Primary internal inteface to 3rd party web service providers
        private TrackingControl _control = new TrackingControl();

        //Internal timer for Auto-Updates CheckBox
        private System.Windows.Threading.DispatcherTimer UpdateTimer = new DispatcherTimer();
        private TimeSpan DelayTime = new TimeSpan(1, 0, 0);


        //Initializers
        public string DelayInHours
        {
            get { return DelayTime.Hours.ToString(); }
            set { /*Do Nothing*/ }
        }

        public string TimerEnabled
        {
            get { return UpdateTimer.IsEnabled == true ? "ON" : "OFF"; }
            set { /* Do Nothing */ }
        }

        public MainWindow()
        {
            InitializeComponent();
            InitializeTimer();
        }

        private void InitializeTimer()
        {
            UpdateTimer.Tick += new EventHandler(UpdateDatabase);
            //set interval to 1 hour
            UpdateTimer.Interval = DelayTime;
            UpdateTimer.IsEnabled = false;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            this._context.Dispose();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            System.Windows.Data.CollectionViewSource trackerDataViewSource =
                ((System.Windows.Data.CollectionViewSource)(this.FindResource("trackerDataViewSource")));
            // Load data by setting the CollectionViewSource.Source property:
            // trackerDataViewSource.Source = [generic data source]

            // *This comment block from the msdn tutorial on WPF/EF Data Binding*
            // Load is an extension method on IQueryable,  
            // defined in the System.Data.Entity namespace. 
            // This method enumerates the results of the query,  
            // similar to ToList but without creating a list. 
            // When used with Linq to Entities this method  
            // creates entity objects and adds them to the context.
            bool ContinueOperation = true;
            try
            {
                _context.Packages.Load();
                _context.Credentials.Load();
            }
            catch(Exception exception)
            {
                //Quit Popup
                ErrorMessageAndQuit(exception);

                ContinueOperation = false;
            }
            
            if(ContinueOperation == true)
            {
                //This function is to insure that the credential entry is present,
                //and create a default entry if one doesnt exist
                CheckForCredentialExistance();

                // After the data is loaded call the DbSet<T>.Local property  
                // to use the DbSet<T> as a binding source. 
                trackerDataViewSource.Source = _context.Packages.Local;
            }
        }

        private void ErrorMessageAndQuit(Exception exception)
        {
            Error_Popup.IsOpen = true;

            ErrorMessage_TextBlock.Text = exception.Message + " Inner exception: " + exception.InnerException;
        }


        //Database Methods
        private void CheckForCredentialExistance()
        {
            var CurrentDBList = _context.Credentials.ToList();
            
            if(CurrentDBList.Count() == 0)
            {
                CredentialData NewEntry = new CredentialData();
                NewEntry.FedExCredentials = new FedExCredentialsData();
                NewEntry.UPSCredentials = new UPSCredentialsData();
                NewEntry.POSTALCredentials = new USPSCredentialsData();

                _context.Credentials.Add(NewEntry);
                _context.SaveChanges();
            }
            else if(CurrentDBList.Count() > 1)
            {
                Console.WriteLine("ERROR: TOO MANY CREDENTIAL ENTRIES!!");
            }
        }

        private void UpdateDatabase(object sender, EventArgs e)
        {
            //passing through this method was necessary due to the difference in args
            //It also allows UpdateDatabase() to be called from elsewhere in the program
            UpdateDatabase();
        }

        private void UpdateDatabase()
        {
            //Need something to show user button was pressed
            Progress.Visibility = System.Windows.Visibility.Visible;

            //Lock down datagrid while updating
            trackerDataDataGrid.IsReadOnly = true;

            //Add in new tracking number entries, delete entries marked for deletion by user
            UpdateLocalDBWithUserInput();

            //Spawn a background thread for DB work so that the GUI thread can continue to update
            BackgroundWorker DBUpdater = new BackgroundWorker();

            //allow worker to report progress during work
            DBUpdater.WorkerReportsProgress = true;

            //what to do in background thread
            DBUpdater.DoWork += new DoWorkEventHandler(delegate(object o, DoWorkEventArgs args)
            {
                //Checks tracking numbers via web service, updates local DB
                UpdateDBFromWebServices();

            });

            //what to do when worker is done
            DBUpdater.RunWorkerCompleted += new RunWorkerCompletedEventHandler(delegate(object o, RunWorkerCompletedEventArgs args)
            {
                //Refresh view so changes to DB are confirmed and seen
                this.trackerDataDataGrid.Items.Refresh();

                //Remove progress bar after actions are completed
                //Might want to delay this action by 500ms or so, to insure the progress bar blips up
                ProgressBarVisibilityDelay();

                //Release datagrid back to editable state
                trackerDataDataGrid.IsReadOnly = false;
            });

            //start worker tasks
            DBUpdater.RunWorkerAsync();
        }

        private void UpdateLocalDBWithUserInput()
        {
            //Create list from DB
            var CurrentDBList = _context.Packages.ToList();

            //Delete any entry via entity state whose box is checked
            foreach (TrackerData package in CurrentDBList)
            {
                if (package.DeleteMe == true)
                {
                    _context.Entry(package).State = EntityState.Deleted;
                }
            }

            //save add/delete changes
            _context.SaveChanges();
        }

        private void StoreAndUpdateCredentials(List<string> NewCredentials, ParcelService Service)
        {
            var CurrentDBList = _context.Credentials.ToList();

            foreach(CredentialData credentials in CurrentDBList)
            {
                if(Service == ParcelService.FedEx)
                {
                    credentials.FedExCredentials.UserKey = B64Encode(NewCredentials[0]);
                    credentials.FedExCredentials.UserPassword = B64Encode(NewCredentials[1]);
                    credentials.FedExCredentials.AccountNumber = B64Encode(NewCredentials[2]);
                    credentials.FedExCredentials.MeterNumber = B64Encode(NewCredentials[3]);
                }

                if(Service == ParcelService.UPS)
                {
                    credentials.UPSCredentials.username = B64Encode(NewCredentials[0]);
                    credentials.UPSCredentials.password = B64Encode(NewCredentials[1]);
                    credentials.UPSCredentials.accessLicenseNumber = B64Encode(NewCredentials[2]);
                }

                if (Service == ParcelService.USPS)
                {
                    credentials.POSTALCredentials._userid = B64Encode(NewCredentials[0]);
                }
            }

            _context.SaveChanges();
        }

        private void RetrieveCredentialsOnLoad()
        {
            var CurrentDBList = _context.Credentials.ToList();

            foreach (CredentialData credentials in CurrentDBList)
            {
                List<string> FedExCreds = new List<string>();
                FedExCreds.Add(B64Decode(credentials.FedExCredentials.UserKey));
                FedExCreds.Add(B64Decode(credentials.FedExCredentials.UserPassword));
                FedExCreds.Add(B64Decode(credentials.FedExCredentials.AccountNumber));
                FedExCreds.Add(B64Decode(credentials.FedExCredentials.MeterNumber));

                _control.UpdateCredentialInformation(FedExCreds, ParcelService.FedEx);


                List<string> UPSCreds = new List<string>();
                UPSCreds.Add(B64Decode(credentials.UPSCredentials.username));
                UPSCreds.Add(B64Decode(credentials.UPSCredentials.password));
                UPSCreds.Add(B64Decode(credentials.UPSCredentials.accessLicenseNumber));

                _control.UpdateCredentialInformation(UPSCreds, ParcelService.UPS);


                List<string> POSTALCreds = new List<string>();
                POSTALCreds.Add(B64Decode(credentials.POSTALCredentials._userid));

                _control.UpdateCredentialInformation(POSTALCreds, ParcelService.USPS);

            }
        }

        public static string B64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string B64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private void ResetCredentialsInDBToDefaults(ParcelService Service)
        {
            var CurrentDBList = _context.Credentials.ToList();

            foreach (CredentialData credentials in CurrentDBList)
            {
                if (Service == ParcelService.FedEx)
                {
                    var DefaultCredentials = _control.RetrieveDefaultCredentials(Service);

                    credentials.FedExCredentials.UserKey = DefaultCredentials[0];
                    credentials.FedExCredentials.UserPassword = DefaultCredentials[1];
                    credentials.FedExCredentials.AccountNumber = DefaultCredentials[2];
                    credentials.FedExCredentials.MeterNumber = DefaultCredentials[3];
                }

                if (Service == ParcelService.UPS)
                {
                    var DefaultCredentials = _control.RetrieveDefaultCredentials(Service);

                    credentials.UPSCredentials.username = DefaultCredentials[0];
                    credentials.UPSCredentials.password = DefaultCredentials[1];
                    credentials.UPSCredentials.accessLicenseNumber = DefaultCredentials[2];
                }

                if (Service == ParcelService.USPS)
                {
                    var DefaultCredentials = _control.RetrieveDefaultCredentials(Service);

                    credentials.POSTALCredentials._userid = DefaultCredentials[0];
                }
            }

            _context.SaveChanges();
        }

        private void UpdateDBFromWebServices()
        {
            //Create list from DB
            var CurrentDBList = _context.Packages.ToList();

            //Pass in Tracking List to Tracking Control for Web Service updating
            _control.UpdateTrackingInformation(CurrentDBList);

            //Commit changes to DB
            _context.SaveChanges();
        }


        //GUI Methods
        private void Update_Click(object sender, RoutedEventArgs e)
        {
            UpdateDatabase();
        }

        private async void ProgressBarVisibilityDelay()
        {
            //prevent hiding the progress bar for half a second,
            //so user knows that button press was registered
            //Even if no events happen
            await Task.Delay(500);
            Progress.Visibility = System.Windows.Visibility.Hidden;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if(HourlyUpdatesBox.IsChecked == true)
            {
                UpdateTimer.IsEnabled = true;
                displayUpdateToggle.Text = "ON";
            }
            else
            {
                UpdateTimer.IsEnabled = false;
                displayUpdateToggle.Text = "OFF";
            }
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void Delay_Click(object sender, RoutedEventArgs e)
        {
            DelayAdjuster.IsOpen = true;
        }

        private void delayButtonClose_Click(object sender, RoutedEventArgs e)
        {
            delayLength.Text = delaySlider.Value.ToString();
            TimeSpan tempSpan = new TimeSpan(Convert.ToInt32(delaySlider.Value), 0, 0);
            DelayTime = tempSpan;
            UpdateTimer.Interval = DelayTime;
            DelayAdjuster.IsOpen = false;
            Console.WriteLine(delaySlider.Value);
        }

        private void OpenAboutPopup_Click(object sender, RoutedEventArgs e)
        {
            About_Popup.IsOpen = true;
        }

        private void CloseAboutPopup_Click(object sender, RoutedEventArgs e)
        {
            About_Popup.IsOpen = false;
        }

        private void CloseUsageMenu_Click(object sender, RoutedEventArgs e)
        {
            Usage_Popup.IsOpen = false;
        }


        //FEDEX
        private void OpenFedExCredentialsMenu_Click(object sender, RoutedEventArgs e)
        {
            FedExCredentialEntry_PopUp.IsOpen = true;
        }

        private void CloseFedExCredentialsMenu_Click(object sender, RoutedEventArgs e)
        {
            //Clear entry fields
            FedExUserKEY.Text = "";
            FedExUserPASSWORD.Text = "";
            FedExUserMETERNUMBER.Text = "";
            FedExUserACCOUNTNUMBER.Text = "";

            FedExCredentialEntry_PopUp.IsOpen = false;
        }

        private void UpdateFedExAccount_Click(object sender, RoutedEventArgs e)
        {
            //show progress bar
            Progress.Visibility = System.Windows.Visibility.Visible;

            List<string> UpdatedInfo = new List<string>();
            if(UpdateFedExToDefaults_CheckBox.IsChecked == false)
            {
                //Create list of user input values
                UpdatedInfo.Add(FedExUserKEY.Text);
                UpdatedInfo.Add(FedExUserPASSWORD.Text);
                UpdatedInfo.Add(FedExUserACCOUNTNUMBER.Text);
                UpdatedInfo.Add(FedExUserMETERNUMBER.Text);

                //Send new information to FedExManager via TrackingControl
                _control.UpdateCredentialInformation(UpdatedInfo, ParcelService.FedEx);

                //Update the database with the new credentials
                StoreAndUpdateCredentials(UpdatedInfo, ParcelService.FedEx);

                //Clear entry fields
                FedExUserKEY.Text = "";
                FedExUserPASSWORD.Text = "";
                FedExUserMETERNUMBER.Text = "";
                FedExUserACCOUNTNUMBER.Text = "";

                FedExCredentialEntry_PopUp.IsOpen = false; 
            }
            else
            {
                //Having this value in the first field of the array tells the FedExManager to reset the credentials
                UpdatedInfo.Add("ResetToDefaults");

                //Send new information to FedExManager via TrackingControl
                _control.UpdateCredentialInformation(UpdatedInfo, ParcelService.FedEx);

                //Blank fields on reset to defaults, and uncheck box
                FedExUserKEY.Text = "";
                FedExUserPASSWORD.Text = "";
                FedExUserMETERNUMBER.Text = "";
                FedExUserACCOUNTNUMBER.Text = "";
                UpdateFedExToDefaults_CheckBox.IsChecked = false;

                //Update DB with new information
                ResetCredentialsInDBToDefaults(ParcelService.FedEx);

                FedExCredentialEntry_PopUp.IsOpen = false;
            }

            ProgressBarVisibilityDelay();
        }

        //UPS
        private void OpenUPSCredentialsMenu_Click(object sender, RoutedEventArgs e)
        {
            UPSCredentialEntry_PopUp.IsOpen = true;
        }
        
        private void CloseUPSCredentialsMenu_Click(object sender, RoutedEventArgs e)
        {
            //Clear entry fields
            UPSUserLicenseNUMBER.Text = "";
            UPSUserNAME.Text = "";
            UPSUserPASSWORD.Text = "";

            UPSCredentialEntry_PopUp.IsOpen = false;
        }

        private void UpdateUPSAccount_Click(object sender, RoutedEventArgs e)
        {
            //show progress bar
            Progress.Visibility = System.Windows.Visibility.Visible;

            List<string> UpdatedInfo = new List<string>();
            if (UpdateUPSToDefaults_CheckBox.IsChecked == false)
            {
                //Create list of user input values
                UpdatedInfo.Add(UPSUserNAME.Text);
                UpdatedInfo.Add(UPSUserPASSWORD.Text);
                UpdatedInfo.Add(UPSUserLicenseNUMBER.Text);

                //Send new information to UPSManager via TrackingControl
                _control.UpdateCredentialInformation(UpdatedInfo, ParcelService.UPS);

                //Update the database with the new credentials
                StoreAndUpdateCredentials(UpdatedInfo, ParcelService.UPS);

                //Clear entry fields
                UPSUserLicenseNUMBER.Text = "";
                UPSUserNAME.Text = "";
                UPSUserPASSWORD.Text = "";

                UPSCredentialEntry_PopUp.IsOpen = false;
            }
            else
            {
                //Having this value in the first field of the array tells the UPSManager to reset the credentials
                UpdatedInfo.Add("ResetToDefaults");

                //Send new information to UPSManager via TrackingControl
                _control.UpdateCredentialInformation(UpdatedInfo, ParcelService.UPS);

                //Blank fields on reset to defaults, uncheck box
                UPSUserLicenseNUMBER.Text = "";
                UPSUserNAME.Text = "";
                UPSUserPASSWORD.Text = "";
                UpdateUPSToDefaults_CheckBox.IsChecked = false;

                //Update DB with new information
                ResetCredentialsInDBToDefaults(ParcelService.UPS);

                UPSCredentialEntry_PopUp.IsOpen = false;
            }

            ProgressBarVisibilityDelay();
        }

        //USPS
        private void OpenUSPSCredentialsMenu_Click(object sender, RoutedEventArgs e)
        {
            USPSCredentialEntry_PopUp.IsOpen = true;
        }

        private void CloseUSPSCredentialsMenu_Click(object sender, RoutedEventArgs e)
        {
            //blank entry fields
            USPSUserID.Text = "";

            USPSCredentialEntry_PopUp.IsOpen = false; 
        }

        private void UpdateUSPSAccount_Click(object sender, RoutedEventArgs e)
        {
            //show progress bar
            Progress.Visibility = System.Windows.Visibility.Visible;

            List<string> UpdatedInfo = new List<string>();
            if (UpdateUSPSToDefaults_CheckBox.IsChecked == false)
            {
                //Create list of user input values
                UpdatedInfo.Add(USPSUserID.Text);

                //Send new information to UPSManager via TrackingControl
                _control.UpdateCredentialInformation(UpdatedInfo, ParcelService.USPS);

                //Update the database with the new credentials
                StoreAndUpdateCredentials(UpdatedInfo, ParcelService.USPS);

                //blank entry fields
                USPSUserID.Text = "";

                USPSCredentialEntry_PopUp.IsOpen = false;  
            }
            else
            {
                //Having this value in the first field of the array tells the UPSManager to reset the credentials
                UpdatedInfo.Add("ResetToDefaults");

                //Send new information to UPSManager via TrackingControl
                _control.UpdateCredentialInformation(UpdatedInfo, ParcelService.USPS);

                //Blank fields on reset to defaults, uncheck box
                USPSUserID.Text = "";
                UpdateUSPSToDefaults_CheckBox.IsChecked = true;

                //Update DB with new information
                ResetCredentialsInDBToDefaults(ParcelService.USPS);

                USPSCredentialEntry_PopUp.IsOpen = false;
            }

            ProgressBarVisibilityDelay();
            
        }

    }
}
