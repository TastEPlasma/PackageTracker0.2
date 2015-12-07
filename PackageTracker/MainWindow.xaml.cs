using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace PackageTracker
{
    public partial class MainWindow : Window
    {
        //In this case, the context is left open per session since it is accessing a local DB
        private TrackerContext DBContext = new TrackerContext();

        //Primary internal inteface to 3rd party web service providers
        private TrackingControl TrackingServiceControl = new TrackingControl();

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
            this.DBContext.Dispose();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.Windows.Data.CollectionViewSource trackerDataViewSource =
                ((System.Windows.Data.CollectionViewSource)(this.FindResource("trackerDataViewSource")));
            
            //try/catch to handle exceptions on database errors, usually DB not found variants
            bool ContinueOperation = true;
            try
            {
                DBContext.Packages.Load();
                DBContext.Credentials.Load();
            }
            catch (Exception exception)
            {
                LockDownMenus();

                ErrorMessageAndQuit(exception);

                ContinueOperation = false;
            }

            if (ContinueOperation == true)
            {
                //This function is to insure that the credential entry is present,
                //and create a default entry if one doesnt exist
                CheckForCredentialExistance();

                // After the data is loaded call the DbSet<T>.Local property
                // to use the DbSet<T> as a binding source.
                trackerDataViewSource.Source = DBContext.Packages.Local;
            }
        }

        private void ErrorMessageAndQuit(Exception exception)
        {
            Error_Popup.IsOpen = true;

            ErrorMessage_TextBlock.Text = exception.Message + " Inner exception: " + exception.InnerException;
        }

        private void LockDownMenus()
        {
            trackerDataDataGrid.IsEnabled = false;
            File_Menu.IsEnabled = false;
            Options_Menu.IsEnabled = false;
            Info_Menu.IsEnabled = false;
            Update_Button.IsEnabled = false;
        }

        //Database Methods
        private void CheckForCredentialExistance()
        {
            var CurrentDBList = DBContext.Credentials.ToList();

            if (CurrentDBList.Count() == 0)
            {
                CredentialData NewEntry = new CredentialData();
                NewEntry.FedExCredentials = new FedExCredentialsData();
                NewEntry.UPSCredentials = new UPSCredentialsData();
                NewEntry.POSTALCredentials = new USPSCredentialsData();

                DBContext.Credentials.Add(NewEntry);
                DBContext.SaveChanges();
            }
            else if (CurrentDBList.Count() > 1)
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
            var CurrentDBList = DBContext.Packages.ToList();

            //Delete any entry via entity state whose box is checked
            foreach (TrackerData package in CurrentDBList)
            {
                if (package.DeleteMe == true)
                {
                    DBContext.Entry(package).State = EntityState.Deleted;
                }
            }

            //save add/delete changes
            DBContext.SaveChanges();
        }

        private void StoreAndUpdateCredentials(FedExCredentialsData NewFedExData)
        {
            var CurrentDBList = DBContext.Credentials.ToList();

            foreach (CredentialData credentials in CurrentDBList)
            {
                credentials.FedExCredentials.UserKey = B64Encode(NewFedExData.UserKey);
                credentials.FedExCredentials.UserPassword = B64Encode(NewFedExData.UserPassword);
                credentials.FedExCredentials.AccountNumber = B64Encode(NewFedExData.AccountNumber);
                credentials.FedExCredentials.MeterNumber = B64Encode(NewFedExData.MeterNumber);
            }

            DBContext.SaveChanges();
        }

        private void StoreAndUpdateCredentials(UPSCredentialsData NewUPSData)
        {
            var CurrentDBList = DBContext.Credentials.ToList();

            foreach (CredentialData credentials in CurrentDBList)
            {
                credentials.UPSCredentials.Username = B64Encode(NewUPSData.Username);
                credentials.UPSCredentials.Password = B64Encode(NewUPSData.Password);
                credentials.UPSCredentials.AccessLicenseNumber = B64Encode(NewUPSData.AccessLicenseNumber);
            }

            DBContext.SaveChanges();
        }

        private void StoreAndUpdateCredentials(USPSCredentialsData NewUSPSData)
        {
            var CurrentDBList = DBContext.Credentials.ToList();

            foreach (CredentialData credentials in CurrentDBList)
            {
                credentials.POSTALCredentials.UserID = B64Encode(NewUSPSData.UserID);
            }

            DBContext.SaveChanges();
        }

        private void RetrieveCredentialsOnLoad()
        {
            var CurrentDBList = DBContext.Credentials.ToList();

            foreach (CredentialData credentials in CurrentDBList)
            {
                FedExCredentialsData NewFedExData = new FedExCredentialsData();
                NewFedExData.UserKey = (B64Decode(credentials.FedExCredentials.UserKey));
                NewFedExData.UserPassword = (B64Decode(credentials.FedExCredentials.UserPassword));
                NewFedExData.AccountNumber = (B64Decode(credentials.FedExCredentials.AccountNumber));
                NewFedExData.MeterNumber = (B64Decode(credentials.FedExCredentials.MeterNumber));

                TrackingServiceControl.UpdateCredentialInformation(NewFedExData);

                UPSCredentialsData NewUPSData = new UPSCredentialsData();
                NewUPSData.Username = (B64Decode(credentials.UPSCredentials.Username));
                NewUPSData.Password = (B64Decode(credentials.UPSCredentials.Password));
                NewUPSData.AccessLicenseNumber = (B64Decode(credentials.UPSCredentials.AccessLicenseNumber));

                TrackingServiceControl.UpdateCredentialInformation(NewUPSData);

                USPSCredentialsData NewUSPSData = new USPSCredentialsData();
                NewUSPSData.UserID = (B64Decode(credentials.POSTALCredentials.UserID));

                TrackingServiceControl.UpdateCredentialInformation(NewUSPSData);
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
            var CurrentDBList = DBContext.Credentials.ToList();

            foreach (CredentialData credentials in CurrentDBList)
            {
                if (Service == ParcelService.FedEx)
                {
                    var DefaultCredentials = new FedExCredentialsData();

                    credentials.FedExCredentials.UserKey = DefaultCredentials.UserKey;
                    credentials.FedExCredentials.UserPassword = DefaultCredentials.UserPassword;
                    credentials.FedExCredentials.AccountNumber = DefaultCredentials.AccountNumber;
                    credentials.FedExCredentials.MeterNumber = DefaultCredentials.MeterNumber;
                }

                if (Service == ParcelService.UPS)
                {
                    var DefaultCredentials = new UPSCredentialsData();

                    credentials.UPSCredentials.Username = DefaultCredentials.Username;
                    credentials.UPSCredentials.Password = DefaultCredentials.Password;
                    credentials.UPSCredentials.AccessLicenseNumber = DefaultCredentials.AccessLicenseNumber;
                }

                if (Service == ParcelService.USPS)
                {
                    var DefaultCredentials = new USPSCredentialsData();

                    credentials.POSTALCredentials.UserID = DefaultCredentials.UserID;
                }
            }

            DBContext.SaveChanges();
        }

        private void UpdateDBFromWebServices()
        {
            //Create list from DB
            var CurrentDBList = DBContext.Packages.ToList();

            //Pass in Tracking List to Tracking Control for Web Service updating
            TrackingServiceControl.UpdateTrackingInformation(CurrentDBList);

            //Commit changes to DB
            DBContext.SaveChanges();
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
            if (HourlyUpdatesBox.IsChecked == true)
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

            if (UpdateFedExToDefaults_CheckBox.IsChecked == false)
            {
                var NewFedExData = new FedExCredentialsData();

                //Populate object with updated values
                NewFedExData.UserKey = FedExUserKEY.Text;
                NewFedExData.UserPassword = FedExUserPASSWORD.Text;
                NewFedExData.AccountNumber = FedExUserACCOUNTNUMBER.Text;
                NewFedExData.MeterNumber = FedExUserMETERNUMBER.Text;

                //Send new information to FedExManager via TrackingControl
                TrackingServiceControl.UpdateCredentialInformation(NewFedExData);

                //Update the database with the new credentials
                StoreAndUpdateCredentials(NewFedExData);

                //Clear entry fields
                FedExUserKEY.Text = "";
                FedExUserPASSWORD.Text = "";
                FedExUserMETERNUMBER.Text = "";
                FedExUserACCOUNTNUMBER.Text = "";
            }
            else
            {
                //Trigger a reset credentials to default in memory
                TrackingServiceControl.ResetCredentialsToDefaults(ParcelService.FedEx);

                //Trigger a reset to defaults in database
                ResetCredentialsInDBToDefaults(ParcelService.FedEx);

                //Blank fields on reset to defaults, and uncheck box
                FedExUserKEY.Text = "";
                FedExUserPASSWORD.Text = "";
                FedExUserMETERNUMBER.Text = "";
                FedExUserACCOUNTNUMBER.Text = "";
                UpdateFedExToDefaults_CheckBox.IsChecked = false;
            }

            FedExCredentialEntry_PopUp.IsOpen = false;
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

            if (UpdateUPSToDefaults_CheckBox.IsChecked == false)
            {
                var NewUPSData = new UPSCredentialsData();

                //Populate object with updated values
                NewUPSData.Username = UPSUserNAME.Text;
                NewUPSData.Password = UPSUserPASSWORD.Text;
                NewUPSData.AccessLicenseNumber = UPSUserLicenseNUMBER.Text;

                //Send new information to UPSManager via TrackingControl
                TrackingServiceControl.UpdateCredentialInformation(NewUPSData);

                //Update the database with the new credentials
                StoreAndUpdateCredentials(NewUPSData);

                //Clear entry fields
                UPSUserLicenseNUMBER.Text = "";
                UPSUserNAME.Text = "";
                UPSUserPASSWORD.Text = "";
            }
            else
            {
                //Trigger reset to defaults in memory
                TrackingServiceControl.ResetCredentialsToDefaults(ParcelService.UPS);

                //Trigger reset to defaults in database
                ResetCredentialsInDBToDefaults(ParcelService.UPS);

                //Blank fields on reset to defaults, uncheck box
                UPSUserLicenseNUMBER.Text = "";
                UPSUserNAME.Text = "";
                UPSUserPASSWORD.Text = "";
                UpdateUPSToDefaults_CheckBox.IsChecked = false;
            }

            UPSCredentialEntry_PopUp.IsOpen = false;
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

            if (UpdateUSPSToDefaults_CheckBox.IsChecked == false)
            {
                var NewUSPSData = new USPSCredentialsData();

                //Populate object with updated values
                NewUSPSData.UserID = USPSUserID.Text;

                //Send new information to UPSManager via TrackingControl
                TrackingServiceControl.UpdateCredentialInformation(NewUSPSData);

                //Update the database with the new credentials
                StoreAndUpdateCredentials(NewUSPSData);

                //blank entry fields
                USPSUserID.Text = "";
            }
            else
            {
                //Trigger a reset to defaults in memory
                TrackingServiceControl.ResetCredentialsToDefaults(ParcelService.USPS);

                //Trigger a reset to defaults in database
                ResetCredentialsInDBToDefaults(ParcelService.USPS);

                //Blank fields on reset to defaults, uncheck box
                USPSUserID.Text = "";
                UpdateUSPSToDefaults_CheckBox.IsChecked = true;
            }

            USPSCredentialEntry_PopUp.IsOpen = false;
            ProgressBarVisibilityDelay();
        }
    }
}