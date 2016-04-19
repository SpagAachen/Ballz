using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ballz.Utils;

namespace Ballz.SessionFactory
{
    static class TeamNames
    {
        
        static Dictionary<string, string[]> BallNamesByCountry = new Dictionary<string, string[]>
        {
            {
                "Germoney",
                new string[]
                {
                    "Klaus",
                    "Peter",
                    "Friedrich",
                    "Hans",
                    "Dieter",
                    "Martin",
                    "Ulrich",
                    "Wolfgang",
                    "Uwe",
                    "Anna",
                    "Brigitte",
                    "Heike",
                    "Klaudia",
                    "Monika",
                    "Ulrike",
                    "Simone",
                    "Laura",
                    "Melanie"
                }
            },
            {
                "Murica",
                new string[]
                {
                    "Steve",
                    "Bill",
                    "Bob",
                    "Ryan",
                    "Tyler",
                    "Jason",
                    "Kyle",
                    "Jake",
                    "Trevor",
                    "Andy",
                    "Emily",
                    "Emma",
                    "Brianna",
                    "Kaitlyn",
                    "Ashley",
                    "Megan",
                    "Rachel",
                    "Mary",
                    "Brenda"
                }
            },
        };

        static Random random = new Random();

        public static string[] GetBallNames(string Country, int count)
        {
            var randomizedNames = BallNamesByCountry[Country].ToArray();
            random.Shuffle(randomizedNames);
            return randomizedNames.Take(count).ToArray();
        }
    }
}
