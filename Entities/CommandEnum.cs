using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CredentialStealer.Entities
{
    public enum CommandEnum
    {
        [CredentialStealer.Utils.Attribute.KeyAttribute("Start")]
        Start,
        [CredentialStealer.Utils.Attribute.KeyAttribute("Exit")]
        Exit,
        [CredentialStealer.Utils.Attribute.KeyAttribute("Destroy")]
        Destroy
    }
}
