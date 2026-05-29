using System;
using System.Collections.Generic;
using FluentAssertions;
using GOILauncher.Multiplayer.Core.Event;
using GOILauncher.Multiplayer.Core.Log;
using Moq;
using NUnit.Framework;

namespace GOILauncher.Multiplayer.Core.Test.Event
{
    [TestFixture]
    public class EventBusTests
    {
        private Mock<ILogger<EventBus>> _loggerMock;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<EventBus>>();
        }

        [Test]
        public void Publish_WithSubscribedHandler_InvokesHandler()
        {
            var bus = new EventBus(_loggerMock.Object);
            var called = false;
            var capturedValue = 0;

            bus.Subscribe<TestEvent>(e =>
            {
                called = true;
                capturedValue = e.Value;
            });

            bus.Publish(new TestEvent(42));

            called.Should().BeTrue();
            capturedValue.Should().Be(42);
        }

        [Test]
        public void Publish_WithMultipleSubscribers_InvokesAllHandlers()
        {
            var bus = new EventBus(_loggerMock.Object);
            var calls = 0;
            var receivedValues = new List<int>();

            bus.Subscribe<TestEvent>(e =>
            {
                calls++;
                receivedValues.Add(e.Value);
            });

            bus.Subscribe<TestEvent>(e =>
            {
                calls++;
                receivedValues.Add(e.Value + 1);
            });

            bus.Publish(new TestEvent(10));

            calls.Should().Be(2);
            receivedValues.Should().BeEquivalentTo(new[] { 10, 11 });
        }

        [Test]
        public void Publish_WithDifferentEventType_DoesNotInvokeOtherTypeHandlers()
        {
            var bus = new EventBus(_loggerMock.Object);
            var testEventCalls = 0;
            var anotherEventCalls = 0;

            bus.Subscribe<TestEvent>(e => testEventCalls++);
            bus.Subscribe<AnotherEvent>(e => anotherEventCalls++);

            bus.Publish(new TestEvent(1));

            testEventCalls.Should().Be(1);
            anotherEventCalls.Should().Be(0);
        }

        [Test]
        public void Dispose_Subscription_RemovesHandler()
        {
            var bus = new EventBus(_loggerMock.Object);
            var calls = 0;

            IDisposable subscription = bus.Subscribe<TestEvent>(e => calls++);

            bus.Publish(new TestEvent(1));
            subscription.Dispose();
            bus.Publish(new TestEvent(2));
            subscription.Dispose();
            bus.Publish(new TestEvent(3));

            calls.Should().Be(1);
        }

        [Test]
        public void Dispose_Subscription_WhenHandlerSubscribedTwice_RemovesOnlyThatSubscription()
        {
            var bus = new EventBus(_loggerMock.Object);
            var calls = 0;
            Action<TestEvent> handler = e => calls++;

            IDisposable firstSubscription = bus.Subscribe(handler);
            bus.Subscribe(handler);

            firstSubscription.Dispose();
            bus.Publish(new TestEvent(1));

            calls.Should().Be(1);
        }

        [Test]
        public void Publish_WhenHandlerSubscribesDuringPublish_NewHandlerRunsOnNextPublishOnly()
        {
            var bus = new EventBus(_loggerMock.Object);
            var firstHandlerCalls = 0;
            var secondHandlerCalls = 0;

            bus.Subscribe<TestEvent>(e =>
            {
                firstHandlerCalls++;
                bus.Subscribe<TestEvent>(x => secondHandlerCalls++);
            });

            bus.Publish(new TestEvent(5));
            bus.Publish(new TestEvent(6));

            firstHandlerCalls.Should().Be(2);
            secondHandlerCalls.Should().Be(1);
        }

        [Test]
        public void Publish_HandlerThrowsException_DoesNotBlockSubsequentHandlers()
        {
            // Verify: Even if one subscriber throws an exception, 
            // the EventBus should catch it and allow subsequent subscribers to execute safely
            var bus = new EventBus(_loggerMock.Object);
            var secondHandlerCalled = false;
            var testException = new InvalidOperationException("Test Error");

            bus.Subscribe<TestEvent>(e => throw testException);
            bus.Subscribe<TestEvent>(e => secondHandlerCalled = true);

            // The Publish method should catch the exception thrown by the first handler and not propagate it, allowing the second handler to execute
            Action act = () => bus.Publish(new TestEvent(1));
            act.Should().NotThrow();
            
            // The second handler should be called even though the first one throws an exception
            secondHandlerCalled.Should().BeTrue("执行不应该因为第一个抛出异常而被阻止");

            // Verify that the logger's Error method was called with a message containing "TestEvent" and the test exception
            _loggerMock.Verify(l => l.Error(testException, It.Is<string>(msg => msg.Contains("TestEvent"))), Times.Once);
        }

        private class TestEvent
        {
            public int Value { get; private set; }

            public TestEvent(int value)
            {
                Value = value;
            }
        }

        private class AnotherEvent
        {
            public string Name { get; private set; }

            public AnotherEvent(string name)
            {
                Name = name;
            }
        }
    }
}
