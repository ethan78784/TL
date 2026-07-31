

using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using TL.ethan.theloner.cutscene;
using TL.ethan.theloner.utils;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Watcher;


namespace TL.ethan.theloner.misc 
{
    public static class LORoomSpecificScripts {

        // Dictionary of room names to an instance of the class responsible for this roomspecificscript
        private static readonly Dictionary<string, Func<UpdatableAndDeletable> > _STORY_EVENTS = new Dictionary<string, Func<UpdatableAndDeletable>> {
                { "PAC_OPEN", () => new LonerTestScript() },
                // Ending cutscene after collecting the broadcast token in Baring Horns
                { "BAH_G01", () => new BaringHorns_EndScene() }
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
                        TheLonerMain.Logger.LogWarning("Adding RoomSpecificScript for room \"" + entry.Key + "\"");
                        room.AddObject(entry.Value.Invoke());
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

            private Player player;
            private int _messageCount = 0;
            private float lastPosition;
            
            private bool cutsceneTriggered;
            private bool getRidOfThoseRatsPleaseGod;
            private bool backAtStatue;

            private int cutsceneTimer = 0;
            private int lookAtTimer = 0;

            private int jumpTimer = 0;
            private int jumpCount = 0;

            private readonly float[] statueRange = { 1400f, 2100f };
            private readonly float statueLocation = 1755;
            
            public override void Update(bool eu) {
                base.Update(eu);
                
                if (player == null && room.game.Players.Count > 0 && room.game.Players[0].realizedCreature != null)
                    player = room.game.Players[0].realizedCreature as Player;

                if (player == null) {
                    return;
                }
                
                // Only run the script if the player is present in this room (which isn't the case by default??)
                if (player.room != room) {
                    return;
                }

                // I was sick and tired of these rats pushing me out of cutscene position so I stun them for like 5 mins
                // LET ME AURAFARM DAMMIT
                if (!getRidOfThoseRatsPleaseGod) {
                    foreach (var creature in room.abstractRoom.creatures) {
                        if (creature.realizedCreature is Rat) {
                            creature.realizedCreature.stun = 3000;
                        }

                        if (creature.realizedCreature is BigMoth moth) {
                            // Get these moths out of the way too but I like 'em so they can just get SM64-Upwarp'd
                            if (moth.Small) {
                                moth.mainBodyChunk.HardSetPosition(new Vector2(moth.mainBodyChunk.pos.x, 2000));
                            }
                        }
                    }

                    Debug.LogWarning("RATS AND MOTHS BEGONE");
                    getRidOfThoseRatsPleaseGod = true;
                }
                
                
                /*
                if (_player == null || _player.room != room || room.game.cameras[0].hud == null || room.game.cameras[0].hud.textPrompt.messages.Count >= 1)
                    return;

                if (_messageCount == 0) {
                    room.game.cameras[0].hud.textPrompt.AddMessage(this.room.game.manager.rainWorld.inGameTranslator.Translate("Test!"), 120, 200, true, true);
                    ++_messageCount;
                }
                */

               // Statue: 
               //   < x = 2180
               //   > x = 1250
               // Location: 1760
               
               // Check if they haven't been moving by comparing their position last update with current position
               bool isMoving = !Mathf.Approximately(lastPosition, player.mainBodyChunk.pos.x);
               lastPosition = player.mainBodyChunk.pos.x;
               
               /*
               Debug.LogWarning(_player.input.ToString());
               Debug.LogWarning(isMoving);
               Debug.LogWarning(_player.mainBodyChunk.pos.x);
               */

               if (!cutsceneTriggered && player.mainBodyChunk.pos.x > statueRange[0] && player.mainBodyChunk.pos.x < statueRange[1]) {
                   // Debug.LogWarning("Ooh pretty");
                   //(player.graphicsModule as PlayerGraphics)?.LookAtPoint(new Vector2(1760, 500), 10f);
               }

               if (!cutsceneTriggered && player.mainBodyChunk.pos.x < statueRange[0]) {
                   TheLonerMain.Logger.LogWarning("Stop here!");
                   // At the end of the statue range, stop
                   (player.graphicsModule as PlayerGraphics)?.LookAtPoint(new Vector2(player.mainBodyChunk.pos.x - 100, player.mainBodyChunk.pos.y), 10f);
                   cutsceneTriggered = true;
               }

               if (cutsceneTriggered) {
                   cutsceneTimer++;
                   
                   RoomCamera camera = room.game.cameras[0];
                   
                   if (player.controller == null) {
                       RainWorld.lockGameTimer = true;

                       if (!camera.InCutscene) {
                           camera.EnterCutsceneMode(player.abstractCreature, RoomCamera.CameraCutsceneType.Standard);
                       }
                        // Override the player's controls with scripted ones
                       player.controller = new LTSStatueLookController(this, player);
                       TheLonerMain.Logger.LogWarning("Entered cutscene!");
                       
                   }

                   if (jumpTimer > 0) {
                       jumpTimer--;
                   }

                   // After stopping for two seconds, look at the statue for three seconds
                   if (cutsceneTimer > 120 && cutsceneTimer < 300) {
                       (player.graphicsModule as PlayerGraphics)?.LookAtPoint(new Vector2(statueLocation, 500), 10f);
                   }

                   // At five seconds, stop looking at it and start walking over in the controller
                   if (cutsceneTimer > 300) {
                       (player.graphicsModule as PlayerGraphics)?.LookAtNothing();
                   }

                   
                   if (player.mainBodyChunk.pos.x >= statueLocation) {
                       backAtStatue = true;
                   }
                   
                   // Once we reach the statue,
                   if (backAtStatue) {
                       // New timer, since I have no idea when in the old one we'll have reached the statue
                       lookAtTimer++;
                       
                       // Look at the statue introspectively for ~4 seconds, jump twice in awe, and keep looking at it until second 6
                       if (lookAtTimer < 360) {
                           (player.graphicsModule as PlayerGraphics)?.LookAtPoint(new Vector2(statueLocation, 500), 10f);
                       }
                       else {

                           // Look around for a little less than a second each direction,
                           
                           // First left
                           if (lookAtTimer < 404) {
                               (player.graphicsModule as PlayerGraphics)?.LookAtNothing();
                               (player.graphicsModule as PlayerGraphics)?.LookAtPoint(new Vector2(statueLocation - 100, player.mainBodyChunk.pos.y), 100f);
                           }
                           // Then right
                           else if (lookAtTimer < 434) {
                               (player.graphicsModule as PlayerGraphics)?.LookAtNothing();
                               (player.graphicsModule as PlayerGraphics)?.LookAtPoint(new Vector2(statueLocation + 100, player.mainBodyChunk.pos.y), 100f);
                           }
                           // Reset the player's look point and start heading back to the left in the controller
                           else if (lookAtTimer < 495) {
                               (player.graphicsModule as PlayerGraphics)?.LookAtNothing();
                           }
                           else {
                               // A little less than 11 seconds in to the statue phase, end the cutscene
                               camera.ExitCutsceneMode();
                               player.controller = null;
                               RainWorld.lockGameTimer = false;
                               TheLonerMain.Logger.LogWarning("Exited cutscene!");
                               this.Destroy();
                           }
                           
                       }
                   }
                   
                   
               }


            }

            public Player.InputPackage GetInput() {

                int x = 0;
                bool jump = false;
                
                // If we're 5 seconds in and not yet at the statue, keep walking towards it
                if (cutsceneTimer > 300 && player.mainBodyChunk.pos.x < statueLocation && lookAtTimer == 0) {
                    x = 1;
                }

                // If we've been looking at the statue for 4 seconds, jump a few times to express how cool it is
                if (lookAtTimer > 240) {

                    if (jumpCount < 2) {
                        if (player.canJump > 0 && jumpTimer == 0) {
                            jump = true;
                            jumpCount++;
                            TheLonerMain.Logger.LogWarning("Boing!");
                            jumpTimer = 30; // Hold down jump for a little less than a full second to get full height
                        }
                    }
                    
                    if (jumpTimer > 1) {
                        jump = true;
                    }

                    // After looking around to make sure noone saw that, head back to the left
                    if (lookAtTimer > 434) {
                        x = -1;
                    }
                }
                
                
                
                return new Player.InputPackage(false, Options.ControlSetup.Preset.None, x, 0, jump, false, false, false, false);
            }

            class LTSStatueLookController : Player.PlayerController {
                private LonerTestScript owner;
                private Player player;

                public LTSStatueLookController(LonerTestScript owner, Player player) {
                    this.owner = owner;
                    this.player = player;
                }

                public override Player.InputPackage GetInput() {
                    return this.owner.GetInput();
                }
            }
        }

        private class BaringHorns_EndScene : UpdatableAndDeletable {
            
            private readonly string[] finalMessage = { "<#732e2c>TID: I'm sorry." };
            
            private Player player;
            
            private FadeOut fadeOutBlack;
            private FadeOut fadeOutRed;
            
            
            private bool isInBroadcast;
            private bool doneFinalSave;

            private bool lastWords;
            
            private bool blowsUpSlugWithMind;

            private int cutsceneTimer = 0;
            private int bombTimer = 0;

            public override void Update(bool eu) {
                base.Update(eu);


                if (player == null) {
                    player = CutsceneUtils.getPlayerFromRoom(room);
                    if (player == null) {
                        return;
                    }
                }

                // Get the chatlog once, when they collect the token
                if (!isInBroadcast && player.chatlog) {
                    isInBroadcast = true;
                    Debug.LogWarning(player.chatlogID);
                }

                // Once the chatlog has ended, after we know it's started, start the scene.
                if (isInBroadcast && !player.chatlog) {
                    cutsceneTimer++;
                    
                    // Keep the player stunned for the whole thing (since normally they'd get unstunned after the chatlog ends)
                    player.Stun(25);

                    // And keep the mushroom slow from the chatlog, too, if the last line of dialogue hasnt been displayed yet
                    if (!lastWords) {
                        player.mushroomCounter = 30;
                    }
                    
                    
                    // After a bit of time (to give the chatlog some time to fully fade out), do a very quick fade-to-black
                    if (cutsceneTimer >= 15 && fadeOutBlack == null) {
                        fadeOutBlack = new FadeOut(room, Color.black, 3f, false);
                        room.AddObject(fadeOutBlack);
                    }

                    // Once the fade to black is done, write a faux, lone chatlog message with the final line
                    if (fadeOutBlack != null && fadeOutBlack.IsDoneFading() && !lastWords) {
                        
                        RoomCamera camera = room.game.cameras[0];
                        ChatLogDisplay fauxFinalMessage = new ChatLogDisplay(camera.hud, finalMessage) {
                            disable_fastDisplay = true
                        };
                        
                        camera.hud.AddPart(fauxFinalMessage);

                        lastWords = true;
                    }

                    // Tick up another timer with a mysterious and unknown purpose after we display the last line of dialogue
                    if (lastWords) {
                        bombTimer++;
                    }

                    // Once the second timer hits 1 second (a little more than that, since mushroom effect is still active),
                    // end the campaign with a bang.
                    if (bombTimer > 60 && !blowsUpSlugWithMind && fadeOutBlack != null) {
                            
                        fadeOutRed = new FadeOut(room, new Color(0.3f, 0f, 0f), 15f, false);
                        room.AddObject(fadeOutRed);
                        room.PlaySound(SoundID.Bomb_Explode, player.mainBodyChunk, false, 2.0f, 1.0f);

                        blowsUpSlugWithMind = true;
                    }


                    // Start slowly fading the red fadeout to be fully transparent, leaving just the black one still in the background
                    if (bombTimer > 120 && fadeOutRed != null) {
                        fadeOutRed.fadeColor = Color.Lerp(fadeOutRed.fadeColor, new Color(0f, 0f, 0f, 1.0f), 0.05f);
                    }


                    // At the end of the scene, save the game, set the campaign-ended flag, and go to the statistics screen
                    if (bombTimer > 200) {
                        SaveDataHelper.THESLUG_NOLOOSEENDS.SaveToCampaign(room.game.GetStorySession.saveState);
                    
                        room.game.GoToRedsGameOver();
                        RainWorldGame.BeatGameMode(room.game, false);
                        doneFinalSave = true;
                    }
                    
                    
                }
            }
        }

        private class TestScene : UpdatableAndDeletable {

            private ScriptedScene cutsceneHelper;

            private List<SceneActions.GenericSceneAction> actions = new List<SceneActions.GenericSceneAction> {
                new SceneActions.GenericSceneAction(action => SceneActions.LookAtPoint(action, new Vector2(20, 20)) ,100) {
                    
                }
            };

            static void testmethod() {
                
            }
        }
        
    } 
}