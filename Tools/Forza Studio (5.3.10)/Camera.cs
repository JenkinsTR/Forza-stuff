using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ForzaStudio
{
    public class Camera
    {
        private Panel Viewport;

        public float MaxLookAngle { get; set; }
        public float MinLookAngle { get; set; }
        public float MaxMoveSpeed { get; set; }
        public float MinMoveSpeed { get; set; }
        public float MaxLookSpeed { get; set; }
        public float MinLookSpeed { get; set; }
        public float MaxMoveAccel { get; set; }
        public float MinMoveAccel { get; set; }
        public float MaxLookAccel { get; set; }
        public float MinLookAccel { get; set; }
        public float MinFieldOfView { get; set; }
        public float MaxFieldOfView { get; set; }

        // orientation
        public readonly Matrix World = Matrix.Identity;
        public Matrix View { get; set; }
        public Matrix Projection { get; set; }

        // need to track velocities
        private bool hasChanged = true;
        public bool HasChanged
        {
            get
            {
                return hasChanged;
            }
        }

        private Vector3 Position;   // Point3
        private Vector3 Velocity;

        public Vector3 ForwardDirection;
        public Vector3 VerticalDirection;
        public Vector3 HorizontalDirection;
        public Vector3 LookAt;

        private Vector2 LookAngle;
        private Vector2 LookVelocity;

        private float FieldOfView;
        private float SpeedScale;
        private float LookSpeedScale;
        private float MoveAcceleration;
        private float LookAcceleration;
        private float ZoomVelocity;
        private float ViewDistance;


        public Camera(Panel viewport)
        {
            Viewport = viewport;

            SpeedScale = 0.02f;
            LookSpeedScale = 0.4f;
            MoveAcceleration = 0.6f;    // lower the value, the slower the acceleration (1 = instantaneous movement)
            LookAcceleration = 0.5f;
            ZoomVelocity = 0;
            ViewDistance = 1000;
            MaxLookAngle = 1.57f;
            MinLookAngle = -1.57f;
            MaxMoveSpeed = 5.0f;
            MinMoveSpeed = 0.01f;
            MaxLookSpeed = 5.0f;
            MinLookSpeed = 0.01f;
            MaxMoveAccel = 1.0f;
            MinMoveAccel = 0.01f;
            MaxLookAccel = 1.0f;
            MinLookAccel = 0.01f;
            FieldOfView = DegreesToRadians(90.0f);
            MinFieldOfView = DegreesToRadians(0.05f);
            MaxFieldOfView = DegreesToRadians(120.0f);
        }

        // immediates
        public void MoveTo(Vector3 position)
        {
            Position = position;
            hasChanged = true;
        }
        public void LookTo(Vector2 direction)
        {
            LookAngle = direction;
            hasChanged = true;
        }

        // acceleration
        public void ApplyForce(Vector3 force)
        {
            Velocity += force * SpeedScale;
            hasChanged = true;
        }
        public void ApplyLookForce(Vector2 force)
        {
            LookVelocity = force * LookSpeedScale;
            hasChanged = true;
        }
        public void ApplyZoomForce(float force)
        {
            ZoomVelocity += force;
        }

        public void Update()
        {
            // update velocities and changed status
            Velocity -= Velocity * MoveAcceleration;
            LookVelocity -= LookVelocity * LookAcceleration;
            ZoomVelocity -= ZoomVelocity * 0.2f;
            hasChanged = Math.Abs(Velocity.Length()) > 0.0001 || Math.Abs(LookVelocity.Length()) > 0.0001 || Math.Abs(ZoomVelocity) > 0.0001;

            // update position and look angles based on velocities
            Position += Velocity;
            LookAngle += LookVelocity;
            FieldOfView *= 1.0f + ZoomVelocity;
            
            // clamp look angles and fov
            LookAngle.X = (float)(((LookAngle.X / (Math.PI * 2)) - (int)(LookAngle.X / (Math.PI * 2))) * Math.PI * 2);	// [-2PI, 2PI]
            LookAngle.Y = Clamp(LookAngle.Y, MinLookAngle, MaxLookAngle);
            FieldOfView = Clamp(FieldOfView, MinFieldOfView, MaxFieldOfView);

            // do some math...
            float sh = (float)Math.Sin(LookAngle.X);
            float sv = (float)Math.Sin(LookAngle.Y);
            float ch = (float)Math.Cos(LookAngle.X);
            float cv = (float)Math.Cos(LookAngle.Y);
            float cx = (float)Math.Cos(LookAngle.X + 1.57f);
            float sx = (float)Math.Sin(LookAngle.X + 1.57f);

            // calculate new look directions
            ForwardDirection = new Vector3(ch * cv, sv, sh * cv);
            VerticalDirection = new Vector3(-ch * sv, cv, -sh * sv);
            HorizontalDirection = new Vector3(cx, 0, sx); // left
            LookAt = Position + ForwardDirection;   // destination

            // calculate transformations
            View = Matrix.CreateLookAt(Position, LookAt, new Vector3(0, 1, 0));
            Projection = Matrix.CreatePerspectiveFieldOfView(FieldOfView, (float)Viewport.Width / (float)Viewport.Height, 0.0001f, ViewDistance);
        }

        public void Zoom(float scale)
        {
            FieldOfView = Clamp(FieldOfView * scale, MinFieldOfView, MaxFieldOfView);
        }

        private float DegreesToRadians(float degrees)
        {
            return (float)(degrees * (Math.PI / 180));
        }

        private float RadiansToDegrees(float radians)
        {
            return (float)(radians * (180 / Math.PI));
        }

        private float Clamp(float value, float lowerBound, float upperBound)
        {
            if (value > upperBound) return upperBound;
            else if (value < lowerBound) return lowerBound;
            else return value;
        }
    }
}
