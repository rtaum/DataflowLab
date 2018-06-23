using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataflowLab
{
    public class InternetWordProcessingPipeline : IDataFlow<string>
    {
        private TransformBlock<string, string> _downloadString;

        private TransformBlock<string, string[]> _createWordList;

        private TransformBlock<string[], string[]> _filterWordList;

        private TransformManyBlock<string[], string> _findReversedWords;

        private ActionBlock<string> _printReversedWords;

        public void BuildPipeline()
        {
            _downloadString = new TransformBlock<string, string>(async uri =>
            {
                Console.WriteLine("Downloading '{0}'...", uri);

                return await new HttpClient().GetStringAsync(uri);
            });

            _createWordList = new TransformBlock<string, string[]>(text =>
            {
                Console.WriteLine("Creating word list...");
                char[] tokens = text.Select(c => char.IsLetter(c) ? c : ' ').ToArray();
                text = new string(tokens);
                return text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            });

            _filterWordList = new TransformBlock<string[], string[]>(words =>
            {
                Console.WriteLine("Filtering word list...");

                return words
                   .Where(word => word.Length > 3)
                   .Distinct()
                   .ToArray();
            });

            _findReversedWords = new TransformManyBlock<string[], string>(words =>
            {
                Console.WriteLine("Finding reversed words...");

                var wordsSet = new HashSet<string>(words);

                return from word in words.AsParallel()
                       let reverse = new string(word.Reverse().ToArray())
                       where word != reverse && wordsSet.Contains(reverse)
                       select word;
            });

            // Prints the provided reversed words to the console.    
            _printReversedWords = new ActionBlock<string>(reversedWord =>
            {
                Console.WriteLine("Found reversed words {0}/{1}",
                   reversedWord, new string(reversedWord.Reverse().ToArray()));
            });
        }

        public async Task SendAsyn(string item)
        {
            await _downloadString.SendAsync(item);
        }

        public async Task Complete()
        {
            _downloadString.Complete();
            await _downloadString.Completion;
            Console.WriteLine("");
        }

        public void Link()
        {
            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

            _downloadString.LinkTo(_createWordList, linkOptions);
            _createWordList.LinkTo(_filterWordList, linkOptions);
            _filterWordList.LinkTo(_findReversedWords, linkOptions);
            _findReversedWords.LinkTo(_printReversedWords, linkOptions);
        }
    }
}
