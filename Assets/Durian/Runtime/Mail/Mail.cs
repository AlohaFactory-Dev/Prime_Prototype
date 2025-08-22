using System;
using System.Collections.Generic;
using Aloha.Coconut;
using Alohacorp.Durian.Model;
using UnityEngine;

namespace Aloha.Durian
{
    public class Mail
    {
        public enum StateEnum
        {
            UNREAD = 1,
            READ = 2,
            RECEIVED = 3
        }
        
        public string Id { get; }
        public StateEnum State { get; }
        public DateTime? SentAt { get; }
        public DateTime? ReceivedAt { get; }
        public DateTime? ExpiresAt { get; }
        public DateTime? ReadAt { get; }
    
        public string Title { get; }
        public string Content { get; }
        public List<Property> Attachments { get; }
    
        public Mail(PlayerMailDto playerMailDto)
        {
            Id = playerMailDto.Id;
            State = (StateEnum)(int)playerMailDto.State;
            SentAt = playerMailDto.SentAt?.ToDateTime();
            ReceivedAt = playerMailDto.ReceivedAt?.ToDateTime();
            ExpiresAt = playerMailDto.ExpiresAt?.ToDateTime();
            ReadAt = playerMailDto.ReadAt?.ToDateTime();
            (Title, Content) = playerMailDto.Mail.I18nContents.Get(playerMailDto.Mail.Title, playerMailDto.Mail.Content);
        
            var mailAttachments = playerMailDto.Mail.Attachments;
            Attachments = new List<Property>();
            for (var i = 0; i < mailAttachments.Count; i++)
            {
                try
                {
                    Attachments.Add(new Property(mailAttachments[i].Content, mailAttachments[i].Quantity.Value));
                }
                catch
                {
                    Debug.LogError($"Failed to parse attachment: {mailAttachments[i]}");
                }
            }
        }
    }
}