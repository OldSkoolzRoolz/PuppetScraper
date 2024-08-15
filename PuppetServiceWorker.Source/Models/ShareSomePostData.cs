


namespace PuppetServiceWorker.Models;

public class ShareSomePostData(string dataIndex, string postId,string postType , string address)
{
    public string DataIndex { get; set; } = dataIndex;

    public string ArticleId { get; set; } = postId;

    public string PostType { get; set; } = postType;

    public string PostSourceUrl { get; set; } = address;


}




public enum ArticlePostType
{
    post = 0,
    video = 1,
    image = 2

}





