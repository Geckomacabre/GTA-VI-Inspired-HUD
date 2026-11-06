using System;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// One tied poop bag (vi_prop_poopbag from the vihudanims pack) and where it is: in someone's right hand, or
    /// loose. The model has no collision of its own, so a loose bag rides on an invisible baseball (CarrierModel,
    /// GTA's own w_am_baseball): the game's physics throws it, bounces it, rolls it and lets people kick it, as it
    /// does the baseball weapon. Loose and moving is Falling (dropped) or Flying (thrown by the player, until it
    /// hits someone); loose and still for a moment is Lying, when it can be picked up.
    /// A thrown bag's hit on a person is found each frame by a ray from where the ball was to where it is (peds
    /// only), or by passing close to the one it was thrown at.
    /// </summary>
    internal sealed class PoopBag
    {
        public enum State { Held, Falling, Lying, Flying, Gone }

        private const int RightHand = 57005;            // SKEL_R_Hand, as the slim jim
        private const float CentreFromKnot = 0.065f;    // the model's origin is the knot; its middle is this far down
        private const int PedRay = 4 | 8;               // ped capsules and ragdolls
        private const float RestSpeed = 0.25f;
        private const int RestMs = 500, LooseMaxMs = 9000;

        public Prop Prop { get; private set; }
        /// <summary>The invisible ball a loose bag rides on, or null while it's in a hand.</summary>
        public Prop Carrier { get; private set; }
        public State Now { get; private set; }
        public Ped Holder { get; private set; }
        /// <summary>Who threw it (the player), until it hits someone or stops.</summary>
        public Ped Thrower { get; private set; }
        /// <summary>The ped it was thrown at, if any (a near miss on them counts).</summary>
        public Ped Target { get; private set; }
        public int LyingSince { get; private set; }

        /// <summary>A lump of poop (vi_prop_poop, out of a burst bag) rather than the bag; its origin is its middle.</summary>
        public bool IsLump { get; private set; }
        private float CentreOff => IsLump ? 0f : CentreFromKnot;

        private Vector3 _last;
        private int _looseAt, _stillSince;

        private PoopBag() { }

        /// <summary>A new bag in `ped`'s right hand, or null when the model can't be had.</summary>
        public static PoopBag InHand(Ped ped)
        {
            Model m = DogCfg.BagModel;
            if (!m.IsInCdImage || !m.Request(1000)) return null;
            Prop p = World.CreateProp(m, ped.Position + new Vector3(0f, 0f, 1f), false, false);
            if (p == null || !p.Exists()) return null;
            Function.Call(Hash.SET_ENTITY_COLLISION, p, false, false);
            var bag = new PoopBag { Prop = p };
            bag.Attach(ped);
            return bag;
        }

        /// <summary>A lump of poop flying out from `at` (a bag bursting), on its own ball; null without a model.</summary>
        public static PoopBag Lump(Vector3 at, Vector3 vel, Ped ignore)
        {
            Model m = DogCfg.LumpModel.IsInCdImage ? DogCfg.LumpModel : DogCfg.LumpFallback;
            if (!m.IsInCdImage || !m.Request(500)) return null;
            Prop p = World.CreateProp(m, at, false, false);
            if (p == null || !p.Exists()) return null;
            Function.Call(Hash.SET_ENTITY_COLLISION, p, false, false);
            p.Rotation = new Vector3((float)(DogCfg.Rng.NextDouble() * 360), (float)(DogCfg.Rng.NextDouble() * 360), (float)(DogCfg.Rng.NextDouble() * 360));
            var lump = new PoopBag { Prop = p, IsLump = true };
            lump.Loose(ignore, vel, State.Falling);
            return lump;
        }

        public bool Exists => Prop != null && Prop.Exists() && Now != State.Gone;

        /// <summary>The middle of the bag.</summary>
        public Vector3 Centre
        {
            get
            {
                if (Carrier != null && Carrier.Exists()) return Carrier.Position;
                if (!Exists) return Vector3.Zero;
                return Prop.GetOffsetPosition(new Vector3(0f, 0f, -CentreOff));
            }
        }

        /// <summary>Into `ped`'s right hand (off its ball, if it was loose).</summary>
        public void Attach(Ped ped)
        {
            if (!Exists) return;
            if (Function.Call<bool>(Hash.IS_ENTITY_ATTACHED, Prop)) Function.Call(Hash.DETACH_ENTITY, Prop, false, false);
            DeleteCarrier();
            int bone = Function.Call<int>(Hash.GET_PED_BONE_INDEX, ped, RightHand);
            Vector3 o = IsLump ? DogCfg.LumpHoldOffset : DogCfg.HoldOffset, r = IsLump ? DogCfg.LumpHoldRotation : DogCfg.HoldRotation;
            _bone = bone;
            _grip = o;
            _hangDir = new Vector3(0f, 0f, -1f);
            Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY, Prop, ped, bone, o.X, o.Y, o.Z, r.X, r.Y, r.Z,
                false, false, false, false, 2, true);
            Holder = ped;
            Thrower = null;
            Target = null;
            Now = State.Held;
        }

        // ---- hanging from the hand -------------------------------------------------------------------------------
        // Bolted to SKEL_R_Hand at the knot, and turned every frame so it hangs straight down, trailing a little as the
        // hand moves. (A bag joined by physics to the hand fell out of it in game; a fixed turn only looked right at
        // one angle of the hand.) The turn is asked of ATTACH_ENTITY_TO_ENTITY as two angles, whose order GTA doesn't
        // document: the bag's own down is read back each frame, and after a few wrong frames the other order is used.
        private static int _hangOrder;
        private int _bone, _bad;
        private Vector3 _grip, _hangDir = new Vector3(0f, 0f, -1f), _asked;

        public void KeepHanging(float dt)
        {
            if (!Exists || Now != State.Held || IsLump || !DogCfg.HoldSwing || Holder == null || !Holder.Exists()) return;
            if (dt > 0.1f) dt = 0.1f;
            // how the last frame's turn came out
            Vector3 p0 = Prop.Position;
            Vector3 got = Prop.GetOffsetPosition(new Vector3(0f, 0f, -1f)) - p0;
            if (_asked.Length() > 0.5f && got.Length() > 0.5f)
            {
                if (Vector3.Dot(got.Normalized, _asked) < 0.8f) { if (++_bad > 8) { _hangOrder ^= 1; _bad = 0; DogCfg.Trace("hanging: the other angle order"); } }
                else _bad = 0;
            }
            // where it should hang: down, trailing the hand's movement a little
            Vector3 target = new Vector3(0f, 0f, -1f) - Holder.Velocity * 0.05f;
            target.Normalize();
            float k = Math.Min(1f, dt * 7f);
            _hangDir = _hangDir + (target - _hangDir) * k;
            _hangDir.Normalize();
            // that direction in the hand bone's own frame
            Vector3 b0 = Function.Call<Vector3>(Hash.GET_PED_BONE_COORDS, Holder, RightHand, 0f, 0f, 0f);
            Vector3 ax = Function.Call<Vector3>(Hash.GET_PED_BONE_COORDS, Holder, RightHand, 1f, 0f, 0f) - b0;
            Vector3 ay = Function.Call<Vector3>(Hash.GET_PED_BONE_COORDS, Holder, RightHand, 0f, 1f, 0f) - b0;
            Vector3 az = Function.Call<Vector3>(Hash.GET_PED_BONE_COORDS, Holder, RightHand, 0f, 0f, 1f) - b0;
            float dx = Vector3.Dot(_hangDir, ax), dy = Vector3.Dot(_hangDir, ay), dz = Vector3.Dot(_hangDir, az);
            float rx, ry;
            if (_hangOrder == 0)
            {
                // the bag's -Z turned by X then Y: (-cos rx sin ry, sin rx, -cos rx cos ry)
                rx = (float)Math.Asin(Num.Clamp(dy, -1f, 1f));
                ry = (float)Math.Atan2(-dx, -dz);
            }
            else
            {
                // by Y then X: (-sin ry, cos ry sin rx, -cos ry cos rx)
                ry = (float)Math.Asin(Num.Clamp(-dx, -1f, 1f));
                rx = (float)Math.Atan2(dy, -dz);
            }
            const float Deg = 180f / (float)Math.PI;
            Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY, Prop, Holder, _bone, _grip.X, _grip.Y, _grip.Z, rx * Deg, ry * Deg, 0f,
                false, false, false, false, 2, true);
            _asked = _hangDir;
        }

        /// <summary>Out of the holder's hand: it pops out and falls (a hit, a fall, a car door).</summary>
        public void Drop(Vector3 push)
        {
            if (!Exists) return;
            Vector3 handVel = Holder != null && Holder.Exists() ? Holder.Velocity : Vector3.Zero;
            Ped from = Holder;
            Loose(from, handVel * 0.6f + push, State.Falling);
        }

        /// <summary>Thrown from the hand at `velocity`; `target` may be null.</summary>
        public void Throw(Ped thrower, Ped target, Vector3 velocity)
        {
            if (!Exists) return;
            Loose(thrower, velocity, State.Flying);
            Thrower = thrower;
            Target = target;
        }

        // Off the hand and onto a ball where it is, turned as it is, moving at `vel`.
        private void Loose(Ped from, Vector3 vel, State s)
        {
            if (Carrier != null && Carrier.Exists() && Function.Call<bool>(Hash.IS_ENTITY_ATTACHED, Carrier))
            {
                Function.Call(Hash.DETACH_ENTITY, Carrier, true, true);
                if (from != null && from.Exists()) Function.Call(Hash.SET_ENTITY_NO_COLLISION_ENTITY, Carrier, from, false);
                Function.Call(Hash.ACTIVATE_PHYSICS, Carrier);
                Function.Call(Hash.SET_ENTITY_VELOCITY, Carrier, vel.X, vel.Y, vel.Z);
                Holder = null;
                Thrower = null;
                Target = null;
                _last = Carrier.Position;
                _looseAt = Game.GameTime;
                _stillSince = 0;
                Now = s;
                return;
            }
            Vector3 centre = Prop.GetOffsetPosition(new Vector3(0f, 0f, -CentreOff));
            Vector3 rot = Prop.Rotation;
            if (Function.Call<bool>(Hash.IS_ENTITY_ATTACHED, Prop)) Function.Call(Hash.DETACH_ENTITY, Prop, false, false);
            Holder = null;
            Thrower = null;
            Target = null;

            Model m = DogCfg.CarrierModel;
            Prop ball = m.IsInCdImage && m.Request(500) ? World.CreateProp(m, centre, rot, true, false) : null;
            if (ball == null || !ball.Exists())
            {
                // no ball: it just goes down where it is, lying on the ground
                Log.Write("DogWalkers", "no " + DogCfg.CarrierModelName + " for the bag to ride on");
                LieHere(centre);
                return;
            }
            Carrier = ball;
            // hidden by alpha, not SET_ENTITY_VISIBLE: an invisible entity hides what's attached to it (the bag)
            Function.Call(Hash.SET_ENTITY_ALPHA, ball, 0, false);
            Function.Call(Hash.SET_ENTITY_COLLISION, ball, true, true);
            if (from != null && from.Exists()) Function.Call(Hash.SET_ENTITY_NO_COLLISION_ENTITY, ball, from, false);
            // the bag's middle on the ball's middle, turned with it
            Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY, Prop, ball, 0, 0f, 0f, CentreOff, 0f, 0f, 0f,
                false, false, false, false, 2, true);
            Function.Call(Hash.ACTIVATE_PHYSICS, ball);
            Function.Call(Hash.SET_ENTITY_VELOCITY, ball, vel.X, vel.Y, vel.Z);
            _last = centre;
            _looseAt = Game.GameTime;
            _stillSince = 0;
            Now = s;
        }

        // ---- thrown like a snowball ---------------------------------------------------------------------------------
        // mpchristmas2's weaponsnowball.meta, AMMO_SNOWBALL: launched at 25 m/s, Damping 0.05, GravityFactor 1, and it
        // goes off on the first thing it touches (LifeTimeAfterImpact 0.001), 10 s at most (LifeTime).
        private const float SnowDamping = 0.05f;
        private const int SnowLifeMs = 10000;
        private const int AllRay = 1 | 2 | 4 | 8 | 16;   // map, vehicles, ped capsules, ragdolls, objects
        private Vector3 _pos, _vel, _spin, _rot;

        /// <summary>
        /// Thrown as the game threw the snowball it stands in for: from `from` at `velocity` (the snowball's own, read
        /// off it the moment it left the hand). Moved by script, with the snowball's physics; it bursts on whatever it
        /// hits first (Step returns that hit) and is gone.
        /// </summary>
        public void ThrowLikeSnowball(Ped thrower, Vector3 from, Vector3 velocity)
        {
            if (!Exists) return;
            if (Function.Call<bool>(Hash.IS_ENTITY_ATTACHED, Prop)) Function.Call(Hash.DETACH_ENTITY, Prop, false, false);
            DeleteCarrier();          // the hand's ball, if it dangled
            Holder = null;
            Thrower = thrower;
            Target = null;
            _pos = from;
            _vel = velocity;
            _rot = Prop.Rotation;
            var rng = DogCfg.Rng;
            _spin = new Vector3((float)(rng.NextDouble() * 2 - 1) * 300f + 420f, (float)(rng.NextDouble() * 2 - 1) * 200f, (float)(rng.NextDouble() * 2 - 1) * 120f);
            _looseAt = Game.GameTime;
            Now = State.Flying;
            Place();
        }

        /// <summary>
        /// Rides the game's own projectile (WEAPON_POOBAG's, made invisible): the game flies it, hits with it and
        /// damages with it, as the snowball; DogWalkers bursts the bag when the projectile goes. Not attached to it:
        /// anything attached to an invisible entity is hidden with it (the user saw the bag vanish once thrown), so
        /// Follow puts the bag on it every frame instead.
        /// </summary>
        public void Ride(Prop projectile)
        {
            if (!Exists || projectile == null || !projectile.Exists()) return;
            if (Function.Call<bool>(Hash.IS_ENTITY_ATTACHED, Prop)) Function.Call(Hash.DETACH_ENTITY, Prop, false, false);
            DeleteCarrier();
            Holder = null;
            _rot = Prop.Rotation;
            var rng = DogCfg.Rng;
            _spin = new Vector3((float)(rng.NextDouble() * 2 - 1) * 300f + 420f, (float)(rng.NextDouble() * 2 - 1) * 200f, (float)(rng.NextDouble() * 2 - 1) * 120f);
            _pos = projectile.Position;
            Now = State.Flying;
            Place();
        }

        /// <summary>A frame of the ride: the bag's middle where the projectile will be this frame, tumbling.</summary>
        public void Follow(Vector3 at, Vector3 vel, float dt)
        {
            if (!Exists) return;
            if (dt > 0.05f) dt = 0.05f;
            _pos = at + vel * dt;                     // the projectile's place is a frame old when it's read
            _rot += _spin * dt;
            Place();
        }

        private void Place()
        {
            // the bag's middle on the flight path
            Vector3 knot = _pos + (Prop.GetOffsetPosition(Vector3.Zero) - Prop.GetOffsetPosition(new Vector3(0f, 0f, -CentreOff)));
            Function.Call(Hash.SET_ENTITY_COORDS_NO_OFFSET, Prop, knot.X, knot.Y, knot.Z, false, false, false);
            Function.Call(Hash.SET_ENTITY_ROTATION, Prop, _rot.X, _rot.Y, _rot.Z, 2, true);
        }

        private Hit StepThrown(float dt)
        {
            var hit = new Hit();
            if (dt <= 0f) return hit;
            if (dt > 0.05f) dt = 0.05f;
            Vector3 from = _pos;
            _vel.Z -= 9.81f * dt;
            _vel *= (float)Math.Exp(-SnowDamping * dt);
            Vector3 to = from + _vel * dt;
            _rot += _spin * dt;
            // The first moments leave objects out: the snowball it replaced (an object) is still at the start point for a
            // frame, and the first ray hit it, bursting the bag in the thrower's hand (the user's "no explosion or fx").
            int flags = Game.GameTime - _looseAt < 150 ? (1 | 2 | 4 | 8) : AllRay;
            if (RayAll(from, to, Thrower, flags, out Vector3 point, out Vector3 normal, out Entity what))
            {
                hit.Happened = true;
                hit.At = point;
                hit.Normal = normal;
                hit.Ped = what as Ped;
                hit.Burst = true;
                Delete();                         // it went off, as a snowball does
                return hit;
            }
            _pos = to;
            Place();
            if (Game.GameTime - _looseAt > SnowLifeMs) Delete();
            return hit;
        }

        internal static bool RayAll(Vector3 from, Vector3 to, Entity ignore, int flags, out Vector3 point, out Vector3 normal, out Entity what)
        {
            point = to;
            normal = new Vector3(0f, 0f, 1f);
            what = null;
            int handle = Function.Call<int>(Hash.START_EXPENSIVE_SYNCHRONOUS_SHAPE_TEST_LOS_PROBE,
                from.X, from.Y, from.Z, to.X, to.Y, to.Z, flags, ignore == null ? 0 : ignore.Handle, 7);
            var didHit = new OutputArgument();
            var end = new OutputArgument();
            var nrm = new OutputArgument();
            var ent = new OutputArgument();
            Function.Call<int>(Hash.GET_SHAPE_TEST_RESULT, handle, didHit, end, nrm, ent);
            if (!didHit.GetResult<bool>()) return false;
            point = end.GetResult<Vector3>();
            normal = nrm.GetResult<Vector3>();
            int h = ent.GetResult<int>();
            if (h != 0 && Function.Call<bool>(Hash.DOES_ENTITY_EXIST, h))
                what = Function.Call<bool>(Hash.IS_ENTITY_A_PED, h) ? (Entity)(Entity.FromHandle(h) as Ped) : Entity.FromHandle(h);
            return true;
        }

        public struct Hit
        {
            public bool Happened;
            public Ped Ped;          // the person or animal a thrown bag hit
            public Vector3 At;
            public Vector3 Normal;   // the surface it hit (a snowball-style throw)
            public bool Burst;       // it went off and is gone (a snowball-style throw)
        }

        /// <summary>One frame of a loose bag. Returns the person a thrown bag hit this frame, if any.</summary>
        public Hit Step(float dt)
        {
            if (Exists && Now == State.Flying && Carrier == null) return StepThrown(dt);
            var hit = new Hit();
            if (!Exists || (Now != State.Falling && Now != State.Flying && Now != State.Lying)) return hit;
            if (Carrier == null || !Carrier.Exists())
            {
                if (Now != State.Lying) LieHere(Prop.Position);
                return hit;
            }
            int now = Game.GameTime;
            Vector3 at = Carrier.Position;

            if (Now == State.Flying)
            {
                bool did = Ray(_last, at, Thrower, out Vector3 point, out Ped who);
                if (!did && Target != null && Target.Exists())
                {
                    if (Function.Call<bool>(Hash.IS_ENTITY_TOUCHING_ENTITY, Carrier, Target)) { did = true; who = Target; point = at; }
                    else
                    {
                        foreach (int b in new[] { 31086, 24818, 11816 })   // head, upper chest, pelvis
                        {
                            Vector3 bone = Function.Call<Vector3>(Hash.GET_PED_BONE_COORDS, Target, b, 0f, 0f, 0f);
                            if (DistanceToSegment(bone, _last, at) < DogCfg.MagnetRadius) { did = true; who = Target; point = bone; break; }
                        }
                    }
                }
                if (did && who != null)
                {
                    hit.Happened = true;
                    hit.Ped = who;
                    hit.At = point;
                    // it drops off them: most of the throw gone, down and a little back the way it came
                    Vector3 v = Carrier.Velocity;
                    Function.Call(Hash.SET_ENTITY_VELOCITY, Carrier, -v.X * 0.15f, -v.Y * 0.15f, Math.Min(v.Z, 0f) * 0.3f);
                    Function.Call(Hash.SET_ENTITY_NO_COLLISION_ENTITY, Carrier, who, false);
                    Now = State.Falling;
                    Thrower = null;
                    Target = null;
                }
            }
            _last = at;

            // Still for a moment: lying there, to be picked up. Kicked or knocked: moving again.
            float speed = Carrier.Velocity.Length();
            if (speed < RestSpeed)
            {
                if (_stillSince == 0) _stillSince = now;
                if (Now != State.Lying && now - _stillSince >= RestMs) { Now = State.Lying; LyingSince = now; Thrower = null; Target = null; }
            }
            else
            {
                _stillSince = 0;
                if (Now == State.Lying) { Now = State.Falling; _looseAt = now; }
            }
            // Bouncing about too long (down a hill, on a moving car): it counts as lying wherever it is.
            if (Now != State.Lying && now - _looseAt > LooseMaxMs) { Now = State.Lying; LyingSince = now; Thrower = null; Target = null; }
            // Fallen through the world: put it back on the ground.
            float ground = GroundZ(at + new Vector3(0f, 0f, 1.5f), at.Z);
            if (at.Z < ground - 2f) Function.Call(Hash.SET_ENTITY_COORDS_NO_OFFSET, Carrier, at.X, at.Y, ground + 0.1f, false, false, false);
            return hit;
        }

        // No ball to be had: the bag set down on its side where it is (no physics).
        private void LieHere(Vector3 centre)
        {
            float ground = GroundZ(centre + new Vector3(0f, 0f, 0.5f), centre.Z);
            Function.Call(Hash.SET_ENTITY_COORDS_NO_OFFSET, Prop, centre.X, centre.Y, ground + (IsLump ? 0.015f : 0.045f), false, false, false);
            Function.Call(Hash.SET_ENTITY_ROTATION, Prop, 90f, 0f, (float)(DogCfg.Rng.NextDouble() * 360.0), 2, true);
            Now = State.Lying;
            LyingSince = Game.GameTime;
        }

        private void DeleteCarrier()
        {
            if (Carrier != null && Carrier.Exists()) Carrier.Delete();
            Carrier = null;
        }

        public void Delete()
        {
            if (Prop != null && Prop.Exists()) Prop.Delete();
            DeleteCarrier();
            Prop = null;
            Holder = null;
            Now = State.Gone;
        }

        // ---- helpers ------------------------------------------------------------------------------------------

        internal static float GroundZ(Vector3 above, float fallback)
        {
            var z = new OutputArgument();
            if (Function.Call<bool>(Hash.GET_GROUND_Z_FOR_3D_COORD, above.X, above.Y, above.Z, z, false, false))
                return z.GetResult<float>();
            return fallback;
        }

        // A ray for people only (the world, cars and objects are the ball's own physics).
        private static bool Ray(Vector3 from, Vector3 to, Entity ignore, out Vector3 point, out Ped who)
        {
            point = to;
            who = null;
            if ((to - from).Length() < 0.005f) return false;
            int handle = Function.Call<int>(Hash.START_EXPENSIVE_SYNCHRONOUS_SHAPE_TEST_LOS_PROBE,
                from.X, from.Y, from.Z, to.X, to.Y, to.Z, PedRay, ignore == null ? 0 : ignore.Handle, 7);
            var didHit = new OutputArgument();
            var end = new OutputArgument();
            var normal = new OutputArgument();
            var ent = new OutputArgument();
            Function.Call<int>(Hash.GET_SHAPE_TEST_RESULT, handle, didHit, end, normal, ent);
            if (!didHit.GetResult<bool>()) return false;
            int h = ent.GetResult<int>();
            if (h == 0 || !Function.Call<bool>(Hash.DOES_ENTITY_EXIST, h) || !Function.Call<bool>(Hash.IS_ENTITY_A_PED, h)) return false;
            point = end.GetResult<Vector3>();
            who = Entity.FromHandle(h) as Ped;
            return who != null;
        }

        private static float DistanceToSegment(Vector3 p, Vector3 a, Vector3 b)
        {
            Vector3 ab = b - a;
            float len2 = Vector3.Dot(ab, ab);
            float t = len2 > 1e-6f ? Num.Clamp01(Vector3.Dot(p - a, ab) / len2) : 0f;
            return (a + ab * t - p).Length();
        }
    }
}
