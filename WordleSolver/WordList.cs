using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WordleSolver
{
    public class WordList
    {
        private readonly string _wordListPath =
            $@"{Path.GetDirectoryName(AppContext.BaseDirectory)}\wordlist.txt";
        private Random _rng = new Random();

        private List<string> _words = new List<string>();
        
        private Dictionary<char, int> _letterCount = new Dictionary<char, int>();
        private Dictionary<string, int> _wordScores = new Dictionary<string, int>();

        private List<Dictionary<char, int>> _letterCountPositional = new List<Dictionary<char, int>>();
        private Dictionary<string, int> _wordScoresPositional = new Dictionary<string, int>();

        public WordList()
        {
            if (!File.Exists(_wordListPath))
            {
                Console.WriteLine($"Could not find 'wordlist.txt' at location {_wordListPath}");
                System.Threading.Thread.Sleep(2500);
                Environment.Exit(0);
            }
            
            string[] wordListFile = File.ReadAllLines(_wordListPath);
            _words.AddRange(wordListFile);

            CalculateWordScores();
            CalculateWordScoresPositional();
        }

        private WordList(List<string> words, Dictionary<string, int> wordScores, Dictionary<string, int> wordScoresPositional)
        {
            _words = words;
            _wordScores = wordScores;
            _wordScoresPositional = wordScoresPositional;
        }

        public WordList GetCloneOfWordList()
        {
            return new WordList(_words, _wordScores, _wordScoresPositional);
        }
        
        public int GetWordListLength()
        {
            return _words.Count;
        }
        
        public void TryRemoveWord(string word)
        {
            if (_words.Contains(word))
            {
                _words.Remove(word);
            }
        }

        public string GetWordListAsStringOrdered()
        {
            return string.Join(", ", _words.OrderBy(word => _wordScores[word]));
        }
        
        public bool ContainsWord(string word)
        {
            return _words.Contains(word);
        }

        public string CalculateWordMiniMax(bool usePositional = false)
        {
            Dictionary<string, int> scores = usePositional ? _wordScoresPositional : _wordScores;
            KeyValuePair<string, int> bestPair = new KeyValuePair<string, int>();

            foreach (var word in scores)
            {
                if (word.Value > bestPair.Value)
                {
                    bestPair = word;
                }
            }

            return bestPair.Key;
        }

        public string CalculateWordMaxUnique(List<char> maximumUniqueLettersWordList)
        {
            CalculateLetterCount();
            KeyValuePair<string, int> bestPair = new KeyValuePair<string, int>();

            foreach (var word in _words)
            {
                int score = 0;
                foreach (var letter in maximumUniqueLettersWordList)
                {
                    if (word.Contains(letter))
                    {
                        score++;
                    }
                }

                if (score > bestPair.Value)
                {
                    bestPair = new KeyValuePair<string, int>(word, score);
                }
            }

            return bestPair.Key;
        }

        public void CalculateLetterCount()
        {
            _letterCount = new Dictionary<char, int>();
            foreach (var letter in Globals.Alphabet)
            {
                _letterCount.Add(letter, 0);
            }

            foreach (var word in _words)
            {
                foreach (var letter in word)
                {
                    _letterCount[letter]++;
                }
            }
        }

        private void CalculateLetterCountPositional()
        {
            _letterCountPositional.Clear();
            for (int i = 0; i < 5; i++)
            {
                _letterCountPositional.Add(new Dictionary<char, int>());
                foreach (var letter in Globals.Alphabet)
                {
                    _letterCountPositional[i].Add(letter, 0);
                }
            }

            foreach (var word in _words)
            {
                for (int i = 0; i < word.Length; i++)
                {
                    char letter = word[i];
                    _letterCountPositional[i][letter]++;
                }
            }
        }

        public void CalculateWordScores()
        {
            CalculateLetterCount();
            _wordScores = new Dictionary<string, int>();
            foreach (var word in _words)
            {
                int score = 0;
                foreach (var letter in word)
                {
                    score += _letterCount[letter];
                }
                _wordScores.Add(word, score);
            }
        }

        public void CalculateWordScoresPositional()
        {
            CalculateLetterCountPositional();
            _wordScoresPositional = new Dictionary<string, int>();
            foreach (var word in _words)
            {
                Dictionary<char, int> score = new Dictionary<char, int>();
                for (int i = 0; i < word.Length; i++)
                {
                    char letter = word[i];
                    if (!score.ContainsKey(letter))
                    {
                        score[letter] = _letterCountPositional[i][letter];
                    }
                    else
                    {
                        score[letter] = Math.Max(score[letter], _letterCountPositional[i][letter]);
                    }
                }

                _wordScoresPositional[word] = score.Values.Sum();
            }
        }

        public void FilterWordListWithMasking(List<List<char>> allowedMask, List<List<char>> mustHaveMask, List<List<char>> forbiddenMask)
        {
            List<string> filteredWordList = new List<string>();
            foreach (var word in _words)
            {
                bool found = false;
                for (int i = 0; i < mustHaveMask.Count; i++)
                {
                    List<char> mustHaveLetters = mustHaveMask[i];
                    if (mustHaveLetters.Count > 0 && word[i] != mustHaveLetters[0])
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    for (int i = 0; i < forbiddenMask.Count; i++)
                    {
                        List<char> forbiddenLetters = forbiddenMask[i];
                        bool failed = false;
                        foreach (var forbiddenLetter in forbiddenLetters)
                        {
                            if (word[i] == forbiddenLetter)
                            {
                                failed = true;
                            }
                        }

                        if (failed)
                        {
                            found = true;
                            break;
                        }
                    }
                    
                    if (!found)
                    {
                        foreach (var letter in Globals.Alphabet)
                        {
                            int count = word.Count(x => x == letter);
                            if (!(allowedMask[count].Contains(letter)))
                            {
                                found = true;
                                break;
                            }
                        }
                        
                        if (!found)
                        {
                            filteredWordList.Add(word);
                        }
                    }
                }
            }

            _words = filteredWordList;
        }

        public void UpdateMaskWithRemainingWords(List<List<char>> allowedMask)
        {
            CalculateLetterCount();
            foreach (var letterCountPair in _letterCount)
            {
                if (letterCountPair.Value == 0)
                {
                    for (int i = 1; i < 3; i++)
                    {
                        if (allowedMask[i].Contains(letterCountPair.Key))
                        {
                            allowedMask[i].Remove(letterCountPair.Key);
                        }
                    }
                }
            }
        }
    }
}