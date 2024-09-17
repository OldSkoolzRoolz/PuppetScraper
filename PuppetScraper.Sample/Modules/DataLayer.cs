


using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;

using PuppetScraper.Sample.Services;

namespace PuppetScraper.Sample.Modules;



public interface IDataLayer
{
}

public class DataLayer
{
    private readonly IDataLayerService _dataLayerService;


    //IDataLayerService _dataLayerService = App.Current.Services.GetRequiredService<IDataLayerService>();

    public DataLayer(IDataLayerService dataLayerService)
    {

        _dataLayerService = dataLayerService;





    }






public List<string> GetFiles()
    {
        return _dataLayerService.GetFilesForDownload();
    }



}


