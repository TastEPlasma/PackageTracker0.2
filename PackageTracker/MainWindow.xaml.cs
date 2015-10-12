using System;
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

            //Create list from DB
            var CurrentDBList = _context.Packages.ToList();

            //Delete any entry via entity state whose box is checked
            foreach (TrackerData package in CurrentDBList)
            {
                if(package.DeleteMe == true)
                {
                    _context.Entry(package).State = EntityState.Deleted;
                }
            }

            //Pass in Tracking List to Tracking Control for Web Service updating
            _control.UpdateTrackingInformation(CurrentDBList);

            //Commit changes to DB
            _context.SaveChanges();

            //Refresh view so changes to DB are confirmed and seen
            this.trackerDataDataGrid.Items.Refresh();

            //Remove progress bar after actions are completed
            //Might want to delay this action by 500ms or so, to insure the progress bar blips up
            Progress.Visibility = System.Windows.Visibility.Hidden;
        }
    }
}
