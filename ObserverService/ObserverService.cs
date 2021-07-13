using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using System.Data.Entity;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace ObserverService
{

    public class ObserverService : IObserverService
    {
        public static ObserverDbContext db = null;
        private static bool isTracking = false;

        public bool GetTrackOption()
        {
            return isTracking;
        }

        public bool ChangeTrackOption()
        {
            if (isTracking)
                Logger.WriteInfo("Tracking stopped");
            else
                Logger.WriteInfo("Tracking started");
            isTracking = !isTracking;
            return (isTracking);
        }

        public int Authenticate(string login, string hash)
        {
            Logger.Write("User trying to authenticate...",
                   ConsoleColor.DarkGreen, ConsoleColor.DarkGray);
            try
            {
                var user = db.Users.Where(u =>
                              u.Login.Trim() == login).FirstOrDefault();
                if (user != null)
                {
                    var userHash = db.Hashes.Where(h =>
                                      h.UserId == user.UserId).FirstOrDefault();
                    if (userHash != null && userHash.UserHash.Trim() == hash)
                    {
                        Logger.WriteSuccess($"User {login} has been authorized");
                        return user.UserId;
                    }
                }
            }
            catch
            {
                Logger.WriteError("Authentication failed, null exception raised");
            }
            Logger.WriteError($"User {login} failed authorization");
            return -1;
        }

        public void ConnectToDataBase()
        {
            try
            {
                Logger.Write("Trying to connect to Database...",
                            ConsoleColor.DarkGreen, ConsoleColor.DarkGray);
                db = new ObserverDbContext();
                Logger.WriteSuccess("Database connection established");
            }
            catch (Exception e)
            {
                Logger.WriteError(e.Message);
            }
        }

        public string[] GetLogTable()
        {
            try
            {
                var logs = (from log in db.EventLogs
                            join type in db.EventTypes on log.EventId equals type.EventId
                            join user in db.Users on log.UserId equals user.UserId
                            select new
                            {
                                Id = log.Id,
                                UserId = log.UserId,
                                Login = user.Login,
                                Coords = log.Coords,
                                TimeCode = log.TimeCode,
                                EventId = log.EventId,
                                Type = type.Type,
                            })?.ToList<object>();

                string[] json = new string[logs.Count()];
                for (int i = 0; i < json.Length; i++)
                {
                    json[i] = JsonSerializer.Serialize(logs.ElementAt(i));
                }
                return (json);
            }
            catch (Exception e)
            {
                Logger.WriteError($"Cannot get log table from db. Description: {e.Message}");
                return null;
            }
        }

        public string[] GetTypes()
        {
            try
            {
                var types = db.EventTypes.ToList();
                string[] json = new string[types.Count()];
                for (int i = 0; i < json.Length; i++)
                {
                    json[i] = JsonSerializer.Serialize(types.ElementAt(i));
                }
                return (json);
            }
            catch (Exception e)
            {
                Logger.WriteError($"Cannot types from db. Description: {e.Message}");
                return null;
            }
        }

        public int GetEventLogsCount()
        {
            int count = 0;

            try
            {
                count = db.EventLogs.Count();
            }
            catch (ArgumentNullException e)
            {
                Logger.WriteError(e.Message);
            }
            return (count);
        }


        public int SendNotification()
        {
            var info = db.Infoes.FirstOrDefault();
            int count = GetEventLogsCount();

            //Notifier.EmailSender.SendSMS(count);
            Notifier.EmailSender.SendMail(info, count);
            return (0);
        }

        public string TestConnection()
        {
            try
            {
                var info = db.Infoes.First();
                Logger.WriteSuccess("Connection test passed");
                return ("PASS");
            }
            catch
            {
                Logger.WriteError("Connection test failed");
                return ("FAIL");
            }
        }

        public void AddEvent(string json)
        {
            try
            {
                var log = JsonSerializer.Deserialize<EventLog>(json);
                db.EventLogs.Add(log);
                db.SaveChanges();
            }
            catch (ArgumentNullException e)
            {
                Logger.WriteError("** Add event error **");
                Logger.WriteError(e.Message);
            }
        }

        public string GetTypeById(int Id)
        {
            try
            {
                return (db.EventTypes.FirstOrDefault(t => t.EventId == Id).Type);
            }
            catch (ArgumentNullException e)
            {
                Logger.WriteError($"Cannot get type with id = {Id}");
                Logger.WriteError(e.Message);
            }
            return (null);
        }

        public string GetLoginById(int Id)
        {
            try
            {
                return (db.Users.FirstOrDefault(u => u.UserId == Id).Login);
            }
            catch (ArgumentNullException e)
            {
                Logger.WriteError($"Cannot get login with id = {Id}");
                Logger.WriteError(e.Message);
            }
            return (null);
        }
    } 
}
