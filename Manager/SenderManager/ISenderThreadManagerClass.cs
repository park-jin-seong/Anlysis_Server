using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Analysis_Server.Structure.Analysis;

namespace Analysis_Server.Manager.SenderManager
{
    public interface ISenderThreadManagerClass
    {
        void AddAnlysisResult(int cameraId, List<AnalysisReultClass> result);
    }
}
