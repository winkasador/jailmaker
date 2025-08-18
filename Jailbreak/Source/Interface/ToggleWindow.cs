using Myra.Graphics2D.UI;

namespace Jailbreak.Interface;

public class ToggleWindow : Window {

    public override void Close()
    {
        this.Visible = !this.Visible;
    }
}