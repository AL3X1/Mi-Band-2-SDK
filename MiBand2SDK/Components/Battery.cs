using MiBand2SDK.Models;
using MiBand2SDK.Utils;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace MiBand2SDK.Components
{
    public class Battery
    {
        private Guid MI_BAND_SERVICE = new Guid("0000fee0-0000-1000-8000-00805f9b34fb");
        private Guid BATTERY_INFO_CHARACTERISTIC = new Guid("00000006-0000-3512-2118-0009af100700");
        private const int NORMAL = 0;
        private const int CHARGING = 1;

        /// <summary>
        /// Getting total battery state as BatteryState Model
        /// </summary>
        /// <returns></returns>
        public async Task<BatteryState> GetCurrentBatteryState()
        {
            BatteryState batteryState = new BatteryState();

            try
            {
                Debug.WriteLine("Getting current battery state");
                GattReadResult gattReadResult = await ReadCurrentCharacteristic();

                if (gattReadResult.Status == GattCommunicationStatus.Success)
                {
                    var batteryData = gattReadResult.Value.ToArray();
                    DateTime lastChargingDate = Convert.ToDateTime($"{batteryData[14]}/{batteryData[13]}/{DateTime.Now.Year} {batteryData[15]}:{batteryData[16]}:{batteryData[17]}");

                    batteryState.ChargeLevel = batteryData[1];
                    batteryState.IsCharging = (batteryData[2] == CHARGING) ? true : false;
                    batteryState.LastCharge = lastChargingDate;
                    batteryState.Cycles = batteryData[18];
                }
            }
            catch (NullReferenceException)
            {
                batteryState.ChargeLevel = 0;
                batteryState.IsCharging = false;
                batteryState.LastCharge = Convert.ToDateTime("0:00");
                batteryState.Cycles = 0;
            }

            return batteryState;
        }

        /// <summary>
        /// Get only battery charge level in percents (int)
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetBatteryChargeLevel()
        {
            GattReadResult gattReadResult = await ReadCurrentCharacteristic();
            int chargeLevel = 0;

            if (gattReadResult.Status == GattCommunicationStatus.Success)
            {
                chargeLevel = gattReadResult.Value.ToArray()[1];
            }

            return chargeLevel;
        }

        /// <summary>
        /// Check if device is charging now
        /// </summary>
        /// <returns></returns>
        public async Task<bool> IsDeviceCharging()
        {
            GattReadResult gattReadResult = await ReadCurrentCharacteristic();
            bool isDeviceCharging = false;
            
            if (gattReadResult.Status == GattCommunicationStatus.Success && gattReadResult.Value.ToArray()[2] == CHARGING)
            {
                isDeviceCharging = true;
            }

            return isDeviceCharging;
        }

        /// <summary>
        /// Get last charging date in dd/mm/yyyy hh:mm:ss format
        /// </summary>
        /// <returns></returns>
        public async Task<DateTime> GetLastChargingDate()
        {
            GattReadResult gattReadResult = await ReadCurrentCharacteristic();

            var batteryData = gattReadResult.Value.ToArray();
            DateTime lastChargingDate = new DateTime();

            if (gattReadResult.Status == GattCommunicationStatus.Success)
            {
                lastChargingDate = Convert.ToDateTime($"{batteryData[14]}/{batteryData[13]}/{DateTime.Now.Year} {batteryData[15]}:{batteryData[16]}:{batteryData[17]}");
            }

            return lastChargingDate;
        }

        /// <summary>
        /// Get total cycles of charge
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetTotalChargeCycles()
        {
            GattReadResult gattReadResult = await ReadCurrentCharacteristic();
            int cycles = 0;

            if (gattReadResult.Status == GattCommunicationStatus.Success)
            {
                cycles = gattReadResult.Value.ToArray()[18];
            }

            return cycles;
        }

        /// <summary>
        /// Read service characteristic with result.
        /// </summary>
        /// <returns></returns>
        private async Task<GattReadResult> ReadCurrentCharacteristic()
        {
            var batteryStateCharacteristic = await Gatt.GetCharacteristicByServiceUuid(MI_BAND_SERVICE, BATTERY_INFO_CHARACTERISTIC);
            GattReadResult gattReadResult = (batteryStateCharacteristic != null) 
                ? await batteryStateCharacteristic.ReadValueAsync(BluetoothCacheMode.Uncached)
                : null;

            return gattReadResult;
        }
    }
}
