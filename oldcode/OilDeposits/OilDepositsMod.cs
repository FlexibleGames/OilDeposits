using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace OilDeposits
{
    public class OilDepositsMod : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterBlockClass("BlockOilDeposit", typeof(BlockOilMaster));            
        }
    }
}
