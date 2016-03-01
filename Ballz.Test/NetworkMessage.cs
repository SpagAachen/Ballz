using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.Test
{
    using global::Ballz.GameSession.Logic;
    using global::Ballz.Messages;

    using Newtonsoft.Json;

    using NUnit.Framework;

    [TestFixture]
    class NetworkMessageTests
    {
        // Arrange
        public NetworkMessage CreateTestObject()
        {
            return new NetworkMessage(NetworkMessage.MessageType.StartGame) { Data = new Player {Id=1337, Name = "SomeName", TeamName = "SomeTeamName"} };
        }

        // Assert
        public void ValidateTestObject(NetworkMessage msg)
        {
            Assert.NotNull(msg);
            Assert.NotNull(msg.Data);

            Assert.AreEqual(msg.Kind, NetworkMessage.MessageType.StartGame);
            Assert.AreEqual(msg.Data.GetType(), typeof(Player));

            var testStruct = (Player)msg.Data;
            Assert.AreEqual(testStruct.Id, 1337);
            Assert.AreEqual(testStruct.Name, "SomeName");
            Assert.AreEqual(testStruct.TeamName, "SomeTeamName");
        }

        // Act
        [Test]
        public void CreateDestroy()
        {
            var obj = CreateTestObject();
            ValidateTestObject(obj);
        }

        [Test]
        public void SerializeDeserialize()
        {
            // Arrange
            var obj = this.CreateTestObject();
            // Act
            var json = JsonConvert.SerializeObject(obj);
            var obj2 = JsonConvert.DeserializeObject<NetworkMessage>(json);
            // Assert
            ValidateTestObject(obj2);
        }
    }
}
