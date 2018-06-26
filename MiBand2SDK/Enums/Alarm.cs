using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiBand2SDK.Enums
{
    public enum AlarmDays
    {
        MONDAY = 1,
        TUESDAY = 2,
        WEDNESDAY = 4,
        THURSDAY = 8,
        FRIDAY = 16,
        SATURDAY = 32,
        SUNDAY = 64,
        ONCE = 128
    }

    public enum AlarmStatus
    {
        ON = 128,
        OFF = 0
    }
}
