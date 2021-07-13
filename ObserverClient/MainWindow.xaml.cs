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

using System.Text.Json;
using System.Text.Json.Serialization;

using ObserverService;
using ObserverClient.ObserverService;

namespace ObserverService
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public enum Events
    {
        LEFT_CLICK = 1,
        RIGHT_CLICK = 2,
        MIDDLE_CLICK = 3,
        MOUSE_MOVE = 4
    };

    public partial class MainWindow : Window
    {
        private bool isTracking = false;
        private ObserverServiceClient client = null;
        private int UserId = -1;
        private Point current;
        private static int eventCount = 0;
        private List<EventSer> events = null;
        private List<EventSer> filtered = null; 
        private readonly object obsLock = new object();

        public MainWindow()
        {
            InitializeComponent();
            client = new ObserverServiceClient("BasicHttpBinding_IObserverService");

        }

        ~MainWindow()
        {
            client.Close();
        }

        private class EventSer : EventLog
        {
            public string Type { get; set; }
            public string Login { get; set; }
        }

        private void MainPanelInit()
        {
            isTracking = false;
            TrackButtonSetProps("Press to Track", Brushes.Gray);

            string[] json = client.GetLogTable();

            eventCount = json.Length;
            SetCoordString();
            SetCountString();
            UpdateEventList();
            ResetFilters();
           
            events = new List<EventSer>();
            filtered = new List<EventSer>();

            for (int i = 0; i < json.Length; i++)
            {
                events.Add(JsonSerializer.Deserialize<EventSer>(json[i]));
            }

            Logs.ItemsSource = events;
        }

        private void ResetFiltersButtonClick(object sender, RoutedEventArgs e)
        {
            ResetFilters();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (isTracking)
            {
                switch (e.ChangedButton)
                {
                    case MouseButton.Left:
                        SendEvent(Events.LEFT_CLICK);
                        break;
                    case MouseButton.Right:
                        SendEvent(Events.RIGHT_CLICK);
                        break;
                    case MouseButton.Middle:
                        SendEvent(Events.MIDDLE_CLICK);
                        break;
                    default:
                        break;
                }
                SetCoordString();
                SetCountString();
            }
        }

        private void SendEvent(Events eventId)
        {
            //Fill data for DB
            EventLog log = new EventLog();
            log.UserId = UserId;
            log.EventId = (int)eventId;
            log.TimeCode = DateTime.Now;
            log.Coords = GetCoordString();

            //Send to the server
            string json = JsonSerializer.Serialize(log);
            client.AddEvent(json);

            //Fill data for DataGrid
            EventSer ev = new EventSer();
            ev.Id = eventCount;
            ev.UserId = log.UserId;
            ev.EventId = log.EventId;
            ev.TimeCode = log.TimeCode;
            ev.Coords = log.Coords;
            ev.Login = client.GetLoginById(ev.UserId);
            ev.Type = client.GetTypeById(ev.EventId);

            //Update datagrid & count state
            lock (obsLock)
            {
                //Updating internal data sources
                if (FilterSingle(ev, FromDate.Value, ToDate.Value))
                    filtered.Add(ev);
                events.Add(ev);

                //Updatimg datagrid
                Logs.Items.Refresh();

                //Updating counter & asking user for sending notification
                eventCount++;
                if (eventCount % 50 == 0)
                    NotifyDialog();
            };

        }
        private void NotifyDialog()
        {
            if (MessageBox.Show("Do you want to send notification?", "", MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes) 
                client.SendNotification();
        }

        private void SetCountString()
        {
            CountInfoLabel.Content = $"Events: {eventCount.ToString()}";
        }

        private string GetCoordString()
        {
            return $"{current.X.ToString()};{current.Y.ToString()}";
        }

        private void SetCoordString()
        {
            PosInfoLabel.Content = $"Current position {GetCoordString()}";
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            //Get current pos
            if (isTracking)
            {
                current = e.GetPosition(this);

                //Check if shifted by more than 10 pixels.
                //If it does, then send notifier to the server
                if (Tracker.checkIfShifted(current))
                {
                    SendEvent(Events.MOUSE_MOVE);
                }
                SetCoordString();
                SetCountString();
            }
        }

        private void ResetFilters()
        {
            FromDate.Value = new DateTime(1970, 01, 01);
            ToDate.Value = new DateTime(2200, 01, 01);
            EventList.SelectedItem = "None";
        }

        private bool FilterSingle(EventSer ev, DateTime? from, DateTime? to)
        {
            string selectedEvent = null;

            FilterInit(ref from, ref to, ref selectedEvent);
            return (FilterEvent(ev, from, to, selectedEvent));
        }

        private bool FilterEvent(EventSer ev, DateTime? from, DateTime? to, string evType)
        {
            return (ev.TimeCode >= from && ev.TimeCode <= to && (evType == "None" || ev.Type == evType));
        }

        private void FilterInit(ref DateTime? from, ref DateTime? to, ref string selEv)
        {
            if (from == null)
                from = DateTime.MinValue;
            if (to == null)
                to = DateTime.MaxValue;

            //now we add our Filter
            selEv = EventList.SelectedItem?.ToString();
        }

        private void FilterTable(DateTime? from, DateTime? to)
        {
            string selectedEvent = null;

            FilterInit(ref from, ref to, ref selectedEvent);
            
            filtered = events?.Where(e => FilterEvent(e, from, to, selectedEvent))?.ToList();

            Logs.ItemsSource = filtered;
        }

        private void UpdateEventList()
        {
            string[] types = client.GetTypes();

            EventList.Items.Add("None");
            if (types != null)
            {
                for (int i = 0; i < types.Length; i++)
                {
                    EventList.Items.Add(JsonSerializer.Deserialize<EventType>(types[i]).Type);
                }
            }
        }

        private void OnEventSelection(object sender, RoutedEventArgs e)
        {
            FilterTable(FromDate.Value, ToDate.Value);
        }

        private void OnDateChange(object sender, EventArgs e)
        {
            FilterTable(FromDate.Value, ToDate.Value);
        }

        private void TrackButtonSetProps(string content, SolidColorBrush color)
        {
            TrackButton.Background = color;
            TrackButton.Content = content;
        }

        private void TrackTumbler(object sender, RoutedEventArgs e)
        {
            if (isTracking)
            {
                TrackButtonSetProps("Press to track", Brushes.Gray);
            }
            else
            {
                TrackButtonSetProps("Tracking...",
                                new SolidColorBrush((Color)ColorConverter
                                   .ConvertFromString("#FFB9DADD")));
            }
            isTracking = client.ChangeTrackOption();
        }

        private void CleanInput()
        {
            this.Login.Text = "";
            this.Password.Password = "";
        }
        private void MessageBoxError(string title, string msg)
        {
            MessageBox.Show(
                   msg,
                   title,
                   MessageBoxButton.OK,
                   MessageBoxImage.Error
            );
        }

        private bool CheckIfCorrectInput(string login, string password)
        {
            if (!Validator.isLoginCorrect(login) ||
               !Validator.isPasswordCorrect(password))
            {
                MessageBoxError("Auth error",
                    "The data you've trying to sign in with is invalid");
                return false;
            }
            return true;
        }

        private bool CheckIfAuthorized(int UId)
        {
            if (UserId == -1)
            {
                MessageBoxError("Auth error",
                   "Cannot authenticate user with this data");
                return (false);
            }
            return (true);
        }

        private void Signin(object sender, RoutedEventArgs e)
        {
            //get data
            string login = this.Login.Text;
            string password = this.Password.Password;
            CleanInput();

            if (!CheckIfCorrectInput(login, password))
                return ;

            //transform password into hash
            
            string hashedPassword = Hash.ComputeSha256Hash(password);

            //send login, hash pair into server
            UserId = client.Authenticate(login, hashedPassword);

            //get answer from server
            if (!CheckIfAuthorized(UserId))
                return ;

            AuthPanel.Visibility = Visibility.Collapsed;
            MainPanel.Visibility = Visibility.Visible;
            MainPanelInit();
        }

        private void Signout(object sender, RoutedEventArgs e)
        {
            this.UserId = -1;
            this.isTracking = false;
            MainPanel.Visibility = Visibility.Collapsed;
            AuthPanel.Visibility = Visibility.Visible;
        }

        private void NotifyTumbler(object sender, RoutedEventArgs e)
        {
            client.SendNotification();
        }
        private void TrackButtonMouseEnter(object sender, RoutedEventArgs e)
        {
        }

    }
}
