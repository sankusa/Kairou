using System;
using Kairou;
using NUnit.Framework;

namespace Kairou.Tests
{
    public class PageTests
    {
        [Test]
        public void AddCommand_Null_ThrowsArgumentNullException()
        {
            var page = new Page();

            Assert.Throws<ArgumentNullException>(() => page.AddCommand(null));
        }

        [Test]
        public void AddCommand_AlreadyAdded_ThrowsArgumentException()
        {
            var page = new Page();
            var command = new PageTests_TestCommand();

            page.AddCommand(command);

            Assert.Throws<ArgumentException>(() => page.AddCommand(command));
        }

        [Test]
        public void RemoveCommand_1Command_0CommandInPage()
        {
            var page = new Page();
            var command = new PageTests_TestCommand();

            page.AddCommand(command);
            page.RemoveCommand(command);

            Assert.AreEqual(0, page.Commands.Count);
        }

        [Test]
        public void RemoveCommand_CommandIsNotInPage_NoError()
        {
            var page = new Page();
            var command = new PageTests_TestCommand();

            page.RemoveCommand(command);

            Assert.AreEqual(0, page.Commands.Count);
        }

        [Test]
        public void RemoveCommandAt_InvalidIndex_NoError()
        {
            var page = new Page();
            page.RemoveCommandAt(-1);
            page.RemoveCommandAt(1);

            Assert.AreEqual(0, page.Commands.Count);
        }

        [Test]
        public void MoveCommand_1Command_1CommandInPage()
        {
            var page = new Page();
            var command1 = new PageTests_TestCommand();
            var command2 = new PageTests_TestCommand();

            page.AddCommand(command1);
            page.AddCommand(command2);
            page.MoveCommand(0, 1);

            Assert.AreEqual(command2, page.Commands[0]);
            Assert.AreEqual(command1, page.Commands[1]);
        }
    }
}

[HiddenCommand]
public partial class PageTests_TestCommand : Command
{
    [CommandExecute]
    public void Execute([Inject] Command command)
    {
        
    }
}
