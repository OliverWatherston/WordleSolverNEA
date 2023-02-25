using System;
using System.Collections.Generic;
using System.Linq;

namespace WordleSolver
{
    public class Solver
    {
        private WordList _remainingWordList = new WordList();

        // masks are lists of lists of characters
        // the outer list represents the position of the letter in the word
        // the inner list represents the letters that are allowed at that position
        // for example, if the correct word is "hello" then the required mask would be [["h"], ["e"], ["l"], ["l"], ["o"]]
        private List<List<char>> _requiredMask = new List<List<char>>();
        private List<List<char>> _forbiddenMask = new List<List<char>>();
        private List<List<char>> _allowedMask = new List<List<char>>();
        public Solver()
        {
            for (int i = 0; i < 5; i++) // initialize the masks
            {
                _requiredMask.Add(new List<char>());
                _forbiddenMask.Add(new List<char>());
            }

            for (int i = 0; i < 4; i++) // initialize the allowed mask
            {
                _allowedMask.Add(new List<char>());

                var allowedMask = _allowedMask[i];

                foreach (var letter in Globals.Alphabet) // add all the letters to the allowed mask
                {
                    allowedMask.Add(letter);
                }
            }
        }

        private void FilterWordList()
        {
            _remainingWordList.FilterWordListWithMasking(_allowedMask, _requiredMask, _forbiddenMask);
        }

        private int CountVowels(string word)
        {
            return word.Count(c => "aeiou".Contains(Char.ToLower(c)));
        }

        private string UseAlreadyFoundPositions()
        {
            // here we invert the required mask and the forbidden mask as we're looking for any words that might help us get more information by reusing correct letter spaces
            List<List<char>> mustHaveMask = new List<List<char>>();
            List<List<char>> forbiddenMask = _requiredMask.ConvertAll(mask => new List<char>(mask)); // make a new list using a "deep copy" of the required mask
            List<List<char>> allowedMask = new List<List<char>>();

            // get all letters that have already been used, so we can avoid using them again
            // we do this by taking the union of the required mask and the forbidden mask
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

            // get all letters that have not been used yet, so we can use them in the allowed mask
            // we do this by taking the difference of the allowed mask and the already used letters
            // if there are no vowels in the difference, then we add a few vowels to the allowed mask
            List<char> priorityLetters = _allowedMask[0].Except(alreadyUsed).ToList();
            List<char> lettersForAllowedMask = new List<char>();
            if (CountVowels(string.Join("", priorityLetters)) == 0)
            {
                lettersForAllowedMask = priorityLetters.Union(new[] { 'a', 'o', 'e' }).ToList();
            }

            for (int i = 0; i < 5; i++) // initialize the masks
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
            
            foreach (var letter in Globals.Alphabet)
            {
                allowedMask[0].Add(letter);
            }

            WordList tempWordList = _remainingWordList.GetCloneOfWordList(); // make a copy of the word list so we don't mess up the original

            tempWordList.FilterWordListWithMasking(allowedMask, mustHaveMask, forbiddenMask); // filter the word list with the inverted masks
            if (tempWordList.GetWordListLength() > 0)
            {
                return tempWordList.CalculateWordMaxUnique(priorityLetters); // return the word with the most unique letters this is to try minimax the number of guesses
            }

            return "";
        }

        public string MakeGuess()
        {
            int hasGreens = _requiredMask.Count(x => x.Count > 0); // minimax reuse solution can only be used if there is at least one green
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
            
            return _remainingWordList.CalculateWordMiniMax(true);
        }

        private void UpdateMustHaveMask(Guess guess)
        {
            string response = guess.Response; // update mask by checking guess for greens
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
            string response = guess.Response; // update forbidden mask by checking guess for yellows and greys
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
            for (int i = 0; i < guess.Word.Length; i++) // count the number of each letter in the guess
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

            foreach (var letterCountPair in letterCount) // this updates our allowed mask based on the guess
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

        private void UpdateMasks(Guess guess)
        {
            UpdateMustHaveMask(guess);
            UpdateForbiddenMask(guess);
            UpdateAllowedMask(guess);
        }

        private void UpdateMaskWithRemainingWords()
        {
            _remainingWordList.UpdateMaskWithRemainingWords(_allowedMask);
        }

        private void RemoveWord(string word)
        {
            _remainingWordList.TryRemoveWord(word);
        }

        public void UpdateSolver(Guess guess)
        {
            // this handles the logic of updating the solver and word list after each guess
            RemoveWord(guess.Word);
            
            UpdateMasks(guess);
            
            FilterWordList();
            
            UpdateMaskWithRemainingWords();
            
            Console.WriteLine($"{_remainingWordList.GetWordListLength()} words remaining.");

            if (_remainingWordList.GetWordListLength() <= 20)
            {
                _remainingWordList.CalculateWordScores();
                Console.WriteLine(_remainingWordList.GetWordListAsStringOrdered()); // print the ordered word list if there are 20 words or less remaining
            }
        }
    }
}