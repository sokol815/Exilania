using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Exilania
{
    public class BlockData
    {
        public static Dictionary<string, int> block_enum;
        public string name;
        public int[] image_pointers;
        public bool background;
        public bool platform;
        public bool transparent;
        public bool bkd_transparent;
        public bool passable;
        public bool random_tiles;
        public bool lighter_background;
        public bool is_hangar;
        public byte liquid_id;
        public byte[] light_source;
        public int block_image_use;
        /// <summary>
        /// empty = -1; brick = 0; dirt/grass = 1; stone1 = 2; wood platform = 3; 4 = furniture/misc; 5 = liquids; 6 = wood; 7 = level 2 stone; 8 = metals; 9 = snow
        /// </summary>
        public sbyte block_group;
        public bool place_wall;
        public Color map_represent;

        public BlockData()
        {
            name = "";
            background = false;
            platform = false;
            transparent = false;
            bkd_transparent = false;
            place_wall = true;
            random_tiles = false;
            lighter_background = false;
            passable = false;
            is_hangar = false;
            light_source = null;
            block_group = 0;
            liquid_id = 0;
            block_image_use = 0;
            image_pointers = new int[0];
            map_represent = Color.White;
        }

        public static void enum_blocks()
        {
            block_enum = new Dictionary<string, int>();
            block_enum.Add("STONE1", 2);
            block_enum.Add("BRICK1", 0);
            block_enum.Add("PLATFORM", 3);
            block_enum.Add("DIRT1", 1);
            block_enum.Add("MISC", 4);
            block_enum.Add("LIQUID1", 5);
            block_enum.Add("WOOD", 6);
            block_enum.Add("STONE2", 7);
            block_enum.Add("METAL1", 8);
            block_enum.Add("SINGLE", 9);
            block_enum.Add("SAND", 10);
        }

        public void hover_over(Point mouse_loc, SpriteBatch s, Display d, World w)
        {
            Vector2 size = d.small_font.MeasureString(name);
            Vector2 size2 = new Vector2();//d.small_font.MeasureString("Owned by: " + owner);
            //d.draw_bounding_box(s, new Rectangle(mouse_loc.X + 35, mouse_loc.Y - 50, (int)Math.Max(size.X, size2.X) + 14, 10));
            d.draw_text(s, d.small_font, "@05" + name, mouse_loc.X + 42 + (int)Math.Max(size.X / 2, size2.X / 2) - (int)(size.X / 2), mouse_loc.Y, 500);
            //d.draw_text(s, d.small_font, "@00Owned by: " + owner, mouse_loc.X + 42 + (int)Math.Max(size.X / 2, size2.X / 2) - (int)(size2.X / 2), mouse_loc.Y - 26, 500);
        }

        public static void export_block_data(List<BlockData> blks)
        {
            /*
             * background = false;
            platform = false;
            transparent = false;
            place_wall = true;
            passable = false;
            light_source = null;
            block_group = 0;
            int b = 0;
            liquid_id = 0;
            block_image_use = 0;
             * */
            System.IO.StreamWriter text_stream = new System.IO.StreamWriter(@"logs/blocks" + DateTime.Now.Ticks.ToString() + ".txt");
            text_stream.AutoFlush = true;
            for (int x = 0; x < blks.Count; x++)
            {
                text_stream.WriteLine("#NEXT BLOCK");
                text_stream.WriteLine("#");
                text_stream.WriteLine("BLOCK:" + blks[x].name);
                text_stream.WriteLine("BLOCK_GROUP:" + blks[x].block_group);
                text_stream.WriteLine("BACKGROUND:" + blks[x].background.ToString());
                text_stream.WriteLine("PLATFORM:" + blks[x].platform.ToString());
                text_stream.WriteLine("TRANSPARENT:" + blks[x].transparent.ToString());
                text_stream.WriteLine("PLACE_WALL:" + blks[x].place_wall.ToString());
                text_stream.WriteLine("PASSABLE:" + blks[x].passable.ToString());
                if (blks[x].light_source != null)
                {
                    text_stream.WriteLine("LIGHT_SOURCE:" + blks[x].light_source[0] + "," + blks[x].light_source[1] + "," + blks[x].light_source[2]);
                }
                else
                {
                    text_stream.WriteLine("LIGHT_SOURCE:null");
                }
                text_stream.WriteLine("LIQUID_ID:" + blks[x].liquid_id);
                text_stream.WriteLine("BLOCK_IMAGE_USE:" + blks[x].block_image_use);
                string img_ptrs = "";
                for (int i = 0; i < blks[x].image_pointers.Length; i++)
                {
                    if (i == 0)
                        img_ptrs = blks[x].image_pointers[i].ToString();
                    else
                        img_ptrs += "," + blks[x].image_pointers[i].ToString();
                }
                text_stream.WriteLine("IMAGE_POINTERS:" + img_ptrs);
            }
            text_stream.Close();
        }
    }
}
