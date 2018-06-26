using MiBand2SDK.Enums;
using MiBand2SDK.Utils;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Foundation;

namespace MiBand2SDK.Components
{
    public class HeartRate
    {
        private static int lastHeartRate = 0;

        public int LastHeartRate
        {
            get { return lastHeartRate; }
        }

        private EventWaitHandle _WaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

        private Guid HEART_RATE_SERVICE = new Guid("0000180d-0000-1000-8000-00805f9b34fb");
        private Guid HEART_RATE_MEASUREMENT_CHARACTERISTIC = new Guid("00002a37-0000-1000-8000-00805f9b34fb");
        private Guid HEART_RATE_CONTROLPOINT_CHARACTERISTIC = new Guid("00002a39-0000-1000-8000-00805f9b34fb");
        private byte[] HEART_RATE_START_COMMAND = new byte[] { 21, 2, 1 };
        private GattCharacteristic _heartRateMeasurementCharacteristic;
        private GattCharacteristic _heartRateControlPointCharacteristic;

        /// <summary>
        /// Subscribe to HeartRate notifications from band.
        /// </summary>
        public async Task SubscribeToHeartRateNotificationsAsync()
        {
            _heartRateMeasurementCharacteristic = await Gatt.GetCharacteristicByServiceUuid(HEART_RATE_SERVICE, HEART_RATE_MEASUREMENT_CHARACTERISTIC);

            Debug.WriteLine("Subscribe to HeartRate notifications from band...");
            if (await _heartRateMeasurementCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify) == GattCommunicationStatus.Success)
                _heartRateMeasurementCharacteristic.ValueChanged += HeartRateMeasurementCharacteristicValueChanged;
        }

        /// <summary>
        /// Subscribe to HeartRate notifications from band.
        /// </summary>
        /// <param name="eventHandler">Handler for interact with heartRate values</param>
        /// <returns></returns>
        public async Task SubscribeToHeartRateNotificationsAsync(TypedEventHandler<GattCharacteristic, GattValueChangedEventArgs> eventHandler)
        {
            _heartRateMeasurementCharacteristic = await Gatt.GetCharacteristicByServiceUuid(HEART_RATE_SERVICE, HEART_RATE_MEASUREMENT_CHARACTERISTIC);

            Debug.WriteLine("Subscribe to HeartRate notifications from band...");
            if (await _heartRateMeasurementCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify) == GattCommunicationStatus.Success)
                _heartRateMeasurementCharacteristic.ValueChanged += eventHandler;
        }

        /// <summary>
        /// Measure current heart rate
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetHeartRateAsync()
        {
            int heartRate = 0;
            if (await StartHeartRateMeasurementAsync() == GattCommunicationStatus.Success)
               heartRate = lastHeartRate;

            return heartRate;
        }

        /// <summary>
        /// Starting heart rate measurement
        /// </summary>
        /// <returns></returns>
        private async Task<GattCommunicationStatus> StartHeartRateMeasurementAsync()
        {
            _heartRateMeasurementCharacteristic = await Gatt.GetCharacteristicByServiceUuid(HEART_RATE_SERVICE, HEART_RATE_MEASUREMENT_CHARACTERISTIC);
            _heartRateControlPointCharacteristic = await Gatt.GetCharacteristicByServiceUuid(HEART_RATE_SERVICE, HEART_RATE_CONTROLPOINT_CHARACTERISTIC);
            GattCommunicationStatus status = GattCommunicationStatus.ProtocolError;

            if (await _heartRateMeasurementCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify) == GattCommunicationStatus.Success)
            {
                Debug.WriteLine("Checking Heart Rate");

                if (await _heartRateControlPointCharacteristic.WriteValueAsync(HEART_RATE_START_COMMAND.AsBuffer()) == GattCommunicationStatus.Success)
                {
                    _heartRateMeasurementCharacteristic.ValueChanged += HeartRateMeasurementCharacteristicValueChanged;
                    status = GattCommunicationStatus.Success;
                    _WaitHandle.WaitOne();
                }
            }

            return status;
        }

        /// <summary>
        /// Handle incoming requests with heart rate from the band.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void HeartRateMeasurementCharacteristicValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            Debug.WriteLine("Getting HeartRate");
            if (sender.Uuid.ToString() == HEART_RATE_MEASUREMENT_CHARACTERISTIC.ToString())
                lastHeartRate = args.CharacteristicValue.ToArray()[1];

            System.Diagnostics.Debug.WriteLine($"HeartRate is {lastHeartRate} bpm");
            _WaitHandle.Set();
        }

        /// <summary>
        /// Set Heart Rate Measurements while sleep
        /// </summary>
        /// <param name="sleepMeasurement"></param>
        /// <returns></returns>
        public async Task<bool> SetHeartRateSleepMeasurement(SleepHeartRateMeasurement sleepMeasurement)
        {
            _heartRateControlPointCharacteristic = await Gatt.GetCharacteristicByServiceUuid(HEART_RATE_SERVICE, HEART_RATE_CONTROLPOINT_CHARACTERISTIC);
            byte[] command = null;

            switch (sleepMeasurement)
            {
                case SleepHeartRateMeasurement.ENABLE:
                    command = new byte[] { 0x15, 0x00, 0x01 };
                    break;

                case SleepHeartRateMeasurement.DISABLE:
                    command = new byte[] { 0x15, 0x00, 0x00 };
                    break;
            }

            return await _heartRateControlPointCharacteristic.WriteValueAsync(command.AsBuffer()) == GattCommunicationStatus.Success;
        }

        /// <summary>
        /// Set Realtime (Continuous) Heart Rate Measurements
        /// </summary>
        /// <param name="measurements"></param>
        /// <returns></returns>
        public async Task<GattCommunicationStatus> SetRealtimeHeartRateMeasurement(RealtimeHeartRateMeasurements measurements)
        {
            _heartRateMeasurementCharacteristic = await Gatt.GetCharacteristicByServiceUuid(HEART_RATE_SERVICE, HEART_RATE_MEASUREMENT_CHARACTERISTIC);
            _heartRateControlPointCharacteristic = await Gatt.GetCharacteristicByServiceUuid(HEART_RATE_SERVICE, HEART_RATE_CONTROLPOINT_CHARACTERISTIC);
            GattCommunicationStatus status = GattCommunicationStatus.ProtocolError;

            if (await _heartRateMeasurementCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify) == GattCommunicationStatus.Success)
            {
                byte[] manualCmd = null;
                byte[] continuousCmd = null;

                switch (measurements)
                {
                    case RealtimeHeartRateMeasurements.ENABLE:
                        manualCmd = new byte[] { 0x15, 0x02, 0 };
                        continuousCmd = new byte[] { 0x15, 0x01, 1 };
                        break;

                    case RealtimeHeartRateMeasurements.DISABLE:
                        manualCmd = new byte[] { 0x15, 0x02, 1 };
                        continuousCmd = new byte[] { 0x15, 0x01, 0 };
                        break;
                }

                if (await _heartRateControlPointCharacteristic.WriteValueAsync(manualCmd.AsBuffer()) == GattCommunicationStatus.Success
                    && await _heartRateControlPointCharacteristic.WriteValueAsync(continuousCmd.AsBuffer()) == GattCommunicationStatus.Success)
                {
                    status = GattCommunicationStatus.Success;
                    _heartRateMeasurementCharacteristic.ValueChanged += HeartRateMeasurementCharacteristicValueChanged;
                }
            }

            return status;
        }

        /// <summary>
        /// Sets Heart Rate Measurement interval in minutes (0 is off)
        /// </summary>
        /// <param name="minutes"></param>
        /// <returns></returns>
        public async Task<bool> SetHeartRateMeasurementInterval(int minutes)
        {
            _heartRateControlPointCharacteristic = await Gatt.GetCharacteristicByServiceUuid(HEART_RATE_SERVICE, HEART_RATE_CONTROLPOINT_CHARACTERISTIC);
            return await _heartRateControlPointCharacteristic.WriteValueAsync(new byte[] { 0x14, (byte)minutes }.AsBuffer()) == GattCommunicationStatus.Success;
        }
    }
}
