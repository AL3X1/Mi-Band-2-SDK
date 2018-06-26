using MiBand2SDK.Enums;
using MiBand2SDK.Models;
using MiBand2SDK.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace MiBand2SDK.Components
{
    public class Notifications
    {
        private Guid ALERT_LEVEL_SERVICE = new Guid("00001802-0000-1000-8000-00805f9b34fb");
        private Guid NOTIFICATION_SERVICE = new Guid("00001811-0000-1000-8000-00805F9B34FB");
        private Guid ALERT_LEVEL_CHARACTERISTIC = new Guid("00002a06-0000-1000-8000-00805f9b34fb");
        private Guid SEND_ALERT_CHARACTERISTIC = new Guid("00002a46-0000-1000-8000-00805f9b34fb");
        private Guid MIBAND2_SERVICE = new Guid("0000fee0-0000-1000-8000-00805f9b34fb");
        private Guid CONFIGURATION_CHARACTERISTIC = new Guid("00000003-0000-3512-2118-0009af100700");


        /// <summary>
        /// Set fithess goal notification on the Band
        /// </summary>
        /// <param name="notification"></param>
        /// <returns></returns>
        public async Task<bool> SetFitnessGoalNotification(FitnessGoalNotification notification)
        {
            GattCharacteristic notificationCharacteristic = await Gatt.GetCharacteristicByServiceUuid(MIBAND2_SERVICE, CONFIGURATION_CHARACTERISTIC);
            byte[] setNotificationCmd = new byte[] { 6, 0, (byte)notification };

            return await notificationCharacteristic.WriteValueAsync(setNotificationCmd.AsBuffer()) == GattCommunicationStatus.Success;
        }

        /// <summary>
        /// Setting display caller information while ringing.
        /// </summary>
        /// <param name="callerInfo"></param>
        /// <returns></returns>
        public async Task<bool> SetDisplayCallerInfo(DisplayCallerInfo callerInfo)
        {
            GattCharacteristic notificationCharacteristic = await Gatt.GetCharacteristicByServiceUuid(MIBAND2_SERVICE, CONFIGURATION_CHARACTERISTIC);
            byte[] setDisplayCallerInfoCmd = new byte[] { 6, 16, 0, (byte)callerInfo };

            return await notificationCharacteristic.WriteValueAsync(setDisplayCallerInfoCmd.AsBuffer()) == GattCommunicationStatus.Success;
        }

        /// <summary>
        /// Send default notification to the Band
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<bool> SendDefaultNotification(NotificationType type)
        {
            GattCharacteristic notificationCharacteristic = await Gatt.GetCharacteristicByServiceUuid(ALERT_LEVEL_SERVICE, ALERT_LEVEL_CHARACTERISTIC);
            byte[] sendNotificationCmd = new byte[] { (byte)type };

            return await notificationCharacteristic.WriteValueAsync(sendNotificationCmd.AsBuffer()) == GattCommunicationStatus.Success;
        }

        /// <summary>
        /// Send custom notification to the Band
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="times"></param>
        /// <returns></returns>
        public async Task<bool> SendCustomNotification(CustomVibrationProfile profile, short times)
        {
            GattCharacteristic notificationCharacteristic = await Gatt.GetCharacteristicByServiceUuid(ALERT_LEVEL_SERVICE, ALERT_LEVEL_CHARACTERISTIC);
            int[] onOffSequence = null;

            switch (profile)
            {
                case CustomVibrationProfile.INFINITE:
                    onOffSequence = new int[] { 1, 1 };
                    break;

                case CustomVibrationProfile.LONG:
                    onOffSequence = new int[] { 500, 1000 };
                    break;

                case CustomVibrationProfile.QUICK:
                    onOffSequence = new int[] { 100, 100 };
                    break;

                case CustomVibrationProfile.SHORT:
                    onOffSequence = new int[] { 200, 200 };
                    break;

                case CustomVibrationProfile.WATER_DROP:
                    onOffSequence = new int[] { 100, 1500 };
                    break;

                case CustomVibrationProfile.RING:
                    onOffSequence = new int[] { 200, 300 };
                    break;
            }

            if (notificationCharacteristic != null && onOffSequence != null)
            {
                short? vibration = (short) onOffSequence[0];
                short? pause = (short) onOffSequence[1];
                byte repeat = (byte)(times * (onOffSequence.Length / 2));
                byte[] sendNotificationCmd = new byte[] { unchecked((byte)-1), (byte)(vibration & 255), (byte)((vibration >> 8) & 255), (byte)(pause & 255), (byte)((pause >> 8) & 255), repeat };

                System.Diagnostics.Debug.WriteLine($"Starting vibrate {repeat} times");
                return await notificationCharacteristic.WriteValueAsync(sendNotificationCmd.AsBuffer()) == GattCommunicationStatus.Success;
            }

            return false;
        }

        /// <summary>
        /// Send more customizable notification to the Band.
        /// </summary>
        /// <param name="vibration">Strength and length of notification</param>
        /// <param name="pause"></param>
        /// <param name="times"></param>
        /// <returns></returns>
        public async Task<bool> SendCustomNotification(short vibration, short pause, short times)
        {
            GattCharacteristic notificationCharacteristic = await Gatt.GetCharacteristicByServiceUuid(ALERT_LEVEL_SERVICE, ALERT_LEVEL_CHARACTERISTIC);

            if (notificationCharacteristic != null)
            {
                byte repeat = (byte)(times * (2 / 2));
                byte[] sendNotificationCmd = new byte[] { unchecked((byte)-1), (byte)(vibration & 255), (byte)((vibration >> 8) & 255), (byte)(pause & 255), (byte)((pause >> 8) & 255), repeat };

                System.Diagnostics.Debug.WriteLine($"Starting vibrate {repeat} times");
                return await notificationCharacteristic.WriteValueAsync(sendNotificationCmd.AsBuffer()) == GattCommunicationStatus.Success;
            }

            return false;
        }

        /// <summary>
        /// Set alarm clock for Band.
        /// </summary>
        /// <param name="status"></param>
        /// <param name="alarmSlot"></param>
        /// <param name="alarmDays"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        public async Task<bool> SetAlarmClock(AlarmStatus status, int alarmSlot, List<AlarmDays> alarmDays, int hour, int minute)
        {
            GattCharacteristic notificationCharacteristic = await Gatt.GetCharacteristicByServiceUuid(MIBAND2_SERVICE, CONFIGURATION_CHARACTERISTIC);
            int maxAlarmSlots = 5;
            int days = 0;

            if (alarmSlot >= maxAlarmSlots)
            {
                Debug.WriteLine("Only 5 slots for alarms available. Saving to 0 slot.");
                alarmSlot = 0;
            }

            if (alarmDays.Count == 0)
            {
                Debug.WriteLine("alarmDays is empty. Setting alarm once.");
                days = 128;
            }
            else
            {
                foreach (var day in alarmDays)
                    days += (int)day;
            }

            Debug.WriteLine($"Setting alarm clock at {hour}:{minute} to slot {alarmSlot}");
            byte[] setAlarmCmd = new byte[] { 0x2, (byte)(status + alarmSlot), (byte)hour, (byte)minute, (byte)days };

            return await notificationCharacteristic.WriteValueAsync(setAlarmCmd.AsBuffer()) == GattCommunicationStatus.Success;
        }

        /// <summary>
        /// Set Do Not Disturb mode. (Unsubscribe to notifications from phone)
        /// </summary>
        /// <param name="dndMode"></param>
        /// <param name="startHours"></param>
        /// <param name="startMinutes"></param>
        /// <param name="endHours"></param>
        /// <param name="endMinutes"></param>
        /// <returns></returns>
        public async Task<bool> DoNotDisturb(DoNotDisturbMode dndMode, 
            byte startHours = 0, byte startMinutes = 0, 
            byte endHours = 0, byte endMinutes = 0)
        {
            var characteristic = await Gatt.GetCharacteristicByServiceUuid(MIBAND2_SERVICE, CONFIGURATION_CHARACTERISTIC);
            byte[] setDnd = null;

            if (dndMode != DoNotDisturbMode.SCHEDULED)
                setDnd = new byte[] { 9, (byte)dndMode };
            else
                setDnd = new byte[] { 9, (byte)dndMode, startHours, startMinutes, endHours, endMinutes };
            
            return await characteristic.WriteValueAsync(setDnd.AsBuffer()) == GattCommunicationStatus.Success;
        }

        /// <summary>
        /// Send notification when user is so lazy
        /// </summary>
        /// <param name="warningMode"></param>
        /// <param name="startHours"></param>
        /// <param name="startMinutes"></param>
        /// <param name="endHours"></param>
        /// <param name="endMinutes"></param>
        /// <returns></returns>
        public async Task<bool> SetInactivityWarnings(InactivityWarningMode warningMode,
            byte startHours, byte startMinutes,
            byte endHours, byte endMinutes)
        {
            var characteristic = await Gatt.GetCharacteristicByServiceUuid(MIBAND2_SERVICE, CONFIGURATION_CHARACTERISTIC);
            byte[] data = new byte[] { 8, (byte)warningMode, 60, 0, startHours, startMinutes, endHours, endMinutes, startHours, startMinutes, endHours, endMinutes };

            return await characteristic.WriteValueAsync(data.AsBuffer()) == GattCommunicationStatus.Success;
        }
    }
}
