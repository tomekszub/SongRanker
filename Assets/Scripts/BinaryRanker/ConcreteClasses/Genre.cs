using Newtonsoft.Json;

namespace Immortus.SongRanker
{
    [JsonObject(MemberSerialization.Fields)]
    public class Genre : UniqueIDProperty<Genre>, IRankable
    {
        string _name;

        [JsonIgnore] public string Name => _name;

        public Genre(string name)
        {
            _name = name;
        }

        public override bool IsEqual(Genre property)
        {
            return _name == property._name;
        }

        public override bool IsValid() => !string.IsNullOrEmpty(_name);

        public string GetDisplayName() => _name;
    }
}