using UnityEngine;

namespace TL.ethan.theloner.hooks {
    public class ILHookImpl {
        
        // Implementation methods for IL hooks are put here for the sake of organization

        
        
        public static void Player_ProcessChatLog_Impl(Player self) {
            // We're now (hopefully) right before Player.ProcessChatLog, line 1633.
            // Yippee!
            TheLonerMain.Logger.LogWarning(self.chatlogID);
            
        }

    }
}