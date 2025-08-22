using System;
using Alohacorp.Durian.Model;

namespace Aloha.Durian
{
    public class Announcement
    {
        public string Id { get; }
        public string Title { get; }
        public string Content { get; }
        public bool ShowImage { get; }
        public string ImageBytes { get; }
        public bool IsActive { get; }
        public DateTime StartsAt { get; }
        public DateTime EndsAt { get; }
        public string RedDotPath { get; }
    
        public Announcement(AnnouncementDto announcementDto, string redDotPath)
        {
            Id = announcementDto.Id.ToString();
            (Title, Content) = announcementDto.I18nContents.Get(announcementDto.Title, announcementDto.Content);
            ShowImage = announcementDto.ShowImage;
            ImageBytes = announcementDto.ImageBytes;
            IsActive = announcementDto.IsActive;
            StartsAt = announcementDto.StartsAt?.ToDateTime() ?? DateTime.MinValue;
            EndsAt = announcementDto.EndsAt?.ToDateTime() ?? DateTime.MaxValue;
        
            RedDotPath = redDotPath;
        }
    }
}