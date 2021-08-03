using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Kityme.Entities
{
    [BsonIgnoreExtraElements]
    public class Cat
    {
        public int atractive = 1;
        public string name;
        public string type;
        public Cat (int _atr, string _name)
        {
            var values = Enum.GetValues(typeof(Types));
            this.atractive = _atr;
            this.name = _name;
            this.type = Enum.GetName(typeof(Types), new Random().Next(0, values.Length));
        }
    }

    public enum Types
    {
        Ninja,
        Mago,
        Youtuber
    }
}