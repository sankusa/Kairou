using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kairou
{
    [Serializable]
    public class BookAndPageSelector : IValidatableAsCommandField
    {
        [SerializeReference, SerializeReferencePopup] IBookSlot _bookSlot;
        public ScriptBook Book => _bookSlot.Book;

        [SerializeField] string _pageId;
        public string PageId => _pageId;

        public string GetSummary()
        {
            if (_bookSlot == null) return "";
            return $"{_bookSlot.OwnerObject} : {_pageId}";
        }

        public IEnumerable<string> Validate(Command command, string fieldName)
        {
            if (_bookSlot == null)
            {
                yield return $"{fieldName} : BookSlot is null";
            }
            else if (_bookSlot.Book == null)
            {
                yield return $"{fieldName} : Book is null";
            }
            else
            {
                if (string.IsNullOrEmpty(_pageId) == false && _bookSlot.Book.Pages.FirstOrDefault(x => x.Id == _pageId) == null)
                {
                    yield return $"{fieldName} : Page is not found. PageId: {_pageId}";
                }
            }
        }
    }
}