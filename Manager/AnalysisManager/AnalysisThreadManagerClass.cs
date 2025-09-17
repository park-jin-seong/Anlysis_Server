using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Analysis_Server.Manager.DBManager;
using Analysis_Server.Manager.SenderManager;
using Analysis_Server.Manager.SetupManager;
using Analysis_Server.Manager.SystemInfoManager;
using Analysis_Server.Structure.Analysis;
using Analysis_Server.Structure.DB;
using Analysis_Server.THD;
using Ninject;

namespace Analysis_Server.Manager.AnalysisManager
{
    public class AnalysisThreadManagerClass : IAnalysisThreadManagerClass
    {
        private List<AnalysisThreadClass> m_analysisThreadClasses;

        private ISystemInfoManagerClass m_systemInfoManagerClass;
        private ISenderThreadManagerClass m_senderThreadManagerClass;
        private IDBManagerClass m_dBManagerClass;
        public AnalysisThreadManagerClass(ISystemInfoManagerClass systemInfoManagerClass, IDBManagerClass dBManagerClass, ISenderThreadManagerClass senderThreadManagerClass)
        {
            m_analysisThreadClasses = new List<AnalysisThreadClass>();

            m_systemInfoManagerClass = systemInfoManagerClass;
            m_senderThreadManagerClass = senderThreadManagerClass;
            m_dBManagerClass = dBManagerClass;
        }

        public void SetAnlysisAndStart()
        {
            foreach (var item in m_dBManagerClass.GetCameraInfosClasses())
            {
                AddAnlysisThread(m_systemInfoManagerClass.GetSystemInfoClass().m_ModelPath, item);
            }
            StartAll();
        }

        public void SendReultData(int cameraId, List<AnalysisReultClass> result)
        {
            m_senderThreadManagerClass.AddAnlysisResult(cameraId, result);
        }
        private void AddAnlysisThread(string modelPath, CameraInfoClass cameraInfoClass)
        {
            m_analysisThreadClasses.Add(new AnalysisThreadClass(modelPath, 
                                                                cameraInfoClass.cameraId, 
                                                                cameraInfoClass.cameraName, 
                                                                cameraInfoClass.cctvUrl, 
                                                                cameraInfoClass.coordx, 
                                                                cameraInfoClass.coordy, 
                                                                cameraInfoClass.isAnalisis));
            m_analysisThreadClasses.Last<AnalysisThreadClass>().SetCallback(SendReultData);
        }
        private void StartAll()  
        {
            foreach(AnalysisThreadClass analysisThreadClass in m_analysisThreadClasses)
            {
                analysisThreadClass.Run();
            }
        }
        public bool PauseByVideoSourceId(string id)
        {
            foreach (AnalysisThreadClass analysisThreadClass in m_analysisThreadClasses)
            {
                if (analysisThreadClass.CheckVideoSourceId(id))
                {
                    analysisThreadClass.pause();
                    return true;
                }
            }
            return false;
        }
        public bool QuitByVideoSourceId(string id)
        {
            foreach (AnalysisThreadClass analysisThreadClass in m_analysisThreadClasses)
            {
                if (analysisThreadClass.CheckVideoSourceId(id))
                {
                    analysisThreadClass.quit();
                    m_analysisThreadClasses.Remove(analysisThreadClass);
                    return true;
                }
            }
            return false;
        }
        public bool RestartByVideoSourceId(string id)
        {
            foreach (AnalysisThreadClass analysisThreadClass in m_analysisThreadClasses)
            {
                if (analysisThreadClass.CheckVideoSourceId(id))
                {
                    analysisThreadClass.restart();
                    return true;
                }
            }
            return false;
        }


    }
}
