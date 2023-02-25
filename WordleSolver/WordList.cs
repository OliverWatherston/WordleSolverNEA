using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WordleSolver
{
    public class WordList
    {
        private string _wordListPath =
            $@"{Path.GetDirectoryName(AppContext.BaseDirectory)}\wordlist.txt";
        private Random _rng = new Random();
        private readonly List<char> _alphabet = "abcdefghijklmnopqrstuvwxyz".ToList();
        
        public List<string> _WordList { get; set; } = new List<string>();
        
        public Dictionary<char, int> LetterCount { get; set; } = new Dictionary<char, int>();
        
        private Dictionary<string, int> _wordScores = new Dictionary<string, int>();

        public List<Dictionary<char, int>> _letterCountPositional = new List<Dictionary<char, int>>();
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
            _WordList.AddRange(wordListFile);

            CalculateWordScores();
            CalculateWordScoresPositional();
        }

        private WordList(List<string> wordList, Dictionary<string, int> wordScores, Dictionary<string, int> wordScoresPositional)
        {
            _WordList = wordList;
            _wordScores = wordScores;
            _wordScoresPositional = wordScoresPositional;
        }

        public WordList GetCloneOfWordList()
        {
            return new WordList(_WordList, _wordScores, _wordScoresPositional);
        }

        public int GetWordListLength()
        {
            return _WordList.Count;
        }

        public string GetHighscoreWord(bool usePositional = false)
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

        public string GetMaximumUniqueLettersWord(List<char> maximumUniqueLettersWordList)
        {
            GenerateLetterCount();
            KeyValuePair<string, int> bestPair = new KeyValuePair<string, int>();

            foreach (var word in _WordList)
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

        public void GenerateLetterCount()
        {
            LetterCount = new Dictionary<char, int>();
            foreach (var letter in _alphabet)
            {
                LetterCount.Add(letter, 0);
            }

            foreach (var word in _WordList)
            {
                foreach (var letter in word)
                {
                    LetterCount[letter]++;
                }
            }
        }

        public void GenerateLetterCountPositional()
        {
            _letterCountPositional.Clear();
            for (int i = 0; i < 5; i++)
            {
                _letterCountPositional.Add(new Dictionary<char, int>());
                foreach (var letter in _alphabet)
                {
                    _letterCountPositional[i].Add(letter, 0);
                }
            }

            foreach (var word in _WordList)
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
            GenerateLetterCount();
            _wordScores = new Dictionary<string, int>();
            foreach (var word in _WordList)
            {
                int score = 0;
                foreach (var letter in word)
                {
                    score += LetterCount[letter];
                }
                _wordScores.Add(word, score);
            }
        }

        public void CalculateWordScoresPositional()
        {
            GenerateLetterCountPositional();
            _wordScoresPositional = new Dictionary<string, int>();
            foreach (var word in _WordList)
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
            foreach (var word in _WordList)
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
                        foreach (var letter in _alphabet)
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

            _WordList = filteredWordList;
        }
    }
}