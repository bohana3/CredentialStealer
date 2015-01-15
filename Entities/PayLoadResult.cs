using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CredentialStealer.Client.Console
{
    public enum PayLoadResultEnum
    {
        Success = 0,
        Failed = -1,
        Other = 1
    };

    public class PayLoadResult
    {
        public string Content { get; set; }
        public PayLoadResultEnum ResultCode { get; set; }
        public Exception Exception { get; set; }
    }
}
