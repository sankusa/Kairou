using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kairou
{
    [Serializable]
    public class SiblingPageSelector : IValidatableAsCommandField
    {
        [SerializeField] string _pageId;
        public string PageId => _pageId;

        public IEnumerable<string> Validate(Command command, string fieldName)
        {
            yield break;
        }
    }
}