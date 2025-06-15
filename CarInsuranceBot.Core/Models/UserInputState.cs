using CarInsuranceBot.Core.Models.StateFlows;

namespace CarInsuranceBot.Core.Models
{
    public class UserInputState
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public MyUser User { get; set; }

        public CreateInsuranceFlow CreateInsuranceFlow { get; set; } = new();
    }
}
