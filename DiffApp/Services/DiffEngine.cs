using DiffApp.Models;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using System.Collections.Generic;
using System.Linq;

namespace DiffApp.Services
{
    public class DiffEngine : IDiffEngine
    {
        public DiffResult Compare(string oldText, string newText)
        {
            var diffBuilder = new SideBySideDiffBuilder(new Differ());
            var diffModel = diffBuilder.BuildDiffModel(oldText ?? string.Empty, newText ?? string.Empty);

            var hunks = BuildHunks(diffModel);

            return new DiffResult(hunks);
        }

        private List<DiffHunk> BuildHunks(SideBySideDiffModel model)
        {
            var hunks = new List<DiffHunk>();
            if (model.OldText.Lines.Count == 0 && model.NewText.Lines.Count == 0)
            {
                return hunks;
            }

            var currentHunk = new DiffHunk { Kind = MapKind(model.OldText.Lines[0].Type == ChangeType.Imaginary ? model.NewText.Lines[0].Type : model.OldText.Lines[0].Type) };
            var inlineDiffer = new InlineDiffBuilder(new Differ());

            for (int i = 0; i < model.OldText.Lines.Count; i++)
            {
                var oldLine = model.OldText.Lines[i];
                var newLine = model.NewText.Lines[i];

                var kind = MapKind(oldLine.Type == ChangeType.Imaginary ? newLine.Type : oldLine.Type);

                if (kind != currentHunk.Kind)
                {
                    if (currentHunk.OldLines.Count > 0 || currentHunk.NewLines.Count > 0)
                    {
                        hunks.Add(currentHunk);
                    }
                    currentHunk = new DiffHunk { Kind = kind };
                }

                if (oldLine.Type == ChangeType.Modified && newLine.Type == ChangeType.Modified)
                {
                    var inlineDiff = inlineDiffer.BuildDiffModel(oldLine.Text ?? string.Empty, newLine.Text ?? string.Empty);
                    var oldPieces = inlineDiff.Lines.Where(p => p.Type != ChangeType.Inserted).ToList();
                    var newPieces = inlineDiff.Lines.Where(p => p.Type != ChangeType.Deleted).ToList();

                    currentHunk.OldLines.Add(CreateDiffLine(oldLine.Position, ChangeType.Deleted, oldPieces));
                    currentHunk.NewLines.Add(CreateDiffLine(newLine.Position, ChangeType.Inserted, newPieces));
                }
                else
                {
                    currentHunk.OldLines.Add(CreateDiffLine(oldLine.Position, oldLine.Type, oldLine.Text));
                    currentHunk.NewLines.Add(CreateDiffLine(newLine.Position, newLine.Type, newLine.Text));
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
