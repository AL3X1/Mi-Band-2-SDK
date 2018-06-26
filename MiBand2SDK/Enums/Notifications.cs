using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiBand2SDK.Enums
{
    public enum FitnessGoalNotification
    {
        ENABLED = 1,
        DISABLED = 0
    }

    public enum NotificationType
    {
        NONE = 0,
        MESSAGE = 1,
        PHONE_CALL = 2,
        VIBRATE_ONLY = 3,
        CUSTOM = 250
    }

    public enum CustomVibrationProfile
    {
        INFINITE,
        SHORT,
        LONG,
        QUICK,
        WATER_DROP,
        RING
    }

    public enum DoNotDisturbMode
    {
        OFF = 130,
        AUTOMATIC = 131,
        SCHEDULED = 129
    }

    public enum InactivityWarningMode
    {
        ON = 1,
        OFF = 0
    }
}
