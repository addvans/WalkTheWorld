using System;
using UnityEngine;
using Verse;

namespace WalkTheWorld
{
    public class Dialog_MessageBoxAdjusted : Dialog_MessageBox
    {
        private const float YOffset = 200f;

        public Dialog_MessageBoxAdjusted(string text, string buttonAText, Action buttonAAction,
            string buttonBText, Action buttonBAction)
            : base(text, buttonAText, buttonAAction, buttonBText, buttonBAction)
        {
            this.windowRect = new Rect(
                (UI.screenWidth - InitialSize.x) / 2f,
                (UI.screenHeight - InitialSize.y) / 2f + YOffset,
                InitialSize.x,
                InitialSize.y
            );
        }
        public override Vector2 InitialSize => new Vector2(500f, 300f);
        protected override void SetInitialSizeAndPosition()
        {
        }
    }
}
