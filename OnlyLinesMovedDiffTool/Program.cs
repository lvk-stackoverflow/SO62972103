using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DiffLib;

namespace OnlyLinesMovedDiffTool
{
    static class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Out.WriteLine("error: invalid configuration");
                return 1;
            }

            string[] source = File.ReadAllLines(args[0]);
            string[] target = File.ReadAllLines(args[1]);

            List<DiffSection> sections = Diff.CalculateSections(source, target).ToList();
            List<DiffElement<string>> aligned = Diff.AlignElements(source, target, sections, new StringSimilarityDiffElementAligner())
               .ToList();

            ILookup<DiffOperation, List<DiffElement<string>>> operations =
                aligned.GroupBy(de => de.Operation).ToLookup(g => g.Key, g => g.ToList());

            if (operations[DiffOperation.Modify].Any() || operations[DiffOperation.Replace].Any())
            {
                ShowSummary(operations);
                return 1;
            }

            List<(DiffElement<string> insertedElement, DiffElement<string> deletedElement)> moved =
                new List<(DiffElement<string> insertedElement, DiffElement<string> deletedElement)>();
            List<DiffElement<string>> inserted = operations[DiffOperation.Insert].SelectMany(l => l).ToList();
            List<DiffElement<string>> deleted = operations[DiffOperation.Delete].SelectMany(l => l).ToList();

            foreach (DiffElement<string> insertedElement in inserted.ToList())
            {
                DiffElement<string> deletedElement = deleted.Find(d => d.ElementFromCollection1.Value == insertedElement.ElementFromCollection2.Value);
                if (!deletedElement.ElementIndexFromCollection1.HasValue)
                    continue;

                inserted.Remove(insertedElement);
                deleted.Remove(deletedElement);
                moved.Add((insertedElement, deletedElement));
            }

            foreach ((DiffElement<string> insertedElement, DiffElement<string> deletedElement) entry in moved.OrderBy(el => el.deletedElement.ElementIndexFromCollection1.Value))
            {
                Console.WriteLine(
                    $"Line {entry.deletedElement.ElementIndexFromCollection1+1} moved to {entry.insertedElement.ElementIndexFromCollection2+1}");

                Console.WriteLine($"   {entry.deletedElement.ElementFromCollection1.Value}");
            }

            return 0;
        }

        private static void ShowSummary(ILookup<DiffOperation,List<DiffElement<string>>> operations)
        {
            Console.WriteLine("Diff consisted of other operations than just moves:");
            Report(DiffOperation.Delete, "deleted");
            Report(DiffOperation.Insert, "inserted");
            Report(DiffOperation.Modify, "modified in-place");
            Report(DiffOperation.Replace, "replaced");

            void Report(DiffOperation operation, string description)
            {
                if (!operations[operation].Any())
                    return;

                int count = operations[operation].Count();
                Console.WriteLine($"  {count} line{(count > 1 ? "s" : "")} was {description}");
            }
        }
    }
}