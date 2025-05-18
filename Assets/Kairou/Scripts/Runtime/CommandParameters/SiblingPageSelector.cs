using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kairou
{
    [Serializable]
    public class SiblingPageSelector : IValidatableAsCommandField
    {
        [SerializeField] string _pageId;
        public string PageId => _pageId;

        public string GetSummary() => _pageId;

        public IEnumerable<string> Validate(Command command, string fieldName)
        {
            if (string.IsNullOrEmpty(_pageId))
            {
                yield return $"{fieldName} : PageId is empty";
            }
            else
            {
                if (command.ParentPage.GetSiblingPages().FirstOrDefault(x => x.Id == _pageId) == null)
                {
                    yield return $"{fieldName} : Page not found. PageId: {_pageId}";
                }
            }
        }
    }
}