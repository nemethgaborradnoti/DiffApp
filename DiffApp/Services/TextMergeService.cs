using DiffApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiffApp.Services
{
    public class TextMergeService : ITextMergeService
    {
        public string MergeBlock(string targetText, DiffHunk hunk, MergeDirection direction)
        {
            var lines = GetLines(targetText);

            // Logic: We are modifying the Target to match the Source for this specific Hunk.

            if (direction == MergeDirection.LeftToRight) // Modifying Right (New) to look like Left (Old)
            {
                if (hunk.Kind == HunkKind.Added)
                {
                    // "Added" means it exists in New but not Old. 
                    // To match Old, we must DELETE these lines from New.
                    // NewLines contain the indices in the New text.
                    RemoveLines(lines, hunk.NewLines);
                }
                else if (hunk.Kind == HunkKind.Removed)
                {
                    // "Removed" means it exists in Old but not New.
                    // To match Old, we must INSERT these lines into New.
                    // We use StartIndexNew as the anchor point.
                    var textToInsert = hunk.OldLines.Select(l => GetText(l)).ToList();
                    InsertLines(lines, hunk.StartIndexNew, textToInsert);
                }
                else if (hunk.Kind == HunkKind.Modified)
                {
                    // Replace New lines with Old lines.
                    ReplaceLines(lines, hunk.NewLines, hunk.OldLines);
                }
            }
            else // RightToLeft: Modifying Left (Old) to look like Right (New)
            {
                if (hunk.Kind == HunkKind.Added)
                {
                    // Exists in New, not Old.
                    // To match New, we INSERT into Old.
                    var textToInsert = hunk.NewLines.Select(l => GetText(l)).ToList();
                    InsertLines(lines, hunk.StartIndexOld, textToInsert);
                }
                else if (hunk.Kind == HunkKind.Removed)
                {
                    // Exists in Old, not New.
                    // To match New, we DELETE from Old.
                    RemoveLines(lines, hunk.OldLines);
                }
                else if (hunk.Kind == HunkKind.Modified)
                {
                    // Replace Old lines with New lines.
                    ReplaceLines(lines, hunk.OldLines, hunk.NewLines);
                }
            }

            return string.Join(Environment.NewLine, lines);
        }

        public string MergeLine(string targetText, DiffLine line, int targetLineIndex, MergeDirection direction)
        {
            // Note: Single line merge is complex because of context. 
            // This implementation assumes the caller knows the specific target index.
            // For now, simpler to implement Block merge primarily.
            // If strictly needed, we would implement similarly to Block merge but for 1 item.

            throw new NotImplementedException("Line-level merge requires strict context management. Block merge is recommended.");
        }

        private List<string> GetLines(string text)
        {
            if (string.IsNullOrEmpty(text)) return new List<string>();
            // Using logic that preserves empty lines
            return text.Replace("\r\n", "\n").Split('\n').ToList();
        }

        private string GetText(DiffLine line)
        {
            return string.Join("", line.Pieces.Select(p => p.Text));
        }

        private void RemoveLines(List<string> textLines, List<DiffLine> linesToRemove)
        {
            // Sort descending to remove from bottom up to avoid index shifting issues
            var indices = linesToRemove
                .Where(l => l.LineNumber.HasValue)
                .Select(l => l.LineNumber.Value - 1) // LineNumber is 1-based usually
                .OrderByDescending(i => i)
                .ToList();

            foreach (var index in indices)
            {
                if (index >= 0 && index < textLines.Count)
                {
                    textLines.RemoveAt(index);
                }
            }
        }

        private void InsertLines(List<string> textLines, int insertIndex, List<string> linesToInsert)
        {
            // Insert index is 0-based.
            if (insertIndex > textLines.Count) insertIndex = textLines.Count;
            if (insertIndex < 0) insertIndex = 0;

            textLines.InsertRange(insertIndex, linesToInsert);
        }

        private void ReplaceLines(List<string> textLines, List<DiffLine> targets, List<DiffLine> sources)
        {
            // Simple replace assuming 1:1 mapping for Modified blocks, or strictly replacing the range.
            // For modified blocks, usually Count is same if we treat them as strict replacements.

            var indices = targets
                .Where(l => l.LineNumber.HasValue)
                .Select(l => l.LineNumber.Value - 1)
                .OrderBy(i => i)
                .ToList();

            if (indices.Count == 0) return;

            int startIndex = indices.First();
            int countToRemove = indices.Count;

            // Validate contiguous (Modified block should be contiguous)
            // If not, we might have issues, but DiffPlex Modified blocks are usually contiguous chunks.

            if (startIndex >= 0 && startIndex < textLines.Count)
            {
                // Remove old range
                // Determine how many to remove. 
                // Careful: if indices are not contiguous, simply removing range is dangerous.
                // Assuming contiguous for Modified hunk.

                int actualRemovable = Math.Min(countToRemove, textLines.Count - startIndex);
                textLines.RemoveRange(startIndex, actualRemovable);

                // Insert new content
                var newContent = sources.Select(l => GetText(l)).ToList();
                textLines.InsertRange(startIndex, newContent);
            }
        }
    }
}