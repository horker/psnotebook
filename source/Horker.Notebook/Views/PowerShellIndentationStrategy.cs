using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Indentation;

namespace Horker.Notebook.Views // Models??
{
    class PowerShellIndentationStrategy : DefaultIndentationStrategy
    {
        private TextEditorOptions _options;

        public PowerShellIndentationStrategy(TextEditorOptions options)
        {
            _options = options;
        }

        public override void IndentLine(TextDocument document, DocumentLine line)
        {
            if (document == null)
                throw new ArgumentNullException("document");

			if (line == null)
				throw new ArgumentNullException("line");

            DocumentLine prevLine = line;
            do
            {
                prevLine = prevLine.PreviousLine;
                if (prevLine == null)
                    return;
            } while (prevLine.Length == 0);

            var prevIndent = TextUtilities.GetWhitespaceAfter(document, prevLine.Offset);
            var indentString = document.GetText(prevIndent);

            var trailingChar = document.GetCharAt(prevLine.Offset + prevLine.Length - 1);
            if (trailingChar == '{' || trailingChar == '(' || trailingChar == '[')
                indentString = indentString + _options.IndentationString;

            // If the previous line is blank only, triom the whole line.

            if (prevIndent.Length == prevLine.Length)
                document.Replace(prevLine.Offset, prevLine.Length, "");

            // copy indentation to line

            var indent = TextUtilities.GetWhitespaceAfter(document, line.Offset);
            document.Replace(indent.Offset, indent.Length, indentString,
                             OffsetChangeMappingType.RemoveAndInsert);
        }

        public void DedentByClosingParen(TextDocument document, int offset)
        {
            var line = document.GetLineByOffset(offset);

            // Examine if the paren is at the begining-of-line.

            var leading = TextUtilities.GetLeadingWhitespace(document, line);
            if (leading.Length == 0 || leading.EndOffset != offset)
                return;

            DocumentLine prevLine = line;
            do
            {
                prevLine = prevLine.PreviousLine;
                if (prevLine == null)
                    return;
            } while (prevLine.Length == 0);

            var indent = TextUtilities.GetWhitespaceAfter(document, prevLine.Offset);
            if (indent.Length == 0)
                return;

            int cutLength;
            if (document.GetCharAt(indent.EndOffset - 1) == '\t')
                cutLength = indent.Length - 1;
            else
                cutLength = Math.Min(indent.Length, _options.IndentationSize);

            var indentString = document.GetText(indent.Offset, indent.Length - cutLength);

            document.Replace(leading.Offset, leading.Length, indentString, OffsetChangeMappingType.RemoveAndInsert);
        }
    }
}
