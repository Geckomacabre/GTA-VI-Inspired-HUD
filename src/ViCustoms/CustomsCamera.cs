using System;
using System.Collections.Generic;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// Leonida Customs' orbit camera (client/menu.lua setView / cameraLoop): a
    /// view per category (front, rear 3/4, wheel, engine bay with the hood open,
    /// cabin, plate ...), eased to over a few frames, kept on this side of the
    /// garage walls, with a shallow depth of field. The player turns it round
    /// the car and zooms (Orbit), within the limits of the view.
    /// </summary>
    internal sealed class CustomsCamera
    {
        private struct View { public float Yaw, Pitch, Dist, Metres; public string Bone; }

        // menu.lua Views
        private static readonly Dictionary<string, View> Views = new Dictionary<string, View>
        {
            ["front"] = new View { Yaw = 0f, Pitch = 8f, Dist = 1.15f },
            ["front34"] = new View { Yaw = -35f, Pitch = 10f, Dist = 1.2f },
            ["rear"] = new View { Yaw = 180f, Pitch = 6f, Dist = 1.1f },
            ["rear34"] = new View { Yaw = 215f, Pitch = 10f, Dist = 1.2f },
            ["side"] = new View { Yaw = 90f, Pitch = 6f, Dist = 1.15f },
            ["top"] = new View { Yaw = 180f, Pitch = 55f, Dist = 1.2f },
            ["engine"] = new View { Yaw = -12f, Pitch = 52f, Dist = 0.62f, Bone = "engine" },
            ["wheel"] = new View { Yaw = 65f, Pitch = 4f, Dist = 0.5f, Bone = "wheel_lf" },
            ["plate"] = new View { Yaw = 180f, Pitch = 10f, Dist = 0.6f },
            ["cabin"] = new View { Yaw = 180f, Pitch = 14f, Metres = 0.95f, Bone = "seat_dside_f" },
        };

        private struct Pose { public float Yaw, Pitch, Dist, X, Y, Z; }

        private readonly Vehicle _veh;
        private Camera _cam;
        private Pose _now, _goal;
        private float _min, _max;
        private bool _hasGoal;
        public string Name { get; private set; }

        public CustomsCamera(Vehicle veh) { _veh = veh; }

        public void Start(string view)
        {
            SetView(view);
            _cam = World.CreateCamera(Vector3.Zero, Vector3.Zero, 45f);
            Function.Call(Hash.SET_CAM_USE_SHALLOW_DOF_MODE, _cam, true);
            Function.Call(Hash.SET_CAM_NEAR_DOF, _cam, 0.6f);
            Function.Call(Hash.SET_CAM_DOF_STRENGTH, _cam, 0.7f);
            Update(0f);
            Function.Call(Hash.RENDER_SCRIPT_CAMS, true, true, 700, true, true, 0);
        }

        public void Stop(bool ease = true)
        {
            if (_cam == null) return;
            Function.Call(Hash.RENDER_SCRIPT_CAMS, false, ease, ease ? 600 : 0, true, true, 0);
            _cam.Delete();
            _cam = null;
        }

        private Vector3? BoneOffset(string bone)
        {
            int index = Function.Call<int>(Hash.GET_ENTITY_BONE_INDEX_BY_NAME, _veh, bone);
            if (index == -1) return null;
            Vector3 world = Function.Call<Vector3>(Hash.GET_WORLD_POSITION_OF_ENTITY_BONE, _veh, index);
            return _veh.GetPositionOffset(world);
        }

        public void SetView(string name)
        {
            if (name == null || !Views.TryGetValue(name, out View view)) { name = "rear34"; view = Views[name]; }
            Vector3 min, max;
            _veh.Model.GetDimensions(out min, out max);
            float length = max.Y - min.Y, height = max.Z - min.Z;
            var goal = new Pose { Yaw = view.Yaw, Pitch = view.Pitch, Dist = (view.Dist > 0f ? view.Dist : 1f) * length + 1.4f, Z = height * 0.12f };
            bool hood = false, boot = false;
            if (name == "engine")
            {
                Vector3 e = BoneOffset("engine") ?? new Vector3(0f, length * 0.3f, 0.2f);
                goal.X = 0f; goal.Y = e.Y; goal.Z = e.Z + 0.15f;
                if (e.Y < 0f) { goal.Yaw = 192f; boot = true; } else hood = true;   // engine in the back
            }
            else if (name == "wheel")
            {
                Vector3 w = BoneOffset("wheel_lf") ?? new Vector3(-0.8f, length * 0.3f, -0.2f);
                goal.X = w.X; goal.Y = w.Y; goal.Z = w.Z;
                goal.Dist = length * view.Dist + 1f;
            }
            else if (name == "plate")
            {
                goal.Y = min.Y; goal.Z = 0f;
            }
            else if (name == "cabin")
            {
                Vector3 seat = BoneOffset("seat_dside_f") ?? new Vector3(-0.4f, 0f, 0f);
                goal.X = 0f; goal.Y = seat.Y + 0.55f; goal.Z = seat.Z + 0.45f;
                goal.Dist = view.Metres;
            }
            if (hood) Function.Call(Hash.SET_VEHICLE_DOOR_OPEN, _veh, 4, false, false);
            else Function.Call(Hash.SET_VEHICLE_DOOR_SHUT, _veh, 4, false);
            if (boot) Function.Call(Hash.SET_VEHICLE_DOOR_OPEN, _veh, 5, false, false);
            else Function.Call(Hash.SET_VEHICLE_DOOR_SHUT, _veh, 5, false);
            _min = name == "cabin" ? 0.5f : Math.Max(1.2f, goal.Dist * 0.55f);
            _max = name == "cabin" ? 1.4f : goal.Dist * 1.9f;
            if (_hasGoal)
            {
                // turn the short way round
                while (goal.Yaw - _now.Yaw > 180f) goal.Yaw -= 360f;
                while (goal.Yaw - _now.Yaw < -180f) goal.Yaw += 360f;
            }
            else
            {
                _now = goal;
                _now.Yaw += 25f; _now.Pitch += 6f; _now.Dist *= 1.25f;
                _hasGoal = true;
            }
            _goal = goal;
            Name = name;
        }

        /// <summary>Turns (degrees) and zooms (a factor on the distance) the view being looked at.</summary>
        public void Orbit(float yaw, float pitch, float zoom)
        {
            if (!_hasGoal) return;
            _goal.Yaw += yaw;
            _goal.Pitch = Num.Clamp(_goal.Pitch + pitch, -4f, 70f);
            if (zoom != 0f) _goal.Dist = Num.Clamp(_goal.Dist * zoom, _min, _max);
        }

        public void Update(float dt)
        {
            if (_cam == null || !_veh.Exists()) return;
            float k = dt <= 0f ? 1f : Math.Min(1f, dt * 7f);
            _now.Yaw += (_goal.Yaw - _now.Yaw) * k;
            _now.Pitch += (_goal.Pitch - _now.Pitch) * k;
            _now.Dist += (_goal.Dist - _now.Dist) * k;
            _now.X += (_goal.X - _now.X) * k;
            _now.Y += (_goal.Y - _now.Y) * k;
            _now.Z += (_goal.Z - _now.Z) * k;

            Vector3 target = _veh.GetOffsetPosition(new Vector3(_now.X, _now.Y, _now.Z));
            double heading = (_veh.Heading + _now.Yaw) * Math.PI / 180.0;
            double pitch = _now.Pitch * Math.PI / 180.0;
            float flat = _now.Dist * (float)Math.Cos(pitch);
            var pos = new Vector3(target.X - (float)Math.Sin(heading) * flat, target.Y + (float)Math.Cos(heading) * flat, target.Z + _now.Dist * (float)Math.Sin(pitch));
            // keep the camera on this side of the shop walls
            RaycastResult hit = World.Raycast(target, pos, IntersectFlags.Map, _veh);
            if (hit.DidHit && hit.HitPosition.DistanceTo(target) > 0.8f)
                pos = hit.HitPosition + (target - hit.HitPosition).Normalized * 0.25f;
            _cam.Position = pos;
            _cam.PointAt(target);
            Function.Call(Hash.SET_CAM_FAR_DOF, _cam, pos.DistanceTo(target) + 3.5f);
            Function.Call(Hash.SET_USE_HI_DOF);
        }
    }
}
