using System.Collections;
using System.Collections.Generic;
using Aloha.Coconut;
using Aloha.Coconut.UI;
using Aloha.CoconutMilk;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ItemObtainedPopup : UISlice, IDimClosable
{
    public class Args : UIOpenArgs
    {
        public List<Property> Properties;

        public Args(List<Property> properties)
        {
            Properties = properties;
        }
    }

    [Inject] private PropertyIconPool _propertyIconPool;
    private List<PropertyIcon> _propertyIcons = new();

    [SerializeField] private RectTransform itemContainer;
    private Args _args;
    private bool isPlaying = false;

    protected override void Open(UIOpenArgs openArgs)
    {
        if (IsOpened) return;
        _args = openArgs as Args;
        base.Open(openArgs);
        transform.SetAsLastSibling();

        foreach (var icon in _propertyIcons)
        {
            _propertyIconPool.Remove(icon);
        }

        _propertyIcons.Clear();
        for (int i = 0; i < _args.Properties.Count; i++)
        {
            var propertyIcon = _propertyIconPool.Get(_args.Properties[i], itemContainer);
            _propertyIcons.Add(propertyIcon);
        }
    }


    public void CloseByDim()
    {
        CloseView();
    }
}