using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kip.Utils.Lucene
{
    public static class AnalyzerUtils
    {
        public static void displayTokensWithPositions(Analyzer analyzer, string text)
        {
            TokenStream stream = analyzer.TokenStream("contents", new StringReader(text));

            TermAttribute term = stream.AddAttribute<TermAttribute>();
            PositionIncrementAttribute posIncr = stream.AddAttribute<PositionIncrementAttribute>();

            int position = 0;
            while (stream.IncrementToken())
            {
                int increament = posIncr.PositionIncrement;
                if (increament > 0)
                {
                    position = position + increament;
                    Console.WriteLine(position + ":");
                }

                Console.Write("[{0}] ", term.Term);
            }
            Console.WriteLine();
        }
    }
}
