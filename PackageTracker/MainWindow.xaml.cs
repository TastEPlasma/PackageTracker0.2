using System;
using System.Timers;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Drawing;
using System.Windows.Forms;
using System.Data.Entity;
using System.ComponentModel;
using FedExWebService;

namespace PackageTracker
{
    public partial class MainWindow : Window
    {
        #region Private Members
        //Required context for Entity Framework to build DB
        //In this case, the context is left open per session since it is accessing a local DB
        private TrackerContext _context = new TrackerContext();

        //Primary internal inteface to 3rd party web service providers
        private TrackingControl _control = new TrackingControl();

        //Internal timer for Auto-Updates CheckBox
        private System.Windows.Threading.DispatcherTimer UpdateTimer = new DispatcherTimer();
        private TimeSpan DelayTime = new TimeSpan(1, 0, 0);
        #endregion

        #region Properties
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
        #endregion

        #region Constructors And Initializers
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
            _context.Packages.Load();

            // After the data is loaded call the DbSet<T>.Local property  
            // to use the DbSet<T> as a binding source. 
            trackerDataViewSource.Source = _context.Packages.Local;
        }
        #endregion

        #region Database Methods
        private void UpdateDatabase(object sender, EventArgs e)
        {
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

        private void UpdateDBFromWebServices()
        {
            //Create list from DB
            var CurrentDBList = _context.Packages.ToList();

            //Pass in Tracking List to Tracking Control for Web Service updating
            _control.UpdateTrackingInformation(CurrentDBList);

            //Commit changes to DB
            _context.SaveChanges();
        }
        #endregion

        #region GUI Methods
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

        #region Webservice specific methods
        //FEDEX
        private void OpenFedExCredentialsMenu_Click(object sender, RoutedEventArgs e)
        {
            FedExCredentialEntry_PopUp.IsOpen = true;
        }

        private void FedExAccountUpdate_Click(object sender, RoutedEventArgs e)
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

                FedExCredentialEntry_PopUp.IsOpen = false;
            }
            else
            {
                //Having this value in the first field of the array tells the FedExManager to reset the credentials
                UpdatedInfo.Add("ResetToDefaults");

                //Send new information to FedExManager via TrackingControl
                _control.UpdateCredentialInformation(UpdatedInfo, ParcelService.FedEx);

                //Blank fields on reset to defaults
                FedExUserKEY.Text = "";
                FedExUserPASSWORD.Text = "";
                FedExUserMETERNUMBER.Text = "";
                FedExUserACCOUNTNUMBER.Text = "";

                FedExCredentialEntry_PopUp.IsOpen = false;
            }

            ProgressBarVisibilityDelay();
        }

        //UPS
        private void OpenUPSCredentialsMenu_Click(object sender, RoutedEventArgs e)
        {
            UPSCredentialEntry_PopUp.IsOpen = true;
        }

        private void UPSAccountUpdate_Click(object sender, RoutedEventArgs e)
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

                UPSCredentialEntry_PopUp.IsOpen = false;
            }
            else
            {
                //Having this value in the first field of the array tells the UPSManager to reset the credentials
                UpdatedInfo.Add("ResetToDefaults");

                //Send new information to UPSManager via TrackingControl
                _control.UpdateCredentialInformation(UpdatedInfo, ParcelService.UPS);

                //Blank fields on reset to defaults
                UPSUserLicenseNUMBER.Text = "";
                UPSUserNAME.Text = "";
                UPSUserPASSWORD.Text = "";

                UPSCredentialEntry_PopUp.IsOpen = false;
            }

            ProgressBarVisibilityDelay();
        }

        //USPS
        private void OpenUSPSCredentialsMenu_Click(object sender, RoutedEventArgs e)
        {
            USPSCredentialEntry_PopUp.IsOpen = true;
        }

        private void USPSAccountUpdate_Click(object sender, RoutedEventArgs e)
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

                USPSCredentialEntry_PopUp.IsOpen = false;
            }
            else
            {
                //Having this value in the first field of the array tells the UPSManager to reset the credentials
                UpdatedInfo.Add("ResetToDefaults");

                //Send new information to UPSManager via TrackingControl
                _control.UpdateCredentialInformation(UpdatedInfo, ParcelService.USPS);

                //Blank fields on reset to defaults
                USPSUserID.Text = "";

                USPSCredentialEntry_PopUp.IsOpen = false;
            }

            ProgressBarVisibilityDelay();
            
        }
        #endregion

        #endregion

    }
}
