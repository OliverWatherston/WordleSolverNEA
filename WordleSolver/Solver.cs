using System;
using System.Collections.Generic;
using System.Linq;

namespace WordleSolver
{
    public class Solver
    {
        public WordList _remainingWordList = new WordList();
        private readonly List<char> _alphabet = "abcdefghijklmnopqrstuvwxyz".ToList();
        
        private List<List<char>> _requiredMask = new List<List<char>>();
        private List<List<char>> _forbiddenMask = new List<List<char>>();
        private List<List<char>> _allowedMask = new List<List<char>>();
        public Solver()
        {
            for (int i = 0; i < 5; i++)
            {
                _requiredMask.Add(new List<char>());
                _forbiddenMask.Add(new List<char>());
            }

            for (int i = 0; i < 4; i++)
            {
                _allowedMask.Add(new List<char>());

                var allowedMask = _allowedMask[i];

                foreach (var c in _alphabet)
                {
                    allowedMask.Add(c);
                }
            }
        }

        public void FilterWordList()
        {
            _remainingWordList.FilterWordListWithMasking(_allowedMask, _requiredMask, _forbiddenMask);
        }

        private int CountVowels(string word)
        {
            return word.Count(c => "aeiou".Contains(Char.ToLower(c)));
        }

        public string UseAlreadyFoundPositions()
        {
            List<List<char>> mustHaveMask = new List<List<char>>();
            List<List<char>> forbiddenMask = _requiredMask.ConvertAll(mask => new List<char>(mask));
            List<List<char>> allowedMask = new List<List<char>>();

            List<char> alreadyUsed = new List<char>();
            List<List<char>> unionOfTwo = _requiredMask.ConvertAll(mask => new List<char>(mask));
            for (int i = 0; i < _requiredMask.Count; i++)
            {
                unionOfTwo[i].AddRange(_forbiddenMask[i]);
            }
            foreach (var letters in unionOfTwo)
            {
                foreach (var letter in letters)
                {
                    alreadyUsed.Add(letter);
                }
            }

            List<char> priorityLetters = _allowedMask[1].Except(alreadyUsed).ToList();
            List<char> lettersForAllowedMask = new List<char>();
            if (CountVowels(string.Join("", priorityLetters)) == 0)
            {
                lettersForAllowedMask = priorityLetters.Union(new[] { 'a', 'o', 'e' }).ToList();
            }

            for (int i = 0; i < 5; i++)
            {
                mustHaveMask.Add(new List<char>());
            }
            
            for (int i = 0; i < 4; i++)
            {
                allowedMask.Add(new List<char>());

                if (i == 0)
                {
                    continue;
                }
                
                var tempAllowedMask = allowedMask[i];

                foreach (var letter in lettersForAllowedMask)
                {
                    tempAllowedMask.Add(letter);
                }
            }
            
            foreach (var c in _alphabet)
            {
                allowedMask[0].Add(c);
            }

            WordList tempWordList = _remainingWordList.GetCloneOfWordList();

            tempWordList.FilterWordListWithMasking(allowedMask, mustHaveMask, forbiddenMask);
            if (tempWordList._WordList.Count > 0)
            {
                return tempWordList.GetMaximumUniqueLettersWord(priorityLetters);
            }

            return "";
        }

        public string MakeGuess()
        {
            int hasGreens = _requiredMask.Count(x => x.Count > 0);
            if (hasGreens > 0 && _remainingWordList.GetWordListLength() > 2)
            {
                string bestWithReuse = UseAlreadyFoundPositions();
                if (bestWithReuse != "")
                {
                    return bestWithReuse;
                }
            }
            
            _remainingWordList.CalculateWordScores();
            _remainingWordList.CalculateWordScoresPositional();

            return _remainingWordList.GetHighscoreWord(true);
        }

        private void UpdateMustHaveMask(Guess guess)
        {
            string response = guess.Response;
            for (int i = 0; i < response.Length; i++)
            {
                char c = response[i];
                if (c == 'G')
                {
                    char guessWordChar = guess.Word[i];
                    if (!_requiredMask[i].Contains(guessWordChar))
                    {
                        _requiredMask[i].Add(guessWordChar);
                    }
                }
            }
        }

        private void UpdateForbiddenMask(Guess guess)
        {
            string response = guess.Response;
            for (var i = 0; i < response.Length; i++)
            {
                char c = response[i];
                string guessWord = guess.Word;
                if (c == 'Y')
                {
                    if (!_forbiddenMask[i].Contains(guessWord[i]))
                    {
                        _forbiddenMask[i].Add(guessWord[i]);
                    }
                }

                if (c == 'X' && guessWord.Count(x => x == guessWord[i]) > 1)
                {
                    if (!_forbiddenMask[i].Contains(guessWord[i]))
                    {
                        _forbiddenMask[i].Add(guessWord[i]);
                    }
                }
            }
        }

        private void UpdateAllowedMask(Guess guess)
        {
            Dictionary<char, List<char>> letterCount = new Dictionary<char, List<char>>();
            for (int i = 0; i < guess.Word.Length; i++)
            {
                char letter = guess.Word[i];
                if (letterCount.ContainsKey(letter))
                {
                    letterCount[letter].Add(guess.Response[i]);
                }
                else
                {
                    letterCount.Add(letter, new List<char>{guess.Response[i]});
                }
            }

            foreach (var letterCountPair in letterCount)
            {
                var letter = letterCountPair.Key;
                var listVal = letterCountPair.Value;
                if (listVal.Contains('X'))
                {
                    int allowedCount = listVal.Count - listVal.Count(x => x == 'X');
                    for (int i = allowedCount + 1; i < 4; i++)
                    {
                        if (_allowedMask[i].Contains(letter))
                        {
                            _allowedMask[i].Remove(letter);
                        }
                    }
                }

                if (listVal.Contains('Y') || listVal.Contains('G'))
                {
                    int requiredCount = listVal.Count(x => x == 'Y') + listVal.Count(x => x == 'G');
                    for (int i = 0; i < requiredCount; i++)
                    {
                        if (_allowedMask[i].Contains(letter))
                        {
                            _allowedMask[i].Remove(letter);
                        }
                    }
                }
            }
        }

        public void UpdateMasks(Guess guess)
        {
            UpdateMustHaveMask(guess);
            UpdateForbiddenMask(guess);
            UpdateAllowedMask(guess);
        }

        public void UpdateMaskWithRemainingWords()
        {
            _remainingWordList.GenerateLetterCount();
            foreach (var letterCountPair in _remainingWordList.LetterCount)
            {
                if (letterCountPair.Value == 0)
                {
                    for (int i = 1; i < 3; i++)
                    {
                        if (_allowedMask[i].Contains(letterCountPair.Key))
                        {
                            _allowedMask[i].Remove(letterCountPair.Key);
                        }
                    }
                }
            }
        }

        public void RemoveWord(string word)
        {
            if (_remainingWordList._WordList.Contains(word))
            {
                _remainingWordList._WordList.Remove(word);
            }
        }
    }
}