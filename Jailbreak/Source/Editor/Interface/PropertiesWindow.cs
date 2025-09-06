using System.Drawing;
using Jailbreak.Interface;
using Myra.Graphics2D.UI;

namespace Jailbreak.Editor.Interface;

public class PropertiesWindow : ToggleWindow {

    public PropertiesWindow() {
        Title = "Properties";

        SetUpWidgets();
    }

    private void SetUpWidgets() {
        TabControl tabControl = new TabControl();

        tabControl.Items.Add(SetUpGeneralPage());

        Content = tabControl;
    }

    private TabItem SetUpGeneralPage() {
        TabItem tab = new TabItem();
        tab.Text = "General";

        HorizontalStackPanel panel = new HorizontalStackPanel();

        tab.Content = panel;
        return tab;
    }

}