
using System.Data.Common;
using Dapper;
using NLog;
using Wilmar.Interface;

namespace Wilmar.SSAS.MDXBuilder.Criteria
{
    public abstract class BaseDAO
    {
        

        public readonly static string strConn = "KDS3";
        private static ILogger logger;


        static BaseDAO()
        {
            ConfigureManager.Initialization(typeof(BaseDAO));
            logger = LogManager.GetCurrentClassLogger();
        }

        



    }
}
