using System;
using UnityEngine;

namespace Kairou
{
    [Serializable]
    public class BookAndPageSelector
    {
        [SerializeReference, SerializeReferencePopup] IBookSlot _bookSlot;
        [SerializeField] string _pageId;
    }
}