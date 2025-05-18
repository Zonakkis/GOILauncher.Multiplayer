using System;
using System.Reflection;
using UnityEngine;

namespace GOILauncher.Multiplayer.Shared.Utilities
{
    public static class Physical2DUtility
    {
        private static readonly PropertyInfo _simulationModeProperty;
        private static readonly object _simulationModecript;
        private static readonly object _simulationModeFixedUpdate;
        private static readonly PropertyInfo _autoSimulationProperty;
        static Physical2DUtility()
        {
            _simulationModeProperty = typeof(Physics2D).GetProperty("simulationMode");
            _autoSimulationProperty = typeof(Physics2D).GetProperty("autoSimulation");
            if (_simulationModeProperty != null)
            {
                _simulationModecript = Enum.Parse(_simulationModeProperty.PropertyType, "Script");
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
                _simulationModeProperty?.SetValue(null, _simulationModecript, null);
            }
        }
    }
}
