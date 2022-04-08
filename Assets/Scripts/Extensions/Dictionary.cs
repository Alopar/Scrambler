using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scrambler
{
    public class Dictionary
    {
        #region FIELDS PRIVATE
        private List<string> _words;
        private List<WordIndex> _wordsIndexes;

        private int[] _countLetterToIndex = new int[] { 3, 4, 5, 6, 7 };

        public Dictionary(Language language = Language.English)
        {
            string filePath = null;
            switch (language)
            {
                case Language.English:
                    filePath = "Dictionaries/english_dictionary";
                    break;
                case Language.Russian:
                    filePath = "Dictionaries/russian_dictionary";
                    break;
            }

            var fileText = Resources.Load<TextAsset>(filePath);
            _words = fileText.text.Replace("\r", "").Split('\n').ToList();

            InitializationIndex();
        }
        #endregion

        #region METHODS PRIVATE
        private void InitializationIndex()
        {
            _wordsIndexes = new List<WordIndex>();
            foreach (var count in _countLetterToIndex)
            {
                var wordIndex = new WordIndex();
                wordIndex.NumberLetter = count;
                wordIndex.BeginIndex = _words.FindIndex(i => i.Length == count);
                wordIndex.EndhIndex = _words.FindLastIndex(i => i.Length == count);

                _wordsIndexes.Add(wordIndex);
            }
        }
        #endregion

        #region METHODS PUBLIC
        public bool TryWords(string word)
        {
            var wordIndex = _wordsIndexes.Find(i => i.NumberLetter == word.Length);
            var wordRange = _words.GetRange(wordIndex.BeginIndex, (wordIndex.EndhIndex + 1) - wordIndex.BeginIndex);
            return wordRange.Exists(i => i == word);
        }

        /// <summary>
        /// ATTENTION! Possible only numbers 3, 4, 5, 6, 7. Other numbers will throw error!
        /// </summary>
        public string GetRandomWord(int numberLetters)
        {
            if (numberLetters < 3 || numberLetters > 7) return $"Error: no words with {numberLetters} letter";

            var wordIndex = _wordsIndexes.Find(i => i.NumberLetter == numberLetters);
            var wordRange = _words.GetRange(wordIndex.BeginIndex, (wordIndex.EndhIndex + 1) - wordIndex.BeginIndex);
            return wordRange[Random.Range(0, wordRange.Count)];
        }
        #endregion
    }

    public struct WordIndex
    {
        public int NumberLetter;
        public int BeginIndex;
        public int EndhIndex;
    }

    public enum Language
    {
        English,
        Russian
    }
}