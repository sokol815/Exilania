using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exilania
{
    public enum CauseDamage
    {
        Drowned,
        Sliced,
        Fell
    }


    public class DamageMove
    {
        public float time_execute;
        public TargetType target;
        public ushort attacker_id;
        public ushort target_id;
        public TargetType attacker;
        public short damage;
        public CauseDamage damage_code;
        public ushort range;

        public DamageMove()
        {

        }

        public DamageMove(float ptime, ushort pattacker_id, TargetType pattacker, ushort ptarget_id, TargetType ptarget, short pdamage, CauseDamage p_cause, ushort prange)
        {
            time_execute = ptime;
            attacker = pattacker;
            attacker_id = pattacker_id;
            target = ptarget;
            target_id = ptarget_id;
            damage = pdamage;
            damage_code = p_cause;
            range = prange;
        }

        public DamageMove(Lidgren.Network.NetIncomingMessage p)
        {
            time_execute = p.ReadSingle();
            attacker = (TargetType)p.ReadByte();
            attacker_id = p.ReadUInt16();
            target = (TargetType)p.ReadByte();
            target_id = p.ReadUInt16();
            damage = p.ReadInt16();
            damage_code = (CauseDamage)p.ReadByte();
            range = p.ReadUInt16();
        }

        public void send_damage(Lidgren.Network.NetOutgoingMessage w)
        {
            w.Write(time_execute);
            w.Write((byte)attacker);
            w.Write((ushort)attacker_id);
            w.Write((byte)target);
            w.Write((ushort)target_id);
            w.Write((short)damage);
            w.Write((byte)damage_code);
            w.Write((ushort)range);
        }
    }
}
