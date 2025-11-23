using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Immortus.SongRanker
{
    public class EditingTabArtist : ContextBaseEditingTabContainer<Artist>
    {
        [SerializeField] TextMeshProUGUI _RankingPositionField;
        [SerializeField] PropertyField _TitleField;
        
        public override void Init(RankerController rankerController, Action onChangeDone)
        {
            base.Init(rankerController, onChangeDone);
            
            var ranking = _rankerController.RankingsController.GetCurrentRanking(RankingsController.RankingType.Artist);

            var sortedRanking = ranking.OrderByDescending(r => r.CustomRating);

            foreach(var rankable in sortedRanking)
                _context.Add(rankable.Value as Artist);
        }

        public override List<string> GetSearchData()
        {
            return SongManager.AllArtistNames.ToList();
        }

        protected override void Refresh()
        {
            _RankingPositionField.text = $"{_currIndex + 1}";

            bool propertyIsValid = PropertiesValidator.ValidateName(_currentElement.Name);
            _TitleField.SetContent(_currentElement.Name, !propertyIsValid, () => Debug.LogError("EDIT"));
        }

        public override void ShowDataWithIndex(int index)
        {
            var artistID = SongManager.GetAllArtists().ToList()[index].ID;

            for (int i = 0; i < _context.Count; i++)
            {
                if (_context[i].ID == artistID)
                {
                    _currIndex = i;
                    OnItemChanged();
                }
            }
        }
    }
}
