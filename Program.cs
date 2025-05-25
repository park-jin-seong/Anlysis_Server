using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Analysis_Server.DI;
using Analysis_Server.Manager.AnalysisManager;
using Analysis_Server.Structure;
using Analysis_Server.Structure.DB;
using Analysis_Server.THD;
using Newtonsoft.Json;
using Ninject;
using Npgsql;

namespace Analysis_Server
{
    public class Program
    {
        private static IKernel kernel;
        static void Main(string[] args)
        {
            SetAnlysisStart();
        }

        private static void SetAnlysisStart()
        {
            kernel = new StandardKernel(new AppModuleClass());
            kernel.Get<AnalysisThreadManagerClass>().SetAnlysisAndStart();
        }
        
    }
}
