using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LexiGrid.DataStructures;

// Diagnostic only: compile alongside the unchanged canonical Trie/TrieNode sources.
// Measures lookup after loading, not Unity/WebAssembly or end-to-end input latency.
public static class LexiGridLookupBenchmark
{
    private static volatile int sink;
    private static string[] words;
    private static Trie trie;
    private static HashSet<string> set;

    private static bool LinearWord(string query)
    {
        string value = query.ToLowerInvariant();
        foreach (string word in words)
            if (string.Equals(word, value, StringComparison.Ordinal)) return true;
        return false;
    }

    private static bool LinearPrefix(string query)
    {
        string value = query.ToLowerInvariant();
        foreach (string word in words)
            if (word.StartsWith(value, StringComparison.Ordinal)) return true;
        return false;
    }

    private static bool HashWord(string query) { return set.Contains(query.ToLowerInvariant()); }

    private static string[] Queries(Random random, bool prefix)
    {
        var queries = new List<string>();
        for (int i = 0; i < 512; i++)
        {
            string word = words[random.Next(words.Length)];
            string hit = prefix ? word.Substring(0, random.Next(3, Math.Min(8, word.Length) + 1)) : word;
            queries.Add(hit.ToUpperInvariant());
            string miss = hit;
            do
            {
                char[] chars = hit.ToCharArray();
                chars[random.Next(chars.Length)] = (char)('a' + random.Next(26));
                miss = new string(chars);
            } while (prefix ? trie.StartsWith(miss) : trie.Search(miss));
            queries.Add(miss.ToUpperInvariant());
        }
        // Same seeded query order for all methods, with exactly 50% hits and misses.
        for (int i = queries.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            string temp = queries[i]; queries[i] = queries[j]; queries[j] = temp;
        }
        return queries.ToArray();
    }

    private static double Measure(Func<string, bool> lookup, string[] queries, int repeats)
    {
        int found = 0;
        var timer = Stopwatch.StartNew();
        for (int r = 0; r < repeats; r++)
            foreach (string query in queries) if (lookup(query)) found++;
        timer.Stop();
        sink = found;
        if (found != 512 * repeats) throw new Exception("Incorrect lookup count");
        return timer.Elapsed.TotalMilliseconds / repeats;
    }

    private static object Summary(double[] milliseconds, int count)
    {
        double[] ordered = milliseconds.OrderBy(x => x).ToArray();
        double median = ordered[ordered.Length / 2];
        return new { MedianBatchMs = median, MedianMicrosecondsPerQuery = median * 1000 / count,
            BatchMsSamples = milliseconds };
    }

    public static object Run(string dictionaryPath)
    {
        words = File.ReadAllLines(dictionaryPath).Select(x => x.Trim().ToLowerInvariant())
            .Where(x => x.Length >= 3 && x.All(char.IsLetter)).ToArray();
        var timer = Stopwatch.StartNew();
        trie = new Trie(); foreach (string word in words) trie.Insert(word);
        timer.Stop(); double trieBuildMs = timer.Elapsed.TotalMilliseconds;
        timer.Restart(); set = new HashSet<string>(words, StringComparer.Ordinal);
        timer.Stop(); double hashBuildMs = timer.Elapsed.TotalMilliseconds;
        var random = new Random(20261015);
        string[] exact = Queries(random, false), prefix = Queries(random, true);
        foreach (string q in exact)
            if (LinearWord(q) != trie.Search(q) || HashWord(q) != trie.Search(q))
                throw new Exception("Exact methods disagree");
        foreach (string q in prefix)
            if (LinearPrefix(q) != trie.StartsWith(q)) throw new Exception("Prefix methods disagree");

        // Warm each method; small fast-method batches are repeated to improve timing resolution.
        Measure(trie.Search, exact, 100); Measure(HashWord, exact, 100);
        Measure(LinearWord, exact, 1); Measure(trie.StartsWith, prefix, 100);
        Measure(LinearPrefix, prefix, 1);
        double[] te = new double[5], he = new double[5], le = new double[5];
        double[] tp = new double[5], lp = new double[5];
        for (int r = 0; r < 5; r++)
        {
            // Reverse order on alternating rounds to reduce fixed-order effects.
            if (r % 2 == 0)
            {
                le[r] = Measure(LinearWord, exact, 1); he[r] = Measure(HashWord, exact, 100);
                te[r] = Measure(trie.Search, exact, 100);
                lp[r] = Measure(LinearPrefix, prefix, 1); tp[r] = Measure(trie.StartsWith, prefix, 100);
            }
            else
            {
                tp[r] = Measure(trie.StartsWith, prefix, 100); lp[r] = Measure(LinearPrefix, prefix, 1);
                te[r] = Measure(trie.Search, exact, 100); he[r] = Measure(HashWord, exact, 100);
                le[r] = Measure(LinearWord, exact, 1);
            }
        }
        return new { Runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            Seed = 20261015, Words = words.Length, TrieWords = trie.WordCount,
            QueriesPerWorkload = exact.Length, HitPercent = 50, Rounds = 5,
            Correctness = "All exact/prefix results agree; each batch has 512 hits and 512 misses",
            TrieBuildMs = trieBuildMs, HashSetBuildMs = hashBuildMs,
            ExactQueryMeanLength = exact.Average(x => x.Length),
            PrefixQueryMeanLength = prefix.Average(x => x.Length),
            ExactLinear = Summary(le, exact.Length), ExactTrie = Summary(te, exact.Length),
            ExactHashSet = Summary(he, exact.Length), PrefixLinear = Summary(lp, prefix.Length),
            PrefixTrie = Summary(tp, prefix.Length) };
    }
}
