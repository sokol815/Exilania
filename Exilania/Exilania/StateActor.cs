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
    public class ActorRollback
    {
        public List<Vector2> position;
        public int player_id;

        public ActorRollback()
        {
            position = new List<Vector2>();
        }

        public ActorRollback(int pid)
        {
            position = new List<Vector2>();
            player_id = pid;
        }
    }

    public class BulletRollback
    {
        public float time_fired;
        public Vector2 origin;
        public int owner_id;
        public TargetType owner_type;
        public string damage;
        public Vector2 velocity;
        bool checked_rollback;
        public float max_fly_time;
        public float cur_fly_time;

        public BulletRollback()
        {

        }

        public BulletRollback(float p_time, Vector2 porigin, int powner_id, TargetType powner_type, string pdamage, Vector2 pvelocity, float pfly_time)
        {
            time_fired = p_time;
            origin = porigin;
            owner_id = powner_id;
            owner_type = powner_type;
            damage = pdamage;
            velocity = pvelocity;
            checked_rollback = false;
            max_fly_time = pfly_time;
            cur_fly_time = 0f;
        }
    }

    /// <summary>
    /// contains all the info as to what the actor has been doing and where they have been for the past n ticks... keeps about 1 second of time.
    /// </summary>
    public class WorldServerHitCalc
    {
        public float cur_world_time;
        public List<ActorRollback> players;
        public List<ActorRollback> npcs;
        public List<BulletRollback> bullets;
    }
}
