using CarInsuranceBot.Core.Enums;

namespace CarInsuranceBot.Core.Models
{
    /// <summary>
    /// User model
    /// </summary>
    public class MyUser
    {
        public Guid Id { get; set; }
        public long TelegramId { get; set; }
        public UserState UserState { get; set; }

        public UserInputState InputState { get; set; }
    }
}
