using Jailbreak.Interface;
using Myra.Graphics2D.UI;

namespace Jailbreak.Editor.Interface;

public class EditorMapPropertiesWindow : ToggleWindow {

    public EditorMapPropertiesWindow() {
        Title = "Properties";

        SetUpWidgets();
    }

    private void SetUpWidgets() {
        TabControl tabControl = new TabControl();
        TabItem infoTab = new TabItem();
        infoTab.Text = "General";
        infoTab.Content = new Label() { Text = "Balls" };

        tabControl.Items.Add(infoTab);

        Content = tabControl;
    }

}