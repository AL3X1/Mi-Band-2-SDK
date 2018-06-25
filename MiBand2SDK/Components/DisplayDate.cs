using MiBand2SDK.Models;
using MiBand2SDK.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace MiBand2SDK.Components
{
    class DisplayDate
    {
        private Guid MI_BAND_SERVICE = new Guid("0000fee0-0000-1000-8000-00805f9b34fb");
        // Текущее время: [4], [5], [6]
        private Guid CURRENT_TIME_CHARACTERISTIC = new Guid("00002a2b-0000-1000-8000-00805f9b34fb");
        private Guid DISPLAY_DATE_CHARACTERISTIC = new Guid("00000003-0000-3512-2118-0009AF100700");

        public async Task<bool> SetDisplayDate(DateMode mode)
        {
            byte[] date = null;

            switch (mode)
            {
                case DateMode.DATEFORMAT_TIME:
                    date = new byte[] { 6, 10, 0, 0 };
                    break;

                case DateMode.DATEFORMAT_DATETIME:
                    date = new byte[] { 6, 10, 0, 3 };
                    break;

                case DateMode.DATEFORMAT_12_HOURS:
                    date = new byte[] { 6, 2, 0, 0 };
                    break;

                case DateMode.DATEFORMAT_24_HOURS:
                    date = new byte[] { 6, 2, 0, 1 };
                    break;
            }

            Debug.WriteLine("Set display time");

            var characteristic = await Gatt.GetCharacteristicByServiceUuid(MI_BAND_SERVICE, DISPLAY_DATE_CHARACTERISTIC);
            var gattWriteResult = await characteristic.WriteValueAsync(date.AsBuffer());
            return gattWriteResult == GattCommunicationStatus.Success;
        }
    }
}
