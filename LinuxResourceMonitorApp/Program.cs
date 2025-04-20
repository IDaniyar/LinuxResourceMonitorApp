using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using System.Threading;

namespace MyApp
{
    internal class Program
    {
        private static ulong _prevIdle, _prevTotal;
        public static void Main()
        {
            Console.WriteLine("=== Ubuntu Server Resource Monitor (From Windows) ===");

            while (true)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // CPU Usage (Linux)
                    var cpuUsage = getLinuxCpuUsage();
                    var memInfo = getLinuxMemoreInfo();
                    var diskInfo = GetLinuxDiskInfo();
                    Console.WriteLine($"CPU Usage: {cpuUsage:N1}%");
                    Console.WriteLine($"Memory: {memInfo.UsedGB:N1} GB / {memInfo.TotalGB:N1} GB");
                    Console.WriteLine($"Disk: {diskInfo.UsedGB:N1} GB / {diskInfo.TotalGB:N1} GB");


                }
                else
                {
                    Console.WriteLine("ERROR: This app requires Linux (Ubuntu 24.04)");
                    break;
                }
                Console.WriteLine(new string('-', 30));
                Thread.Sleep(2000);
            }
        }

        private static double getLinuxCpuUsage()
        {
            try
            {
                var lines = File.ReadAllLines("/proc/stat");
                var cpuLine = lines.First(l => l.StartsWith("cpu "));
                var values = cpuLine.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(ulong.Parse).ToArray();
                var idle = values[3];
                var total = values.Aggregate(0UL, (acc, val) => acc + val);
                var diffidle = idle - _prevIdle;
                var diffTotal = total - _prevTotal;

                _prevIdle = idle;
                _prevTotal = total;

                return 100.0 * (diffTotal - diffidle) / diffTotal;
            }
            catch { return -1; }
        }

        private static (double TotalGB, double UsedGB) getLinuxMemoreInfo()
        {
            var lines = File.ReadLines("/proc/meminfo");
            var memTotal = ulong.Parse(lines.First(l => l.StartsWith("MemTotal")).Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
            var memAvailable = ulong.Parse(lines.First(l => l.StartsWith("MemAvailable")).Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);

            return (memTotal / 1048576.0, (memTotal - memAvailable) / 1048576.0);
        }

        private static (double TotalGB, double UsedGB) GetLinuxDiskInfo()
        {
            try
            {
                var rootDrive = DriveInfo.GetDrives().First(d => d.Name == "/" && d.IsReady);

                return (rootDrive.TotalSize / 1073741824.0,
                    (rootDrive.TotalSize - rootDrive.AvailableFreeSpace) / 1073741824.0);
            }
            catch { return (0, 0); }
        }

    }
}