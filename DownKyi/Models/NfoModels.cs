using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DownKyi.Models;

[Serializable]
[XmlRoot("movie")]
public class MovieMetadata 
{
        
    [XmlElement("title")]
    public string Title { get; set; }
    
    [XmlElement("plot")]
    public string Plot { get; set; }
    
    [XmlElement("year")]
    public string Year { get; set; }
    
    [XmlElement("genre")]
    public List<string> Genres { get; set; }
    
    [XmlElement("tag")]
    public List<string> Tags { get; set; }
    
    [XmlElement("actor")]
    public List<Actor> Actors { get; set; }
    
    [XmlElement("uniqueid")]
    public  UniqueId BilibiliId { get; set; }
    
    [XmlElement("premiered")]
    public string Premiered { get; set; }
    
    [XmlElement("rating")]
    public List<Rating> Ratings { get; set; }
}

[Serializable]
public class UniqueId
{
    [XmlAttribute("type")]
    public string Type { get; set; }

    [XmlText]
    public string Value { get; set; }

    public UniqueId() { }

    public UniqueId(string type, string value)
    {
        Type = type;
        Value = value;
    }
}



[Serializable]
public class Actor
{
    [XmlElement("name")]
    public string Name { get; set; }

    [XmlElement("role")]
    public string Role { get; set; }
    
    public Actor() { }
    public Actor(string name, string role)
    {
        Name = name;
        Role = role;
    }
}

[Serializable]
public class Rating
{
    [XmlAttribute("name")]
    public string Name { get; set; }  
    
    
    [XmlAttribute("max")]
    public int Max { get; set; } 
    
    [XmlAttribute("default")]
    public bool IsDefault { get; set; } 
    
    [XmlText]
    public float Value { get; set; }
    
    public Rating() { }

    public Rating(string name, float value, int max = 10, bool isDefault = false)
    {
        Name = name;
        Value = value;
        Max = max;
        IsDefault = isDefault;
    }
}