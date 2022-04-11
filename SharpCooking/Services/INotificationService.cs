using System;

namespace SharpCooking.Services
{
    public class NotificationEventArgs : EventArgs
    {
        public string Title { get; set; }
        public string Message { get; set; }
    }

    public interface INotificationService
    {
        event EventHandler NotificationReceived;
        void Initialize();
        int SendNotification(string title, string message, DateTime? notifyTime = null);
        void ReceiveNotification(string title, string message);
        void DeleteNotification(int id);
    }
}
