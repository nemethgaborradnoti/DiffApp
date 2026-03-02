using DiffPlex;
using DiffPlex.Chunkers;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using DiffPlexPiece = DiffPlex.DiffBuilder.Model.DiffPiece;
using DiffPlexChangeType = DiffPlex.DiffBuilder.Model.ChangeType;

namespace DiffApp.Services
{
    public class ComparisonService : IComparisonService
    {
        private readonly IChunker _characterChunker;
        private readonly IChunker _lineChunker;
        private readonly IChunker _wordChunker;
        private readonly Differ _differ;
        private readonly SideBySideDiffBuilder _diffBuilder;
        private readonly InlineDiffBuilder _inlineDiffBuilder;

        public ComparisonService()
        {
            _characterChunker = new CharacterChunker();
            _lineChunker = new LineChunker();
            _wordChunker = new WordChunker();
            _differ = new Differ();
            _diffBuilder = new SideBySideDiffBuilder(_differ, _lineChunker, _wordChunker);
            _inlineDiffBuilder = new InlineDiffBuilder(_differ);
        }

        public ComparisonResult Compare(string oldText, string newText, CompareSettings settings)
        {
            string oldTextProcessed = oldText ?? string.Empty;
            string newTextProcessed = newText ?? string.Empty;

            var diffModel = _diffBuilder.BuildDiffModel(oldTextProcessed, newTextProcessed, ignoreWhitespace: false);

            var blocks = BuildBlocks(diffModel, settings);

            blocks = CoalesceBlocks(blocks);

            blocks = AlignModifiedBlocks(blocks);

            return new ComparisonResult(blocks);
        }

        private List<ChangeBlock> BuildBlocks(SideBySideDiffModel model, CompareSettings settings)
        {
            var blocks = new List<ChangeBlock>();
            if (model.OldText.Lines.Count == 0 && model.NewText.Lines.Count == 0)
            {
                return blocks;
            }

            ChangeBlock? currentBlock = null;

            int currentRealIndexOld = 0;
            int currentRealIndexNew = 0;

            for (int i = 0; i < model.OldText.Lines.Count; i++)
            {
                var oldLine = model.OldText.Lines[i];
                var newLine = model.NewText.Lines[i];

                BlockType kind = BlockType.Unchanged;
                bool isWhitespaceChange = false;

                bool areLinesIdentical = string.Equals(oldLine.Text, newLine.Text, StringComparison.Ordinal);

                if (oldLine.Type == DiffPlexChangeType.Imaginary && newLine.Type == DiffPlexChangeType.Inserted)
                {
                    kind = BlockType.Added;
                    isWhitespaceChange = string.IsNullOrWhiteSpace(newLine.Text);
                }
                else if (oldLine.Type == DiffPlexChangeType.Deleted && newLine.Type == DiffPlexChangeType.Imaginary)
                {
                    kind = BlockType.Removed;
                    isWhitespaceChange = string.IsNullOrWhiteSpace(oldLine.Text);
                }
                else if (areLinesIdentical)
                {
                    kind = BlockType.Unchanged;
                    isWhitespaceChange = false;
                }
                else if (oldLine.Type == DiffPlexChangeType.Modified || newLine.Type == DiffPlexChangeType.Modified)
                {
                    kind = BlockType.Modified;
                    isWhitespaceChange = IsWhitespaceOnlyChange(oldLine.Text, newLine.Text);
                }
                else if (oldLine.Type == DiffPlexChangeType.Unchanged && newLine.Type == DiffPlexChangeType.Unchanged)
                {
                    if (!string.Equals(oldLine.Text, newLine.Text, StringComparison.Ordinal))
                    {
                        kind = BlockType.Modified;
                        isWhitespaceChange = true;
                    }
                    else
                    {
                        kind = BlockType.Unchanged;
                    }
                }

                if (currentBlock == null || currentBlock.Kind != kind)
                {
                    if (currentBlock != null)
                    {
                        blocks.Add(currentBlock);
                    }

                    currentBlock = new ChangeBlock
                    {
                        Kind = kind,
                        StartIndexOld = currentRealIndexOld,
                        StartIndexNew = currentRealIndexNew,
                        IsWhitespaceChange = isWhitespaceChange
                    };
                }
                else
                {
                    if (!isWhitespaceChange && currentBlock.IsWhitespaceChange)
                    {
                        currentBlock.IsWhitespaceChange = false;
                    }
                }

                if (kind == BlockType.Modified)
                {
                    GenerateInlineDiff(currentBlock, oldLine, newLine);
                }
                else
                {
                    currentBlock.OldLines.Add(CreateChangeLine(oldLine.Position, oldLine.Type, oldLine.Text, false));
                    currentBlock.NewLines.Add(CreateChangeLine(newLine.Position, newLine.Type, newLine.Text, false));
                }

                if (oldLine.Type != DiffPlexChangeType.Imaginary)
                {
                    currentRealIndexOld++;
                }
                if (newLine.Type != DiffPlexChangeType.Imaginary)
                {
                    currentRealIndexNew++;
                }
            }

            if (currentBlock != null)
            {
                blocks.Add(currentBlock);
            }

            return blocks;
        }

        private List<ChangeBlock> CoalesceBlocks(List<ChangeBlock> sourceBlocks)
        {
            if (sourceBlocks.Count < 2) return sourceBlocks;

            var result = new List<ChangeBlock>();
            var current = sourceBlocks[0];

            for (int i = 1; i < sourceBlocks.Count; i++)
            {
                var next = sourceBlocks[i];

                if (current.Kind != BlockType.Unchanged && next.Kind != BlockType.Unchanged)
                {
                    current.Kind = BlockType.Modified;
                    current.OldLines.AddRange(next.OldLines);
                    current.NewLines.AddRange(next.NewLines);

                    if (!next.IsWhitespaceChange)
                    {
                        current.IsWhitespaceChange = false;
                    }
                }
                else
                {
                    result.Add(current);
                    current = next;
                }
            }

            result.Add(current);
            return result;
        }

        private List<ChangeBlock> AlignModifiedBlocks(List<ChangeBlock> blocks)
        {
            foreach (var block in blocks)
            {
                if (block.Kind == BlockType.Modified)
                {
                    RealignBlock(block);
                }
            }
            return blocks;
        }

        private void RealignBlock(ChangeBlock block)
        {
            var oldRealLines = block.OldLines.Where(x => x.Kind != DiffChangeType.Imaginary).ToList();
            var newRealLines = block.NewLines.Where(x => x.Kind != DiffChangeType.Imaginary).ToList();

            var oldText = string.Join(Environment.NewLine, oldRealLines.Select(x => GetTextFromFragments(x.Fragments)));
            var newText = string.Join(Environment.NewLine, newRealLines.Select(x => GetTextFromFragments(x.Fragments)));

            var subModel = _diffBuilder.BuildDiffModel(oldText, newText, ignoreWhitespace: false);

            var alignedOldLines = new List<ChangeLine>();
            var alignedNewLines = new List<ChangeLine>();

            int oldIndex = 0;
            int newIndex = 0;

            for (int i = 0; i < subModel.OldText.Lines.Count; i++)
            {
                var diffOld = subModel.OldText.Lines[i];
                var diffNew = subModel.NewText.Lines[i];

                if (diffOld.Type == DiffPlexChangeType.Imaginary)
                {
                    alignedOldLines.Add(CreateChangeLine(null, DiffPlexChangeType.Imaginary, (string?)null, true));
                }
                else
                {
                    if (oldIndex < oldRealLines.Count)
                    {
                        var originalLine = oldRealLines[oldIndex];
                        if (diffOld.Type == DiffPlexChangeType.Modified && diffNew.Type == DiffPlexChangeType.Modified)
                        {
                            var inlineDiff = _inlineDiffBuilder.BuildDiffModel(
                                diffOld.Text ?? string.Empty,
                                diffNew.Text ?? string.Empty,
                                ignoreWhitespace: false,
                                ignoreCase: false,
                                _characterChunker);

                            var oldPieces = inlineDiff.Lines.Where(p => p.Type != DiffPlexChangeType.Inserted).ToList();
                            alignedOldLines.Add(CreateChangeLine(originalLine.LineNumber, DiffPlexChangeType.Deleted, oldPieces, true));
                        }
                        else
                        {
                            alignedOldLines.Add(CreateChangeLine(originalLine.LineNumber, diffOld.Type, diffOld.Text, true));
                        }
                        oldIndex++;
                    }
                }

                if (diffNew.Type == DiffPlexChangeType.Imaginary)
                {
                    alignedNewLines.Add(CreateChangeLine(null, DiffPlexChangeType.Imaginary, (string?)null, true));
                }
                else
                {
                    if (newIndex < newRealLines.Count)
                    {
                        var originalLine = newRealLines[newIndex];
                        if (diffOld.Type == DiffPlexChangeType.Modified && diffNew.Type == DiffPlexChangeType.Modified)
                        {
                            var inlineDiff = _inlineDiffBuilder.BuildDiffModel(
                                diffOld.Text ?? string.Empty,
                                diffNew.Text ?? string.Empty,
                                ignoreWhitespace: false,
                                ignoreCase: false,
                                _characterChunker);

                            var newPieces = inlineDiff.Lines.Where(p => p.Type != DiffPlexChangeType.Deleted).ToList();
                            alignedNewLines.Add(CreateChangeLine(originalLine.LineNumber, DiffPlexChangeType.Inserted, newPieces, true));
                        }
                        else
                        {
                            alignedNewLines.Add(CreateChangeLine(originalLine.LineNumber, diffNew.Type, diffNew.Text, true));
                        }
                        newIndex++;
                    }
                }
            }

            block.OldLines.Clear();
            block.OldLines.AddRange(alignedOldLines);
            block.NewLines.Clear();
            block.NewLines.AddRange(alignedNewLines);
        }

        private void GenerateInlineDiff(ChangeBlock block, DiffPiece oldLine, DiffPiece newLine)
        {
            var inlineDiff = _inlineDiffBuilder.BuildDiffModel(
                oldLine.Text ?? string.Empty,
                newLine.Text ?? string.Empty,
                ignoreWhitespace: false,
                ignoreCase: false,
                _characterChunker);

            List<DiffPlexPiece> oldPieces = inlineDiff.Lines.Where(p => p.Type != DiffPlexChangeType.Inserted).ToList();
            List<DiffPlexPiece> newPieces = inlineDiff.Lines.Where(p => p.Type != DiffPlexChangeType.Deleted).ToList();

            block.OldLines.Add(CreateChangeLine(oldLine.Position, DiffPlexChangeType.Deleted, oldPieces, true));
            block.NewLines.Add(CreateChangeLine(newLine.Position, DiffPlexChangeType.Inserted, newPieces, true));
        }

        private string GetTextFromFragments(IReadOnlyList<TextFragment> fragments)
        {
            if (fragments == null || fragments.Count == 0) return string.Empty;
            return string.Join("", fragments.Select(f => f.Text));
        }

        private bool IsWhitespaceOnlyChange(string? oldText, string? newText)
        {
            if (oldText == null && newText == null) return true;
            if (oldText == null || newText == null) return false;

            return string.Equals(oldText.Trim(), newText.Trim(), StringComparison.Ordinal);
        }

        private ChangeLine CreateChangeLine(int? lineNumber, DiffPlexChangeType kind, string? text, bool isInModifiedBlock)
        {
            var internalKind = MapChangeType(kind);
            bool isWhitespace = string.IsNullOrWhiteSpace(text);

            var fragments = text is null
                ? new List<TextFragment>()
                : new List<TextFragment>
                {
                    new TextFragment
                    {
                        Text = text,
                        Kind = internalKind,
                        IsWhitespaceChange = isWhitespace
                    }
                };

            return new ChangeLine
            {
                LineNumber = lineNumber,
                Kind = internalKind,
                Fragments = fragments,
                IsInModifiedBlock = isInModifiedBlock
            };
        }

        private ChangeLine CreateChangeLine(int? lineNumber, DiffPlexChangeType kind, List<DiffPlexPiece> pieces, bool isInModifiedBlock)
        {
            return new ChangeLine
            {
                LineNumber = lineNumber,
                Kind = MapChangeType(kind),
                Fragments = pieces.Select(p => new TextFragment
                {
                    Text = p.Text,
                    Kind = MapChangeType(p.Type),
                    IsWhitespaceChange = string.IsNullOrWhiteSpace(p.Text)
                }).ToList(),
                IsInModifiedBlock = isInModifiedBlock
            };
        }

        private DiffChangeType MapChangeType(DiffPlexChangeType type)
        {
            return type switch
            {
                DiffPlexChangeType.Inserted => DiffChangeType.Inserted,
                DiffPlexChangeType.Deleted => DiffChangeType.Deleted,
                DiffPlexChangeType.Modified => DiffChangeType.Modified,
                DiffPlexChangeType.Unchanged => DiffChangeType.Unchanged,
                DiffPlexChangeType.Imaginary => DiffChangeType.Imaginary,
                _ => DiffChangeType.Unchanged
            };
        }
    }
}