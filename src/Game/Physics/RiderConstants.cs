using System;
using System.Collections.Generic;
using OpenTK;
namespace linerider.Game
{
    public class RiderConstants
    {
        public static readonly Vector2d[] DefaultRider = new[] {
                new Vector2d(0, 0),
                new Vector2d(0, 5),
                new Vector2d(15, 5),
                new Vector2d(17.5, 0),
                new Vector2d(5, 0),
                new Vector2d(5, -5.5),
                new Vector2d(11.5, -5),
                new Vector2d(11.5, -5),
                new Vector2d(10, 5),
                new Vector2d(10, 5),

                new Vector2d(0, 200),
                new Vector2d(0, 205),
                new Vector2d(15, 205),
                new Vector2d(17.5, 200),
                new Vector2d(5, 200),
                new Vector2d(5, 194.5),
                new Vector2d(11.5, 195),
                new Vector2d(11.5, 195),
                new Vector2d(10, 205),
                new Vector2d(10, 205),

                new Vector2d(0, -200),
                new Vector2d(0, -195),
                new Vector2d(15, -195),
                new Vector2d(17.5, -200),
                new Vector2d(5, -200),
                new Vector2d(5, -205.5),
                new Vector2d(11.5, -205),
                new Vector2d(11.5, -205),
                new Vector2d(10, -195),
                new Vector2d(10, -195),
            };

        public static readonly Vector2d[] DefaultScarf = new[] {
                new Vector2d(-2,-0.5),
                new Vector2d(-3.5,-0.5),
                new Vector2d(-5.5,-0.5),
                new Vector2d(-7,-0.5),
                new Vector2d(-9,-0.5),
                new Vector2d(-11.5,-0.5),
        };


        public const double EnduranceFactor = 0.0285;
        public const double StartingMomentum = 0.4;
        public static Vector2d Gravity = new Vector2d(0, 0.175);
        public const int SledTL = 0;
        public const int SledBL = 1;
        public const int SledBR = 2;
        public const int SledTR = 3;
        public const int BodyButt = 4;
        public const int BodyShoulder = 5;
        public const int BodyHandLeft = 6;
        public const int BodyHandRight = 7;
        public const int BodyFootLeft = 8;
        public const int BodyFootRight = 9;

        public const int SledTL2 = 10;
        public const int SledBL2 = 11;
        public const int SledBR2 = 12;
        public const int SledTR2 = 13;
        public const int BodyButt2 = 14;
        public const int BodyShoulder2 = 15;
        public const int BodyHandLeft2 = 16;
        public const int BodyHandRight2 = 17;
        public const int BodyFootLeft2 = 18;
        public const int BodyFootRight2 = 19;

        public const int SledTL3 = 20;
        public const int SledBL3 = 21;
        public const int SledBR3 = 22;
        public const int SledTR3 = 23;
        public const int BodyButt3 = 24;
        public const int BodyShoulder3 = 25;
        public const int BodyHandLeft3 = 26;
        public const int BodyHandRight3 = 27;
        public const int BodyFootLeft3 = 28;
        public const int BodyFootRight3 = 29;

        public static readonly Bone[] Bones;
        public static readonly Bone[] ScarfBones;

        public static readonly Bone[] Bones2;
        public static readonly Bone[] Bones3;

        static RiderConstants()
        {
            var bonelist = new List<Bone>();
            bonelist.Add(CreateBone(SledTL, SledBL));
            bonelist.Add(CreateBone(SledBL, SledBR));
            bonelist.Add(CreateBone(SledBR, SledTR));
            bonelist.Add(CreateBone(SledTR, SledTL));
            bonelist.Add(CreateBone(SledTL, SledBR));
            bonelist.Add(CreateBone(SledTR, SledBL));

            bonelist.Add(CreateBone(SledTL, BodyButt, breakable: true));
            bonelist.Add(CreateBone(SledBL, BodyButt, breakable: true));
            bonelist.Add(CreateBone(SledBR, BodyButt, breakable: true));

            bonelist.Add(CreateBone(BodyShoulder, BodyButt));
            bonelist.Add(CreateBone(BodyShoulder, BodyHandLeft));
            bonelist.Add(CreateBone(BodyShoulder, BodyHandRight));
            bonelist.Add(CreateBone(BodyButt, BodyFootLeft));
            bonelist.Add(CreateBone(BodyButt, BodyFootRight));
            bonelist.Add(CreateBone(BodyShoulder, BodyHandRight));

            bonelist.Add(CreateBone(BodyShoulder, SledTL, breakable: true));
            bonelist.Add(CreateBone(SledTR, BodyHandLeft, breakable: true));
            bonelist.Add(CreateBone(SledTR, BodyHandRight, breakable: true));
            bonelist.Add(CreateBone(BodyFootLeft, SledBR, breakable: true));
            bonelist.Add(CreateBone(BodyFootRight, SledBR, breakable: true));

            bonelist.Add(CreateBone(BodyShoulder, BodyFootLeft, repel: true));
            bonelist.Add(CreateBone(BodyShoulder, BodyFootRight, repel: true));

            bonelist.Add(CreateBone(SledTL2, SledBL2));
            bonelist.Add(CreateBone(SledBL2, SledBR2));
            bonelist.Add(CreateBone(SledBR2, SledTR2));
            bonelist.Add(CreateBone(SledTR2, SledTL2));
            bonelist.Add(CreateBone(SledTL2, SledBR2));
            bonelist.Add(CreateBone(SledTR2, SledBL2));

            bonelist.Add(CreateBone(SledTL2, BodyButt2, breakable: true));
            bonelist.Add(CreateBone(SledBL2, BodyButt2, breakable: true));
            bonelist.Add(CreateBone(SledBR2, BodyButt2, breakable: true));

            bonelist.Add(CreateBone(BodyShoulder2, BodyButt2));
            bonelist.Add(CreateBone(BodyShoulder2, BodyHandLeft2));
            bonelist.Add(CreateBone(BodyShoulder2, BodyHandRight2));
            bonelist.Add(CreateBone(BodyButt2, BodyFootLeft2));
            bonelist.Add(CreateBone(BodyButt2, BodyFootRight2));
            bonelist.Add(CreateBone(BodyShoulder2, BodyHandRight2));

            bonelist.Add(CreateBone(BodyShoulder2, SledTL2, breakable: true));
            bonelist.Add(CreateBone(SledTR2, BodyHandLeft2, breakable: true));
            bonelist.Add(CreateBone(SledTR2, BodyHandRight2, breakable: true));
            bonelist.Add(CreateBone(BodyFootLeft2, SledBR2, breakable: true));
            bonelist.Add(CreateBone(BodyFootRight2, SledBR2, breakable: true));

            bonelist.Add(CreateBone(BodyShoulder2, BodyFootLeft2, repel: true));
            bonelist.Add(CreateBone(BodyShoulder2, BodyFootRight2, repel: true));

            bonelist.Add(CreateBone(SledTL3, SledBL3));
            bonelist.Add(CreateBone(SledBL3, SledBR3));
            bonelist.Add(CreateBone(SledBR3, SledTR3));
            bonelist.Add(CreateBone(SledTR3, SledTL3));
            bonelist.Add(CreateBone(SledTL3, SledBR3));
            bonelist.Add(CreateBone(SledTR3, SledBL3));

            bonelist.Add(CreateBone(SledTL3, BodyButt3, breakable: true));
            bonelist.Add(CreateBone(SledBL3, BodyButt3, breakable: true));
            bonelist.Add(CreateBone(SledBR3, BodyButt3, breakable: true));

            bonelist.Add(CreateBone(BodyShoulder3, BodyButt3));
            bonelist.Add(CreateBone(BodyShoulder3, BodyHandLeft3));
            bonelist.Add(CreateBone(BodyShoulder3, BodyHandRight3));
            bonelist.Add(CreateBone(BodyButt3, BodyFootLeft3));
            bonelist.Add(CreateBone(BodyButt3, BodyFootRight3));
            bonelist.Add(CreateBone(BodyShoulder3, BodyHandRight3));

            bonelist.Add(CreateBone(BodyShoulder3, SledTL3, breakable: true));
            bonelist.Add(CreateBone(SledTR3, BodyHandLeft3, breakable: true));
            bonelist.Add(CreateBone(SledTR3, BodyHandRight3, breakable: true));
            bonelist.Add(CreateBone(BodyFootLeft3, SledBR3, breakable: true));
            bonelist.Add(CreateBone(BodyFootRight3, SledBR3, breakable: true));

            bonelist.Add(CreateBone(BodyShoulder3, BodyFootLeft3, repel: true));
            bonelist.Add(CreateBone(BodyShoulder3, BodyFootRight3, repel: true));

            Bones = bonelist.ToArray();
            bonelist = new List<Bone>();
            AddScarfBone(bonelist, 1);
            AddScarfBone(bonelist, 2);
            AddScarfBone(bonelist, 3);
            AddScarfBone(bonelist, 4);
            AddScarfBone(bonelist, 5);
            AddScarfBone(bonelist, 6);
            ScarfBones = bonelist.ToArray();
        }
        private static void AddScarfBone(List<Bone> bones, int index)
        {
            var even = index % 2 == 0;
            bones.Add(new Bone(index - 1, index, even ? 1.5 : 2.0, false, false));
        }

        private static Bone CreateBone(int a, int b, bool breakable = false, bool repel = false)
        {
            var rest = (DefaultRider[a] - DefaultRider[b]).Length;
            if (repel)
            {
                rest *= 0.5;
            }
            var ret = new Bone(a, b, rest, breakable, repel);
            return ret;
        }
    }
}