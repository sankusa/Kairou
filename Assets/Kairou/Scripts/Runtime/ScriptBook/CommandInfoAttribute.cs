using System;

namespace Kairou
{
    public class CommandInfoAttribute : Attribute
    {
        public string CategoryPath { get; }
        public string CommandName { get; }

        public CommandInfoAttribute(string categoryPath, string commandPath)
        {
            CategoryPath = categoryPath;
            CommandName = commandPath;
        }
    }
}