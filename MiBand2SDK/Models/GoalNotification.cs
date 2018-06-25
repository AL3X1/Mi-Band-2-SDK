using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiBand2SDK.Models
{
    public enum GoalNotification
    {
        ENABLED = 1,
        DISABLED = 0
    }

    public enum NotificationLevels
    {
        NONE = 0,
        MESSAGE = 1,
        PHONE_CALL = 2,
        VIBRATE_ONLY = 3,
        CUSTOM = 250
    }

    public enum CustomNotificationLevels
    {
        // TODO: Add custom notification Levels
    }
}
