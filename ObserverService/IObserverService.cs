using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ObserverService
{
    [ServiceContract]
    public interface IObserverService
    {
        [OperationContract]
        string TestConnection();

        [OperationContract]
        bool GetTrackOption();

        [OperationContract]
        bool ChangeTrackOption();

        [OperationContract]
        int GetEventLogsCount();

        [OperationContract]
        void AddEvent(string log);

        [OperationContract]
        void ConnectToDataBase();

        [OperationContract]
        int SendNotification();

        [OperationContract]
        int Authenticate(string login, string hash);

        [OperationContract]
        string[] GetLogTable();

        [OperationContract]
        string[] GetTypes();

        [OperationContract]
        string GetTypeById(int Id);

        [OperationContract]
        string GetLoginById(int Id);

    }

    [DataContract]
    public partial class User
    {
        [Key]
        [DataMember]
        public int UserId { get; set; }
        [DataMember]
        public int Rights { get; set; }
        [DataMember]
        public string Login { get; set; }
    }

    [DataContract]
    public partial class Info
    {
        [Key]
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Sender { get; set; }
        [DataMember]
        public string SenderEmail { get; set; }
        [DataMember]
        public string SenderPwd { get; set; }
        [DataMember]
        public string Receiver { get; set; }
        [DataMember]
        public string ReceiverEmail { get; set; }
        [DataMember]
        public int HostPort { get; set; }
        [DataMember]
        public string HostName { get; set; }
        [DataMember]
        public bool SslEnable { get; set; }
    }

    [DataContract]
    public partial class Hash
    {
        [Key]
        [DataMember]
        public int HashId { get; set; }
        [DataMember]
        public int UserId { get; set; }
        [DataMember]
        public string UserHash { get; set; }
    }

    [DataContract]
    public partial class EventType
    {
        [Key]
        [DataMember]
        public int EventId { get; set; }
        [DataMember]
        public string Type { get; set; }
    }

    [DataContract]
    public partial class EventLog
    {
        [Key]
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public int EventId { get; set; }
        [DataMember]
        public int UserId { get; set; }
        [DataMember]
        public DateTime TimeCode { get; set; }
        [DataMember]
        public string Coords { get; set; }

        [ForeignKey("EventId")]
        public EventType EventType { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
    }

    [DataContract]
    public partial class ObserverDbContext : DbContext
    {
        public ObserverDbContext() : base("ObserverDBConnection")
        {
        }

        [DataMember]
        public DbSet<User> Users { get; set; }
        [DataMember]
        public DbSet<Info> Infoes { get; set; }
        [DataMember]
        public DbSet<Hash> Hashes { get; set; }
        [DataMember]
        public DbSet<EventType> EventTypes { get; set; }
        [DataMember]
        public DbSet<EventLog> EventLogs { get; set; }

    }
}
