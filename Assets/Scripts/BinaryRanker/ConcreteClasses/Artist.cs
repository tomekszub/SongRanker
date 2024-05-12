using Newtonsoft.Json;

namespace Immortus.SongRanker
{
    [JsonObject(MemberSerialization.Fields)]
    public class Artist : UniqueIDProperty<Artist>, IRankable
    {
        string _name;
        int _countryId;

        public string Name => _name;

        public Artist(string name, int countryId)
        {
            _name = name;
            _countryId = countryId;
        }

        public override bool IsEqual(Artist property)
        {
            return _name == property._name && _countryId == property._countryId;
        }

        public override bool IsValid() => !string.IsNullOrEmpty(_name);

        public string GetDisplayName() => _name;
    }
}