using System.Collections.Generic;

namespace TL.ethan.theloner.utils {
    
    public class SaveDataHelper {

        public static readonly SaveDataHolder LONER_ASCEND = new SaveDataHolder("LONERASCEND");
        public static readonly SaveDataHolder THESLUG_NOLOOSEENDS = new SaveDataHolder("NOLOOSEENDS");
        
        

        // Holds and handles saving and loading the save data of a specific type, so we don't have to do it manually every time
        public class SaveDataHolder {
            // The key that's saved and loaded to unrecognizedSaveStrings
            public readonly string saveDataKey;

            public SaveDataHolder(string inkey) {
                saveDataKey = TheLonerMain.PLUGIN_GUID + inkey;
            }

            /// <summary>
            /// Saves this flag to the current campaign.
            ///
            /// Note: can only save data to this campaign if the current campaign is a "story session"– basically, anything that isn't an expedition.
            /// </summary>
            /// <param name="state">The campaign's savestate instance</param>
            public void SaveToCampaign(SaveState state) {
                
                List<string> saveStrings = state.unrecognizedSaveStrings;

                if (!saveStrings.Contains(saveDataKey)) {
                    saveStrings.Add(saveDataKey);
                }
                
            }


            /// <summary>
            /// Removes this flag from the current campaign. Does nothing if this flag doesn't exist in the save data for this campaign.
            /// </summary>
            /// <param name="state"></param>
            public void DeleteFromCampaign(SaveState state) {
                
                List<string> saveStrings = state.unrecognizedSaveStrings;
                
                if (saveStrings.Contains(saveDataKey)) {
                    saveStrings.Remove(saveDataKey);
                }
            }
            

            /// <summary>
            /// Gets the value of this piece of save data for the current campaign.
            /// </summary>
            /// <param name="state">The world of this campaign.</param> 
            /// <returns>true if the flag is present, false if it's absent or the game is not in a story session. </returns>
            public bool GetFlagInCampaign(SaveState state) {
                List<string> saveStrings = state.unrecognizedSaveStrings;
                return saveStrings.Contains(saveDataKey);
            }
            
            
        }
        
    }
    
    
}