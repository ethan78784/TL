

using System.Collections.Generic;

namespace TL.ethan.theloner.misc 
{
    public static class LORoomSpecificScripts {

        // Dictionary of room names to an instance of the class responsible for this roomspecificscript
        private static Dictionary<string, UpdatableAndDeletable> STORY_EVENTS = new Dictionary<string, UpdatableAndDeletable> {
                { "PAC_OPEN", new LonerTestScript() }
        };
        
        
        // This is called for every room in the game, which allows us to add scripts to run in specific rooms
        // (Identical to how it works in MSCRoomSpecificScript and its vanilla counterpart)
        public static void AddRoomSpecificScript(Room room) {

            string roomID = room.abstractRoom.name;
            
            // Story-related scripts
            if (room.game.IsStorySession) {
                
                // Add any scripts with keys that match the roomID
                foreach (var entry in STORY_EVENTS) {
                    if (roomID.Equals(entry.Key)) {
                        room.AddObject(entry.Value);
                    }
                }
                
            }
            
        }
        

        private class LonerTestScript : UpdatableAndDeletable {
            
            #region TechnicalYap
            // All instances of UpdatableAndDeletable require a defined room,
            // But I realizd that, since all RoomSpecificScripts are added as an object using `room.AddObject()`,
            // And the object's room is set there,
            // I technically don't need to manually set the room here, or even pass it to its constructor(?)
            // This allows me to store all of these events in a biggol list above, for better organization
            #endregion
            
            public LonerTestScript() {
                
            }
            
        }
        
        
    } 
}