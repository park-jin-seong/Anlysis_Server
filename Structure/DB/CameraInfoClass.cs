using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analysis_Server.Structure.DB
{
    public class CameraInfoClass
    {
        public int m_idx { get; }
        public string m_videoSourceId { get; }
        public string m_cameraName { get; }
        public string m_rtspUrl { get; }
        public double m_coordx { get; }
        public double m_coordy { get; }

        public CameraInfoClass(int idx, string videoSourceId, string cameraName, string rtspUrl, double coordx, double coordy)
        {
            m_idx = idx;
            m_videoSourceId = videoSourceId;
            m_cameraName = cameraName;
            m_rtspUrl = rtspUrl;
            m_coordx = coordx;
            m_coordy = coordy;
        }
    }
}
