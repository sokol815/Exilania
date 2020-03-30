using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Exilania
{
    public class WorldManager
    {
        public List<WorldDefiner> available_worlds;
        public static int next_world_id = 1;
        int cur_option;

        public WorldManager()
        {
            cur_option = -1;
            available_worlds = new List<WorldDefiner>();
            if (!System.IO.Directory.Exists(@"worlds"))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(@"worlds");
                }
                catch
                {
                    Exilania.text_stream.WriteLine("Had an error... could not create the folder 'worlds'... aborting!");
                }
            }
            string[] all_files = Directory.GetFiles(@"worlds/", "*.wld");
            Array.Sort(all_files);
            StreamReader fs;
            BinaryReader r;
            for (int x = 0; x < all_files.Length; x++)
            {
                if (!all_files[x].Contains("backup"))
                {
                    fs = new System.IO.StreamReader(all_files[x]);
                    r = new System.IO.BinaryReader(fs.BaseStream);
                    available_worlds.Add(new WorldDefiner(r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadString(), r.ReadInt32(), r.ReadInt32()));
                    r.Close();
                }
            }
            available_worlds.Sort((x, y) => x.world_number.CompareTo(y.world_number));
            if (available_worlds.Count > 0)
                next_world_id = available_worlds[available_worlds.Count - 1].world_number + 1;
            else
                next_world_id = 1;
            Exilania.debug = "Next world ID: " + next_world_id;
        }

        public void update(Input input, Exilania e)
        {
            int prev_option = cur_option;
            if (input.mouse_cur_spot.X < Exilania.screen_size.X / 2 - 200 || input.mouse_cur_spot.X > Exilania.screen_size.X / 2 + 300)
            {
                cur_option = -2;
                return;
            }
            if (input.mouse_now.Y < Exilania.screen_size.Y * (.30f))
            {
                if (input.mouse_now.Y > Exilania.screen_size.Y * (.25f))
                    cur_option = -1;
                else
                    cur_option = -2;
            }
            else if (input.mouse_now.Y > Exilania.screen_size.Y * (.30f + (available_worlds.Count * .05)))
            {
                cur_option = -2;
            }
            else
            {
                float loc = (float)input.mouse_now.Y / (float)Exilania.screen_size.Y;
                loc -= .285f;
                cur_option = (int)(loc * 100f) / 5;
            }
            if (cur_option != prev_option)
            {
                Exilania.sounds.hit_block.Play(Exilania.sounds.master_volume, 0f, 0f);
            }
            if (input.left_clicked && input.mouse_now.X > -1 && input.mouse_now.Y > -1 &&
                input.mouse_now.X <= Exilania.screen_size.X && input.mouse_now.Y <= Exilania.screen_size.Y && e.IsActive)
            {
                Exilania.sounds.finish_hit_block.Play(Exilania.sounds.master_volume,0f,0f);
                input.left_clicked = false;
                switch (cur_option)
                {
                    case -2: //no option currently selected.
                        break;
                    case -1: //create new world
                        Exilania.gstate = 91;
                        Exilania.world_definition_manager.chosen_world_name = "";
                        Exilania.disable_chat = true;
                        input.key_input = "";
                        break;
                    default:
                        e.start_world("world" + available_worlds[cur_option].world_number,e.GraphicsDevice);
                        Exilania.gstate = 99;
                        break;
                }
            }
        }

        public void draw_choosing_menu(SpriteBatch s, Display d)
        {
            string add_beginning = "@00";
            s.Draw(d.planet_bkd, new Rectangle((Exilania.screen_size.X / 2) - (1920 / 2), (Exilania.screen_size.Y / 2) - (1080 / 2), 1920, 1080), new Rectangle(0, 0, 1920, 1080), Color.White);
            Vector2 size_text = d.font.MeasureString("World Menu");
            d.draw_text(s, d.font, "@03World Menu", Exilania.screen_size.X / 2 - (int)size_text.X / 2, 10, Exilania.screen_size.X);
            s.Draw(d.Exilania_title, new Rectangle(Exilania.screen_size.X - 400, (int)((float)Exilania.screen_size.Y - 100), 392, 87), new Rectangle(0, 0, 392, 87), Color.White);

            d.draw_text(s, d.middle_font, (cur_option == -1?"@08":"@00")+"<<Create New World>>", Exilania.screen_size.X / 2 - 166, (int)((float)Exilania.screen_size.Y * (.25f)), Exilania.screen_size.X / 2);
            for (int x = 0; x < available_worlds.Count; x++)
            {
                if (x == cur_option)
                    add_beginning = "@08";
                else
                    add_beginning = "@00";
                d.draw_text(s, d.middle_font, add_beginning + available_worlds[x].ToString() + (Exilania.cur_open_world == "world" + available_worlds[x].world_number ? " @04*Running*" : ""), Exilania.screen_size.X / 2 - 166, (int)((float)Exilania.screen_size.Y * (.30f + ((float)x * .05f))), Exilania.screen_size.X / 2);
            }
        }

       
    }

    public class WorldDefiner
    {
        public string world_name;
        public int seed_number;
        public string file_name; //defaults to World1.wld, World2.wld... etc. backups are located at World1_backup.wld, World2_backup.wld... etc.
        public int world_number;
        public int unique_id;
        public int width;
        public int height;

        public WorldDefiner()
        {

        }

        public WorldDefiner(int pnumber, int punique_id, int seed, string name, int pwidth, int pheight)
        {
            world_number = pnumber;
            unique_id = punique_id;
            seed_number = seed;
            world_name = name;
            width = pwidth;
            height = pheight;
        }

        public override string ToString()
        {
            return world_number + ". @03" + world_name + " @00(" + width + " x " + height + ")";
        }
    }
}
