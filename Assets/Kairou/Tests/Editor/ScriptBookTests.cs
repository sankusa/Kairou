using System;
using NUnit.Framework;
using UnityEngine;
namespace Kairou.Tests
{
    public class ScriptBookTests
    {
        [Test]
        public void AddPage_1Page_1PageInScriptBook()
        {
            var scriptBook = new ScriptBook();
            scriptBook.AddPage(new Page());

            Assert.AreEqual(1, scriptBook.Pages.Count);
        }

        [Test]
        public void AddPage_2Page_2PageInScriptBook()
        {
            var scriptBook = new ScriptBook();
            scriptBook.AddPage(new Page());
            scriptBook.AddPage(new Page());

            Assert.AreEqual(2, scriptBook.Pages.Count);
        }

        [Test]
        public void AddPage_Null_ThrowsArgumentNullException()
        {
            var scriptBook = new ScriptBook();

            Assert.Throws<ArgumentNullException>(() => scriptBook.AddPage(null));
        }

        [Test]
        public void AddPage_AlreadyAdded_ThrowsArgumentException()
        {
            var scriptBook = new ScriptBook();
            var page = new Page();
            scriptBook.AddPage(page);

            Assert.Throws<ArgumentException>(() => scriptBook.AddPage(page));
        }

        [Test]
        public void RemovePage_1Page_EmptyScriptBook()
        {
            var scriptBook = new ScriptBook();
            var page = new Page();
            scriptBook.AddPage(page);
            scriptBook.RemovePage(page);

            Assert.AreEqual(0, scriptBook.Pages.Count);
        }

        [Test]
        public void RemovePage_2Page_1PageInScriptBook()
        {
            var scriptBook = new ScriptBook();
            var page1 = new Page();
            var page2 = new Page();
            scriptBook.AddPage(page1);
            scriptBook.AddPage(page2);
            scriptBook.RemovePage(page1);

            Assert.AreEqual(1, scriptBook.Pages.Count);
        }

        [Test]
        public void RemovePage_PageIsNotInScriptBook_NoError()
        {
            var scriptBook = new ScriptBook();
            var page = new Page();

            scriptBook.RemovePage(page);

            Assert.AreEqual(0, scriptBook.Pages.Count);
        }

        [Test]
        public void RemovePageAt_InvalidIndex_NoError()
        {
            var scriptBook = new ScriptBook();
            scriptBook.RemovePageAt(-1);
            scriptBook.RemovePageAt(1);

            Assert.AreEqual(0, scriptBook.Pages.Count);
        }

        [Test]
        public void MovePage_1Page_MovedSuccessfully()
        {
            var scriptBook = new ScriptBook();
            var page1 = new Page();
            var page2 = new Page();
            scriptBook.AddPage(page1);
            scriptBook.AddPage(page2);

            scriptBook.MovePage(0, 1);

            Assert.AreEqual(page1, scriptBook.Pages[1]);
            Assert.AreEqual(page2, scriptBook.Pages[0]);
        }
    }
}


