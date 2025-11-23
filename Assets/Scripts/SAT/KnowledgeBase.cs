/*
adaptação para Unity do agente wumpus simplificado do prof. Guilherme 
usando SAT solver Z3
*/

using UnityEngine;
using Microsoft.Z3;
using System.Collections.Generic;

public class KnowledgeBase {
    // Atributos
    private Context ctx; // Contexto: necessário para usar o Z3
    private List<BoolExpr> clauses;  // Lista de cláusulas no formato do Z3
    private HashSet<string> clauseSignatures; // Evitar duplicação de cláusulas

    // Construtor
    public KnowledgeBase() {
        ctx = new Context();
        clauses = new List<BoolExpr>();
        clauseSignatures = new HashSet<string>();
    }

    // Informa cláusula alpha à KB
    public void Tell(BoolExpr alpha) {
        string key = alpha.ToString();
        // verifica se alpha já está na KB antes de inserir
        if (!clauseSignatures.Contains(key)) {
            clauseSignatures.Add(key);
            clauses.Add(alpha);
        }
    }

    // Consulta se alpha é conseqência lógica da KB
    public bool Ask(BoolExpr alpha) {
        // inicializando solver
        Solver s = ctx.MkSolver();
        foreach (var c in clauses)
            s.Add(c);
        // testando se KB ∧ ¬alpha é insatisfatível
        s.Add(ctx.MkNot(alpha));
        // se for, KB ⊨ alpha
        return s.Check() == Status.UNSATISFIABLE;
    }

    public Context Ctx => ctx; // expoe o ctx para outros scripts (read only)
}
