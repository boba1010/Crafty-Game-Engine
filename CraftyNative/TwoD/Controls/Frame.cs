namespace CraftyNative.TwoD.Controls;

public sealed class Frame : UIElement
{
    private Stack<Page> Pages { get; set; } = [];
    public Page CurrentPage => Pages.Peek();
    public bool CanGoBack => Pages.Count > 1;

    internal override UIElement? FindHit(float x, float y)
    {
        return CurrentPage.Content.FindHit(x, y);
    }

    public void Navigate(Page page)
    {
        Pages.Push(page);
    }

    public void GoBack()
    {
        Pages.Pop();
    }

    internal override void Render(double deltaTime)
    {
        CurrentPage.Render(deltaTime);
    }
}
