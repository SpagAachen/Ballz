using Ballz.GameSession.World;
using Ballz.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.GameSession.Logic.Weapons
{
    public class Drill: WeaponControl
    {
        public Drill(Ball ball, Ballz game) : base(ball, game) { }

        public override string Icon { get; } = "Drill";

        public override string Name { get; } = "Drill";

        const float IntervalBetweenTerrainModification = 0.25f;
        const float MaxDrillDurationPerTurn = 10f;

        float DrillDuration = 0;

        float TerrainModificationTimer = float.PositiveInfinity;

        public override void Update(float elapsedSeconds, Dictionary<InputMessage.MessageType, bool> keysPressed, out bool turnEndindActionHappened, out bool canSwitchWeapon)
        {
            base.Update(elapsedSeconds, keysPressed, out turnEndindActionHappened, out canSwitchWeapon);

            if (keysPressed[InputMessage.MessageType.ControlsAction] && DrillDuration < MaxDrillDurationPerTurn)
            {
                DrillDuration += elapsedSeconds;
                if (TerrainModificationTimer > IntervalBetweenTerrainModification)
                {
                    var p = Ball.Position + Ball.AimDirection * 1.5f;
                    Game.Match.World.StaticGeometry.SubtractCircle(p.X, p.Y, 1.02f);
                    TerrainModificationTimer = 0;
                }
            }

            TerrainModificationTimer += elapsedSeconds;

            if (DrillDuration > MaxDrillDurationPerTurn)
            {
                turnEndindActionHappened = true;
            }
        }

        public override void OnTurnStart()
        {
            base.OnTurnStart();
            TerrainModificationTimer = float.PositiveInfinity;
            DrillDuration = 0;
        }
    }
}
