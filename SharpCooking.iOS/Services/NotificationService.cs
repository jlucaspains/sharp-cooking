using System;
using System.Globalization;
using System.Linq;
using Foundation;
using SharpCooking.Services;
using UIKit;
using UserNotifications;

namespace SharpCooking.iOS.Services
{
    public class iOSNotificationService : INotificationService
    {
        int messageId;
        bool hasNotificationsPermission;

        public event EventHandler NotificationReceived;

        public iOSNotificationService()
        {
            Initialize();
        }

        public void Initialize()
        {
            // request the permission to use local notifications
            UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert, (approved, err) =>
            {
                hasNotificationsPermission = approved;
            });
        }

        public int SendNotification(string title, string message, DateTime? notifyTime = null)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope

            // EARLY OUT: app doesn't have permissions
            if (!hasNotificationsPermission)
            {
                return 0;
            }

            messageId++;

            var content = new UNMutableNotificationContent()
            {
                Title = title,
                Subtitle = "",
                Body = message,
                Badge = 1
            };

            UNNotificationTrigger trigger;
            if (notifyTime != null)
            {
                // Create a calendar-based trigger.
                trigger = UNCalendarNotificationTrigger.CreateTrigger(GetNSDateComponents(notifyTime.Value), false);
            }
            else
            {
                // Create a time-based trigger, interval is in seconds and must be greater than 0.
                trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(0.25, false);
            }

            var request = UNNotificationRequest.FromIdentifier(messageId.ToString(CultureInfo.InvariantCulture), content, trigger);
            UNUserNotificationCenter.Current.AddNotificationRequest(request, (err) =>
            {
                if (err != null)
                {
                    throw new Exception($"Failed to schedule notification: {err}");
                }
            });

            return messageId;
#pragma warning restore CA2000 // Dispose objects before losing scope
        }

        public void ReceiveNotification(string title, string message)
        {
            var args = new NotificationEventArgs()
            {
                Title = title,
                Message = message
            };
            NotificationReceived?.Invoke(null, args);
        }

        NSDateComponents GetNSDateComponents(DateTime dateTime)
        {
            return new NSDateComponents
            {
                Month = dateTime.Month,
                Day = dateTime.Day,
                Year = dateTime.Year,
                Hour = dateTime.Hour,
                Minute = dateTime.Minute,
                Second = dateTime.Second
            };
        }

        public void DeleteNotification(int id)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope

            UILocalNotification[] localNotifications = UIApplication.SharedApplication.ScheduledLocalNotifications;

            //Traverse this array to get the UILocalNotification we want according to the key
            var toCancel = localNotifications
                .FirstOrDefault(item => item.UserInfo.ObjectForKey(new NSString("key")).ToString() == id.ToString(CultureInfo.InvariantCulture));

            if (toCancel != null)
                UIApplication.SharedApplication.CancelLocalNotification(toCancel);
#pragma warning restore CA2000 // Dispose objects before losing scope
        }
    }
}