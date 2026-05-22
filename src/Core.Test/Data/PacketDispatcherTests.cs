using FluentAssertions;
using GOILauncher.Multiplayer.Core.Data;
using LiteNetLib.Utils;
using NUnit.Framework;
using System.Reflection;

namespace GOILauncher.Multiplayer.Core.Test.Data
{
    [TestFixture]
    public class PacketDispatcherTests
    {
        private NetPacketProcessor _processor;
        private PacketDispatcher _dispatcher;
        private NetDataWriter _writer;

        [SetUp]
        public void Setup()
        {
            _processor = new NetPacketProcessor();
            _dispatcher = new PacketDispatcher(_processor);
            _writer = new NetDataWriter();
        }

        [Test]
        public void RegisterStruct_And_Dispatch_CallsAction_WithCorrectData()
        {
            var called = false;
            TestStructPacket receivedPacket = default;

            _dispatcher.RegisterStruct<TestStructPacket>((packet, peer) =>
            {
                called = true;
                receivedPacket = packet;
            });

            var sendPacket = new TestStructPacket { Value = 42 };
            _processor.WriteNetSerializable(_writer, sendPacket);

            var reader = new NetDataReader(_writer.CopyData());
            _dispatcher.Dispatch(null, reader);

            called.Should().BeTrue();
            receivedPacket.Value.Should().Be(42);
        }

        [Test]
        public void RuntimeNetSerializableWriter_WithInterfaceTypedPacket_WritesConcretePacketId()
        {
            var called = false;
            TestStructPacket receivedPacket = default;

            _dispatcher.RegisterStruct<TestStructPacket>((packet, peer) =>
            {
                called = true;
                receivedPacket = packet;
            });

            INetSerializable sendPacket = new TestStructPacket { Value = 42 };
            var bytes = WriteNetSerializableWithRuntimeType(sendPacket);

            var reader = new NetDataReader(bytes);
            _dispatcher.Dispatch(null, reader);

            called.Should().BeTrue();
            receivedPacket.Value.Should().Be(42);
        }

        [Test]
        public void RegisterClass_And_Dispatch_CallsAction_WithCorrectData()
        {
            var called = false;
            TestClassPacket receivedPacket = null;

            _dispatcher.RegisterClass<TestClassPacket>((packet, peer) =>
            {
                called = true;
                receivedPacket = packet;
            });

            var sendPacket = new TestClassPacket { Message = "Hello World" };
            _processor.Write(_writer, sendPacket);

            var reader = new NetDataReader(_writer.CopyData());
            _dispatcher.Dispatch(null, reader);

            called.Should().BeTrue();
            receivedPacket.Should().NotBeNull();
            receivedPacket.Message.Should().Be("Hello World");
        }

        public struct TestStructPacket : INetSerializable
        {
            public int Value { get; set; }

            public void Serialize(NetDataWriter writer)
            {
                writer.Put(Value);
            }

            public void Deserialize(NetDataReader reader)
            {
                Value = reader.GetInt();
            }
        }

        public class TestClassPacket 
        {
            public string Message { get; set; }
        }

        private static byte[] WriteNetSerializableWithRuntimeType(INetSerializable packet)
        {
            var writerType = typeof(PacketDispatcher).Assembly.GetType("GOILauncher.Multiplayer.Network.NetSerializablePacketWriter");
            var write = writerType.GetMethod("Write", BindingFlags.Public | BindingFlags.Static);
            return (byte[])write.Invoke(null, new object[] { packet });
        }
    }
}
