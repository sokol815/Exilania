using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Lidgren.Network;


namespace Exilania
{

    public class Furniture
    {
        public string name;
        public ushort furniture_id;
        public Point top_left;
        public Point place_origin;
        public bool[] flags;
        public byte[] light_source;
        public string owner;
        public string message;
        public byte state;
        public int power_storage;
        public List<ItemConnector> connections;
        public int setTimeout;
        public byte share_power;
        public List<Facet> facets;

        public static bool running_proximity = false;
        public static bool calced_enter_proximity = false;
        public static bool calced_leave_proximity = false;
        public static bool did_enter_proximity = false;
        public static bool did_leave_proximity = false;
        public static long dtime = 0;

        public Furniture()
        {
            name = "";
            furniture_id = ushort.MaxValue;
            top_left = new Point();
            place_origin = new Point();
            flags = new bool[6];
            flags[(int)FFLAGS.PASSABLE] = true;
            flags[(int)FFLAGS.BREAK_BELOW] = true;
            owner = "";
            message = "";
            state = 0;
            power_storage = 0;
            connections = new List<ItemConnector>();
            setTimeout = 0;
            share_power = 255;
            facets = new List<Facet>();
        }

        public Furniture(System.IO.BinaryReader r)
        {
            furniture_id = r.ReadUInt16();
            name = Exilania.furniture_manager.furniture[furniture_id].name;
            top_left = new Point(r.ReadUInt16(), r.ReadUInt16());
            place_origin = new Point(r.ReadUInt16(), r.ReadUInt16());
            flags = new bool[] { r.ReadBoolean(), r.ReadBoolean(), r.ReadBoolean(), false, r.ReadBoolean(),r.ReadBoolean() };
            if (r.ReadByte() == 3)
                light_source = new byte[3] { r.ReadByte(), r.ReadByte(), r.ReadByte() };
            owner = r.ReadString();
            message = r.ReadString();
            state = r.ReadByte();
            power_storage = r.ReadInt32();
            connections = new List<ItemConnector>();
            int num_connections = r.ReadInt32();
            for (int x = 0; x < num_connections; x++)
            {
                connections.Add(new ItemConnector(r));
            }
            setTimeout = r.ReadInt32();
            share_power = r.ReadByte();
            facets = new List<Facet>();
            foreach (var t in Exilania.furniture_manager.furniture[furniture_id].facets)
            {
                facets.Add(new Facet(furniture_id, top_left, facets.Count, t));
            }
        }

        public Furniture(NetIncomingMessage r, int new_world_id)
        {
            furniture_id = r.ReadUInt16();
            name = Exilania.furniture_manager.furniture[furniture_id].name;
            top_left = new Point(r.ReadUInt16(), r.ReadUInt16());
            place_origin = new Point(r.ReadUInt16(), r.ReadUInt16());
            flags = new bool[] { r.ReadBoolean(), r.ReadBoolean(), r.ReadBoolean(), false, r.ReadBoolean(),r.ReadBoolean()};
            if (r.ReadByte() == 3)
                light_source = new byte[3] { r.ReadByte(), r.ReadByte(), r.ReadByte() };
            owner = r.ReadString();
            message = r.ReadString();
            state = r.ReadByte();
            power_storage = r.ReadInt32();
            connections = new List<ItemConnector>();
            int num_connections = r.ReadInt32();
            for (int x = 0; x < num_connections; x++)
            {
                connections.Add(new ItemConnector(r));
            }
            setTimeout = r.ReadInt32();
            share_power = r.ReadByte();
            facets = new List<Facet>();
            foreach (var t in Exilania.furniture_manager.furniture[furniture_id].facets)
            {
                facets.Add(new Facet(furniture_id, top_left, facets.Count, t));
            }
        }

        public Furniture(Point loc, string powner, FurnitureData orig, World w)
        {
            top_left = loc;
            place_origin = new Point(loc.X, loc.Y + orig.image_frames[0].height - 1);
            name = orig.name;
            furniture_id = orig.furniture_id;
            flags = new bool[6];
            orig.flags.CopyTo(flags, 0);
            owner = powner;
            message = orig.message;
            state = 0;
            power_storage = orig.power_storage;
            if (orig.light_source != null)
            {
                light_source = new byte[3];
                orig.light_source.CopyTo(light_source, 0);
            }
            connections = new List<ItemConnector>();
            setTimeout = orig.timeout;
            share_power = orig.share_power;
            facets = new List<Facet>();
            foreach (var t in Exilania.furniture_manager.furniture[furniture_id].facets)
            {
                facets.Add(new Facet(furniture_id, top_left, facets.Count, t));
            }
        }

        public void write_furniture(System.IO.BinaryWriter w)
        {
            w.Write((ushort)furniture_id);
            w.Write((ushort)top_left.X);
            w.Write((ushort)top_left.Y);
            w.Write((ushort)place_origin.X);
            w.Write((ushort)place_origin.Y);
            w.Write(flags[0]);
            w.Write(flags[1]);
            w.Write(flags[2]);
            w.Write(flags[4]);
            w.Write(flags[5]);
            if (light_source == null)
            {
                w.Write((byte)0);
            }
            else
            {
                w.Write((byte)3);
                w.Write(light_source[0]);
                w.Write(light_source[1]);
                w.Write(light_source[2]);
            }
            w.Write(owner);
            w.Write(message);
            w.Write((byte)state);
            w.Write(power_storage);
            w.Write(connections.Count);
            for (int x = 0; x < connections.Count; x++)
            {
                connections[x].write_connector(w);
            }
            w.Write(setTimeout);
            w.Write(share_power);
        }

        public void send_furniture(NetOutgoingMessage w)
        {
            w.Write((ushort)furniture_id);
            w.Write((ushort)top_left.X);
            w.Write((ushort)top_left.Y);
            w.Write((ushort)place_origin.X);
            w.Write((ushort)place_origin.Y);
            w.Write(flags[0]);
            w.Write(flags[1]);
            w.Write(flags[2]);
            w.Write(flags[4]);
            w.Write(flags[5]);
            if (light_source == null)
            {
                w.Write((byte)0);
            }
            else
            {
                w.Write((byte)3);
                w.Write(light_source[0]);
                w.Write(light_source[1]);
                w.Write(light_source[2]);
            }
            w.Write(owner);
            w.Write(message);
            w.Write((byte)state);
            w.Write(power_storage);
            w.Write(connections.Count);
            for (int x = 0; x < connections.Count; x++)
            {
                connections[x].send_connector(w);
            }
            w.Write(setTimeout);
            w.Write(share_power);
        }

        public void hover_over(Point mouse_loc, SpriteBatch s, Display d, World w,bool draw_text)
        {
            Point from = get_rect().Center;
            bool player_conn = false;
            for (int x = 0; x < connections.Count; x++)
            {
                Point to = new Point();
                if (connections[x].target_type == TargetType.Player)
                {
                    to = new Point((int)w.players[connections[x].target_id].avatar.world_loc.X,(int)w.players[connections[x].target_id].avatar.world_loc.Y);
                    player_conn = true;
                }
                else if (connections[x].target_type == TargetType.Furniture)
                {
                    to = w.furniture[connections[x].target_id].get_rect().Center;
                    player_conn = false;
                }
                else
                    continue;
                float angle = (float)Math.Atan2(to.Y - from.Y, to.X - from.X);
                double length = Acc.get_distance(from, to);
                if(!player_conn)
                    s.Draw(d.sprites, new Rectangle(from.X - (int)w.top_left.X, from.Y - (int)w.top_left.Y, (int)length, 1), new Rectangle(440, 552, 3, 1), Color.White, angle, new Vector2(0, 1), SpriteEffects.None, 0);
                else
                    s.Draw(d.sprites, new Rectangle(from.X - (int)w.top_left.X, from.Y - (int)w.top_left.Y, (int)length, 1), new Rectangle(446, 552, 3, 1), Color.White, angle, new Vector2(0, 1), SpriteEffects.None, 0);
                //d.draw_text(s,d.small_font, "@05" + connections[x].ToString(), mouse_loc.X + 30, mouse_loc.Y + 20 + x * 20,1000);
            }
            if (draw_text)
            {
                if (Exilania.furniture_manager.furniture[furniture_id].max_power_storage > 0)
                {
                    d.draw_text(s, d.small_font, "@00P: @05" + power_storage + " @00/ @05" + Exilania.furniture_manager.furniture[furniture_id].max_power_storage + " @00(" + owner + ")",
                        mouse_loc.X, mouse_loc.Y, 500);
                }
                else
                {
                    d.draw_text(s, d.small_font, "@00Owned by: " + owner, mouse_loc.X, mouse_loc.Y, 500);
                }
            }
        }

        public Rectangle get_rect()
        {
            return new Rectangle(top_left.X * 24, top_left.Y * 24, Exilania.furniture_manager.furniture[furniture_id].image_frames[state].width * 24, 
                Exilania.furniture_manager.furniture[furniture_id].image_frames[state].height * 24);
        }

        public void connection_integrity(World w)
        {
            bool delete_conn = false;
            for (int x = 0; x < connections.Count; x++)
            {
                delete_conn = false;
                switch (connections[x].target_type)
                {
                    case TargetType.Furniture:
                        if (w.furniture.Count <= connections[x].target_id || Acc.get_distance(w.furniture[connections[x].target_id].get_rect().Center, get_rect().Center) > CollisionHashTable.world_size_cell
                            || w.furniture[connections[x].target_id].flags[(int)FFLAGS.EMPTY])
                            delete_conn = true;
                        break;
                    case TargetType.Player:
                        if (w.players.Count <= connections[x].target_id || Acc.get_distance(w.players[connections[x].target_id].avatar.world_loc, get_rect().Center) > CollisionHashTable.world_size_cell)
                            delete_conn = true;
                        break;
                    case TargetType.NPC:
                        break;
                    case TargetType.Projectile:
                        break;
                    case TargetType.World:
                        break;
                }
                if (delete_conn)
                {
                    connections.RemoveAt(x);
                    x--;
                }
            }
        }

        public int get_frame(Point draw_loc, World w)
        {
            int x = w.wraparound_x(draw_loc.X - top_left.X);
            int y = draw_loc.Y - top_left.Y;
            int width = Exilania.furniture_manager.furniture[furniture_id].image_frames[state].width;
            int height = Exilania.furniture_manager.furniture[furniture_id].image_frames[state].height;
            return Exilania.furniture_manager.furniture[furniture_id].image_frames[state].images[x + (y * width)];
        }

        public void update_furniture(World w)
        {
            connection_integrity(w);
            if (Exilania.furniture_manager.furniture[furniture_id].actions_proximity != "")
            {
                //List<int> players_close = w.collision_table.get_players_in_range(get_rect().Center, 500, w);
                string outer_instruc = "";
                string text_run = Exilania.furniture_manager.furniture[furniture_id].actions_proximity;
                if (Acc.script_has_outer_parenthesis(text_run))
                {
                    outer_instruc = Acc.script_remove_content_of_outer_parenthesis(text_run);
                    text_run = Acc.script_remove_outer_parentheses(text_run);
                }
                running_proximity = true;
                calced_enter_proximity = false;
                calced_leave_proximity = false;
                did_enter_proximity = false;
                did_leave_proximity = false;
                run_scripting(null, w, outer_instruc, text_run);
                running_proximity = false;
                
            }
            if (Exilania.furniture_manager.furniture[furniture_id].actions_power != "")
            {
                string outer_instruc = "";
                string text_run = Exilania.furniture_manager.furniture[furniture_id].actions_power;
                if (Acc.script_has_outer_parenthesis(text_run))
                {
                    outer_instruc = Acc.script_remove_content_of_outer_parenthesis(text_run);
                    text_run = Acc.script_remove_outer_parentheses(text_run);
                }
                run_scripting(null, w, outer_instruc, text_run);
            }
            if (Exilania.furniture_manager.furniture[furniture_id].actions_time != "" && setTimeout != 0 && dtime % setTimeout == 0)
            {
                string outer_instruc = "";
                string text_run = Exilania.furniture_manager.furniture[furniture_id].actions_time;
                if (Acc.script_has_outer_parenthesis(text_run))
                {
                    outer_instruc = Acc.script_remove_content_of_outer_parenthesis(text_run);
                    text_run = Acc.script_remove_outer_parentheses(text_run);
                }
                run_scripting(null, w, outer_instruc, text_run);
            }
           
        }

        public void update_facets(World w)
        {
            if (facets.Count > 0)
            {
                foreach (var t in facets)
                {
                    t.update_facet(w, this);
                }
            }
        }

        public bool run_click_function(Actor a, World w)
        {
            string outer_instruc = "";
            string text_run = Exilania.furniture_manager.furniture[furniture_id].actions_click;
            if (Acc.script_has_outer_parenthesis(text_run))
            {
                outer_instruc = Acc.script_remove_content_of_outer_parenthesis(text_run);
                text_run = Acc.script_remove_outer_parentheses(text_run);
            }

            return run_scripting(a, w, outer_instruc, text_run);
        }


        public bool run_scripting(Actor a, World w, string outer_instruc, string text_run)
        {
            bool cont = true;
            if (outer_instruc != "") //these are pre-conditions, only continue if true
            {
                string[] items = outer_instruc.Split(' ');
                for (int x = 0; x < items.Length; x++)
                {
                    if (cont && items[x].Length > 0)
                    {
                        cont = check_condition(a, w, items[x]);
                        if (!cont)
                        {
                            switch (items[x])
                            {
                                case "actorisfriend":
                                    Exilania.display.add_message("@00You are not affiliated with this " + name);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            if (!cont)
                return false;
            string[] triggers = text_run.Split(';');
            bool doing_if = false;
            bool if_all = true;
            bool anything_happened = false;
            bool desired = true;
            bool cur_if_state = false;
            for (int i = 0; i < triggers.Length; i++)
            {
                doing_if = false;
                string[] cur_trigger = triggers[i].Split(' ');
                for (int j = 0; j < cur_trigger.Length; j++)
                {
                    if (cur_trigger[j].Length > 0)
                    {
                        if (cur_trigger[j][cur_trigger[j].Length - 1] == ',')
                        {
                            cur_trigger[j] = cur_trigger[j].Substring(0, cur_trigger[j].Length - 1);
                        }
                        if (cur_trigger[j] == "IF")
                        {
                            doing_if = true; //now doing an if statement
                        }
                        else if (cur_trigger[j] == "AND")
                        {
                            if_all = true;
                        }
                        else if (cur_trigger[j] == "OR")
                        {
                            if_all = false;
                        }
                        else if (cur_trigger[j] == "THEN")
                        {
                            doing_if = false; //now switching to actions
                        }
                        else if (cur_trigger[j] == "END")
                        {
                            return true;
                        }
                        else
                        {
                            if (doing_if)
                            {
                                if (cur_trigger[j][0] == '!')
                                {
                                    
                                    desired = false;
                                    cur_trigger[j] = cur_trigger[j].Substring(1);
                                }
                                else
                                    desired = true;
                                if (check_condition(a, w, cur_trigger[j])!= desired)
                                {
                                    if (if_all)
                                    {
                                        cur_if_state = false;
                                        j = cur_trigger.Length;
                                        continue;
                                    }
                                }
                            }
                            else
                            {
                                anything_happened = true;
                                do_action(a, w, cur_trigger[j]);
                            }
                        }
                    }
                }
            }
            return anything_happened;
        }

        public void do_action(Actor a, World w, string action)
        {
            if (action.Contains('('))
            {
                string property = Acc.script_remove_content_of_outer_parenthesis(action); 
                string value = Acc.script_remove_outer_parentheses(action);
                action_interpret(a, w, property, value);
            }
            else
            {
                action_interpret(a, w, action, "");
            }
        }

        public void action_interpret(Actor a, World w, string item, string value)
        {
            int fid = -1;
            switch (item)
            {
                case "xloc":
                    fid = w.map[top_left.X, top_left.Y].furniture_index;
                    Exilania.furniture_manager.furniture[furniture_id].clear_map_pointers(w, top_left);
                    top_left.X += resolve_to_number(value);
                    Exilania.furniture_manager.furniture[furniture_id].set_map_pointers(fid,w, top_left);
                    break;
                case "yloc":
                    fid = w.map[top_left.X, top_left.Y].furniture_index;
                    Exilania.furniture_manager.furniture[furniture_id].clear_map_pointers(w, top_left);
                    top_left.Y += resolve_to_number(value);
                    Exilania.furniture_manager.furniture[furniture_id].set_map_pointers(fid,w, top_left);
                    break;
                case "state":
                    set_state(a, w, item, value);
                    break;
                case "advancestate":
                    fid = w.map[top_left.X, top_left.Y].furniture_index;
                    Exilania.furniture_manager.furniture[furniture_id].clear_map_pointers(w, top_left);
                    state++;
                    if (state >= Exilania.furniture_manager.furniture[furniture_id].image_frames.Count)
                        state = 0;
                    Exilania.furniture_manager.furniture[furniture_id].set_map_pointers(fid, w, top_left);
                    break;
                case "passable":
                    flags[(int)FFLAGS.PASSABLE] = bool.Parse(value);
                    break;
                case "usepower":
                    power_storage -= resolve_to_number(value);
                    break;
                case "takepower":
                    take_power(a, w, value);
                    break;
                case "light":
                    set_light_source(a, w, value);
                    break;
                case "liquidtransfer":
                    transfer_liquid(w, value);
                    break;
                case "transport":
                    transport_person(w, value);
                    break;
                case "particle":
                    create_particles(w, value);
                    break;
                case "openchest":
                    open_chest(a, w, value);
                    break;
                case "debug":
                    Exilania.display.add_message(value);
                    break;
                case "play":
                    play_sounds(a,w,value);
                    break;
                default:
                    condition_interpret(a, w, item, value);
                    break;

            }
        }

        public void play_sounds(Actor a, World w, string value)
        {
            float pan = 0f;
            float rand_pitch = (float)((float)(Exilania.rand.Next(0,201)-100f)) * .01f;
            switch (value)
            {
                case "slamdoor":
                    Exilania.sounds.door_change.Play(Exilania.sounds.master_volume,rand_pitch,pan);
                    break;
                default:
                    break;
            }
        }

        public void open_chest(Actor a, World w, string value)
        {
            int furn_id = w.map[top_left.X,top_left.Y].furniture_index;
            int chest_id = -1;
            for (int i = 0; i < w.chests.Count; i++)
            {
                if (w.chests[i].furniture_id == furn_id)
                {
                    chest_id = i;
                    i = w.chests.Count;
                }
            }
            if (chest_id == -1)
            {
                Exilania.display.add_message("@08Error... not a chest, cannot open.");
                return;
            }
            if (a.active_chest_id == chest_id)
            {
                a.active_chest_id = -1;
                return;
            }
            string sec= w.chests[chest_id].security;
            if (sec.Contains(a.name) || sec.Contains("~ALL~") || sec.Contains("~TEAM[" + a.stats.team + "]~"))
            {
                if (Acc.get_distance((int)a.world_loc.X, (int)a.world_loc.Y,
                    (top_left.X * 24) + (Exilania.furniture_manager.furniture[furniture_id].image_frames[state].width * 24 / 2),
                    (top_left.Y * 24) + (Exilania.furniture_manager.furniture[furniture_id].image_frames[state].height * 24 / 2)) > ItemChest.max_range)
                {
                    return;
                }
                a.items.show_backpack = true;
                a.active_chest_id = chest_id;
            }
        }

        public void create_particles(World w, string value)
        {

        }

        public void transport_person(World w, string value)
        {
            string[] t = value.Split(',');
            int player_id_transport = -1;
            
            switch (t[0])
            {
                case "proximity":
                    for (int i = 0; i < connections.Count; i++)
                    {
                        if (connections[i].conn_type == ItemConnectionType.Proximity && connections[i].target_type == TargetType.Player)
                        {
                            player_id_transport = connections[i].target_id;
                        }
                    }
                    break;
            }
            if (player_id_transport > -1 && owner.Contains(w.players[player_id_transport].avatar.name))
            {
                //allowed to transport!
                //state 0 means send to next left transporter
                //state 1 means send to next right transporter
                List<int> transporter_furnitures = new List<int>();
                for (int i = 0; i < w.furniture.Count; i++)
                {
                    if (!w.furniture[i].flags[(int)FFLAGS.EMPTY] && w.furniture[i].name.ToLower().Contains("transporter"))
                    {
                        transporter_furnitures.Add(i);
                    }
                }
                transporter_furnitures.Sort((x, y) => w.furniture[x].top_left.X.CompareTo(w.furniture[y].top_left.X));
                int this_furn_temp_id = -1;
                for (int i = 0; i < transporter_furnitures.Count; i++)
                {
                    if (w.furniture[transporter_furnitures[i]].top_left == top_left)
                    {
                        this_furn_temp_id = i;
                        i = transporter_furnitures.Count;
                    }
                }
                int look_id = -1;
                if (state == 1)
                { //trying to send to the left.
                    look_id = this_furn_temp_id - 1;
                    if (look_id < 0)
                        look_id = transporter_furnitures.Count - 1;
                }
                else
                { //trying to send to the right.
                    look_id = this_furn_temp_id + 1;
                    if (look_id >= transporter_furnitures.Count)
                        look_id = 0;
                }
                look_id = transporter_furnitures[look_id];
                int x_dist = Acc.get_min_horizontal_distance(get_rect().Center.X,w.furniture[look_id].get_rect().Center.X, w.map.GetLength(0) * 24);
                double tot_dist = Acc.get_distance(0, top_left.Y * 24, x_dist, w.furniture[look_id].top_left.Y * 24);
                if (tot_dist <= int.Parse(t[1]))
                {
                    //can transport...
                    Vector2 delta_pos = new Vector2(w.players[player_id_transport].avatar.world_loc.X - (top_left.X * 24),
                        w.players[player_id_transport].avatar.world_loc.Y - (top_left.Y * 24));

                    Rectangle cur_bounding_box = w.players[player_id_transport].avatar.bounding_box;
                    cur_bounding_box.X += (int)w.players[player_id_transport].avatar.world_loc.X;
                    cur_bounding_box.Y += (int)w.players[player_id_transport].avatar.world_loc.Y;

                    Exilania.particle_manager.initialize_n_particles_in_rectangle(cur_bounding_box, 100, "Particle", w);


                    w.players[player_id_transport].avatar.world_loc = new Vector2(w.furniture[look_id].top_left.X * 24, w.furniture[look_id].top_left.Y * 24);
                    w.players[player_id_transport].avatar.world_loc += delta_pos;

                    cur_bounding_box = w.players[player_id_transport].avatar.bounding_box;
                    cur_bounding_box.X += (int)w.players[player_id_transport].avatar.world_loc.X;
                    cur_bounding_box.Y += (int)w.players[player_id_transport].avatar.world_loc.Y;

                    Exilania.particle_manager.initialize_n_particles_in_rectangle(cur_bounding_box, 100, "Particle", w);

                    Exilania.display.add_message("@05" + t[2]);
                }
            }
        }

        public void transfer_liquid(World w, string value)
        {
            int liquid_id = 0;
            int amt_liquid_transferring = 0;
            FrameFurniture f = Exilania.furniture_manager.furniture[furniture_id].image_frames[state];
            string[] t = value.Split(',');
            switch (t[0])
            {
                case "thisright":
                    for (int y = top_left.Y; y < top_left.Y + f.height; y++)
                    {
                        if (w.map[w.wraparound_x(top_left.X + f.width), y].liquid_level > 0)
                        {
                            amt_liquid_transferring += w.map[w.wraparound_x(top_left.X + f.width), y].liquid_level;
                            liquid_id = w.map[w.wraparound_x(top_left.X + f.width), y].liquid_id;
                        }
                    }
                    break;
            }
            int liquid_moved = 0;
            for (int x = 0; x < connections.Count; x++)
            {
                if (connections[x].conn_type == ItemConnectionType.LiquidPumpTo && connections[x].target_type == TargetType.Furniture)
                {
                    switch (t[1])
                    {
                        case "destright":
                            f = Exilania.furniture_manager.furniture[w.furniture[connections[x].target_id].furniture_id].image_frames[w.furniture[connections[x].target_id].state];
                            for (int y = w.furniture[connections[x].target_id].top_left.Y; y < w.furniture[connections[x].target_id].top_left.Y + f.height; y++)
                            {
                                if (amt_liquid_transferring > 0 && w.map[w.wraparound_x(w.furniture[connections[x].target_id].top_left.X + f.width), y].liquid_level < LiquidSimulator.max_liquid
                                    && (w.map[w.wraparound_x(w.furniture[connections[x].target_id].top_left.X + f.width), y].liquid_id == liquid_id 
                                    || w.map[w.wraparound_x(w.furniture[connections[x].target_id].top_left.X + f.width), y].liquid_id == 0))
                                {
                                    byte donate_amt = (byte)Math.Min(LiquidSimulator.max_liquid - w.map[w.wraparound_x(w.furniture[connections[x].target_id].top_left.X + f.width), y].liquid_level, amt_liquid_transferring);
                                    amt_liquid_transferring -= donate_amt;
                                    liquid_moved += donate_amt;
                                    Point loc = new Point(w.wraparound_x(w.furniture[connections[x].target_id].top_left.X + f.width), y);
                                    w.liquid_simulator.add_liquid_of_interest(loc, 0);
                                    if (w.map[w.wraparound_x(w.furniture[connections[x].target_id].top_left.X + f.width), y].liquid_cell_id != -1)
                                    {
                                        w.liquid_simulator.add_liquid_to_cell(w.map[w.wraparound_x(w.furniture[connections[x].target_id].top_left.X + f.width), y].liquid_cell_id, donate_amt, w, true);
                                    }
                                    else
                                    {
                                        w.map[w.wraparound_x(w.furniture[connections[x].target_id].top_left.X + f.width), y].liquid_level += (byte)donate_amt;
                                        w.map[w.wraparound_x(w.furniture[connections[x].target_id].top_left.X + f.width), y].liquid_id = (byte)liquid_id;
                                        
                                    }
                                    Point zt = new Point();
                                    for (int i = 0; i < World.xes.Length; i++)
                                    {
                                        zt = new Point(w.wraparound_x(loc.X + World.xes[i]), loc.Y + World.yes[i]);
                                        if (zt.Y > -1 && zt.Y < w.map.GetLength(1))
                                        {
                                            w.liquid_simulator.add_liquid_of_interest(zt, 0);
                                            zt.X -= zt.X % LiquidSimulator.cell_size;
                                            zt.Y -= zt.Y % LiquidSimulator.cell_size;
                                            if (!w.liquid_simulator.cells_need_update.Contains(zt))
                                                w.liquid_simulator.cells_need_update.Add(loc);
                                        }
                                    }
                                    //w.liquid_simulator.redo_cells_at_loc(new Point(loc.X, loc.Y), w);
                                    loc.X -= loc.X % LiquidSimulator.cell_size;
                                    loc.Y -= loc.Y % LiquidSimulator.cell_size;
                                    if (!w.liquid_simulator.cells_need_update.Contains(loc))
                                        w.liquid_simulator.cells_need_update.Add(loc);
                                    w.liquid_simulator.redo_cells_at_loc(loc, w);
                                }
                            }

                            break;
                    }
                }
            }
            f = Exilania.furniture_manager.furniture[furniture_id].image_frames[state];
            switch (t[0])
            {
                case "thisright":
                    for (int y = top_left.Y; y < top_left.Y + f.height; y++)
                    {
                        if (liquid_moved > 0 && w.map[w.wraparound_x(top_left.X + f.width), y].liquid_id == liquid_id && w.map[w.wraparound_x(top_left.X + f.width), y].liquid_level > 0)
                        {
                            Point loc = new Point(w.wraparound_x(top_left.X + f.width), y);
                            w.liquid_simulator.add_liquid_of_interest(loc, 0);
                            byte donate_amt = (byte)Math.Min(w.map[w.wraparound_x(top_left.X + f.width), y].liquid_level, liquid_moved);
                            liquid_moved -= donate_amt;
                            w.liquid_simulator.take_liquid_from_cell_at_loc(new Point(w.wraparound_x(top_left.X + f.width), y), donate_amt, w, true);
                            w.liquid_simulator.add_liquid_of_interest(new Point(w.wraparound_x(top_left.X + f.width), y), liquid_id);
                        }
                    }
                    break;
            }
        }

        public void set_light_source(Actor a, World w, string value)
        {
            string[] str = value.Split(',');
            if (str[0] == "0" && str[1] == "0" && str[2] == "0")
            {
                light_source = null;
                return;
            }
            light_source = new byte[] { byte.Parse(str[0]), byte.Parse(str[1]), byte.Parse(str[2]) };
        }

        public void take_power(Actor a, World w, string value)
        {
            int tot_wanted = 0;
            int need_amt = 0;
            if(value == "max")
            {
                tot_wanted = Exilania.furniture_manager.furniture[furniture_id].max_power_storage - power_storage;
            }
            else
                tot_wanted = resolve_to_number(value);
            
            for (int x = 0; x < connections.Count; x++)
            {
                
                if (connections[x].conn_type == ItemConnectionType.PowerCharging)
                {
                    if (connections[x].target_type == TargetType.Furniture)
                    { //from furniture
                        if (w.furniture[connections[x].target_id].power_storage > 0)
                        {
                            need_amt = Math.Min(tot_wanted, w.furniture[connections[x].target_id].power_storage);
                            if (need_amt + power_storage > Exilania.furniture_manager.furniture[furniture_id].max_power_storage)
                            {
                                need_amt = Exilania.furniture_manager.furniture[furniture_id].max_power_storage - power_storage;
                                tot_wanted = need_amt;
                            }
                            w.furniture[connections[x].target_id].power_storage -= need_amt;
                            power_storage += need_amt;
                            tot_wanted -= need_amt;
                        }
                        if (tot_wanted <= 0)
                            return;
                    }
                    else if(connections[x].target_type == TargetType.Player)
                    { //from player
                        if (w.players[connections[x].target_id].avatar.stats.share_power && w.players[connections[x].target_id].avatar.stats.power.cur_val > 0)
                        {
                            need_amt = Math.Min(w.players[connections[x].target_id].avatar.stats.power.cur_val, tot_wanted);
                            w.players[connections[x].target_id].avatar.stats.power.change_val(-need_amt);
                            power_storage += need_amt;
                        }
                        else
                        {
                            connections.RemoveAt(x);
                            x--;
                        }
                    }
                }
            }
        }

        public void set_state(Actor a, World w, string item, string value)
        {
            int fid = w.map[w.wraparound_x(top_left.X), top_left.Y].furniture_index;
            Exilania.furniture_manager.furniture[furniture_id].clear_map_pointers(w, top_left);
            if (value.Contains(','))
            {
                string[] parts = value.Split(',');
                state = (byte)resolve_to_number(parts[2]);
                top_left.X = resolve_to_number(parts[0]);
                top_left.Y = resolve_to_number(parts[1]);
                Exilania.furniture_manager.furniture[furniture_id].set_map_pointers(fid, w, top_left);
            }
            else
            {
                state = (byte)resolve_to_number(value);
                Exilania.furniture_manager.furniture[furniture_id].set_map_pointers(fid, w, top_left);
            }
        }

        public bool check_condition(Actor a, World w, string cond)
        {
            if (cond.Contains('('))
            {
                string property = Acc.script_remove_content_of_outer_parenthesis(cond);
                string value = Acc.script_remove_outer_parentheses(cond);
                bool t = condition_interpret(a, w, property, value);
                return t;
            }
            else
            {
                bool t = condition_interpret(a, w, cond,"");
                return t;
            }
        }

        public bool condition_interpret(Actor a, World w, string item, string value)
        {
            switch (item)
            {
                case "countactivetransporters":
                    return transporter_check(w, value);
                case "cantransferliquid":
                    return transfer_check(w, value);
                case "connection":
                    return connection_check(w, value);
                case "liquid":
                    return liquid_location(w, value);
                case "standtoright":
                    return ((int)a.world_loc.X / 24 >= top_left.X + Exilania.furniture_manager.furniture[furniture_id].image_frames[state].width);
                case "standtoleft":
                    return ((int)a.world_loc.X / 24 < top_left.X);
                case "standtoside":
                    return ((int)a.world_loc.X / 24 >= top_left.X + Exilania.furniture_manager.furniture[furniture_id].image_frames[state].width) || ((int)a.world_loc.X / 24 < top_left.X) ;
                case "actorisfriend":
                    return owner.Contains(a.name);
                case "free":
                    return loc_is_free(value,w);
                case "state":
                    return state == resolve_to_number(value);
                case "enterproximity":
                    return enter_proximity(w,value);
                case "leaveproximity":
                    return leave_proximity(w, value);
                case "power":
                    return power_storage >= resolve_to_number(value);
                case "sunambience":
                    return sun_ambience(w, value);
                case "eval":
                    return evaluate(w, value);
                default:
                    Exilania.display.add_message("@08ERROR, unknown command '" + item + "'");
                    break;
            }
            return false;
        }

        public bool transporter_check(World w, string value)
        {
            string[] t = value.Split(',');
            int max_distance = resolve_to_number(t[1]);
            int min_num_transporters = resolve_to_number(t[0]);

            for (int i = 0; i < w.furniture.Count; i++)
            {
                if (!w.furniture[i].flags[(int)FFLAGS.EMPTY] && w.furniture[i].name.Contains("Transporter") && w.furniture[i].power_storage >= 20 &&
                    !(w.furniture[i].top_left.X == top_left.X && w.furniture[i].top_left.Y == top_left.Y))
                {
                    return true;
                }
            }

            return false;
        }

        public bool transfer_check(World w, string value)
        {
            int liquid_type = 0;
            int amt_liquid_transferring = 0;
            FrameFurniture f = Exilania.furniture_manager.furniture[furniture_id].image_frames[state];
            string[] t = value.Split(',');
            int min_amount = resolve_to_number(t[2]);
            switch (t[0])
            {
                case "thisright":
                    for (int y = top_left.Y; y < top_left.Y + f.height; y++)
                    {
                        if (w.map[w.wraparound_x(top_left.X + f.width), y].liquid_level > 0 && (liquid_type == 0 || w.map[w.wraparound_x(top_left.X + f.width), y].liquid_id == liquid_type))
                        {
                            amt_liquid_transferring += w.map[w.wraparound_x(top_left.X + f.width), y].liquid_level;
                            liquid_type = w.map[w.wraparound_x(top_left.X + f.width), y].liquid_id;
                        }
                    }
                    break;
            }
            if (amt_liquid_transferring < min_amount)
            {
                return false;
            }
            amt_liquid_transferring = 0;
            for (int x = 0; x < connections.Count; x++)
            {
                if (connections[x].conn_type == ItemConnectionType.LiquidPumpTo && connections[x].target_type == TargetType.Furniture)
                {
                    switch (t[1])
                    {
                        case "destright":
                            f = Exilania.furniture_manager.furniture[w.furniture[connections[x].target_id].furniture_id].image_frames[w.furniture[connections[x].target_id].state];
                            for (int y = w.furniture[connections[x].target_id].top_left.Y; y < w.furniture[connections[x].target_id].top_left.Y + f.height; y++)
                            {
                                if (w.map[w.wraparound_x(w.furniture[connections[x].target_id].top_left.X + f.width), y].liquid_level < LiquidSimulator.max_liquid &&
                                    (liquid_type == w.map[w.wraparound_x(w.furniture[connections[x].target_id].top_left.X + f.width), y].liquid_id||
                                    w.map[w.wraparound_x(w.furniture[connections[x].target_id].top_left.X + f.width), y].liquid_id == 0))
                                {
                                    amt_liquid_transferring += LiquidSimulator.max_liquid - w.map[w.wraparound_x(w.furniture[connections[x].target_id].top_left.X + f.width), y].liquid_level;
                                }
                            }
                            if (amt_liquid_transferring >= min_amount)
                            {
                                return true;
                            }
                            break;
                    }
                }
            }
            return false;
        }

        public bool liquid_location(World w, string value)
        {
            FrameFurniture f = Exilania.furniture_manager.furniture[furniture_id].image_frames[state];
            string[] t = value.Split(',');
            int min_amount = resolve_to_number(t[1]);
            switch (t[0])
            {
                case "right":
                    for (int y = top_left.Y; y < top_left.Y + f.height; y++)
                    {
                        if (w.map[w.wraparound_x(top_left.X + f.width), y].liquid_level >= min_amount)
                        {
                            return true;
                        }
                    }
                    break;
            }
            return false;
        }

        public bool evaluate(World w, string value)
        {
            if (value.Contains('='))
            {
                string[] vals = value.Split('=');
                return resolve_to_number(vals[0]) == resolve_to_number(vals[1]);
            }
            else
            {
                return resolve_to_number(value) >= 1;
            }
        }

        public bool connection_check(World w, string value)
        {
            int val_look = 1;
            int cur_Val = 0;
            string switcher = value.ToLower();
            if (value.Contains(','))
            {
                string[] t = value.Split(',');
                val_look = resolve_to_number(t[1]);
                switcher = t[0].ToLower();
            }
            ItemConnectionType conn = ItemConnectionType.Slave;
            switch (switcher)
            {
                case "powersource":
                    conn = ItemConnectionType.PowerCharging;
                    break;
                case "liquidpumpto":
                    conn = ItemConnectionType.LiquidPumpTo;
                    break;
                case "proximity":
                    conn = ItemConnectionType.Proximity;
                    break;
            }
            for (int x = 0; x < connections.Count; x++)
            {
                if (connections[x].conn_type == conn)
                    cur_Val++;
            }
            return cur_Val >= val_look;
        }

        public bool sun_ambience(World w, string value)
        {
            if (w.day_only_portion >= 1f)
                return false;

            for (int x = top_left.X; x < top_left.X + Exilania.furniture_manager.furniture[furniture_id].image_frames[state].width; x++)
            {
                for (int y = top_left.Y; y < top_left.Y + Exilania.furniture_manager.furniture[furniture_id].image_frames[state].height; y++)
                {
                    if (w.has_transparent_neighbors(new Point(x, y)))
                        return true;
                }
            }
            return false;
        }

        public bool enter_proximity(World w, string vars)
        {
            
            string[] items = vars.Split(',');
            if (!calced_enter_proximity)
            {
                calced_enter_proximity = true;
                int dist = resolve_to_number(items[0]);
               
                List<int> players_approach = w.collision_table.get_players_in_range(get_rect().Center, dist, w);

                if (players_approach.Count > 0)
                {
                    
                    bool ret_be_true = false;
                    bool has_cur = false;
                    for (int i = 0; i < players_approach.Count; i++)
                    {
                        has_cur = false;
                        for (int j = 0; j < connections.Count; j++)
                        {
                            if (connections[j].target_type == TargetType.Player && connections[j].target_id == players_approach[i])
                                has_cur = true;
                        }
                        if (!has_cur && match_actor_to_permission_type(items[1], w.players[players_approach[i]].avatar))
                        {
                            ret_be_true = true;
                            connections.Add(new ItemConnector(players_approach[i], TargetType.Player, ItemConnectionType.Proximity, 0, 0));
                            did_enter_proximity = true;
                        }
                    }
                    return ret_be_true;
                }
                else
                    return false;
            }
            return did_enter_proximity;
        }

        public bool leave_proximity(World w, string vars)
        {
            string[] items = vars.Split(',');
            if (!calced_leave_proximity)
            {
                calced_leave_proximity = true;
                int dist = resolve_to_number(items[0]);
               
                List<int> players_approach = w.collision_table.get_players_in_range(get_rect().Center, dist, w);


                bool has_left = false;
                for (int i = 0; i < connections.Count; i++)
                {
                    if (connections[i].target_type == TargetType.Player)
                    {
                        if (players_approach.Count == 0)
                        { //if there are no players approaching, everyone has left... remove player connection, set leave proximity to true.
                            if (match_actor_to_permission_type(items[1], w.players[connections[i].target_id].avatar))
                            {
                                did_leave_proximity = true;
                            }
                            connections.RemoveAt(i);
                            i--;
                        }
                        else
                        { //there are some players in proximity
                            has_left = true;
                            for (int j = 0; j < players_approach.Count; j++)
                            { //run through each player in proximity
                                if (connections[i].target_id == players_approach[j])
                                { //if that player is already on the list and still in proximity, they have not left.
                                    has_left = false;
                                }
                            }
                            if (has_left)
                            { //the player is no longer in proximity, they have left.
                                if (match_actor_to_permission_type(items[1], w.players[connections[i].target_id].avatar))
                                {
                                    did_leave_proximity = true;
                                }
                                connections.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                }
                return did_leave_proximity;
            }
            return did_leave_proximity;
        }

        public bool match_actor_to_permission_type(string permissions, Actor a)
        {
            if (permissions == "owner")
            {
                if (owner.Contains(a.name))
                {
                    return true;
                }
                else
                    return false;
            }
            else if (permissions == "anyone")
                return true;


            return false;
        }

        /// <summary>
        /// free can take either:
        /// free(xloc-1)
        /// or
        /// free(xloc,yloc,state2)
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="w"></param>
        /// <returns></returns>
        public bool loc_is_free(string loc, World w)
        {
            if (loc.Contains(','))
            {
                string[] pieces = loc.Split(',');
                int tlx = resolve_to_number(pieces[0]);
                int tly = resolve_to_number(pieces[1]);
                int state_check = resolve_to_number(pieces[2]);
                if (pieces.Length == 3)
                {
                    for (int x = tlx; x < tlx + Exilania.furniture_manager.furniture[furniture_id].image_frames[state_check].width; x++)
                    {
                        for (int y = tly; y < tly + Exilania.furniture_manager.furniture[furniture_id].image_frames[state_check].height; y++)
                        {
                            if (y < 0 || y >= w.map.GetLength(1))
                                return false;
                            if ((w.map[w.wraparound_x(x), y].fgd_block_type == -1 || w.map[w.wraparound_x(x), y].fgd_block_type == 5) &&
                               (w.map[w.wraparound_x(x), y].furniture_index == -1 || w.map[w.wraparound_x(x), y].furniture_index == w.map[top_left.X, top_left.Y].furniture_index))
                            {

                            }
                            else
                                return false;
                        }
                    }
                    return true;
                }
                else if (pieces.Length == 4)
                {
                    int durx = resolve_to_number(pieces[2]);
                    int dury = resolve_to_number(pieces[3]);
                    for (int x = tlx; x < tlx + durx; x++)
                    {
                        for (int y = tly; y < tly + dury; y++)
                        {
                            if (y < 0 || y >= w.map.GetLength(1))
                                return false;
                            if ((w.map[w.wraparound_x(x), y].fgd_block_type == -1 || w.map[w.wraparound_x(x), y].fgd_block_type == 5) &&
                               (w.map[w.wraparound_x(x), y].furniture_index == -1 || w.map[w.wraparound_x(x), y].furniture_index == w.map[top_left.X, top_left.Y].furniture_index))
                            {

                            }
                            else
                                return false;
                        }
                    }
                    return true;
                }
            }

            int loc_check = resolve_to_number(loc);
            if (loc.Contains('x'))
            {
                if (loc_check > top_left.X)
                    loc_check += Exilania.furniture_manager.furniture[furniture_id].image_frames[state].width -1;
                for (int i = 0; i < Exilania.furniture_manager.furniture[furniture_id].image_frames[state].height; i++)
                {
                    if (top_left.Y + i < 0 || top_left.Y + i >= w.map.GetLength(1))
                        return false;
                    if ((w.map[w.wraparound_x(loc_check), top_left.Y + i].fgd_block_type == -1 ||
                         w.map[w.wraparound_x(loc_check), top_left.Y + i].fgd_block_type == 5) && w.map[w.wraparound_x(loc_check), top_left.Y + i].furniture_index == -1)
                    {

                    }
                    else
                        return false;
                }
                return true;
            }
            else if (loc.Contains('y'))
            {
                if (loc_check > top_left.Y)
                    loc_check += Exilania.furniture_manager.furniture[furniture_id].image_frames[state].height -1;
                for (int i = 0; i < Exilania.furniture_manager.furniture[furniture_id].image_frames[state].width; i++)
                {
                    if (loc_check < 0 || loc_check >= w.map.GetLength(1))
                        return false;
                    if ((w.map[w.wraparound_x(top_left.X + i), loc_check].fgd_block_type != -1 ||
                         w.map[w.wraparound_x(top_left.X + i), loc_check].fgd_block_type != 5) && w.map[w.wraparound_x(top_left.X + i), loc_check].furniture_index == -1)
                    {

                    }
                    else
                        return false;
                }
                return true;
            }
            return false;
        }


        public int resolve_to_number(string property)
        {

            if (property.Contains(' '))
            {
                property = property.Replace(" ","");
            }
            while (property.Contains('('))
            {
                string inner = Acc.get_inner_parenthesis(property);
                int start_loc = property.IndexOf(inner) - 1;
                int end_loc = start_loc + 2 + inner.Length;
                string first_half = property.Substring(0, start_loc);
                string second_half = property.Substring(end_loc);
                property = first_half + resolve_to_number(inner).ToString() + second_half;

            }
            if (property.Contains('+'))
            {
                int tot = 0;
                string[] pieces = property.Split('+');
                for (int x = 0; x < pieces.Length; x++)
                {
                    tot += resolve_to_number(pieces[x]);
                }
                return tot;
            }
            if (property.Contains('-'))
            {
                int tot = 0;
                string[] pieces = property.Split('-');
                for (int x = 0; x < pieces.Length; x++)
                {
                    if (x > 0)
                        tot -= resolve_to_number(pieces[x]);
                    else
                        tot = resolve_to_number(pieces[x]);
                }
                return tot;
            }
            if (property.Contains('*'))
            {
                int tot = 0;
                string[] pieces = property.Split('*');
                for (int x = 0; x < pieces.Length; x++)
                {
                    if (x > 0)
                        tot *= resolve_to_number(pieces[x]);
                    else
                        tot = resolve_to_number(pieces[x]);
                }
                return tot;
            }
            if (property.Contains('/'))
            {
                int tot = 0;
                string[] pieces = property.Split('/');
                for (int x = 0; x < pieces.Length; x++)
                {
                    if (x > 0)
                    {
                        if (resolve_to_number(pieces[x]) > 0)
                            tot /= resolve_to_number(pieces[x]);
                        else
                            tot = 0;
                    }
                    else
                        tot = resolve_to_number(pieces[x]);
                }
                return tot;
            }
            if (property.Contains('%'))
            {
                int tot = 0;
                string[] pieces = property.Split('%');
                for (int x = 0; x < pieces.Length; x++)
                {
                    if (x > 0)
                    {
                        if (resolve_to_number(pieces[x]) > 0)
                            tot %= resolve_to_number(pieces[x]);
                        else
                            tot = 0;
                    }
                    else
                        tot = resolve_to_number(pieces[x]);
                }
                return tot;
            }
            if (property.Contains('^'))
            {
                int tot = 0;
                string[] pieces = property.Split('^');
                for (int x = 0; x < pieces.Length; x++)
                {
                    if (x > 0)
                    {
                        tot = (int)Math.Pow(tot, resolve_to_number(pieces[x]));
                    }
                    else
                        tot = resolve_to_number(pieces[x]);
                }
                return tot;
            }
            switch (property)
            {
                case "placexloc": return place_origin.X;
                case "placeyloc": return place_origin.Y;
                case "xloc": return top_left.X;
                case "yloc": return top_left.Y;
                case "state": return state;
                case "power_storage": return power_storage;
                case "max_power_storage": return Exilania.furniture_manager.furniture[furniture_id].max_power_storage;
                case "power_usage": return Exilania.furniture_manager.furniture[furniture_id].state_power_usage[state];
                case "width": return Exilania.furniture_manager.furniture[furniture_id].image_frames[state].width;
                case "height": return Exilania.furniture_manager.furniture[furniture_id].image_frames[state].height;
                case "true": return 1;
                case "false": return 0;
                default:
                    break;
            }
            if (property.Contains('d'))
            {
                string[] parts = property.Split('d');
                int num_die = Int32.Parse(parts[0]);
                int modifier = 0;
                int num_sides = Int32.Parse(parts[1]);
                for (int x = 0; x < num_die; x++)
                {
                    modifier += Exilania.rand.Next(1, num_sides + 1);
                }
                return modifier;
            }
            else
            {
                if (property.Length > 0)
                    return Int32.Parse(property);
                else return 0;
            }
        }
    }
}
