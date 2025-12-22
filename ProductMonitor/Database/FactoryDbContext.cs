using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductMonitor.Database
{
    public static class FactoryDbContext
    {
        private const string ConnectionString = @"Server=localhost;Database=FactoryMonitor;Trusted_Connection=True;";

        /// <summary>
        /// 保存报警日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="triggerTime"></param>
        /// <param name="duration"></param>
        public static void SaveAlarm(string message,DateTime triggerTime,int duration)
        {
            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(@"
                        INSERT INTO AlarmLogs (Message, TriggerTime, Duration)
                        VALUES (@msg, @time, @dur)", conn))
                    {
                        cmd.Parameters.AddWithValue("@msg", message);
                        cmd.Parameters.AddWithValue("@time", triggerTime);
                        cmd.Parameters.AddWithValue("@dur", duration);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB] 保存报警失败：{ ex.Message}");
            }
        }

        public static void SaveDeviceSnapshot(int totalEnergy,int voltage,int speed,
            int pressure,int flowRate,int frequency,int power)
        {
            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open ();
                    using (var cmd = new SqlCommand(@"
                        INSERT INTO DeviceSnapshots 
                        (TotalEnergy, Voltage, Speed, Pressure, FlowRate, Frequency, Power)
                        VALUES (@e, @v, @s, @p, @f, @freq, @pow)", conn))
                    {
                        cmd.Parameters.AddWithValue("@e", totalEnergy);
                        cmd.Parameters.AddWithValue("@v", voltage);
                        cmd.Parameters.AddWithValue("@s", speed);
                        cmd.Parameters.AddWithValue("@p", pressure);
                        cmd.Parameters.AddWithValue("@f", flowRate);
                        cmd.Parameters.AddWithValue("@freq", frequency);
                        cmd.Parameters.AddWithValue("@pow", power);
                        cmd.ExecuteNonQuery();
                    }
                
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB] 保存快照失败：{ex.Message}");
            }
        }

    }
}
