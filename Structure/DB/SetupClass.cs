using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Analysis_Server.Structure.DB
{
    public class SetupClass
    {
        public int m_idx { get;}
        public string m_serverIp { get;}
        public string m_serverPort { get; }
        public List<string> m_assignedVideoSourceIds { get;}

        public SetupClass(int idx, string serverIp, string serverPort, string assignedVideoSourceIds)
        {
            m_idx = idx;
            m_serverIp = serverIp;
            m_serverPort = serverPort;
            m_assignedVideoSourceIds = JsonConvert.DeserializeObject<List<string>>(assignedVideoSourceIds);
        }
    }
}
