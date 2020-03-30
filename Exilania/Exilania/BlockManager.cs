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
    public class BlockManager
    {
        public List<BlockData> blocks;
        /// <summary>
        /// custom_items gets populated when a new item is built from pieces that do not exist outside of the pre-built items.
        /// the int identifier for an item is each of it's piece id's in order with a . separating them from which a hash code is generated.
        /// this will contain the image of the item so it can be drawn in-game.
        /// </summary>
        public Dictionary<int, Texture2D> custom_item_images;

        public BlockManager()
        {
            blocks = new List<BlockData>();
            custom_item_images = new Dictionary<int, Texture2D>();

            if (System.IO.File.Exists(@"blocks.txt"))
            {
                System.IO.StreamReader r = new System.IO.StreamReader(@"blocks.txt");
                string line = "";
                BlockData p = new BlockData();
                bool cont = true;
                while (cont)
                {
                    line = r.ReadLine();
                    if (line == "" || line[0] == '#')
                    {
                        //skip this line
                    }
                    else
                    {
                        string[] items = line.Split(':');
                        switch (items[0].ToLower())
                        {
                            case "block":
                                if (p.name == "")
                                {
                                    p.name = items[1].Trim();
                                }
                                else
                                {
                                    blocks.Add(p);
                                    Exilania.text_stream.WriteLine("Block '" + p.name + "' Loaded.");
                                    p = new BlockData();
                                    p.name = items[1].Trim();
                                }
                                break;
                            case "block_group":
                                p.block_group = sbyte.Parse(items[1]);
                                break;
                            case "background":
                                p.background = bool.Parse(items[1]);
                                break;
                            case "platform":
                                p.platform = bool.Parse(items[1]);
                                break;
                            case "transparent":
                                p.transparent = bool.Parse(items[1]);
                                break;
                            case "place_wall":
                                p.place_wall = bool.Parse(items[1]);
                                break;
                            case "passable":
                                p.passable = bool.Parse(items[1]);
                                break;
                            case "light_source":
                                if (!items[1].Contains("null"))
                                {
                                    items = items[1].Split(',');
                                    p.light_source = new byte[] { byte.Parse(items[0]),byte.Parse(items[1]),byte.Parse(items[2]) };
                                }
                                break;
                            case "liquid_id":
                                p.liquid_id = byte.Parse(items[1]);
                                break;
                            case "block_image_use":
                                p.block_image_use = byte.Parse(items[1]);
                                break;
                            case "image_pointers":
                                items = items[1].Split(',');
                                p.image_pointers = new int[items.Length];
                                for (int x = 0; x < items.Length; x++)
                                {
                                    p.image_pointers[x] = int.Parse(items[x]);
                                }
                                break;
                            case "random_tiles":
                                p.random_tiles = Boolean.Parse(items[1]);
                                break;
                            case "background_transparent":
                                p.bkd_transparent = Boolean.Parse(items[1]);
                                break;
                            case "lighter_background":
                                p.lighter_background = Boolean.Parse(items[1]);
                                break;
                            case "map_color":
                                items = Acc.script_remove_outer_parentheses(items[1]).Split(',');
                                p.map_represent = new Color(byte.Parse(items[0]),byte.Parse(items[1]),byte.Parse(items[2]));
                                break;
                            case "is_hangar": p.is_hangar = Boolean.Parse(items[1]);
                                break;
                            default:
                                Exilania.text_stream.WriteLine("UNHANDLED type " + items[0]);
                                break;
                        }
                    }
                    if (r.EndOfStream)
                    {
                        blocks.Add(p);
                        Exilania.text_stream.WriteLine("Block '" + p.name + "' Loaded.");
                        cont = false;
                    }
                }
                r.Close();
            }
            else
            {
                Exilania.text_stream.Write("ERROR! No blocks.txt file.");
            }

        }

        public sbyte get_block_by_name(string name)
        {
            name = name.ToLower();
            for (int x = 0; x < blocks.Count; x++)
            {
                if (blocks[x].name.ToLower() == name)
                {
                    return (sbyte)x;
                }
            }
            return -1;
        }
    }
}
