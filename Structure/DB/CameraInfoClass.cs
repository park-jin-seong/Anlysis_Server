using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analysis_Server.Structure.DB
{
    public class CameraInfoClass
    {
        public int cameraId { get; }
        public string cameraName { get; }
        public string cctvUrl { get; }
        public float coordx { get; }
        public float coordy { get; }
        public bool isAnalisis { get; }

        public CameraInfoClass(int cameraId, string cameraName, string cctvUrl, float coordx, float coordy, bool isAnalisis)
        {
            this.cameraId = cameraId;
            this.cameraName = cameraName;
            this.cctvUrl = cctvUrl;
            this.coordx = coordx;
            this.coordy = coordy;
            this.isAnalisis = isAnalisis;
        }
    }
}
