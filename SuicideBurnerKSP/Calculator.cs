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
/*
using KRPC.Client;
using KRPC.Client.Services.SpaceCenter;
*/

namespace SuicideBurnerKSP
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class Calculator : MonoBehaviour
    {
        bool DEBUG_LINES = true;
        int DEBUG_LINE_LENGTH = 25;
        float DEBUG_LINE_DESTROY_TIMESTEP = 0.04f;

        Vector3d vesselVelocity;
        Vector3 shipPosition;
        Trajectory trajectory = new Trajectory();
        float altitude;
        Vessel vessel = new Vessel();
        bool activated = false;
        Vector3 SurfaceNormal;
        Vector3 ShipUp;
        Vessel thisVessel;
        ModuleEngines engines;
        //ModuleEngines enginesModules;
        Vector2 HorizontalSpeed;

        bool debug = false;

        double acceleration = 0.0d;
        float maxDeceleration = 0.0f;
        float stopDist = 0.0f;
        float burnStart = 0.0f;

        float burnTolerance = 7.5f;
        float thrust = 0.0f;

        int printTickInterval = 50;
        int tick = 0;

        float landingCorrectionLaziness = 0.5f;
        float lindingCorrectionAccuracy = 1.0f; // in angles, degrees

        private void Start()
        {
            print("-  -  -  -  - Suicide Burner V: [0.2.3]");
        }

        void Update()
        {

            //////
            thisVessel = FlightGlobals.ActiveVessel;
            //////

            Vector3 DesiredDebugPosition = Vector3.Reflect(thisVessel.srf_velocity.normalized, -SurfaceNormal);

            if (DEBUG_LINES)
            {
                DrawLine(thisVessel.transform.position, thisVessel.transform.up.normalized * DEBUG_LINE_LENGTH + thisVessel.transform.position, new Color(255.0f, 0.0f, 0.0f), DEBUG_LINE_DESTROY_TIMESTEP);
                DrawLine(thisVessel.transform.position, thisVessel.velocityD.normalized * DEBUG_LINE_LENGTH + thisVessel.transform.position, new Color(0.0f, 0.0f, 255.0f), DEBUG_LINE_DESTROY_TIMESTEP);
                DrawLine(thisVessel.transform.position, DesiredDebugPosition.normalized * DEBUG_LINE_LENGTH + thisVessel.transform.position, new Color(0.0f, 255.0f, 0.0f), DEBUG_LINE_DESTROY_TIMESTEP);
                DrawLine(thisVessel.transform.position, SurfaceNormal * DEBUG_LINE_LENGTH + thisVessel.transform.position, new Color(50.0f, 50.0f, 50.0f), DEBUG_LINE_DESTROY_TIMESTEP);
                DrawLine(thisVessel.transform.position, new Vector3(0, 1.0f, 0) * DEBUG_LINE_LENGTH + thisVessel.transform.position, new Color(50.0f, 50.0f, 50.0f), DEBUG_LINE_DESTROY_TIMESTEP);
                DrawLine(thisVessel.transform.position, (thisVessel.transform.up.normalized-DesiredDebugPosition.normalized).normalized * DEBUG_LINE_LENGTH + thisVessel.transform.position, new Color(255.0f, 255.0f, 0.0f), DEBUG_LINE_DESTROY_TIMESTEP);
            }

            HorizontalSpeed = new Vector2((float)vesselVelocity.x, (float)vesselVelocity.y);

            engines = thisVessel.FindPartModuleImplementing<ModuleEngines>();

            #region Calculate thrust Atmospheric/Vaccum
            if (thisVessel.atmDensity == 0)
            {
                acceleration = (engines.MaxThrustOutputVac(true))/(double)(thisVessel.totalMass);
            }
            else
            {
                acceleration = (engines.MaxThrustOutputAtm(false, true, 1, thisVessel.atmosphericTemperature, thisVessel.atmDensity))/thisVessel.totalMass;
            }
            #endregion
            
            vesselVelocity = thisVessel.velocityD;
            shipPosition = thisVessel.transform.position;
            altitude = (float)thisVessel.altitude;
            SurfaceNormal = thisVessel.mainBody.GetSurfaceNVector(thisVessel.latitude, thisVessel.longitude);
            ShipUp = thisVessel.transform.up;

            #region Keyboard shortcuts

            if (Input.GetKeyDown(KeyCode.P) && activated == true)
            {
                activated = false;
                thisVessel.OnFlyByWire -= EngineControlThread;
                thisVessel.OnFlyByWire -= landingVectorControl;
                print("-------------------- Suicide Burn Script Deactivated");
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                activated = true;
                thisVessel.OnFlyByWire += EngineControlThread;
                thisVessel.OnFlyByWire += landingVectorControl;
                print("-------------------- Suicide Burn Script Activated");
            }

            if(Input.GetKeyDown(KeyCode.L))
            {
                Quaternion angle = Quaternion.Euler(0.0f, 45.0f, 0.0f);
                thisVessel.SetRotation(angle);
            }

            if (Input.GetKeyDown(KeyCode.J) && debug == true)
            {
                debug = false;
            }
            else if(Input.GetKeyDown(KeyCode.J) && debug != true)
            {
                debug = true;
            }

            #endregion

            ////////calculating the place where we need to start burning

            maxDeceleration = (float)(acceleration - thisVessel.graviticAcceleration.magnitude);
            stopDist = (float)Math.Round(((thisVessel.speed * thisVessel.speed) / (2 * maxDeceleration)), 4);
            burnStart = (float)Math.Round((stopDist + (altitude - thisVessel.heightFromTerrain)), 4) + (thisVessel.transform.localScale.y)*3 + burnTolerance;

            if(altitude <= burnStart && stopDist >= burnTolerance/2)// && thisVessel.velocityD.y < 0)//(altitude + burnTolerance >= burnStart && altitude - burnTolerance <= burnStart || altitude < burnStart && thisVessel.velocityD.y < 0)
            {
                thrust = 1.0f;
                //print("-------------------- Thrusting");
            }
            else
            {
                thrust = 0.0f;
                //print("-------------------- Thrust = 0000");
            }
            tick++;
            if(tick >= printTickInterval)
            {
                tick = 0;
                if (debug)                             ////////////////////          DEBUG   CONSOLE   OUTPUT          ////////////////////
                {
                    print("-------------------- burnStart:" + burnStart.ToString());
                    print("-------------------- stopDist:" + stopDist.ToString());
                    print("-------------------- Altitude:" + altitude.ToString());
                    print("-------------------- Thrust:" + thrust.ToString());
                    print("-------------------- ShipAcceleration:" + thisVessel.acceleration.magnitude.ToString());
                    print("-------------------- ShipAccelerationNormalized:" + thisVessel.acceleration.normalized.ToString());
                    print("- - - - - - - - - -  YAW" + FlightGlobals.ship_rotation.eulerAngles.z.ToString());
                    print("- - - - - - - - - -  PITCH" + FlightGlobals.ship_rotation.eulerAngles.y.ToString());
                    print("- - - - - - - - - -  ROLL" + FlightGlobals.ship_rotation.eulerAngles.x.ToString());
                    print("- - - - - - - - - -  Normal" + SurfaceNormal.ToString());
                    print("- - - - - - - - - -  Up" + ShipUp.ToString());
                    print("- - - - - - - - - -  Angle" + Vector3.Dot(SurfaceNormal, ShipUp));
                    print("- - - - - - - - - -  Gravity" + thisVessel.graviticAcceleration.magnitude);
                    print("-------------------- ShipVelocity:" + thisVessel.velocityD.normalized);
                    print("-------------------- Max Acceleration:" + acceleration);//First().maxFuelFlow);
                    print("-------------------- Total Mass:" + thisVessel.totalMass);
                }
                /*
                for (int i = 0; i < thisVessel.FindPartModulesImplementing<ModuleEngines>().Count; i++)
                {
                    print("-------------------- Active Engines" + engines[i]);
                }
                */
            }
        }

        //engine and tilt controllers

        void EngineControlThread(FlightCtrlState state)
        {
            state.mainThrottle = thrust;
        }

        void landingVectorControl(FlightCtrlState state)
        {
            Vector3d velocity = thisVessel.srf_velocity.normalized;

            Vector3 DesiredPosition = Vector3.Reflect(velocity, -SurfaceNormal);

            if (Vector3.Angle(thisVessel.transform.up.normalized, DesiredPosition.normalized) <= lindingCorrectionAccuracy)
            {
                state.pitch = ((DesiredPosition.normalized - thisVessel.transform.up.normalized).normalized.x) * landingCorrectionLaziness;
                state.yaw = ((DesiredPosition.normalized - thisVessel.transform.up.normalized).normalized.y) * landingCorrectionLaziness;
                //state.roll = (float)(Math.Atan2(vesselVelocity.z, vesselVelocity.y) - Math.Atan2(DesiredPosition.z, DesiredPosition.y)) * landingCorrectionLaziness;
                
            }

            /*
            state.pitch = (float)(Math.Atan2(vesselVelocity.z, vesselVelocity.x) - Math.Atan2(DesiredPosition.z, DesiredPosition.x)) * landingCorrectionLaziness;
            state.yaw = (float)(Math.Atan2(vesselVelocity.y, vesselVelocity.x) - Math.Atan2(DesiredPosition.y, DesiredPosition.x)) * landingCorrectionLaziness;
            state.roll = (float)(Math.Atan2(vesselVelocity.z, vesselVelocity.y) - Math.Atan2(DesiredPosition.z, DesiredPosition.y)) * landingCorrectionLaziness;
            */

            //state.pitch = (float)(velocity.y)*landingCorrectionLaziness;
            //state.yaw = (float)(-velocity.z)* landingCorrectionLaziness;
        }

        private void OnDestroy()
        {
            thrust = 0.0f;
            thisVessel.OnFlyByWire -= EngineControlThread;
            thisVessel.OnFlyByWire -= landingVectorControl;
        }

        void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.2f)
        {
            GameObject myLine = new GameObject();
            myLine.transform.position = start;
            myLine.AddComponent<LineRenderer>();
            LineRenderer lr = myLine.GetComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
            lr.startColor = color;
            lr.startWidth = 0.1f;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            GameObject.Destroy(myLine, duration);
        }
    }
}
