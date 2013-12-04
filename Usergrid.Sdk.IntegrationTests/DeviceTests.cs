using NUnit.Framework;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk.IntegrationTests {
    [TestFixture]
    public class DeviceTests : BaseTest {
        public class MyCustomUserGridDevice : UsergridDevice {
            public string DeviceType { get; set; }
        }

        [Test]
        public async void ShouldCrudDevices() {
            const string deviceName = "test_device2";
            IClient client = await InitializeClientAndLogin(AuthType.Organization);
            await DeleteDeviceIfExists(client, deviceName);

            await client.CreateDevice(new UsergridDevice {Name = deviceName});
            //get device and assert
            UsergridDevice device = await client.GetDevice<UsergridDevice>(deviceName);
            Assert.That(device.Name, Is.EqualTo(deviceName));

            //create a custom device
            await DeleteDeviceIfExists(client, deviceName);
            const string deviceTypeiPhone = "iPhone";

            await client.CreateDevice(new MyCustomUserGridDevice {Name = deviceName, DeviceType = deviceTypeiPhone});
            //get device and assert
            MyCustomUserGridDevice myCustomDevice = await client.GetDevice<MyCustomUserGridDevice>(deviceName);
            Assert.That(myCustomDevice.Name, Is.EqualTo(deviceName));
            Assert.That(myCustomDevice.DeviceType, Is.EqualTo(deviceTypeiPhone));

            //update device type
            const string deviceTypeAndroid = "Android";

            myCustomDevice.DeviceType = deviceTypeAndroid;
            await client.UpdateDevice(myCustomDevice);

            //get device and assert
            myCustomDevice = await client.GetDevice<MyCustomUserGridDevice>(deviceName);
            Assert.That(myCustomDevice.Name, Is.EqualTo(deviceName));
            Assert.That(myCustomDevice.DeviceType, Is.EqualTo(deviceTypeAndroid));
        }
    }
}