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
            int deltaQ = Q - other.Q, deltaR = R - other.R;
            return (Math.Abs(deltaQ) + Math.Abs(deltaR) + Math.Abs(deltaQ + deltaR)) / 2;
        }

        public Vector3 ToWorldPosition(float size)
        {
            float worldX = size * (3f / 2f * Q);
            float worldZ = size * (MathF.Sqrt(3) * (R + Q / 2f));
            return new Vector3(worldX, 0, worldZ);
        }

        public static HexCoord FromWorldPosition(Vector3 worldPosition, float size)
        {
            float fractionalQ = (2f / 3f * worldPosition.x) / size;
            float fractionalR = (-1f / 3f * worldPosition.x + MathF.Sqrt(3) / 3f * worldPosition.z) / size;
            return Round(fractionalQ, fractionalR);
        }

        private static HexCoord Round(float fractionalQ, float fractionalR)
        {
            float fractionalS = -fractionalQ - fractionalR;
            int roundedQ = (int)MathF.Round(fractionalQ);
            int roundedR = (int)MathF.Round(fractionalR);
            int roundedS = (int)MathF.Round(fractionalS);
            float diffQ = MathF.Abs(roundedQ - fractionalQ);
            float diffR = MathF.Abs(roundedR - fractionalR);
            float diffS = MathF.Abs(roundedS - fractionalS);
            if (diffQ > diffR && diffQ > diffS) roundedQ = -roundedR - roundedS;
            else if (diffR > diffS) roundedR = -roundedQ - roundedS;
            return new HexCoord(roundedQ, roundedR);
        }

        public bool Equals(HexCoord other) => Q == other.Q && R == other.R;
        public override bool Equals(object obj) => obj is HexCoord hexCoord && Equals(hexCoord);
        public override int GetHashCode() => HashCode.Combine(Q, R);
        public static bool operator ==(HexCoord a, HexCoord b) => a.Equals(b);
        public static bool operator !=(HexCoord a, HexCoord b) => !a.Equals(b);
        public override string ToString() => $"({Q},{R})";
    }
}
