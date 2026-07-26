

using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace TL.ethan.theloner.misc 
{
    public static class LORoomSpecificScripts {

        // Dictionary of room names to an instance of the class responsible for this roomspecificscript
        private static readonly Dictionary<string, UpdatableAndDeletable> _STORY_EVENTS = new Dictionary<string, UpdatableAndDeletable> {
                { "PAC_OPEN", new LonerTestScript() }
        };
        
        
        // This is called for every room in the game, which allows us to add scripts to run in specific rooms
        // (Identical to how it works in MSCRoomSpecificScript and its vanilla counterpart)
        public static void AddRoomSpecificScript(Room room) {

            // This can be absent when loading sometimes!! Will lead to an infinite loop if not checked...
            if (room.game == null) {
                return;
            }
            
            string roomID = room.abstractRoom.name;
            
            // Story-related scripts
            if (room.game.IsStorySession) {
                
                // Add any scripts with keys that match the roomID
                foreach (var entry in _STORY_EVENTS) {
                    if (roomID.Equals(entry.Key)) {
                        Debug.Log("Adding RoomSpecificScript for room \"" + entry.Key + "\"");
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

            private Player _player;
            private int _messageCount = 0;
            
            public override void Update(bool eu) {
                base.Update(eu);
                
                if (_player == null && room.game.Players.Count > 0 && room.game.Players[0].realizedCreature != null)
                    _player = room.game.Players[0].realizedCreature as Player;
                
                if (_player == null || _player.room != room || room.game.cameras[0].hud == null || room.game.cameras[0].hud.textPrompt.messages.Count >= 1)
                    return;

                if (_messageCount == 0) {
                    room.game.cameras[0].hud.textPrompt.AddMessage(this.room.game.manager.rainWorld.inGameTranslator.Translate("Test!"), 120, 200, true, true);
                    ++_messageCount;
                }

                Debug.Log(_player.mainBodyChunk.pos.x);

                (_player.graphicsModule as PlayerGraphics)?.LookAtPoint(new Vector2(0, 100), 10f);


            }
            
            
        }
        
        
    } 
}