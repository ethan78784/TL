using System;
using UnityEngine;

namespace TL.ethan.theloner.cutscene {
    public class SceneActions {
        
        
        
        /// <summary>
        /// A generic scene action, whose runner is executed every update for the action's whole duration.
        /// </summary>
        public class GenericSceneAction {

            public int duration;
            public int sceneTimer;

            public Action runner;

            public ScriptedScene ownerScene;

            public bool lastRun = false;
            
            public GenericSceneAction(Action runner, int duration) {
                this.duration = duration;
                this.runner = runner;
            }

            public virtual void RunStep() {
                runner.Invoke();
                sceneTimer++;
            }

            
            private ScriptedScene.CutscenePlayerController takeControl() {
                ScriptedScene.CutscenePlayerController controller = new ScriptedScene.CutscenePlayerController();
                ownerScene.player.controller = controller;
                return controller;
            }


            public void MoveLeft() {
                if (ownerScene.controller == null) {
                    ownerScene.controller = takeControl();
                }
                ownerScene.controller.movementX = -1;
            }
            
            public void MoveRight() {
                if (ownerScene.controller == null) {
                    ownerScene.controller = takeControl();
                }
                ownerScene.controller.movementX = 1;
            }
            
            public void LookUp() {
                Player player = ownerScene.player;
                Vector2 pos = player.mainBodyChunk.pos;
                ((PlayerGraphics) ownerScene.player.graphicsModule).LookAtPoint(new Vector2(pos.x, pos.y + 100f), 100f);
            }
    
        }

        
        /// <summary>
        /// A scene action whose runner is only executed once. This action will do nothing for the rest of its duration.
        /// </summary>
        public class RunOnceSceneAction : GenericSceneAction {
            
            public RunOnceSceneAction(Action runner, int duration) : base(runner, duration) {}

            public override void RunStep() {
                if (sceneTimer == 0) {
                    base.RunStep();
                }
            }
        }
        
    }
}