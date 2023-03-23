using System;
using System.Collections.Generic;
using System.Linq;

namespace WordleSolver
{
    public class Solver
    {
        private readonly WordList _remainingWordList = new WordList();

        // masks are lists of lists of characters
        // the outer list represents the position of the letter in the word
        // the inner list represents the letters that are allowed at that position
        // for example, if the correct word is "hello" then the required mask would be [["h"], ["e"], ["l"], ["l"], ["o"]]
        private readonly Mask _requiredMask = new Mask(5);
        private readonly Mask _forbiddenMask = new Mask(5);
        private readonly Mask _allowedMask = new Mask(4);
        public Solver()
        {
            _allowedMask.AddToAllPositionMasks(Globals.Alphabet); // initialise our allowed mask by adding alphabet to all submasks
        }
        
        private static int CountVowels(string word)
        {
            // count the number of vowels in a word by checking a condition on count for if the letter is in our vowel character list
            return word.Count(letter => Globals.Vowels.Contains(Char.ToLower(letter)));
        }

        private void FilterWordList()
        {
            _remainingWordList.FilterWordListUsingMasks(_allowedMask, _requiredMask, _forbiddenMask);
        }

        public bool HasGuessesRemaining()
        {
            return _remainingWordList.GetWordListLength() > 0; // check if there are any words left in the word list
        }

        private string UseAlreadyFoundPositions()
        {
            // here we invert the required mask and the forbidden mask as we're looking for any words that might help us get more information by reusing correct letter spaces
            var invertedMustHaveMask = new Mask(5);
            var invertedForbiddenMask = new Mask(5);
            invertedForbiddenMask.CopyOtherMask(_requiredMask); // make a new list using a "deep copy" of the required mask
            var invertedAllowedMask = new Mask(4);

            // get all letters that have already been used, so we can avoid using them again
            // we do this by calling a GetAllUnique method which returns unique letters for each mask
            // we then use .Distinct() to remove duplicates and .ToList() to turn it back into a List
            var alreadyUsed = new List<char>();
            alreadyUsed.AddRange(_requiredMask.GetAllUnique());
            alreadyUsed.AddRange(_forbiddenMask.GetAllUnique());
            alreadyUsed = alreadyUsed.Distinct().ToList();

            // get all letters that have not been used yet, so we can use them in the allowed mask
            // we do this by taking the difference of the allowed mask and the already used letters
            // if there are no vowels in the difference, then we add a few vowels to the allowed mask
            var priorityLetters = _allowedMask.GetIndex(1).Except(alreadyUsed).ToList();
            var lettersForAllowedMask = new List<char>();
            lettersForAllowedMask.AddRange(priorityLetters); // add the priority letters to the allowed mask
            if (CountVowels(string.Join("", priorityLetters)) == 0)
            {
                lettersForAllowedMask.AddRange(new[] { 'a', 'o', 'e' }); // check if there are any vowels in the priority letters, if not, add some
            }
            lettersForAllowedMask = lettersForAllowedMask.Distinct().ToList(); // remove duplicates

            invertedAllowedMask.AddToAllPositionMasks(lettersForAllowedMask, 1);
            invertedAllowedMask.AddToPositionMask(Globals.Alphabet, 0);

            var invertedWordList = new WordList(); // make a new word list object so we don't mess up the original

            invertedWordList.FilterWordListUsingMasks(invertedAllowedMask, invertedMustHaveMask, invertedForbiddenMask); // filter the temp word list with the inverted masks
            if (invertedWordList.GetWordListLength() > 0)
            {
                return invertedWordList.CalculateReuseBestGuessWordUsingMinimax(priorityLetters); // return the word with the most unique letters this is to try minimax the number of guesses
            }

            return null;
        }

        private bool CanReuseGreen()
        {
            var hasGreens = _requiredMask.GetCountOfAllPositionMasks();
            return hasGreens > 0 && _remainingWordList.GetWordListLength() > 2; // boolean check to see if we have any greens to make use of all positions that have already been found by reusing them
        }

        private string HandleNormalGuessCalculation()
        {
            _remainingWordList.CalculateAllWordScores(); // calculate the scores for each word in the word list
            return _remainingWordList.CalculateBestGuessWordMiniMax();
        }

        public string MakeGuess()
        {
            if (!CanReuseGreen())
            {
                return HandleNormalGuessCalculation();
            }

            string bestGuessUsingReuse = UseAlreadyFoundPositions();
            return bestGuessUsingReuse ?? HandleNormalGuessCalculation();
        }

        private void UpdateMustHaveMask(Guess guess)
        {
            var guessFeedback = guess.Feedback; // update mask by checking guess for greens
            for (var i = 0; i < guessFeedback.Length; i++)
            {
                if (guessFeedback[i] != 'G')
                {
                    continue;
                }
                
                var guessWordChar = guess.Word[i];
                _requiredMask.AddToPositionMaskUnique(i, guessWordChar);
            }
        }

        private void UpdateForbiddenMask(Guess guess)
        {
            var guessFeedback = guess.Feedback; // update forbidden mask by checking guess for yellows and greys
            for (var i = 0; i < guessFeedback.Length; i++)
            {
                var feedbackLetter = guessFeedback[i];
                var guessWord = guess.Word;
                var guessWordChar = guessWord[i];
                
                if (feedbackLetter == 'Y')
                {
                    _forbiddenMask.AddToPositionMaskUnique(i, guessWordChar);
                }

                if (feedbackLetter == 'X' && guessWord.Count(x => x == guessWordChar) > 1)
                {
                    _forbiddenMask.AddToPositionMaskUnique(i, guessWordChar);
                }
            }
        }
        
        private void UpdateAllowedMask(Guess guess)
        {
            var feedbackLetterGuessRelation = new Dictionary<char, List<char>>();
            for (var i = 0; i < guess.Word.Length; i++) // count the number of each letter in the guess
            {
                var letter = guess.Word[i];
                feedbackLetterGuessRelation.TryAdd(letter, new List<char>()); // attempt to add letter to dictionary initialising a new list if it doesn't exist otherwise this just skips
                feedbackLetterGuessRelation[letter].Add(guess.Feedback[i]);
            }

            foreach (var pair in feedbackLetterGuessRelation) // this updates our allowed mask based on the guess
            {
                HandleAllowedMaskUpdate(pair.Key, pair.Value);
            }
        }

        private void HandleAllowedMaskUpdate(char guessLetter, List<char> guessFeedbackLetters)
        {
            if (guessFeedbackLetters.Contains('X'))
            {
                var allowedCount = guessFeedbackLetters.Count - guessFeedbackLetters.Count(x => x == 'X');
                for (var i = allowedCount + 1; i < 4; i++)
                {
                    _allowedMask.RemoveFromPositionMask(i, guessLetter);
                }
            }

            if (!(guessFeedbackLetters.Contains('Y') || guessFeedbackLetters.Contains('G')))
            {
                return;
            }
            
            var requiredCount = guessFeedbackLetters.Count(x => x == 'Y') + guessFeedbackLetters.Count(x => x == 'G');
            for (var i = 0; i < requiredCount; i++)
            {
                _allowedMask.RemoveFromPositionMask(i, guessLetter);
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

            if (_remainingWordList.GetWordListLength() > 20)
            {
                return;
            }
            
            _remainingWordList.CalculateAllWordScores();
            Console.WriteLine(_remainingWordList.GetWordListAsStringOrdered()); // print the ordered word list if there are 20 words or less remaining
        }
    }
}