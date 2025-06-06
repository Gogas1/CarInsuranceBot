using CarInsuranceBot.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarInsuranceBot.Core.Models
{
    public class MyUser
    {
        public Guid Id { get; set; }
        public long TelegramId { get; set; }
        public UserState UserState { get; set; }

        public UserInputState InputState { get; set; }
    }
}
