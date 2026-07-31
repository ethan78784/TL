using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using MonoMod.Cil;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace TL.ethan.theloner.cutscene {
    
    
    
    public class ScriptedScene {
        
        public Player player;
        public Room room;
        public RoomCamera camera;
        
        /// <summary>
        /// A list of actions– instances of subclasses in the SceneActions class– which hold a function to be ran every update for a specified amount of updates.
        /// </summary>
        public List<SceneActions.GenericSceneAction> actions;
        
        public SceneActions.GenericSceneAction currentAction;
        public int updatesLeft;

        public bool sceneComplete = false;

        public CutscenePlayerController controller;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="actions">A list of actions– instances of subclasses in the SceneActions class– which hold a function to be ran every update for a specified amount of updates.</param>
        public ScriptedScene(Player player, Room room, RoomCamera camera, List<SceneActions.GenericSceneAction> actions) {
            this.player = player;
            this.room = room;
            this.camera = camera;
            this.actions = actions;
        }

        public void Step() {

            if (sceneComplete) {
                return;
            }

            if (updatesLeft <= 0 || currentAction == null) {
                GetNextAction();
            }

            if (currentAction != null) {
                updatesLeft--;
                if (updatesLeft == 0) {
                    currentAction.lastRun = true;
                }
                
                currentAction.runner();
            }
            
            

            
            
        }
        
        private void GetNextAction() {
            
            if (actions.Count != 0) {
                currentAction = actions.Unshift();
                updatesLeft = currentAction.duration;
                currentAction.ownerScene = this;
            }
            else {
                sceneComplete = true;
            }
            
        }
        
        

        
        public class CutscenePlayerController : Player.PlayerController {
           
            public int movementX;
            public int movementY;

            public bool jumpHeld;
            public bool throwHeld;
            public bool pickupHeld;
            public bool mapHeld;

            public bool crouchToggle;

            public override Player.InputPackage GetInput() {
                return new Player.InputPackage(
                    false, Options.ControlSetup.Preset.None,
                    movementX,
                    movementY,
                    jumpHeld,
                    throwHeld,
                    pickupHeld,
                    mapHeld,
                    crouchToggle
                );
            }
        }



        
        
    }
    
    
}