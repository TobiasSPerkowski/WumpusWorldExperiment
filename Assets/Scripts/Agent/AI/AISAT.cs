using System;
using System.Linq;
using UnityEngine;
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
        private List<(int x, int y)> exploredCells;

        private void Start()
        {
            _gameManager = GameManager.Instance;  
            _kb = new KnowledgeBase();
            ctx = _kb.Ctx;
            exploredCells = new List<(int,int)>();
        }

        public override void FirstTurn()
        {
            // move to initial cell
            var start = _agent.startCoord;
            _agentMove.MoveAgent(start);
            _agentSense.SenseCell();
            exploredCells.Add((start.x, start.y));
            
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
            // update enviroment knowledge
            _agentSense.SenseCell();

            // get current cell 
            var x = _agent.coords.x;
            var y = _agent.coords.y;
            var cell = _gameManager.AgentsMap[x, y];
            
            // check if found gold
            if (cell.Exists(e => e.tag == "Gold"))
            {
                Debug.Log("Gold found!");
                _gameManager.SetGameOver(true);
                return;
            }

            // update knowledge base
            _kb.Tell(ctx.MkNot(P(x,y)));

            if (cell.Exists(e => e.tag == "Breeze"))
                _kb.Tell(B(x,y));
            else
                _kb.Tell(ctx.MkNot(B(x,y)));

            if (cell.Exists(e => e.tag == "Wall"))
            {
                _kb.Tell(W(x,y));
                _agentMove.BumpWall();
                return;
            }
            
            _kb.Tell(ctx.MkNot(W(x,y)));

            // choose next move and make it
            chooseMove(x, y);

        }

        // transform coordinates into boolean variable
        private BoolExpr B(int x, int y) => ctx.MkBoolConst($"B_{x}_{y}");
        private BoolExpr P(int x, int y) => ctx.MkBoolConst($"P_{x}_{y}");
        private BoolExpr W(int x, int y) => ctx.MkBoolConst($"W_{x}_{y}");

        // check if coordinate is safe
        private bool isSafe(int x, int y)
        {
            // avoid cycles
            if (exploredCells.Exists(c => (c.x == x && c.y == y)))
                return false;

            // if KB ⊨ -P(x,y) ^ -W(x,y), cell is safe
            bool noPit = _kb.Ask(ctx.MkNot(P(x, y))); // asserts its not a pit
            bool isWall = _kb.Ask(W(x, y)); // checks if its a wall
            return (noPit && !isWall);
        }

        // get coordinates of all neighbors
        List<(int x, int y)> GetNeighbors(int x, int y, int maxX, int maxY)
        {
            var list = new List<(int,int)>();
            if (x > 0)         list.Add((x-1, y));
            if (x < maxX - 1)  list.Add((x+1, y));
            if (y > 0)         list.Add((x, y-1));
            if (y < maxY - 1)  list.Add((x, y+1));
            return list;
        }

        // get and decide next move
        private void chooseMove(int posX, int posY)
        {
            // list all neighboring cells
            int mapSizeX = _gameManager.gridMax.x;
            int mapSizeY = _gameManager.gridMax.y;
            var neighbors = GetNeighbors(posX, posY, mapSizeX, mapSizeY);

            // get moves
            var safeMoves = neighbors.Where(c => isSafe(c.x, c.y)).ToList();

            // decide move
            if (safeMoves.Count > 0)
            {
                var chosen = safeMoves[0]; // change to a better heuristic
                _agentMove.MoveAgent(new Vector2Int(chosen.x, chosen.y));
                exploredCells.Add(chosen);
            }
            else
                _agentMove.MoveAgent(_agentMove.MoveBack());
        }

    }
}