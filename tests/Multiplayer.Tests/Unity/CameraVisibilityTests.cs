using FluentAssertions;
using GOILauncher.Multiplayer.Unity.Models;
using NUnit.Framework;
using UnityEngine;

namespace GOILauncher.Multiplayer.Tests.Unity
{
    [TestFixture]
    public class CameraVisibilityTests
    {
        // orthographicSize 5 与 aspect 2 给出 halfHeight = 5、halfWidth = 10。
        // 相机特意不放在原点，这样"按相机算"和"按世界原点算"的结果不会巧合相同。
        private const float OrthographicSize = 5f;
        private const float Aspect = 2f;
        private static readonly Vector3 CameraPosition = new Vector3(100f, 50f, -10f);

        private static bool IsWithinView(float x, float y)
        {
            return CameraVisibility.IsWithinView(
                CameraPosition, OrthographicSize, Aspect, new Vector3(x, y, 0f));
        }

        [Test]
        public void PointAtCameraCenter_IsVisible()
        {
            IsWithinView(100f, 50f).Should().BeTrue();
        }

        // 左右边界是 halfWidth(10) + PlayerWidth(3.4)，即 86.6 和 113.4。
        [TestCase(113.3f, true)]
        [TestCase(113.5f, false)]
        [TestCase(86.7f, true)]
        [TestCase(86.5f, false)]
        public void HorizontalBound_IsViewportPlusOnePlayerWidth(float x, bool expected)
        {
            IsWithinView(x, 50f).Should().Be(expected);
        }

        // 上边界是 halfHeight(5) + PlayerHeight(3.7) = 58.7；
        // 下边界用的是 PlayerWidth(3.4) 而不是高度，即 41.6。
        [TestCase(58.6f, true)]
        [TestCase(58.8f, false)]
        [TestCase(41.7f, true)]
        [TestCase(41.5f, false)]
        public void VerticalBound_IsViewportPlusTheMarginForThatEdge(float y, bool expected)
        {
            IsWithinView(100f, y).Should().Be(expected);
        }

        /// <summary>
        /// 把上下外扩量不一致这件事钉住：同样超出视野 3.5 米，上边还在、下边已经出去了。
        /// 下边用 PlayerWidth 是改动前的原样行为，是否为笔误尚未确认；真要改成 PlayerHeight，
        /// 这条会先红，而不是让一次顺手修改静悄悄改掉裁剪范围。
        /// </summary>
        [Test]
        public void TopAndBottomMargins_AreNotSymmetric()
        {
            const float beyondViewport = 3.5f;

            IsWithinView(100f, 50f + OrthographicSize + beyondViewport).Should().BeTrue();
            IsWithinView(100f, 50f - OrthographicSize - beyondViewport).Should().BeFalse();
        }

        /// <summary>
        /// 本次 bug 的核心：可见性是相机位置的函数。同一个点在相机移动后结论不同，
        /// 所以这个判断必须每帧重算，不能挂在远端玩家的发包节奏上。
        /// </summary>
        [Test]
        public void Visibility_FollowsTheCameraNotTheTarget()
        {
            var target = new Vector3(100f, 50f, 0f);

            CameraVisibility.IsWithinView(
                    new Vector3(100f, 50f, -10f), OrthographicSize, Aspect, target)
                .Should().BeTrue();
            CameraVisibility.IsWithinView(
                    new Vector3(200f, 50f, -10f), OrthographicSize, Aspect, target)
                .Should().BeFalse();
        }

        [Test]
        public void WiderAspect_ExtendsOnlyTheHorizontalExtent()
        {
            var justOutsideAtAspect2 = new Vector3(120f, 50f, 0f);

            CameraVisibility.IsWithinView(CameraPosition, OrthographicSize, 2f, justOutsideAtAspect2)
                .Should().BeFalse();
            CameraVisibility.IsWithinView(CameraPosition, OrthographicSize, 4f, justOutsideAtAspect2)
                .Should().BeTrue();
        }

        [Test]
        public void DepthIsIgnored()
        {
            CameraVisibility.IsWithinView(
                    CameraPosition, OrthographicSize, Aspect, new Vector3(100f, 50f, 9999f))
                .Should().BeTrue();
        }
    }
}
