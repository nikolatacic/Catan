using System;
using UnityEngine;

namespace GameCore.Board
{
    [Serializable]
    public struct HexCoord : IEquatable<HexCoord>
    {
        public int Q, R;

        public HexCoord(int q, int r) { Q = q; R = r; }

        public static readonly HexCoord[] Directions = new[]
        {
            new HexCoord(1, 0), new HexCoord(1, -1), new HexCoord(0, -1),
            new HexCoord(-1, 0), new HexCoord(-1, 1), new HexCoord(0, 1)
        };

        public HexCoord[] Neighbors()
        {
            var result = new HexCoord[6];
            for (int i = 0; i < 6; i++)
                result[i] = new HexCoord(Q + Directions[i].Q, R + Directions[i].R);
            return result;
        }

        public int Distance(HexCoord other)
        {
            int dq = Q - other.Q, dr = R - other.R;
            return (Math.Abs(dq) + Math.Abs(dr) + Math.Abs(dq + dr)) / 2;
        }

        public Vector3 ToWorldPosition(float size)
        {
            float x = size * (3f / 2f * Q);
            float z = size * (MathF.Sqrt(3) * (R + Q / 2f));
            return new Vector3(x, 0, z);
        }

        public static HexCoord FromWorldPosition(Vector3 pos, float size)
        {
            float q = (2f / 3f * pos.x) / size;
            float r = (-1f / 3f * pos.x + MathF.Sqrt(3) / 3f * pos.z) / size;
            return Round(q, r);
        }

        private static HexCoord Round(float q, float r)
        {
            float s = -q - r;
            int rq = (int)MathF.Round(q), rr = (int)MathF.Round(r), rs = (int)MathF.Round(s);
            float dq = MathF.Abs(rq - q), dr = MathF.Abs(rr - r), ds = MathF.Abs(rs - s);
            if (dq > dr && dq > ds) rq = -rr - rs;
            else if (dr > ds) rr = -rq - rs;
            return new HexCoord(rq, rr);
        }

        public bool Equals(HexCoord other) => Q == other.Q && R == other.R;
        public override bool Equals(object obj) => obj is HexCoord h && Equals(h);
        public override int GetHashCode() => HashCode.Combine(Q, R);
        public static bool operator ==(HexCoord a, HexCoord b) => a.Equals(b);
        public static bool operator !=(HexCoord a, HexCoord b) => !a.Equals(b);
        public override string ToString() => $"({Q},{R})";
    }
}
