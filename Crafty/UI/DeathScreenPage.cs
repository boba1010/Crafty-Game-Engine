using CraftyNative.UI;
using System.Numerics;

namespace Crafty.UI;

public sealed class DeathScreenPage : Page
{
    public event Action? RespawnRequested;
    public event Action? QuitRequested;

    private readonly Grid _root = new()
    {
        HorizontalAlignment = HorizontalAlignment.Stretch,
        VerticalAlignment = VerticalAlignment.Stretch,
        BackgroundColor = new(0.35f, 0.02f, 0.02f, 0.65f)
    };

    private readonly Grid _content = new()
    {
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        BackgroundColor = Vector4.Zero,
        Size = new(360, 300),
    };

    public DeathScreenPage()
    {
        BackgroundColor = Vector4.Zero;
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Stretch;

        Initialize();
    }

    private void Initialize()
    {
        _root.Rows.Add(GridLength.Star);
        _content.Rows.Add(new(180f, GridUnit.Pixel));
        _content.Rows.Add(new(40f, GridUnit.Pixel));
        _content.Rows.Add(new(10f, GridUnit.Pixel));
        _content.Rows.Add(new(40f, GridUnit.Pixel));

        _root.Add(_content, 0, 0);

        Label title = new()
        {
            Text = "YOU DIED!",
            Size = new(360, 80),
            IsTextCentered = true,
            FontScale = 2f,
            Foreground = new(0.9f, 0.12f, 0.12f, 1f)
        };

        _content.Add(title, 0, 0);

        Button respawnBtn = new()
        {
            Text = "Respawn",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Size = new(400, 40)
        };

        respawnBtn.Clicked += RespawnBtn_Clicked;

        _content.Add(respawnBtn, 1, 0);

        Button quitBtn = new()
        {
            Text = "Quit & Save",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Size = new(400, 40)
        };

        quitBtn.Clicked += QuitBtn_Clicked;

        _content.Add(quitBtn, 3, 0);

        Content = _root;
    }

    private void QuitBtn_Clicked()
    {
        QuitRequested?.Invoke();
    }

    private void RespawnBtn_Clicked()
    {
        RespawnRequested?.Invoke();
    }
}
