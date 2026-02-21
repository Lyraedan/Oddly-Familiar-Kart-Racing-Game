using System;
using System.Collections.Generic;
using System.Linq;
using Modio.Mods;
using Modio.Unity.UI.Panels;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Modio.Unity.UI.Components
{
    public class ModioUIGroup<TResource, TModioUIContainer> : MonoBehaviour 
        where TModioUIContainer : MonoBehaviour, IModioUIPropertiesOwner, IModioUIResourceContainer<TResource>
        where TResource : IModioInfo
    {
        static readonly Dictionary<TResource, TModioUIContainer> TempActive = new Dictionary<TResource, TModioUIContainer>();

        TModioUIContainer _template;

        readonly List<TModioUIContainer> _active = new List<TModioUIContainer>();
        readonly List<TModioUIContainer> _inactive = new List<TModioUIContainer>();

        (IReadOnlyList<TResource> mods, int selectionIndex) _displayOnEnable;

        [SerializeField, Tooltip("(Optional) The root layout to rebuild before performing selections")]
        RectTransform _layoutRebuilder;

        void Awake()
        {
            _template = GetComponentInChildren<TModioUIContainer>();

            if (_template != null)
            {
                _template.gameObject.SetActive(false);
                _inactive.Add(_template);
            }
            else
            {
                Debug.LogWarning(
                    $"{nameof(ModioUIGroup<TResource, TModioUIContainer>)} {gameObject.name} could not find a child {nameof(TModioUIContainer)} template, disabling.",
                    this
                );

                enabled = false;
            }
        }

        void OnEnable()
        {
            if (_displayOnEnable.mods != null)
            {
                SetMods(_displayOnEnable.mods, _displayOnEnable.selectionIndex);
                _displayOnEnable = default;
            }
        }

        public void SetMods(IReadOnlyList<TResource> mods, int selectionIndex = 0)
        {
            if (!enabled)
            {
                _displayOnEnable = (mods, selectionIndex);

                return;
            }

            //Treat a null mod list as an empty mod list
            mods ??= Array.Empty<TResource>();

            TempActive.Clear();

            foreach (TModioUIContainer uiMod in _active)
            {
                if (mods.Contains(uiMod.Resource) && !TempActive.ContainsKey(uiMod.Resource))
                    TempActive.Add(uiMod.Resource, uiMod);
                else
                {
                    uiMod.gameObject.SetActive(false);
                    uiMod.SetResource(default(TResource));

                    _inactive.Add(uiMod);
                }
            }

            _active.Clear();

            for (var i = 0; i < mods.Count; i++)
            {
                bool active = TempActive.Remove(mods[i], out TModioUIContainer uiMod);

                if (!active)
                {
                    if (_inactive.Any())
                    {
                        int lastIndex = _inactive.Count - 1;
                        uiMod = _inactive[lastIndex];

                        _inactive.RemoveAt(lastIndex);
                    }
                    else
                    {
                        uiMod = Instantiate(_template.gameObject, _template.transform.parent)
                            .GetComponent<TModioUIContainer>();
                    }

                    uiMod.SetResource(mods[i]);
                }

                uiMod.transform.SetSiblingIndex(i);
                if (!active) uiMod.gameObject.SetActive(true);

                _active.Add(uiMod);
            }

            var eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                ModioLog.Error?.Log("You are missing an event system, which the Modio UI requires to work. Consider adding ModioUI_InputCapture to your scene");
                return;
            }

            var currentSelectedGameObject = eventSystem.currentSelectedGameObject;
            var shouldDoSelection = currentSelectedGameObject == null || !currentSelectedGameObject.activeInHierarchy;

            if (!shouldDoSelection && _active.Count > 0 && selectionIndex == 0)
            {
                // Force the selection if we have a child selected, and we should be setting to index 0 (as it's a new, non additive, search)
                shouldDoSelection |= currentSelectedGameObject.transform.parent == _active[0].transform.parent;
            }

            if (shouldDoSelection)
            {
                //Ensure layouts have been applied, otherwise we'll snap scrollviews to their old positions
                if (_layoutRebuilder != null) LayoutRebuilder.ForceRebuildLayoutImmediate(_layoutRebuilder);

                var currentFocusedPanel = ModioPanelManager.GetInstance().CurrentFocusedPanel;
                if (_active.Count > 0)
                {
                    currentFocusedPanel.SetSelectedGameObject(
                        _active[Mathf.Min(selectionIndex, _active.Count - 1)].gameObject
                    );
                }
                else
                {
                    if (currentFocusedPanel != null) currentFocusedPanel.DoDefaultSelection();
                }
            }
        }
    }
}
