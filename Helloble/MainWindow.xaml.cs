using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Security.Cryptography;

namespace Helloble
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DeviceWatcher deviceWatcher;
        private ObservableCollection<BluetoothLEDeviceDisplay> KnownDevices = new ObservableCollection<BluetoothLEDeviceDisplay>();
        private List<DeviceInformation> UnknownDevices = new List<DeviceInformation>();
        private List<GattCharacteristic> connectedDeviceCharacteristics = new List<GattCharacteristic>();
        private DeviceInformation connectedDevice;
        private bool connected;
        GattDeviceService selectedService;
        private GattCharacteristic dataCharacteristics;
        private DeviceInformation last_device;
        private readonly object deviceListLock = new object();

        public MainWindow()
        {
            InitializeComponent();
            ListDevice.ItemsSource = KnownDevices;
        }

        private void StartBleDeviceWatcher()
        {
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected", "System.Devices.Aep.Bluetooth.Le.IsConnectable", "System.Devices.Aep.IsPresent", "System.Devices.Aep.SignalStrength" };

            string aqsAllBluetoothLEDevices = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\" AND System.Devices.Aep.IsPresent:=System.StructuredQueryType.Boolean#True AND System.ItemNameDisplay:~<\"Eko\")";
            
            deviceWatcher =
                    DeviceInformation.CreateWatcher(
                        aqsAllBluetoothLEDevices,
                        requestedProperties,
                        DeviceInformationKind.AssociationEndpoint);
            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            deviceWatcher.Removed += DeviceWatcher_Removed;
            deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
            deviceWatcher.Stopped += DeviceWatcher_Stopped;
            deviceWatcher.Start();
            
        }


        private void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            Console.Write(deviceInfo.Name.ToString() + " ");
            Console.Write("---IsPresent: " + deviceInfo.Properties["System.Devices.Aep.IsPresent"].ToString());
            Console.Write("---IsConnectable: " + deviceInfo.Properties["System.Devices.Aep.Bluetooth.Le.IsConnectable"]);
            Console.WriteLine("---RSSI = " + deviceInfo.Properties.Single(d => d.Key == "System.Devices.Aep.SignalStrength").Value);

            this.Dispatcher.Invoke(() =>
            {
                if ((bool)deviceInfo.Properties["System.Devices.Aep.Bluetooth.Le.IsConnectable"] )
                {
                    var ekoDevice = new BluetoothLEDeviceDisplay(deviceInfo);

                    if (KnownDevice(ekoDevice.Id.ToString()))
                    {
                        KnownDevices.Add(ekoDevice);
                    }
                }
            });
        }

        private void CustomOnPairingRequested(DeviceInformationCustomPairing sender, DevicePairingRequestedEventArgs args)
        {
            args.Accept();
        }

        private void PrintPacket(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            byte[] data;
            CryptographicBuffer.CopyToByteArray(args.CharacteristicValue, out data);
            
            for (int i = 0; i < data.Length; i += 2)
            {
                short raw = BitConverter.ToInt16(data, i);

                //short transformed = transformation(raw);

                data[i + 1] = (byte)(raw >> 8);
                data[i] = (byte)(raw & 0xFF);
            }

            foreach( var i in data)
            {
                Console.Write(i);
            }

            Console.WriteLine("--");
        }

        private short Transformation(short s)
        {
            return (short)1;// _filter.FilterSample(s);
        }

        private void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            if (sender == deviceWatcher)
            {
                    this.Dispatcher.Invoke(async () =>
                    {
                        BluetoothLEDeviceDisplay bleDeviceDisplay = FindBluetoothLEDeviceDisplay(deviceInfoUpdate.Id);
                        if (bleDeviceDisplay != null)
                        {
                            bleDeviceDisplay.Update(deviceInfoUpdate);
                            return;
                        }
                        else
                        {

                            var deviceInfo = await DeviceInformation.CreateFromIdAsync(deviceInfoUpdate.Id);
                            if ((bool)deviceInfo.Properties["System.Devices.Aep.Bluetooth.Le.IsConnectable"])
                            {
                                bleDeviceDisplay = new BluetoothLEDeviceDisplay(deviceInfo);
                                KnownDevices.Add(bleDeviceDisplay);
                            }
                            
                        }
                    });
                
            }
        }

        private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            if (sender == deviceWatcher)
            {
                this.Dispatcher.Invoke(() =>
                {
                    BluetoothLEDeviceDisplay bleDeviceDisplay = FindBluetoothLEDeviceDisplay(deviceInfoUpdate.Id);
                    if (bleDeviceDisplay != null)
                    {
                        KnownDevices.Remove(bleDeviceDisplay);
                    }
                });
            }
        }

        private void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object e)
        {
            if (sender == deviceWatcher)
            {
                Console.WriteLine("Done here");
            }
        }

        private void DeviceWatcher_Stopped(DeviceWatcher sender, object e)
        {
            if (sender == deviceWatcher)
            {
                Console.WriteLine("Done here");
            }
        }

        private void StopBleDeviceWatcher()
        {
            if (deviceWatcher != null)
            {
                // Unregister the event handlers.
                deviceWatcher.Added -= DeviceWatcher_Added;
                deviceWatcher.Updated -= DeviceWatcher_Updated;
                deviceWatcher.Removed -= DeviceWatcher_Removed;
                deviceWatcher.EnumerationCompleted -= DeviceWatcher_EnumerationCompleted;
                deviceWatcher.Stopped -= DeviceWatcher_Stopped;

                // Stop the watcher.
                deviceWatcher.Stop();
                deviceWatcher = null;
            }
        }

        private BluetoothLEDeviceDisplay FindBluetoothLEDeviceDisplay(string id)
        {
            foreach (BluetoothLEDeviceDisplay bleDeviceDisplay in KnownDevices)
            {
                if (bleDeviceDisplay.Id == id)
                {
                    return bleDeviceDisplay;
                }
            }
            return null;
        }

        private Boolean KnownDevice(string id)
        {
            foreach (var device in KnownDevices)
            {
                if (device.Id == id)
                {
                    return false;
                } 
            }
            return true;
        }

        private Boolean FindUnkownDevices(DeviceInformation Unkowndevice)
        {
            if (UnknownDevices.Contains(Unkowndevice))
            {
                return false;
            }
            else
            {
                UnknownDevices.Add(Unkowndevice);
                return true;
            }
        }

        private void Enumerate_Click(object sender, RoutedEventArgs e)
        {
            if (deviceWatcher == null && Enumerate.Content.ToString() != "Stop enumerating")
            {
                StartEnumeration();
            } else
            {
                StopEnumeration();
            }
        }

        private void StartEnumeration()
        {
            //KnownDevices.Clear();
            StartBleDeviceWatcher();
            Enumerate.Content = "Stop enumerating";
        }

        private void StopEnumeration()
        {
            StopBleDeviceWatcher();
            Enumerate.Content = "Start";
        }

        private async void ListDevice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine(ListDevice.SelectedItem);
            StopEnumeration();

            if (connected)
            {
                Disconnect_device();
            }

            var selected = (BluetoothLEDeviceDisplay)ListDevice.SelectedItem;
            foreach (var i in KnownDevices)
            {
                if (i == selected)
                {
                    Console.WriteLine(i.Id);
                    await GetServicesAsync(i);
                    if (connected == false)
                    {
                        Console.WriteLine(i.Id);
                        await GetServicesAsync(i);
                    }
                }
            }
            if (!connected)
            {
                ListDevice.SelectedItem = null;
            }
        }

        private async Task GetServicesAsync(BluetoothLEDeviceDisplay device)
        {
            DeviceInformation deviceInfo = await DeviceInformation.CreateFromIdAsync(device.Id);
            deviceInfo.Pairing.Custom.PairingRequested += CustomOnPairingRequested;
            var result = await deviceInfo.Pairing.Custom.PairAsync(DevicePairingKinds.ConfirmOnly, DevicePairingProtectionLevel.Encryption);

            if (result.Status.ToString() == "Paired" || result.Status.ToString() == "AlreadyPaired" || result.Status.ToString() == "OperationAlreadyInProgress")
            {
                connectedDevice = deviceInfo;
                var pairedDevice = await BluetoothLEDevice.FromIdAsync(deviceInfo.Id);
                GattDeviceServicesResult services;
                try
                {
                    services = await pairedDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);
                }
                catch
                {
                    services = await pairedDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);
                }

                if (services.Status == GattCommunicationStatus.Success)
                {
                    foreach (var service in services.Services)
                    {
                        ListServices.Items.Add(service);                   
                    }
                    connected = true;
                    last_device = deviceInfo;

                }
                
            } else
            {
                connected = false;
            }
        }

        private async void ListServices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (connected)
            {
                ListCharacteristics.Items.Clear();
                selectedService = ListServices.SelectedItem as GattDeviceService;
                var characteristics = await selectedService.GetCharacteristicsAsync();
                foreach (var charact in characteristics.Characteristics)
                {
                    ListCharacteristics.Items.Add(charact);
                    
                    connectedDeviceCharacteristics.Add(charact);
                }
            }
        }

        private async void ListCharacteristics_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (connected)
            {
                foreach (var i in connectedDeviceCharacteristics)
                {
                    if (i.Uuid.ToString() != sender.ToString() && "ba9c5360-9999-11e3-966f-0002a5d5c51b" != sender.ToString())
                    {
                        var yt = await i.WriteClientCharacteristicConfigurationDescriptorWithResultAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                        var it = await i.GetDescriptorsAsync(BluetoothCacheMode.Uncached);
                        dataCharacteristics = i;
                        dataCharacteristics.ValueChanged += PrintPacket;
                    }
                }
            }
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            Disconnect_device();
        }

        private void Disconnect_device()
        {
            if (connected == true)
            {
                if (dataCharacteristics != null)
                {
                    dataCharacteristics.ValueChanged -= PrintPacket;
                    dataCharacteristics = null;
                }

                foreach (var i in ListServices.Items)
                {
                    GattDeviceService service = (GattDeviceService)i;
                    service.Dispose();
                    service = null;
                }

                connectedDevice = null;
                connected = false;
                ListServices.Items.Clear();
                ListCharacteristics.Items.Clear();
                connectedDeviceCharacteristics.Clear();
                //KnownDevices.Clear();
                GC.Collect();
                StartEnumeration();
                StopEnumeration();
            }
            ListDevice.SelectedItem = null;
            StartEnumeration();
        }

    }


    public class DeviceList : ObservableCollection<BluetoothLEDeviceDisplay>
    {
        public DeviceList() : base()
        {

        }
    }

    public class BluetoothLEDeviceDisplay
    {
        public BluetoothLEDeviceDisplay(DeviceInformation deviceInfoIn)
        {
            DeviceInformation = deviceInfoIn;
        }

        public DeviceInformation DeviceInformation { get; private set; }

        public string Id => DeviceInformation.Id;
        public string Name => DeviceInformation.Name;
        public bool IsPaired => DeviceInformation.Pairing.IsPaired;
        public bool IsConnected => (bool?)DeviceInformation.Properties["System.Devices.Aep.IsConnected"] == true;
        public bool IsConnectable => (bool?)DeviceInformation.Properties["System.Devices.Aep.Bluetooth.Le.IsConnectable"] == true;
        public string LongName => Name.ToString() + ": " + Id.ToString().Remove(0, Id.Length - 4);

        public IReadOnlyDictionary<string, object> Properties => DeviceInformation.Properties;

        public void Update(DeviceInformationUpdate deviceInfoUpdate)
        {
            DeviceInformation.Update(deviceInfoUpdate);

            OnPropertyChanged("Id");
            OnPropertyChanged("Name");
            OnPropertyChanged("DeviceInformation");
            OnPropertyChanged("IsPaired");
            OnPropertyChanged("IsConnected");
            OnPropertyChanged("Properties");
            OnPropertyChanged("IsConnectable");
        }

        protected void OnPropertyChanged(string name)
        {
            Console.Write(name);
        }

        public override string ToString()
        {
            return Name.ToString() + ": " + Id.ToString().Remove(0, Id.Length - 4);
        }
    }

}
