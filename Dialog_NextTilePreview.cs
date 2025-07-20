using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            // Устанавливаем начальную позицию окна
            this.windowRect = new Rect(
                (UI.screenWidth - InitialSize.x) / 2f,
                (UI.screenHeight - InitialSize.y) / 2f + YOffset,
                InitialSize.x,
                InitialSize.y
            );
        }

        // Опционально: можно переопределить InitialSize для изменения размера
        public override Vector2 InitialSize => new Vector2(500f, 300f);

        // Отключаем автоматическое центрирование окна
        protected override void SetInitialSizeAndPosition()
        {
            // Оставляем пустым, чтобы не перезаписывалась наша позиция
        }
    }
}
