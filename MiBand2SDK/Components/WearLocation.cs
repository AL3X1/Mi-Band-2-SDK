using MiBand2SDK.Enums;
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
    public class WearLocation
    {
        private Guid MIBAND2_SERVICE = new Guid("0000fee0-0000-1000-8000-00805f9b34fb");
        private Guid USER_SETTINGS_CHARACTERISTIC = new Guid("00000008-0000-3512-2118-0009af100700");
        private Guid CONFIGURATION_CHARACTERISTIC = new Guid("00000003-0000-3512-2118-0009af100700");

        public async Task<bool> SetWearLocation(Enums.WearLocation location)
        {
            GattCharacteristic wearLocationCharacteristic = await Gatt.GetCharacteristicByServiceUuid(MIBAND2_SERVICE, USER_SETTINGS_CHARACTERISTIC);
            byte[] setWearLocationCmd = new byte[] { 32, 0, 0, (byte)location };

            return await wearLocationCharacteristic.WriteValueAsync(setWearLocationCmd.AsBuffer()) == GattCommunicationStatus.Success;
        }

        public async Task<bool> SwitchInfoByRotateWrist(WristMode mode)
        {
            GattCharacteristic wearLocationCharacteristic = await Gatt.GetCharacteristicByServiceUuid(MIBAND2_SERVICE, CONFIGURATION_CHARACTERISTIC);
            byte[] switchInfoByRotateCmd = new byte[] { 6, 13, 0, (byte)mode };

            return await wearLocationCharacteristic.WriteValueAsync(switchInfoByRotateCmd.AsBuffer()) == GattCommunicationStatus.Success;
        }

        public async Task<bool> ActivateDisplayByRotateWrist(WristMode mode)
        {
            GattCharacteristic wearLocationCharacteristic = await Gatt.GetCharacteristicByServiceUuid(MIBAND2_SERVICE, CONFIGURATION_CHARACTERISTIC);
            byte[] activateByRotateCmd = new byte[] { 6, 5, 0, (byte)mode };

            return await wearLocationCharacteristic.WriteValueAsync(activateByRotateCmd.AsBuffer()) == GattCommunicationStatus.Success;
        }
    }
}
