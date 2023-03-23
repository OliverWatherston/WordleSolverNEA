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
            if (!File.Exists(_wordListPath)) // check if word list exists in same folder as executable else exit
            {
                Console.WriteLine($"Could not find 'wordlist.txt' at location {_wordListPath}");
                System.Threading.Thread.Sleep(2500);
                Environment.Exit(0);
            }
            
            string[] wordListFile = File.ReadAllLines(_wordListPath); // assign word list to the string list _words
            _words.AddRange(wordListFile);

            CalculateWordScores();
            CalculateWordScoresPositional();
        }

        private WordList(List<string> words, Dictionary<string, int> wordScores, Dictionary<string, int> wordScoresPositional) // overload constructor for cloning
        {
            _words = words;
            _wordScores = wordScores;
            _wordScoresPositional = wordScoresPositional;
        }

        public WordList GetCloneOfWordList() // return a clone of the word list
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

        public string CalculateWordMiniMax(bool usePositional = false)
        {
            Dictionary<string, int> scores = usePositional ? _wordScoresPositional : _wordScores; // allow use of two different scoring methods, but only positional is currently used
            KeyValuePair<string, int> bestPair = new KeyValuePair<string, int>();

            foreach (var word in scores) // simply compare all words in the scores dictionary and return the one with the highest score
            {
                if (word.Value > bestPair.Value)
                {
                    bestPair = word;
                }
            }

            return bestPair.Key;
        }

        public string CalculateWordMaxUnique(List<char> priorityLetters)
        {
            CalculateLetterCount();
            KeyValuePair<string, int> bestPair = new KeyValuePair<string, int>();

            foreach (var word in _words)
            {
                int score = 0;
                foreach (var letter in priorityLetters) // compare each word in word list to look for priority letters
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
            foreach (var letter in Globals.Alphabet) // initialise the dictionary for each letter in the alphabet
            {
                _letterCount.Add(letter, 0);
            }

            foreach (var word in _words)
            {
                foreach (var letter in word) // count the number of times each letter appears in the word list
                {
                    _letterCount[letter]++;
                }
            }
        }

        private void CalculateLetterCountPositional()
        {
            _letterCountPositional.Clear();
            for (int i = 0; i < 5; i++) // initialise the dictionary for each position in the word list
            {
                _letterCountPositional.Add(new Dictionary<char, int>());
                foreach (var letter in Globals.Alphabet) // initialise the dictionary for each letter in the alphabet
                {
                    _letterCountPositional[i].Add(letter, 0);
                }
            }

            foreach (var word in _words)
            {
                for (int i = 0; i < word.Length; i++) // count the number of times each letter appears in each position in the word list
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
            foreach (var word in _words) // score each word in the word list by adding the number of times each letter appears in the word list, this is the basis for the minimax algorithm
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
            foreach (var word in _words) // score each word in the word list by adding the number of times each letter appears in that position in the word list, this is the basis for the positional minimax algorithm
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
                for (int i = 0; i < mustHaveMask.Count; i++) // check if the word contains the must have letters
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
                        foreach (var forbiddenLetter in forbiddenLetters) // check if the word contains any of the forbidden letters
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
                        foreach (var letter in Globals.Alphabet) // check if the word contains any letters that are not allowed in the allowed mask
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
                            filteredWordList.Add(word); // if the word passes all the checks, add it to the filtered word list
                        }
                    }
                }
            }

            _words = filteredWordList; // replace the word list with the filtered word list
        }

        public void UpdateMaskWithRemainingWords(List<List<char>> allowedMask)
        {
            CalculateLetterCount();
            foreach (var letterCountPair in _letterCount) // remove any letters from the allowed mask that are not in the remaining words
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