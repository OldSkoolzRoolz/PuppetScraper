// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PuppetScraper.Sample.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{


    [ObservableProperty] [NotifyDataErrorInfo] [Required] [Url]
    private string _targetAddress;






    [ObservableProperty]
    private string _outerLoop;


    [ObservableProperty] [NotifyDataErrorInfo] [Required] [MinLength(5)]
    private string _innerLoop;

    [ObservableProperty] private string _nextSelector;

    [ObservableProperty] private int _pageCount;
    [ObservableProperty] private bool  _isProfileLoaded;

    [ObservableProperty] private bool  _lastActionSuccess;







    public MainWindowViewModel()
    {
        _outerLoop = "";
        _innerLoop = "";
        _targetAddress = "https://www.sharesome.com";
        _nextSelector = "*[@text.contains('next')";
        _pageCount = 15;

        PreloadFormValues();
    }






    private void PreloadFormValues()
    {
        throw new NotImplementedException();
    }




private void BeginScrapingSiteCommand()
    {}







}