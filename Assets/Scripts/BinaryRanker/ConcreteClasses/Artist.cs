using Newtonsoft.Json;

namespace Immortus.SongRanker
{
    [JsonObject(MemberSerialization.Fields)]
    public class Artist : UniqueIDProperty<Artist>, IRankable
    {
        string _name;
        int _countryId;

        public int ID => _id;
        public string Name => _name;

        public Artist(string name, int countryId)
        {
            _name = name;
            _countryId = countryId;
        }
        
        public override bool IsEqual(Artist property)
        {
            if(property._id != -1 && _id == property._id)
                return true;
            
            return _name == property._name && (property._countryId == -1 || _countryId == property._countryId);
        }

        public override bool IsValid() => !string.IsNullOrEmpty(_name);

        public string GetDisplayName() => _name;
    }
}