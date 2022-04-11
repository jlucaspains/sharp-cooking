using System;
using SharpCooking.Services;
using TinyIoC;
using UserNotifications;

namespace SharpCooking.iOS.Services
{
    public class iOSNotificationReceiver : UNUserNotificationCenterDelegate
    {
        // Called if app is in the foreground.
        public override void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
        {
            if (notification == null || completionHandler == null)
                return;

            ProcessNotification(notification);
            completionHandler(UNNotificationPresentationOptions.Alert);
        }

        // Called if app is in the background, or killed state.
        public override void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
        {
            if (response == null || completionHandler == null)
                return;

            if (response.IsDefaultAction)
            {
                ProcessNotification(response.Notification);
            }
            completionHandler();
        }

        void ProcessNotification(UNNotification notification)
        {
            string title = notification.Request.Content.Title;
            string message = notification.Request.Content.Body;

            TinyIoCContainer.Current.Resolve<INotificationService>().ReceiveNotification(title, message);
        }
    }
}