using Microsoft.AspNetCore.Html;

namespace YouTubeApiProject.Models
{
    public class YouTubeVideoModel
    {
        public string ResType { get; set; }
        public IHtmlContent Title { get; set; }
        public IHtmlContent Description { get; set; }
        public string ThumbnailUrl { get; set; }
        public string VideoId { get; set; }
        public string ChannelId { get; set; }
        public IHtmlContent ChannelTitle { get; set; }
        public DateTimeOffset? PublishAt { get; set; }

        public string? GetPublish()
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan? difference = now - PublishAt?.ToUniversalTime();

            int? time;
            string? text;

            if (difference?.TotalSeconds < 60)
            {
                time = (int?)difference?.TotalSeconds;
                text = time == 1 ? "second ago" : "seconds ago";
            }
            else if (difference?.TotalMinutes < 60)
            {
                time = (int?)difference?.TotalMinutes;
                text = time == 1 ? "minute ago" : "minutes ago";
            }
            else if (difference?.TotalHours < 24)
            {
                time = (int?)difference?.TotalHours;
                text = time == 1 ? "hour ago" : "hours ago";
            }
            else if (difference?.TotalDays < 7)
            {
                time = (int?)difference?.TotalDays;
                text = time == 1 ? "day ago" : "days ago";
            }
            else if (difference?.TotalDays < 30)
            {
                time = (int?)difference?.TotalDays / 7; // Convert to weeks
                text = time == 1 ? "week ago" : "weeks ago";
            }
            else if (difference?.TotalDays < 365)
            {
                time = (int?)difference?.TotalDays / 30; // Convert to months
                text = time == 1 ? "month ago" : "months ago";
            }
            else
            {
                time = (int?)difference?.TotalDays / 365; // Convert to years
                text = time == 1 ? "year ago" : "years ago";
            }


            return $"{time} {text}";
        }
    }

    public class PageHandler
    {
        public string ResType { get; set; }
        public string nextPage { get; set; }
        public string previousPage { get; set; }
        public int? resultPerPage { get; set; }
        public int currentPage { get; set; }
        public string search { get; set; }
        public string[] filter { get; set; }
    }
}
