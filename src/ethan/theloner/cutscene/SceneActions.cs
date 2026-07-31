using System;
using MonoMod.Cil;
using UnityEngine;

namespace TL.ethan.theloner.cutscene {
    public class SceneActions {
        
        
        
        /// <summary>
        /// A generic scene action, whose runner is executed every update for the action's whole duration.
        /// </summary>
        public class GenericSceneAction {

            public int duration;
            public int sceneTimer;

            public Action<GenericSceneAction> runner;

            public ScriptedScene ownerScene;

            public bool lastRun = false;
            
            public GenericSceneAction(Action<GenericSceneAction> runner, int duration) {
                this.duration = duration;
                this.runner = runner;
            }

            public virtual void RunStep() {
                runner.Invoke(this);
                sceneTimer++;
            }

            
            public ScriptedScene.CutscenePlayerController takeControl() {
                ScriptedScene.CutscenePlayerController controller = new ScriptedScene.CutscenePlayerController();
                ownerScene.player.controller = controller;
                return controller;
            }
            
    
        }

        
        /// <summary>
        /// A scene action whose runner is only executed once. This action will do nothing for the rest of its duration.
        /// </summary>
        public class RunOnceSceneAction : GenericSceneAction {
            
            public RunOnceSceneAction(Action<GenericSceneAction> runner, int duration) : base(runner, duration) {}

            public override void RunStep() {
                if (sceneTimer == 0) {
                    base.RunStep();
                }
            }
        }
        
        
        
        public static void MoveLeft(GenericSceneAction self) {
            if (self.ownerScene.controller == null) {
                self.ownerScene.controller = self.takeControl();
            }
            self.ownerScene.controller.movementX = -1;
        }
            
        public static void MoveRight(GenericSceneAction self) {
            if (self.ownerScene.controller == null) {
                self.ownerScene.controller = self.takeControl();
            }
            self.ownerScene.controller.movementX = 1;
        }
            
        public static void LookUp(GenericSceneAction self) {
            Player player = self.ownerScene.player;
            Vector2 pos = player.mainBodyChunk.pos;
            ((PlayerGraphics) self.ownerScene.player.graphicsModule).LookAtPoint(new Vector2(pos.x, pos.y + 100f), 100f);
        }
        
        public static void LookAtPoint(GenericSceneAction self, Vector2 point) {
            ((PlayerGraphics) self.ownerScene.player.graphicsModule).LookAtPoint(point, 100f);
        }
        
        
    }
    
}