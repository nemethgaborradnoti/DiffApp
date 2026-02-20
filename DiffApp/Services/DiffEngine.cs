using DiffApp.Models;
using DiffApp.Services.Interfaces;
using DiffPlex;
using DiffPlex.Chunkers;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using System;
using System.Collections.Generic;
using System.Linq;

// Alias definition to resolve ambiguity between DiffApp.Models.DiffPiece and DiffPlex.DiffBuilder.Model.DiffPiece
using DiffPlexPiece = DiffPlex.DiffBuilder.Model.DiffPiece;

namespace DiffApp.Services
{
    public class DiffEngine : IDiffEngine
    {
        public DiffResult Compare(string oldText, string newText, DiffOptions options)
        {
            string oldTextProcessed = oldText ?? string.Empty;
            string newTextProcessed = newText ?? string.Empty;

            IChunker chunker = options.Precision == DiffPrecision.Character
                ? new CharacterChunker()
                : new WordChunker();

            var diffBuilder = new SideBySideDiffBuilder(new Differ(), new LineChunker(), chunker);
            var diffModel = diffBuilder.BuildDiffModel(oldTextProcessed, newTextProcessed);

            var hunks = BuildHunks(diffModel, options);

            return new DiffResult(hunks);
        }

        private List<DiffHunk> BuildHunks(SideBySideDiffModel model, DiffOptions options)
        {
            var hunks = new List<DiffHunk>();
            if (model.OldText.Lines.Count == 0 && model.NewText.Lines.Count == 0)
            {
                return hunks;
            }

            int currentOldIndex = 0;
            int currentNewIndex = 0;

            GetEffectiveTypes(model.OldText.Lines[0], model.NewText.Lines[0], options, out var startOldType, out var startNewType);
            var startKind = MapKind(startOldType == ChangeType.Imaginary ? startNewType : startOldType);

            var currentHunk = new DiffHunk
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

                GetEffectiveTypes(oldLine, newLine, options, out var effectiveOldType, out var effectiveNewType);

                var kind = MapKind(effectiveOldType == ChangeType.Imaginary ? effectiveNewType : effectiveOldType);

                if (kind != currentHunk.Kind)
                {
                    if (currentHunk.OldLines.Count > 0 || currentHunk.NewLines.Count > 0)
                    {
                        hunks.Add(currentHunk);
                    }
                    currentHunk = new DiffHunk
                    {
                        Kind = kind,
                        StartIndexOld = currentOldIndex,
                        StartIndexNew = currentNewIndex
                    };
                }

                if (effectiveOldType == ChangeType.Modified && effectiveNewType == ChangeType.Modified)
                {
                    var inlineDiff = inlineDiffer.BuildDiffModel(oldLine.Text ?? string.Empty, newLine.Text ?? string.Empty);

                    // Explicitly using DiffPlexPiece here
                    List<DiffPlexPiece> oldPieces = inlineDiff.Lines.Where(p => p.Type != ChangeType.Inserted).ToList();
                    List<DiffPlexPiece> newPieces = inlineDiff.Lines.Where(p => p.Type != ChangeType.Deleted).ToList();

                    currentHunk.OldLines.Add(CreateDiffLine(oldLine.Position, ChangeType.Deleted, oldPieces));
                    currentHunk.NewLines.Add(CreateDiffLine(newLine.Position, ChangeType.Inserted, newPieces));

                    currentOldIndex++;
                    currentNewIndex++;
                }
                else
                {
                    currentHunk.OldLines.Add(CreateDiffLine(oldLine.Position, effectiveOldType, oldLine.Text));
                    currentHunk.NewLines.Add(CreateDiffLine(newLine.Position, effectiveNewType, newLine.Text));

                    if (oldLine.Type != ChangeType.Imaginary) currentOldIndex++;
                    if (newLine.Type != ChangeType.Imaginary) currentNewIndex++;
                }
            }

            if (currentHunk.OldLines.Count > 0 || currentHunk.NewLines.Count > 0)
            {
                hunks.Add(currentHunk);
            }

            return hunks;
        }

        // Using DiffPlexPiece explicitly in arguments
        private void GetEffectiveTypes(DiffPlexPiece oldLine, DiffPlexPiece newLine, DiffOptions options, out ChangeType oldType, out ChangeType newType)
        {
            oldType = oldLine.Type;
            newType = newLine.Type;

            if (options.IgnoreWhitespace)
            {
                bool isWhitespaceDifference = IsWhitespaceOnlyChange(oldLine, newLine);
                if (isWhitespaceDifference)
                {
                    oldType = ChangeType.Unchanged;
                    newType = ChangeType.Unchanged;
                }
            }
        }

        private bool IsWhitespaceOnlyChange(DiffPlexPiece oldLine, DiffPlexPiece newLine)
        {
            string oldText = oldLine.Text ?? string.Empty;
            string newText = newLine.Text ?? string.Empty;

            if (oldLine.Type == ChangeType.Imaginary)
            {
                return string.IsNullOrWhiteSpace(newText);
            }
            if (newLine.Type == ChangeType.Imaginary)
            {
                return string.IsNullOrWhiteSpace(oldText);
            }

            return string.Equals(oldText.Trim(), newText.Trim(), StringComparison.Ordinal);
        }

        private DiffLine CreateDiffLine(int? lineNumber, ChangeType kind, string text)
        {
            var pieces = text is null
                ? new List<Models.DiffPiece>()
                : new List<Models.DiffPiece> { new Models.DiffPiece { Text = text, Kind = kind } };

            return new DiffLine
            {
                LineNumber = lineNumber,
                Kind = kind,
                Pieces = pieces
            };
        }

        // Using DiffPlexPiece explicitly in arguments
        private DiffLine CreateDiffLine(int? lineNumber, ChangeType kind, List<DiffPlexPiece> pieces)
        {
            return new DiffLine
            {
                LineNumber = lineNumber,
                Kind = kind,
                Pieces = pieces.Select(p => new Models.DiffPiece { Text = p.Text, Kind = p.Type }).ToList()
            };
        }

        private HunkKind MapKind(ChangeType type)
        {
            return type switch
            {
                ChangeType.Inserted => HunkKind.Added,
                ChangeType.Deleted => HunkKind.Removed,
                ChangeType.Unchanged => HunkKind.Unchanged,
                ChangeType.Modified => HunkKind.Modified,
                _ => HunkKind.Unchanged,
            };
        }
    }
}