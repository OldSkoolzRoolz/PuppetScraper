// "@    @      @    @

namespace KC.Dropins.PuppetScraper.Models;

public class TargetProfile
{
    public TargetProfile()
    {
    }






    /// ctor
    public TargetProfile(string? name)
    {
        ProfileName = name;
    }






    //internal tracking id
    public int id { get; }

    //public dupe profile eliminator
    public string? ProfileId { get; set; }

    //auto created date stamp
    public string DateAdded { get; }

    //target site name
    public string? ProfileName { get; set; }

    //target site description
    public string? ProfileDescription { get; set; }

    public string? ProfileUrl { get; set; }

    //a parent element of the items. Often times
    //this is the "container" div
    public string? SelectorOne { get; set; }

    //this selector is a parent (topmost) element of a single item
    //some sites use custom element types like 'article' or 'post' to
    //deliniate each item. This element can also contain an attribute with a form of 'id' 'postid' or 'data-id'
    //that can be used to prevent scraping the same items over and over.
    public string? SelectorTwo { get; set; }

    //if further filtering needs to be done this selector can be used
    // otherwise the scraper will search for the target element type from the previous selector point.
    public string? SelectorThree { get; set; }

    //if your are scraping large sets of data that uses pagination this should be true
    public bool UsePagination { get; set; }

    // pagination scheme used prev/next buttons or infinite scrolling.
    public string? PaginationType { get; set; }

    // the selector to the next button for automatic paging
    public string? PaginationNextSelector { get; set; }

    // is your target videos or images or both
    public string? TargetType { get; set; }

    public bool IsDirty { get; }

    public bool IsLocked { get; }
}

public class TargetElementType
{
    public static string Video { get { return "Video"; } }

    public static string Image { get { return "Image"; } }
    public static string Both { get { return "Both"; } }
}

public class PaginationTypeUsed
{
    public static string PrevNext { get { return "PrevNext"; } }
    public static string InfiniteScrolling { get { return "InfiniteScroller"; } }
    public static string None { get { return "SinglePage"; } }
}