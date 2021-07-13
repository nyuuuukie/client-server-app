using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ServiceModel;
using System.ServiceModel.Description;
using ObserverService;

using System.Globalization;
using System.Threading;
using Microsoft.Win32;

namespace ObserverServiceHost
{
    class Program
    {
        static string GetLocation()
        {
            /*
            Console.Write("Protocol: ");
            string protocol = Console.ReadLine();
            Console.Write("IP: ");
            string ipaddr = Console.ReadLine();
            Console.Write("Port: ");
            string port = Console.ReadLine();

            string location = $"{protocol}://{ipaddr}:{port}/";

            return location;
            */
            return ("http://localhost:8080/");
        }

        public static void WaitKey(string message, ConsoleKey key)
        {
            do
            {
                Console.WriteLine(message);
            }
            while (Console.ReadKey().Key != key);
        }

        public static List<EventLog> getLogs()
        {
            using (ObserverDbContext db2 = new ObserverDbContext())
            {
                return db2.EventLogs.ToList();
            }
        }

        static void Main(string[] args)
        {
            string location;
            Uri baseAddress = null;
            ServiceHost host = null;

            do
            {
                try
                {
                    location = GetLocation();
                    baseAddress = new Uri(location);
                }
                catch (UriFormatException e)
                {
                    ObserverService.Logger.WriteError(e.Message);
                    continue;
                }
            } while (false);

            try
            {
                host = new ServiceHost(
                            typeof(ObserverService.ObserverService), baseAddress);
                host.Open();
                Logger.WriteSuccess($"Service running at {host.BaseAddresses[0]}");

                ObserverService.ObserverService obs = new ObserverService.ObserverService();
                if (obs == null)
                    Logger.WriteError("Cannot create instance of service");
                else
                {
                    obs.ConnectToDataBase();
                    if (ObserverService.ObserverService.db == null)
                        Logger.WriteError("Cannot create connection to DB");
                }

                obs.TestConnection();
            }
            catch (ArgumentNullException e)
            {
                Logger.WriteError(e.Message);
                host?.Abort();
                host = null;
            }
            catch (InvalidOperationException e)
            {
                Logger.WriteError(e.Message);
                host?.Abort();
                host = null;
            }
            catch (CommunicationException e)
            {
                Logger.WriteError(e.Message);
                host?.Abort();
                host = null;
            }
            finally
            {
                WaitKey($"\rPress {ConsoleKey.Q.ToString()} to quit...", ConsoleKey.Q);
                host?.Close();
            }
        }
    }
}
