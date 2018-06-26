using MiBand2SDK.Models;
using MiBand2SDK.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace MiBand2SDK.Components
{
    public class Activity
    {
        private Guid MI_BAND_SERVICE = new Guid("0000fee0-0000-1000-8000-00805f9b34fb");
        private Guid STEP_INFO_CHARACTERISTIC = new Guid("00000007-0000-3512-2118-0009af100700");
        private Guid USER_SETTINGS_CHARACTERISTIC = new Guid("00000008-0000-3512-2118-0009AF100700");

        /// <summary>
        /// Get steps and activity info.
        /// </summary>
        /// <returns>StepInfo object as Activity data.</returns>
        public async Task<StepInfo> GetStepInfoAsync()
        {
            var characteristic = await Gatt.GetCharacteristicByServiceUuid(MI_BAND_SERVICE, STEP_INFO_CHARACTERISTIC);
            var gattReadResult = await characteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
            StepInfo stepInfo = new StepInfo();

            if (gattReadResult.Status == GattCommunicationStatus.ProtocolError)
            {
                Debug.WriteLine("Protocol error. Trying to take values from cache");
                gattReadResult = await characteristic.ReadValueAsync(BluetoothCacheMode.Cached);
            }

            var data = gattReadResult.Value.ToArray();
            int totalSteps = ((data[1] & 255) | ((data[2] & 255) << 8));
            int distance = ((((data[5] & 255) | ((data[6] & 255) << 8)) | (data[7] & 16711680)) | ((data[8] & 255) << 24));
            int calories = ((((data[9] & 255) | ((data[10] & 255) << 8)) | (data[11] & 16711680)) | ((data[12] & 255) << 24));

            stepInfo.Steps = totalSteps;
            stepInfo.Distance = distance;
            stepInfo.Calories = calories;

            return stepInfo;
        }

        /// <summary>
        /// Show notification on Band when steps per day value reached.
        /// </summary>
        /// <param name="stepsPerDay"></param>
        /// <returns></returns>
        public async Task<bool> SetFitnessGoalAsync(int stepsPerDay)
        {
            GattCharacteristic userSettingsCharacteristic = await Gatt.GetCharacteristicByServiceUuid(MI_BAND_SERVICE, USER_SETTINGS_CHARACTERISTIC);
            byte[] setFitnessGoalStartCmd = new byte[] { 0x10, 0x0, 0x0 };
            byte[] setFitnessGoalEndCmd = new byte[] { 0, 0 };

            byte[] data = new byte[] { (byte)(stepsPerDay & 0xff), (byte)((stepsPerDay >> 8) & 0xff) };
            List<byte> bytes = new List<byte>();
            bytes = setFitnessGoalStartCmd.Concat(data).ToList();
            bytes = bytes.Concat(setFitnessGoalEndCmd).ToList();

            return await userSettingsCharacteristic.WriteValueAsync(bytes.ToArray().AsBuffer()) == GattCommunicationStatus.Success;
        }
    }
}
