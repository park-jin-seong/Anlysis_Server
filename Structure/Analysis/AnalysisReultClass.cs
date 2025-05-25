using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analysis_Server.Structure.Analysis
{
    public class AnalysisReultClass
    {
        public float m_x { get; set; }
        public float m_y { get; set; }
        public float m_width { get; set; }
        public float m_height { get; set; }
        public float m_score { get; set; }
        public int m_classId { get; set; }

        public AnalysisReultClass(float x, float y, float width, float height, float score, int classId)
        {
            m_x = x;
            m_y = y;
            m_width = width;
            m_height = height;
            m_score = score;
            m_classId = classId;
        }
    }
}
