using Microsoft.EntityFrameworkCore;
using ProductMonitor.Entities;
using System;
using System.Threading.Tasks;

namespace ProductMonitor.Database;

public class FactoryDbContext : DbContext
{
    public DbSet<AlarmLog> AlarmLogs { get; set; }
    public DbSet<DeviceSnapshot> DeviceSnapshots { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            
            optionsBuilder.UseSqlServer(
                @"Server=localhost;Database=FactoryMonitor;Trusted_Connection=True;Encrypt=False;");
        }
    }

    /// <summary>
    /// 异步保存报警日志
    /// </summary>
    public static async Task SaveAlarmAsync(string message, DateTime triggerTime, int duration)
    {
        try
        {
            await using var context = new FactoryDbContext();
            context.AlarmLogs.Add(new AlarmLog
            {
                Message = message,
                TriggerTime = triggerTime,
                Duration = duration
            });
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[EF] 保存报警失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 异步保存设备快照
    /// </summary>
    public static async Task SaveDeviceSnapshotAsync(
        int totalEnergy, int voltage, int speed, int pressure,
        int flowRate, int frequency, int power)
    {
        try
        {
            await using var context = new FactoryDbContext();
            context.DeviceSnapshots.Add(new DeviceSnapshot
            {
                TotalEnergy = totalEnergy,
                Voltage = voltage,
                Speed = speed,
                Pressure = pressure,
                FlowRate = flowRate,
                Frequency = frequency,
                Power = power
            });
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[EF] 保存快照失败: {ex.Message}");
        }
    }
}