using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace Exilania
{
    public struct block_changes
    {
        public Point loc;
        public int block_id;
        public bool is_foreground;
    }
    public struct Player_Tag
    {
        public int player_game_index_id;
        public bool host;
    }
    public class Server
    {
        public NetServer udp_server;
        public bool accepting_users = true;
        public List<DamageMove> damages;

        public Server()
        {
            damages = new List<DamageMove>();
            udp_server = new NetServer(Exilania.udp_server_config);
        }

        public void connect(Exilania g)
        {
            udp_server.Start();
            damages = new List<DamageMove>();
        }

        public void server_messages(Exilania g, Display d, World w)
        {
            //Exilania.debug = udp_server.Statistics.ToString();
            NetIncomingMessage inc;
            if (w != null && damages.Count > 0)
            {
                process_damage(w);
            }
            while ((inc = udp_server.ReadMessage()) != null)
            {
                switch (inc.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        consume_message(inc,g,d, w);
                        break;
                     case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)inc.ReadByte();
                        string reason = inc.ReadString();
                        Exilania.text_stream.WriteLine("Server: Status changed: " + reason);
                        if (status == NetConnectionStatus.Connected)
                        {
                            Player_Tag p;
                            p.player_game_index_id = -1;
                            if (Exilania.people_connected == 0)
                            {
                                p.host = true;
                            }
                            else
                                p.host = false;
                            if (Exilania.people_connected < 255 && Exilania.people_connected < Exilania.settings.max_users)
                            {
                                Exilania.people_connected++;
                                inc.SenderConnection.Tag = p;
                                NetOutgoingMessage m = udp_server.CreateMessage();
                                m.Write((byte)1);
                                m.Write(Exilania.people_connected);
                                m.Write(Exilania.settings.force_new_character);
                                udp_server.SendMessage(m, inc.SenderConnection, NetDeliveryMethod.ReliableUnordered);
                                d.add_message("Server: Received response from Client. Connected. " + reason);
                            }
                            else
                            {
                                d.add_message("Server: Too many users in-game already... cannot add any more.");
                                inc.SenderConnection.Disconnect("Too many users in-game.");
                            }
                        }
                        break;
                    default:
                        d.add_message("Server: Some sort of message received " + inc.MessageType);
                        Exilania.text_stream.WriteLine("Server: Unhandled type: " + inc.MessageType + " " + inc.ReadString());
                        break;
                }
                udp_server.Recycle(inc);
            }
        }

        public void consume_message(NetIncomingMessage p, Exilania g, Display d, World w)
        {
            byte type = p.ReadByte();
            switch (type)
            {
                case 1: //identify yourself response
                    break;
                case 2: //message being broadcast
                    relay_chat(p,g,d);
                    break;
                case 3://received new player_character
                    add_new_player(p, g, d, w);
                    break;
                case 5://sending mouse coordinates. and position coordinates!
                    relay_mouse_and_position_coordinates(p, d, w);
                    break;
                case 7: //received input change from someone.
                    relay_key_commands(p, d, w);
                    break;
                case 8://received block removal
                    receive_block_remove(p, d, w);
                    break;
                case 9://received place block
                    receive_block_place(p, d, w);
                    break;
                case 10://nothing... only server sends world data to clients.
                    break;
                case 11://running a trigger on furniture
                    receive_run_trigger(p, d, w);
                    break;
                case 12://received client shoot bullet
                    receive_shoot_bullet(p, d, w);
                    break;
                case 13://received client place furniture
                    receive_place_furniture(p, d, w);
                    break;
                case 14://received client remove furniture
                    receive_remove_furniture(p, d, w);
                    break;
                case 15://receive_furniture_connection
                    receive_furniture_connection(p, d, w);
                    break;
                case 16://receive_furniture_disconnect
                    receive_furniture_disconnect(p, d, w);
                    break;
                case 17://receive attack plant
                    receive_attack_plant(p, d, w);
                    break;
                case 18://receive place plant
                    break;
                case 19://receive change item in hands
                    receive_change_hands_item(p, d, w);
                    break;
                case 20://receive swing arms
                    receive_swing_arms(p, d, w);
                    break;
                case 23://receive someone attacked someone else
                    receive_damage_request(p, d, w);
                    break;
                case 24://receive someone changed a chest
                    receive_changed_chest(p, w);
                    break;
                case 25://receive someone place a plant
                    receive_place_plant(p, w);
                    break;
                case 64://client quitting
                    remove_client(p, d, w);
                    break;
            }
        }


        public void relay_chat(NetIncomingMessage p,Exilania g,Display d)
        {
            NetOutgoingMessage i = udp_server.CreateMessage();
            i.Write((byte)2);
            i.Write(p.ReadByte());
            string msg = p.ReadString();
            i.Write(msg);
            udp_server.SendToAll(i, p.SenderConnection, NetDeliveryMethod.ReliableOrdered, 5);

        }

        public void msg_to_all(string msg)
        {
            NetOutgoingMessage i = udp_server.CreateMessage();
            i.Write((byte)2);
            i.Write((byte)Exilania.game_my_user_id);
            i.Write(msg);
            udp_server.SendToAll(i, NetDeliveryMethod.ReliableOrdered);
        }

        public void add_new_player(NetIncomingMessage p, Exilania g, Display d, World w)
        {
            //add the received character into the world and initialize them.
            Player new_player_char = new Player(p);
            d.add_message("@05Player " + new_player_char.avatar.name + " has joined us.");
            new_player_char.avatar.world_loc = new Vector2(w.world_spawn_loc.X,w.world_spawn_loc.Y);// w.spawn_actor(new_player_char.avatar.bounding_box);
            int player_id = -1;
            for (int x = 0; x < w.players.Count; x++)
            {
                if (w.players[x].is_player_empty)
                {
                    player_id = x;
                    x = w.players.Count;
                }
            }
            if (player_id != -1)
            {
                w.players[player_id] = new_player_char;
            }
            else
            {
                w.players.Add(new_player_char);
                player_id = w.players.Count - 1;
            }
            //send the received character out to everyone else
            NetOutgoingMessage i = udp_server.CreateMessage();
            i.Write((byte)4);
            new_player_char.send_player(i);
            i.Write((byte)player_id);
            i.Write((Single)new_player_char.avatar.world_loc.X);
            i.Write((Single)new_player_char.avatar.world_loc.Y);
            udp_server.SendToAll(i, p.SenderConnection, NetDeliveryMethod.ReliableOrdered, 4);
            //if not the host, send the world to the player.
            if (w.players.Count > 1)
            {
                send_world_to_client(p, g, d, w);
            }
            //then initialize the player... these two run on the same reliable channel, so 1 can only happen after the other.
            initialize_player_world((byte)player_id, p, g, d, w);
        }

        public void remove_client(NetIncomingMessage p, Display d, World w)
        {
            byte quitter_id = p.ReadByte();
            if (quitter_id < w.players.Count && !w.players[quitter_id].is_player_empty)
            {
                w.players[quitter_id].is_player_empty = true;
                d.add_message("@03" + w.players[quitter_id].avatar.name + " @03has quit.");
                //w.players[quitter_id] = null;
                Exilania.people_connected--;
                NetOutgoingMessage i = udp_server.CreateMessage();
                i.Write((byte)64);
                i.Write(quitter_id);
                udp_server.SendToAll(i, p.SenderConnection, NetDeliveryMethod.ReliableOrdered, 4);
                p.SenderConnection.Disconnect("You have been successfully logged off.");
            }
            //w.players[quitter_id] = null;
        }

        /// <summary>
        /// this will be called upon first creation of a new player... it will send all data for all players and the players game_id
        /// </summary>
        /// <param name="p"></param>
        /// <param name="g"></param>
        /// <param name="d"></param>
        /// <param name="w"></param>
        public void initialize_player_world(byte player_id, NetIncomingMessage p, Exilania g, Display d, World w)
        {
            NetOutgoingMessage i = udp_server.CreateMessage();
            i.Write((byte)3);
            i.WriteTime(false);
            i.Write(w.world_time);
            i.Write((byte)player_id);
            i.Write((byte)w.players.Count);
            for (int x = 0; x < w.players.Count; x++)
            {
                w.players[x].send_player(i);
                i.Write((Single)w.players[x].avatar.world_loc.X);
                i.Write((Single)w.players[x].avatar.world_loc.Y);
            }
            udp_server.SendMessage(i, p.SenderConnection, NetDeliveryMethod.ReliableOrdered,6);
            
        }

        public void send_world_to_client(NetIncomingMessage p, Exilania g, Display d, World w)
        {
            /*System.Security.Cryptography.HashAlgorithm ha = System.Security.Cryptography.HashAlgorithm.Create();
            System.IO.FileStream fs = new System.IO.FileStream(@"worlds/" + w.name + ".wld",System.IO.FileMode.Open,System.IO.FileAccess.Read);
            byte[] hash = ha.ComputeHash(fs);
            fs.Close();
            Exilania.text_stream.WriteLine("Hash code before resaving: "+BitConverter.ToString(hash));*/

            w.write_world();

            /*fs = new System.IO.FileStream(@"worlds/" + w.name + ".wld", System.IO.FileMode.Open, System.IO.FileAccess.Read);
            hash = ha.ComputeHash(fs);
            fs.Close();
            Exilania.text_stream.WriteLine("Hash code after resaving: " + BitConverter.ToString(hash));*/


            string file_name = "world"+w.world_number.ToString();
            byte[] data_write = System.IO.File.ReadAllBytes(@"worlds/" + file_name + ".wld");
            System.IO.FileStream fs = new System.IO.FileStream(@"worlds/network_game.wlz", System.IO.FileMode.Create, System.IO.FileAccess.Write);
            using (fs)
            {
                System.IO.Compression.GZipStream gzip = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Compress);
                gzip.Write(data_write,0,(int)data_write.Length);
            }
            data_write = System.IO.File.ReadAllBytes(@"worlds/network_game.wlz");
            System.IO.File.Delete(@"worlds/network_game.wlz");
            NetOutgoingMessage i = udp_server.CreateMessage();
            i.Write((byte)10);
            i.Write(file_name);
            i.Write(data_write.Length);
            i.Write(data_write);
            udp_server.SendMessage(i, p.SenderConnection, NetDeliveryMethod.ReliableOrdered,6);
        }

        public void relay_position_coordinates(NetIncomingMessage p, int player_id, Display d, World w)
        {
            NetOutgoingMessage i = udp_server.CreateMessage();
            i.Write((byte)5);
            i.Write((byte)player_id);
            if (w.players[player_id].avatar.inertia.X == 0 && w.players[player_id].avatar.inertia.Y == 0)
            {
                i.Write((Single)w.players[player_id].avatar.world_loc.X);
                i.Write((Single)w.players[player_id].avatar.world_loc.Y);
            }
            else
            {
                i.Write((Single)0);
                i.Write((Single)0);
            }
            udp_server.SendMessage(i, p.SenderConnection, NetDeliveryMethod.UnreliableSequenced, 1);
        }

        public void relay_key_commands(NetIncomingMessage p, Display d, World w)
        {
            int player_id = p.ReadByte();
            scheduled_movement t = new scheduled_movement(p);
            if (player_id != Exilania.game_my_user_id)
            {
                w.players[player_id].avatar.input.movement_schedule.Add(t);
            }
            NetOutgoingMessage i = udp_server.CreateMessage();
            i.Write((byte)7);
            i.Write((byte)player_id);
            i.WriteTime(true);
            i.Write(w.world_time);
            t.send_movement(i);
            udp_server.SendToAll(i, p.SenderConnection, NetDeliveryMethod.ReliableSequenced, 2);
        }

        public void relay_mouse_and_position_coordinates(NetIncomingMessage p, Display d, World w)
        {
            int player_id = p.ReadByte();
            w.players[player_id].avatar.input.mouse_loc.X = p.ReadInt32();
            w.players[player_id].avatar.input.mouse_loc.Y = p.ReadInt32();
            NetOutgoingMessage i = udp_server.CreateMessage();
            i.Write((byte)6);
            i.Write((byte)player_id);
            i.Write(w.players[player_id].avatar.input.mouse_loc.X);
            i.Write(w.players[player_id].avatar.input.mouse_loc.Y);
            if (w.players[player_id].avatar.inertia.X == 0 && w.players[player_id].avatar.inertia.Y == 0)
            {
                i.Write((Single)w.players[player_id].avatar.world_loc.X);
                i.Write((Single)w.players[player_id].avatar.world_loc.Y);
            }
            else
            {
                i.Write((Single)0);
                i.Write((Single)0);
            }

            udp_server.SendToAll(i, p.SenderConnection, NetDeliveryMethod.UnreliableSequenced, 1);
            relay_position_coordinates(p, player_id, d, w);
        }

        public void receive_block_remove(NetIncomingMessage p, Display d, World w)
        {
            byte player_id = p.ReadByte();
            if (w.players.Count > player_id)
            {
                Point temp = new Point(p.ReadUInt16(), p.ReadUInt16());
                bool use_bkd = p.ReadBoolean();
                if(player_id != Exilania.game_my_user_id)
                    w.remove_block(temp, use_bkd);
                NetOutgoingMessage i = udp_server.CreateMessage();
                i.Write((byte)8);
                i.Write(player_id);
                i.Write((ushort)temp.X);
                i.Write((ushort)temp.Y);
                i.Write(use_bkd);
                udp_server.SendToAll(i, p.SenderConnection, NetDeliveryMethod.ReliableSequenced, 3);
            }

        }

        public void receive_block_place(NetIncomingMessage p, Display d, World w)
        {
            byte player_id = p.ReadByte();
            if (w.players.Count > player_id)
            {
                Point temp = new Point(p.ReadUInt16(), p.ReadUInt16());
                bool use_bkd = p.ReadBoolean();
                byte block_placing = p.ReadByte();
                if(player_id!= Exilania.game_my_user_id)
                    w.place_block(temp, (sbyte)block_placing, use_bkd, false);
                NetOutgoingMessage i = udp_server.CreateMessage();
                i.Write((byte)9);
                i.Write(player_id);
                i.Write((ushort)temp.X);
                i.Write((ushort)temp.Y);
                i.Write(block_placing);
                i.Write(use_bkd);
                udp_server.SendToAll(i, p.SenderConnection, NetDeliveryMethod.ReliableSequenced, 3);
            }
        }

        public void receive_run_trigger(NetIncomingMessage p, Display d, World w)
        {
            byte owner = p.ReadByte();
            int furniture_id = p.ReadInt32();
            byte trigger_id = p.ReadByte();
            if (owner != Exilania.game_my_user_id)
            {
                switch (trigger_id)
                {
                    case 0: //running click function
                        w.furniture[furniture_id].run_click_function(w.players[owner].avatar, w);
                        break;
                }
            }
            NetOutgoingMessage i = udp_server.CreateMessage();
            i.Write((byte)11);
            i.Write((byte)owner);
            i.Write(furniture_id);
            i.Write(trigger_id);
            udp_server.SendToAll(i, p.SenderConnection, NetDeliveryMethod.ReliableOrdered, 4);
        }

        public void receive_shoot_bullet(NetIncomingMessage p, Display d, World w)
        {
            byte owner = p.ReadByte();
            byte pic = p.ReadByte();
            float angle = p.ReadSingle();
            Particle.initialize_particle(w.bullets, Exilania.particle_manager.get_particle_id_by_name("Bronze Ball"),
                5f, angle, (int)w.players[owner].avatar.world_loc.X, (int)w.players[owner].avatar.world_loc.Y, 1, false, d);
            NetOutgoingMessage i = udp_server.CreateMessage();
            i.Write((byte)12);
            i.Write(owner);
            i.Write(pic);
            i.Write(angle);
            udp_server.SendToAll(i, p.SenderConnection, NetDeliveryMethod.ReliableOrdered, 6);
        }

        public void receive_place_furniture(NetIncomingMessage p, Display d, World w)
        {
            byte owner = p.ReadByte();
            Point loc = new Point(p.ReadInt32(), p.ReadInt32());
            int furniture_id = p.ReadInt32();
            if (owner != Exilania.game_my_user_id)
            {
                Exilania.furniture_manager.furniture[furniture_id].try_place(w.players[owner].avatar, loc, w, new Rectangle());
            }
            NetOutgoingMessage i = udp_server.CreateMessage();
            i.Write((byte)13);
            i.Write((byte)owner);
            i.Write(loc.X);
            i.Write(loc.Y);
            i.Write(furniture_id);
            udp_server.SendToAll(i, p.SenderConnection, NetDeliveryMethod.ReliableOrdered, 4);
        }

        public void receive_remove_furniture(NetIncomingMessage p, Display d, World w)
        {
            byte owner = p.ReadByte();
            Point loc = new Point(p.ReadInt32(), p.ReadInt32());
            if (owner != Exilania.game_my_user_id)
            {
                int id_pickup = w.furniture[w.map[loc.X, loc.Y].furniture_index].furniture_id;
                Exilania.furniture_manager.furniture[id_pickup].remove_furniture(w, loc);
            }
            NetOutgoingMessage i = udp_server.CreateMessage();
            i.Write((byte)14);
            i.Write((byte)owner);
            i.Write(loc.X);
            i.Write(loc.Y);
            udp_server.SendToAll(i, p.SenderConnection, NetDeliveryMethod.ReliableOrdered, 4);
        }

        public void receive_furniture_connection(NetIncomingMessage p, Display d, World w)
        {
            int player_id = p.ReadByte();
            int furn_from = w.map[p.ReadUInt16(), p.ReadUInt16()].furniture_index;
            int furn_to = w.map[p.ReadUInt16(), p.ReadUInt16()].furniture_index;
            ItemConnectionType t = (ItemConnectionType)p.ReadByte();
            if (player_id != Exilania.game_my_user_id)
            {
                w.furniture[furn_from].connections.Add(new ItemConnector(furn_to, TargetType.Furniture, t, 0, 0));
            }
            NetOutgoingMessage i = udp_server.CreateMessage();
            i.Write((byte)15);
            i.Write((byte)player_id);
            i.Write((ushort)w.furniture[furn_from].top_left.X);
            i.Write((ushort)w.furniture[furn_from].top_left.Y);
            i.Write((ushort)w.furniture[furn_to].top_left.X);
            i.Write((ushort)w.furniture[furn_to].top_left.Y);
            i.Write((byte)t);
            udp_server.SendToAll(i, p.SenderConnection, NetDeliveryMethod.ReliableOrdered, 4);
        }

        public void receive_furniture_disconnect(NetIncomingMessage p, Display d, World w)
        {
            int player_id = p.ReadByte();
            int furn_from = w.map[p.ReadUInt16(), p.ReadUInt16()].furniture_index;
            int furn_to = w.map[p.ReadUInt16(), p.ReadUInt16()].furniture_index;
            if (player_id != Exilania.game_my_user_id)
            {
                for (int x = 0; x < w.furniture[furn_from].connections.Count; x++)
                {
                    if (w.furniture[furn_from].connections[x].target_type == TargetType.Furniture && w.furniture[furn_from].connections[x].target_id == furn_to)
                    {
                        w.furniture[furn_from].connections.RemoveAt(x);
                        x--;
                    }
                }
            }
            NetOutgoingMessage i = udp_server.CreateMessage();
            i.Write((byte)16);
            i.Write((byte)player_id);
            i.Write((ushort)w.furniture[furn_from].top_left.X);
            i.Write((ushort)w.furniture[furn_from].top_left.Y);
            i.Write((ushort)w.furniture[furn_to].top_left.X);
            i.Write((ushort)w.furniture[furn_to].top_left.Y);
            udp_server.SendToAll(i, p.SenderConnection, NetDeliveryMethod.ReliableOrdered, 4);
        }

        public void send_destroy_plant(World w, Point top_left)
        {
            NetOutgoingMessage i = udp_server.CreateMessage();
            i.Write((byte)17);
            i.Write((byte)0);
            i.Write(top_left.X);
            i.Write(top_left.Y);
            udp_server.SendToAll(i, NetDeliveryMethod.ReliableOrdered);
        }

        public void receive_attack_plant(NetIncomingMessage p, Display d, World w)
        {
            int player_id = p.ReadByte();
            Point attack_loc = new Point(p.ReadInt32(), p.ReadInt32());
            if (player_id != Exilania.game_my_user_id)
            {
                int plant_id = w.map[attack_loc.X, attack_loc.Y].plant_index;
                if (w.plants[plant_id].destroy_give_items(attack_loc, w, w.players[player_id].avatar, plant_id))
                {
                    w.plants[plant_id].unset_map(w);
                    for (int x = plant_id + 1; x < w.plants.Count; x++)
                    {
                        w.plants[x].unset_map(w);
                        w.plants[x].reset_map(w, x - 1);
                    }
                    w.plants.RemoveAt(plant_id);
                }
            }
            NetOutgoingMessage i = udp_server.CreateMessage();
            i.Write((byte)17);
            i.Write((byte)player_id);
            i.Write(attack_loc.X);
            i.Write(attack_loc.Y);
            udp_server.SendToAll(i,p.SenderConnection, NetDeliveryMethod.ReliableOrdered, 4);
        }

        public void receive_change_hands_item(NetIncomingMessage p, Display d, World w)
        {
            int player_id = p.ReadByte();
            bool is_left = p.ReadBoolean();
            Cubby c = new Cubby(p);
            if (player_id != Exilania.game_my_user_id)
            {
                if (is_left)
                {
                    w.players[player_id].avatar.items.hotbar_items[0] = new Cubby(c);
                    w.players[player_id].avatar.items.cur_left_hand = 0;
                    w.players[player_id].avatar.items.equip_item(w.players[player_id].avatar, d, true, false);
                }
                else
                {
                    w.players[player_id].avatar.items.hotbar_items[1] = new Cubby(c);
                    w.players[player_id].avatar.items.cur_right_hand = 1;
                    w.players[player_id].avatar.items.equip_item(w.players[player_id].avatar, d, false, true);
                }
            }
            NetOutgoingMessage i = udp_server.CreateMessage();
            i.Write((byte)19);
            i.Write((byte)player_id);
            i.Write(is_left);
            c.send_cubby(i);
            udp_server.SendToAll(i, p.SenderConnection, NetDeliveryMethod.Unreliable,0);
        }

        public void receive_swing_arms(NetIncomingMessage p, Display d, World w)
        {
            int player_id = p.ReadByte();
            bool is_left = p.ReadBoolean();
            if (player_id != Exilania.game_my_user_id)
            {
                if (is_left)
                {
                    if ((!w.players[player_id].avatar.items.temporary.is_empty && w.players[player_id].avatar.items.temporary.phys_item != null && w.players[player_id].avatar.items.temporary.phys_item.can_swing) ||
                        w.players[player_id].avatar.items.cur_left_hand != -1 && w.players[player_id].avatar.items.hotbar_items[w.players[player_id].avatar.items.cur_left_hand].phys_item != null
                        && w.players[player_id].avatar.items.hotbar_items[w.players[player_id].avatar.items.cur_left_hand].phys_item.can_swing)
                    {
                        if (!w.players[player_id].avatar.items.temporary.is_empty && w.players[player_id].avatar.items.temporary.phys_item != null && w.players[player_id].avatar.items.temporary.phys_item.can_swing)
                        {
                            w.players[player_id].avatar.block_place_wait_time = w.players[player_id].avatar.items.temporary.phys_item.swing_speed;
                            w.players[player_id].avatar.action_swinging_right = false;
                            w.players[player_id].avatar.action_swinging = true;
                            w.players[player_id].avatar.cur_action_swing_time = 0;
                            w.players[player_id].avatar.action_swing_time = w.players[player_id].avatar.items.temporary.phys_item.swing_speed;
                        }
                        else
                        {
                            w.players[player_id].avatar.block_place_wait_time = w.players[player_id].avatar.items.hotbar_items[w.players[player_id].avatar.items.cur_left_hand].phys_item.swing_speed;
                            w.players[player_id].avatar.action_swinging_right = false;
                            w.players[player_id].avatar.action_swinging = true;
                            w.players[player_id].avatar.cur_action_swing_time = 0;
                            w.players[player_id].avatar.action_swing_time = w.players[player_id].avatar.items.hotbar_items[w.players[player_id].avatar.items.cur_left_hand].phys_item.swing_speed;
                        }
                    }
                }
                else
                {
                    if (w.players[player_id].avatar.items.cur_right_hand != -1 && w.players[player_id].avatar.items.hotbar_items[w.players[player_id].avatar.items.cur_right_hand].phys_item != null 
                        && w.players[player_id].avatar.items.hotbar_items[w.players[player_id].avatar.items.cur_right_hand].phys_item.can_swing)
                    {
                        w.players[player_id].avatar.block_place_wait_time = w.players[player_id].avatar.items.hotbar_items[w.players[player_id].avatar.items.cur_right_hand].phys_item.swing_speed;
                        w.players[player_id].avatar.action_swinging_right = true;
                        w.players[player_id].avatar.action_swinging = true;
                        w.players[player_id].avatar.cur_action_swing_time = 0;
                        w.players[player_id].avatar.action_swing_time = w.players[player_id].avatar.items.hotbar_items[w.players[player_id].avatar.items.cur_right_hand].phys_item.swing_speed;
                    }
                }
            }
            NetOutgoingMessage i = udp_server.CreateMessage();
            i.Write((byte)20);
            i.Write((byte)player_id);
            i.Write(is_left);
            udp_server.SendToAll(i, p.SenderConnection, NetDeliveryMethod.Unreliable,0);
        }

        public void send_world_time(float time)
        {
            NetOutgoingMessage i = udp_server.CreateMessage();
            i.Write((byte)21);
            i.WriteTime(false);
            i.Write(time);
            udp_server.SendToAll(i, NetDeliveryMethod.ReliableOrdered);
        }

        public void send_death_notice(DamageMove t, World w)
        {
            NetOutgoingMessage m = udp_server.CreateMessage();
            if (t.target == TargetType.Player)
            {
                w.players[t.target_id].avatar.world_loc.X = w.world_spawn_loc.X;
                w.players[t.target_id].avatar.world_loc.Y = w.world_spawn_loc.Y;
                w.players[t.target_id].avatar.dloc_server = w.players[t.target_id].avatar.world_loc;
                w.players[t.target_id].avatar.stats.life.change_val(w.players[t.target_id].avatar.stats.life.max_val);
                w.players[t.target_id].avatar.stats.power.change_val(w.players[t.target_id].avatar.stats.power.max_val);
                w.players[t.target_id].avatar.stats.ammo.change_val(w.players[t.target_id].avatar.stats.ammo.max_val);
                w.players[t.target_id].avatar.stats.breath.change_val(w.players[t.target_id].avatar.stats.breath.max_val);
                Exilania.display.add_message("@08" + w.players[t.target_id].avatar.name + " has " + t.damage_code.ToString().ToLower() + "!");
            }
            else
            {
                w.npcs[t.target_id].empty = true;
            }
            m.Write((byte)22);
            t.send_damage(m);
            udp_server.SendToAll(m, NetDeliveryMethod.ReliableOrdered);
        }

        public void receive_damage_request(NetIncomingMessage m, Display d, World w)
        {
            d.add_message("@05Attack message length: " + m.LengthBytes);
            int num_attack = m.ReadUInt16();
            for (int i = 0; i < num_attack; i++)
            {
                DamageMove t = new DamageMove(m);
                Rectangle target_rect = new Rectangle();
                if (t.target == TargetType.Player)
                {
                    target_rect = new Rectangle((int)w.players[t.target_id].avatar.world_loc.X - w.players[t.target_id].avatar.bounding_box.Width / 2,
                        (int)w.players[t.target_id].avatar.world_loc.Y - w.players[t.target_id].avatar.bounding_box.Height / 2,
                        w.players[t.target_id].avatar.bounding_box.Width, w.players[t.target_id].avatar.bounding_box.Height);
                }
                else
                {
                    target_rect = new Rectangle((int)w.npcs[t.target_id].world_loc.X - w.npcs[t.target_id].bounding_box.Width / 2,
                        (int)w.npcs[t.target_id].world_loc.Y - w.npcs[t.target_id].bounding_box.Height / 2, w.npcs[t.target_id].bounding_box.Width, w.npcs[t.target_id].bounding_box.Height);
                }
                Circle c = new Circle(new Point((int)w.players[t.attacker_id].avatar.world_loc.X, (int)w.players[t.attacker_id].avatar.world_loc.Y), t.range);
                if (c.intersects_rectangle(target_rect))
                {
                    damages.Add(t);
                }
            }
        }

        /// <summary>
        /// this function is called to clear out the damages list.
        /// </summary>
        /// <param name="w"></param>
        public void process_damage(World w)
        {
            NetOutgoingMessage i = udp_server.CreateMessage();
            i.Write((byte)23);
            i.Write((ushort)damages.Count);
            foreach (var m in damages)
            {
                if (m.target == TargetType.Player)
                {
                    w.players[m.target_id].avatar.damages.Add(m);
                }
                else
                {
                    w.npcs[m.target_id].damages.Add(m);
                }
                m.send_damage(i);
            }
            damages.Clear();
            udp_server.SendToAll(i, NetDeliveryMethod.ReliableOrdered);
        }

        public void receive_changed_chest(NetIncomingMessage p, World w)
        {
            byte player_id = p.ReadByte();
            ushort furn_id = p.ReadUInt16();
            byte item_id = p.ReadByte();
            Cubby temp = new Cubby(p);
            
            if (player_id != Exilania.game_my_user_id)
            {
                //this is not originating from the server, do something about it
                for (int i = 0; i < w.chests.Count; i++)
                {
                    if (w.chests[i].furniture_id == furn_id)
                    {
                        w.chests[i].items[item_id] = new Cubby(temp);
                        i = w.chests.Count;
                    }
                }
            }
            NetOutgoingMessage j = udp_server.CreateMessage();
            j.Write((byte)24);
            j.Write((byte)player_id);
            j.Write((ushort)furn_id);
            j.Write((byte)item_id);
            temp.send_cubby(j);
            udp_server.SendToAll(j, p.SenderConnection, NetDeliveryMethod.ReliableOrdered, 0);
        }

        public void receive_place_plant(NetIncomingMessage i, World w)
        {
            w.plants.Add(new Plant(i));
            w.plants[w.plants.Count - 1].reset_map(w, w.plants.Count - 1);
            NetOutgoingMessage m = udp_server.CreateMessage();
            m.Write((byte)25);
            w.plants[w.plants.Count - 1].send_plant(m);
            udp_server.SendToAll(m, i.SenderConnection, NetDeliveryMethod.ReliableOrdered, 0);
        }

        public void send_placed_plant(World w, int plant_id)
        {
            NetOutgoingMessage m = udp_server.CreateMessage();
            m.Write((byte)25);
            w.plants[plant_id].send_plant(m);
            udp_server.SendToAll(m, NetDeliveryMethod.ReliableOrdered);
        }

        public void send_grow_plants(List<GrowthEvent> g)
        {
            NetOutgoingMessage m = udp_server.CreateMessage();
            m.Write((byte)26);
            m.Write((ushort)g.Count);
            for (int i = 0; i < g.Count; i++)
            {
                g[i].send_growth_event(m);
            }
            udp_server.SendToAll(m, NetDeliveryMethod.ReliableOrdered);
        }
    }
}
