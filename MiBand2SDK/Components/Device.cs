using MiBand2SDK.Models;
using MiBand2SDK.Utils;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Foundation;

namespace MiBand2SDK.Components
{
    public class Device
    {
        private Guid MIBAND2_SERVICE = new Guid("0000fee0-0000-1000-8000-00805f9b34fb");
        private Guid DEVICE_NAME_SERVICE = new Guid("00001800-0000-1000-8000-00805f9b34fb");
        private Guid DEVICE_NAME_CHARACTERISTIC = new Guid("00002a00-0000-1000-8000-00805f9b34fb");
        private Guid DEVICE_INFO_SERVICE = new Guid("0000180a-0000-1000-8000-00805f9b34fb");
        private Guid SERIAL_NUMBER_CHARACTERISTIC = new Guid("00002a25-0000-1000-8000-00805f9b34fb");
        private Guid HARDWARE_REVISION_CHARACTERISTIC = new Guid("00002a27-0000-1000-8000-00805f9b34fb");
        private Guid SOFTWARE_REVISION_CHARACTERISTIC = new Guid("00002a28-0000-1000-8000-00805f9b34fb");
        private Guid DEVICE_EVENT_CHARACTERISTIC = new Guid("00000010-0000-3512-2118-0009af100700");

        /// <summary>
        /// Subscribe to device events (touch to band)
        /// </summary>
        /// <param name="eventHandler"></param>
        /// <returns></returns>
        public async Task SubscribeToDeviceEventNotificationsAsync(TypedEventHandler<GattCharacteristic, GattValueChangedEventArgs> eventHandler)
        {
            var deviceCharacteristic = await Gatt.GetCharacteristicByServiceUuid(MIBAND2_SERVICE, DEVICE_EVENT_CHARACTERISTIC);
            var deviceNotify = await deviceCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);

            Debug.WriteLine("Subscribe to device event notifications");
            if (deviceNotify == GattCommunicationStatus.Success)
                deviceCharacteristic.ValueChanged += eventHandler;
        }

        /// <summary>
        /// Get device information as DeviceInfo object
        /// </summary>
        /// <returns></returns>
        public async Task<DeviceInfo> GetDeviceInfo()
        {
            string deviceName = await GetDeviceName();
            string serialNumber = await GetSerialNumber();
            string hardwareRevision = await GetHardwareRevision();
            string softwareRevision = await GetSoftwareRevision();

            return new DeviceInfo()
            {
                DeviceName = deviceName,
                HardwareVersion = hardwareRevision,
                SerialNumber = serialNumber,
                SoftwareVersion = softwareRevision
            };
        }

        public async Task<string> GetDeviceName()
        {
            var deviceNameCharacteristic = await Gatt.GetCharacteristicByServiceUuid(DEVICE_NAME_SERVICE, DEVICE_NAME_CHARACTERISTIC);
            GattReadResult deviceNameReadResult = await deviceNameCharacteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
            return Encoding.UTF8.GetString(deviceNameReadResult.Value.ToArray());
        }

        public async Task<string> GetSerialNumber()
        {
            var serialNumberCharacteristic = await Gatt.GetCharacteristicByServiceUuid(DEVICE_INFO_SERVICE, SERIAL_NUMBER_CHARACTERISTIC);
            GattReadResult serialNumberReadResult = await serialNumberCharacteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
            return Encoding.UTF8.GetString(serialNumberReadResult.Value.ToArray());
        }

        public async Task<string> GetHardwareRevision()
        {
            var hardwareRevisionCharacteristic = await Gatt.GetCharacteristicByServiceUuid(DEVICE_INFO_SERVICE, HARDWARE_REVISION_CHARACTERISTIC);
            GattReadResult hardwareRevisionReadResult = await hardwareRevisionCharacteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
            return Encoding.UTF8.GetString(hardwareRevisionReadResult.Value.ToArray());
        }

        public async Task<string> GetSoftwareRevision()
        {
            var softwareRevisionCharacteristic = await Gatt.GetCharacteristicByServiceUuid(DEVICE_INFO_SERVICE, SOFTWARE_REVISION_CHARACTERISTIC);
            GattReadResult softwareRevisionReadResult = await softwareRevisionCharacteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
            return Encoding.UTF8.GetString(softwareRevisionReadResult.Value.ToArray());
        }
    }
}
