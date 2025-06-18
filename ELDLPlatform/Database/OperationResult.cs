using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ELDLPlatform.Database
{
    // Struct to contain operation result
    // For data formating, see documentation
    internal struct OperationResult
    {
        internal OperationStatus transactionStatus = OperationStatus.Failed;
        internal string message = "";
        internal string dataJSON = "";

        public OperationResult(OperationStatus transactionStatus, string message, string dataJSON)
        {
            this.transactionStatus = transactionStatus;
            this.message = message;
            this.dataJSON = dataJSON;
        }

        // Use this instead of default constructor
        // It is a safety measure that is necessary due to how empty constructors implemented for structures
        // Simply using new() in specific instances may ignore default constructor even thought it allowed to use
        // It can lead to exceptions in unexpected places due to values not being initialised
        public static OperationResult Default() { return new OperationResult(OperationStatus.Failed, "", ""); }
    }
}