



using KC.Dropins.PuppetScraper.Models;

using PuppetScraper.Data;
using PuppetScraper.Models;

namespace PuppetScraper.Modules;

public class ProfileMaintenance : IProfileMaintenance
{
    public Task<TargetProfile?> GetProfileByProfileId(int profileid)
    {
        var profile = TrackingDb.SelectSiteProfileById(profileid);

        return Task.FromResult(profile);
    }






    public Task<TargetProfile?> GetProfileByProfileID(int profileid)
    {
        throw new NotImplementedException();
    }






    public Task<List<TargetSiteName>> GetProfileNames()
    {
        throw new NotImplementedException();
    }






    public Task<List<TargetProfile>> GetAllTargetProfiles()
    {
        throw new NotImplementedException();
    }






    public Task<bool> UpdateProfileSettings(TargetProfile profile)
    {
        throw new NotImplementedException();
    }






    public Task<bool> InsertNewProfile(TargetProfile profile)
    {
        throw new NotImplementedException();
    }
}