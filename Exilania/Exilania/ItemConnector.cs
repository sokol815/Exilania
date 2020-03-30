using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exilania
{
    /// <summary>
    /// these Enums are used to describe the relationships between furniture and other things in the world (like actors)
    /// using the enum in conjunction with a TargetType and then 2 ints to describe everything necessary
    /// </summary>
    public enum ItemConnectionType
    {
        Master = 0, //master to this target
        Slave = 1, //slave to this target
        PowerReceiving = 2, //using power from this target
        PowerCharging = 3, //charging this target
        Healing = 4, //healing this target
        Attacking = 5, //attack a target... like a gun or something
        Defending = 6, //defend a target... e.g. with shields?
        Proximity = 7, //has activated a proximity sensor
        Repulsing = 8, //push things away from you
        Attracting = 9, //tractor beam things towards you
        LiquidPumpFrom = 10,//receiving liquid from a liquidstore or a liquidpumpto
        LiquidPumpTo = 11, //Sending liquid to either a liquidpumpfrom or to a liquid store
        GravityNullify = 12, //removing gravity in the area
        Drilling = 13, //drilling the surrounding area
        LiquidStore = 14, //use with liquid pump from to add liquid to a container
        LiquidRelease = 15, //sending liquid to another source
        Grapple = 16, //used for grappling objects, or grappling blocks.
        Default = 17 //do not connect.
    }

    public enum TargetType
    {
        Furniture = 0, Player = 1, NPC = 2, Projectile = 3, World = 4, Vehicle = 5, Empty = 6,zone = 7
    }

    /// <summary>
    /// intended to describe relationships between objects and items they interact with
    /// </summary>
    public class ItemConnector
    {
        public ItemConnectionType conn_type;
        public TargetType target_type;
        public int target_id;
        public int data_one;
        public int data_two;

        public ItemConnector()
        {

        }

        public ItemConnector(int ptarget_id, TargetType t, ItemConnectionType ict, int d1, int d2)
        {
            conn_type = ict;
            target_type = t;
            target_id = ptarget_id;
            data_one = d1;
            data_two = d2;
        }

        public ItemConnector(System.IO.BinaryReader r)
        {
            conn_type = (ItemConnectionType)r.ReadByte();
            target_type = (TargetType)r.ReadByte();
            target_id = r.ReadInt32();
            data_one = r.ReadInt32();
            data_two = r.ReadInt32();
        }

        public ItemConnector(Lidgren.Network.NetIncomingMessage r)
        {
            conn_type = (ItemConnectionType)r.ReadByte();
            target_type = (TargetType)r.ReadByte();
            target_id = r.ReadInt32();
            data_one = r.ReadInt32();
            data_two = r.ReadInt32();
        }

        public void write_connector(System.IO.BinaryWriter w)
        {
            w.Write((byte)conn_type);
            w.Write((byte)target_type);
            w.Write(target_id);
            w.Write(data_one);
            w.Write(data_two);
        }

        public void send_connector(Lidgren.Network.NetOutgoingMessage w)
        {
            w.Write((byte)conn_type);
            w.Write((byte)target_type);
            w.Write(target_id);
            w.Write(data_one);
            w.Write(data_two);
        }

        public override string ToString()
        {
            return "@18Connected to " + target_type.ToString() + ":" + target_id + " in " + conn_type.ToString() + " mode.";
        }
    }
}
