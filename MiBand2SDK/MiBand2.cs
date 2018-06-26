using MiBand2SDK.Components;
using MiBand2SDK.Utils;
using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;

namespace MiBand2SDK
{
    public class MiBand2
    {
        public Identity Identity = new Identity();
        public Battery Battery = new Battery();
        public Device Device = new Device();
        public Display Display = new Display();
        public Activity Activity = new Activity();
        public HeartRate HeartRate = new HeartRate();
        public WearLocation WearLocation = new WearLocation();
        public Notifications Notifications = new Notifications();
        
        /// <summary>
        /// Connect to paired device
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ConnectAsync()
        {
            Identity auth = new Identity();
            DeviceInformation device = await auth.GetPairedBand();

            if (device != null)
            {
                Gatt.bluetoothLEDevice = await BluetoothLEDevice.FromIdAsync(device.Id);
                return Gatt.bluetoothLEDevice != null;
            }

            return false;
        }

        /// <summary>
        /// Connect to device by id
        /// </summary>
        /// <param name="deviceInfo"></param>
        /// <returns></returns>
        public async Task<bool> ConnectAsync(DeviceInformation deviceInfo)
        {
            if (deviceInfo != null)
            {
                Gatt.bluetoothLEDevice = await BluetoothLEDevice.FromIdAsync(deviceInfo.Id);
                return Gatt.bluetoothLEDevice != null;
            }

            return false;
        }

        /// <summary>
        /// Unpair band from device
        /// </summary>
        public async Task UnpairDeviceAsync()
        {
            if (Gatt.bluetoothLEDevice != null)
                await Gatt.bluetoothLEDevice.DeviceInformation.Pairing.UnpairAsync();
        }

        public bool IsConnected() => Gatt.bluetoothLEDevice != null && Gatt.bluetoothLEDevice.ConnectionStatus == BluetoothConnectionStatus.Connected;
    }
}
