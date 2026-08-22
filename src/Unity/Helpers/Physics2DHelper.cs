using System;
using System.Reflection;
using UnityEngine;

namespace GOILauncher.Multiplayer.Unity.Helpers
{
    public static class Physics2DHelper
    {
        private static readonly PropertyInfo _simulationModeProperty;
        private static readonly object _simulationModeScript;
        private static readonly object _simulationModeFixedUpdate;
        private static readonly PropertyInfo _autoSimulationProperty;
        static Physics2DHelper()
        {
            _simulationModeProperty = typeof(Physics2D).GetProperty("simulationMode");
            _autoSimulationProperty = typeof(Physics2D).GetProperty("autoSimulation");
            if (_simulationModeProperty != null)
            {
                _simulationModeScript = Enum.Parse(_simulationModeProperty.PropertyType, "Script");
                _simulationModeFixedUpdate = Enum.Parse(_simulationModeProperty.PropertyType, "FixedUpdate");
            }
        }

        public static void SetAutoSimulation(bool autoSimulation)
        {
            if (autoSimulation)
            {
                _autoSimulationProperty?.SetValue(null, true, null);
                _simulationModeProperty?.SetValue(null, _simulationModeFixedUpdate, null);
            }
            else
            {
                _autoSimulationProperty?.SetValue(null, false, null);
                _simulationModeProperty?.SetValue(null, _simulationModeScript, null);
            }
        }
    }
}