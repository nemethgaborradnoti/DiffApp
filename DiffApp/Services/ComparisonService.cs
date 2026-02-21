using DiffApp.Models;
using DiffApp.Services.Interfaces;
using DiffPlex;
using DiffPlex.Chunkers;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using System;
using System.Collections.Generic;
using System.Linq;

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

            IChunker chunker = settings.Precision == PrecisionLevel.Character
                ? new CharacterChunker()
                : new WordChunker();

            var diffBuilder = new SideBySideDiffBuilder(new Differ(), new LineChunker(), chunker);
            var diffModel = diffBuilder.BuildDiffModel(oldTextProcessed, newTextProcessed);

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

            GetEffectiveTypes(model.OldText.Lines[0], model.NewText.Lines[0], settings, out var startOldType, out var startNewType);
            var startKind = MapKind(startOldType == DiffPlexChangeType.Imaginary ? startNewType : startOldType);

            var currentBlock = new ChangeBlock
            {
                Kind = startKind,
                StartIndexOld = currentOldIndex,
                StartIndexNew = currentNewIndex
            };

            var inlineDiffer = new InlineDiffBuilder(new Differ());

            for (int i = 0; i < model.OldText.Lines.Count; i++)
            {
                var oldLine = model.OldText.Lines[i];
                var newLine = model.NewText.Lines[i];

                GetEffectiveTypes(oldLine, newLine, settings, out var effectiveOldType, out var effectiveNewType);

                var kind = MapKind(effectiveOldType == DiffPlexChangeType.Imaginary ? effectiveNewType : effectiveOldType);

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
                        StartIndexNew = currentNewIndex
                    };
                }

                if (effectiveOldType == DiffPlexChangeType.Modified && effectiveNewType == DiffPlexChangeType.Modified)
                {
                    var inlineDiff = inlineDiffer.BuildDiffModel(oldLine.Text ?? string.Empty, newLine.Text ?? string.Empty);

                    List<DiffPlexPiece> oldPieces = inlineDiff.Lines.Where(p => p.Type != DiffPlexChangeType.Inserted).ToList();
                    List<DiffPlexPiece> newPieces = inlineDiff.Lines.Where(p => p.Type != DiffPlexChangeType.Deleted).ToList();

                    currentBlock.OldLines.Add(CreateChangeLine(oldLine.Position, DiffPlexChangeType.Deleted, oldPieces));
                    currentBlock.NewLines.Add(CreateChangeLine(newLine.Position, DiffPlexChangeType.Inserted, newPieces));

                    currentOldIndex++;
                    currentNewIndex++;
                }
                else
                {
                    currentBlock.OldLines.Add(CreateChangeLine(oldLine.Position, effectiveOldType, oldLine.Text));
                    currentBlock.NewLines.Add(CreateChangeLine(newLine.Position, effectiveNewType, newLine.Text));

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

        private void GetEffectiveTypes(DiffPlexPiece oldLine, DiffPlexPiece newLine, CompareSettings settings, out DiffPlexChangeType oldType, out DiffPlexChangeType newType)
        {
            oldType = oldLine.Type;
            newType = newLine.Type;

            if (settings.IgnoreWhitespace)
            {
                bool isWhitespaceDifference = IsWhitespaceOnlyChange(oldLine, newLine);
                if (isWhitespaceDifference)
                {
                    oldType = DiffPlexChangeType.Unchanged;
                    newType = DiffPlexChangeType.Unchanged;
                }
            }
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

        private ChangeLine CreateChangeLine(int? lineNumber, DiffPlexChangeType kind, string text)
        {
            var internalKind = MapChangeType(kind);
            var fragments = text is null
                ? new List<TextFragment>()
                : new List<TextFragment> { new TextFragment { Text = text, Kind = internalKind } };

            return new ChangeLine
            {
                LineNumber = lineNumber,
                Kind = internalKind,
                Fragments = fragments
            };
        }

        private ChangeLine CreateChangeLine(int? lineNumber, DiffPlexChangeType kind, List<DiffPlexPiece> pieces)
        {
            return new ChangeLine
            {
                LineNumber = lineNumber,
                Kind = MapChangeType(kind),
                Fragments = pieces.Select(p => new TextFragment
                {
                    Text = p.Text,
                    Kind = MapChangeType(p.Type)
                }).ToList()
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