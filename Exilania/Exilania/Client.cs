using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace Exilania
{
    public class Client
    {
        public NetClient udp_client;

        public Client()
        {
            udp_client = new NetClient(Exilania.udp_client_config);
        }

        public void connect(string host, int port, Exilania g)
        {
            udp_client.Start();
            NetOutgoingMessage hail = udp_client.CreateMessage();
            hail.Write("This is the hail message from ");
            udp_client.Connect(host, port, hail);
        }

        static public bool empty_players_found = false;
        /// <summary>
        /// this is used for managing all clients to make sure they do not get behind in key frames.
        /// </summary>
        static public int updates_per_draw = 1;

        public void client_messages(Exilania g, Display d, ref World w)
        {
            NetIncomingMessage inc;

            while ((inc = udp_client.ReadMessage()) != null)
            {
                switch (inc.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        consume_message(inc, g, d, ref w, g);
                        break;
                    case NetIncomingMessageType.DiscoveryResponse:
                        d.add_message("Client: Received response from Server. Connected.");
                        udp_client.Connect(inc.SenderEndPoint);

                        break;
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.ErrorMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.VerboseDebugMessage:
                        Exilania.text_stream.WriteLine("Client: Unhandled type: " + inc.MessageType + " " + inc.ReadString());
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)inc.ReadByte();
                        string reason = inc.ReadString();
                        if (status == NetConnectionStatus.Connected)
                            d.add_message("Client: Received response from Server. Connected.");
                        else if (status == NetConnectionStatus.Disconnected)
                        {
                            d.add_message("Client: Connect refused.. " + reason);
                            if (Exilania.game_my_user_id != -1)
                            {
                                Exilania.saved_players.players[Exilania.cur_using_local_id] = w.players[Exilania.game_my_user_id];
                                Exilania.saved_players.save_players();
                                if(Exilania.game_server)
                                    w.write_world();
                            }
                            Exilania.gstate = 10;
                        }
                        Exilania.text_stream.WriteLine("Client: Status changed: " + reason);
                        break;


                    default:
                        Exilania.text_stream.WriteLine("Client: Unhandled type: " + inc.MessageType + " " + inc.ReadString());

                        break;

                }
                udp_client.Recycle(inc);
            }
        }

        public void consume_message(NetIncomingMessage inc, Exilania g, Display d, ref World w, Exilania e)
        {
            byte type = inc.ReadByte();
            switch (type)
            {
                case 1: //received confirmation that we have been logged in.. requesting player
                    greet_server(inc, d, w);
                    break;
                case 2: //receiving a chat
                    receive_chat(inc, g, d, w);
                    break;
                case 3: //receiving player list initialization
                    receive_all_players_and_id(inc, d, w);
                    break;
                case 4: //receiving new player
                    receive_player(inc, d, w);
                    break;
                case 5: // receiving individual position coordinates --- uses unreliable sequenced 1)
                    receive_position_coordinates(inc, w);
                    break;
                case 6: //receiving mouse and position of other players.
                    receive_mouse_and_position_coordinates(inc, w);
                    break;
                case 7: //receive input information
                    receive_key_commands(inc, d, w);
                    break;
                case 8://receive block remove
                    receive_remove_block(inc, d, w);
                    break;
                case 9://receive block place
                    receive_place_block(inc, d, w);
                    break;
                case 10://receive world
                    receive_world(inc, d, ref w, e);
                    break;
                case 11://receive run trugger
                    receive_run_trigger(inc, d, w);
                    break;
                case 12: //receive shoot bullet
                    receive_shoot_bullet(inc, d, w);
                    break;
                case 13://receive place furniture
                    receive_place_furniture(inc, d, w);
                    break;
                case 14://receive remove furniture
                    receive_remove_furniture(inc, d, w);
                    break;
                case 15://receive furniture connection
                    receive_furniture_connection(inc, d, w);
                    break;
                case 16://receive furniture disconnect
                    receive_furniture_disconnect(inc, d, w);
                    break;
                case 17://receive attack plant
                    receive_plant_attack(inc, d, w);
                    break;
                case 18://receive place plant
                    break;
                case 19://receive character switch item in hands
                    receive_changed_item_in_hands(inc, d, w);
                    break;
                case 20://receive swing arms
                    receive_swing_arms(inc, d, w);
                    break;
                case 21://receive time of day
                    receive_world_time(inc, d, w);
                    break;
                case 22://receive death notice of some creature
                    receive_death_notice(inc, w);
                    break;
                case 23://receive damage of some creature
                    receive_damage_notice(inc, w);
                    break;
                case 24: //receive changed chest
                    receive_changed_chest(inc, w);
                    break;
                case 25: //receive place plant
                    receive_place_plant(inc, w);
                    break;
                case 26: //receive grow plant
                    receive_grow_plants(inc, w);
                    break;
                case 64://client has quit.. remove them!
                    remove_client(inc, d, w);
                    break;
            }
        }

        /// <summary>
        /// the server has acknowledged that we have signed on. here it is sending us specific information
        /// </summary>
        /// <param name="p"></param>
        /// <param name="d"></param>
        /// <param name="w"></param>
        public void greet_server(NetIncomingMessage p, Display d, World w)
        {
            Exilania.people_connected = p.ReadByte();
            Exilania.settings.force_new_character = p.ReadBoolean();
            if (Exilania.settings.force_new_character)
                Exilania.gstate = 70;
            else
                Exilania.gstate = 60;
        }

        public void send_chat(int my_user_id, string message)
        {
            NetOutgoingMessage m = udp_client.CreateMessage();
            m.Write((byte)2);
            m.Write((byte)my_user_id);
            m.Write(message);
            udp_client.SendMessage(m, NetDeliveryMethod.ReliableOrdered,5);
        }

        public void receive_chat(NetIncomingMessage p, Exilania g, Display d, World w)
        {
            byte player_id = p.ReadByte();
            string msg = p.ReadString();
            if (player_id < w.players.Count)
            {
                if (msg[0] == '!' && Exilania.gstate == 100)
                {
                    for (int i = 0; i < msg.Length; i++)
                    {
                        if (msg[i] == '!')
                        {
                            msg = msg.Substring(0, i) + msg.Substring(i + 1);
                            break;
                        }
                    }
                    d.fading_text.Add(new FadeText("@00"+msg, 3000, (int)w.players[player_id].avatar.world_loc.X, 
                        (int)w.players[player_id].avatar.world_loc.Y - 60, true,TargetType.Player,player_id,w));
                }
                else
                    d.add_message("@05" + w.players[player_id].avatar.name + "@00: " + msg);
                Exilania.text_stream.WriteLine("@05" + w.players[player_id].avatar.name + "@00: " + msg);
            }
            else
            {
                d.add_message("@05New Player@00: " + msg);
                Exilania.text_stream.WriteLine(Acc.sanitize_text_color("@05New Player@00: " + msg));
            }
        }

        /// <summary>
        /// send to the server your character information.
        /// </summary>
        public void send_user_info(Player p)
        {
            NetOutgoingMessage m = udp_client.CreateMessage();
            m.Write((byte)3); //sending player data
            p.send_player(m);
            udp_client.SendMessage(m, NetDeliveryMethod.ReliableUnordered);
            Exilania.text_stream.WriteLine("Client sending player chosen to server.");
        }

        /// <summary>
        /// corresponds to server send message 3
        /// </summary>
        /// <param name="p"></param>
        /// <param name="d"></param>
        /// <param name="w"></param>
        public void receive_all_players_and_id(NetIncomingMessage p, Display d, World w)
        {
            double dtime = NetTime.Now - p.ReadTime(false);
            w.world_time = p.ReadSingle() + (float)dtime;
            Exilania.game_my_user_id = p.ReadByte();
            if (!Exilania.game_server)
            {
                int num_players = p.ReadByte();
                for (int x = 0; x < num_players; x++)
                {
                    Player temp = new Player(p);
                    temp.avatar.world_loc = new Vector2(p.ReadSingle(), p.ReadSingle());
                    w.players.Add(temp);
                    /*w.minimap.map_pieces.Add(new MapMarker(new Point((int)temp.avatar.world_loc.X / 24, (int)temp.avatar.world_loc.Y / 24), MarkerType.OTHER_PLAYER, w.players.Count - 1));
                    if (w.players.Count - 1 == Exilania.game_my_user_id)
                    {
                        w.minimap.map_pieces[w.minimap.map_pieces.Count - 1].type = MarkerType.PLAYER;
                    }*/
                }
            }
            w.do_composition_lighting();
            w.players[Exilania.game_my_user_id].avatar.input.hold_left_mouse_click = false;
            d.add_message("@05You (" + w.players[w.players.Count - 1].avatar.name + "@05) have joined the server.");
            d.add_message("@05There " + (Exilania.people_connected == 1 ? "is" : "are") + " now " + Exilania.people_connected + (Exilania.people_connected == 1 ? " person" : " people") + " in this game.");
            Exilania.gstate = 100;
            w.total_fps = new Timing("SPF");
        }

        public void receive_player(NetIncomingMessage p, Display d, World w)
        {
            if (!Exilania.game_server)
            {
                Player temp = new Player(p);
                byte player_id = p.ReadByte();
                temp.avatar.world_loc = new Vector2(p.ReadSingle(), p.ReadSingle());
                if (w.players.Count > player_id)
                {
                    w.players[player_id] = temp;
                }
                else
                    w.players.Add(temp);
                d.add_message("@05Player " + w.players[player_id].avatar.name + " has joined us.");
                d.temp_show_chat = true;
            }
        }


        /// <summary>
        /// sequenced 1 is the mouse and cur_stats.
        /// </summary>
        /// <param name="mouse_loc"></param>
        public void send_mouse_coordinates(Point mouse_loc, Actor me)
        {
            NetOutgoingMessage m = udp_client.CreateMessage();
            m.Write((byte)5);
            m.Write((byte)Exilania.game_my_user_id);
            m.Write(mouse_loc.X);
            m.Write(mouse_loc.Y);
            udp_client.SendMessage(m, NetDeliveryMethod.UnreliableSequenced, 1);
        }

        /// <summary>
        /// sending packet 6 causes this to occur.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="w"></param>
        public void receive_mouse_and_position_coordinates(NetIncomingMessage p, World w)
        {
            int player_id = p.ReadByte();
            if (!Exilania.game_server && w!= null && w.players.Count > player_id)
            {
                w.players[player_id].avatar.input.mouse_loc.X = p.ReadInt32();
                w.players[player_id].avatar.input.mouse_loc.Y = p.ReadInt32();
                w.players[player_id].avatar.dloc_server.X = p.ReadSingle();
                w.players[player_id].avatar.dloc_server.Y = p.ReadSingle();
            }
        }

        /// <summary>
        /// you sent your mouse coords, now it sends back your position. this is server packet 5
        /// </summary>
        /// <param name="p"></param>
        /// <param name="w"></param>
        public void receive_position_coordinates(NetIncomingMessage p, World w)
        {
            int player_id = p.ReadByte();
            if (!Exilania.game_server && w != null && w.players.Count > player_id)
            {
                w.players[player_id].avatar.dloc_server.X = p.ReadSingle();
                w.players[player_id].avatar.dloc_server.Y = p.ReadSingle();
            }
        }

        public void send_key_commands(scheduled_movement t)
        {
            NetOutgoingMessage m = udp_client.CreateMessage();
            m.Write((byte)7);
            m.Write((byte)Exilania.game_my_user_id);
            t.send_movement(m);
            udp_client.SendMessage(m, NetDeliveryMethod.UnreliableSequenced, 2);
        }

        public void receive_key_commands(NetIncomingMessage p, Display d, World w)
        {
            int player_id = p.ReadByte();
            if (!Exilania.game_server && w != null && w.players.Count > player_id)
            {
                float dtime = ((float)(NetTime.Now - p.ReadTime(true)));
                w.world_time = p.ReadSingle() + dtime;
                w.tiles_spread_frame = (int)w.world_time / (int)World.tiles_spread_sec_between_frame;
                scheduled_movement t = new scheduled_movement(p);
                w.players[player_id].avatar.input.movement_schedule.Add(t);
            }
        }

        public void send_remove_block(Point location, bool bkd, World w)
        {
            NetOutgoingMessage m = udp_client.CreateMessage();
            m.Write((byte)8);
            m.Write((byte)Exilania.game_my_user_id);
            m.Write((ushort)w.wraparound_x(location.X));
            m.Write((ushort)location.Y);
            m.Write(bkd);
            udp_client.SendMessage(m, NetDeliveryMethod.ReliableSequenced, 3);
        }

        public void receive_remove_block(NetIncomingMessage p, Display d, World w)
        {
            int player_id = p.ReadByte();
            if (!Exilania.game_server && w != null && w.players.Count > player_id)
            {
                w.remove_block(new Point(p.ReadUInt16(), p.ReadUInt16()), p.ReadBoolean());
            }
        }

        public void send_place_block(Point location, bool bkd, byte block_type, World w)
        {
            NetOutgoingMessage m = udp_client.CreateMessage();
            m.Write((byte)9);
            m.Write((byte)Exilania.game_my_user_id);
            m.Write((ushort)w.wraparound_x(location.X));
            m.Write((ushort)location.Y);
            m.Write(bkd);
            m.Write(block_type);
            udp_client.SendMessage(m, NetDeliveryMethod.ReliableSequenced, 3);
        }

        public void receive_place_block(NetIncomingMessage p, Display d, World w)
        {
            int player_id = p.ReadByte();
            if (!Exilania.game_server && w != null && w.players.Count > player_id)
            {
                w.place_block(new Point(p.ReadUInt16(), p.ReadUInt16()), (sbyte)p.ReadByte(), p.ReadBoolean(),true);
            }
        }

        public void receive_world(NetIncomingMessage p, Display d, ref World w, Exilania e)
        {
            
            string world_name = p.ReadString();
            //world_name = "Received_World";
            int num_bytes = p.ReadInt32();
            Exilania.text_stream.WriteLine("Received a world entitled " + world_name + " filesize: " + num_bytes + " bytes.");
            byte[] bt_arr = new byte[num_bytes];
            bt_arr = p.ReadBytes(num_bytes);
            System.IO.File.WriteAllBytes(@"worlds/network_client_game.wlz", bt_arr);

            using (System.IO.FileStream fs = new System.IO.FileStream(@"worlds/network_client_game.wld", System.IO.FileMode.Create, System.IO.FileAccess.Write))
            using (System.IO.FileStream fd = new System.IO.FileStream(@"worlds/network_client_game.wlz", System.IO.FileMode.Open, System.IO.FileAccess.Read))
            using (System.IO.Stream csStream = new System.IO.Compression.GZipStream(fd,System.IO.Compression.CompressionMode.Decompress))
            {
                byte[] buffer = new byte[1024];
                int nRead;
                while ((nRead = csStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fs.Write(buffer, 0, nRead);
                }

            }
            /*System.Security.Cryptography.HashAlgorithm ha = System.Security.Cryptography.HashAlgorithm.Create();
            System.IO.FileStream fs = new System.IO.FileStream(@"worlds/" + world_name + ".wld", System.IO.FileMode.Open, System.IO.FileAccess.Read);
            byte[] hash = ha.ComputeHash(fs);
            fs.Close();
            Exilania.text_stream.WriteLine("Hash code of received world: " + BitConverter.ToString(hash));*/

            w = new World("network_client_game",e.GraphicsDevice);
            System.IO.File.Delete(@"worlds/network_client_game.wld");
            System.IO.File.Delete(@"worlds/network_client_game.wlz");

        }

        public void send_run_trigger(int furniture_id, int trigger_id)
        {
            NetOutgoingMessage p = udp_client.CreateMessage();
            p.Write((byte)11);
            p.Write((byte)Exilania.game_my_user_id);
            p.Write(furniture_id);
            p.Write((byte)trigger_id);
            udp_client.SendMessage(p, NetDeliveryMethod.ReliableOrdered, 4);
        }

        public void receive_run_trigger(NetIncomingMessage p, Display d, World w)
        {
            int player_id = p.ReadByte();
            if (!Exilania.game_server && w != null && w.players.Count > player_id)
            {
                int furniture_id = p.ReadInt32();
                byte trigger_id = p.ReadByte();
                switch (trigger_id)
                {
                    case 0://click function
                        w.furniture[furniture_id].run_click_function(w.players[player_id].avatar, w);
                        break;
                }
            }
        }

        public void fire_bullet(int pic_id, double angle)
        {
            NetOutgoingMessage p = udp_client.CreateMessage();
            p.Write((byte)12);
            p.Write((byte)Exilania.game_my_user_id);
            p.Write((byte)pic_id);
            p.Write((float)angle);
            udp_client.SendMessage(p, NetDeliveryMethod.ReliableOrdered, 6);
        }

        public void receive_shoot_bullet(NetIncomingMessage p, Display d, World w)
        {
            int player_id = p.ReadByte();
            if (!Exilania.game_server && w != null && w.players.Count > player_id)
            {
                byte pic = p.ReadByte();
                float angle = p.ReadSingle();
                Particle.initialize_particle(w.bullets, Exilania.particle_manager.get_particle_id_by_name("Steel Ball"), 
                    5f, angle, (int)w.players[player_id].avatar.world_loc.X, (int)w.players[player_id].avatar.world_loc.Y, 1,false,  d);
            }
        }

        public void send_place_furniture(Point world_loc, int furniture_id)
        {
            NetOutgoingMessage p = udp_client.CreateMessage();
            p.Write((byte)13);
            p.Write((byte)Exilania.game_my_user_id);
            p.Write(world_loc.X);
            p.Write(world_loc.Y);
            p.Write(furniture_id);
            udp_client.SendMessage(p, NetDeliveryMethod.ReliableOrdered, 4);
        }

        public void receive_place_furniture(NetIncomingMessage p, Display d, World w)
        {
            int player_id = p.ReadByte();
            if (!Exilania.game_server && w != null && w.players.Count > player_id)
            {
                Point loc = new Point(p.ReadInt32(), p.ReadInt32());
                int furniture_id = p.ReadInt32();
                Exilania.furniture_manager.furniture[furniture_id].try_place(w.players[player_id].avatar, loc, w, new Rectangle());
            }
        }

        public void send_remove_furniture(Point world_loc)
        {
            NetOutgoingMessage p = udp_client.CreateMessage();
            p.Write((byte)14);
            p.Write((byte)Exilania.game_my_user_id);
            p.Write(world_loc.X);
            p.Write(world_loc.Y);
            udp_client.SendMessage(p, NetDeliveryMethod.ReliableOrdered, 4);
        }

        public void receive_remove_furniture(NetIncomingMessage p, Display d, World w)
        {
            int player_id = p.ReadByte();
            if (!Exilania.game_server && w != null && w.players.Count > player_id)
            {
                Point loc = new Point(p.ReadInt32(), p.ReadInt32());
                int id_pickup = w.furniture[w.map[loc.X, loc.Y].furniture_index].furniture_id;
                Exilania.furniture_manager.furniture[id_pickup].remove_furniture(w, loc);
            }
        }

        public void send_furniture_connection(Point furn_from, Point furn_to, ItemConnectionType t)
        {
            NetOutgoingMessage p = udp_client.CreateMessage();
            p.Write((byte)15);
            p.Write((byte)Exilania.game_my_user_id);
            p.Write((ushort)furn_from.X);
            p.Write((ushort)furn_from.Y);
            p.Write((ushort)furn_to.X);
            p.Write((ushort)furn_to.Y);
            p.Write((byte)t);
            udp_client.SendMessage(p, NetDeliveryMethod.ReliableOrdered, 4);
        }

        public void receive_furniture_connection(NetIncomingMessage p, Display d, World w)
        {
            int player_id = p.ReadByte();
            if (!Exilania.game_server && w != null && w.players.Count > player_id)
            {
                int furn_from = w.map[p.ReadUInt16(), p.ReadUInt16()].furniture_index;
                int furn_to = w.map[p.ReadUInt16(), p.ReadUInt16()].furniture_index;
                ItemConnectionType t = (ItemConnectionType)p.ReadByte();
                w.furniture[furn_from].connections.Add(new ItemConnector(furn_to, TargetType.Furniture, t, 0, 0));
            }
        }

        public void send_furniture_disconnect(Point furn_from, Point furn_to)
        {
            NetOutgoingMessage p = udp_client.CreateMessage();
            p.Write((byte)16);
            p.Write((byte)Exilania.game_my_user_id);
            p.Write((ushort)furn_from.X);
            p.Write((ushort)furn_from.Y);
            p.Write((ushort)furn_to.X);
            p.Write((ushort)furn_to.Y);
            udp_client.SendMessage(p, NetDeliveryMethod.ReliableOrdered, 4);
        }

        public void receive_furniture_disconnect(NetIncomingMessage p, Display d, World w)
        {
            int player_id = p.ReadByte();
            if (!Exilania.game_server && w != null && w.players.Count > player_id)
            {
                int furn_from = w.map[p.ReadUInt16(), p.ReadUInt16()].furniture_index;
                int furn_to = w.map[p.ReadUInt16(), p.ReadUInt16()].furniture_index;
                for (int x = 0; x < w.furniture[furn_from].connections.Count; x++)
                {
                    if (w.furniture[furn_from].connections[x].target_type == TargetType.Furniture && w.furniture[furn_from].connections[x].target_id == furn_to)
                    {
                        w.furniture[furn_from].connections.RemoveAt(x);
                        x--;
                    }
                }
            }
        }

        public void send_plant_attack(Point loc)
        {
            NetOutgoingMessage p = udp_client.CreateMessage();
            p.Write((byte)17);
            p.Write((byte)Exilania.game_my_user_id);
            p.Write(loc.X);
            p.Write(loc.Y);
            udp_client.SendMessage(p, NetDeliveryMethod.ReliableOrdered, 4);
        }

        public void receive_plant_attack(NetIncomingMessage p, Display d, World w)
        {
            int player_id = p.ReadByte();
            Point attack_loc = new Point(p.ReadInt32(), p.ReadInt32());
            if (!Exilania.game_server && w != null && w.players.Count > player_id)
            {
                int plant_id = w.map[attack_loc.X, attack_loc.Y].plant_index;
                if (player_id == 0)
                {
                    w.plants[plant_id].unset_map(w);
                    for (int x = plant_id + 1; x < w.plants.Count; x++)
                    {
                        w.plants[x].unset_map(w);
                        w.plants[x].reset_map(w, x - 1);
                    }
                    w.plants.RemoveAt(plant_id);
                }
                else
                {
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
            }
        }

        public void send_changed_item_in_hands(Cubby c, bool is_left)
        {
            NetOutgoingMessage i = udp_client.CreateMessage();
            i.Write((byte)19);
            i.Write((byte)Exilania.game_my_user_id);
            i.Write(is_left);
            c.send_cubby(i);
            udp_client.SendMessage(i, NetDeliveryMethod.Unreliable);
        }

        public void receive_changed_item_in_hands(NetIncomingMessage p, Display d, World w)
        {
            int player_id = p.ReadByte();
            bool is_left = p.ReadBoolean();
            Cubby c = new Cubby(p);
            if (!Exilania.game_server && w != null && w.players.Count > player_id)
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
        }

        public void send_swing_arms(bool is_left)
        {
            NetOutgoingMessage p = udp_client.CreateMessage();
            p.Write((byte)20);
            p.Write((byte)Exilania.game_my_user_id);
            p.Write(is_left);
            udp_client.SendMessage(p, NetDeliveryMethod.Unreliable);
        }

        public void receive_swing_arms(NetIncomingMessage p, Display d, World w)
        {
            int player_id = p.ReadByte();
            bool is_left = p.ReadBoolean();
            if (!Exilania.game_server && w != null && w.players.Count > player_id)
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
        }

        public void receive_world_time(NetIncomingMessage p, Display d, World w)
        {
            if (!Exilania.game_server)
            {
                double dtime = NetTime.Now - p.ReadTime(false);
                w.world_time = p.ReadSingle() + (float)dtime;
            }
        }

        public void receive_death_notice(NetIncomingMessage p, World w)
        {
            if (!Exilania.game_server)
            {
                DamageMove t = new DamageMove(p);
                if (t.target == TargetType.Player)
                {
                    Exilania.display.fading_text.Add(new FadeText("@08-" + t.damage, 800, (int)w.players[t.target_id].avatar.world_loc.X + (Exilania.rand.Next(0, 40) - 20),
                        (int)w.players[t.target_id].avatar.world_loc.Y - (Exilania.rand.Next(20, 40)), true, true));
                    w.players[t.target_id].avatar.world_loc.X = w.world_spawn_loc.X;
                    w.players[t.target_id].avatar.world_loc.Y = w.world_spawn_loc.Y;
                    w.players[t.target_id].avatar.dloc_server = w.players[t.target_id].avatar.world_loc;
                    w.players[t.target_id].avatar.stats.life.change_val(w.players[t.target_id].avatar.stats.life.max_val);
                    w.players[t.target_id].avatar.stats.power.change_val(w.players[t.target_id].avatar.stats.power.max_val);
                    w.players[t.target_id].avatar.stats.ammo.change_val(w.players[t.target_id].avatar.stats.ammo.max_val);
                    w.players[t.target_id].avatar.stats.breath.change_val(w.players[t.target_id].avatar.stats.breath.max_val);
                    w.players[t.target_id].avatar.damages.Clear();
                    Exilania.display.add_message("@08" + w.players[t.target_id].avatar.name + " has " + t.damage_code.ToString().ToLower() + "!");
                }
                else
                {
                    w.npcs[t.target_id].empty = true;
                }
            }
        }

        public void receive_damage_notice(NetIncomingMessage p, World w)
        {
            if (!Exilania.game_server && w.players.Count > 0)
            {
                DamageMove t = new DamageMove();
                int num = p.ReadUInt16();
                for (int i = 0; i < num; i++)
                {
                    t = new DamageMove(p);
                    if (t.target == TargetType.Player)
                    {
                        w.players[t.target_id].avatar.damages.Add(t);
                    }
                    else if (t.target == TargetType.NPC)
                    {
                        w.npcs[t.target_id].damages.Add(t);
                    }
                }
            }
        }

        public void send_damage_notice(List<DamageMove> t)
        {
            NetOutgoingMessage i = udp_client.CreateMessage();
            i.Write((byte)23);
            i.Write((ushort)t.Count);
            for (int p = 0; p < t.Count; p++)
            {
                t[p].send_damage(i);
            }
            udp_client.SendMessage(i, NetDeliveryMethod.ReliableOrdered);
        }

        public void receive_changed_chest(NetIncomingMessage p, World w)
        {
            byte player_id = p.ReadByte();
            ushort furn_id = p.ReadUInt16();
            byte item_id = p.ReadByte();
            Cubby temp = new Cubby(p);

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

        public void send_changed_chest(int furniture_id, int change_id, Cubby item)
        {
            NetOutgoingMessage i = udp_client.CreateMessage();
            i.Write((byte)24);
            i.Write((byte)Exilania.game_my_user_id);
            i.Write((ushort)furniture_id);
            i.Write((byte)change_id);
            item.send_cubby(i);
            udp_client.SendMessage(i, NetDeliveryMethod.ReliableOrdered);
        }

        public void send_place_plant(World w, int plant_id)
        {
            if (!Exilania.game_server)
            {
                NetOutgoingMessage i = udp_client.CreateMessage();
                i.Write((byte)25);
                w.plants[plant_id].send_plant(i);
                udp_client.SendMessage(i, NetDeliveryMethod.ReliableOrdered);
            }
        }

        public void receive_place_plant(NetIncomingMessage p, World w)
        {
            if (!Exilania.game_server)
            {
                w.plants.Add(new Plant(p));
                w.plants[w.plants.Count - 1].reset_map(w, w.plants.Count - 1);
            }
        }

        public void receive_grow_plants(NetIncomingMessage p, World w)
        {
            if (!Exilania.game_server)
            {
                GrowthEvent[] t = new GrowthEvent[p.ReadUInt16()];
                for (int i = 0; i < t.Length; i++)
                {
                    t[i] = new GrowthEvent(p);
                }
                Exilania.plant_manager.run_growth_batch(t, w);
            }
        }

        public void remove_client(NetIncomingMessage p, Display d, World w)
        {
            int player_id = p.ReadByte();
            if (!Exilania.game_server && w != null && w.players.Count > player_id)
            {
                w.players[player_id].is_player_empty = true;
                d.add_message("@03" + w.players[player_id].avatar.name + " @03has quit.");
            }
        }

        public void quit(byte quitter_id)
        {
            NetOutgoingMessage m = udp_client.CreateMessage();
            m.Write((byte)64); // quit message
            m.Write((byte)quitter_id);
            udp_client.SendMessage(m, NetDeliveryMethod.ReliableOrdered,5);
        }
    }
}
