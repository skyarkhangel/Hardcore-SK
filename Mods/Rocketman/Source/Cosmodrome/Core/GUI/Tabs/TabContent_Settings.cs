using UnityEngine;
using Verse;

namespace RocketMan.Tabs
{
    public class TabContent_Settings : ITabContent
    {
        public override Texture2D Icon => TexTab.Settings;
        public override string Label => KeyedResources.RocketMan_Tab;
        public override bool ShouldShow => true;

        private Texture2D graphic = ContentFinder<Texture2D>.Get("RocketMan/UI/rocketman_main_nobackground", true);

        public override void DoContent(Rect rect)
        {
            GUIUtility.ExecuteSafeGUIAction(() =>
            {
                if (RocketPrefs.WarmingUp)
                {
                    GUIUtility.ExecuteSafeGUIAction(() =>
                    {
                        GUIFont.Font = GUIFontSize.Medium;
                        GUIFont.Anchor = TextAnchor.MiddleCenter;
                        if (Find.TickManager.Paused)
                            Widgets.Label(rect, KeyedResources.RocketMan_Settings_PleaseWait);
                        else
                            Widgets.Label(rect, KeyedResources.RocketMan_Settings_PleaseUnpause);
                    });
                }
                else
                {
                    Rect imageRect = rect.TopPartPixels(200);
                    imageRect.width = 685 - 180;
                    Widgets.DrawTextureFitted(imageRect, graphic, 1.0f);
                    rect.yMin += 215;
                    RocketMod.DoSettings(rect);
                }
            });
        }

        public override void OnSelect()
        {
            base.OnSelect();
        }

        public override void OnDeselect()
        {
            base.OnDeselect();
        }
    }
}