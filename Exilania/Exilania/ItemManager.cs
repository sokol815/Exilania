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
    public class ItemManager
    {
        public List<ItemPiece> item_pieces;
        /// <summary>
        /// custom_items gets populated when a new item is built from pieces that do not exist outside of the pre-built items.
        /// the int identifier for an item is each of it's piece id's in order with a . separating them from which a hash code is generated.
        /// this will contain the image of the item so it can be drawn in-game.
        /// </summary>
        public Dictionary<int, Texture2D> custom_item_images;

        public ItemManager()
        {
            item_pieces = new List<ItemPiece>();
            custom_item_images = new Dictionary<int, Texture2D>();

            if (System.IO.File.Exists(@"item_pieces.txt"))
            {
                System.IO.StreamReader r = new System.IO.StreamReader(@"item_pieces.txt");
                string line = "";
                ItemPiece p = new ItemPiece();
                bool cont = true;
                while (cont)
                {
                    line = r.ReadLine();
                    if (line[0] == '#')
                    {
                        //skip this line
                    }
                    else
                    {
                        string[] items = line.Split(':');
                        switch (items[0])
                        {
                            case "PIECE":
                                if(p.name == "")
                                {
                                    p.name = items[1];
                                }
                                else
                                {
                                    item_pieces.Add(p);
                                    Exilania.text_stream.WriteLine("Item Piece '" + p.name+"' Loaded.");
                                    p = new ItemPiece();
                                    p.name = items[1];
                                }
                                break;
                            case "IMAGE":
                                items = Acc.get_inner_parenthesis(items[1]).Split(',');
                                p.image = new Rectangle(Int32.Parse(items[0]), Int32.Parse(items[1]), Int32.Parse(items[2]), Int32.Parse(items[3]));
                                break;
                            case "HAND-ATTACH":
                                items = items[1].Split(',');
                                p.hand_attach_point = new Point(Int32.Parse(items[0]), Int32.Parse(items[1]));
                                p.has_hand_attach_point = true;
                                break;
                            case "ATTACH-POINT":
                                items = Acc.get_inner_parenthesis(items[1]).Split(',');
                                p.item_attach_points.Add(new Point(Int32.Parse(items[0]), Int32.Parse(items[1])));
                                break;
                            case "CLICK":
                                p.click_action += items[1];
                                break;
                            case "DATA":
                                items = items[1].Split('=');
                                p.data.Add(items[0], items[1]);
                                break;
                            case "BREAK-BLOCK":
                                items = items[1].Split(';');
                                for (int x = 0; x < items.Length; x++)
                                {
                                    if (items[x] != "")
                                    {
                                        string[] mitems = items[x].Split('=');

                                        int key = BlockData.block_enum[mitems[0]];
                                        int value = Int32.Parse(mitems[1]);
                                        if (p.break_block.ContainsKey(key))
                                        {
                                            p.break_block[key] = Math.Min(p.break_block[key], value);
                                        }
                                        else
                                        {
                                            p.break_block.Add(key, value);
                                        }
                                    }
                                }
                                if (items.Length > 0)
                                {
                                    p.break_block.Add(BlockData.block_enum["SINGLE"], 0);
                                }
                                break;
                            case "MATERIAL":
                                items = items[1].Split(';');
                                for (int x = 0; x < items.Length; x++)
                                {
                                    if (items[x] != "")
                                    {
                                        string[] mitems = items[x].Split('=');

                                        string key = mitems[0];
                                        int value = Int32.Parse(mitems[1]);
                                        if (p.materials.ContainsKey(key))
                                        {
                                            p.materials[key] = Math.Min(p.materials[key], value);
                                        }
                                        else
                                        {
                                            p.materials.Add(key, value);
                                        }
                                    }
                                }
                                break;
                            case "CRAFT-REQUIRE":
                                if(items[1].Trim().ToLower()!="none")
                                    p.craft_require = items[1].Trim().ToLower();
                                break;
                            case "COMPLEXITY":
                                p.complexity = int.Parse(items[1]);
                                break;
                            case "WORTH":
                                p.worth = int.Parse(items[1]);
                                break;
                            default:
                                Exilania.text_stream.WriteLine("UNHANDLED type " + items[0]);
                                break;
                        }
                    }
                    if (r.EndOfStream)
                    {
                        item_pieces.Add(p);
                        Exilania.text_stream.WriteLine("Item Piece '" + p.name + "' Loaded.");
                        cont = false;
                    }
                }
                r.Close();
            }
            else
            {
                Exilania.text_stream.Write("ERROR! NO ITEM PIECES DEFINING items....");
            }

        }

    }
}
