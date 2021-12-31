using System;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using SharpCooking.Services;
using AndroidApp = Android.App.Application;

namespace SharpCooking.Droid.Services
{
    public class AndroidNotificationService : INotificationService
    {
        const string ChannelId = "default";
        const string ChannelName = "Default";
        const string ChannelDescription = "The default channel for notifications.";

        public const string TitleKey = "title";
        public const string MessageKey = "message";

        bool _channelInitialized;
        int _messageId;
        int _pendingIntentId;

        NotificationManager _manager;

        public event EventHandler NotificationReceived;

        public static AndroidNotificationService Instance { get; private set; }

        public AndroidNotificationService() => Initialize();

        public void Initialize()
        {
            if (Instance == null)
            {
                CreateNotificationChannel();
                Instance = this;
            }
        }

        public int SendNotification(string title, string message, DateTime? notifyTime = null)
        {
            var result = 0;

            if (!_channelInitialized)
            {
                CreateNotificationChannel();
            }

            if (notifyTime != null)
            {
#pragma warning disable CA2000 // Dispose objects before losing scope
                var intent = new Intent(AndroidApp.Context, typeof(AlarmHandler));
#pragma warning restore CA2000 // Dispose objects before losing scope
                intent.PutExtra(TitleKey, title);
                intent.PutExtra(MessageKey, message);

                result = _pendingIntentId++;

                PendingIntent pendingIntent = PendingIntent.GetBroadcast(AndroidApp.Context, _pendingIntentId++, intent, PendingIntentFlags.CancelCurrent);
                long triggerTime = GetNotifyTime(notifyTime.Value);
                AlarmManager alarmManager = AndroidApp.Context.GetSystemService(Context.AlarmService) as AlarmManager;
                alarmManager.Set(AlarmType.RtcWakeup, triggerTime, pendingIntent);
            }
            else
            {
                Show(title, message);
            }

            return result;
        }

        public void ReceiveNotification(string title, string message)
        {
            var args = new NotificationEventArgs()
            {
                Title = title,
                Message = message,
            };
            NotificationReceived?.Invoke(null, args);
        }

        public void Show(string title, string message)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            var intent = new Intent(AndroidApp.Context, typeof(MainActivity));
            intent.PutExtra(TitleKey, title);
            intent.PutExtra(MessageKey, message);

            PendingIntent pendingIntent = PendingIntent.GetActivity(AndroidApp.Context, _pendingIntentId++, intent, PendingIntentFlags.UpdateCurrent);

            NotificationCompat.Builder builder = new NotificationCompat.Builder(AndroidApp.Context, ChannelId)
                .SetContentIntent(pendingIntent)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetSmallIcon(Resource.Drawable.notification)
                .SetDefaults((int)NotificationDefaults.Sound | (int)NotificationDefaults.Vibrate);

            Notification notification = builder.Build();
            _manager.Notify(_messageId++, notification);
#pragma warning restore CA2000 // Dispose objects before losing scope
        }

        public void DeleteNotification(int id)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            var intent = new Intent(AndroidApp.Context, typeof(AlarmHandler));
#pragma warning restore CA2000 // Dispose objects before losing scope
            PendingIntent pendingIntent = PendingIntent.GetBroadcast(AndroidApp.Context, id, intent, PendingIntentFlags.CancelCurrent);
            AlarmManager alarmManager = AndroidApp.Context.GetSystemService(Context.AlarmService) as AlarmManager;
            alarmManager.Cancel(pendingIntent);
        }

        void CreateNotificationChannel()
        {
            _manager = (NotificationManager)AndroidApp.Context.GetSystemService(AndroidApp.NotificationService);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
#pragma warning disable CA2000 // Dispose objects before losing scope
                var channelNameJava = new Java.Lang.String(ChannelName);
                var channel = new NotificationChannel(ChannelId, channelNameJava, NotificationImportance.Default)
                {
                    Description = ChannelDescription
                };
#pragma warning restore CA2000 // Dispose objects before losing scope
                _manager.CreateNotificationChannel(channel);
            }

            _channelInitialized = true;
        }

        static long GetNotifyTime(DateTime notifyTime)
        {
            DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(notifyTime);
            double epochDiff = (new DateTime(1970, 1, 1) - DateTime.MinValue).TotalSeconds;
            long utcAlarmTime = utcTime.AddSeconds(-epochDiff).Ticks / 10000;
            return utcAlarmTime; // milliseconds
        }
    }
}