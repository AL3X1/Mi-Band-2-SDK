using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiBand2SDK.Enums
{
    public enum DisplayCallerInfo
    {
        ENABLED = 1,
        DISABLED = 0
    }

    public enum DisplayItems
    {
        CLOCK = 1,
        STEPS = 2,
        DISTANCE = 4,
        CALORIES = 8,
        HEART_RATE = 16,
        BATTERY = 32
    }

    public enum MetricSystem
    {
        METRIC = 0,
        IMPERIAL = 1
    }
}
