using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MimeKit;
using MailKit.Net.Smtp;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace ObserverService
{
    class Notifier
    {
        public static class EmailSender
        {
            public static string Receiver { get; set; }
            public static string Sender { get; set; }
            public static string SenderPwd { get; set; }

            public static int hostPort { get; set; }
            public static string hostName { get; set; }
            public static bool sslEnable { get; set; }

            /*public static void SendSMS(int eventCount)
            {
                string accountSid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
                string authToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");

                TwilioClient.Init(accountSid, authToken);

                var message = MessageResource.Create(
                    body: $"Current events count is {eventCount}",
                    from: new Twilio.Types.PhoneNumber("+10000000000"),
                    to: new Twilio.Types.PhoneNumber("+79600430025")
                );
            }*/

            public static void SendMail(Info info, int eventCount)
            {
                var message = new MimeMessage();

                message.From.Add(new MailboxAddress(info.Sender, info.SenderEmail));
                message.To.Add(new MailboxAddress(info.Receiver, info.ReceiverEmail));
                message.Subject = "Events notifier";

                message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = "<table bgcolor='#eed44c' border='0' cellpadding'0' cellspacing='0' " +
                           "style='margin:0; padding:0' width='100%'>" +
                           "<tr><td height='100%'>" +
                                $"<h3> Current count of events is: {eventCount}</h3>" +
                           "</td></tr></table>".Replace("'", "\"")
                };

                using (var client = new SmtpClient())
                {
                    try
                    {
                        client.Connect(info.HostName, info.HostPort, info.SslEnable);
                        client.Authenticate(info.SenderEmail, info.SenderPwd);
                        client.Send(message);
                        client.Disconnect(true);
                        Logger.WriteSuccess($"Notification has been sent to {info.ReceiverEmail}");
                    }
                    catch (Exception e)
                    {
                        Logger.WriteError($"Notification failed. Description: {e.Message}");
                    }

                }
            }
        }
    }
}
