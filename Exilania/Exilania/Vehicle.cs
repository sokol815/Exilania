using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;


namespace Exilania
{
    public enum ControlChange
    {
        ToggleDrill = 0,
        Left = 1,
        Right = 2,
        Up = 3,
        Down = 4,
        AutoPilot = 5,
        ToggleAlpha = 6,
        ToggleBeta = 7,
        ToggleDelta = 8,
        ToggleGamma = 9,
        ToggleOmega = 10
    }
    public class KeyAction
    {
        public char key_lowercase;
        public ControlChange control;

        public KeyAction()
        {

        }

    }

    public class Engine
    {
        public float total_thrust_in_pounds; //changes how hard the acceleration is.
        public float muzzle_velocity; //denotes maximum vectorial speed; must be multiplied by the component velocity percentage to get maximum 1d speed.
        /// <summary>
        /// //limit from 0 to 2PI -- compounds with the vehicles angle to create the thrust vector... meaning that anytime the angle changes, the component velocities change, too.
        /// </summary>
        public float base_angle;
        public Vector2 component_velocity_percentages; //denotes exact multiplacations of total_thrust to multiply * seconds elapsed to add to velocity each time unit.
        public float power_usage_per_second;
        public float power_sub_integer_remainder_used_bucket;//holds all partial power usage... integer component 
        public float seconds_running; //used for the spin up of the engine... increases when petal is on the metal, decreases when it is not.
        public int engine_type; //combustion=mid, jet=slow, electric=instant
        public static CubicSpline[] engine_spinup_profiles = new CubicSpline[] {
            new CubicSpline(new double[]{0,2,5,10,100},new double[]{.05,.4,.7,1,1}), //combustion engine
            new CubicSpline(new double[]{0,4,10,30,100},new double[]{.05,.4,.7,1,1}), //jet engine
            new CubicSpline(new double[]{0,30},new double[]{1,1}), //electric engine
        };

        public Engine()
        {
        }

        public Engine(float thrust, float max_velocity, float pangle, float use_power_sec)
        {
            total_thrust_in_pounds = thrust;
            muzzle_velocity = max_velocity;
            base_angle = pangle%((float)Math.PI * 2f);
            if (base_angle < 0)
            {
                base_angle += (float)Math.PI * 2f;
            }

        }
    }

    public class Vehicle
    {
        public Voxel[,] map;
        public string owner;
        public string pressed_keys;
        public string last_pressed_keys;
        public List<KeyAction> key_actions;
        public List<Furniture> furniture;
        public Container hull_integrity;
        public Container air;
        public Container power;
        public bool air_tight;
        public Vector2 world_center;
        public float angle;
        public float max_speed; //inches per second
        public float mass_in_kg; //e.g. 1 block has n kilograms of mass (different materials have different weights)
        //Acceleration = thrust/mass(we'll substitute weight); max speed = thrust muzzle velocity
        public float horizontal_thrust_in_pounds; //pounds force -- larger number = faster acceleration
        public float max_horizontal_speed; //in in/s
        public float vertical_thrust_in_pounds; //pounds force -- larger number = faster acceleration
        public float max_vertical_speed; //in in/s
        public Vector2 velocity; //in in/s... yeah, I know!
        public List<Rectangle> collision_rects;
        public Rectangle main_collision;
    
        public Vehicle()
        {

        }

        public Vehicle(Rectangle dimensions, Vector2 location)
        {

        }

        public Vehicle(Rectangle dimensions, Vehicle old)
        {

        }
    }
}
