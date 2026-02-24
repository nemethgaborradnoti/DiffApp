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

            // Always run diff with ignoreWhitespace: false to capture all differences.
            // The filtering will happen visually based on the calculated flags.
            var diffBuilder = new SideBySideDiffBuilder(new Differ(), new LineChunker(), chunker);
            var diffModel = diffBuilder.BuildDiffModel(oldTextProcessed, newTextProcessed, ignoreWhitespace: false);

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

            int currentOldIndex = 0;
            int currentNewIndex = 0;

            var startOldLine = model.OldText.Lines[0];
            var startNewLine = model.NewText.Lines[0];

            var startKind = MapKind(startOldLine.Type == DiffPlexChangeType.Imaginary ? startNewLine.Type : startOldLine.Type);

            var currentBlock = new ChangeBlock
            {
                Kind = startKind,
                StartIndexOld = currentOldIndex,
                StartIndexNew = currentNewIndex,
                IsWhitespaceChange = true // Assume true initially, set to false if significant change found
            };

            var inlineDiffer = new InlineDiffBuilder(new Differ());

            for (int i = 0; i < model.OldText.Lines.Count; i++)
            {
                var oldLine = model.OldText.Lines[i];
                var newLine = model.NewText.Lines[i];

                var kind = MapKind(oldLine.Type == DiffPlexChangeType.Imaginary ? newLine.Type : oldLine.Type);
                bool isLineWhitespaceOnly = IsWhitespaceOnlyChange(oldLine, newLine);

                if (kind != currentBlock.Kind)
                {
                    if (currentBlock.OldLines.Count > 0 || currentBlock.NewLines.Count > 0)
                    {
                        blocks.Add(currentBlock);
                    }
                    currentBlock = new ChangeBlock
                    {
                        Kind = kind,
                        StartIndexOld = currentOldIndex,
                        StartIndexNew = currentNewIndex,
                        IsWhitespaceChange = true
                    };
                }

                // If the current line represents a significant change, mark the block as significant
                if (!isLineWhitespaceOnly && kind != BlockType.Unchanged)
                {
                    currentBlock.IsWhitespaceChange = false;
                }

                if (oldLine.Type == DiffPlexChangeType.Modified && newLine.Type == DiffPlexChangeType.Modified)
                {
                    var inlineDiff = inlineDiffer.BuildDiffModel(oldLine.Text ?? string.Empty, newLine.Text ?? string.Empty, ignoreWhitespace: false, ignoreCase: false, new CharacterChunker());

                    List<DiffPlexPiece> oldPieces = inlineDiff.Lines.Where(p => p.Type != DiffPlexChangeType.Inserted).ToList();
                    List<DiffPlexPiece> newPieces = inlineDiff.Lines.Where(p => p.Type != DiffPlexChangeType.Deleted).ToList();

                    currentBlock.OldLines.Add(CreateChangeLine(oldLine.Position, DiffPlexChangeType.Deleted, oldPieces, true));
                    currentBlock.NewLines.Add(CreateChangeLine(newLine.Position, DiffPlexChangeType.Inserted, newPieces, true));

                    currentOldIndex++;
                    currentNewIndex++;
                }
                else
                {
                    currentBlock.OldLines.Add(CreateChangeLine(oldLine.Position, oldLine.Type, oldLine.Text, false));
                    currentBlock.NewLines.Add(CreateChangeLine(newLine.Position, newLine.Type, newLine.Text, false));

                    if (oldLine.Type != DiffPlexChangeType.Imaginary) currentOldIndex++;
                    if (newLine.Type != DiffPlexChangeType.Imaginary) currentNewIndex++;
                }
            }

            if (currentBlock.OldLines.Count > 0 || currentBlock.NewLines.Count > 0)
            {
                blocks.Add(currentBlock);
            }

            return blocks;
        }

        private bool IsWhitespaceOnlyChange(DiffPlexPiece oldLine, DiffPlexPiece newLine)
        {
            string oldText = oldLine.Text ?? string.Empty;
            string newText = newLine.Text ?? string.Empty;

            if (oldLine.Type == DiffPlexChangeType.Imaginary)
            {
                return string.IsNullOrWhiteSpace(newText);
            }
            if (newLine.Type == DiffPlexChangeType.Imaginary)
            {
                return string.IsNullOrWhiteSpace(oldText);
            }

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
                    // For inline pieces, if the text is just whitespace, it's a whitespace change fragment
                    IsWhitespaceChange = string.IsNullOrWhiteSpace(p.Text)
                }).ToList(),
                IsInModifiedBlock = isInModifiedBlock
            };
        }

        private BlockType MapKind(DiffPlexChangeType type)
        {
            return type switch
            {
                DiffPlexChangeType.Inserted => BlockType.Added,
                DiffPlexChangeType.Deleted => BlockType.Removed,
                DiffPlexChangeType.Unchanged => BlockType.Unchanged,
                DiffPlexChangeType.Modified => BlockType.Modified,
                _ => BlockType.Unchanged,
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