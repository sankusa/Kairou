using System;
using System.Collections.Generic;
using System.Text;

namespace Kairou
{
    readonly struct IndentScope : IDisposable
    {
        readonly CodeBuilder _builder;

        public IndentScope(CodeBuilder builder)
        {
            _builder = builder;
            _builder.IndentLevel++;
        }

        public void Dispose()
        {
            _builder.IndentLevel--;
        }
    }

    readonly struct BlockScope : IDisposable
    {
        readonly CodeBuilder _builder;

        public BlockScope(CodeBuilder builder)
        {
            _builder = builder;
            _builder.BeginBlock();
        }

        public void Dispose()
        {
            _builder.EndBlock();
        }
    }

    public class CodeBuilder
    {
        readonly StringBuilder _sb = new();
        public int IndentLevel { get; set; }

        public void Append(string value = "")
        {
            if (string.IsNullOrEmpty(value)) return;
            _sb.Append(value);
        }

        public void Append(StringBuilder value)
        {
            if (value == null) return;
            _sb.Append(value);
        }

        public void AppendLine(string value = "")
        {
            _sb.AppendLine(value);
        }

        public void AppendIndentedLine(string value = "")
        {
            AppendIndent();
            _sb.AppendLine(value);
        }

        public void BeginBlock()
        {
            AppendIndentedLine("{");
            IndentLevel++;
        }

        public void EndBlock()
        {
            IndentLevel--;
            AppendIndentedLine("}");
        }

        public void AppendIndent()
        {
            _sb.Append(' ', IndentLevel * 4);
        }

        public override string ToString() => _sb.ToString();
    }
}