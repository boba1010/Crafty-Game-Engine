using CraftyNative.TwoD.Controls;
using System.Collections.ObjectModel;

namespace CraftyNative.TwoD;

public class UIElementCollection : Collection<UIElement>
{
    private readonly UIElement _owner;

    public UIElementCollection(UIElement owner)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    public new void Add(UIElement child)
    {
        if (child == null) throw new ArgumentNullException(nameof(child));
        if (child.Parent != null) throw new InvalidOperationException("Element already belongs to another visual tree.");

        base.Add(child);

        child.Parent = _owner;

        OnElementAdded(child);
    }

    public new void Remove(UIElement child)
    {
        base.Remove(child);

        child.Parent = null;

        OnElementRemoved(child);
    }

    public new void Clear()
    {
        var itemsToRemove = new UIElement[Count];
        CopyTo(itemsToRemove, 0);

        base.Clear();

        foreach (var item in itemsToRemove)
        {
            item.Parent = null;
            OnElementRemoved(item);
        }
    }

    private void OnElementAdded(UIElement child)
    {
        _owner.Layout(_owner.Bounds);
    }

    private void OnElementRemoved(UIElement child)
    {
        _owner.Layout(_owner.Bounds);
    }
}
