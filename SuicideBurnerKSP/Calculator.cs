using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using KSP;
using VehiclePhysics;
using UnityEngine.UI;
using KSP.UI.Screens;

namespace SuicideBurnerKSP
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class Calculator : MonoBehaviour
    {
        Vector3d vesselSpeed;
        Vector3 impactPoint;
        Vector3 shipPosition;
        Trajectory trajectory = new Trajectory();
        float shipAngleDiff;
        float altitude;
        float apoapsis;
        Vessel vessel = new Vessel();
        TrajectoryLibrary.Patch trajectoryPatch = new TrajectoryLibrary.Patch();
        private Rect windowPosition = new Rect();

        /*
        public override void OnStart(StartState state)
        {
            if(state != StartState.Editor)
            {
                
            }
        }
        */
        void Update()
        {

            vesselSpeed = FlightGlobals.ship_velocity;
            shipPosition = FlightGlobals.ship_position;
            altitude = (float)FlightGlobals.ship_altitude;

            try
            {
                //trajectoryPatch.
            }
            catch
            {

            }

            //apoapsis = (float)trajectory.end
            //impactPoint = trajectoryPatch.ImpactPosition;
            shipAngleDiff = Vector3.Dot(FlightGlobals.upAxis, (impactPoint - shipPosition).normalized);
            

        }

        /*
        public static Vector3 GetImpactPoint()
        {
            if (FlightGlobals.ActiveVessel != null)
            {
                foreach (Trajectory.Patch patch in Trajectory.fetch.Patches)
                {
                    if (patch.ImpactPosition != null)
                    {
                        return (Vector3)patch.ImpactPosition;
                    }
                }
            }
            return new Vector3(0,0,0);
        }
        public static double GetImpactTime()
        {
            if(FlightGlobals.ActiveVessel != null)
            {
                foreach (Trajectories.Trajectory.Patch patch in Trajectories.Trajectory.fetch.Patches)
                {
                    if(patch.ImpactPosition != null)
                    {
                        return patch.EndTime;
                    }
                }
            }
            return (0.000000000000000000000001d);
        }
        */
    }
}
