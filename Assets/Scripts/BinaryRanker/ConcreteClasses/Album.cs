using Newtonsoft.Json;

namespace Immortus.SongRanker
{
    [JsonObject(MemberSerialization.Fields)]
    public class Album : UniqueIDProperty<Album>, IRankable
    {
        string _name;
        int _artistId;

        [JsonIgnore] public string Name => _name;
        [JsonIgnore] public int ArtistID => _artistId;

        public Album(string name, int artistId)
        {
            _name = name;
            _artistId = artistId;
        }

        public override bool IsEqual(Album property)
        {
            return _name == property._name && _artistId == property._artistId;
        }

        public override bool IsValid() => !string.IsNullOrEmpty(_name);

        public string GetDisplayName() => _name;
    }
}