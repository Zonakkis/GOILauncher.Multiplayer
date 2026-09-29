using System;
using System.Text;
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
    /// <c>SetProceduralFloat</c> + <c>RebuildTextures</c> 把金色烘进生成的贴图里。金色是像素而不是
    /// shader 参数，所以在 Android 上「自定义皮肤」与「金罐」天生二选一——这与它们在领域上本就互斥一致。
    /// 还有一条容易踩的：<c>RebuildTextures</c> 只重写生成贴图的像素，**不会把结果重新绑回
    /// <c>mainTexture</c>**，所以写金度前必须先把槽位指回 <c>GetGeneratedTextures()[0]</c>，
    /// 否则槽位上挂的是别的静态贴图，金色烘出来也没人显示（详见 <see cref="ApplyTo"/>）。</item>
    /// </list>
    /// 身体（Diogenes）等非罐子部件：PC 上材质没有 <c>_Goldness</c>，Android 上不是 <c>ProceduralMaterial</c>，
    /// 两条路都天然把金度当空操作，所以本类对所有槽位通用，调用方不需要区分部件。
    /// </remarks>
    public static class SkinMaterial
    {
        /// <summary>金度在这个差值以内就认为「没变」，省掉一次 substance 重烘。</summary>
        private const float GoldnessEpsilon = 0.0001f;

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
        /// <paramref name="hasCustomSkin"/> 表示这名玩家这个槽位到底有没有自定义皮肤——
        /// 没有的话 <paramref name="texture"/> 是调用方递进来的原版基线，安卓上会被忽略
        /// （原版罐子由 substance 按金度现生成，见下面分支里的说明）。
        /// 平台差异见类型说明；调用方只管把该槽位算出来的贴图和金度递进来。
        /// 写远端实例时传的是 <c>renderer.material</c>（每个实例自己那份副本），绝不是 <c>sharedMaterial</c>。
        /// </summary>
        public static void ApplyTo(Material material, Texture2D texture, float goldness, bool hasCustomSkin)
        {
#if ANDROID
            // ProceduralMaterial 弃用告警在 Android 上无法避免（见 ReadGoldness 的说明），压掉。
#pragma warning disable 0618
            var procedural = material as ProceduralMaterial;
            if (procedural != null)
            {
                if (hasCustomSkin)
                {
                    // 自定义皮肤：压上贴图就完事。安卓上金度是烘进贴图的，皮肤一压上就看不见了，
                    // 所以这里连金度都不设——设了也没有任何可见效果。
                    if (texture != null)
                    {
                        procedural.mainTexture = texture;
                    }
                    return;
                }

                // 没有自定义皮肤：原版罐子由 substance 按金度现生成。
                //
                // 关键一步是把槽位指回 substance 自己的 basecolor 输出。重烘改的是生成贴图的像素，
                // 不会把结果重新绑回 _MainTex；槽位一旦被任何静态贴图（比如内嵌的原版黑罐 PNG）
                // 占着，金色烘出来也没人显示——这正是金罐在安卓上「不生效」的原因。
                // 实测输出顺序是 basecolor / normal / metallic，_MainTex 用的就是第一张
                // （本地原件 mainIsGenerated=True 且等于 generated[0]）。
                var generated = procedural.GetGeneratedTextures();
                if (generated != null && generated.Length > 0)
                {
                    procedural.mainTexture = generated[0];
                }

                // 金度没变就不必再烘：重烘是让 Substance 把生成器重跑一遍，不便宜。
                // 槽位上面已经无条件指回生成贴图了，跳过重烘只是省掉这次重跑。
                var current = procedural.GetProceduralFloat(GameConstants.GoldnessProceduralInput);
                if (Mathf.Abs(current - goldness) > GoldnessEpsilon)
                {
                    procedural.SetProceduralFloat(GameConstants.GoldnessProceduralInput, goldness);
                    // 异步重烘：排进 Substance 帧末队列，不阻塞主线程。
                    procedural.RebuildTextures();
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

        /// <summary>
        /// 诊断用：把这个材质「金度是怎么落的」摊成一行文本，只读不改。
        /// 用来在真机上回答三件事：远端那份材质是不是 ProceduralMaterial、<c>_MainTex</c>
        /// 现在指向的是 substance 生成的贴图还是我们写进去的静态图、以及生成的贴图
        /// 是不是和别的材质共用同一个对象（比对文本里的 #实例ID 即可）。
        /// 整段包在 try 里：探针本身绝不能影响玩法。
        /// </summary>
        public static string Describe(Material material)
        {
            if (material == null)
            {
                return "material=null";
            }

            try
            {
                var text = new StringBuilder();
                text.Append("mat=").Append(material.name);
                text.Append("#").Append(material.GetInstanceID());
                text.Append("[").Append(material.GetType().Name).Append("]");

#if ANDROID
#pragma warning disable 0618
                var procedural = material as ProceduralMaterial;
                if (procedural == null)
                {
                    text.Append(" procedural=no");
                    text.Append(" mainTex=").Append(DescribeTexture(material.mainTexture));
                    return text.ToString();
                }

                text.Append(" procedural=yes");
                text.Append(" mainTex=").Append(DescribeTexture(procedural.mainTexture));

                var generated = procedural.GetGeneratedTextures();
                text.Append(" generated=");
                if (generated == null)
                {
                    text.Append("null");
                }
                else
                {
                    text.Append(generated.Length).Append("[");
                    for (var i = 0; i < generated.Length; i++)
                    {
                        if (i > 0) text.Append(",");
                        text.Append(DescribeTexture(generated[i]));
                    }
                    text.Append("]");
                    text.Append(" mainIsGenerated=").Append(
                        IndexOfGenerated(generated, procedural.mainTexture) >= 0);
                }
#pragma warning restore 0618
#else
                text.Append(" hasGoldnessProp=").Append(material.HasProperty(GameConstants.GoldnessProperty));
                text.Append(" mainTex=").Append(DescribeTexture(material.mainTexture));
#endif
                text.Append(" goldness=").Append(ReadGoldness(material));
                return text.ToString();
            }
            catch (Exception ex)
            {
                return "describe-failed: " + ex.Message;
            }
        }

        private static string DescribeTexture(Texture texture)
        {
            if (texture == null)
            {
                return "null";
            }
            return texture.name + "#" + texture.GetInstanceID() + "[" + texture.GetType().Name + "]";
        }

#if ANDROID
        private static int IndexOfGenerated(Texture[] generated, Texture candidate)
        {
            if (generated == null || candidate == null)
            {
                return -1;
            }
            for (var i = 0; i < generated.Length; i++)
            {
                if (ReferenceEquals(generated[i], candidate))
                {
                    return i;
                }
            }
            return -1;
        }
#endif
    }
}
