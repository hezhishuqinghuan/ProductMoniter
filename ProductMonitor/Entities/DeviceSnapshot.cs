using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductMonitor.Entities
{
    public class DeviceSnapshot
    {
        public int Id { get; set; }
        public int TotalEnergy { get; set; }      // 总电能(KW)
        public int Voltage { get; set; }          // 电压(V)
        public int Speed { get; set; }            // 转速(r/min)
        public int Pressure { get; set; }         // 气压(0.1 MPa)
        public int FlowRate { get; set; }         // 流量(m³/h)
        public int Frequency { get; set; }        // 频率(Hz)
        public int Power { get; set; }            // 功率(W)
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    }
}
