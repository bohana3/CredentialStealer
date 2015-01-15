using CredentialStealer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CredentialStealer.Client.Console
{
    public class PayLoad
    {
        public PayLoad()
        {

        }

        public PayLoadResult Start()
        {
            PayLoadResult res = new PayLoadResult();
            res.ResultCode = 0;
            System.Console.WriteLine("Start PayLoad");
            return res;
        }

        public PayLoadResult Stop()
        {
            PayLoadResult res = new PayLoadResult();
            res.ResultCode = 0;
            System.Console.WriteLine("Stop PayLoad");
            return res;
        }
    }
}
