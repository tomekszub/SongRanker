﻿using System;
using TMPro;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Immortus.SongRanker
{
    public class CreateAlbumPopup : MonoBehaviour
    {
        [SerializeField] RecommendedValuesPanel _RecommendedValuesPanel;
        [SerializeField] TextMeshProUGUI _AlbumNameText;
        [SerializeField] TMP_InputField _InputField;
        [SerializeField] Button _SaveButton;
        [SerializeField] Button _CancelButton;

        Action<int> _onSave;

        public void SetData(string name, Action<int> onSave, Action onCancel)
        {
            _onSave = onSave;

            _AlbumNameText.text = name;
            _InputField.onValueChanged.RemoveAllListeners();
            _InputField.onValueChanged.AddListener(InputValueChanged);

            _SaveButton.onClick.RemoveAllListeners();
            _SaveButton.onClick.AddListener(Save);

            _CancelButton.onClick.RemoveAllListeners();
            _CancelButton.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
                onCancel?.Invoke();
            });

            _RecommendedValuesPanel.SetData(SongManager.AllArtistNames.ToList());

            gameObject.SetActive(true);
        }

        public void UseRecommendedValue(TextMeshProUGUI source) => _InputField.text = source.text;

        void InputValueChanged(string value)
        {
            bool valid = !string.IsNullOrEmpty(value);

            _SaveButton.gameObject.SetActive(valid);

            _RecommendedValuesPanel.Refresh(value);
        }

        void Save()
        {
            var ids = SongManager.GetArtistIDsByNames(new() { _InputField.text });

            _onSave?.Invoke(ids[0]);

            gameObject.SetActive(false);
        }
    }
}