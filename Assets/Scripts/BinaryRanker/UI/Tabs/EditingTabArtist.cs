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
        [SerializeField] PropertyField _SongsCountField;
        [SerializeField] PropertyField _SongsAverageField;
        [SerializeField] PropertyField _SongsCustomRankingField;
        [SerializeField] RecyclableVerticalScrollView _SongsView;
        
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
            
            var rankingsObject = _rankerController.RankingsController.GetArtistRanking(_currentElement);
            
            _SongsCountField.SetContent(rankingsObject.SongCount.ToString());
            _SongsAverageField.SetContent(rankingsObject.AvgRating.ToString("F2"));
            _SongsCustomRankingField.SetContent(rankingsObject.CustomRating.ToString("F2"));

            List<IRecyclableData> recyclableDatas = new();

            if(SongManager.ArtistToSongs.TryGetValue(_currentElement.ID, out var songs))
            {
                List<(int, Song)> songTuples = new();
                foreach(var song in songs)
                    songTuples.Add((_rankerController.GetRankingPosition(song.ID), song));
                
                foreach(var songTuple in songTuples.OrderByDescending(st => st.Item1))
                {
                    if(songTuple.Item1 == 0)
                        recyclableDatas.Add(new SimpleStringRecyclableData($"{songTuple.Item2.Name}"));
                    else
                        recyclableDatas.Insert(0, new SimpleStringRecyclableData($"{songTuple.Item1}. {songTuple.Item2.Name}"));
                }
            }

            _SongsView.RefreshData(recyclableDatas);
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
