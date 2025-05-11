using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Immortus
{
    public enum ComparisonResult
    {
        None,
        Better,
        Worse,
        Same
    }

    public class Ranker<T>
    {
        public Action OnRankingChanged;
        public Action<List<T>> OnCurrentlyComapredElementChanged;

        List<List<T>> _ranking;
        T _newElement;
        bool _comparingDone = true;
        Coroutine _rankingCoroutine;
        ComparisonResult _comparisonResult;
        int _index;
        int _start;
        int _end;
        Stack<(int, int, int)> _history = new();

        public List<List<T>> Ranking => _ranking;

        public Ranker() { }

        public Ranker(List<List<T>> ranking)
        {
            _ranking = ranking;
        }

        public void Stop()
        {
            if(_comparingDone)
                return;
            
            if(_rankingCoroutine != null)
                CoroutineHandler.StopCoroutine(_rankingCoroutine);
            
            _comparingDone = true;
        }
        
        public void SetComparisonResult(ComparisonResult result)
        {
            if(_comparisonResult == ComparisonResult.None)
                _comparisonResult = result;
        }

        public bool AddElement(T element)
        {
            if (!_comparingDone)
                return false;

            if (_ranking == null || _ranking.Count == 0)
            {
                _ranking = new(){new List<T> { element }};
                OnRankingChanged?.Invoke();
                return true;
            }

            _newElement = element;
            _comparingDone = false;
            _comparisonResult = ComparisonResult.None;
            _index = _ranking.Count / 2;
            _start = 0;
            _end = _ranking.Count - 1;
            SaveHistory();
            _rankingCoroutine = CoroutineHandler.StartCoroutine(RankElement());
            return true;
        }

        public void Undo()
        {
            if (_comparingDone || _comparisonResult != ComparisonResult.None || _history.Count <= 1)
                return;

            _history.Pop();
            var lastState = _history.Peek();
            _index = lastState.Item1;
            _start = lastState.Item2;
            _end = lastState.Item3;
            CallOnCurrentlyComparedElementChanged();
        }

        IEnumerator RankElement()
        {
            CallOnCurrentlyComparedElementChanged();

            while (true)
            {
                yield return new WaitUntil(() => _comparisonResult != ComparisonResult.None);

                if (_comparisonResult == ComparisonResult.Same)
                {
                    FinishComparing(false);
                    yield break;
                }

                (_index, _start, _end) = GetNextIndexToCompare(_comparisonResult == ComparisonResult.Better, _start, _end);

                _comparisonResult = ComparisonResult.None;

                if (_start == -1)
                {
                    FinishComparing(true);
                    yield break;
                }

                SaveHistory();
                CallOnCurrentlyComparedElementChanged();
            }

            void FinishComparing(bool insert)
            {
                if(insert)
                    _ranking.Insert(_index, new() { _newElement });
                else
                    _ranking[_index].Add(_newElement);

                _history.Clear();
                OnRankingChanged?.Invoke();
                _comparingDone = true;
                _rankingCoroutine = null;
            }
        }

        void CallOnCurrentlyComparedElementChanged()
        {
            OnCurrentlyComapredElementChanged?.Invoke(_ranking[_index]);
        }

        void SaveHistory() => _history.Push((_index, _start, _end));

        (int, int, int) GetNextIndexToCompare(bool shouldBeHigher, int startingIndex, int endIndex)
        {
            int range = endIndex - startingIndex;

            if (range == 0)
                return (shouldBeHigher ? startingIndex : startingIndex + 1, -1, -1);
            if (range == 1 && !shouldBeHigher)
                return (endIndex + 1, -1, -1);

            int midIndex = startingIndex + ((range + 1) / 2);

            int index, start, end;

            if (shouldBeHigher)
            {
                start = startingIndex;
                end = midIndex - 1;
            }
            else
            {
                start = midIndex + 1;
                end = endIndex;
            }

            index = start + ((end - start + 1) / 2);

            return (index, start, end);
        }
    }
}