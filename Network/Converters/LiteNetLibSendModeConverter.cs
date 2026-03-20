using GOILauncher.Multiplayer.Network.Enums;
using LiteNetLib;
using System;

namespace GOILauncher.Multiplayer.Network.Converters
{
    internal class LiteNetLibSendModeConverter : IConverter<SendMode, DeliveryMethod>
    {
        DeliveryMethod IConverter<SendMode, DeliveryMethod>.Convert(SendMode source)
        {
            DeliveryMethod deliveryMethod;
            switch (source)
            {
                case SendMode.ReliableOrdered:
                    deliveryMethod = DeliveryMethod.ReliableOrdered;
                    break;
                case SendMode.Unreliable:
                    deliveryMethod = DeliveryMethod.Unreliable;
                    break;
                case SendMode.ReliableUnordered:
                    deliveryMethod = DeliveryMethod.ReliableUnordered;
                    break;
                case SendMode.Sequenced:
                    deliveryMethod = DeliveryMethod.Sequenced;
                    break;
                case SendMode.ReliableSequenced:
                    deliveryMethod = DeliveryMethod.ReliableSequenced;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, null);
            }
            return deliveryMethod;
        }
    }
}
