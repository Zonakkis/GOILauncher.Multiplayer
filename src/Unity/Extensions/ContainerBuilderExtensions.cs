using System;
using Autofac;
using Autofac.Builder;
using UnityEngine;

namespace GOILauncher.Multiplayer.Unity.Extensions
{
    public static class ContainerBuilderExtensions
    {
        public static IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle> RegisterComponent<T>(this ContainerBuilder builder, Action<T> action) where T : MonoBehaviour
        {
            return builder.Register(ctx =>
            {
                var obj = new GameObject(typeof(T).Name);
                var component = obj.AddComponent<T>();
                ctx.InjectProperties(component);
                action(component);
                return component;
            });
        }
    }
}
