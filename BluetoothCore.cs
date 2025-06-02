using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Media.Devices.Core;
using Windows.Media.Playback;
using Windows.Storage.Streams;
using System.Diagnostics;
using System.Xml.Linq;

namespace BluetoothCore
{

    ///<summary
    /// 设备服务与信息绑定数据类型
    /// </summary>

    /// <summary>
    /// 设备信息类
    /// </summary>
    public class DevicesInfo
    {
        public string Name = string.Empty;
        public string Address = string.Empty;
        public string ConnectionStatus = string.Empty;
    }
    internal class BluetoothCore
    {

        ///<summary>
        ///选中的蓝牙设备
        ///</summary>
        BluetoothLEDevice ?currentbluetoothDevice;
        ///<summary>
        ///波形写入特征对象
        ///</summary>
        GattCharacteristic ?gattCharacteristic;

        ///<summary>
        ///蓝牙服务对象
        ///</summary>
        List<GattDeviceService> ?gattDeviceService;

        ///<summary>
        ///蓝牙扫描器实例
        ///</summary>
        BluetoothLEAdvertisementWatcher watcher = new BluetoothLEAdvertisementWatcher();

        ///<summary>
        ///蓝牙设备列表
        ///</summary>
        List<BluetoothLEDevice> Devices = new List<BluetoothLEDevice>();
        List<ulong> DevicesAddress = new List<ulong>();
        List<string> DeviceNames = new List<string>();

        /// <summary>
        /// 开始搜索蓝牙设备
        /// </summary>
        public void StartScanBlueDevices()
        {
            Devices.Clear(); //清空设备列表
            watcher.ScanningMode = BluetoothLEScanningMode.Active; //设置扫描模式为主动扫描

            watcher.Received += async (w, args) =>
            {
                try
                {
                    BluetoothLEDevice device = await BluetoothLEDevice.FromBluetoothAddressAsync(args.BluetoothAddress);
                    if (device != null && !DevicesAddress.Contains(device.BluetoothAddress))
                    {
                        DevicesAddress.Add(device.BluetoothAddress); // 添加设备地址到列表
                        Devices.Add(device); // dispatcher炸掉
                        DeviceNames.Add(device.Name); // 添加设备名称到列表
                        Console.WriteLine($"Added new device: {device.Name} ({device.BluetoothAddress})");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting device: {ex.Message}");
                }
            };
            watcher.Start();//开始扫描蓝牙设备
        }

        /// <summary>
        /// 停止扫描蓝牙设备
        /// </summary>
        public void StopScanBlueDevices()
        {
            watcher.Stop();//停止扫描
        }

        /// <summary>
        /// 连接蓝牙设备
        ///</summary>
        public bool CurrentBluetoothDevice(string DeviceName= "47L121000")
        {
            if (DeviceNames.IndexOf(DeviceName) == -1)
            {
                Console.WriteLine($"设备不在列表中！");
                return false; // 如果设备名称不在列表中，返回false
            }
            else
            {
                int t = DeviceNames.IndexOf(DeviceName); //获取设备名称在列表中的索引
                currentbluetoothDevice = Devices[t]; //根据索引获取设备实例
                return true; // 返回true表示设备已成功选择
                //return Devices[t];
            }
        }
        ///<summary>
        ///查询蓝牙设备服务
        ///</summary>
        public async Task ConnectBluetoothDevices()
        {
            if (currentbluetoothDevice == null)
            {
                Console.WriteLine($"请先选择一个蓝牙设备！");
                return;
            }
            GattDeviceServicesResult result = await currentbluetoothDevice.GetGattServicesAsync();
            if (result.Status == GattCommunicationStatus.Success)
            {
                var services = result.Services;
                foreach (var service in services)
                {
                    Console.WriteLine($"Service: {service.Uuid}");

                    GattCharacteristicsResult characteristicsResult = await service.GetCharacteristicsAsync();
                    var characteristics = characteristicsResult.Characteristics;
                    foreach (var Characteristics in characteristics)
                    {
                        Console.WriteLine($"Characteristic: {Characteristics.Uuid}, Properties: {Characteristics.CharacteristicProperties}");
                        if (Characteristics.Uuid == Guid.Parse("0000150a-0000-1000-8000-00805f9b34fb"))//获取0x150a特征
                        {
                            gattCharacteristic = Characteristics;//保存实例特征
                        }
                    }
                    if (service.Uuid == Guid.Parse("0000180c-0000-1000-8000-00805f9b34fb"))//获取0x150a的服务
                    {
                        gattDeviceService.Add(service);//保存服务实例
                    }
                    else
                    {
                        service.Dispose(); // 释放服务资源
                    }
                }
            }
        }
        /// <summary>
        /// 返回蓝牙设备列表
        ///</summary>
        public List<BluetoothLEDevice> GetDevices()
        {
            return Devices;
        }

        ///<summary>
        /// 获取设备电量
        /// </summary>

        ///<summary>
        ///蓝牙设备基本信息返回
        ///</summary>
        public DevicesInfo GetDevicesInfo()
        {
            DevicesInfo devicesInfo = new DevicesInfo();
            if (currentbluetoothDevice == null)
            {
                Console.WriteLine($"请先选择一个蓝牙设备！");
                return devicesInfo; // 返回空信息
            }
            devicesInfo.Name = currentbluetoothDevice.Name;
            devicesInfo.Address = currentbluetoothDevice.BluetoothAddress.ToString(); // 将地址转换为十六进制字符串
            devicesInfo.ConnectionStatus = currentbluetoothDevice.ConnectionStatus.ToString();
            return devicesInfo;
        }

        ///<summary>
        ///关闭蓝牙连接
        ///</summary>
        public void CloseBluetoothDevice()
        {
            if (gattDeviceService != null &&  currentbluetoothDevice != null)
            {
                for (int i = 0; i < gattDeviceService.Count()-1;i++)
                {
                    gattDeviceService[i].Dispose(); // 释放服务资源
                    currentbluetoothDevice.Dispose(); // 释放设备资源
                    Console.WriteLine($"释放完成");
                }
            }
            else
            {
                Console.WriteLine($"释放失败，蓝牙设备为null");
            }
        }

        ///<summary>
        ///写入通道信息
        ///</summary>
        public async Task WriteCharacteristics(string Rawdata)
        {
            byte[] data = Enumerable.Range(0, Rawdata.Length / 2)
            .Select(i => Convert.ToByte(Rawdata.Substring(i * 2, 2), 16))
            .ToArray(); //转换16进制字符串为字节数组
            DataWriter writer = new DataWriter();
            writer.WriteBytes(data);
            try
            {
                if (gattCharacteristic != null)
                {
                    var status = await gattCharacteristic.WriteValueAsync(writer.DetachBuffer(), GattWriteOption.WriteWithoutResponse);
                    if (status == GattCommunicationStatus.Success)
                    {
                        Console.WriteLine($"写入成功{Rawdata}");
                    }
                    else
                    {
                        Console.WriteLine($"写入失败，状态: {status}");
                    }
                }
                else
                {
                    Console.WriteLine($"蓝牙服务为NULL！写入失败");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"写入失败,错误代码:{ex}");
            }
            
        }
        /// <summary>
        /// 测试用途
        /// </summary>
        /// <returns></returns>
        public async Task Test()
        {
            if(gattDeviceService == null)
            {
                Console.WriteLine($"请先连接蓝牙设备！");
                return;
            }
            GattCharacteristicsResult characteristicsResult = await gattDeviceService.GetCharacteristicsAsync();
            var characteristics = characteristicsResult.Characteristics;
            foreach (var Characteristics in characteristics)
            {
                Console.WriteLine("AAAAA");
                Console.WriteLine($"Characteristic: {Characteristics.Uuid}, Properties: {Characteristics.CharacteristicProperties}");
            }
            gattDeviceService.Dispose(); // 释放服务资源
        }
    }
}
