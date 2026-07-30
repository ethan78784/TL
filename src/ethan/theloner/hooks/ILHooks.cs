using System;
using System.Linq.Expressions;
using System.Reflection.Emit;
using BepInEx.Logging;
using MonoMod.Cil;
using MoreSlugcats;
using UnityEngine;
using Logger = UnityEngine.Logger;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace TL.ethan.theloner.hooks {
    public class ILHooks {

        public static bool hooksEnabled = false;
        
        public static void Apply() {

            if (hooksEnabled) {
                return;
            }
            
            TheLonerMain.Logger.LogWarning("Applying IL hooks for mod \"" + TheLonerMain.PLUGIN_NAME + "\"!");

            // IL hooks are unstable, and may throw an exception when applied, so we throw 'em into a try-catch statement to avoid grinding the entire game to a halt,
            // and to make it easier to debug.
            try {

                //IL.Player.ProcessChatLog += Player_ProcessChatLog;
                
                hooksEnabled = true;
            }
            catch (Exception e) {
                TheLonerMain.Logger.LogError("IL hooks for mod \"" + TheLonerMain.PLUGIN_NAME + "\" failed to apply!");
                TheLonerMain.Logger.LogError("Error message: \"" + e + "\"");
            }
            
        }


        private static void Player_ProcessChatLog(ILContext context) {
            
            // Lemme break this down real quick for posterity's sake, so you know exactly what I'm doing here and why:
            #region LetMeBreakItDownForYouLoner
                // When a chatlog token is collected, a call to `Player.InitChatLog()` is made.
                // This only sets a few properties– the actual handling of this chatlog is done on the next update, by `UpdateMSC` calling `ProcessChatLog()`,
                // Which is the method I'm mixing into here.
                
                // I want to run some custom code for a cutscene when a specific chatlog token is collected. However, `ProcessChatLog()` is ran every update indiscriminately.
                // The first if statement within the method simply blocks the rest of it from running if there's no chatlog token set up,
                // and while I could normal hook into it and just make that check myself, too...
                // I don't like the (admittedly negligble) extra overhead from performing that exact same check twice, and I figured I could use the experience with IL hooking,
                // So apologies to you, dear reader, if you're reading this comment because this mod somehow broke another mod's something-or-other due to me being silly with something.
                
                // All that out of the way, I'm just simply putting my code right after that first if statement, to only run the check for the chatlog ID if the game has already checked to make sure a chatlog is here and ready.
            #endregion

            var cursor = new ILCursor(context);
            
            // I'm comfortable putting it right here– right after the if condition checking to make sure that the current chatlog isn't SI9
            // this.mushroomCounter = 25 is a unique enough line in this method... I think.
            cursor.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(0), // this
                x => x.MatchLdcI4(25),
                x => x.MatchStfld<Player>("mushroomCounter") // .mushroomCounter
            );

            cursor.Emit(OpCodes.Ldarg_0); // Load "this"
            //cursor.EmitDelegate<Func<Player, Action<Player>>>((Player self) => ILHookImpl.Player_ProcessChatLog_Impl);
        }
        
    }
}