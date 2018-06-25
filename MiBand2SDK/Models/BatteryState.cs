using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiBand2SDK.Models
{
    public class BatteryState
    {
        public int ChargeLevel { get; set; }
        public DateTime LastCharge { get; set; }
        public bool IsCharging { get; set; }
        public int Cycles { get; set; }
    }
}
