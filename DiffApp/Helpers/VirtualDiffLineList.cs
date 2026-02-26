namespace DiffApp.Helpers
{
    public class VirtualDiffLineList : IList<DiffLineViewModel>, IList
    {
        private readonly List<BlockMapping> _mappings = new();
        private readonly bool _isUnifiedMode;
        private int _count;

        private struct BlockMapping
        {
            public int StartIndex;
            public ChangeBlock Block;
            public int Height;
        }

        public VirtualDiffLineList(ComparisonResult result, bool isUnifiedMode)
        {
            _isUnifiedMode = isUnifiedMode;
            if (result?.Blocks != null)
            {
                BuildMappings(result.Blocks);
            }
        }

        private void BuildMappings(IEnumerable<ChangeBlock> blocks)
        {
            int currentIndex = 0;

            foreach (var block in blocks)
            {
                int height = 0;

                if (_isUnifiedMode)
                {
                    if (block.Kind == BlockType.Modified)
                    {
                        height = block.OldLines.Count + block.NewLines.Count;
                    }
                    else if (block.Kind == BlockType.Added)
                    {
                        height = block.NewLines.Count;
                    }
                    else if (block.Kind == BlockType.Removed)
                    {
                        height = block.OldLines.Count;
                    }
                    else
                    {
                        height = block.OldLines.Count;
                    }
                }
                else
                {
                    height = Math.Max(block.OldLines.Count, block.NewLines.Count);
                }

                if (height > 0)
                {
                    _mappings.Add(new BlockMapping
                    {
                        StartIndex = currentIndex,
                        Block = block,
                        Height = height
                    });
                    currentIndex += height;
                }
            }

            _count = currentIndex;
        }

        public DiffLineViewModel this[int index]
        {
            get => CreateViewModel(index);
            set => throw new NotSupportedException();
        }

        object? IList.this[int index]
        {
            get => this[index];
            set => throw new NotSupportedException();
        }

        public int Count => _count;

        public bool IsReadOnly => true;

        public bool IsFixedSize => true;

        public object SyncRoot => this;

        public bool IsSynchronized => false;

        private DiffLineViewModel CreateViewModel(int globalIndex)
        {
            int mapIndex = BinarySearchBlock(globalIndex);
            var mapping = _mappings[mapIndex];
            int localIndex = globalIndex - mapping.StartIndex;

            bool isFirst = (localIndex == 0);
            bool isLast = (localIndex == mapping.Height - 1);

            if (_isUnifiedMode)
            {
                return CreateUnifiedViewModel(mapping.Block, localIndex, isFirst, isLast);
            }
            else
            {
                return CreateSplitViewModel(mapping.Block, localIndex, isFirst, isLast);
            }
        }

        private int BinarySearchBlock(int globalIndex)
        {
            int left = 0;
            int right = _mappings.Count - 1;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                var map = _mappings[mid];

                if (globalIndex >= map.StartIndex && globalIndex < map.StartIndex + map.Height)
                {
                    return mid;
                }

                if (globalIndex < map.StartIndex)
                {
                    right = mid - 1;
                }
                else
                {
                    left = mid + 1;
                }
            }

            return Math.Min(left, _mappings.Count - 1);
        }

        private DiffLineViewModel CreateSplitViewModel(ChangeBlock block, int localIndex, bool isFirst, bool isLast)
        {
            var leftLine = localIndex < block.OldLines.Count ? block.OldLines[localIndex] : null;
            var rightLine = localIndex < block.NewLines.Count ? block.NewLines[localIndex] : null;

            return new DiffLineViewModel(block, leftLine, rightLine, isFirst, isLast);
        }

        private DiffLineViewModel CreateUnifiedViewModel(ChangeBlock block, int localIndex, bool isFirst, bool isLast)
        {
            if (block.Kind == BlockType.Modified)
            {
                if (localIndex < block.OldLines.Count)
                {
                    return new DiffLineViewModel(block, block.OldLines[localIndex], null, isFirst, isLast);
                }
                else
                {
                    return new DiffLineViewModel(block, null, block.NewLines[localIndex - block.OldLines.Count], isFirst, isLast);
                }
            }
            else if (block.Kind == BlockType.Added)
            {
                return new DiffLineViewModel(block, null, block.NewLines[localIndex], isFirst, isLast);
            }
            else if (block.Kind == BlockType.Removed)
            {
                return new DiffLineViewModel(block, block.OldLines[localIndex], null, isFirst, isLast);
            }
            else
            {
                return new DiffLineViewModel(block, block.OldLines[localIndex], block.NewLines[localIndex], isFirst, isLast);
            }
        }

        public IEnumerator<DiffLineViewModel> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(DiffLineViewModel item) => throw new NotSupportedException();
        public void Clear() => throw new NotSupportedException();
        public bool Contains(DiffLineViewModel item) => false;
        public void CopyTo(DiffLineViewModel[] array, int arrayIndex) => throw new NotSupportedException();
        public bool Remove(DiffLineViewModel item) => throw new NotSupportedException();
        public int IndexOf(DiffLineViewModel item) => -1;
        public void Insert(int index, DiffLineViewModel item) => throw new NotSupportedException();
        public void RemoveAt(int index) => throw new NotSupportedException();
        public int Add(object? value) => throw new NotSupportedException();
        public bool Contains(object? value) => false;
        public int IndexOf(object? value) => -1;
        public void Insert(int index, object? value) => throw new NotSupportedException();
        public void Remove(object? value) => throw new NotSupportedException();
        public void CopyTo(Array array, int index) => throw new NotSupportedException();
    }
}