using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common.Menu;
using SharpDX;
using Ensage.Common.Extensions;


namespace TehPucuk
{
    internal class Program
    {
        private static ParticleEffect rangeDisplay;
        private static bool ownTowers = true;
        private static bool enemyTowers = true;
        private static Hero me;
        private static float lastRange;
        private static readonly Menu Menu = new Menu("TowerRange", "towerRange", true);
        // ReSharper disable once CollectionNeverQueried.Local
        private static readonly List<ParticleEffect> Effects = new List<ParticleEffect>(); // keep references

        private static void Main()
        {
            var ally = new MenuItem("ownTowers", "Range of allied towers").SetValue(true);
            var enemy = new MenuItem("enemyTowers", "Range of enemy towers").SetValue(true);

            ownTowers = ally.GetValue<bool>();
            enemyTowers = enemy.GetValue<bool>();

            ally.ValueChanged += MenuItem_ValueChanged;
            enemy.ValueChanged += MenuItem_ValueChanged;

            Menu.AddItem(ally);
            Menu.AddItem(enemy);

            Menu.AddToMainMenu();

            HandleTowers();
            Game.OnFireEvent += Game_OnFireEvent;
            if (rangeDisplay == null)
            {
                return;
            }
            rangeDisplay = null;
        }

        // ReSharper disable once InconsistentNaming
        private static void MenuItem_ValueChanged(object sender, OnValueChangeEventArgs e)
        {
            var item = sender as MenuItem;

            // ReSharper disable once PossibleNullReferenceException
            if (item.Name == "ownTowers") ownTowers = e.GetNewValue<bool>();
            else enemyTowers = e.GetNewValue<bool>();

            HandleTowers();
        }

        private static void Game_OnFireEvent(FireEventEventArgs args)
        {
            if (args.GameEvent.Name == "dota_game_state_change")
            {
                var state = (GameState) args.GameEvent.GetInt("new_state");
                if (state == GameState.Started || state == GameState.Prestart )
                HandleTowers();
                rangeDisplay = null;
                HandleRange();
                
            }
            
        }

        private static void HandleRange()
        {
            if (me == null || !me.IsValid)
            {
                me = ObjectMgr.LocalHero;
                if (rangeDisplay == null)
                {
                    return;
                }
                rangeDisplay = null;
                return;
            }
            if (rangeDisplay == null)
            {
                rangeDisplay = me.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf");
                lastRange = me.GetAttackRange() + me.HullRadius + 25;
                rangeDisplay.SetControlPoint(1, new Vector3(lastRange, 0, 0));
            }
            else
            {
                if (lastRange != (me.GetAttackRange() + me.HullRadius + 25))
                {
                    lastRange = me.GetAttackRange() + me.HullRadius + 25;
                    rangeDisplay.Dispose();
                    rangeDisplay = me.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf");
                    rangeDisplay.SetControlPoint(1, new Vector3(lastRange, 0, 0));
                }
            }
        }
        private static void HandleTowers()
        {
            if (!Game.IsInGame)
                return;

            foreach (var e in Effects)
            {
                e.Dispose();
            }
            Effects.Clear();

            var player = ObjectMgr.LocalPlayer;
            if (player == null)
                return;
            var towers =
                ObjectMgr.GetEntities<Building>()
                    .Where(x => x.IsAlive && x.ClassID == ClassID.CDOTA_BaseNPC_Tower)
                    .ToList();
            if (!towers.Any())
                return;
            
            if (player.Team == Team.Observer)
            {
                foreach (var effect in towers.Select(tower => tower.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf")))
                {
                    effect.SetControlPoint(1, new Vector3(850, 0, 0));
                    Effects.Add(effect);
                }
            }
            else
            {
                if (enemyTowers)
                {
                    foreach (var effect in towers.Where(x => x.Team != player.Team).Select(tower => tower.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf")))
                    {
                        effect.SetControlPoint(1, new Vector3(850, 0, 0));
                        Effects.Add(effect);
                    }
                }
                if (ownTowers)
                {
                    foreach (var effect in towers.Where(x => x.Team == player.Team).Select(tower => tower.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf")))
                    {
                        effect.SetControlPoint(1, new Vector3(850, 0, 0));
                        Effects.Add(effect);
                    }
                }
            }
        }
    }
}
