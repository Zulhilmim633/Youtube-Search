using System.Text;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.AspNetCore.Html;
using YouTubeApiProject.Models;

namespace YouTubeApiProject.Services
{
    public class YouTubeApiService
    {
        private readonly string _apiKey;

        public YouTubeApiService(IConfiguration configuration)
        {
            _apiKey = configuration["YouTubeApiKey"]; // Fetch API key from appsettings.json
        }

        public async Task<List<object>> SearchVideosAsync(string query, string filter, int result, string pageToken, int page)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new Exception("Empty API Key detected in 'appsettings.json'. If you are a webmaster, please make sure you insert API Key first before running server again");
            }

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = _apiKey,
                ApplicationName = this.GetType().ToString()
            });

            var searchRequest = youtubeService.Search.List("snippet");
            searchRequest.Q = query;  // User's query from form input
            searchRequest.MaxResults = result;

            var parts = HexToString(filter).Split(",");

            (string u, string d, string o) = (parts[0], parts[1], parts[2]);

            //Set specific page for the result
            if (pageToken != "")
            {
                searchRequest.PageToken = pageToken;
            }

            //Assign to filter base on video duration
            if (d != "D")
            {
                searchRequest.VideoDuration = ByDuration(d);
            }

            //Filter response base on date of video uploaded
            searchRequest.PublishedAfterDateTimeOffset = ByDate(u);

            //Check if filter is set, change request type to video
            if ((d != "D" && u != "D") || d != "D")
            {
                searchRequest.Type = "video";
            }

            //Sort response video base on user request
            if (o != "D")
            {
                searchRequest.Order = SortBy(o);
            }

            //Execute request
            var searchResponse = await searchRequest.ExecuteAsync();

            var videos = searchResponse.Items.Select(item => new YouTubeVideoModel
            {
                ResType = item.Id.Kind,
                Title = new HtmlString(item.Snippet.Title),
                Description = new HtmlString(item.Snippet.Description),
                ThumbnailUrl = item.Snippet.Thumbnails.Medium.Url,
                VideoId = item.Id.VideoId,
                ChannelId = item.Snippet.ChannelId,
                ChannelTitle = new HtmlString(item.Snippet.ChannelTitle),
                PublishAt = item.Snippet.PublishedAtDateTimeOffset
            }).ToList<object>(); //Make sure list accept any data type

            //Insert first element of the list as page handling and shifting results to the right
            videos.Insert(0, new PageHandler
            {
                ResType = "paging",
                nextPage = searchResponse.NextPageToken,
                previousPage = searchResponse.PrevPageToken,
                resultPerPage = result,
                currentPage = page,
                search = query,
                filter = [u, d, o]
            });

            return videos;
        }

        public static SearchResource.ListRequest.VideoDurationEnum ByDuration(string duration)
        {
            return duration switch
            {
                "l" => SearchResource.ListRequest.VideoDurationEnum.Long__,
                "m" => SearchResource.ListRequest.VideoDurationEnum.Medium,
                "s" => SearchResource.ListRequest.VideoDurationEnum.Short__,
                _ => SearchResource.ListRequest.VideoDurationEnum.Any,
            };
        }

        public static DateTimeOffset ByDate(string date)
        {
            return date switch
            {
                "h" => DateTimeOffset.UtcNow.AddHours(-1),
                "t" => DateTimeOffset.UtcNow.AddHours(-24),
                "w" => DateTimeOffset.UtcNow.AddDays(-7),
                "m" => DateTimeOffset.UtcNow.AddDays(-30),
                "y" => DateTimeOffset.UtcNow.AddDays(-360),
                _ => DateTimeOffset.Parse("1970-01-01T00:00:00.000Z")
            };
        }

        public static SearchResource.ListRequest.OrderEnum SortBy(string order)
        {
            return order switch
            {
                "r" => SearchResource.ListRequest.OrderEnum.Rating,
                "d" => SearchResource.ListRequest.OrderEnum.Date,
                "v" => SearchResource.ListRequest.OrderEnum.ViewCount,
                "t" => SearchResource.ListRequest.OrderEnum.Title,
                _ => SearchResource.ListRequest.OrderEnum.Relevance
            };
        }

        public static string HexToString(string hex)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hex.Length; i += 2)
            {
                int value = Convert.ToInt32(hex.Substring(i, 2), 16);
                sb.Append((char)value);
            }
            return sb.ToString();
        }
    }
}
