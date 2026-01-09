using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace LightHouse.UIExtensions
{
    public static class DropdownFieldExtensions
    {
        /// <summary>
        /// Met à jour les choix d'un DropdownField tout en conservant la sélection existante si possible.
        /// </summary>
        /// <param name="dropdown">Le DropdownField à modifier</param>
        /// <param name="newChoices">La nouvelle liste de choix</param>
        public static void UpdateChoices(this DropdownField dropdown, List<string> newChoices)
        {
            if (dropdown == null || newChoices == null)
                return;

            string previousSelection = dropdown.value;

            dropdown.choices = newChoices;

            if (newChoices.Contains(previousSelection))
            {
                dropdown.SetValueWithoutNotify(previousSelection);
            }
            else if (newChoices.Count > 0)
            {
                dropdown.SetValueWithoutNotify(newChoices[0]);
            }
            else
            {
                dropdown.SetValueWithoutNotify(string.Empty);
            }
            dropdown.MarkDirtyRepaint();
        }
    }
}
