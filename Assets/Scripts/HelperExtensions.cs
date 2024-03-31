using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class HelperExtensions
{
    public static bool IsNewListValid(List<string> newValues, List<string> oldValues)
    {
        bool valid = false;

        if (newValues.Count != 0)
        {
            valid = false;

            var currentValuesOrdered = newValues.OrderBy(s => s).ToArray();
            var currentOrdered = oldValues.OrderBy(s => s).ToArray();

            for (int i = 0; i < newValues.Count; i++)
            {
                if (i == oldValues.Count)
                {
                    valid = true;
                    break;
                }

                if (currentValuesOrdered[i] != currentOrdered[i] && !string.IsNullOrEmpty(currentValuesOrdered[i]))
                {
                    valid = true;
                    break;
                }
            }
        }

        return valid;
    }
}
