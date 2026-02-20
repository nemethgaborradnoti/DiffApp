using DiffApp.Models;
using DiffPlex;
using DiffPlex.Chunkers;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DiffApp.Services
{
    public class DiffEngine : IDiffEngine
    {
        public DiffResult Compare(string oldText, string newText, DiffOptions options)
        {
            string oldTextProcessed = oldText ?? string.Empty;
            string newTextProcessed = newText ?? string.Empty;

            if (options.IgnoreWhitespace)
            {
                oldTextProcessed = RemoveWhitespace(oldTextProcessed);
                newTextProcessed = RemoveWhitespace(newTextProcessed);
            }

            IChunker wordChunker = options.Precision == DiffPrecision.Character
                ? new CharacterChunker()
                : new WordChunker();

            var diffBuilder = new SideBySideDiffBuilder(new Differ(), new LineChunker(), wordChunker);
            var diffModel = diffBuilder.BuildDiffModel(oldTextProcessed, newTextProcessed);

            var hunks = BuildHunks(diffModel);

            return new DiffResult(hunks);
        }

        private string RemoveWhitespace(string input)
        {
            return Regex.Replace(input, @"\s+", " ").Trim();
        }

        private List<DiffHunk> BuildHunks(SideBySideDiffModel model)
        {
            var hunks = new List<DiffHunk>();
            if (model.OldText.Lines.Count == 0 && model.NewText.Lines.Count == 0)
            {
                return hunks;
            }

            // Track actual line indices for insertion contexts
            int currentOldIndex = 0;
            int currentNewIndex = 0;

            var currentHunk = new DiffHunk
            {
                Kind = MapKind(model.OldText.Lines[0].Type == ChangeType.Imaginary ? model.NewText.Lines[0].Type : model.OldText.Lines[0].Type),
                StartIndexOld = currentOldIndex,
                StartIndexNew = currentNewIndex
            };

            var inlineDiffer = new InlineDiffBuilder(new Differ());

            for (int i = 0; i < model.OldText.Lines.Count; i++)
            {
                var oldLine = model.OldText.Lines[i];
                var newLine = model.NewText.Lines[i];

                var kind = MapKind(oldLine.Type == ChangeType.Imaginary ? newLine.Type : oldLine.Type);

                // Any change in kind starts a new block. 
                // Unchanged lines break the block sequence as per requirements.
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

                if (oldLine.Type == ChangeType.Modified && newLine.Type == ChangeType.Modified)
                {
                    var inlineDiff = inlineDiffer.BuildDiffModel(oldLine.Text ?? string.Empty, newLine.Text ?? string.Empty);
                    var oldPieces = inlineDiff.Lines.Where(p => p.Type != ChangeType.Inserted).ToList();
                    var newPieces = inlineDiff.Lines.Where(p => p.Type != ChangeType.Deleted).ToList();

                    currentHunk.OldLines.Add(CreateDiffLine(oldLine.Position, ChangeType.Deleted, oldPieces));
                    currentHunk.NewLines.Add(CreateDiffLine(newLine.Position, ChangeType.Inserted, newPieces));

                    currentOldIndex++;
                    currentNewIndex++;
                }
                else
                {
                    currentHunk.OldLines.Add(CreateDiffLine(oldLine.Position, oldLine.Type, oldLine.Text));
                    currentHunk.NewLines.Add(CreateDiffLine(newLine.Position, newLine.Type, newLine.Text));

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

        private DiffLine CreateDiffLine(int? lineNumber, ChangeType kind, List<DiffPlex.DiffBuilder.Model.DiffPiece> pieces)
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