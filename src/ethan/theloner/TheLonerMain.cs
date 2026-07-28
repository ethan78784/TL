using System;
using System.Security.Permissions;
using BepInEx;
using Menu;
using RWCustom;
using SlugBase;
using TL.ethan.theloner.misc;
using UnityEngine;


#pragma warning disable CS0618 // SecurityAction.RequestMinimum is obsolete. However, this does not apply to the mod, which still needs it. Suppress the warning indicating that it is obsolete.
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618



namespace TL.ethan.theloner {
    
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class TheLonerMain : BaseUnityPlugin {
        
        public const string PLUGIN_GUID = "ethan.unfortunatecircumstances";
        public const string PLUGIN_NAME = "The Loner";
        public const string PLUGIN_VERSION = "0.1.0";
        public bool initalized = false;

        private readonly SlugcatStats.Name _LONER_ID = new SlugcatStats.Name("lonelyscuggy"); 
        
        
        public void OnEnable() {
            Debug.LogWarning("Initalizing!!");
            
            // Only ever do this once– we set a flag past this point, and never run any of this again if that flag has been set
            // This is so tools like RainReloader don't double-subscribe to hooked functions, which would cause chaos
            if (initalized) return;
            initalized = true;
            
            // Hook subscribers
            
            // RoomSpecificScripts
            On.Room.Loaded += OnLoadRoom;

            // Locking slugcat selection menu when Loner ascends
            On.Menu.SlugcatSelectMenu.ctor += SlugcatSelectMenu_Init;
            On.Menu.SlugcatSelectMenu.UpdateStartButtonText += SlugcatSelectMenu_UpdateStartButtonText;
            On.Menu.SlugcatSelectMenu.ContinueStartedGame += SlugcatSelectMenu_ContinueStartedGame;

        }

        // Only used for hot-reload tools such as RainReloader, but they're very useful, so I decided to add support for those anyways
        public void OnDisable() {
            
            if (!initalized) return;
            initalized = false;
            
            // Unhook subscribers

            On.Room.Loaded -= OnLoadRoom;

            On.Menu.SlugcatSelectMenu.ctor -= SlugcatSelectMenu_Init;
            On.Menu.SlugcatSelectMenu.UpdateStartButtonText -= SlugcatSelectMenu_UpdateStartButtonText;
            On.Menu.SlugcatSelectMenu.ContinueStartedGame -= SlugcatSelectMenu_ContinueStartedGame;
        }


        private void OnLoadRoom(On.Room.orig_Loaded originalCall, Room room) {
            originalCall(room);
            LORoomSpecificScripts.AddRoomSpecificScript(room);
        }


        private bool isLonerAscended = false;
        
        
        private void SlugcatSelectMenu_Init(On.Menu.SlugcatSelectMenu.orig_ctor originalCall, SlugcatSelectMenu self, ProcessManager processManager) {
            originalCall(self, processManager);
            
            // When constructing the select menu, look for the save-game data tied to the loner, and if they've ascended, store that in a boolean for easy access later.
            if (self.saveGameData.ContainsKey(_LONER_ID)) {
                SlugcatSelectMenu.SaveGameData saveData = self.saveGameData[_LONER_ID];
                if (saveData != null) {
                    isLonerAscended = saveData.ascended;    
                }
                
            }
            

        }
        
        
        private void SlugcatSelectMenu_UpdateStartButtonText(On.Menu.SlugcatSelectMenu.orig_UpdateStartButtonText originalCall, SlugcatSelectMenu self) {
            
            // If we're on The Loner's page, and they've ascended, and we aren't trying to restart the campaign,
            // replace the text on the continue button with "STATISTICS" to denote the campaign's end.
            // We'll override its behavior elsewhere
            if (
                self.GetSaveGameData(self.slugcatPageIndex) != null 
                && !self.restartChecked
                && self.slugcatPages[self.slugcatPageIndex].slugcatNumber == _LONER_ID
                && isLonerAscended
                ) {
                self.startButton.menuLabel.text = "STATISTICS";
            }
            else {
                originalCall(self);
            }
        }

        
        // Prevent continuing The Loner's campaign after they've ascended, instead going to the Statistics screen
        private void SlugcatSelectMenu_ContinueStartedGame(On.Menu.SlugcatSelectMenu.orig_ContinueStartedGame originalCall, SlugcatSelectMenu self, SlugcatStats.Name selectedScugID) {

            if (selectedScugID == _LONER_ID && isLonerAscended) {
                // Temporarily store the campaign's score and results here to display in the Statistics screen we're about to switch to
                self.redSaveState = self.manager.rainWorld.progression.GetOrInitiateSaveState(_LONER_ID, (RainWorldGame) null, self.manager.menuSetup, false);
                self.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Statistics);
                self.PlaySound(SoundID.MENU_Switch_Page_Out);
            }
            else {
                originalCall(self, selectedScugID);
            }
            
        }
        
        
        
    }
}