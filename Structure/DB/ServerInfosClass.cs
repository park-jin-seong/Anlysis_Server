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
    public class ServerInfosClass
    {
        public int serverId { get;}
        public string serverIp { get;}
        public int serverPort { get; }

        public string serverType { get; }
        public string osId { get; }

        public string osPw { get; }

        public ServerInfosClass(int serverId, string serverIp, int serverPort, string serverType, string osId, string osPw)
        {
            this.serverId = serverId;
            this.serverIp = serverIp;
            this.serverPort = serverPort;
            this.serverType = serverType;
            this.osId = osId;
            this.osPw = osPw;
        }
    }
}
