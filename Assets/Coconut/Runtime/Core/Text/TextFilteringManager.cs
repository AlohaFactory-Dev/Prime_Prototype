using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Aloha.Coconut
{
    public static class TextFilteringManager
    {
        private struct BannedTextData
        {
            [CSVColumn] public string bannedWord;
        }

        private static List<string> _bannedWords;

        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeInitializeOnLoad()
        {
            _bannedWords = null;
        }

        public static UniTask<(bool, string)> IsValid(string text)
        {
            if (_bannedWords == null) Initialize();
            
            if (ContainsBlankSpace(text, out string message)) return UniTask.FromResult((false, message));
            if (ContainsSpecialCharacters(text, out message)) return UniTask.FromResult((false, message));
            if (ContainsBannedWord(text, out message)) return UniTask.FromResult((false, message));

            return UniTask.FromResult((true, message));
        }

        private static void Initialize()
        {
            _bannedWords = new List<string>();
            var table = TableManager.Get<BannedTextData>("text_filter");
            foreach (var data in table)
            {
                _bannedWords.Add(data.bannedWord.ToLower());
            }
        }

        private static bool ContainsBlankSpace(string text, out string message)
        {
            bool result = text.Contains(" ") || String.IsNullOrWhiteSpace(text);
            message = result ? TextTableV2.Get("TextFilter/BlankSpace") : "";

            return result;
        }

        private static bool ContainsSpecialCharacters(string text, out string message)
        {
            string checker = Regex.Replace(text, @"[^a-zA-Z0-9가-힣\s]", "");
            if (text.Equals(checker) == false)
            {
                message = TextTableV2.Get("TextFilter/InvalidCharacter");
                return true;
            }

            message = "";
            return false;
        }

        private static bool ContainsBannedWord(string text, out string message)
        {
            text = text.ToLower();

            foreach (string bannedWord in _bannedWords)
            {
                if (!text.Contains(bannedWord)) continue;

                message = $"{TextTableV2.Get("TextFilter/BannedWordIncluded")}(\"{bannedWord}\")";
                return true;
            }

            message = "";
            return false;
        }
    }
}