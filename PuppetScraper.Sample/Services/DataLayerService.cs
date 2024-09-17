


using System.Collections.Generic;

namespace PuppetScraper.Sample.Services;


public interface IDataLayerService
{
    List<string> GetFilesForDownload();

}



public class DataLayerService : IDataLayerService
{
    public DataLayerService()
    {
    }

    public List<string> GetFilesForDownload()
    {
        throw new System.NotImplementedException();
    }

}