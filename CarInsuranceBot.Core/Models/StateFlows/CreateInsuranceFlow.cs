namespace CarInsuranceBot.Core.Models.StateFlows
{
    /// <summary>
    /// Workflow model to store cache keys of user documents data
    /// </summary>
    public class CreateInsuranceFlow
    {
        public string IdCacheKey { get; set; } = string.Empty;
        public string DriverLicenseCacheKey { get; set; } = string.Empty;
    }
}
