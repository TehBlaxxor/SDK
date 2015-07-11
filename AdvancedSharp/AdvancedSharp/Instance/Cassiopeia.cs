using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

using LeagueSharp;
using LeagueSharp.SDK.Core;
using LeagueSharp.SDK.Core.Enumerations;
using LeagueSharp.SDK.Core.Extensions;
using LeagueSharp.SDK.Core.IDrawing;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.Core.Wrappers;
using System.Drawing;
using System.Net.Mime;
using System.Runtime.Remoting.Channels;
using LeagueSharp.SDK.Core.Events;
using LeagueSharp.SDK.Core.Extensions.SharpDX;
using LeagueSharp.SDK.Core.Math.Polygons;
using LeagueSharp.SDK.Core.Math.Prediction;
using LeagueSharp.SDK.Core.UI.IMenu;
using LeagueSharp.SDK.Core.Utils;
using Color = System.Drawing.Color;
using SharpDX;

using Menu = LeagueSharp.SDK.Core.UI.IMenu.Menu;

namespace AdvancedSharp.Instance
{
    internal class Cassiopeia
    {
        public static Menu Z;
        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static Spell Q, W, E, R;
        public static List<Spell> Spells = new List<Spell>();
        public static float LastQ = 0;
        public static float LastE = 0;
        public static int[] abilitySequence;
        public static int q = 0, w = 0, e = 0, r = 0;


        public Cassiopeia()
        {
            abilitySequence = new int[] {1, 3, 3, 2, 3, 4, 3, 1, 3, 1, 4, 1, 1, 2, 2, 4, 2, 2};
            Q = new Spell(SpellSlot.Q, 850f);
            Spells.Add(Q);
            Q.SetSkillshot(0.75f, Q.Instance.SData.CastRadius, float.MaxValue, false, SkillshotType.SkillshotCircle);
            W = new Spell(SpellSlot.W, 850f);
            Spells.Add(W);
            W.SetSkillshot(0.5f, W.Instance.SData.CastRadius, W.Instance.SData.MissileSpeed, false,
                SkillshotType.SkillshotCircle);
            E = new Spell(SpellSlot.E, 700f);
            Spells.Add(E);
            E.SetTargetted(0.2f, float.MaxValue);
            R = new Spell(SpellSlot.R, 825f);
            Spells.Add(R);
            R.SetSkillshot(0.3f, (float) (80*Math.PI/180), float.MaxValue, false, SkillshotType.SkillshotCone);

            Z = new Menu("Adv# - Cassiopeia", "root", true);

            Menu MCombo = new Menu("Combo", "combo");
            {
                MCombo.Add(new MenuBool("combo.q", "Use Q", true));
                MCombo.Add(new MenuBool("combo.w", "Use W", true));
                MCombo.Add(new MenuBool("combo.e", "Use E", true));
                MCombo.Add(new MenuBool("combo.r", "Use R", true));
                MCombo.Add(new MenuSeparator("combo.spacer1", "R Settings"));
                MCombo.Add(new MenuSlider("combo.r.minfacing", "R Minimum Facing", 1, 1, 5));
                MCombo.Add(new MenuSlider("combo.r.minhit", "R Minimum Hit", 1, 1, 5));
                MCombo.Add(new MenuBool("combo.r.smart", "Smart Ultimate", true));
            }
            Z.Add(MCombo);

            Menu MHarass = new Menu("Harass", "harass");
            {
                MHarass.Add(new MenuBool("harass.q", "Use Q", true));
                MHarass.Add(new MenuBool("harass.w", "Use W", true));
                MHarass.Add(new MenuBool("harass.e", "Use E", true));
                MHarass.Add(new MenuBool("harass.spacer1", " "));
                MHarass.Add(new MenuBool("harass.e.restriction", "E Only If Poisoned", true));
            }
            Z.Add(MHarass);

            Menu MLH = new Menu("Last Hit", "lasthit");
            {
                MLH.Add(new MenuBool("lasthit.e", "Use E", true));
                MLH.Add(new MenuBool("lasthit.e.auto", "Use E Automatically", false));
            }
            Z.Add(MLH);

            Menu MLC = new Menu("Lane Clear", "laneclear");
            {
                MLC.Add(new MenuBool("laneclear.q", "Use Q", true));
                MLC.Add(new MenuBool("laneclear.w", "Use W", true));
                MLC.Add(new MenuBool("laneclear.e", "Use E", true));
                MLC.Add(new MenuSeparator("laneclear.spacer1", "E Settings"));
                MLC.Add(new MenuBool("laneclear.e.restriction", "E Only If Poisoned", true));
                MLC.Add(new MenuBool("laneclear.e.lasthit", "E Only If Last Hit", true));
                MLC.Add(new MenuSlider("laneclear.w.restriction", "W Minimum Hit", 3, 1, 10));
            }
            Z.Add(MLC);

            Menu Misc = new Menu("Misc", "misc");
            {
                Misc.Add(new MenuBool("misc.manamenagertm", "Restrict Mana Usage", true));
                Misc.Add(new MenuSlider("misc.manamenagertm.slider", "Minimum Mana", 35, 0, 95));
                Misc.Add(new MenuBool("misc.spacer1", "Item Stack"));
                Misc.Add(new MenuBool("misc.itemstack", "Item Stacking", true));
                Misc.Add(new MenuSeparator("misc.spacer3", "Kill Steal"));
                Misc.Add(new MenuBool("misc.qks", "Kill Secure with Q", true));
                Misc.Add(new MenuBool("misc.wks", "Kill Secure with W", true));
                Misc.Add(new MenuBool("misc.eks", "Kill Secure with E", true));
                Misc.Add(new MenuSlider("misc.edelay", "E Delay", 0, 0, 500));
                Misc.Add(new MenuSeparator("misc.spacer4", "Miscellaneous"));
                Misc.Add(new MenuBool("misc.autospells", "Auto Level Spells", true));
                Misc.Add(new MenuSeparator("misc.spacer5", "Events"));
                Misc.Add(new MenuBool("misc.gc", "W on gap closer", true));
                Misc.Add(new MenuSlider("misc.gc.hp", "if HP < ", 30));
                Misc.Add(new MenuBool("misc.aablock", "Auto Attack Block in combo", false));


            }
            Z.Add(Misc);

            Menu Drawings = new Menu("Drawings", "drawings");
            {
                Drawings.Add(new MenuBool("draw", "Drawings", true));
                Drawings.Add(new MenuBool("draw.q", "Draw Q Range", true));
                Drawings.Add(new MenuBool("draw.w", "Draw W Range", true));
                Drawings.Add(new MenuBool("draw.e", "Draw E Range", true));
                Drawings.Add(new MenuBool("draw.r", "Draw R Range", true));
                Drawings.Add(new MenuBool("draw.tg", "Draw Target", true));
            }
            Z.Add(Drawings);

            Z.Add(new MenuBool("credits1", "Credits:"));
            Z.Add(new MenuBool("credits2", "TehBlaxxor - Coding"));
            Z.Add(new MenuBool("credits3", "Hoes - Coding"));
            Z.Add(new MenuBool("credits4", "Hawk - Testing"));


            Z.Attach();

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Gapcloser.OnGapCloser += AntiGapCloser;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
        }

        private void Game_OnUpdate(EventArgs args)
        {
            Orbwalker.Attack = true;
            KS();
            // TearStack();
            AutoLevel();
            var target = TargetSelector.GetTarget(900f, DamageType.Magical);
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Orbwalk:
                {
                    if (Player.Distance(target.Position) > 440)
                        Orbwalker.Attack = false;
                    else
                        Orbwalker.Attack = true;
                }
                    Orbwalker.Attack = true;
                    Combo();
                    AABlock();
                    break;
                case OrbwalkerMode.LaneClear:
                    LaneClear();
                    JungleClear();
                    break;
                case OrbwalkerMode.LastHit:
                    LastHit();
                    break;
                case OrbwalkerMode.Hybrid:
                    Harass();
                    break;



            }
        }

        private static void AABlock()
        {
            var aaBlock = Z["misc"]["misc.aablock"].GetValue<MenuBool>();
            if (aaBlock.Value)
            {
                Orbwalker.Attack = false;
            }
        }

        public static void AutoE()
        {
            if (Z["misc"]["misc.manamenagertm"].GetValue<MenuBool>().Value &&
                Z["misc"]["misc.manamenagertm.slider"].GetValue<MenuSlider>().Value > Player.ManaPercent)
                return;

            if (!Z["lasthit"]["lasthit.e.auto"].GetValue<MenuBool>().Value || !E.IsReady())
                return;

            foreach (
                var unit in
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(
                            x =>
                                x.IsEnemy && !x.IsDead && E.IsInRange(x) && Player.GetSpellDamage(x, SpellSlot.E) > x.Health + 5 &&
                                x.IsValidTarget()))
            {
                E.CastOnUnit(unit);
            }
        }

        private void AntiGapCloser(object sender, Gapcloser.GapCloserEventArgs e)
        {
            if (Z["misc"]["misc.gc"].GetValue<MenuBool>().Value
                && W.IsReady()
                && Player.HealthPercent <= Z["misc"]["misc.gc.hp"].GetValue<MenuSlider>().Value
                && e.Sender.IsValidTarget())
            {
                W.Cast(e.End);
            }
        }

        private void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.Q)
                LastQ = Environment.TickCount;
            if (args.Slot == SpellSlot.E)
                LastE = Environment.TickCount;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            var target = TargetSelector.GetTarget(850f, DamageType.Magical);
            if (Z["draw"]["draw.tg"].GetValue<MenuBool>().Value && target.IsValidTarget())
            {
                Drawing.DrawCircle(target.Position, 75f, Color.DarkRed);
            }

            foreach (var x in Spells.Where(x => Z["draw." + x.Slot.ToString().ToLowerInvariant()].GetValue<MenuBool>().Value)
                )
            {
                Drawing.DrawCircle(Player.Position, x.Range, x.IsReady()
                    ? Color.Green
                    : Color.Red
                    );
            }
        }

        public static void Combo()
        {
            var target = TargetSelector.GetTarget(900f, DamageType.Magical);

            if (!target.IsValidTarget())
                return;
            if (Z["combo"]["combo.q"].GetValue<MenuBool>().Value)
            {
                if ((target.Health + 50 < Player.GetSpellDamage(target, SpellSlot.Q)
                    && Q.IsReady())
                    ||
                    (!target.HasBuffOfType(BuffType.Poison) && E.IsInRange(target) && E.IsReady() && Q.IsReady() &&
                     Player.Mana >= Q.Instance.ManaCost + 2 * E.Instance.ManaCost)
                    || (!target.HasBuffOfType(BuffType.Poison) && E.Level < 1 && Q.IsReady() && Q.IsInRange(target))
                    ||
                    (Q.IsReady() && E.IsReady() && E.IsInRange(target) &&
                     target.Health + 25 < Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.E) &&
                     Player.Mana >= Q.Instance.ManaCost + E.Instance.ManaCost))
                    Q.Cast(target);
            }
            if (Z["combo"]["combo.w"].GetValue<MenuBool>().Value)
            {
                if ((Player.HealthPercent <= 25 && !Player.IsFacing(target) && target.IsFacing(Player) &&
                     target.IsValidTarget((W.Range / 3) * 2) && W.IsReady() && target.MoveSpeed >= Player.MoveSpeed)
                    ||
                    (!target.HasBuffOfType(BuffType.Poison) && Q.Delay * 1000 + LastQ < Environment.TickCount &&
                     !Q.IsReady() && W.IsReady() && E.IsReady() && E.IsInRange(target) &&
                     Player.Mana >= W.Instance.ManaCost + 2 * E.Instance.ManaCost)
                    ||
                    (!target.HasBuffOfType(BuffType.Poison) && Q.Delay * 1000 + LastQ < Environment.TickCount &&
                     !Q.IsReady() && W.IsReady() && E.IsReady() && E.IsInRange(target) &&
                     Player.Mana >= W.Instance.ManaCost + E.Instance.ManaCost &&
                     Player.GetSpellDamage(target, SpellSlot.W) + Player.GetSpellDamage(target, SpellSlot.E) > target.Health + 25)
                    ||
                    (!target.HasBuffOfType(BuffType.Poison) && Q.Delay * 1000 + LastQ < Environment.TickCount &&
                     (!Q.IsReady() || Player.GetSpellDamage(target, SpellSlot.Q) < target.Health + 25) && W.IsReady() && W.IsInRange(target) &&
                     Player.GetSpellDamage(target, SpellSlot.W) > target.Health + 25))
                    W.Cast(target);
            }
            if (Z["combo"]["combo.e"].GetValue<MenuBool>().Value)
            {
                if ((target.HasBuffOfType(BuffType.Poison) && E.IsReady() && target.IsValidTarget(E.Range) &&
                     Environment.TickCount > LastE + Z["misc"]["misc.edelay"].GetValue<MenuSlider>().Value)
                    || (E.IsReady() && target.IsValidTarget(E.Range) && target.Health + 25 <
Player.GetSpellDamage(target, SpellSlot.E)))
                    E.CastOnUnit(target);
            }

            EasyRLogic();
            SmartR();
        }
        private static void EasyRLogic()
        {

            var rTarget = TargetSelector.GetTarget(R.Range, DamageType.Magical);
            var rSpell = Z["combo"]["combo.r"].GetValue<MenuBool>().Value;
            var rminhitSpell = Z["combo"]["combo.r.minhit"].GetValue<MenuSlider>().Value;
            var rfaceSpell = Z["combo"]["combo.r.minfacing"].GetValue<MenuSlider>().Value;


            List<Obj_AI_Hero> targets = GameObjects.EnemyHeroes.Where(o => R.WillHit(o, rTarget.Position)
                                                                       && o.Distance(Player.Position) < 600).ToList();

            var facing =
                GameObjects.EnemyHeroes.Where(
                    x => R.WillHit(x, rTarget.Position)
                         && x.IsFacing(Player)
                         && !x.IsDead
                         && x.IsValidTarget(600));

            if (rSpell)
            {
                if ((targets.Count() >= rminhitSpell
                    || facing.Count() >= rfaceSpell)
                    && R.IsReady()
                    && rTarget.Health >= (Player.GetSpellDamage(rTarget, SpellSlot.Q)
                    + 2 * Player.GetSpellDamage(rTarget, SpellSlot.E) 
                    + Player.GetSpellDamage(rTarget, SpellSlot.R)))
                {
                    R.Cast(rTarget);
                }
            }
        }

        private static void SmartR()
        {
            var srSpell = Z["combo"]["combo.r.smart"].GetValue<MenuBool>().Value;
            var rTarget = TargetSelector.GetTarget(R.Range, DamageType.Magical);


            if (srSpell)
            {
                if (rTarget.IsValidTarget(500))
                {
                    if (rTarget.IsFacing(Player))
                    {
                        if (Player.HealthPercent + 25 <= rTarget.HealthPercent
                            && R.IsReady())
                        {
                            R.Cast(rTarget);
                        }

                        if (rTarget.HasBuffOfType(BuffType.Poison)
                            && E.IsReady() && R.IsReady()
                            && Player.Mana >= (2 * E.Instance.ManaCost + R.Instance.ManaCost)
                            && rTarget.Health < (Player.GetSpellDamage(rTarget, SpellSlot.E) * 4 + Player.GetSpellDamage(rTarget, SpellSlot.R)))
                        {
                            R.Cast(rTarget);
                        }
                        if (!rTarget.HasBuffOfType(BuffType.Poison)
                            && Q.IsReady() && E.IsReady() && R.IsReady()
                            && rTarget.Health < (Player.GetSpellDamage(rTarget, SpellSlot.Q)
                            + Player.GetSpellDamage(rTarget, SpellSlot.E) * 4
                            + Player.GetSpellDamage(rTarget, SpellSlot.R))
                            && Player.Mana >= (Q.Instance.ManaCost + 2 * E.Instance.ManaCost + R.Instance.ManaCost))
                        {
                            R.Cast(rTarget);
                        }
                    }
                }
            }
           
        }
        public static void Harass()
        {
            if (Z["misc"]["misc.manamenagertm"].GetValue<MenuBool>().Value && Z["misc"]["misc.manamenagertm.slider"].GetValue<MenuSlider>().Value > Player.ManaPercent)
                return;

            var target = TargetSelector.GetTarget(850f, DamageType.Magical);
            if (Z["harass"]["harass.q"].GetValue<MenuBool>().Value 
                && Q.IsReady()
                && Q.IsInRange(target))
                Q.Cast(target);

            if (Z["harass"]["harass.w"].GetValue<MenuBool>().Value
                && W.IsReady()
                && W.IsInRange(target))
                W.Cast(target);

            if (Z["harass"]["harass.e"].GetValue<MenuBool>().Value
                && E.IsReady() && E.IsInRange(target) 
                && Environment.TickCount > LastE + Z["misc"]["misc.edelay"].GetValue<MenuSlider>().Value
                && ((target.HasBuffOfType(BuffType.Poison) && Z["harass"]["harass.e.restriction"].GetValue<MenuBool>().Value)
                || (!target.HasBuffOfType(BuffType.Poison) && !Z["harass"]["harass.e.restriction"].GetValue<MenuBool>().Value)))
                E.CastOnUnit(target);
        }

        public static void KS()
        {
            if (E.IsReady() && Z["misc"]["misc.eks"].GetValue<MenuBool>().Value)
            {
                foreach (var x in GameObjects.EnemyHeroes.Where(x => !x.IsDead
                    && x.IsValidTarget(E.Range)
                    && x.Health + 10 < Player.GetSpellDamage(x, SpellSlot.E)))
                {
                    E.CastOnUnit(x);
                }
            }

            if (Q.IsReady() && Z["misc"]["misc.qks"].GetValue<MenuBool>().Value)
            {
                foreach (var x in GameObjects.EnemyHeroes.Where(x => !x.IsDead
                    && x.IsValidTarget(Q.Range)
                    && x.Health + 25 < Player.GetSpellDamage(x, SpellSlot.Q)))
                {
                    Q.Cast(x);
                }
            }

            if (W.IsReady() && Z["misc"]["misc.wks"].GetValue<MenuBool>().Value)
            {
                foreach (var x in GameObjects.EnemyHeroes.Where(x => !x.IsDead
                    && x.IsValidTarget(W.Range)
                    && x.Health + 25 < Player.GetSpellDamage(x, SpellSlot.W)))
                {
                    if ((!Q.IsReady() && !x.HasBuffOfType(BuffType.Poison) && Q.Delay * 1000 + LastQ < Environment.TickCount)
                        || (Q.IsReady() && Player.GetSpellDamage(x, SpellSlot.Q) < x.Health))
                        W.Cast(x);
                }
            }
        }

        /*
        private static void TearStack()
        {

            if (!Z["misc"]["misc.itemstack"].GetValue<MenuBool>().Value
                || !Player.InFountain())
                return;

            if (Q.IsReady()
                && ((Items.HasItem(ItemData.Tear_of_the_Goddess.Id)
                     || Items.HasItem(ItemData.Archangels_Staff.Id))))
            {
                Q.Cast(Player.Position.Extend(Game.CursorPos, Q.Range));
            }
        }

         */


        private static void AutoLevel()
        {
            int qL = Player.Spellbook.GetSpell(SpellSlot.Q).Level + q;
            int wL = Player.Spellbook.GetSpell(SpellSlot.W).Level + w;
            int eL = Player.Spellbook.GetSpell(SpellSlot.E).Level + e;
            int rL = Player.Spellbook.GetSpell(SpellSlot.R).Level + r;
            if (qL + wL + eL + rL < ObjectManager.Player.Level)
            {
                int[] level = { 0, 0, 0, 0 };
                for (int i = 0; i < ObjectManager.Player.Level; i++)
                {
                    level[abilitySequence[i] - 1] = level[abilitySequence[i] - 1] + 1;
                }
                if (qL < level[0]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
                if (wL < level[1]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
                if (eL < level[2]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
                if (rL < level[3]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.R);

            }
        }
        public static void LaneClear()
        {
            if (Z["misc"]["misc.manamenagertm"].GetValue<MenuBool>().Value &&
                Z["misc"]["misc.manamenagertm.slider"].GetValue<MenuSlider>().Value > Player.ManaPercent)
                return;

            var minionCount =
                GameObjects.EnemyMinions.Where(m => m.IsValid && m.Distance(Player) < Q.Range).ToList();

            var jungleMinion = ObjectManager.Get<Obj_AI_Minion>().Where(x => x.Team == GameObjectTeam.Neutral
                                                                             && !x.IsDead
                                                                             &&
                                                                             x.Distance(ObjectManager.Player.Position) <=
                                                                             Q.Range)
                .OrderBy(x => x.MaxHealth)
                .FirstOrDefault();

            if (jungleMinion != null)
            {
                return;
            }


            /*
            MinionManager.FarmLocation QFarmLocation =
                Q.GetCircularFarmLocation(
                    MinionManager.GetMinionsPredictedPositions(MinionManager.GetMinions(Q.Range),
                        Q.Delay, Q.Width, Q.Speed,
                        Player.Position, Q.Range,
                        false, SkillshotType.SkillshotCircle), Q.Width);

            MinionManager.FarmLocation WFarmLocation =
                W.GetCircularFarmLocation
                    (MinionManager.GetMinionsPredictedPositions(MinionManager.GetMinions(W.Range),
                        W.Delay, W.Width, W.Speed,
                        Player.Position, W.Range,
                        false, SkillshotType.SkillshotCircle), W.Width);
             */

            foreach (var minion in minionCount)
            {
                if (minion == null)
                    return;

                if (Z["laneclear"]["laneclear.q"].GetValue<MenuBool>().Value
                    && Q.IsReady()
                    && Player.Mana >= Q.Instance.ManaCost + 2 * E.Instance.ManaCost)
                    Q.Cast(minion);

                /*
                var whit =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(x => x.Distance(WFarmLocation.Position) <= W.Width && !x.IsDead && x.IsEnemy);
                 */

                if (Z["laneclear"]["laneclear.w"].GetValue<MenuBool>().Value
                    && W.IsReady() &&
                    minionCount.Count() >= Z["laneclear"]["laneclear.w.restriction"].GetValue<MenuSlider>().Value)
                    W.Cast(minion);

                var emin =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(x => !x.IsDead && x.IsEnemy && E.IsInRange(x))
                        .OrderBy(x => x.Health)
                        .FirstOrDefault();

                if (Z["laneclear"]["laneclear.e"].GetValue<MenuBool>().Value && E.IsReady())
                {
                    if ((emin.HasBuffOfType(BuffType.Poison)
                        && !Z["laneclear"]["laneclear.e.lasthit"].GetValue<MenuBool>().Value)

                        || (!emin.HasBuffOfType(BuffType.Poison)
                            && !Z["laneclear"]["laneclear.e.restriction"].GetValue<MenuBool>().Value)

                        || (emin.HasBuffOfType(BuffType.Poison)
                        && Z["laneclear"]["laneclear.e.lasthit"].GetValue<MenuBool>().Value
                        && emin.Health <=
Player.GetSpellDamage(emin, SpellSlot.E)
))
                        E.CastOnUnit(emin);
                }
            }
        }


        public static void JungleClear()
        {
            var jungle =
                GameObjects.Jungle.Where(m => m.IsValid &&
                    m.Distance(Player) < Q.Range).ToList();

            if (!jungle.Any())
                return;

            var bigjungle = jungle.First();

            if (Q.IsReady() &&
                bigjungle.IsValidTarget(Q.Range))
            {
                Q.Cast(bigjungle);
            }

            if (E.IsReady()
                && bigjungle.HasBuffOfType(BuffType.Poison)
                && bigjungle.IsValidTarget(E.Range))
            {
                E.Cast(bigjungle);
            }

            if (W.IsReady()
                && bigjungle.IsValidTarget(W.Range))
            {
                W.Cast(bigjungle);
            }
        }
        public static void LastHit()
        {
            if (Z["misc"]["misc.manamenagertm"].GetValue<MenuBool>().Value
                && Z["misc"]["misc.manamenagertm.slider"].GetValue<MenuSlider>().Value > Player.ManaPercent)
                return;

            if (!Z["lasthit"]["lasthit.e"].GetValue<MenuBool>().Value || !E.IsReady())
                return;

            foreach (var min in ObjectManager.Get<Obj_AI_Minion>().Where(x =>
                E.IsInRange(x)
                && !x.IsDead
                && x.IsEnemy
                && x.Health + 5 <
Player.GetSpellDamage(x, SpellSlot.E)))
            {
                E.CastOnUnit(min);
            }
        }
    }
}