namespace CarInsuranceBot.Core.Configuration
{
    public class BotConfiguration
    {
        /// <summary>
        /// Bot access token
        /// </summary>
        public required string Token { get; set; } = string.Empty;
        /// <summary>
        /// Administrators telegram ids for admin features access
        /// </summary>
        public List<long> AdminIds { get; set; } = new();

        /// <summary>
        /// 32 bytes long key
        /// </summary>
        public required string SecretKey { get; set; } = string.Empty;
        public required string Public256Key { get; set; } = string.Empty;
        public required string Private256Key { get; set; } = string.Empty;
        /// <summary>
        /// Mindee API key
        /// </summary>
        public required string MindeeKey { get; set; } = string.Empty;
        /// <summary>
        /// OpenAI API key
        /// </summary>
        public string OpenAiKey { get; set; } = string.Empty;

        public BotConfiguration()
        {

        }

        public BotConfiguration(
            string token,
            string secretKey,
            string public256Key,
            string private256Key,
            string mindeeKey,
            string openAiKey)
        {
            Token = token;
            SecretKey = secretKey;
            Public256Key = public256Key;
            Private256Key = private256Key;
            MindeeKey = mindeeKey;
            OpenAiKey = openAiKey;
        }

        public BotConfiguration(
            string token,
            string secretKey,
            List<long> adminIds,
            string public256Key,
            string private256Key,
            string mindeeKey,
            string openAiKey)
            : this(
                  token,
                  secretKey,
                  public256Key,
                  private256Key,
                  mindeeKey,
                  openAiKey)
        {
            AdminIds = adminIds;
        }
    }
}
