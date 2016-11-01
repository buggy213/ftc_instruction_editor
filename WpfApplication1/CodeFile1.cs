using System.Collections.Generic;

public struct JsonObject
{
    public List<ObjectElement> elements;   
}
public struct ObjectElement
{
    public KeyValuePair<string, string> element;
}
