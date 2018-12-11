using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using KSP;
using KSP.UI.Screens;//required for ApplicationLauncherButton type

namespace PopulationMod
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]

    public class PopModGUI : MonoBehaviour
    {
        internal static string assetFolder = "PopulationMod/CivilianManagement/Assets/";
        private static ApplicationLauncherButton CivPopButton = null;
        static bool CivPopGUIOn = false;
        internal bool CivPopTooltip = false;
        private GUIStyle _windowstyle, _labelstyle;
        private bool hasInitStyles = false;

        private static ApplicationLauncherButton appButton = null;

        /// <summary>
        /// Awake this instance.  Pre-existing method in Unity that runs before KSP loads.
        /// </summary>
        public void Awake()
        {
            //Debug.Log (debuggingClass.modName + "Starting Awake()");//What I am using to debug
            DontDestroyOnLoad(this);
            GameEvents.onGUIApplicationLauncherReady.Add(OnAppLauncherReady);//when AppLauncher can take apps, give it OnAppLauncherReady (mine)
            GameEvents.onGUIApplicationLauncherDestroyed.Add(OnAppLauncherDestroyed);//Not sure what this does
        }

        public void OnAppLauncherDestroyed()
        {
            if (appButton != null)
            {
                OnToggleFalse();
                ApplicationLauncher.Instance.RemoveApplication(appButton);
            }
        }


        /// <summary>
        /// Raises the app launcher ready event.  Method to create an app button on the AppLauncher, as well as tell
        /// what/how the GUI is loaded.
        /// </summary>
        public void OnAppLauncherReady()
        {
            InitStyle();
            string iconFile = "PopModIcon";//This is the name of the file that stores the mod's icon to be used in the appLauncher
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER && appButton == null)
            {//i.e. if running for the first time
                appButton = ApplicationLauncher.Instance.AddModApplication(
                  OnToggleTrue,                           //Run OnToggleTrue() when user clicks button
                  OnToggleFalse,                          //Run OnToggleFalse() when user clicks button again
                  null, null, null, null,                 //do nothing during hover, exiting, enable/disable
                  ApplicationLauncher.AppScenes.ALWAYS,   //When to show applauncher/GUI button
                  GameDatabase.Instance.GetTexture(assetFolder + iconFile, false));//where to look for mod applauncher icon
                                                                                   //Debug.Log (debuggingClass.modName + "Finishing Awake()");//What I am using to debug
            }
            CivPopGUIOn = false;
        }


        /// <summary>
        /// Presumably what to do when the user opens/clicks the button.  Called from OnAppLauncherReady.
        /// </summary>
        private static void OnToggleTrue()
        {
            //Debug.Log (debuggingClass.modName + "Starting OnToggleTrue()");
            CivPopGUIOn = true;//turns on flag for GUI
                               //Debug.Log (debuggingClass.modName + "Turning on GUI");
        }

        /// <summary>
        /// Presumably what to do when the user closes the button.  Called from OnAppLauncherReady.
        /// </summary>
        private static void OnToggleFalse()
        {
            //Debug.Log (debuggingClass.modName + "Starting OnToggleFalse()");
            CivPopGUIOn = false;//turns off flag for GUI
                                //Debug.Log (debuggingClass.modName + "Turning off GUI");
        }

        /// <summary>
        /// I'm not sure what this is for...but it was already here and it seems to work.
        /// </summary>
        private void InitStyle()
        {
            _windowstyle = new GUIStyle(HighLogic.Skin.window);
            _labelstyle = new GUIStyle(HighLogic.Skin.label);
            hasInitStyles = true;
        }

        /// <summary>
        /// OnGUI() is called by the game and every time it refreshes the GUI.  I just need it to check if the GUI is
        /// enabled and if it is, show it.
        /// </summary>
        public void OnGUI()
        {//Executes code whenever screen refreshes.  Extension to enable use of button along main bar on top-right of screen.
            if (CivPopGUIOn)
            {
                PopulationManagementGUI();//If button is enabled, display rectangle.
            }//end if
        }//end OnGui extension

        /// <summary>
        /// This method controls how the window actually looks when the HUD window is displayed.
        /// </summary>
        void PopulationManagementGUI()
        {
            GUI.BeginGroup(new Rect(Screen.width / 2 - 250, Screen.height / 2 - 250, 500, 500));
            GUILayout.BeginVertical("box");
            GUILayout.Label("CIVPOP PlaceHolder GUI");
            if (GUILayout.Button("Close this Window", GUILayout.Width(200f)))
                OnToggleFalse();
            GUILayout.Label("Can this display?");
            GUILayout.EndVertical();
            GUI.EndGroup();
        }
    }
}