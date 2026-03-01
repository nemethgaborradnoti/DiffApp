namespace DiffApp.Services
{
    public class MergeService : IMergeService
    {
        public string MergeBlock(string targetText, ChangeBlock block, MergeDirection direction)
        {
            var lines = GetLines(targetText);

            List<ChangeLine> sourceLines;
            List<ChangeLine> targetLines;

            int insertIndex;

            if (direction == MergeDirection.LeftToRight)
            {
                sourceLines = block.OldLines;
                targetLines = block.NewLines;
                insertIndex = block.StartIndexNew;
            }
            else
            {
                sourceLines = block.NewLines;
                targetLines = block.OldLines;
                insertIndex = block.StartIndexOld;
            }

            var textToInsert = sourceLines
                .Where(l => l.Kind != DiffChangeType.Imaginary)
                .Select(l => GetText(l))
                .ToList();

            if (block.Kind == BlockType.Added)
            {
                if (direction == MergeDirection.LeftToRight)
                {
                    RemoveLines(lines, targetLines);
                }
                else
                {
                    InsertLines(lines, insertIndex, textToInsert);
                }
            }
            else if (block.Kind == BlockType.Removed)
            {
                if (direction == MergeDirection.LeftToRight)
                {
                    InsertLines(lines, insertIndex, textToInsert);
                }
                else
                {
                    RemoveLines(lines, targetLines);
                }
            }
            else if (block.Kind == BlockType.Modified)
            {
                ReplaceLines(lines, targetLines, textToInsert);
            }

            return string.Join(Environment.NewLine, lines);
        }

        public string MergeLine(string targetText, ChangeLine line, int targetLineIndex, MergeDirection direction)
        {
            throw new NotImplementedException();
        }

        private List<string> GetLines(string text)
        {
            if (string.IsNullOrEmpty(text)) return new List<string>();
            return text.Replace("\r\n", "\n").Split('\n').ToList();
        }

        private string GetText(ChangeLine line)
        {
            return string.Join("", line.Fragments.Select(p => p.Text));
        }

        private void RemoveLines(List<string> textLines, List<ChangeLine> linesToRemove)
        {
            var indices = linesToRemove
                .Where(l => l.LineNumber.HasValue)
                .Select(l => l.LineNumber.Value - 1)
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
            if (insertIndex > textLines.Count) insertIndex = textLines.Count;
            if (insertIndex < 0) insertIndex = 0;

            textLines.InsertRange(insertIndex, linesToInsert);
        }

        private void ReplaceLines(List<string> textLines, List<ChangeLine> targets, List<string> newContent)
        {
            var firstRealLine = targets.FirstOrDefault(l => l.LineNumber.HasValue);

            if (firstRealLine == null || !firstRealLine.LineNumber.HasValue)
                return;

            int startIndex = firstRealLine.LineNumber.Value - 1;

            int countToRemove = targets.Count(l => l.Kind != DiffChangeType.Imaginary);

            if (startIndex >= 0 && startIndex < textLines.Count)
            {
                int actualRemovable = Math.Min(countToRemove, textLines.Count - startIndex);

                if (actualRemovable > 0)
                {
                    textLines.RemoveRange(startIndex, actualRemovable);
                }

                textLines.InsertRange(startIndex, newContent);
            }
        }
    }
}