using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductMonitor.Entities
{
    public class AlarmLog
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime TriggerTime { get; set; }
        public int Duration { get; set; }
        
    }
}
