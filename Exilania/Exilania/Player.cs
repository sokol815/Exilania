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
using Lidgren.Network;

namespace Exilania
{
    public class Player
    {
        public static int max_complexity = 1000;
        public int credits;
        public Actor avatar;
        /// <summary>
        /// this is used for networking... not stored into files or anything, though.
        /// </summary>
        public bool is_player_empty = false;

        public Player()
        {
        }

        public Player(Actor a, int plevel, int pcredits, int pexperience, int pcomplexity)
        {
            avatar = a;
            avatar.items = new Inventory(true);
            Item temp_item;
            temp_item = new Item();
            temp_item.add_piece(Exilania.item_manager.item_pieces[5], 5, -1, 0, 0, 0);
            temp_item.construct_item("Pick-Axe Head");
            a.items.pickup_item(temp_item);
            credits = pcredits;
        }

        public Player(System.IO.BinaryReader r)
        {
            credits = r.ReadInt32();
            avatar = new Actor(r);
        }

        public Player(NetIncomingMessage r)
        {
            is_player_empty = r.ReadBoolean();
            credits = r.ReadInt32();
            avatar = new Actor(r);
        }



        public void send_player(NetOutgoingMessage m)
        {
            m.Write(is_player_empty);
            m.Write(credits);
            avatar.send_actor(m);
        }

        public void write_player(System.IO.BinaryWriter w)
        {
            w.Write(credits);
            avatar.write_actor(w);
        }

        public void get_input(float time, Input i, Display d, World w, Exilania e)
        {
            avatar.input.update_input(time, i, d, w, e, true);
            avatar.items.input(avatar, d,w.top_left);
        }

        public void draw_player(SpriteBatch s, Display d, World w)
        {
            avatar.draw_actor(s, 1f, d);
        }


        public override string ToString()
        {
            return avatar.name + ":   " + avatar.stats.level + "   Complexity: " + avatar.stats.complexity; 
        }
    }
}
