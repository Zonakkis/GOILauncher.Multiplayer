using System;
using System.Reflection;
using UnityEngine;

namespace GOILauncher.Multiplayer.Unity.Helpers
{
    public static class Physics2DHelper
    {
        public static void SetAutoSimulation(bool autoSimulation)
        {
#if WINDOWS
                Physics2D.simulationMode = autoSimulation ? SimulationMode.Script : SimulationMode.FixedUpdate;
#elif ANDROID
                Physics2D.autoSimulation = autoSimulation;
#endif
        }
    }
}