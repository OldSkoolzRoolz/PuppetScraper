using KC.Dropins.PuppetScraper.Models;

using PuppetScraper.Models;
using PuppetScraper.Modules;

namespace PuppetScraper.Models;

public interface IProfileMaintenance
{
    Task<TargetProfile?> GetProfileByProfileID(int profileid);

    Task<List<TargetSiteName>> GetProfileNames();

    Task<List<TargetProfile>> GetAllTargetProfiles();

    Task<bool> UpdateProfileSettings(TargetProfile profile);

    Task<bool> InsertNewProfile(TargetProfile profile);
}