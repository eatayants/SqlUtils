using System;
using System.Collections;
using System.Collections.Generic;

namespace SqlUtils
{
    internal class History : IEnumerable<string>, IEnumerable
    {
	    private const int Blocksize = 0x2710;
	    private readonly List<string> _commandList = new List<string>();
        private static readonly TimeSpan DiscardTimeout = new TimeSpan(1, 0, 0, 0);
        private List<DateTime> _insertionTimes;
	    private const int MaxlistsizeHard = 0x7fffffff;
	    private const int MaxlistsizeSoft = 0x1fffc;

	    internal void Add(string command)
        {
            this._commandList.Add(command);
            if ((this._commandList.Count % Blocksize) == 0)
            {
                if (this._insertionTimes == null)
                {
                    this._insertionTimes = new List<DateTime>();
                }
                this._insertionTimes.Add(DateTime.Now);
            }
            if (this._commandList.Count == MaxlistsizeHard)
            {
                Console.WriteLine(@"WARNING: History memory limit reached. Removing bottom ~25%.");
                this.RemoveBlocks((MaxlistsizeHard >> 2) / Blocksize);
            }
            else if (this._commandList.Count > MaxlistsizeSoft)
            {
                int blockCount = 0;
                foreach (DateTime time in this._insertionTimes)
                {
                    if ((DateTime.Now - time) > DiscardTimeout)
                    {
                        blockCount++;
                    }
                }
                if (blockCount > 0)
                {
                    Console.WriteLine(@"WARNING: Clearing history older than 1 day to relieve memory pressure.");
                    this.RemoveBlocks(blockCount);
                }
            }
        }

        internal void Clear()
        {
            this._commandList.Clear();
            if (this._insertionTimes != null)
            {
                this._insertionTimes.Clear();
            }
        }

        private void RemoveBlocks(int blockCount)
        {
            this._commandList.RemoveRange(0, blockCount * Blocksize);
            this._insertionTimes.RemoveRange(0, blockCount);
        }

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            return _commandList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _commandList.GetEnumerator();
        }

        internal int Count
        {
            get
            {
                return _commandList.Count;
            }
        }

        internal string this[int index]
        {
            get
            {
                return _commandList[index];
            }
        }
    }
}

