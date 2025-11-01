using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Immortus.SongRanker
{
    public class PopupController : MonoBehaviour
    {
        public static PopupController Instance;
        
        [SerializeField] SimpleHintEditPopup _SimpleHintEditPopup;
        [SerializeField] SimpleEditPopup _SimpleEditPopup;
        [SerializeField] MultiHintEditPopup _MultiHintEditPopup;
        [SerializeField] CreateAlbumPopup _CreateAlbumPopup;
        [SerializeField] ConfirmationPopup _ConfirmationPopup;
        [SerializeField] ProgressPopup _ProgressPopup;

        public void Awake()
        {
            if(Instance != null)
            {
                Destroy(gameObject);
                return;
            }
                
            Instance = this;
        }

        public static void ShowConfirmationPopup(string message, Action onConfirm, Action onCancel = null)
        {
            Instance?._ConfirmationPopup.Show(message, onConfirm, onCancel);
        }
        
        public static void ShowProgressPopup() => Instance._ProgressPopup.Show();
        
        public static void SetProgressPopupContent(float normalizedValue, string text) => Instance?._ProgressPopup.SetProgress(normalizedValue, text);
        
        public static void HideProgressPopup() => Instance._ProgressPopup.Hide();

        public static void ShowSimpleEditPopup(string title, string oldValue, Action<string> onSave,
            Func<string, bool> onValidate)
        {
            if(Instance == null)
                return;
            
            Instance._SimpleEditPopup.Show(title, oldValue, onSave, onValidate);
        }
        
        public static void ShowHintEditPopup(string title, string oldValue, Action<string> onSave,
            Func<string, bool> onValidate, List<string> recommendedValues)
        {
            if(Instance == null)
                return;

            Instance._SimpleHintEditPopup.Show(title, oldValue, onSave, onValidate, recommendedValues);
        }
        
        public static void HideHintEditPopup() => Instance?._SimpleHintEditPopup.Hide();
        
        public static void ShowMultipleEditPopup(string title, List<string> oldValues, Action<List<string>> onSave,
            Func<List<string>, bool> onValidate, List<string> recommendedValues)
        {
            if(Instance == null)
                return;

            Instance._MultiHintEditPopup.Show(title, oldValues, onSave, onValidate, recommendedValues);
        }

        public static void ShowCreateAlbumPopup(string albumName, Action<int> onSave, Action onCancel)
        {
            if(Instance == null)
                return;
            
            Instance._CreateAlbumPopup.Show(albumName, onSave, onCancel);
        }
    }
}
