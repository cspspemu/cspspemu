using System;
using System.Text;

namespace SafeILGenerator.Ast.Utils
{
    public sealed class IndentedStringBuilder
    {
        private StringBuilder StringBuilder = new StringBuilder();
        private bool _startingLine = true;
        private int _indentLevel;
        private int IndentLevelSpaceCount = 4;

        public void Indent(Action action)
        {
            _indentLevel++;
            try
            {
                action();
            }
            finally
            {
                _indentLevel--;
            }
        }

        public void UnIndent(Action action)
        {
            _indentLevel--;
            try
            {
                action();
            }
            finally
            {
                _indentLevel++;
            }
        }

        public IndentedStringBuilder Write(string inlineText)
        {
            if (inlineText.Length > 0)
            {
                if (_startingLine)
                {
                    _startingLine = false;
                    StringBuilder.Append(new String(' ', Math.Max(_indentLevel, 0) * IndentLevelSpaceCount));
                }
                StringBuilder.Append(inlineText);
            }
            return this;
        }

        public IndentedStringBuilder WriteNewLine()
        {
            _startingLine = true;
            StringBuilder.Append("\n");
            return this;
        }

        /*
        public void Write(string Text)
        {
            var Lines = Text.Split('\n');
            for (int n = 0; n < Lines.Length; n++)
            {
                if (n > 0)
                {
                    WriteNewLine();
                }
                WriteTextWithoutLineBreaks(Lines[n]);
            }
        }
        */

        public override string ToString()
        {
            return StringBuilder.ToString();
        }
    }
}