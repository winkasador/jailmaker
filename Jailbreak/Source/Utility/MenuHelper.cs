using Myra.Graphics2D.UI;
using System.Collections.Generic;

namespace Jailbreak.Utility;

public static class MenuHelper {

    public static IEnumerable<MenuItem> GetAllMenuItems(Menu menu) {
        foreach (var item in menu.Items) {
            if (item is MenuItem menuItem) {
                yield return menuItem;

                foreach (var subItem in GetAllMenuItems(menuItem)) {
                    yield return subItem;
                }
            }
        }
    }

    public static IEnumerable<MenuItem> GetAllMenuItems(MenuItem parent) {
        foreach (var item in parent.Items) {
            if (item is MenuItem menuItem) {
                yield return menuItem;

                foreach (var subItem in GetAllMenuItems(menuItem)) {
                    yield return subItem;
                }
            }
        }
    }

}
