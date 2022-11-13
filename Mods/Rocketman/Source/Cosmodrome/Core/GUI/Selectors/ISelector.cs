using System;
using UnityEngine;
using Verse;

namespace RocketMan
{
    public abstract class ISelector : Window
    {
        protected readonly Action closeAction;

        protected bool integrated;

        public Vector2 scrollPosition = Vector2.zero;

        public ISelector(bool integrated = false, Action closeAction = null)
        {
            this.integrated = integrated;
            this.closeAction = closeAction;
        }

        public override void DoWindowContents(Rect inRect)
        {
            integrated = false;
            GUIUtility.ExecuteSafeGUIAction(() =>
            {
                if (Widgets.ButtonText(inRect.BottomPartPixels(30), KeyedResources.RocketMan_Close))
                {
                    Close();
                }
                inRect.yMax -= 35;
                DoContent(inRect);
            });
        }

        public void DoIntegratedContents(Rect inRect)
        {
            GUIUtility.ExecuteSafeGUIAction(() =>
            {
                integrated = true;
                DoContent(inRect);
            });
        }

        public abstract void DoContent(Rect inRect);

        public override void Close(bool doCloseSound = true)
        {
            if (!integrated)
                base.Close(doCloseSound);
            else
                closeAction.Invoke();
        }
    }
}