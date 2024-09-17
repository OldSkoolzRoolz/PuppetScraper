// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"

using System.Collections.ObjectModel;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using System.Linq;
using System.Collections;
using System.Collections.Generic;

using PuppetScraper.Models;
using PuppetScraper.Modules;


namespace PuppetScraper.Sample.UserControls;

public partial class ProfileListControl : UserControl
{
    List<TargetSiteName> ProfielNameSource { get; set; } = new List<TargetSiteName>();






    public ProfileListControl()
    {
        InitializeComponent();
        LoadData();
    }






    private void LoadData()
    {
        IProfileMaintenance maint = new ProfileMaintenance();
        var list = maint.GetProfileNames().GetAwaiter().GetResult();
        // ProfileList.Items = list.Select(x => x).OrderBy(x => x.ProfileName);
    }
}