using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace MiBand2SDK.Utils
{
    class Gatt
    {
        /// <summary>
        /// Property, responsive to connection with band via GATT protocol.
        /// If device is not connected to phone, property will be null.
        /// </summary>
        public static BluetoothLEDevice bluetoothLEDevice { get; set; }

        /// <summary>
        /// Get GATT characteristic by service UUID.
        /// Service UUID can be taken from unofficial MI Band 2 Protocol - http://jellygom.com/2016/09/30/Mi-Band-UUID.html
        /// </summary>
        /// <param name="serviceUuid">GATT Service UUID</param>
        /// <param name="characteristicUuid">GATT Characteristic UUID</param>
        /// <returns>GattCharacteristic object if characteristic is exists, else returns null</returns>
        public static async Task<GattCharacteristic> GetCharacteristicByServiceUuid(Guid serviceUuid, Guid characteristicUuid)
        {
            if (bluetoothLEDevice == null)
                throw new Exception("Cannot get characteristic from service: Device is disconnected.");

            GattDeviceServicesResult service = await bluetoothLEDevice.GetGattServicesForUuidAsync(serviceUuid);
            GattCharacteristicsResult currentCharacteristicResult = await service.Services[0].GetCharacteristicsForUuidAsync(characteristicUuid);
            GattCharacteristic characteristic;

            if (currentCharacteristicResult.Status == GattCommunicationStatus.AccessDenied || currentCharacteristicResult.Status == GattCommunicationStatus.ProtocolError)
            {
                System.Diagnostics.Debug.WriteLine($"Error while getting characteristic: {characteristicUuid.ToString()} - {currentCharacteristicResult.Status}");
                characteristic = null;
            }
            else
            {
                characteristic = currentCharacteristicResult.Characteristics[0];
            }

            return characteristic;
        }

        /// <summary>
        /// Get List of all characteristics from the specified service
        /// </summary>
        /// <param name="serviceUuid">GATT Service UUID</param>
        /// <returns></returns>
        public static async Task<GattCharacteristicsResult> GetAllCharacteristicsFromService(Guid serviceUuid)
        {
            if (bluetoothLEDevice == null)
                throw new Exception("Cannot get characteristic from service: Device is disconnected.");

            var service = await bluetoothLEDevice.GetGattServicesForUuidAsync(serviceUuid);
            return await service.Services[0].GetCharacteristicsAsync();
        }

        /// <summary>
        /// Get one service by his UUID
        /// </summary>
        /// <param name="serviceUuid"></param>
        /// <returns></returns>
        public static async Task<GattDeviceServicesResult> GetServiceByUuid(Guid serviceUuid)
        {
            if (bluetoothLEDevice == null)
                throw new Exception("Cannot get characteristic from service: Device is disconnected.");

            return await bluetoothLEDevice.GetGattServicesForUuidAsync(serviceUuid);
        }
    }
}