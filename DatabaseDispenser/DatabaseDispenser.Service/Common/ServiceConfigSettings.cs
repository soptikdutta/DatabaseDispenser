using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseDispenser.Service.Common
{
    public static class ServiceConfigSettings
    {
        public static string DocumentDbEndpointUrl
        {
            get
            {
                return CommonFunctions.GetAppKey("Database", "EndpointUrl", "https://the-goods-backend-poc.documents.azure.com:443/");
            }
        }

        public static string AuthKey
        {
            get
            {
                return CommonFunctions.GetAppKey("Database", "AuthKey", "JIIVda3t8Ry2ziT3C3DXPzSEqOAlbwUwqA9Jj4dkDfwt0YEpFusYuGWvj2nWgbNTk006dDfEymYT0zV4eW3BuA==");
            }
        }

        public static int MinimumDatabases
        {
            get
            {
                return int.Parse(CommonFunctions.GetAppKey("Database", "MinimumDatabases", "5"));
            }
        }

        public static int MaximumDatabases
        {
            get
            {
                return int.Parse(CommonFunctions.GetAppKey("Database", "MaximumDatabases", "10"));
            }
        }
    }
}
