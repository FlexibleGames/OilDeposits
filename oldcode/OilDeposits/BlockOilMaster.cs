using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace OilDeposits
{
    public class BlockOilMaster : Block, IBlockFlowing
    {
        public string Flow { get; set; }
        public Vec3i FlowNormali { get; set; }

        public bool IsLava => false;

        public string Height { get; set; }

        public BlockOilMaster()
        {
            if (this.Attributes != null)
            {
                maxDepRadius = this.Attributes["maxDepositRadius"].AsInt();
                minDepRadius = this.Attributes["minDepositRadius"].AsInt();
                minDepDepth = this.Attributes["minDepositDepth"].AsInt();
                spoutChance = this.Attributes["surfaceSpoutChance"].AsInt();
            }
        }

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            string text = this.Variant["flow"];
            this.Flow = ((text != null) ? string.Intern(text) : null);
            Vec3i flowNormali;
            if (this.Flow == null)
            {
                flowNormali = null;
            }
            else
            {
                Cardinal cardinal = Cardinal.FromInitial(this.Flow);
                flowNormali = ((cardinal != null) ? cardinal.Normali : null);
            }
            this.FlowNormali = flowNormali;
            string text2 = this.Variant["height"];
            this.Height = ((text2 != null) ? string.Intern(text2) : null);
        }

        public override bool ShouldPlayAmbientSound(IWorldAccessor world, BlockPos pos)
        {
            return world.BlockAccessor.GetBlock(pos.X, pos.Y + 1, pos.Z).Id == 0 && world.BlockAccessor.GetBlock(pos.X, pos.Y - 1, pos.Z).SideSolid[BlockFacing.UP.Index];
        }

        public override bool IsLiquid()
        {
            return true;
        }
        

        public override bool TryPlaceBlockForWorldGen(IBlockAccessor blockAccessor, BlockPos pos, BlockFacing onBlockFace, LCGRandom worldgenRandom)
        {
            // we will assume depth is taken care of by the deposit generator
            int randRadius = worldgenRandom.NextInt(maxDepRadius + 1);
            if (randRadius < minDepRadius) randRadius = minDepRadius;
            bool spoutme = worldgenRandom.NextInt(101) <= spoutChance;
            ClimateCondition climateCondition = blockAccessor.GetClimateAt(pos, EnumGetClimateMode.WorldGenValues);
            if (climateCondition.WorldGenTemperature > 20 && climateCondition.WorldgenRainfall <= 0.2)
            {
                // desert conditions... oil!
                randRadius += 5;
            }
            List<BlockPos> bubble = BuildBubble(blockAccessor, pos, randRadius, spoutme);
            if (bubble.Count == 0) return false;
            foreach (BlockPos blockPos in bubble)
            {
                // bringing the oil!
                blockAccessor.SetBlock(this.BlockId, blockPos);
            }
            return true;
        }

        private List<BlockPos> BuildBubble(IBlockAccessor blockAccessor, BlockPos pos, int radius, bool spout)
        {
            List<BlockPos> bubble = new List<BlockPos>();
            int squaredradius = radius * radius;
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    for (int z = -radius; z <= radius; z++)
                    {
                        if (pos.DistanceSqTo(pos.X + x, pos.Y + y, pos.Z + z) <= squaredradius && (pos.Y + y) > 0)
                        {
                            bubble.Add(pos.AddCopy(x, y, z));
                        }
                    }
                }
            }
            if (spout)
            {
                for (int sy = pos.Y+radius-1; sy < blockAccessor.GetTerrainMapheightAt(pos)+radius; sy++)
                {
                    bubble.Add(pos.AddCopy(0, pos.Y + sy, 0));
                }
            }
            return bubble;
        }

        private int maxDepRadius;
        private int minDepRadius;
        private int minDepDepth;
        private int spoutChance;


    }
}
