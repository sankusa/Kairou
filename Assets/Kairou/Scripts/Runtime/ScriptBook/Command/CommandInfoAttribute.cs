using System;

namespace Kairou
{
    public class CommandInfoAttribute : Attribute
    {
        public string CategoryName { get; }
        public string CommandName { get; }

        public CommandInfoAttribute(string categoryName, string commandName)
        {
            CategoryName = categoryName;
            CommandName = commandName;
        }
    }
}