using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ELDLPlatform.Database
{
    internal readonly struct DBData
    {
        internal readonly Dictionary<string, string> data;
        internal readonly string serializedData;
        internal readonly OperationStatus operationStatus;

        public DBData(Dictionary<string, string> data, OperationStatus operationStatus)
        {
            this.data = data;
            if (data.Count != 0)
            {
                serializedData = JsonSerializer.Serialize(data);
            }
            else { serializedData = ""; }
            
            this.operationStatus = operationStatus;
        }

        internal static DBData Default() { return new DBData([], OperationStatus.Failed); }
    }
}
