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
        private static bool ownTowers = true;
        private static bool enemyTowers = true;
        private static bool attackrange = true;
		private static ParticleEffect rangeDisplay;
		private static float lastRange;
		private static Hero me;
        private static readonly Menu Menu = new Menu("Display", "towerRange", true);
        // ReSharper disable once CollectionNeverQueried.Local
        private static readonly List<ParticleEffect> Effects = new List<ParticleEffect>(); // keep references

        private static void Main()
        {
            var ally = new MenuItem("ownTowers", "Range of allied towers").SetValue(true);
            var enemy = new MenuItem("enemyTowers", "Range of enemy towers").SetValue(true);
            var jarak = new MenuItem("attackrange", "Range of hero attack").SetValue(true);

            ownTowers = ally.GetValue<bool>();
            enemyTowers = enemy.GetValue<bool>();
            attackrange= jarak.GetValue<bool>();

            ally.ValueChanged += MenuItem_ValueChanged;
            enemy.ValueChanged += MenuItem_ValueChanged;
            jarak.ValueChanged += MenuItem_ValueChanged;

            Menu.AddItem(ally);
            Menu.AddItem(enemy);
            Menu.AddItem(jarak);

            Menu.AddToMainMenu();

            DisplayRange();
            Game.OnFireEvent += Game_OnFireEvent;
        }

        // ReSharper disable once InconsistentNaming
        private static void MenuItem_ValueChanged(object sender, OnValueChangeEventArgs e)
        {
            var item = sender as MenuItem;

            // ReSharper disable once PossibleNullReferenceException
            if (item.Name == "ownTowers") ownTowers = e.GetNewValue<bool>();
            else enemyTowers = e.GetNewValue<bool>();

            DisplayRange();
        }

        private static void Game_OnFireEvent(FireEventEventArgs args)
        {
            if (args.GameEvent.Name == "dota_game_state_change")
            {
                var state = (GameState) args.GameEvent.GetInt("new_state");
                if (state == GameState.Started || state == GameState.Prestart )
                    DisplayRange();
            }
        }

        private static void DisplayRange()
        {
            if (!Game.IsInGame)
                return;

            foreach (var e in Effects)
            {
                e.Dispose();
            }
            rangeDisplay.Dispose();
            Effects.Clear();
            me = ObjectMgr.LocalHero;
            var player = ObjectMgr.LocalPlayer;
            rangeDisplay = null;			
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
	            if (attackrange)
	            {
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
            }
        }
    }
}
