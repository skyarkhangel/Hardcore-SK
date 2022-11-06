using UnityEngine;
using Verse;

namespace RocketMan.Tabs
{
    public abstract class ITabContent
    {
        private bool selected;

        public bool Selected
        {
            get => selected;
            set
            {
                if (selected == value)
                {
                    return;
                }
                selected = value;
                if (value) OnSelect();
                else OnDeselect();
            }
        }

        public abstract Texture2D Icon { get; }
        public abstract bool ShouldShow { get; }

        public virtual float LabelWidth => GUIFont.CalcSize(Label).x;

        public abstract string Label { get; }
        public abstract void DoContent(Rect rect);

        public virtual void OnSelect()
        {
            if (!RocketPrefs.WarmingUp)
            {
                RocketMod.Instance.WriteSettings();
            }
        }

        public virtual void OnDeselect()
        {
            if (!RocketPrefs.WarmingUp)
            {
                RocketMod.Instance.WriteSettings();
            }
        }
    }
}