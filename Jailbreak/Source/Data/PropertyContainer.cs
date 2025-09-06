using System.Collections.Generic;

namespace Jailbreak.Data;

public class PropertyContainer {

    private Dictionary<string, Dictionary<string, string>> properties;

    public PropertyContainer(string fileContents) {
        properties = new Dictionary<string, Dictionary<string, string>>();

        string[] lines = fileContents.Split("\n");
        string currentSection = "";

        foreach(string line in lines) {
            if(line.StartsWith('[')) {
                currentSection = line.Trim().Substring(1, line.IndexOf(']') - 1);
                properties.Add(currentSection, new Dictionary<string, string>());
            }
            if(currentSection != "") {
                if(line.Contains('=')) {
                    var property = line.Trim().Split('=');
                    if (properties[currentSection].ContainsKey(property[0])) continue;
                    properties[currentSection].Add(property[0], property[1]);
                }
            }
        }
    }

    public Dictionary<string, string> GetSection(string sectionName) {
        return properties[sectionName];
    }

    public Dictionary<string, string> GetSectionOrEmpty(string sectionName) {
        if (properties.TryGetValue(sectionName, out var section)) {
            return section;
        }
        else return new Dictionary<string, string>();
    }

    public string GetProperty(string propertyName, string sectionName) {
        return GetSection(sectionName)[propertyName];
    }

    public string GetPropertyOrDefault(string propertyName, string sectionName, string defaultValue) {
        if (GetSectionOrEmpty(sectionName).TryGetValue(propertyName, out var property)) {
            return property;
        }
        else return defaultValue;
    }

    public bool ContainsProperty(string propertyName, string sectionName) {
        return properties.ContainsKey(sectionName) && properties[sectionName].ContainsKey(propertyName);
    }

}