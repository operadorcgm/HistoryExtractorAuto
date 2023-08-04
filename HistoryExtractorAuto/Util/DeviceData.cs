using System;
using System.Collections.Generic;
using System.Text;

namespace HistoryExtractorAuto.Util
{
    class DeviceData
    {
        public int idSensor { get; set; }
        public string nameDevice { get; set; }
        public string uptPercent { get; set; }
        public string dnTime { get; set; }
        public int idDevice { get; set; }

        public string avgPercent { get; set; }
        public int idSensorAvg { get; set; }
        public string nameSensorAvg { get; set; }

    }
}
