using MiBand2SDK.Enums;
using MiBand2SDK.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace MiBand2SDK.Components
{
    public class Display
    {
        private Guid MI_BAND_SERVICE = new Guid("0000fee0-0000-1000-8000-00805f9b34fb");
        private Guid CURRENT_TIME_CHARACTERISTIC = new Guid("00002a2b-0000-1000-8000-00805f9b34fb");
        private Guid DISPLAY_DATE_CHARACTERISTIC = new Guid("00000003-0000-3512-2118-0009AF100700");
        private Guid CONFIGURATION_CHARACTERISTIC = new Guid("00000003-0000-3512-2118-0009af100700");

        /// <summary>
        /// Set display date mode (datetime or time, 12 hour or 24 hour)
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Set display items like calories, steps, distance etc.
        /// Display clock cannot be removed.
        /// </summary>
        /// <param name="items">Display items from Models.DisplayItems enumerable.</param>
        /// <returns></returns>
        public async Task<bool> SetDisplayItems(IEnumerable<byte> items)
        {
            GattCharacteristic displayItemsCharacteristic = await Gatt.GetCharacteristicByServiceUuid(MI_BAND_SERVICE, CONFIGURATION_CHARACTERISTIC);
            byte[] changeScreensCmd = new byte[] { 10, 1, 0, 0, 1, 2, 3, 4, 5 };
            byte allItems = 1;

            foreach (var item in items)
            {
                allItems += item;
            }

            changeScreensCmd[1] |= allItems;

            return await displayItemsCharacteristic.WriteValueAsync(changeScreensCmd.ToArray().AsBuffer()) == GattCommunicationStatus.Success;
        }

        /// <summary>
        /// Set metric system (default - metric)
        /// </summary>
        /// <param name="metricSystem"></param>
        /// <returns></returns>
        public async Task<bool> SetMetricSystem(MetricSystem metricSystem)
        {
            var characteristic = await Gatt.GetCharacteristicByServiceUuid(MI_BAND_SERVICE, CONFIGURATION_CHARACTERISTIC);
            byte[] setMetricSystemCmd = new byte[] { 6, 3, 0, (byte)metricSystem };

            return await characteristic.WriteValueAsync(setMetricSystemCmd.ToArray().AsBuffer()) == GattCommunicationStatus.Success;
        }
    }
}
