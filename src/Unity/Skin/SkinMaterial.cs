using GOILauncher.Multiplayer.Unity.Models;
using UnityEngine;

namespace GOILauncher.Multiplayer.Unity.Skin
{
    /// <summary>
    /// 皮肤外观落到「材质」这一层的平台差异都收在这里。上面各层只谈「这个槽位的贴图和金度是什么」，
    /// 具体怎么写进材质——PC 是 shader 属性、Android 是 Substance 的 procedural input——只此一处知道。
    /// 这与 <see cref="Helpers.Physics2DHelper"/> 用平台常量二选一是同一套路。
    /// </summary>
    /// <remarks>
    /// 罐子金度的两套机制（见 docs/agent/game-runtime.md 的 Pot Skin）：
    /// <list type="bullet">
    /// <item>PC：普通 <see cref="Material"/>，金度是 shader 浮点属性
    /// <see cref="GameConstants.GoldnessProperty"/>（<c>_Goldness</c>），用 <c>GetFloat</c>/<c>SetFloat</c>，
    /// 与贴图各自独立、可叠加。</item>
    /// <item>Android：罐子是 <c>ProceduralMaterial</c>，金度是 procedural input
    /// <see cref="GameConstants.GoldnessProceduralInput"/>（<c>Goldness</c>，无下划线），靠
    /// <c>SetProceduralFloat</c> + <c>RebuildTextures</c> 把金色烘进生成的贴图里。烘出来的贴图会占用
    /// <c>mainTexture</c>，所以在 Android 上「自定义皮肤」与「金罐」天生二选一——这与它们在领域上本就互斥一致。</item>
    /// </list>
    /// 身体（Diogenes）等非罐子部件：PC 上材质没有 <c>_Goldness</c>，Android 上不是 <c>ProceduralMaterial</c>，
    /// 两条路都天然把金度当空操作，所以本类对所有槽位通用，调用方不需要区分部件。
    /// </remarks>
    public static class SkinMaterial
    {
        /// <summary>
        /// 读出材质当前的金度。没有金度概念的材质（身体、非程序化）返回 0，让金度对这些槽位天然惰性。
        /// 读本地皮肤走 <c>sharedMaterial</c>，那份在 Android 上就是罐子的 <c>ProceduralMaterial</c> 本体。
        /// </summary>
        public static float ReadGoldness(Material material)
        {
#if ANDROID
            // ProceduralMaterial 在 Unity 2018.1 起被标记过时，但 Android 版游戏用的正是这套 Substance
            // 材质，它是这个平台上唯一的金度机制，弃用告警在这里无法避免也不该修，压掉即可。
#pragma warning disable 0618
            var procedural = material as ProceduralMaterial;
            return procedural != null
                ? procedural.GetProceduralFloat(GameConstants.GoldnessProceduralInput)
                : 0f;
#pragma warning restore 0618
#else
            return material.HasProperty(GameConstants.GoldnessProperty)
                ? material.GetFloat(GameConstants.GoldnessProperty)
                : 0f;
#endif
        }

        /// <summary>
        /// 把贴图和金度落到材质上。<paramref name="texture"/> 为 null 表示不改贴图。
        /// 平台差异见类型说明；调用方只管把该槽位算出来的贴图和金度递进来。
        /// 写远端实例时传的是 <c>renderer.material</c>（每个实例自己那份副本），绝不是 <c>sharedMaterial</c>。
        /// </summary>
        public static void ApplyTo(Material material, Texture2D texture, float goldness)
        {
#if ANDROID
            // ProceduralMaterial 弃用告警在 Android 上无法避免（见 ReadGoldness 的说明），压掉。
#pragma warning disable 0618
            var procedural = material as ProceduralMaterial;
            if (procedural != null)
            {
                // 罐子：金罐与皮肤二选一。有金度就重烘 substance 生成原生金罐贴图（会占用 mainTexture），
                // 此时不再贴自定义贴图；没金度就照常贴贴图（黑罐基线 / 自定义皮肤）。
                if (goldness > 0f)
                {
                    procedural.SetProceduralFloat(GameConstants.GoldnessProceduralInput, goldness);
                    // 必须用同步重烘。RebuildTextures() 是异步排进 Substance 帧末队列，而远端实例经
                    // renderer.material 拷出来的这份副本不在队列里，异步重烘不会触发，金色永远出不来（实机确认：
                    // 换成 Immediately 后 Android 观察端才看得到金罐）。Immediately 当场重烘，绕开这一点。
                    procedural.RebuildTexturesImmediately();
                }
                else if (texture != null)
                {
                    procedural.mainTexture = texture;
                }
                return;
            }
#pragma warning restore 0618

            // 身体等非程序化材质：只有贴图，没有金度。
            if (texture != null)
            {
                material.mainTexture = texture;
            }
#else
            if (texture != null)
            {
                material.mainTexture = texture;
            }
            if (material.HasProperty(GameConstants.GoldnessProperty))
            {
                material.SetFloat(GameConstants.GoldnessProperty, goldness);
            }
#endif
        }
    }
}
