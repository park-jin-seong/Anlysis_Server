using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Analysis_Server.Manager.SystemInfoManager;
using Newtonsoft.Json;

namespace Analysis_Server.Manager.SetupManager
{
    internal class SystemInfoManagerClass : ISystemInfoManagerClass
    {
        private String m_systemInfoPath;
        private SystemInfoClass m_SystemInfoClass;

        public SystemInfoManagerClass()
        {
            m_systemInfoPath = @"Setup\SystemInfo.json";


            if (!new FileInfo(m_systemInfoPath).Exists)
            {
                MakeFile();
            }
            String[] m_systemInfoContentsArray;
            String m_systemInfoContents = "";
            m_systemInfoContentsArray = File.ReadAllLines(m_systemInfoPath, Encoding.UTF8);
            foreach (String line in m_systemInfoContentsArray)
            {
                m_systemInfoContents += line;
            }
            m_SystemInfoClass = JsonConvert.DeserializeObject<SystemInfoClass>(m_systemInfoContents);
        }

        public SystemInfoClass GetSystemInfoClass()
        {
            return m_SystemInfoClass;
        }

        private void MakeFile()
        {
            SystemInfoClass m_SystemInfoClass_Defult = new SystemInfoClass(@"yolov10n.onnx", 1, "127.0.0.1", "5432", "postgres", "password");
            String jsonParse_Default = JsonConvert.SerializeObject(m_SystemInfoClass_Defult);
            WriteFile(m_systemInfoPath, jsonParse_Default);
        }
        private void WriteFile(string path, string contents)
        {
            //파일을 만들고 문자열을 파일에 추가함
            //폴더(디렉터리)가 없는 경우 폴더를 먼저 만들고 그 다음 파일을 만들고 문자열을 파일에 추가함
            var filePath = path;

            var directory = Path.GetDirectoryName(filePath);    //filePath에 있는 디렉터리만 가져옴
            Directory.CreateDirectory(directory);   //디렉터리를 만들어 줌

            File.WriteAllText(filePath, contents);
        }
    }
}
