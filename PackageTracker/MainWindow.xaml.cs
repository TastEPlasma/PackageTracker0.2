using System;
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
using System.Data.Entity;
using System.ComponentModel;
using FedExWebService;

namespace PackageTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TrackerContext _context = new TrackerContext();
        private TrackingControl _control = new TrackingControl();


        public MainWindow()
        {
            InitializeComponent();
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


        private void Update_Click(object sender, RoutedEventArgs e)
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
            Console.WriteLine("Begin update DB");
            //Create list from DB
            var CurrentDBList = _context.Packages.ToList();

            //Pass in Tracking List to Tracking Control for Web Service updating
            _control.UpdateTrackingInformation(CurrentDBList);

            //Commit changes to DB
            _context.SaveChanges();
        }


        private async void ProgressBarVisibilityDelay()
        {
            await Task.Delay(500);
            Progress.Visibility = System.Windows.Visibility.Hidden;
        }


    }
}
