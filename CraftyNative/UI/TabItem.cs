using CraftyNative.ThreeD.Meshes;

namespace CraftyNative.UI;

public sealed class TabItem(string header, object content, Mesh? icon = null, string? tooltipDescription = null)
{
    public string Header { get; set; } = header;
    public object Content { get; set; } = content;
    public Mesh? Icon { get; set; } = icon;
    public string? TooltipDescription { get; set; } = tooltipDescription;
}
