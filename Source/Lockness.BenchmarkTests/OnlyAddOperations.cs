﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using BenchmarkDotNet.Attributes.Exporters;

namespace CodeDonkeys.Lockness.BenchmarkTests
{
    [RankColumn]
    [Config(typeof(GeneralConfig))]
    [PlainExporter]
    public class OnlyAddOperations
    {
        private readonly Dictionary<string, Type> setNames = new Dictionary<string, Type>
        {
            {"HarrisLinkedList", typeof(HarrisLinkedList<int>)},
            {"HarrisLinkedListWithBacklinkAndSuccessorFlag", typeof(HarrisLinkedListWithBacklinkAndSuccessorFlag<int>)},
            {"SkipListWithBacklink", typeof(SkipListWithBacklink<int>)},
            {"StrippedHashTable", typeof(StripedHashTable<int>)},
            {"LockBasedHashTable", typeof(LockBasedHashTable<int>)},
            {"LockBasedList", typeof(LockBasedList<int>)}
        };

        private Task[] tasks;

        [Params(100, 10000, 1000000)]
        public int KeysCount { get; set; }

        [Params(1, 2, 8)]
        public int ThreadsCount { get; set; }

        [Params("HarrisLinkedList", "HarrisLinkedListWithBacklinkAndSuccessorFlag", "SkipListWithBacklink", "StrippedHashTable", "LockBasedHashTable", "LockBasedList")]
        public string TypeName { get; set; }

        [Params("Direct", "Reverse", "Random")]
        public string Order { get; set; }

        [IterationSetup]
        public void Initialize()
        {
            var set = (ISet<int>)Activator.CreateInstance(setNames[TypeName], Comparer<int>.Default);
            var tasksInDirectOrder = Enumerable.Range(0, ThreadsCount).Select(i => new Task(() => AddElement(i, set)));
            switch (Order)
            {
                case "Reverse":
                    tasks = tasksInDirectOrder.Reverse().ToArray();
                    break;
                case "Random":
                    tasks = tasksInDirectOrder.OrderBy(a => Guid.NewGuid()).ToArray();
                    break;
                default:
                    tasks = tasksInDirectOrder.ToArray();
                    break;
            }
        }

        private void AddElement(int start, ISet<int> collection)
        {
            for (var i = start; i < KeysCount; i += ThreadsCount)
            {
                collection.Add(i);
            }

        }

        [Benchmark]
        public Task[] AddList()
        {
            foreach (var task in tasks)
            {
                task.Start();
            }
            
            Task.WaitAll(tasks);

            return tasks;
        }
    }
}