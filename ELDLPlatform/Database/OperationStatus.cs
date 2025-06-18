using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELDLPlatform.Database
{
    internal enum OperationStatus : short
    {
        Failed = 0, // Data not found
        Success = 1, // Success
        Error = -1 // Data not extracted due to process interruption
    }
}
