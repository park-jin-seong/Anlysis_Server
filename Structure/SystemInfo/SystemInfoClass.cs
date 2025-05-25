using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Analysis_Server.Structure;

namespace Analysis_Server
{
    public class SystemInfoClass
    {
        public string m_ModelPath { get; set; }
        public DataBaseClass m_dataBaseClass { get; set; }

        public SystemInfoClass() 
        {
            m_ModelPath = "";
            m_dataBaseClass = new DataBaseClass();
        }

        public SystemInfoClass(string modelPath, int DBType, string ip, string port, string id, string pw)
        {
            m_ModelPath = modelPath;
            m_dataBaseClass = new DataBaseClass(DBType, ip, port, id, pw);
        }


    }
}
