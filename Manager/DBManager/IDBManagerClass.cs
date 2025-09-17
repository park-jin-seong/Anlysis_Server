using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Analysis_Server.Structure.DB;

namespace Analysis_Server.Manager.DBManager
{
    public interface IDBManagerClass
    {
        ServerInfosClass GetServerInfosClass();
        List<CameraInfoClass> GetCameraInfosClasses();
    }
}
