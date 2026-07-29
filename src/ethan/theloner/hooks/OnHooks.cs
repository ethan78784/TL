using Menu;
using TL.ethan.theloner.misc;

namespace TL.ethan.theloner.hooks {
   
    
    public class OnHooks {

        
        /// <summary>
        /// Subscribes all of the mod's OnHooks to their various methods
        /// </summary>
        public static void Apply() {
            // RoomSpecificScripts
            On.Room.Loaded += OnLoadRoom;

            // Locking slugcat selection menu when Loner ascends
            On.Menu.SlugcatSelectMenu.ctor += SlugcatSelectMenu_Init;
            On.Menu.SlugcatSelectMenu.UpdateStartButtonText += SlugcatSelectMenu_UpdateStartButtonText;
            On.Menu.SlugcatSelectMenu.ContinueStartedGame += SlugcatSelectMenu_ContinueStartedGame;
        }


        /// <summary>
        /// Unsubscribes all of the mod's OnHooks. Only used in niche cases, such as when mods are hot-reloaded by a tool like RainReloader (which I use)
        /// </summary>
        public static void Remove() {
            On.Room.Loaded -= OnLoadRoom;

            On.Menu.SlugcatSelectMenu.ctor -= SlugcatSelectMenu_Init;
            On.Menu.SlugcatSelectMenu.UpdateStartButtonText -= SlugcatSelectMenu_UpdateStartButtonText;
            On.Menu.SlugcatSelectMenu.ContinueStartedGame -= SlugcatSelectMenu_ContinueStartedGame;
        }
        
        
        
        // ========================= ===== ========================= 
        // ========================= HOOKS ========================= 
        // ========================= ===== ========================= 
        
        
        // For applying RoomSpecificScripts
        private static void OnLoadRoom(On.Room.orig_Loaded originalCall, Room room) {
            originalCall(room);
            LORoomSpecificScripts.AddRoomSpecificScript(room);
        }

        
        // For modifying the selection menus of The Loner and The Slug to lock their campaigns after completion, similar to Hunter, Artificer, and Saint
        
        private static readonly SlugcatStats.Name _LONER_ID = new SlugcatStats.Name("lonelyscuggy"); 
        private static bool isLonerAscended = false;
        
        /// Stores the state of whether or not either slugcat (currently just The Loner) is ascended
        private static void SlugcatSelectMenu_Init(On.Menu.SlugcatSelectMenu.orig_ctor originalCall, SlugcatSelectMenu self, ProcessManager processManager) {
            originalCall(self, processManager);
            
            // When constructing the select menu, look for the save-game data tied to the loner, and if they've ascended, store that in a boolean for easy access later.
            if (self.saveGameData.ContainsKey(_LONER_ID)) {
                SlugcatSelectMenu.SaveGameData saveData = self.saveGameData[_LONER_ID];
                if (saveData != null) {
                    isLonerAscended = saveData.ascended;    
                }
                
            }
        }
        
        /// Updates the start button text for either slugcat to read "STATISTICS" instead of "CONTINUE" if their campaign has been completed.
        private static void SlugcatSelectMenu_UpdateStartButtonText(On.Menu.SlugcatSelectMenu.orig_UpdateStartButtonText originalCall, SlugcatSelectMenu self) {
            
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

        
        /// Prevents continuing The Loner's campaign after they've ascended, instead sending the player to the Statistics screen.
        private static void SlugcatSelectMenu_ContinueStartedGame(On.Menu.SlugcatSelectMenu.orig_ContinueStartedGame originalCall, SlugcatSelectMenu self, SlugcatStats.Name selectedScugID) {

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