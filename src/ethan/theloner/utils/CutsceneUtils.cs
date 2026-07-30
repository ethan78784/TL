using JetBrains.Annotations;

namespace TL.ethan.theloner.utils {
    
    public static class CutsceneUtils {


        /// <summary>
        /// Gets the player in the given room, if they're present.
        /// </summary>
        /// <param name="room">The given room</param>
        /// <param name="playerNum">The player you're trying to get. In normal gameplay, there'll only ever be one player-- player 0. But in Jolly co-op, there can be up to four players, with indexes 0-3.</param>
        /// <returns></returns>
        [CanBeNull]
        public static Player getPlayerFromRoom(Room room, int playerNum = 0) {

            Player player = null;
            if (room.game.Players.Count > 0 && room.game.Players[playerNum].realizedCreature != null) {
                player = room.game.Players[playerNum].realizedCreature as Player;
            }
            return player;

        }
        
        
        
    }
}