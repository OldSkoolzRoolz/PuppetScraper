// "@    @      @    @

namespace PuppetScraper.Models
{
    public class TargetSiteName
    {
        public TargetSiteName(int Id, string profileName)
        {
            this.Id = Id;
            this.ProfileName = profileName;
        }






        //Represents the unique integer for internal tracking]
        public int Id { get; set; } = 0;

        //User generated name of the site profile
        public string ProfileName { get; set; } = string.Empty;
    }
}