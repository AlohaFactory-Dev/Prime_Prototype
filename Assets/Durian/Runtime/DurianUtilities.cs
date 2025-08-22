using System;
using System.Collections.Generic;
using System.Linq;
using Alohacorp.Durian.Model;
using I2.Loc;

namespace Aloha.Durian
{
    public static class DurianUtilities
    {
        public static (string title, string content) Get(this IEnumerable<I18nContent> i18NContents, string defaultTitle, string defaultContent)
        {
            I18nContent targetContent = null;
            string languageCode = LocalizationManager.CurrentLanguageCode;
            targetContent = i18NContents.FirstOrDefault(i18 => i18.LanguageCode == languageCode);
            if (targetContent == null) targetContent = i18NContents.FirstOrDefault(i18 => languageCode.StartsWith(i18.LanguageCode));
            if (targetContent == null) targetContent = i18NContents.FirstOrDefault(i18 => i18.LanguageCode.StartsWith(languageCode));
            if (targetContent == null) targetContent = i18NContents.FirstOrDefault(i18 => i18.LanguageCode == "en-US");
        
            if(targetContent != null) return (targetContent.Title, targetContent.Content);
            return (defaultTitle, defaultContent);
        }
    
        public static (string title, string content) Get(this IEnumerable<I18nContentDto> i18NContents, string defaultTitle, string defaultContent)
        {
            I18nContentDto targetContent = null;
            string languageCode = LocalizationManager.CurrentLanguageCode;
            targetContent = i18NContents.FirstOrDefault(i18 => i18.LanguageCode == languageCode);
            if (targetContent == null) targetContent = i18NContents.FirstOrDefault(i18 => languageCode.StartsWith(i18.LanguageCode));
            if (targetContent == null) targetContent = i18NContents.FirstOrDefault(i18 => i18.LanguageCode.StartsWith(languageCode));
            if (targetContent == null) targetContent = i18NContents.FirstOrDefault(i18 => i18.LanguageCode == "en-US");
        
            if(targetContent != null) return (targetContent.Title, targetContent.Content);
            return (defaultTitle, defaultContent);
        }
    
        // 서버에서 넘어오는 timestamp는 UTC로 되어있지만, 클라이언트 코드는 UTC+9를 기준으로 돌아감
        public static DateTime ToDateTime(this long utcTimestampMilliseconds)
        {
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dt = dt.AddMilliseconds(utcTimestampMilliseconds).AddHours(9);
            return dt;
        }
    }
}