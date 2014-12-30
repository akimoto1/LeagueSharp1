using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using SharpDX;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace AkiSivir
{
    class Program
    {
        public static string ChampName = "Sivir";
        public static Orbwalking.Orbwalker Orbwalker;
        private static readonly Obj_AI_Hero player = ObjectManager.Player;
        public static Spell Q, W;
        public static SpellSlot IgniteSlot;
        public static Items.Item Dfg, Gunblade;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        public static Menu AkiMenu;


        private static void Game_OnGameLoad(EventArgs args)
        {
            if (player.BaseSkinName != ChampName) return;

            Q = new Spell(SpellSlot.Q, 1100);
            W = new Spell(SpellSlot.W, 900);

            Q.SetSkillshot(0.5f, 80f, 1200, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.5f, 80f, 1200, false, SkillshotType.SkillshotLine);

            IgniteSlot = player.GetSpellSlot("SummonerDot");

            Dfg = new Items.Item(3128, 750f);
            Gunblade = new Items.Item(3146, 700f);

            AkiMenu = new Menu("Aki" + ChampName, ChampName, true);

            AkiMenu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(AkiMenu.SubMenu("Orbwalker"));

            var ts = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(ts);
            AkiMenu.AddSubMenu(ts);

            AkiMenu.AddSubMenu(new Menu("Combo", "Combo"));
            AkiMenu.SubMenu("Combo").AddItem(new MenuItem("useQ", "Usar Q?").SetValue(true));
            AkiMenu.SubMenu("Combo").AddItem(new MenuItem("useW", "Usar W?").SetValue(true));
            AkiMenu.SubMenu("Combo").AddItem(new MenuItem("useItems", "Usar Items?").SetValue(true));
            AkiMenu.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));

            AkiMenu.AddSubMenu(new Menu("Harass", "Harass"));
            AkiMenu.SubMenu("Harass").AddItem(new MenuItem("hQ", "Usar Q?").SetValue(true));
            AkiMenu.SubMenu("Harass").AddItem(new MenuItem("Harassmana", "Mana para usar").SetValue(new Slider(30)));
            AkiMenu.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "Harass").SetValue(new KeyBind('C', KeyBindType.Press)));
            AkiMenu.SubMenu("Harass").AddItem(new MenuItem("HarassToggle", "Harass").SetValue(new KeyBind('T', KeyBindType.Toggle)));

            AkiMenu.AddSubMenu(new Menu("Killsteal", "Roubar Kill"));
            AkiMenu.SubMenu("Killsteal").AddItem(new MenuItem("KillQ", "Roubar com Q?").SetValue(true));
            AkiMenu.SubMenu("Killsteal").AddItem(new MenuItem("KillI", "Roubar com Ignite?").SetValue(true));

            AkiMenu.AddSubMenu(new Menu("Drawing", "Drawing"));
            AkiMenu.SubMenu("Drawing").AddItem(new MenuItem("DrawQ", "Mostrar Q?").SetValue(true));
            AkiMenu.SubMenu("Drawing").AddItem(new MenuItem("DrawAA", "Mostrar Range?").SetValue(true));




            AkiMenu.AddItem(new MenuItem("Packet", "Packet Casting").SetValue(true));

            AkiMenu.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;

            Game.PrintChat("<font color=\"#FFF300\">AkiSivir</font>" + " Loaded");
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            if (AkiMenu.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }

            if (AkiMenu.Item("HarassActive").GetValue<KeyBind>().Active || AkiMenu.Item("HarassToggle").GetValue<KeyBind>().Active)
            {
                Harass();
            }

            KillSteal();
        }




        static void Drawing_OnDraw(EventArgs args)
        {
            if (AkiMenu.Item("DrawQ").GetValue<bool>() == true)
            {
                Utility.DrawCircle(player.Position, Q.Range, Color.Blue);
            }
            if (AkiMenu.Item("DrawAA").GetValue<bool>() == true)
            {
                Utility.DrawCircle(player.Position, player.AttackRange, Color.Blue);
            }





        }

        public static void KillSteal()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target == null) return;
            var igniteDmg = player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);

            if (target.IsValidTarget(Q.Range) && Q.IsReady() && AkiMenu.Item("KillQ").GetValue<bool>() == true && ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q) > target.Health)
            {
                Q.Cast(target, AkiMenu.Item("Packet").GetValue<bool>());
            }


            if (AkiMenu.Item("KillI").GetValue<bool>() == true && player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                if (igniteDmg > target.Health && player.Distance(target, false) < 600)
                {
                    player.Spellbook.CastSpell(IgniteSlot, target);
                }

            }




        }



        public static void Harass()
        {
            if (player.Mana / player.MaxMana * 100 > AkiMenu.SubMenu("Harass").Item("Harassmana").GetValue<Slider>().Value)
                return;


            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target == null)
                return;
            {
                if (target.IsValidTarget(Q.Range) && Q.IsReady() && AkiMenu.Item("hQ").GetValue<bool>() == true)
                {
                    Q.Cast(target, AkiMenu.Item("Packet").GetValue<bool>());
                }


            }

        }


        public static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target == null) return;


            if (Gunblade.IsReady() && AkiMenu.Item("useItems").GetValue<bool>() == true)
            {
                Gunblade.Cast(target);
            }

            if (Dfg.IsReady() && AkiMenu.Item("useItems").GetValue<bool>() == true)
            {
                Dfg.Cast(target);
            }

            if (target.IsValidTarget(Q.Range) && Q.IsReady() && AkiMenu.Item("useQ").GetValue<bool>() == true)
            {
                Q.Cast(target, AkiMenu.Item("Packet").GetValue<bool>());



            }

            if (target.IsValidTarget(W.Range) && W.IsReady() && AkiMenu.Item("useW").GetValue<bool>() == true)
            {
                W.Cast(target, AkiMenu.Item("Packet").GetValue<bool>());
            }

        }
    }
}