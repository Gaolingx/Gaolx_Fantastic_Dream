// Magica Cloth 2.
// Copyright (c) 2023 MagicaSoft.
// https://magicasoft.jp
using System;
using UnityEngine;

namespace MagicaCloth2
{
    /// <summary>
    /// MagicaCloth manager API
    /// </summary>
    public static partial class MagicaManager
    {
        /// <summary>
        /// シミュレーション開始前イベント
        /// Pre-simulation event.
        /// </summary>
        public static Action OnPreSimulation;

        /// <summary>
        /// シミュレーション完了後イベント
        /// Post-simulation event.
        /// </summary>
        public static Action OnPostSimulation;


        /// <summary>
        /// グローバルタイムスケールを変更します
        /// Change the global time scale.
        /// </summary>
        /// <param name="timeScale">0.0-1.0</param>
        public static void SetGlobalTimeScale(float timeScale)
        {
            if (IsPlaying())
            {
                Team.globalTimeScale = Mathf.Clamp01(timeScale);
            }
        }

        /// <summary>
        /// グローバルタイムスケールを取得します
        /// Get the global time scale.
        /// </summary>
        /// <returns></returns>
        public static float GetGlobalTimeScale()
        {
            if (IsPlaying())
            {
                return Team.globalTimeScale;
            }
            else
                return 1.0f;
        }

    }
}
