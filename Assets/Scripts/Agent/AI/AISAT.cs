using System;
using System.Linq;
using System.Collections.Generic;
using GameManagement;
using Microsoft.Z3;

namespace Agent.AI
{
    public class AISAT : AIBasic
    {
        // Core components
        private GameManager _gameManager;
        private KnowledgeBase _kb;
        private Context ctx;

        private void Start()
        {
            _gameManager = GameManager.Instance;  
            _kb = new KnowledgeBase();
            ctx = _kb.Ctx;
        }

        public override void FirstTurn()
        {
            _agentMove.MoveCell();    // Move to initial position
            _agentSense.SenseCell();  // Gather initial environment data

            // initialize the Knowledge Base with the world rules (pits and breezes)
            int mapSizeX = _gameManager.gridMax.x;
            int mapSizeY = _gameManager.gridMax.y;
            for (int i = 0; i < mapSizeX; i++)
                for (int j = 0; j < mapSizeY; j++)
                {
                    // B(i,j) => (P(i-1,j) v P(i+1,j) v P(i,j-1) v P(i,j+1))
                    var lits = new List<BoolExpr>(); // literals
                    lits.Add(ctx.MkNot(B(i,j)));
                    if (i > 0)          lits.Add(P(i-1,j));
                    if (i < mapSizeX-1) lits.Add(P(i+1,j));
                    if (j > 0)          lits.Add(P(i,j-1));
                    if (j < mapSizeY-1) lits.Add(P(i,j+1));
                    BoolExpr alpha = ctx.MkOr(lits.ToArray());
                    _kb.Tell(alpha);

                    // (P(i-1,j) v P(i+1,j) v P(i,j-1) v P(i,j+1)) => B(i,j)
                    if (i > 0) {
                        BoolExpr b = B(i,j);
                        BoolExpr notP = ctx.MkNot(P(i-1,j));
                        _kb.Tell(ctx.MkOr(b, notP));
                    }
                    if (i < mapSizeX-1) {
                        BoolExpr b = B(i,j);
                        BoolExpr notP = ctx.MkNot(P(i+1,j));
                        _kb.Tell(ctx.MkOr(b, notP));
                    }
                    if (j > 0) {
                        BoolExpr b = B(i,j);
                        BoolExpr notP = ctx.MkNot(P(i,j-1));
                        _kb.Tell(ctx.MkOr(b, notP));
                    }
                    if (j < mapSizeY-1) {
                        BoolExpr b = B(i,j);
                        BoolExpr notP = ctx.MkNot(P(i,j+1));
                        _kb.Tell(ctx.MkOr(b, notP));
                    }
                }
        }

        public override void PlayTurn()
        {
            // get current cell 
            var x = _agent.coords.x;
            var y = _agent.coords.y;
            var cell = _gameManager.AgentsMap[x, y];

            // tell the KB current position is safe (would be dead otherwise)
            _kb.Tell(ctx.MkNot(P(x,y)));
            
            // tell the KB if theres a breeze or not
            if (cell.Exists(e => e.tag == "Breeze"))
                _kb.Tell(B(x,y));
            else
                _kb.Tell(ctx.MkNot(B(x,y)));

            // ask kb "where to go"
            
            
            /* 
            // Query the knowledge base to determine the next action
            switch (_prologInterface.QueryKb(_agent.name, _agent.coords))
            {
                case "attack" or "shoot" or "shootarrow":
                    _agentAction.TryShootingArrow();  // Attack nearby monster
                    break;
                case "pickup":
                    _agentAction.PickUpGold();        // Collect valuable item
                    break;
                case "discard":
                    _agentAction.Discard();           // Remove item from inventory
                    break;
                case "bumpwall":
                    _agentMove.BumpWall();           // Interact with obstacle
                    break;
                case "moveback":
                    _agentMove.MoveAgent(_agentMove.MoveBack());  // Return to previous position
                    break;
                case "move":
                    _agentMove.MoveCell();           // Move to new position
                    break;
            }
            */

            // Update knowledge of environment after action
            _agentSense.SenseCell();
        }

        private BoolExpr B(int x, int y) => ctx.MkBoolConst($"B_{x}_{y}");

        private BoolExpr P(int x, int y) => ctx.MkBoolConst($"P_{x}_{y}");
    }
}