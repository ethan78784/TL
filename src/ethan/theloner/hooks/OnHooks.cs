using Menu;
using TL.ethan.theloner.misc;
using TL.ethan.theloner.utils;

namespace TL.ethan.theloner.hooks {
   
    
    public static class OnHooks {


        public static bool hooksEnabled = false;
        
        /// <summary>
        /// Subscribes all of the mod's OnHooks to their various methods
        /// </summary>
        public static void Apply() {

            if (hooksEnabled) {
                return;
            }
            
            // RoomSpecificScripts
            On.Room.Loaded += OnLoadRoom;

            // Locking slugcat selection menu for The Loner or The Slug when either of their campaigns are complete
            On.Menu.SlugcatSelectMenu.ctor += SlugcatSelectMenu_Init;
            On.Menu.SlugcatSelectMenu.UpdateStartButtonText += SlugcatSelectMenu_UpdateStartButtonText;
            On.Menu.SlugcatSelectMenu.ContinueStartedGame += SlugcatSelectMenu_ContinueStartedGame;
            On.Menu.StoryGameStatisticsScreen.CommunicateWithUpcomingProcess += StoryGameStatisticsScreen_CommunicateWithUpcomingProcess;
            
            // Replacing the default sleep-screen background with a custom one for The Slug's ending
            On.Menu.StoryGameStatisticsScreen.AddBkgIllustration += StoryGameStatisticScreen_AddBkgIllustration;
            hooksEnabled = true;
        }


        /// <summary>
        /// Unsubscribes all of the mod's OnHooks. Only used in niche cases, such as when mods are hot-reloaded by a tool like RainReloader (which I use)
        /// </summary>
        public static void Remove() {

            if (!hooksEnabled) {
                return;
            }
            
            On.Room.Loaded -= OnLoadRoom;

            On.Menu.SlugcatSelectMenu.ctor -= SlugcatSelectMenu_Init;
            On.Menu.SlugcatSelectMenu.UpdateStartButtonText -= SlugcatSelectMenu_UpdateStartButtonText;
            On.Menu.SlugcatSelectMenu.ContinueStartedGame -= SlugcatSelectMenu_ContinueStartedGame;
            
            On.Menu.StoryGameStatisticsScreen.CommunicateWithUpcomingProcess -= StoryGameStatisticsScreen_CommunicateWithUpcomingProcess;
            On.Menu.StoryGameStatisticsScreen.AddBkgIllustration -= StoryGameStatisticScreen_AddBkgIllustration;

            hooksEnabled = false;
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
        private static readonly SlugcatStats.Name _THESLUG_ID = new SlugcatStats.Name("theslug"); 
        private static bool isLonerAscended = false;
        private static bool isSlugPurposeFulfilled = false;
        
        /// Stores the state of whether or not either slugcat (currently just The Loner) is ascended
        private static void SlugcatSelectMenu_Init(On.Menu.SlugcatSelectMenu.orig_ctor originalCall, SlugcatSelectMenu self, ProcessManager processManager) {
            originalCall(self, processManager);
            
            
            PlayerProgression progressionManager = self.manager.rainWorld.progression;

            if (progressionManager.IsThereASavedGame(_THESLUG_ID)) {
                TheLonerMain.Logger.LogWarning("Worked!!!");
                
                SaveState state = progressionManager.GetOrInitiateSaveState(_THESLUG_ID, null, self.manager.menuSetup, false);
                isSlugPurposeFulfilled = SaveDataHelper.THESLUG_NOLOOSEENDS.GetFlagInCampaign(state);
                TheLonerMain.Logger.LogWarning(isSlugPurposeFulfilled);
                
                progressionManager.ClearOutSaveStateFromMemory();
            }
            
        }
        
        /// Updates the start button text for either slugcat to read "STATISTICS" instead of "CONTINUE" if their campaign has been completed.
        private static void SlugcatSelectMenu_UpdateStartButtonText(On.Menu.SlugcatSelectMenu.orig_UpdateStartButtonText originalCall, SlugcatSelectMenu self) {
            
            // If we have a save game on the current page that we're not currently trying to restart,
            // AND we're on The Loner or The Slug's page, AND either of their campaign-locking flags have been checked, 
            // replace the text on the continue button with "STATISTICS" to denote the campaign's end.
            // We'll override its behavior elsewhere
            if (self.GetSaveGameData(self.slugcatPageIndex) != null && !self.restartChecked) {

                if (
                    (self.slugcatPages[self.slugcatPageIndex].slugcatNumber == _LONER_ID && isLonerAscended)
                    || (self.slugcatPages[self.slugcatPageIndex].slugcatNumber == _THESLUG_ID && isSlugPurposeFulfilled)
                ) {
                    self.startButton.menuLabel.text = "STATISTICS";
                    return;
                }
                
            }
            originalCall(self);
        }

        
        /// Prevents continuing The Loner's or The Slug's campaign after they've ascended/completed it, instead sending the player to the Statistics screen.
        private static void SlugcatSelectMenu_ContinueStartedGame(On.Menu.SlugcatSelectMenu.orig_ContinueStartedGame originalCall, SlugcatSelectMenu self, SlugcatStats.Name selectedScugID) {

            if ( (selectedScugID == _LONER_ID && isLonerAscended) || selectedScugID == _THESLUG_ID && isSlugPurposeFulfilled ) {
                // Temporarily store the campaign's score and results here to display in the Statistics screen we're about to switch to
                self.redSaveState = self.manager.rainWorld.progression.GetOrInitiateSaveState(selectedScugID, (RainWorldGame) null, self.manager.menuSetup, false);
                self.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Statistics);
                self.PlaySound(SoundID.MENU_Switch_Page_Out);
            }
            else {
                originalCall(self, selectedScugID);
            }
            
        }


        /// When going from the game-over statistics screen back to the main menu, sets the flags that lock The Slug or The Loner's campaign (since otherwise they wont be locked until the main menu screen is re-created on game startup(?)_
        private static void StoryGameStatisticsScreen_CommunicateWithUpcomingProcess(On.Menu.StoryGameStatisticsScreen.orig_CommunicateWithUpcomingProcess originalCall, StoryGameStatisticsScreen self, MainLoopProcess nextProcess) {
            originalCall(self, nextProcess);

            if (nextProcess is SlugcatSelectMenu menu && RainWorld.lastActiveSaveSlot == _THESLUG_ID) {
                menu.slugcatPageIndex = menu.indexFromColor(_THESLUG_ID);
                isSlugPurposeFulfilled = SaveDataHelper.THESLUG_NOLOOSEENDS.GetFlagInCampaign(self.saveState);
                menu.UpdateSelectedSlugcatInMiscProg();
            }
        }

        /// When viewing The Slug's statistics after their non-ascension ending, replace the default sleep-screen background with a custom one (empty for now)
        private static void StoryGameStatisticScreen_AddBkgIllustration(On.Menu.StoryGameStatisticsScreen.orig_AddBkgIllustration originalCall, StoryGameStatisticsScreen self) {
            
            if (RainWorld.lastActiveSaveSlot == _THESLUG_ID) {
                
                if (SaveDataHelper.THESLUG_NOLOOSEENDS.GetFlagInCampaign(self.saveState)) {
                    self.scene = new InteractiveMenuScene(self, self.pages[0], MenuScene.SceneID.Empty);
                    self.pages[0].subObjects.Add(self.scene);
                }   
                else {
                    originalCall(self);
                }
            }
            else {
                originalCall(self);
            }
        }
    }
}