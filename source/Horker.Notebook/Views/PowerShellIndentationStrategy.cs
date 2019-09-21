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

			var previousLine = line.PreviousLine;
            if (previousLine == null)
                return;

            var indentationSegment = TextUtilities.GetWhitespaceAfter(document, previousLine.Offset);
            var indentation = document.GetText(indentationSegment);

            var lineString = document.GetText(previousLine.Offset, previousLine.Length);
            var trailingChar = lineString[lineString.Length - 1];
            if (trailingChar == '{' || trailingChar == '(' || trailingChar == '[')
                indentation = indentation + _options.IndentationString;

            // copy indentation to line
            indentationSegment = TextUtilities.GetWhitespaceAfter(document, line.Offset);
            document.Replace(indentationSegment.Offset, indentationSegment.Length, indentation,
                             OffsetChangeMappingType.RemoveAndInsert);
        }
    }
}
