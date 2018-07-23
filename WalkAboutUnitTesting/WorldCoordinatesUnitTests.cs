using KspWalkAbout.Entities;
using NUnit.Framework;

namespace WalkAboutUnitTesting
{
    [TestFixture]
    class WorldCoordinatesUnitTests
    {
        private WorldCoordinates _JohnoGroats;
        private WorldCoordinates _landsEnd;

        [OneTimeSetUp]
        public void BeforeAnyTesting()
        {
            _landsEnd = new WorldCoordinates(50.066388888888889d, -5.7147222222222222d, 0, 6371000);
            _JohnoGroats = new WorldCoordinates(58.643888888888889d, -3.07, 0, 6371000);
        }

        [Test]
        public void CalculatesNewCoordinates_NoDeltaAlt()
        {
            // Arrange
            var orig = _landsEnd;
            var route = new GreatCircle(_landsEnd, _JohnoGroats);

            // Act
            var dest = orig.Travel(route);

            // Assert
            Assert.That(dest.Latitude, Is.EqualTo(_JohnoGroats.Latitude).Within(0.0001), "Miscalculation of final latitude");
            Assert.That(dest.Longitude, Is.EqualTo(_JohnoGroats.Longitude).Within(0.0001), "Miscalculation of final longitude");
            Assert.That(dest.Altitude, Is.EqualTo(_JohnoGroats.Altitude).Within(0.0001), "Miscalculation of final altitude");
        }
    }
}
