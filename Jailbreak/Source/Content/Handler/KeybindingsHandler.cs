using System.Collections.Generic;
using Jailbreak.Data.Dto;
using Jailbreak.Input;

namespace Jailbreak.Content.Handler;

public class KeybindingsHandler : IContentHandler<List<KeyBinding>> {

    private DynamicContentManager _contentManager;

    public KeybindingsHandler(DynamicContentManager contentManager) {
        _contentManager = contentManager;
    }

    public List<KeyBinding> Handle(byte[] data) {
        List<KeyBinding> bindings = new();

        var yaml = System.Text.Encoding.UTF8.GetString(data);

        var bindingDtos = _contentManager.GetDeserializer().Deserialize<Dictionary<string, KeyBindingDto>>(yaml);
        foreach(var kvp in bindingDtos) {
            var id = kvp.Key;
            var binding = kvp.Value.ToKeyBinding(id);

            bindings.Add(binding);
        }

        return bindings;
    }

}