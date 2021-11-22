using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cryptogram_backend.Modules
{
    public class Scrambler
    {
        public String Answer { get; }
        public String ScrambledAnswer { get; }

        public Scrambler(string answer)
        {
            Answer = answer;
            ScrambledAnswer = CreateCryptogram(answer);
        }

        private String CreateCryptogram(string answer)
        {
            String acceptedLetters = "qwertyuiopasdfghjklöäzxcvbnm";
            String unUsedLetters = "qwertyuiopasdfghjklöäzxcvbnm";
            List<LetterDictionary> letterList = new List<LetterDictionary>();
            String scrambledAnswer = "";
            foreach (char _letter in answer)
            {
                char letter = char.ToLower(_letter);
                var foundLetter = letterList.Where(c => c.A == letter).FirstOrDefault();

                if (foundLetter != null)
                {
                    scrambledAnswer += foundLetter.B;
                    continue;
                }
                if (acceptedLetters.Contains(letter))
                {
                    int randNum = new Random().Next(0, unUsedLetters.Length - 1);
                    scrambledAnswer += unUsedLetters[randNum];
                    letterList.Add(new LetterDictionary { A = letter, B = unUsedLetters[randNum] });

                    unUsedLetters = unUsedLetters.Remove(randNum, 1);
                    continue;
                }
                scrambledAnswer += letter;
            }

            return scrambledAnswer;
        }
    }

    public class LetterDictionary
    {
        public char A;
        public char B;
    }
}
