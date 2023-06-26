using KspWalkAbout.Entities;
using NUnit.Framework;

namespace WalkAboutUnitTesting
{
    [TestFixture]
    public class GreatCircleUnitTests
    {
        private WorldCoordinates _JohnoGroats;
        private WorldCoordinates _landsEnd;
        private WorldCoordinates _KscFlagpole;
        private WorldCoordinates _VAB;

        [OneTimeSetUp]
        public void BeforeAnyTesting()
        {
            _landsEnd = new WorldCoordinates(50.066388888888889d, -5.7147222222222222d, 0, 6371000);
            _JohnoGroats = new WorldCoordinates(58.643888888888889d, -3.07, 0, 6371000);
            _KscFlagpole = new WorldCoordinates(-0.094169025159008, -74.6535078892799, 64.3840068761492, 600000);
            _VAB = new WorldCoordinates(-0.0967767169198357, -74.6194886953435, 67.5464817753527, 600000);
        }

        [Test]
        public void RecordsOrigin()
        {
            // Arrange
            var orig = _landsEnd;
            var dest = _JohnoGroats;

            // Act
            var route = new GreatCircle(orig, dest);

            // Assert
            Assert.That(
                orig.Latitude,
                Is.EqualTo(_landsEnd.Latitude).Within(0.0001),
                "Recorded origin's latitude did not match supplied values");
            Assert.That(
                orig.Longitude,
                Is.EqualTo(_landsEnd.Longitude).Within(0.0001),
                "Recorded origin's longitude did not match supplied values");
            Assert.That(
                orig.Altitude,
                Is.EqualTo(_landsEnd.Altitude).Within(0.0001),
                "Recorded origin's altitude did not match supplied values");
            Assert.That(
                orig.WorldRadius,
                Is.EqualTo(_landsEnd.WorldRadius).Within(0.0001),
                "Recorded origin's radius did not match supplied values");
        }

        [Test]
        public void RecordsDestination()
        {
            // Arrange
            var orig = _landsEnd;
            var dest = _JohnoGroats;

            // Act
            var route = new GreatCircle(orig, dest);

            // Assert
            Assert.That(
                dest.Latitude,
                Is.EqualTo(_JohnoGroats.Latitude).Within(0.0001),
                "Recorded destination's latitude did not match supplied values");
            Assert.That(
                dest.Longitude,
                Is.EqualTo(_JohnoGroats.Longitude).Within(0.0001),
                "Recorded destination's longitude did not match supplied values");
            Assert.That(
                dest.Altitude,
                Is.EqualTo(_JohnoGroats.Altitude).Within(0.0001),
                "Recorded destination's altitude did not match supplied values");
            Assert.That(
                dest.WorldRadius,
                Is.EqualTo(_JohnoGroats.WorldRadius).Within(0.0001),
                "Recorded destination's radius did not match supplied values");
        }

        [Test]
        public void CalculatesGreatCircle_NoDeltaAlt()
        {
            // Arrange
            var orig = _landsEnd;
            var dest = _JohnoGroats;
            var expectedDistance = 968853.54671313858d;
            var expectedIntialBearing = 9.1198181045040769d;

            // Act
            var route = new GreatCircle(orig, dest);

            // Assert
            Assert.That(
                route.DeltaASL,
                Is.EqualTo(0).Within(0.0001),
                "Miscalculated altitude difference");
            Assert.That(
                route.DistanceAtDestAlt,
                Is.EqualTo(expectedDistance).Within(0.0001),
                "Miscalculated distance at destination's altitude");
            Assert.That(
                route.DistanceAtOrigAlt,
                Is.EqualTo(expectedDistance).Within(0.0001),
                "Miscalculated distance at origin's altitude");
            Assert.That(
                route.DistanceAtSeaLevel,
                Is.EqualTo(expectedDistance).Within(0.0001),
                "Miscalculated distance at sea level");
            Assert.That(
                route.DistanceWithAltChange,
                Is.EqualTo(expectedDistance).Within(0.0001),
                "Miscalculated distance including altitude difference");
            Assert.That(
                route.ForwardAzimuth,
                Is.EqualTo(expectedIntialBearing).Within(0.0001),
                "Miscalulated initial bearing");
        }

        [Test]
        public void CalculatesGreatCircle_Kerbin()
        {
            // Arrange
            var orig = _KscFlagpole;
            var dest = _VAB;
            var expectedDistanceAtDestinationAltitude = 357.3329782527984d;
            var expectedDistanceAtOriginAltitude = 357.33109503718202d;
            var expectedDistanceAtSeaLeval = 357.29275513850934d;
            var expectedDistanceWithAltitudeChange = 357.33203664499024d;
            var expectedIntialBearing = 94.383387027330514d;
            var expectedAltitudeChange = 3.1624748992035023d;

            // Act
            var route = new GreatCircle(orig, dest);

            // Assert
            Assert.That(
                route.DeltaASL,
                Is.EqualTo(expectedAltitudeChange).Within(0.0001),
                "Miscalculated altitude difference");
            Assert.That(
                route.DistanceAtDestAlt,
                Is.EqualTo(expectedDistanceAtDestinationAltitude).Within(0.0001),
                "Miscalculated distance at destination's altitude");
            Assert.That(
                route.DistanceAtOrigAlt,
                Is.EqualTo(expectedDistanceAtOriginAltitude).Within(0.0001),
                "Miscalculated distance at origin's altitude");
            Assert.That(
                route.DistanceAtSeaLevel,
                Is.EqualTo(expectedDistanceAtSeaLeval).Within(0.0001),
                "Miscalculated distance at sea level");
            Assert.That(
                route.DistanceWithAltChange,
                Is.EqualTo(expectedDistanceWithAltitudeChange).Within(0.0001),
                "Miscalculated distance including altitude difference");
            Assert.That(
                route.ForwardAzimuth,
                Is.EqualTo(expectedIntialBearing).Within(0.0001),
                "Miscalulated initial bearing");
        }

        [Test]
        public void GreatCircle_Travel_to_WorldCoordinate()
        {
            // Arrange
            var orig = _KscFlagpole;
            var dest = _VAB;

            // Act
            var route = new GreatCircle(orig, dest);
            var endpoint = orig.Travel(route);

            // Assert
            Assert.That(
                endpoint.Latitude,
                Is.EqualTo(dest.Latitude).Within(0.0001),
                "Unacceptable variance in calculated endpoint latitude");
            Assert.That(
                endpoint.Longitude,
                Is.EqualTo(dest.Longitude).Within(0.0001),
                "Unacceptable variance in calculated endpoint longitude");
            Assert.That(
                endpoint.Altitude,
                Is.EqualTo(dest.Altitude).Within(0.0001),
                "Unacceptable variance in calculated endpoint altitude");
        }
    }
}
