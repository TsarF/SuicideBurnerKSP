using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using KSP.Localization;
using UnityEngine;


namespace SuicideBurnerKSP
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class TrajectoryLibrary
    {
        public class VesselState
        {
            public CelestialBody ReferenceBody { get; set; }

            // universal time
            public double Time { get; set; }

            // position in world frame relatively to the reference body
            public Vector3d Position { get; set; }

            // velocity in world frame relatively to the reference body
            public Vector3d Velocity { get; set; }

            // tells wether the patch starting from this state is superimposed on a stock KSP patch, or null if
            // something makes it diverge (atmospheric entry for example)
            public Orbit StockPatch { get; set; }

            public VesselState(Vessel vessel)
            {
                ReferenceBody = vessel.orbit.referenceBody;
                Time = Planetarium.GetUniversalTime();
                Position = vessel.GetWorldPos3D() - ReferenceBody.position;
                Velocity = vessel.obt_velocity;
                StockPatch = vessel.orbit;
            }

            public VesselState()
            {
            }
        }

        public struct Point
        {
            public Vector3 pos;
            public Vector3 aerodynamicForce;
            public Vector3 orbitalVelocity;

            /// <summary>
            /// Ground altitude above (or under) sea level, in meters.
            /// </summary>
            public float groundAltitude;

            /// <summary>
            /// Universal time
            /// </summary>
            public double time;
        }

        public class Patch
        {
            public VesselState StartingState { get; set; }

            public double EndTime { get; set; }

            public bool IsAtmospheric { get; set; }

            // // position array in body space (world frame centered on the body) ; only used when isAtmospheric is true
            public Point[] AtmosphericTrajectory { get; set; }

            // only used when isAtmospheric is false
            public Orbit SpaceOrbit { get; set; }

            public Vector3? ImpactPosition { get; set; }

            public Vector3? RawImpactPosition { get; set; }

            public Vector3? ImpactVelocity { get; set; }

            public Point GetInfo(float altitudeAboveSeaLevel)
            {
                if (!IsAtmospheric)
                    throw new Exception("Trajectory info available only for atmospheric patches");

                if (AtmosphericTrajectory.Length == 1)
                    return AtmosphericTrajectory[0];
                else if (AtmosphericTrajectory.Length == 0)
                    return new Point();

                float absAltitude = (float)StartingState.ReferenceBody.Radius + altitudeAboveSeaLevel;
                float sqMag = absAltitude * absAltitude;

                // TODO: optimize by doing a dichotomic search (this function assumes that altitude variation is monotonic anyway)
                int idx = 1;
                while (idx < AtmosphericTrajectory.Length && AtmosphericTrajectory[idx].pos.sqrMagnitude > sqMag)
                    ++idx;

                float coeff = (absAltitude - AtmosphericTrajectory[idx].pos.magnitude)
                    / Mathf.Max(0.00001f, AtmosphericTrajectory[idx - 1].pos.magnitude - AtmosphericTrajectory[idx].pos.magnitude);
                coeff = Math.Min(1.0f, Math.Max(0.0f, coeff));

                Point res = new Point
                {
                    pos = AtmosphericTrajectory[idx].pos * (1.0f - coeff) + AtmosphericTrajectory[idx - 1].pos * coeff,
                    aerodynamicForce = AtmosphericTrajectory[idx].aerodynamicForce * (1.0f - coeff) +
                                           AtmosphericTrajectory[idx - 1].aerodynamicForce * coeff,
                    orbitalVelocity = AtmosphericTrajectory[idx].orbitalVelocity * (1.0f - coeff) +
                                          AtmosphericTrajectory[idx - 1].orbitalVelocity * coeff,
                    groundAltitude = AtmosphericTrajectory[idx].groundAltitude * (1.0f - coeff) +
                                         AtmosphericTrajectory[idx - 1].groundAltitude * coeff,
                    time = AtmosphericTrajectory[idx].time * (1.0f - coeff) + AtmosphericTrajectory[idx - 1].time * coeff
                };

                return res;
            }
        }
    }
}
