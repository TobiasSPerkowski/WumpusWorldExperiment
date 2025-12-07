/*
adaptação para Unity do agente wumpus simplificado do prof. Guilherme 
usando SAT solver Z3
*/

using UnityEngine;
using Microsoft.Z3;
using System.Collections.Generic;

public class KnowledgeBase 
{
    private Context ctx; // required by Z3
    private List<BoolExpr> clauses;  // clauses list in Z3's format
    private HashSet<string> clauseSignatures; // avoid repeated clauses

    public KnowledgeBase() 
    {
        ctx = new Context();
        clauses = new List<BoolExpr>();
        clauseSignatures = new HashSet<string>();
    }

    // Check if alpha is a logical consequence of the KB
    public bool Ask(BoolExpr alpha) {
        // Initializing solver
        Solver s = ctx.MkSolver();
        foreach (var c in clauses)
            s.Add(c);
        // test if KB ∧ ¬alpha is unsat
        s.Add(ctx.MkNot(alpha));
        // if it is, KB ⊨ alpha
        return s.Check() == Status.UNSATISFIABLE;
    }

    // Informs the alpha clause to the KB
    public void Tell(BoolExpr alpha) {
        string key = alpha.ToString();
        // check if alpha is already in the KB before adding it
        if (!clauseSignatures.Contains(key)) {
            clauseSignatures.Add(key);
            clauses.Add(alpha);
        }
    }

    public Context Ctx => ctx; // exposes ctx as read-only
}
