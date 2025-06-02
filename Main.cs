using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BluetoothCore;
using Windows.ApplicationModel.Store.Preview;
using Windows.Devices.Bluetooth;
using Windows.Media.Core;


namespace DGLABAPP
{
    class Program
    {
        static async Task Main(string[] args)
        {
            BluetoothCore.BluetoothCore bluetoothCore = new BluetoothCore.BluetoothCore();
            Console.WriteLine("蓝牙设备扫描已开始...");
            bluetoothCore.StartScanBlueDevices();
            Thread.Sleep(500);

            bluetoothCore.StopScanBlueDevices();
            Console.WriteLine("蓝牙设备扫描已停止...");

            List<BluetoothLEDevice> devices = bluetoothCore.GetDevices(); // 获取扫描到的蓝牙设备列表
            Console.WriteLine(devices.Count);
            foreach (var i in devices)
            {
                Console.WriteLine($"设备名称: {i.Name}, 地址: {i.BluetoothAddress}, 连接状态: {i.ConnectionStatus}");
            }
            
            bluetoothCore.CurrentBluetoothDevice(); // 替换为实际的设备名称
            DevicesInfo devicesInfo = bluetoothCore.GetDevicesInfo();
            await bluetoothCore.ConnectBluetoothDevices(); // 等待连接完成
            for(int i = 0; i < 10; i++) 
            {
                //Console.WriteLine($"发送数据: {BitConverter.ToString(data).Replace("-", " ")}");
                //await bluetoothCore.WriteCharacteristics("b01401000a0a0a0a2828282800000000000000ff");
                //Thread.Sleep(300); // 等待1秒
            }
            for (int i = 0; i < 100; i++)
            {
                //Console.WriteLine($"发送数据: {BitConverter.ToString(data).Replace("-", " ")}");
                await bluetoothCore.WriteCharacteristics("b011000100000000000000ff0a0a0a0a64646464");
                Thread.Sleep(300); // 等待1秒
            }
            bluetoothCore.CloseBluetoothDevice(); // 释放资源
            Console.WriteLine($"\n\n\n报告如下: \nNAME：{devicesInfo.Name},\nADDRESS：{devicesInfo.Address},\nConnectionStatus：{devicesInfo.ConnectionStatus}");
        }

        private static void Result_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            throw new NotImplementedException();
        }
    }
}
