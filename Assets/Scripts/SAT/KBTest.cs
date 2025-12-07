using UnityEngine;
using Microsoft.Z3;

public class KBTester : MonoBehaviour
{
    void Start()
    {
        // Instancia a base de conhecimento
        KnowledgeBase kb = new KnowledgeBase();
        Context ctx = kb.Ctx;

        // Variáveis booleanas
        BoolExpr B = ctx.MkBoolConst("B");
        BoolExpr P0 = ctx.MkBoolConst("P0");
        BoolExpr P1 = ctx.MkBoolConst("P1");
        BoolExpr P2 = ctx.MkBoolConst("P2");
        BoolExpr P3 = ctx.MkBoolConst("P3");

        // Fórmulas lógicas (B <=> P0 v P1 v P2 v P3)
        BoolExpr formula0 = ctx.MkOr(ctx.MkNot(B), P0, P1, P2, P3);
        BoolExpr formula1 = ctx.MkOr(B, ctx.MkNot(P0));
        BoolExpr formula2 = ctx.MkOr(B, ctx.MkNot(P1));
        BoolExpr formula3 = ctx.MkOr(B, ctx.MkNot(P2));
        BoolExpr formula4 = ctx.MkOr(B, ctx.MkNot(P3));

        // Populando a KB
        kb.Tell(formula0);
        kb.Tell(formula1);
        kb.Tell(formula2);
        kb.Tell(formula3);
        kb.Tell(formula4);

        // Digamos que P3 é verdadeiro
        kb.Tell(P3);

        // Consultando se B é consequência lógica da KB
        Debug.Log("KB ⊨ B ?  " + kb.Ask(B));   // deve ser True

        // Consultando se P0 é consequência lógica da KB
        Debug.Log("KB ⊨ P0 ?  " + kb.Ask(P0));   // deve ser False
    }
}
