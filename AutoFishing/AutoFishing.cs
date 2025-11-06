using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AutoFishing
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private Harmony harmony;
        private const string HARMONY_ID = "com.violet.AutoFishingmod";

        private static bool patchesApplied = false;

        protected override void OnAfterSetup()
        {
            base.OnAfterSetup();
            try
            {
                if (patchesApplied)
                {
                    Debug.Log("[自动钓鱼补丁] 已存在，跳过重复加载。");
                    return;
                }

                harmony = new Harmony("com.violet.AutoFishingmod");
                harmony.PatchAll();
                patchesApplied = true;
                Debug.Log("[自动钓鱼补丁] 补丁已注册并标记为已加载。");
            }
            catch (Exception ex)
            {
                Debug.LogError("[自动钓鱼补丁] 初始化失败:" + ex);
            }
        }

        protected override void OnBeforeDeactivate()
        {
            base.OnBeforeDeactivate();
            try
            {
                if (harmony != null)
                {
                    harmony.UnpatchAll(HARMONY_ID);
                    harmony = null;
                    patchesApplied = false; // 解除标志
                    Debug.Log("[自动钓鱼补丁] 所有补丁已卸载。");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[自动钓鱼补丁] OnBeforeDeactivate error: " + ex);
            }
        }

        [HarmonyPatch(typeof(Action_FishingV2))]
        public static class Action_FishingV2_Patch
        {
            private static bool isRewarding = false;

            // ✅ 等待时间减半
            [HarmonyPostfix]
            [HarmonyPatch("TransToWaiting")]
            public static void TransToWaiting_Postfix(Action_FishingV2 __instance)
            {
                var waitTimeField = AccessTools.Field(typeof(Action_FishingV2), "waitTime");
                if (waitTimeField == null) return;
                float waitTime = (float)waitTimeField.GetValue(__instance);
                waitTime *= 0.3f;
                waitTimeField.SetValue(__instance, waitTime);
            }

            // ✅ 自动成功判定
            [HarmonyPostfix]
            [HarmonyPatch("TransToRing")]
            public static void TransToRing_Postfix(Action_FishingV2 __instance)
            {
                __instance.StartCoroutine(AutoSuccessAfterDelay(__instance));
            }

            private static IEnumerator AutoSuccessAfterDelay(Action_FishingV2 __instance)
            {
                Debug.Log("[FishingPatch] 自动钓鱼成功判定启动");
                yield return new WaitForSeconds(0.2f);
                try
                {
                    var method = AccessTools.Method(typeof(Action_FishingV2), "TransToSuccessback");
                    method?.Invoke(__instance, null);
                    Debug.Log("[FishingPatch] 已自动判定钓鱼成功");
                }
                catch (Exception ex)
                {
                    Debug.LogError("[FishingPatch] 自动成功异常：" + ex);
                }
            }
          
        }
    }
}
