# Mi Band 2 SDK

SDK for pairing and interacting with Mi Band 2 via .NET and UWP.

## Project Description
This project was written to interact Mi Band 2 from UWP applications.

Used GATT services and characteristics is unofficial and taken from projects like:

[FitForMiBand for UWP](https://github.com/superhans205/FitForMiBand)

[Gadgetbridge](https://github.com/Freeyourgadget/Gadgetbridge)

[Mi Band GATT UUID's](http://jellygom.com/2016/09/30/Mi-Band-UUID.html)

## System Requirements

You need installed Visual Studio with UWP on the board.

Minimum version: Windows 10.0.10586

# How to use

**To avoid unhandled exceptions, first connect and pair (authenticate) Application with Mi Band 2.**

## Connect to Band

```C#
// First initialize MiBand2 object to interact with Band.
MiBand2 band = new MiBand2;
```

First you need find and connect device in Windows 10 manually.
When device was connected, you need connect and pair it with application programmatically.
You need always connect your application to band during application launch.

```C#
// Check if connection was established
if (await band.ConnectAsync())
{
    // Do some stuff...
}
```

## First authentication
```C#
// Trying to connect to band and authenticate first time.
// While authentication process started, you need to touch you band, when you see the message.
if (await band.ConnectAsync() && await band.Identity.AuthenticateAsync())
{
    // Do some stuff...
}
```

## Subsequent use
You don't need make authentication process during the first launch.
You can just check is application already authenticated with band and continue use sdk.

```C#
// Check if already authentified
if (await band.ConnectAsync() && await band.Identity.IsAuthenticated())
{
   // Do some stuff...    
}
```

## Battery

Battery property returns [BatteryState](https://github.com/AL3X1/Mi-Band-2-SDK/blob/master/MiBand2SDK/Models/BatteryState.cs) object. 
You can take charge level, total cycles of charging, last charging date, and check is device charging right now.

To get battery status, use this steps:

```C#
if (await band.ConnectAsync() && await band.Identity.IsAuthenticated())
{
    // Get all BatteryState 
    BatteryState batteryInfo = await band.Battery.GetCurrentBatteryState();

    // Get the charge level
    int chargeLevel = batteryInfo.ChargeLevel;

    // Get the charging status
    bool isCharging = batteryInfo.IsCharging;

    // Get the last charge date
    DateTime lastCharge = batteryInfo.LastCharge;
}
```

If you need only one property, you can take this value from property getter.
```C#
// Get the charge level
int chargeLevel = await band.Battery.GetBatteryChargeLevel();

// Get the charging status
bool isCharging = await band.Battery.IsDeviceCharging();

// Get the last charge date
DateTime lastCharge = await band.Battery.GetLastChargingDate();
```

## Heart rate

To start simple heart rate measurement:
```C#
int heartRate = await band.HeartRate.GetHeartRateAsync();
```

If you need to work with heart rate in background and catch the measurement events, you need to create your background task, then connect to Band from background task. Look at this sample:

```C#
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace BackgroundTasks
{
    public sealed class CheckHeartRateInBackgroundTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _defferal;
        private static MiBand2SDK.MiBand2 band = new MiBand2SDK.MiBand2();

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _defferal = taskInstance.GetDeferral();

            if (await band.ConnectAsync())
            {
                // You will receive heart rate measurements from the band
                await band.HeartRate.SubscribeToHeartRateNotificationsAsync((sender, args) =>
                {
                    int currentHeartRate = args.CharacteristicValue.ToArray()[1];
                    System.Diagnostics.Debug.WriteLine($"Current heartrate from background task is {currentHeartRate} bpm ");
                });
            }  
        }

        public static async void RegisterAndRunAsync()
        {
            var taskName = typeof(CheckHeartRateInBackgroundTask).Name;
            IBackgroundTaskRegistration checkHeartRateInBackground = BackgroundTaskRegistration.AllTasks.Values.FirstOrDefault(t => t.Name == taskName);

            if (band.IsConnected() && band.Identity.IsAuthenticated())
            {
                if (checkHeartRateInBackground != null)
                    checkHeartRateInBackground.Unregister(true);

                var deviceTrigger = new DeviceUseTrigger();
                var deviceInfo = await band.Identity.GetPairedBand();

                var taskBuilder = new BackgroundTaskBuilder
                {
                    Name = taskName,
                    TaskEntryPoint = typeof(CheckHeartRateInBackgroundTask).ToString(),
                    IsNetworkRequested = false
                };

                taskBuilder.SetTrigger(deviceTrigger);
                BackgroundTaskRegistration task = taskBuilder.Register();

                await deviceTrigger.RequestAsync(deviceInfo.Id);
            }
        }
    }
}
```

Then just run it in some class:
```C#
BackgroundTasks.CheckHeartRateInBackgroundTask.RegisterAndRunAsync();
```

Also, if you need to take only value from last heart rate measurement after background task or method call, you can do this:
```C#
int lastHeartRate = band.HeartRate.LastHeartRate;
```
