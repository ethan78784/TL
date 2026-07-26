using System;
using System.Security.Permissions;
using BepInEx;
using TL.ethan.theloner.misc;


#pragma warning disable CS0618 // SecurityAction.RequestMinimum is obsolete. However, this does not apply to the mod, which still needs it. Suppress the warning indicating that it is obsolete.
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618



namespace TL.ethan.theloner {
    
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class TheLonerMain : BaseUnityPlugin {
        
        public const string PLUGIN_GUID = "ethan.lonescug";
        public const string PLUGIN_NAME = "The Loner";
        public const string PLUGIN_VERSION = "0.1.0";
        public bool initalized;
        
        public void OnEnable() {
            
            // Only ever do this once– we set a flag past this point, and never run any of this again if that flag has been set
            // This is so tools like RainReloader don't double-subscribe to hooked functions, which would cause chaos
            if (initalized) return;
            initalized = true;
            
            // Hook subscribers
            On.RoomSpecificScript.AddRoomSpecificScript += new On.RoomSpecificScript.hook_AddRoomSpecificScript(OnAddRoomSpecificScripts);

        }

        public void OnDisable() {
            
            if (!initalized) return;
            initalized = false;
            
            // Unhook subscribers
            On.RoomSpecificScript.AddRoomSpecificScript -= new On.RoomSpecificScript.hook_AddRoomSpecificScript(OnAddRoomSpecificScripts);
            
        }


        private void OnAddRoomSpecificScripts(On.RoomSpecificScript.orig_AddRoomSpecificScript originalCall, Room room) {
            originalCall(room);
            LORoomSpecificScripts.AddRoomSpecificScript(room);
        }
    }
}