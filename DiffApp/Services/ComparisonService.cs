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
        public ComparisonResult Compare(string oldText, string newText, CompareSettings settings)
        {
            string oldTextProcessed = oldText ?? string.Empty;
            string newTextProcessed = newText ?? string.Empty;

            IChunker chunker = new CharacterChunker();

            var diffBuilder = new SideBySideDiffBuilder(new Differ(), new LineChunker(), chunker);

            var diffModel = diffBuilder.BuildDiffModel(oldTextProcessed, newTextProcessed, ignoreWhitespace: true);

            var blocks = BuildBlocks(diffModel, settings);

            return new ComparisonResult(blocks);
        }

        private List<ChangeBlock> BuildBlocks(SideBySideDiffModel model, CompareSettings settings)
        {
            var blocks = new List<ChangeBlock>();
            if (model.OldText.Lines.Count == 0 && model.NewText.Lines.Count == 0)
            {
                return blocks;
            }

            var inlineDiffer = new InlineDiffBuilder(new Differ());
            ChangeBlock? currentBlock = null;

            for (int i = 0; i < model.OldText.Lines.Count; i++)
            {
                var oldLine = model.OldText.Lines[i];
                var newLine = model.NewText.Lines[i];

                BlockType kind = BlockType.Unchanged;
                bool isWhitespaceChange = false;

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
                        StartIndexOld = i,
                        StartIndexNew = i,
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
                    var inlineDiff = inlineDiffer.BuildDiffModel(oldLine.Text ?? string.Empty, newLine.Text ?? string.Empty, ignoreWhitespace: false, ignoreCase: false, new CharacterChunker());

                    List<DiffPlexPiece> oldPieces = inlineDiff.Lines.Where(p => p.Type != DiffPlexChangeType.Inserted).ToList();
                    List<DiffPlexPiece> newPieces = inlineDiff.Lines.Where(p => p.Type != DiffPlexChangeType.Deleted).ToList();

                    currentBlock.OldLines.Add(CreateChangeLine(oldLine.Position, DiffPlexChangeType.Deleted, oldPieces, true));
                    currentBlock.NewLines.Add(CreateChangeLine(newLine.Position, DiffPlexChangeType.Inserted, newPieces, true));
                }
                else
                {
                    currentBlock.OldLines.Add(CreateChangeLine(oldLine.Position, oldLine.Type, oldLine.Text, false));
                    currentBlock.NewLines.Add(CreateChangeLine(newLine.Position, newLine.Type, newLine.Text, false));
                }
            }

            if (currentBlock != null)
            {
                blocks.Add(currentBlock);
            }

            return blocks;
        }

        private bool IsWhitespaceOnlyChange(string? oldText, string? newText)
        {
            if (oldText == null && newText == null) return true;
            if (oldText == null || newText == null) return false;

            return string.Equals(oldText.Trim(), newText.Trim(), StringComparison.Ordinal);
        }

        private ChangeLine CreateChangeLine(int? lineNumber, DiffPlexChangeType kind, string text, bool isInModifiedBlock)
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