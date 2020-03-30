using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Exilania
{
    public class ActorStats
    {
        public Container life;
        public FloatContainer breath;
        public Container power;
        public bool share_power;
        public bool pvp = true;
        public int team;
        public Container ammo;
        public int experience;
        public Container experience_to_level;
        public int level; //not written, derived from experience
        public int complexity;
        public string armor; //something like 1d4+1
        public float jump_speed;
        public float run_speed;
        public float jump_max_time;
        public float jump_cur_time; //not written
        public bool can_swim;
        public bool boyant;
        public bool jump_can_resume;
        public float life_regen;
        public float accum_life_regen; //not written
        public float ammo_regen;
        public float accum_ammo_regen; //not written
        public float energy_regen;
        public float accum_energy_regen; //not written
        public float water_jump_proficiency;
        public int max_safe_fall;


        public ActorStats()
        {
            pvp = true;
            team = -1;
            life = new Container(50, 50);
            breath = new FloatContainer(15f, 15f);
            power = new Container(100, 100);
            share_power = true;
            ammo = new Container(0, 0);
            experience = 0;
            level = 1 + (int)Math.Pow(experience / 5, .5f);
            experience_to_level = new Container(0, (int)Math.Pow((level), 2) * 5);
            complexity = 12;
            armor = "0";
            jump_speed = 350f;
            run_speed = 350f;
            jump_max_time = 0.20f;
            jump_cur_time = 0f; //not written
            can_swim = true;
            boyant = false;
            jump_can_resume = false;
            life_regen = 0f;
            accum_life_regen = 0f;
            ammo_regen = 0f;
            accum_ammo_regen = 0f;
            energy_regen = 0f;
            accum_energy_regen = 0f;
            water_jump_proficiency = 0.55f;
            max_safe_fall = 500;
            //(int)Math.Pow((plevel-1),2)*5; <-- formula for experience required to reach level plevel.
        }

        public ActorStats(System.IO.BinaryReader r)
        {
            life = new Container(r);
            breath = new FloatContainer(r);
            power = new Container(r);
            share_power = r.ReadBoolean();
            ammo = new Container(r);
            experience = r.ReadInt32();
            level = 1 + (int)Math.Pow(experience / 5, .5f);
            if(level > 1)
                experience_to_level = new Container(experience - ((int)Math.Pow((level-1), 2) * 5),((int)Math.Pow((level), 2) * 5) - (int)Math.Pow((level-1), 2) * 5);
            else
                experience_to_level = new Container(experience, (int)Math.Pow((level), 2) * 5);
            complexity = r.ReadInt32();
            armor = r.ReadString();
            jump_speed = r.ReadSingle();
            run_speed = r.ReadSingle();
            jump_max_time = r.ReadSingle();
            jump_cur_time = 0f;
            can_swim = r.ReadBoolean();
            boyant = r.ReadBoolean();
            jump_can_resume = r.ReadBoolean();
            life_regen = r.ReadSingle();
            accum_life_regen = 0f;
            ammo_regen = r.ReadSingle();
            accum_ammo_regen = 0f;
            energy_regen = r.ReadSingle();
            accum_energy_regen = 0f;
            water_jump_proficiency = r.ReadSingle();
            max_safe_fall = r.ReadInt32();
        }

        public ActorStats(Lidgren.Network.NetIncomingMessage r)
        {
            life = new Container(r);
            breath = new FloatContainer(r);
            power = new Container(r);
            share_power = r.ReadBoolean();
            ammo = new Container(r);
            experience = r.ReadInt32();
            level = 1 + (int)Math.Pow(experience / 5, .5f);
            if (level > 1)
                experience_to_level = new Container(experience - ((int)Math.Pow((level-1), 2) * 5), ((int)Math.Pow((level), 2) * 5) - (int)Math.Pow((level-1), 2) * 5);
            else
                experience_to_level = new Container(experience, (int)Math.Pow((level), 2) * 5);
            complexity = r.ReadInt32();
            armor = r.ReadString();
            jump_speed = r.ReadSingle();
            run_speed = r.ReadSingle();
            jump_max_time = r.ReadSingle();
            jump_cur_time = 0f;
            can_swim = r.ReadBoolean();
            boyant = r.ReadBoolean();
            jump_can_resume = r.ReadBoolean();
            life_regen = r.ReadSingle();
            accum_life_regen = 0f;
            ammo_regen = r.ReadSingle();
            accum_ammo_regen = 0f;
            energy_regen = r.ReadSingle();
            accum_energy_regen = 0f;
            water_jump_proficiency = r.ReadSingle();
            max_safe_fall = r.ReadInt32();
        }

        public void write_ActorStats(System.IO.BinaryWriter w)
        {
            life.write_container(w);
            breath.write_FloatContainer(w);
            power.write_container(w);
            w.Write(share_power);
            ammo.write_container(w);
            w.Write(experience);
            w.Write(complexity);
            w.Write(armor);
            w.Write(jump_speed);
            w.Write(run_speed);
            w.Write(jump_max_time);
            w.Write(can_swim);
            w.Write(boyant);
            w.Write(jump_can_resume);
            w.Write(life_regen);
            w.Write(ammo_regen);
            w.Write(energy_regen);
            w.Write(water_jump_proficiency);
            w.Write(max_safe_fall);
        }

        public void send_ActorStats(Lidgren.Network.NetOutgoingMessage w)
        {
            life.send_container(w);
            breath.send_FloatContainer(w);
            power.send_container(w);
            w.Write(share_power);
            ammo.send_container(w);
            w.Write(experience);
            w.Write(complexity);
            w.Write(armor);
            w.Write(jump_speed);
            w.Write(run_speed);
            w.Write(jump_max_time);
            w.Write(can_swim);
            w.Write(boyant);
            w.Write(jump_can_resume);
            w.Write(life_regen);
            w.Write(ammo_regen);
            w.Write(energy_regen);
            w.Write(water_jump_proficiency);
            w.Write(max_safe_fall);
        }

        public void get_experience_crafting(int complexity, Actor a)
        {
            complexity -= level;
            if (complexity > 0)
            {
                experience += complexity;
                experience_to_level.change_val(complexity);
                if (experience_to_level.cur_val == experience_to_level.max_val)
                {
                    level++;
                    experience_to_level = new Container(experience - ((int)Math.Pow((level-1), 2) * 5),((int)Math.Pow((level), 2) * 5) - (int)Math.Pow((level-1), 2) * 5);
                    Exilania.display.fading_text.Add(new FadeText("@05Level Up!", 2000,
                            (int)a.world_loc.X + 18, (int)a.world_loc.Y - 48, true, false));
                }
               // Exilania.debug = "Level: " + level + " EXP: " + experience + " Stats " + experience_to_level.ToString();
            }
        }

        public void draw_stats(SpriteBatch s)
        {
            string drawing = "";
            Vector2 size = new Vector2();

            life.draw_at(s, new Point(30, 10), 164, 20, 0);
            s.Draw(Exilania.display.sprites, new Rectangle(3, 6, 24, 24), Exilania.display.frames[629], Color.White);
            drawing = life.ToString();
            size = Exilania.display.small_font.MeasureString(drawing);
            drawing = "@00" + drawing;
            Exilania.display.draw_text_with_outline(s, Exilania.display.small_font, drawing, 114 - (int)(size.X/2), 8, 400, AccColors.Black);

            int start_y = 35; 
            if (breath.cur_val != breath.max_val)
            {
                breath.draw_at(s, new Point(30, start_y), 164, 20, 2);
                s.Draw(Exilania.display.sprites, new Rectangle(3, start_y - 4, 24, 24), Exilania.display.frames[661], Color.White);
                drawing = breath.ToString();
                size = Exilania.display.small_font.MeasureString(drawing);
                drawing = "@00" + drawing;
                Exilania.display.draw_text_with_outline(s, Exilania.display.small_font, drawing, 114 - (int)(size.X / 2), start_y - 2, 400, AccColors.Black);
                start_y += 25;
            }

            power.draw_at(s, new Point(30, start_y), 164, 20, 1);
            s.Draw(Exilania.display.sprites, new Rectangle(3, start_y-4, 24, 24), Exilania.display.frames[890], Color.White);
            drawing = power.ToString();
            size = Exilania.display.small_font.MeasureString(drawing);
            drawing = "@00" + drawing;
            Exilania.display.draw_text_with_outline(s, Exilania.display.small_font, drawing, 114 - (int)(size.X / 2), start_y-2, 400, AccColors.Black);
            start_y += 25;

            experience_to_level.draw_at(s, new Point(30, start_y), 164, 20, 3);
            s.Draw(Exilania.display.sprites, new Rectangle(3, start_y-4, 24, 24), Exilania.display.frames[889], Color.White);
            drawing = experience_to_level.ToString();
            size = Exilania.display.small_font.MeasureString(drawing);
            drawing = "@00" + drawing;
            Exilania.display.draw_text_with_outline(s, Exilania.display.small_font, drawing, 114 - (int)(size.X / 2), start_y-2, 400, AccColors.Black);
        }
    }
}
