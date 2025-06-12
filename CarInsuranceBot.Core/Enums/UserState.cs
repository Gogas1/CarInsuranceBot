using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarInsuranceBot.Core.Enums
{
    public enum UserState
    {
        None,
        Processing,
        Home,

        DocumentsAwait,
        DocumentsDataConfirmationAwait,

        PriceConfirmationAwait,
        PriceSecondConfirmationAwait,

        TestUserState
    }
}
